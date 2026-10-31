"""
ZONE — The Fracture: Blender Asset Generation Script
=====================================================
Run this script in Blender 5.2 to auto-generate base game assets:
- Building structures (modular walls, floors, roofs)
- Prop models (crates, barrels, supply drops)
- Monster base meshes (all 7 species)
- Character base meshes (4 faction armor sets)
- Terrain heightmap tools

Usage:
    blender --background --python generate_assets.py -- --output ./exports --type all
    blender --background --python generate_assets.py -- --output ./exports --type buildings
    blender --background --python generate_assets.py -- --output ./exports --type props
    blender --background --python generate_assets.py -- --output ./exports --type monsters
    blender --background --python generate_assets.py -- --output ./exports --type characters

Output: FBX files with PBR materials, ready for Unity import.
"""

import bpy
import bmesh
import math
import os
import sys
import argparse
import random
from mathutils import Vector

# ============================================================
# UTILITIES
# ============================================================

def clear_scene():
    """Clear the entire Blender scene."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for block in bpy.data.meshes:
        bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        bpy.data.materials.remove(block)

def save_fbx(filepath, objects):
    """Save selected objects as FBX."""
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_anim=False,
        mesh_smooth_type='FACE'
    )

def create_material(name, base_color, roughness=0.8, metallic=0.0):
    """Create a basic PBR material."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*base_color, 1.0)
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
    return mat

def apply_material(obj, mat):
    """Apply a material to an object."""
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)

def normalize_path(path):
    """Normalize a file path for the OS."""
    return os.path.normpath(path).replace('\\', '/')

# ============================================================
# BUILDING GENERATION
# ============================================================

def create_modular_wall(width=4, height=3, thickness=0.2, name="Wall_Straight"):
    """Create a straight modular wall segment."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height/2))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (width/2, thickness/2, height/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Add weathering detail — bevel edges
    bpy.ops.object.modifier_add(type='BEVEL')
    obj.modifiers["Bevel"].width = 0.02
    obj.modifiers["Bevel"].segments = 2

    mat = create_material(f"{name}_Mat", (0.35, 0.32, 0.28), roughness=0.9)
    apply_material(obj, mat)
    return obj

def create_modular_floor(width=4, depth=4, thickness=0.15, name="Floor_Square"):
    """Create a modular floor piece."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 0))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (width/2, depth/2, thickness/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    mat = create_material(f"{name}_Mat", (0.25, 0.23, 0.20), roughness=0.95)
    apply_material(obj, mat)
    return obj

def create_corner_pillar(height=3, width=0.5, name="Pillar_Corner"):
    """Create a corner pillar for buildings."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height/2))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (width/2, width/2, height/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.ops.object.modifier_add(type='BEVEL')
    obj.modifiers["Bevel"].width = 0.03
    obj.modifiers["Bevel"].segments = 3

    mat = create_material(f"{name}_Mat", (0.30, 0.28, 0.25), roughness=0.85)
    apply_material(obj, mat)
    return obj

def create_doorway(width=1.2, height=2.2, wall_thickness=0.2, name="Wall_Doorway"):
    """Create a wall section with a doorway opening."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height/2 + 0.4))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (width/2 + 0.4, wall_thickness/2, (height + 0.8)/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Cut out doorway with boolean
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height/2))
    cutter = bpy.context.active_object
    cutter.name = "Doorway_Cutter"
    cutter.scale = (width/2, wall_thickness, height/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_add(type='BOOLEAN')
    obj.modifiers["Boolean"].operation = 'DIFFERENCE'
    obj.modifiers["Boolean"].object = cutter
    bpy.ops.object.modifier_apply(modifier="Boolean")
    bpy.data.objects.remove(cutter)

    mat = create_material(f"{name}_Mat", (0.35, 0.32, 0.28), roughness=0.9)
    apply_material(obj, mat)
    return obj

def create_window_wall(width=4, height=3, wall_thickness=0.2, name="Wall_Window"):
    """Create a wall section with a window opening."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height/2))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (width/2, wall_thickness/2, height/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Cut out window
    win_w, win_h = 1.0, 1.2
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, height * 0.55))
    cutter = bpy.context.active_object
    cutter.name = "Window_Cutter"
    cutter.scale = (win_w/2, wall_thickness, win_h/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_add(type='BOOLEAN')
    obj.modifiers["Boolean"].operation = 'DIFFERENCE'
    obj.modifiers["Boolean"].object = cutter
    bpy.ops.object.modifier_apply(modifier="Boolean")
    bpy.data.objects.remove(cutter)

    mat = create_material(f"{name}_Mat", (0.35, 0.32, 0.28), roughness=0.9)
    apply_material(obj, mat)
    return obj

def create_ruined_building(width=8, depth=6, height=4, name="Building_Ruined"):
    """Create a ruined building with broken walls."""
    objects = []

    # Floor
    floor = create_modular_floor(width, depth, 0.15, f"{name}_Floor")
    objects.append(floor)

    # Walls (some broken/missing)
    wall_h = height
    # Back wall (intact)
    back_wall = create_modular_wall(width, wall_h, 0.2, f"{name}_Wall_Back")
    back_wall.location = (0, depth/2, 0)
    objects.append(back_wall)

    # Left wall (partially collapsed)
    left_wall = create_modular_wall(wall_h, 0.2, 0.2, f"{name}_Wall_Left")
    left_wall.rotation_euler = (0, 0, math.radians(90))
    left_wall.location = (-width/2, 0, 0)
    left_wall.scale = (1, depth/2/wall_h, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    objects.append(left_wall)

    # Right wall (doorway)
    right_wall = create_doorway(1.2, 2.2, 0.2, f"{name}_Wall_Right")
    right_wall.rotation_euler = (0, 0, math.radians(90))
    right_wall.location = (width/2, 0, 0)
    right_wall.scale = (1, depth/2, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    objects.append(right_wall)

    # Front wall (partially destroyed — just use a low wall)
    front_wall = create_modular_wall(width, wall_h * 0.4, 0.2, f"{name}_Wall_Front")
    front_wall.location = (0, -depth/2, 0)
    objects.append(front_wall)

    return objects

def generate_buildings(output_dir):
    """Generate all building assets."""
    print("[Blender] Generating building assets...")
    objects = []

    assets = [
        ("Modular_Wall_Straight", lambda: [create_modular_wall(4, 3, 0.2, "Wall_Straight")]),
        ("Modular_Wall_Doorway", lambda: [create_doorway(1.2, 2.2, 0.2, "Wall_Doorway")]),
        ("Modular_Wall_Window", lambda: [create_window_wall(4, 3, 0.2, "Wall_Window")]),
        ("Modular_Floor", lambda: [create_modular_floor(4, 4, 0.15, "Floor_Square")]),
        ("Modular_Pillar", lambda: [create_corner_pillar(3, 0.5, "Pillar_Corner")]),
        ("Building_Ruined_Small", lambda: create_ruined_building(8, 6, 4, "Building_Ruined_Small")),
        ("Building_Ruined_Medium", lambda: create_ruined_building(12, 8, 5, "Building_Ruined_Medium")),
        ("Building_Ruined_Large", lambda: create_ruined_building(16, 10, 6, "Building_Ruined_Large")),
    ]

    for name, create_fn in assets:
        clear_scene()
        objs = create_fn()
        filepath = os.path.join(output_dir, "buildings", f"{name}.fbx")
        os.makedirs(os.path.dirname(filepath), exist_ok=True)
        save_fbx(filepath, objs)
        print(f"  ✅ Exported: {name}.fbx")

# ============================================================
# PROP GENERATION
# ============================================================

def create_crate(size=1.0, name="Prop_Crate"):
    """Create a wooden supply crate."""
    bpy.ops.mesh.primitive_cube_add(size=size, location=(0, 0, size/2))
    obj = bpy.context.active_object
    obj.name = name

    # Bevel for worn look
    bpy.ops.object.modifier_add(type='BEVEL')
    obj.modifiers["Bevel"].width = 0.04
    obj.modifiers["Bevel"].segments = 2

    # Add plank grooves with edge loops
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bm = bmesh.from_edit_mesh(obj.data)
    bmesh.ops.subdivide_edges(bm, edges=bm.edges, cuts=2, use_grid_fill=True)
    bmesh.update_edit_mesh(obj.data)
    bpy.ops.object.mode_set(mode='OBJECT')

    mat = create_material(f"{name}_Mat", (0.45, 0.30, 0.15), roughness=0.9)
    apply_material(obj, mat)
    return obj

def create_barrel(radius=0.4, height=1.0, name="Prop_Barrel"):
    """Create a metal barrel."""
    bpy.ops.mesh.primitive_cylinder_add(radius=radius, depth=height, vertices=16, location=(0, 0, height/2))
    obj = bpy.context.active_object
    obj.name = name

    # Add reinforcing bands
    bpy.ops.object.modifier_add(type='SOLIDIFY')
    obj.modifiers["Solidify"].thickness = 0.02

    mat = create_material(f"{name}_Mat", (0.20, 0.25, 0.30), roughness=0.5, metallic=0.7)
    apply_material(obj, mat)
    return obj

def create_supply_drop(size=1.5, name="Prop_SupplyDrop"):
    """Create a supply drop crate with parachute remnants."""
    # Main crate
    bpy.ops.mesh.primitive_cube_add(size=size, location=(0, 0, size/2))
    obj = bpy.context.active_object
    obj.name = name

    bpy.ops.object.modifier_add(type='BEVEL')
    obj.modifiers["Bevel"].width = 0.05
    obj.modifiers["Bevel"].segments = 3

    mat = create_material(f"{name}_Mat", (0.15, 0.15, 0.12), roughness=0.6, metallic=0.3)
    apply_material(obj, mat)

    # Add a red stripe (emissive)
    stripe_mat = bpy.data.materials.new(name=f"{name}_Stripe_Mat")
    stripe_mat.use_nodes = True
    bsdf = stripe_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (0.8, 0.1, 0.1, 1.0)
        bsdf.inputs["Emission Color"].default_value = (0.8, 0.1, 0.1, 1.0)
        bsdf.inputs["Emission Strength"].default_value = 0.5

    # Add stripe as a separate mesh
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, size * 0.7))
    stripe = bpy.context.active_object
    stripe.name = f"{name}_Stripe"
    stripe.scale = (size/2 + 0.01, size/2 * 0.15, size/2 * 0.1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    apply_material(stripe, stripe_mat)

    return [obj, stripe]

def create_lantern(radius=0.15, height=0.3, name="Prop_Lantern"):
    """Create a small lantern/light source."""
    bpy.ops.mesh.primitive_cylinder_add(radius=radius, depth=height, vertices=8, location=(0, 0, height/2))
    obj = bpy.context.active_object
    obj.name = name

    mat = create_material(f"{name}_Mat", (0.3, 0.3, 0.25), roughness=0.4, metallic=0.6)
    apply_material(obj, mat)

    # Emissive glass
    bpy.ops.mesh.primitive_uv_sphere_add(radius=radius * 0.7, location=(0, 0, height/2))
    glass = bpy.context.active_object
    glass.name = f"{name}_Glass"

    glass_mat = bpy.data.materials.new(name=f"{name}_Glass_Mat")
    glass_mat.use_nodes = True
    bsdf = glass_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (1.0, 0.8, 0.4, 1.0)
        bsdf.inputs["Emission Color"].default_value = (1.0, 0.8, 0.4, 1.0)
        bsdf.inputs["Emission Strength"].default_value = 2.0
        bsdf.inputs["Transmission Weight"].default_value = 0.8
    apply_material(glass, glass_mat)

    return [obj, glass]

def generate_props(output_dir):
    """Generate all prop assets."""
    print("[Blender] Generating prop assets...")
    assets = [
        ("Prop_Crate", lambda: [create_crate(1.0, "Prop_Crate")]),
        ("Prop_Crate_Small", lambda: [create_crate(0.7, "Prop_Crate_Small")]),
        ("Prop_Barrel", lambda: [create_barrel(0.4, 1.0, "Prop_Barrel")]),
        ("Prop_Barrel_Rusty", lambda: [create_barrel(0.45, 1.1, "Prop_Barrel_Rusty")]),
        ("Prop_SupplyDrop", lambda: create_supply_drop(1.5, "Prop_SupplyDrop")),
        ("Prop_Lantern", lambda: create_lantern(0.15, 0.3, "Prop_Lantern")),
    ]

    for name, create_fn in assets:
        clear_scene()
        objs = create_fn()
        if not isinstance(objs, list):
            objs = [objs]
        filepath = os.path.join(output_dir, "props", f"{name}.fbx")
        os.makedirs(os.path.dirname(filepath), exist_ok=True)
        save_fbx(filepath, objs)
        print(f"  ✅ Exported: {name}.fbx")

# ============================================================
# MONSTER GENERATION
# ============================================================

def create_monster_base(species_name, body_shape, color, **kwargs):
    """Create a base monster mesh based on species parameters."""
    body_type = kwargs.get('body_type', 'humanoid')
    scale = kwargs.get('scale', 1.0)
    limbs = kwargs.get('limbs', 4)

    if body_type == 'quadruped':
        # Quadruped base (Burrower, Swarmling)
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.5 * scale, location=(0, 0, 0.5 * scale))
        body = bpy.context.active_object
        body.name = f"{species_name}_Body"
        body.scale = (1.5, 0.8, 0.7)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        # Head
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.35 * scale, location=(0.8 * scale, 0, 0.4 * scale))
        head = bpy.context.active_object
        head.name = f"{species_name}_Head"

        # Legs (4)
        leg_positions = [
            (0.4 * scale, 0.3 * scale, 0),
            (0.4 * scale, -0.3 * scale, 0),
            (-0.4 * scale, 0.3 * scale, 0),
            (-0.4 * scale, -0.3 * scale, 0),
        ]
        legs = []
        for i, pos in enumerate(leg_positions):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.12 * scale, depth=0.5 * scale, location=pos)
            leg = bpy.context.active_object
            leg.name = f"{species_name}_Leg_{i}"
            legs.append(leg)

        objects = [body, head] + legs

    elif body_type == 'humanoid':
        # Humanoid base (Stalker, Wraith)
        # Torso
        bpy.ops.mesh.primitive_cube_add(size=0.6 * scale, location=(0, 0, 1.2 * scale))
        torso = bpy.context.active_object
        torso.name = f"{species_name}_Torso"
        torso.scale = (0.5, 0.3, 0.8)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        # Head
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.25 * scale, location=(0, 0, 1.9 * scale))
        head = bpy.context.active_object
        head.name = f"{species_name}_Head"

        # Arms
        bpy.ops.mesh.primitive_cylinder_add(radius=0.1 * scale, depth=0.7 * scale,
                                              location=(0.35 * scale, 0, 1.2 * scale),
                                              rotation=(0, math.radians(90), 0))
        arm_r = bpy.context.active_object
        arm_r.name = f"{species_name}_Arm_R"

        bpy.ops.mesh.primitive_cylinder_add(radius=0.1 * scale, depth=0.7 * scale,
                                              location=(-0.35 * scale, 0, 1.2 * scale),
                                              rotation=(0, math.radians(90), 0))
        arm_l = bpy.context.active_object
        arm_l.name = f"{species_name}_Arm_L"

        # Legs
        bpy.ops.mesh.primitive_cylinder_add(radius=0.12 * scale, depth=0.8 * scale,
                                              location=(0.15 * scale, 0, 0.4 * scale))
        leg_r = bpy.context.active_object
        leg_r.name = f"{species_name}_Leg_R"

        bpy.ops.mesh.primitive_cylinder_add(radius=0.12 * scale, depth=0.8 * scale,
                                              location=(-0.15 * scale, 0, 0.4 * scale))
        leg_l = bpy.context.active_object
        leg_l.name = f"{species_name}_Leg_L"

        objects = [torso, head, arm_r, arm_l, leg_r, leg_l]

    elif body_type == 'massive':
        # Massive base (Goliath, Behemoth)
        bpy.ops.mesh.primitive_ico_sphere_add(radius=1.5 * scale, location=(0, 0, 1.5 * scale), subdivisions=2)
        body = bpy.context.active_object
        body.name = f"{species_name}_Body"
        body.scale = (1.2, 1.0, 1.5)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        # Add bumpy surface
        bpy.ops.object.modifier_add(type='SUBSURF')
        body.modifiers["Subdivision"].levels = 2

        # Massive legs
        for i, x in enumerate([-0.8, 0.8]):
            for j, y in enumerate([-0.5, 0.5]):
                bpy.ops.mesh.primitive_cylinder_add(radius=0.3 * scale, depth=1.5 * scale,
                                                      location=(x * scale, y * scale, 0.75 * scale))
                leg = bpy.context.active_object
                leg.name = f"{species_name}_Leg_{i}_{j}"

        # Head
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.5 * scale, location=(0, 0, 2.8 * scale))
        head = bpy.context.active_object
        head.name = f"{species_name}_Head"

        objects = [body, head]

    elif body_type == 'serpentine':
        # Serpentine base (Lurker)
        bpy.ops.mesh.primitive_cylinder_add(radius=0.3 * scale, depth=2.0 * scale,
                                              location=(0, 0, 0.3 * scale),
                                              rotation=(math.radians(90), 0, 0))
        body = bpy.context.active_object
        body.name = f"{species_name}_Body"

        # Flatten for crawling
        body.scale = (0.8, 1.0, 1.5)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        # Sensory appendages (whiskers)
        for i, y in enumerate([-0.2, 0.2]):
            bpy.ops.mesh.primitive_cone_add(radius1=0.02, radius2=0.0, depth=0.4,
                                              location=(0, y * scale, 0.4 * scale),
                                              rotation=(math.radians(-30), 0, 0))
            whisker = bpy.context.active_object
            whisker.name = f"{species_name}_Whisker_{i}"

        objects = [body]

    else:
        objects = []

    # Apply material to all
    mat = create_material(f"{species_name}_Mat", color, roughness=0.6)
    for obj in objects:
        apply_material(obj, mat)

    # Add subtle disfigurement (random vertex displacement)
    for obj in objects:
        if obj.type == 'MESH':
            for v in obj.data.vertices:
                v.co.x += random.uniform(-0.02, 0.02)
                v.co.y += random.uniform(-0.02, 0.02)
                v.co.z += random.uniform(-0.02, 0.02)

    return objects

def generate_monsters(output_dir):
    """Generate all monster base meshes."""
    print("[Blender] Generating monster assets...")
    monsters = [
        ("Monster_Burrower", "burrower", 'quadruped', (0.2, 0.15, 0.1), scale=1.2),
        ("Monster_Stalker", "stalker", 'humanoid', (0.1, 0.08, 0.12), scale=1.1),
        ("Monster_Lurker", "lurker", 'serpentine', (0.05, 0.05, 0.08), scale=1.3),
        ("Monster_Swarmling", "swarmling", 'quadruped', (0.3, 0.25, 0.05), scale=0.5),
        ("Monster_Goliath", "goliath", 'massive', (0.15, 0.1, 0.05), scale=2.0),
        ("Monster_Wraith", "wraith", 'humanoid', (0.08, 0.05, 0.12), scale=1.0),
        ("Monster_Behemoth", "behemoth", 'massive', (0.12, 0.08, 0.03), scale=3.5),
    ]

    for name, species, body_type, color, **kwargs in monsters:
        clear_scene()
        objs = create_monster_base(species, body_type, color, body_type=body_type, **kwargs)
        filepath = os.path.join(output_dir, "monsters", f"{name}.fbx")
        os.makedirs(os.path.dirname(filepath), exist_ok=True)
        save_fbx(filepath, objs)
        print(f"  ✅ Exported: {name}.fbx")

# ============================================================
# CHARACTER GENERATION
# ============================================================

def create_character_base(faction_name, armor_color, accent_color, **kwargs):
    """Create a base character mesh with faction armor."""
    scale = kwargs.get('scale', 1.0)
    armor_weight = kwargs.get('armor_weight', 'medium')  # light, medium, heavy

    # Torso
    bpy.ops.mesh.primitive_cube_add(size=0.5 * scale, location=(0, 0, 1.1 * scale))
    torso = bpy.context.active_object
    torso.name = f"{faction_name}_Torso"

    if armor_weight == 'heavy':
        torso.scale = (0.55, 0.35, 0.9)
    elif armor_weight == 'medium':
        torso.scale = (0.45, 0.3, 0.85)
    else:  # light
        torso.scale = (0.35, 0.25, 0.8)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Head
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.2 * scale, location=(0, 0, 1.7 * scale))
    head = bpy.context.active_object
    head.name = f"{faction_name}_Head"

    # Helmet (faction-specific)
    if armor_weight != 'light':
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.22 * scale, location=(0, 0, 1.7 * scale))
        helmet = bpy.context.active_object
        helmet.name = f"{faction_name}_Helmet"

    # Arms
    arm_radius = 0.08 * scale if armorWeight == 'light' else 0.12 * scale
    for side, x in [('R', 0.3 * scale), ('L', -0.3 * scale)]:
        bpy.ops.mesh.primitive_cylinder_add(radius=arm_radius, depth=0.7 * scale,
                                              location=(x, 0, 1.1 * scale),
                                              rotation=(0, math.radians(90), 0))
        arm = bpy.context.active_object
        arm.name = f"{faction_name}_Arm_{side}"

    # Legs
    leg_radius = 0.1 * scale
    for side, x in [('R', 0.12 * scale), ('L', -0.12 * scale)]:
        bpy.ops.mesh.primitive_cylinder_add(radius=leg_radius, depth=0.85 * scale,
                                              location=(x, 0, 0.42 * scale))
        leg = bpy.context.active_object
        leg.name = f"{faction_name}_Leg_{side}"

    # Gather objects
    objects = [obj for obj in bpy.data.objects if obj.name.startswith(faction_name)]

    # Armor material
    armor_mat = create_material(f"{faction_name}_Armor_Mat", armor_color, roughness=0.6, metallic=0.4)
    accent_mat = create_material(f"{faction_name}_Accent_Mat", accent_color, roughness=0.3, metallic=0.6)

    for obj in objects:
        if "Helmet" in obj.name or "Torso" in obj.name:
            apply_material(obj, armor_mat)
        elif "Arm" in obj.name or "Leg" in obj.name:
            apply_material(obj, armor_mat)
        else:
            apply_material(obj, accent_mat)

    return objects

def generate_characters(output_dir):
    """Generate all character/faction base meshes."""
    print("[Blender] Generating character assets...")
    factions = [
        ("Wardens_Soldier", (0.2, 0.25, 0.3), (0.8, 0.7, 0.3), armor_weight='heavy'),
        ("Wardens_Officer", (0.15, 0.2, 0.25), (0.9, 0.8, 0.2), armor_weight='heavy'),
        ("Scourge_Fighter", (0.3, 0.15, 0.1), (0.6, 0.2, 0.1), armor_weight='medium'),
        ("Scourge_Shaman", (0.2, 0.1, 0.05), (0.5, 0.3, 0.0), armor_weight='light'),
        ("Drifters_Scavenger", (0.25, 0.22, 0.18), (0.6, 0.5, 0.2), armor_weight='light'),
        ("Drifters_Mercenary", (0.2, 0.18, 0.15), (0.4, 0.4, 0.35), armor_weight='medium'),
        ("Traders_Guard", (0.15, 0.15, 0.12), (0.8, 0.7, 0.5), armor_weight='medium'),
        ("Traders_Merchant", (0.3, 0.25, 0.15), (0.7, 0.6, 0.3), armor_weight='light'),
        ("Player_Base", (0.15, 0.15, 0.15), (0.3, 0.3, 0.3), armor_weight='medium'),
    ]

    for name, armor_color, accent_color, **kwargs in factions:
        clear_scene()
        objs = create_character_base(name, armor_color, accent_color, **kwargs)
        filepath = os.path.join(output_dir, "characters", f"{name}.fbx")
        os.makedirs(os.path.dirname(filepath), exist_ok=True)
        save_fbx(filepath, objs)
        print(f"  ✅ Exported: {name}.fbx")

# ============================================================
# MAIN
# ============================================================

def main():
    parser = argparse.ArgumentParser(description="ZONE Asset Generator")
    parser.add_argument('--output', '-o', default='./exports', help='Output directory')
    parser.add_argument('--type', '-t', default='all', choices=['all', 'buildings', 'props', 'monsters', 'characters'])

    # Parse args (Blender passes -- as separator)
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1:]
    else:
        argv = []

    args = parser.parse_args(argv)
    output_dir = os.path.abspath(args.output)

    print(f"\n{'='*60}")
    print(f"  ZONE — The Fracture: Asset Generator")
    print(f"  Output: {output_dir}")
    print(f"  Type: {args.type}")
    print(f"{'='*60}\n")

    os.makedirs(output_dir, exist_ok=True)

    if args.type in ('all', 'buildings'):
        generate_buildings(output_dir)
    if args.type in ('all', 'props'):
        generate_props(output_dir)
    if args.type in ('all', 'monsters'):
        generate_monsters(output_dir)
    if args.type in ('all', 'characters'):
        generate_characters(output_dir)

    print(f"\n{'='*60}")
    print(f"  Asset generation complete!")
    print(f"  Files exported to: {output_dir}")
    print(f"{'='*60}\n")

if __name__ == "__main__":
    main()