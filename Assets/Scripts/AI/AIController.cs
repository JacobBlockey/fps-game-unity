using UnityEngine;
using UnityEngine.AI;

namespace ZONE.AI
{
    /// <summary>
    /// Core AI controller — state machine with perception (sight, sound, smell).
    /// Used by both faction NPCs and monsters via composition.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIController : MonoBehaviour
    {
        [Header("Perception — Sight")]
        [SerializeField] private float sightRange = 40f;
        [SerializeField] private float fieldOfView = 120f;
        [SerializeField] private float sightUpdateInterval = 0.2f;
        [SerializeField] private LayerMask sightObstructionLayers;

        [Header("Perception — Sound")]
        [SerializeField] private float hearingRange = 20f;
        [SerializeField] private float soundDecayTime = 3f;

        [Header("Perception — Smell")]
        [SerializeField] private float smellRange = 5f;
        [SerializeField] private bool canSmell = false;

        [Header("Memory")]
        [SerializeField] private float memoryDuration = 10f; // How long AI remembers last known position
        [SerializeField] private float searchDuration = 15f; // How long AI searches before giving up

        [Header("Debug")]
        [SerializeField] private bool debugPerception = false;

        private NavMeshAgent _agent;
        private AIState _currentState = AIState.Idle;
        private Transform _currentTarget;
        protected Vector3 _lastKnownTargetPosition;
        private float _lastSightTime;
        private float _lastSoundTime;
        private Vector3 _lastSoundPosition;
        private float _sightTimer;
        private float _searchTimer;

        public AIState CurrentState => _currentState;
        public Transform CurrentTarget => _currentTarget;
        public Vector3 LastKnownTargetPosition => _lastKnownTargetPosition;
        public NavMeshAgent Agent => _agent;
        public bool HasTarget => _currentTarget != null;

        public delegate void StateChangedHandler(AIState newState, AIState oldState);
        public event StateChangedHandler OnStateChanged;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            _sightTimer += Time.deltaTime;

            if (_sightTimer >= sightUpdateInterval)
            {
                _sightTimer = 0f;
                UpdatePerception();
            }

            UpdateStateMachine();
        }

        private void UpdatePerception()
        {
            // Find the nearest visible target
            Collider[] potentialTargets = Physics.OverlapSphere(transform.position, sightRange);

            Transform bestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var targetCol in potentialTargets)
            {
                var target = targetCol.transform;
                if (target == transform) continue;

                // Check if this is a valid target (player or hostile faction/monster)
                if (!IsValidTarget(target)) continue;

                // Check FOV
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                if (angle > fieldOfView * 0.5f) continue;

                // Check line of sight
                float distance = Vector3.Distance(transform.position, target.position);
                if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, out _, distance, sightObstructionLayers))
                    continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = target;
                }
            }

            if (bestTarget != null)
            {
                _currentTarget = bestTarget;
                _lastKnownTargetPosition = bestTarget.position;
                _lastSightTime = Time.time;

                if (_currentState == AIState.Idle || _currentState == AIState.Patrol)
                    ChangeState(AIState.Alert);
            }
            else if (_currentTarget != null && Time.time - _lastSightTime > memoryDuration)
            {
                // Lost sight of target — start searching
                if (_currentState == AIState.Combat || _currentState == AIState.Alert)
                    ChangeState(AIState.Search);
            }
        }

        /// <summary>
        /// Called when a sound event occurs nearby. The AI will investigate.
        /// </summary>
        public void HearSound(Vector3 soundPosition, float volume = 1f)
        {
            float distance = Vector3.Distance(transform.position, soundPosition);
            if (distance > hearingRange * volume) return;

            _lastSoundPosition = soundPosition;
            _lastSoundTime = Time.time;

            if (_currentState == AIState.Idle || _currentState == AIState.Patrol)
            {
                _lastKnownTargetPosition = soundPosition;
                ChangeState(AIState.Investigate);
            }
        }

        /// <summary>
        /// Called when a smell event occurs (for monsters with smell capability).
        /// </summary>
        public void DetectSmell(Vector3 smellPosition)
        {
            if (!canSmell) return;
            float distance = Vector3.Distance(transform.position, smellPosition);
            if (distance > smellRange) return;

            if (_currentState == AIState.Idle)
            {
                _lastKnownTargetPosition = smellPosition;
                ChangeState(AIState.Investigate);
            }
        }

        private void UpdateStateMachine()
        {
            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdle();
                    break;
                case AIState.Patrol:
                    HandlePatrol();
                    break;
                case AIState.Alert:
                    HandleAlert();
                    break;
                case AIState.Investigate:
                    HandleInvestigate();
                    break;
                case AIState.Combat:
                    HandleCombat();
                    break;
                case AIState.Search:
                    HandleSearch();
                    break;
                case AIState.Flee:
                    HandleFlee();
                    break;
            }
        }

        protected virtual void HandleIdle()
        {
            // Wait, occasionally transition to patrol
        }

        protected virtual void HandlePatrol()
        {
            // Move between patrol waypoints
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                // Get next patrol point (implementation depends on patrol system)
            }
        }

        protected virtual void HandleAlert()
        {
            if (_currentTarget != null && HasLineOfSight(_currentTarget))
            {
                ChangeState(AIState.Combat);
            }
            else
            {
                _agent.SetDestination(_lastKnownTargetPosition);
                if (!_agent.pathPending && _agent.remainingDistance < 1f)
                    ChangeState(AIState.Search);
            }
        }

        protected virtual void HandleInvestigate()
        {
            _agent.SetDestination(_lastSoundPosition);
            if (!_agent.pathPending && _agent.remainingDistance < 1f)
            {
                // Look around, then return to previous state
                ChangeState(AIState.Search);
            }
        }

        protected virtual void HandleCombat()
        {
            if (_currentTarget == null || Time.time - _lastSightTime > memoryDuration)
            {
                ChangeState(AIState.Search);
                return;
            }

            _lastKnownTargetPosition = _currentTarget.position;

            // Combat behavior is overridden by subclasses (NPC vs Monster)
        }

        protected virtual void HandleSearch()
        {
            _searchTimer += Time.deltaTime;
            if (_searchTimer >= searchDuration)
            {
                _currentTarget = null;
                _searchTimer = 0f;
                ChangeState(AIState.Patrol);
            }

            // Wander around last known position
            if (!_agent.pathPending && _agent.remainingDistance < 1f)
            {
                Vector3 wanderPoint = _lastKnownTargetPosition + Random.insideUnitSphere * 8f;
                wanderPoint.y = _lastKnownTargetPosition.y;
                _agent.SetDestination(wanderPoint);
            }
        }

        protected virtual void HandleFlee()
        {
            // Run away from target
            if (_currentTarget == null)
            {
                ChangeState(AIState.Idle);
                return;
            }

            Vector3 fleeDirection = (transform.position - _currentTarget.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * 20f;
            _agent.SetDestination(fleeTarget);

            if (Vector3.Distance(transform.position, _currentTarget.position) > sightRange * 2f)
                ChangeState(AIState.Idle);
        }

        protected bool HasLineOfSight(Transform target)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > sightRange) return false;

            return !Physics.Raycast(
                transform.position + Vector3.up,
                direction,
                distance,
                sightObstructionLayers
            );
        }

        protected virtual bool IsValidTarget(Transform target)
        {
            // Override in subclasses to define hostility rules
            return false;
        }

        protected void ChangeState(AIState newState)
        {
            if (_currentState == newState) return;
            AIState oldState = _currentState;
            _currentState = newState;
            _searchTimer = 0f;
            OnStateChanged?.Invoke(newState, oldState);
        }

        public void SetPatrolDestination(Vector3 destination)
        {
            if (_currentState == AIState.Patrol || _currentState == AIState.Idle)
                _agent.SetDestination(destination);
        }

        public void ForceState(AIState state)
        {
            ChangeState(state);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!debugPerception) return;

            // Sight range
            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, sightRange);

            // Hearing range
            Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            // Last known position
            if (_currentState == AIState.Search || _currentState == AIState.Alert)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_lastKnownTargetPosition + Vector3.up, Vector3.one);
            }
        }
#endif
    }

    public enum AIState
    {
        Idle,
        Patrol,
        Alert,
        Investigate,
        Combat,
        Search,
        Flee
    }
}
