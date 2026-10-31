using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZONE.Systems
{
    /// <summary>
    /// Weight-based inventory system. No slot count — items are limited by total weight.
    /// Different backpacks change the weight capacity and movement penalty.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Base Capacity")]
        [SerializeField] private float baseCarryWeight = 25f; // Without a backpack
        [SerializeField] private float maxCarryWeight = 100f; // Hard cap even with best backpack

        [Header("Encumbrance Thresholds")]
        [SerializeField] private float lightLoadThreshold = 0.4f;   // < 40% = no penalty
        [SerializeField] private float mediumLoadThreshold = 0.7f;  // < 70% = minor penalty
        [SerializeField] private float heavyLoadThreshold = 0.9f;   // < 90% = major penalty
        [SerializeField] private float lightSpeedMultiplier = 1.0f;
        [SerializeField] private float mediumSpeedMultiplier = 0.85f;
        [SerializeField] private float heavySpeedMultiplier = 0.65f;
        [SerializeField] private float overloadedSpeedMultiplier = 0.4f;

        [Header("Quick Slots")]
        [SerializeField] private int quickSlotCount = 5;

        [Header("Backpack")]
        [SerializeField] private BackpackDefinition currentBackpack;

        // Internal state
        private List<InventoryItem> _items = new();
        private float _currentWeight;
        private InventoryItem[] _quickSlots;
        private int _activeQuickSlot = 0;

        public float CurrentWeight => _currentWeight;
        public float MaxWeight => currentBackpack != null ? Mathf.Min(baseCarryWeight + currentBackpack.weightBonus, maxCarryWeight) : baseCarryWeight;
        public int ItemCount => _items.Count;
        public BackpackDefinition CurrentBackpack => currentBackpack;
        public InventoryItem[] QuickSlots => _quickSlots;
        public int ActiveQuickSlot => _activeQuickSlot;

        public delegate void InventoryChangedHandler();
        public event InventoryChangedHandler OnInventoryChanged;

        public delegate void QuickSlotChangedHandler(int slot, InventoryItem item);
        public event QuickSlotChangedHandler OnQuickSlotChanged;

        private void Awake()
        {
            _quickSlots = new InventoryItem[quickSlotCount];
        }

        /// <summary>
        /// Try to add an item to the inventory. Returns false if over encumbered.
        /// </summary>
        public bool AddItem(ItemDefinition itemDef, int quantity = 1)
        {
            float addedWeight = itemDef.weight * quantity;

            if (_currentWeight + addedWeight > MaxWeight)
            {
                Debug.Log($"[Inventory] Cannot add {itemDef.displayName} — would exceed weight limit ({_currentWeight + addedWeight:F1}/{MaxWeight:F1})");
                return false;
            }

            // Try to stack with existing items
            if (itemDef.isStackable)
            {
                InventoryItem existing = _items.Find(i => i.definition.itemID == itemDef.itemID);
                if (existing != null)
                {
                    existing.quantity += quantity;
                    _currentWeight += addedWeight;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            _items.Add(new InventoryItem { definition = itemDef, quantity = quantity });
            _currentWeight += addedWeight;
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove items from the inventory.
        /// </summary>
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            InventoryItem item = _items.Find(i => i.definition.itemID == itemID);
            if (item == null || item.quantity < quantity) return false;

            item.quantity -= quantity;
            _currentWeight -= item.definition.weight * quantity;

            if (item.quantity <= 0)
            {
                _items.Remove(item);
                // Remove from quick slots if present
                for (int i = 0; i < _quickSlots.Length; i++)
                {
                    if (_quickSlots[i] != null && _quickSlots[i].definition.itemID == itemID)
                    {
                        _quickSlots[i] = null;
                        OnQuickSlotChanged?.Invoke(i, null);
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Get the encumbrance speed multiplier for the player controller.
        /// </summary>
        public float GetEncumbranceMultiplier()
        {
            float loadRatio = _currentWeight / MaxWeight;

            if (loadRatio < lightLoadThreshold) return lightSpeedMultiplier;
            if (loadRatio < mediumLoadThreshold) return mediumSpeedMultiplier;
            if (loadRatio < heavyLoadThreshold) return heavySpeedMultiplier;
            return overloadedSpeedMultiplier;
        }

        /// <summary>
        /// Get the encumbrance tier for UI feedback.
        /// </summary>
        public EncumbranceTier GetEncumbranceTier()
        {
            float loadRatio = _currentWeight / MaxWeight;
            if (loadRatio < lightLoadThreshold) return EncumbranceTier.Light;
            if (loadRatio < mediumLoadThreshold) return EncumbranceTier.Medium;
            if (loadRatio < heavyLoadThreshold) return EncumbranceTier.Heavy;
            return EncumbranceTier.Overloaded;
        }

        /// <summary>
        /// Bind an item from the inventory to a quick slot.
        /// </summary>
        public void AssignQuickSlot(int slot, string itemID)
        {
            if (slot < 0 || slot >= quickSlotCount) return;

            InventoryItem item = _items.Find(i => i.definition.itemID == itemID);
            if (item != null && item.definition.canBeQuickSlotted)
            {
                _quickSlots[slot] = item;
                OnQuickSlotChanged?.Invoke(slot, item);
            }
        }

        /// <summary>
        /// Activate the currently selected quick slot item.
        /// </summary>
        public void UseActiveQuickSlot()
        {
            if (_activeQuickSlot < 0 || _activeQuickSlot >= quickSlotCount) return;
            if (_quickSlots[_activeQuickSlot] == null) return;

            InventoryItem item = _quickSlots[_activeQuickSlot];
            // The actual use behavior depends on item type — weapon equips, consumables consume, etc.
            // This is a hook for the weapon/consumable systems to pick up.
            OnItemUsed?.Invoke(item);
        }

        public delegate void ItemUsedHandler(InventoryItem item);
        public event ItemUsedHandler OnItemUsed;

        /// <summary>
        /// Cycle to the next quick slot.
        /// </summary>
        public void CycleQuickSlot(int direction = 1)
        {
            _activeQuickSlot = (_activeQuickSlot + direction + quickSlotCount) % quickSlotCount;
            OnQuickSlotChanged?.Invoke(_activeQuickSlot, _quickSlots[_activeQuickSlot]);
        }

        /// <summary>
        /// Set the active quick slot directly.
        /// </summary>
        public void SetActiveQuickSlot(int slot)
        {
            if (slot >= 0 && slot < quickSlotCount)
            {
                _activeQuickSlot = slot;
                OnQuickSlotChanged?.Invoke(slot, _quickSlots[slot]);
            }
        }

        /// <summary>
        /// Equip a different backpack. Changes max carry weight.
        /// </summary>
        public void EquipBackpack(BackpackDefinition newBackpack)
        {
            // If current weight exceeds new backpack limit, items stay but player is overloaded
            currentBackpack = newBackpack;
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Get all items in the inventory (read-only).
        /// </summary>
        public IReadOnlyList<InventoryItem> GetAllItems()
        {
            return _items.AsReadOnly();
        }

        /// <summary>
        /// Check if the player has a specific item.
        /// </summary>
        public bool HasItem(string itemID, int quantity = 1)
        {
            InventoryItem item = _items.Find(i => i.definition.itemID == itemID);
            return item != null && item.quantity >= quantity;
        }

        /// <summary>
        /// Transfer an item to another inventory (for trading/stashing).
        /// </summary>
        public bool TransferItem(InventorySystem target, string itemID, int quantity = 1)
        {
            if (!HasItem(itemID, quantity)) return false;

            InventoryItem item = _items.Find(i => i.definition.itemID == itemID);
            if (!target.AddItem(item.definition, quantity)) return false;

            RemoveItem(itemID, quantity);
            return true;
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public ItemDefinition definition;
        public int quantity;
    }

    public enum EncumbranceTier
    {
        Light,
        Medium,
        Heavy,
        Overloaded
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "ZONE/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemID;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public float weight = 1f;
        public int maxStack = 99;
        public bool isStackable = true;
        public bool canBeQuickSlotted = true;
        public ItemCategory category;
        public int baseValue = 10; // Value in Shards
    }

    public enum ItemCategory
    {
        Weapon,
        Ammo,
        Consumable,
        Medical,
        Crafting,
        MonsterPart,
        Misc,
        Quest
    }

    [CreateAssetMenu(fileName = "NewBackpack", menuName = "ZONE/Backpack Definition")]
    public class BackpackDefinition : ScriptableObject
    {
        public string backpackName;
        [TextArea] public string description;
        public float weightBonus = 20f; // Additional carry weight
        public float speedPenalty = 0f; // Movement speed penalty (0 = none)
        public int quickSlotBonus = 0; // Additional quick slots
        public int baseValue = 500;
    }
}
