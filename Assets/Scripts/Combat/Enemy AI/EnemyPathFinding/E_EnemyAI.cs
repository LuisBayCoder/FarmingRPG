using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class E_EnemyAI : MonoBehaviour
{   // [SerializeField] private Transform[] attackPositions; // The 4 positions around the player (left, right, up, down)
    [SerializeField] private float detectionRadius = 5f; // Detection radius
    [SerializeField] private float minAttackDistance = 0.1f; // Minimum distance to trigger attack
    [SerializeField] private float maxAttackRange = 0.3f; // Maximum distance to trigger attack
    [SerializeField] private float pathFindingStopDistance = 1.75f; // Distance to stop pathfinding when close enough to the target
    [SerializeField] private float pathUpdateDelay = 0.5f; // Time between path recalculations
    [SerializeField] private NPCPath npcPath = null; // A* pathfinding script
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private bool isDebugMode = false; // Debug mode flag
    [SerializeField] private float roamDuration = 3f; // Duration of roaming before changing direction
    [SerializeField] private Transform[] patrolPoints; // Patrol waypoints used while roaming
    [SerializeField] private float patrolArrivalDistance = 0.15f; // Distance that counts as arriving at a patrol point
    [SerializeField] private bool avoidCrowdedPatrolPoints = true; // Prefer patrol points that are not occupied by other enemies
    [SerializeField] private float patrolPointCrowdingRadius = 0.9f; // Radius used to detect crowding at a patrol point
    [SerializeField] private Collider2D patrolAreaCollider = null; // Optional patrol/wake area bounds
    [SerializeField] private bool autoDiscoverPatrolPointsInScene = true; // Rebind patrol points for cloned enemies
    [SerializeField] private string checkpointParentObjectName = "EnemyCheckPoints_1"; // Parent object containing child checkpoints
    [SerializeField] private string patrolPointTag = "EnemyPatrolPoint"; // Tag used to locate scene patrol points
    [SerializeField] private float patrolPointSearchRadius = 8f; // Max distance to collect patrol points (<=0 means unlimited)
    [SerializeField] private int maxDiscoveredPatrolPoints = 4; // Maximum points auto-loaded from scene
    [SerializeField] private bool autoCreatePatrolPointsIfMissing = true; // Create local patrol points when scene references are missing
    [SerializeField] private float fallbackPatrolHalfDistance = 1.5f; // Half-distance used for generated patrol points
    [SerializeField] private bool useAStarForGeneratedPatrolPoints = true; // Generate fallback patrol endpoints from reachable A* cells
    [SerializeField] private int generatedPatrolSearchCells = 6; // Maximum grid-cell distance for A* patrol generation
    [SerializeField] private float attackPositionMoveSpeed = 5f; // Speed for moving to attack position
    [SerializeField] private float attackDuration = 0.8f; // Time to pause movement when attacking
    [SerializeField] private float recoveryDuration = 0.5f; // Short pause after attack before resuming
    [SerializeField] private float attackStopDistance = 0.8f; // Distance to stop from player before attacking
    [SerializeField] private float attackRangeExitBuffer = 0.35f; // Extra distance before cancelling an attack once committed
    [SerializeField] private float horizontalApproachThreshold = 0.1f; // Prefer side attacks if horizontal movement is present
    [SerializeField] private float attackMarkerArrivalDistance = 0.2f; // Distance to chosen marker that counts as in-position

    // runtime attack control
    private bool attackTimerRunning = false;
    private bool isPerformingAttack = false;
    private Coroutine attackCoroutine = null; // reference to the running attack coroutine

    private GameObject player; // Reference to the player GameObject
    //private Vector2Int playerGridPosition; // Player's grid position for pathfinding  
    private Transform playerTransform;
    private Transform enemyAttackPosition; // Legacy parent container for attack positions
    private Transform attackFromLeft;
    private Transform attackFromRight;
    private Transform attackFromUp;
    private Transform attackFromDown;
    public float checkInterval = 2f; // Time between overlap checks
    public float moveDistance = 1f; // Distance to move away if overlapping

    private Collider2D enemyCollider;
    private Animator animator;
    private Transform targetPosition; // The current target position for the enemy
    private Vector2Int finishPosition; // Target grid position for pathfinding
    public bool playerDetected = false; // Flag to check if the player is detected
    private float pathUpdateTimer; // Timer to control path updates
    public bool isInAttackRange = false;
    public SceneName npcCurrentScene;
    private bool isAvoidingCollision = false;
    private bool isMovingToAttackPosition = false; // Flag to track if enemy is moving to attack position
    private Vector3 targetAttackPosition; // Target attack position
    private float attackEnterRange;
    private float attackExitRange;
    private Vector3 previousFramePosition;
    private Vector2 lastWalkingDirection = Vector2.zero;
    private int lockedAttackIndex = -1;
    private AttackAxis lockedAttackAxis = AttackAxis.None;
    private Grid sceneGrid;
    private float sideDebugLogTimer = 0f;
    private const float SideDebugLogInterval = 0.25f;
    private int patrolPointIndex = 0;
    private int patrolDirection = 1;
    private float patrolPauseTimer = 0f;
    private float patrolPathUpdateTimer = 0f;
    private float lastPathRecalculationTime = -Mathf.Infinity;
    [SerializeField] private float pathRecalculationCooldown = 2f; // Cooldown time for path recalculations
    private Vector3 lastPatrolPosition;
    private float patrolStuckTimer = 0f;
    [SerializeField] private float patrolStuckTimeout = 5f;
    private Transform fallbackPatrolRoot;
    private Transform fallbackPatrolPointA;
    private Transform fallbackPatrolPointB;

    private enum AttackAxis
    {
        None,
        Horizontal,
        Vertical
    }

    [SerializeField] private NPCMovement npcMovement; // Reference to the NPCMovement script

    [SerializeField] private Vector2 attackOffsetDown = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetUp = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetRight = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetLeft = Vector2.zero;

    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Recovering,
        Avoiding
    }

    private State state; // Current state of the enemy
    private EnemyPathfinding enemyPathfinding;
    private Vector3 originalTargetPosition; // Store the target position before avoiding

    private bool hasChosenAttackPosition = false; // Flag to prevent repeatedly choosing attack positions

    private void Awake()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();

        enemyPathfinding = GetComponent<EnemyPathfinding>();
        RecalculateAttackRanges();

        // Convert string to SceneName enum
        if (System.Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, out SceneName sceneName))
        {
            npcCurrentScene = sceneName;
        }
        else
        {
            // Handle case where scene name doesn't match any enum value
            Debug.LogWarning($"Scene name '{SceneManager.GetActiveScene().name}' not found in SceneName enum");
        }
    }

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
        npcPath = GetComponent<NPCPath>();
        TryResolvePlayerReferences();

        if (npcMovement != null)
        {
            npcMovement.EnemyAfterSceneLoad();
        }

        animator.SetBool("isAttacking", false); // Ensure attack animation is not playing at start

        sceneGrid = FindObjectOfType<Grid>();

        StartResetAnimation();

        enemyCollider = GetComponent<Collider2D>();

        // Start periodic checks
        InvokeRepeating(nameof(CheckForCollision), checkInterval, checkInterval);

        previousFramePosition = transform.position;
    }

    private void StartResetAnimation()
    {

        ResetMovementAnimation(); // Reset all movement animations

        state = State.Roaming; // Set initial state to Roaming
        EnsurePatrolPointsAvailable();
        InitializePatrolState();

        StartCoroutine(RoamingRoutine());
    }
    private void CheckForCollision()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, enemyCollider.bounds.size, 0);

        foreach (Collider2D collider in colliders)
        {
            if (collider != enemyCollider && collider.CompareTag("Enemy"))
            {
                ResolveCollision(collider);
                break; // Only handle the first collision for simplicity
            }
        }
    }
    private void ResolveCollision(Collider2D otherCollider)
    {
        isAvoidingCollision = true;

        // Move away from the collider
        Vector2 directionAway = (transform.position - otherCollider.transform.position).normalized;
        Vector3 targetPosition = transform.position + (Vector3)(directionAway * moveDistance);

        // Smoothly move to the target position
        StartCoroutine(MoveToAvoid(targetPosition));
    }
    private IEnumerator MoveToAvoid(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float duration = 1f; // Duration of the avoidance movement
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isAvoidingCollision = false; // Resume normal behavior
    }

    private void OnDrawGizmos()
    {
        // Visualize the overlap area for debugging
        if (enemyCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, enemyCollider.bounds.size);
        }
    }
    private IEnumerator RoamingRoutine()//new
    {
        while (state == State.Roaming)
        {
            if (isAvoidingCollision)
            {
                // If avoiding collision, skip roaming logic
                yield return null;
                continue;
            }

            if (patrolPauseTimer > 0f)
            {
                patrolPauseTimer -= Time.deltaTime;
                enemyPathfinding.MoveTo(Vector2.zero);
                yield return null;
                continue;
            }

            // Roaming: NPCMovement handles A* stepping and animation per step.
            UpdatePathToPatrolPoint();
            CheckPatrolStuck();
            yield return null;
        }
    }

    private void InitializePatrolState()
    {
        EnsurePatrolPointsAvailable();

        patrolPointIndex = 0;
        patrolDirection = 1;
        patrolPauseTimer = 0f;
        patrolPathUpdateTimer = 0f;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Vector2Int currentGrid = GetGridTargetFromWorld(transform.position);

        // Start from the closest patrol point that is also on a different grid cell when possible.
        int closestDifferentGridIndex = -1;
        float closestDifferentGridSqrDistance = float.MaxValue;
        int closestAnyIndex = -1;
        float closestAnySqrDistance = float.MaxValue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            float sqrDistance = ((Vector2)(patrolPoints[i].position - transform.position)).sqrMagnitude;
            if (sqrDistance < closestAnySqrDistance)
            {
                closestAnySqrDistance = sqrDistance;
                closestAnyIndex = i;
            }

            Vector2Int patrolGrid = GetGridTargetFromWorld(patrolPoints[i].position);
            if (patrolGrid != currentGrid && sqrDistance < closestDifferentGridSqrDistance)
            {
                closestDifferentGridSqrDistance = sqrDistance;
                closestDifferentGridIndex = i;
            }
        }

        patrolPointIndex = closestDifferentGridIndex >= 0 ? closestDifferentGridIndex : (closestAnyIndex >= 0 ? closestAnyIndex : 0);
        lastPatrolPosition = transform.position;
        patrolStuckTimer = 0f;

        // Desync path updates across enemies so groups do not replan on the same frame.
        float desyncWindow = Mathf.Max(0f, pathRecalculationCooldown);
        if (desyncWindow > 0f)
        {
            lastPathRecalculationTime = Time.time - Random.Range(0f, desyncWindow);
        }
        else
        {
            lastPathRecalculationTime = Time.time;
        }
    }

    private void CheckPatrolStuck()
    {
        // If the enemy hasn't moved far enough within the timeout, cancel the current
        // move step and force a path rebuild so it doesn't freeze against a corner.
        if ((transform.position - lastPatrolPosition).sqrMagnitude > 0.01f)
        {
            lastPatrolPosition = transform.position;
            patrolStuckTimer = 0f;
            return;
        }

        patrolStuckTimer += Time.deltaTime;
        if (patrolStuckTimer < Mathf.Max(0.5f, patrolStuckTimeout))
        {
            return;
        }

        // Stuck — cancel the blocked step and immediately rebuild from current position.
        patrolStuckTimer = 0f;
        lastPatrolPosition = transform.position;
        if (npcMovement != null) npcMovement.CancelNPCMovement();
        if (npcPath != null) npcPath.ClearPath();
        patrolPathUpdateTimer = 0f; // Force immediate path recalc next frame.
        // Keep current checkpoint target locked and try again.
    }

    private bool TrySelectReachableNextPatrolPoint(Vector2Int currentGrid)
    {
        if (patrolPoints == null || patrolPoints.Length <= 1)
        {
            return false;
        }

        int currentIndex = Mathf.Clamp(patrolPointIndex, 0, patrolPoints.Length - 1);
        List<int> reachableIndices = new List<int>();

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (i == currentIndex || patrolPoints[i] == null)
            {
                continue;
            }

            Vector2Int candidateGrid = GetGridTargetFromWorld(GetPatrolPointWorldPosition(i));
            if (candidateGrid == currentGrid)
            {
                continue;
            }

            int pathSteps = GetAStarPathSteps(currentGrid, candidateGrid);
            if (pathSteps > 1)
            {
                reachableIndices.Add(i);
            }
        }

        if (reachableIndices.Count == 0)
        {
            return false;
        }

        if (!avoidCrowdedPatrolPoints)
        {
            patrolPointIndex = reachableIndices[Random.Range(0, reachableIndices.Count)];
            return true;
        }

        int lowestCrowdCount = int.MaxValue;
        List<int> bestIndices = new List<int>();
        float crowdRadius = Mathf.Max(0.1f, patrolPointCrowdingRadius);

        for (int i = 0; i < reachableIndices.Count; i++)
        {
            int candidateIndex = reachableIndices[i];
            int crowdCount = CountNearbyEnemiesAtPatrolPoint(candidateIndex, crowdRadius);

            if (crowdCount < lowestCrowdCount)
            {
                lowestCrowdCount = crowdCount;
                bestIndices.Clear();
                bestIndices.Add(candidateIndex);
            }
            else if (crowdCount == lowestCrowdCount)
            {
                bestIndices.Add(candidateIndex);
            }
        }

        List<int> pool = bestIndices.Count > 0 ? bestIndices : reachableIndices;
        patrolPointIndex = pool[Random.Range(0, pool.Count)];
        return true;
    }

    private int CountNearbyEnemiesAtPatrolPoint(int pointIndex, float radius)
    {
        if (patrolPoints == null || pointIndex < 0 || pointIndex >= patrolPoints.Length || patrolPoints[pointIndex] == null)
        {
            return 0;
        }

        Vector3 point = GetPatrolPointWorldPosition(pointIndex);
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(point, radius);
        int count = 0;

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider2D overlap = overlaps[i];
            if (overlap == null)
            {
                continue;
            }

            if (overlap.transform == transform)
            {
                continue;
            }

            if (overlap.CompareTag("Enemy"))
            {
                count++;
            }
        }

        return count;
    }

    private bool HasActiveAStarMovement()
    {
        if (npcMovement != null && npcMovement.npcIsMoving)
        {
            return true;
        }

        return npcPath != null
            && npcPath.npcMovementStepStack != null
            && npcPath.npcMovementStepStack.Count > 0;
    }

    private Vector2 GetPatrolDirection()
    {
        EnsurePatrolPointsAvailable();

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return Vector2.zero;
        }

        if (patrolPoints.Length == 1 || patrolPoints[patrolPointIndex] == null)
        {
            Vector3 singlePointTarget = GetPatrolPointWorldPosition(0);
            return ((Vector2)singlePointTarget - (Vector2)transform.position).normalized;
        }

        Vector3 targetWorldPosition = GetPatrolPointWorldPosition(patrolPointIndex);
        Vector2 toTarget = (Vector2)targetWorldPosition - (Vector2)transform.position;
        if (toTarget.magnitude <= patrolArrivalDistance)
        {
            AdvancePatrolPoint();
            patrolPauseTimer = Mathf.Max(0f, roamDuration);
            return Vector2.zero;
        }

        return toTarget.normalized;
    }

    private void UpdatePathToPatrolPoint()
    {
        EnsurePatrolPointsAvailable();

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (npcPath != null) npcPath.ClearPath();
            return;
        }

        int clampedPatrolIndex = Mathf.Clamp(patrolPointIndex, 0, patrolPoints.Length - 1);
        Vector3 targetWorldPosition = GetPatrolPointWorldPosition(clampedPatrolIndex);
        Vector2 toTarget = (Vector2)targetWorldPosition - (Vector2)transform.position;
        Vector2Int currentGrid = GetGridTargetFromWorld(transform.position);
        Vector2Int targetGrid = GetGridTargetFromWorld(targetWorldPosition);
        bool hasActiveAStarMovement = HasActiveAStarMovement();

        if (toTarget.magnitude <= patrolArrivalDistance)
        {
            // Do not switch patrol targets while an existing path step is still being consumed.
            if (hasActiveAStarMovement)
            {
                return;
            }

            TrySelectReachableNextPatrolPoint(currentGrid);
            patrolPauseTimer = Mathf.Max(0f, roamDuration);
            if (npcPath != null) npcPath.ClearPath();
            return;
        }

        // A* works on grid cells, so if we share a cell with the patrol point,
        // finish the last short world-space approach before selecting a new target.
        if (targetGrid == currentGrid)
        {
            if (hasActiveAStarMovement)
            {
                return;
            }

            if (npcMovement != null) npcMovement.CancelNPCMovement();
            if (npcPath != null) npcPath.ClearPath();
            if (enemyPathfinding != null)
            {
                enemyPathfinding.MoveTo(toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector2.zero);
            }
            return;
        }

        if (enemyPathfinding != null)
        {
            enemyPathfinding.MoveTo(Vector2.zero);
        }

        // Keep the current destination locked until the current A* path is fully consumed.
        if (hasActiveAStarMovement)
        {
            return;
        }

        // Throttle path recalculations
        if (Time.time - lastPathRecalculationTime < pathRecalculationCooldown)
        {
            return;
        }

        lastPathRecalculationTime = Time.time;

        Vector2Int finalTargetGrid = targetGrid;
        int pathStepsToCurrentTarget = GetAStarPathSteps(currentGrid, finalTargetGrid);

        if (pathStepsToCurrentTarget <= 1)
        {
            if (!TrySelectReachableNextPatrolPoint(currentGrid))
            {
                if (npcPath != null) npcPath.ClearPath();
                return;
            }

            finalTargetGrid = GetGridTargetFromWorld(GetPatrolPointWorldPosition(patrolPointIndex));
            pathStepsToCurrentTarget = GetAStarPathSteps(currentGrid, finalTargetGrid);
            if (pathStepsToCurrentTarget <= 1)
            {
                if (npcPath != null) npcPath.ClearPath();
                return;
            }
        }

        NPCScheduleEvent patrolEvent = new NPCScheduleEvent(
            0, 0, 0, 0, Weather.none, Season.none, npcCurrentScene,
            new GridCoordinate(finalTargetGrid.x, finalTargetGrid.y), null
        );

        BuildPath(patrolEvent);
    }

    private void AdvancePatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length <= 1)
        {
            return;
        }

        int currentIndex = Mathf.Clamp(patrolPointIndex, 0, patrolPoints.Length - 1);
        List<int> validNextIndices = new List<int>();

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                continue;
            }

            if (i == currentIndex)
            {
                continue;
            }

            validNextIndices.Add(i);
        }

        if (validNextIndices.Count == 0)
        {
            return;
        }

        // Pick a random next checkpoint (not the current one).
        // UpdatePathToPatrolPoint will validate it has a real multi-step path.
        patrolPointIndex = validNextIndices[Random.Range(0, validNextIndices.Count)];
    }

    // New method to handle movement animation based on direction
    private void SetMovementAnimation(Vector2 direction)
    {

        if (animator == null)
        {
            Debug.LogError("Animator component is not assigned.");
            return;
        }

        ResetMovementAnimation(); // Reset all movement animations


        // Determine primary direction based on the larger component
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {

            Debug.Log("Setting movement animation for direction: " + direction);
            if (direction.x > 0)
            {
                animator.SetBool(Settings.walkRight, true);
                if (isDebugMode) Debug.Log("Enemy moving right");
            }
            else
            {
                animator.SetBool(Settings.walkLeft, true);
                if (isDebugMode) Debug.Log("Enemy moving left");
            }
        }
        else
        {
            // Vertical movement is dominant
            if (direction.y > 0)
            {
                animator.SetBool(Settings.walkUp, true);
                if (isDebugMode) Debug.Log("Enemy moving up");
            }
            else
            {
                animator.SetBool(Settings.walkDown, true);
                if (isDebugMode) Debug.Log("Enemy moving down");
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            TryResolvePlayerReferences();
            if (playerTransform == null)
            {
                // Spawned clones can exist before player is created in a scene.
                playerDetected = false;
                return;
            }
        }

        TrackWalkingDirection();
        RecalculateAttackRanges();

        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool attackCommitted = state == State.Attacking || state == State.Recovering || isPerformingAttack || isMovingToAttackPosition || attackTimerRunning || hasChosenAttackPosition;
        float attackRangeForCurrentState = attackCommitted ? attackExitRange : attackEnterRange;

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            if (isDebugMode) Debug.Log("Player detected within detection radius.");

            // Always mark player detected
            playerDetected = true;

            if (lockedAttackAxis == AttackAxis.None)
            {
                Vector2 initialOffset = (Vector2)transform.position - (Vector2)playerTransform.position;
                lockedAttackAxis = Mathf.Abs(initialOffset.x) >= Mathf.Abs(initialOffset.y)
                    ? AttackAxis.Horizontal
                    : AttackAxis.Vertical;

                if (isDebugMode) Debug.Log($"[EnemyAI] Locked attack axis {lockedAttackAxis}");
            }

            // Keep preferred attack side updated while chasing.
            // This avoids committing too early from a noisy initial detection offset.
            if (!isInAttackRange && !hasChosenAttackPosition)
            {
                lockedAttackIndex = GetAttackIndexFromRelativePlayerSide();
            }

            LogAttackSideDebug(distanceToPlayer);

            // If currently attacking/recovering or already performing attack -> handle cancel when player moves out of attack range
            if (state == State.Attacking || state == State.Recovering || isPerformingAttack || isMovingToAttackPosition)
            {
                // If we are still repositioning (not yet swinging), allow cancel when target leaves range.
                // Once attack is actively playing, do not interrupt mid-swing.
                bool canCancelCurrentAttack = !isPerformingAttack && !attackTimerRunning;

                if (canCancelCurrentAttack && distanceToPlayer > attackExitRange)
                {
                    if (isDebugMode) Debug.Log("Player moved outside attack exit range - cancelling attack.");

                    // Stop any running attack coroutine
                    if (attackCoroutine != null)
                    {
                        StopCoroutine(attackCoroutine);
                        attackCoroutine = null;
                    }

                    // Reset attack-related flags
                    attackTimerRunning = false;
                    isPerformingAttack = false;
                    isMovingToAttackPosition = false;
                    hasChosenAttackPosition = false;
                    lockedAttackIndex = -1;
                    lockedAttackAxis = AttackAxis.None;

                    // Reset animator flags
                    if (animator != null)
                    {
                        animator.SetBool("isAttacking", false);
                        ResetAttackAnimations();
                    }

                    // Resume chasing
                    state = State.Chasing;
                    // update path right away
                    UpdatePathToPlayer();
                    return;
                }

                // While attacking/recovering keep movement cancelled so enemy doesn't jitter
                if (npcMovement != null) npcMovement.CancelNPCMovement();
                if (npcPath != null) npcPath.ClearPath();

                // If we are repositioning to the attack marker, continue that movement even in attacking state.
                if (isMovingToAttackPosition)
                {
                    UpdateMoveToAttackPosition();
                }

                // stay in current attack/recovery state
                return;
            }

            // Not attacking/recovering -> normal chasing behavior
            state = State.Chasing;

            float distanceToAttackTarget = float.MaxValue;
            if (TryGetAttackTargetWorldPosition(lockedAttackIndex, out Vector3 attackTargetWorldPos))
            {
                distanceToAttackTarget = Vector3.Distance(transform.position, attackTargetWorldPos);
            }

            // Stop movement only when the enemy is close to the selected attack marker.
            if (distanceToAttackTarget <= attackMarkerArrivalDistance)
            {
                isInAttackRange = true;

                if (lockedAttackIndex < 0)
                {
                    lockedAttackIndex = GetAttackIndexFromRelativePlayerSide();
                }

                // Stop movement so enemy will remain at the selected attack side.
                if (npcMovement != null) npcMovement.CancelNPCMovement();
                if (npcPath != null) npcPath.ClearPath();
            }
            else
            {
                isInAttackRange = false;
                // Build/update path towards stop position near the player
                UpdatePathToPlayer();
            }

            // Trigger attack based on maxAttackRange (actual attack range), not just stop distance
            if (distanceToPlayer <= attackRangeForCurrentState)
            {
                AttackPlayer();
            }
            else
            {
                AttackPlayerFalse();
            }
        }
        else
        {
            // Reset player detection when out of range
            if (!playerDetected) return;
            if (isDebugMode) Debug.Log("Player out of detection radius.");
            playerDetected = false;
            isInAttackRange = false;
            hasChosenAttackPosition = false; // Reset attack position flag when player leaves detection range
            lockedAttackIndex = -1;
            lockedAttackAxis = AttackAxis.None;

            // Cancel any attack movement/flags and reset animations so the enemy doesn't remain "stuck" in Attacking
            isMovingToAttackPosition = false;
            isPerformingAttack = false;

            // Stop any running attack coroutine
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            attackTimerRunning = false;

            targetPosition = null;
            if (npcMovement != null) npcMovement.CancelNPCMovement();
            if (npcPath != null) npcPath.ClearPath();

            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
                ResetAttackAnimations();
            }

            state = State.Roaming;
            StartCoroutine(RoamingRoutine());
        }
        // Check distance to target and stop if within attack range
        if (targetPosition != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

            // Check if the enemy is within the attack range
            if (distanceToTarget <= minAttackDistance)
            {
                AttackPlayer(); // Trigger the attack

                targetPosition = null; // Clear the target to stop movement
            }
            else
            {
                AttackPlayerFalse();
            }
        }

        // Handle smooth movement to attack position
        if (isMovingToAttackPosition)
        {
            UpdateMoveToAttackPosition();
        }
    }

    private void UpdateMoveToAttackPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetAttackPosition, attackPositionMoveSpeed * Time.deltaTime);

        // Check if we've reached the target position
        if (Vector3.Distance(transform.position, targetAttackPosition) < 0.01f)
        {
            transform.position = targetAttackPosition; // Snap to exact position
            isMovingToAttackPosition = false;
            if (isDebugMode) Debug.Log("Enemy reached attack position");

            BeginAttackAnimation();

            // Start attack cooldown/timer once we've reached the attack position
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private string GetStateNameFromHash(int hash)
    {
        if (hash == Animator.StringToHash("Base Layer.AttackStateName")) // Replace "Base Layer.AttackStateName" with the actual state name
        {
            return "AttackStateName";
        }
        else if (hash == Animator.StringToHash("Base Layer.IdleStateName")) // Replace "Base Layer.IdleStateName" with the actual state name
        {
            return "IdleStateName";
        }
        // Add more states as needed
        else
        {
            return "Unknown State";
        }
    }

    private void UpdatePathToPlayer()
    {
        if (isInAttackRange) return;

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer > 0f) return;

        pathUpdateTimer = pathUpdateDelay;

        if (lockedAttackIndex < 0)
        {
            lockedAttackIndex = GetAttackIndexFromRelativePlayerSide();
        }

        if (TryGetAttackTargetWorldPosition(lockedAttackIndex, out Vector3 attackTargetWorldPos))
        {
            Vector2Int markerGridTarget = GetGridTargetFromWorld(attackTargetWorldPos);
            markerGridTarget = AlignGridTargetToAttackAxis(lockedAttackIndex, markerGridTarget);

            NPCScheduleEvent markerChaseEvent = new NPCScheduleEvent(
                0, 0, 0, 0, Weather.none, Season.none, npcCurrentScene,
                new GridCoordinate(markerGridTarget.x, markerGridTarget.y), null
            );

            if (isDebugMode)
            {
                Debug.Log($"[EnemyAI] Pathing to attack index {lockedAttackIndex} world {attackTargetWorldPos} grid {markerGridTarget}");
            }

            npcPath.BuildPath(markerChaseEvent);
            return;
        }

        // compute the position that is 'attackStopDistance' away from the player on the line from player -> enemy
        Vector2 dir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        Vector2 stopPos = (Vector2)playerTransform.position + dir * attackStopDistance;

        Vector2Int gridTarget = new Vector2Int(
            Mathf.RoundToInt(stopPos.x),
            Mathf.RoundToInt(stopPos.y)
        );

        NPCScheduleEvent chaseEvent = new NPCScheduleEvent(
            0, 0, 0, 0, Weather.none, Season.none, npcCurrentScene,
            new GridCoordinate(gridTarget.x, gridTarget.y), null
        );

        npcPath.BuildPath(chaseEvent);
    }

    // Trigger attack animation when the enemy is in range
    private void AttackPlayer()
    {
        // Only choose attack position if we haven't already chosen one
        if (!hasChosenAttackPosition)
        {
            // enter attacking state and stop movement/pathing
            state = State.Attacking;
            if (npcMovement != null) npcMovement.CancelNPCMovement();
            if (npcPath != null) npcPath.ClearPath();

            if (animator != null)
            {
                ResetAttackAnimations();
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (isDebugMode) Debug.Log("Animator state: " + GetStateNameFromHash(stateInfo.fullPathHash));
            }
            else
            {
                if (isDebugMode) Debug.LogError("Animator component is null.");
            }

            // Choose attack animation based on relative position to the player
            if (playerTransform != null)
            {
                Vector2 dir = (playerTransform.position - transform.position).normalized;
                int chosenIndex = lockedAttackIndex >= 0 ? lockedAttackIndex : GetAttackIndexFromRelativePlayerSide();
                if (isDebugMode) Debug.Log($"[EnemyAI] Attacking with side index {chosenIndex}");
                lockedAttackIndex = chosenIndex;
                SetAttackAnimationByIndex(chosenIndex);
                if (animator != null)
                {
                    if (isDebugMode) Debug.Log("Deferring isAttacking until marker is reached");
                }

                // Move to the chosen attack marker unless already close enough to it.
                float distanceToAttackPosition = Vector3.Distance(transform.position, targetAttackPosition);
                bool atAttackPosition = distanceToAttackPosition <= 0.08f;

                if (atAttackPosition)
                {
                    hasChosenAttackPosition = true;
                    // start attack coroutine immediately (no movement)
                    if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                    attackCoroutine = StartCoroutine(AttackCooldownRoutine());
                    if (isDebugMode) Debug.Log("Attacking in place at attack marker");
                }
                else
                {
                    // Move to the marker that matches the attack side.
                    isMovingToAttackPosition = true;
                    hasChosenAttackPosition = true;
                    if (isDebugMode) Debug.Log($"Enemy moving to attack marker {chosenIndex} based on dir {dir}, distance {distanceToAttackPosition:F2}");
                }
            }
            else
            {
                // Fallback: if there are no child attack positions, do an in-place attack
                hasChosenAttackPosition = true;
                if (animator != null)
                {
                    ResetAttackAnimations();
                    animator.SetBool("isAttacking", true);
                }
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(AttackCooldownRoutine());
                if (isDebugMode) Debug.Log("Fallback attack in place (no child positions)");
            }
        }
    }

    // New method to set attack animation based on attack position index
    private void SetAttackAnimationByIndex(int index)
    {
        // Reset all attack animations first
        ResetAttackAnimations();

        // Prefer explicit per-side markers on the player; fallback to legacy indexed child positions.
        Transform selectedAttackPosition = GetAttackPositionTransform(index);
        if (selectedAttackPosition == null)
        {
            targetAttackPosition = transform.position;
            if (isDebugMode) Debug.LogWarning("No attack marker found; attacking in current position.");
            return;
        }

        Vector3 basePosition = selectedAttackPosition.position;

        // Set the appropriate attack animation and apply offset based on index
        switch (index)
        {
            case 0: // Down attack
                animator.SetBool("isAttackingDown", true);
                targetAttackPosition = basePosition;
                if (isDebugMode) Debug.Log("Setting attack animation: Down");
                break;
            case 1: // Up attack
                animator.SetBool("isAttackingUp", true);
                targetAttackPosition = basePosition;
                if (isDebugMode) Debug.Log("Setting attack animation: Up");
                break;
            case 2: // Right attack
                animator.SetBool("isAttackingRight", true);
                targetAttackPosition = basePosition;
                if (isDebugMode) Debug.Log("Setting attack animation: Right");
                break;
            case 3: // Left attack
                animator.SetBool("isAttackingLeft", true);
                targetAttackPosition = basePosition;
                if (isDebugMode) Debug.Log("Setting attack animation: Left");
                break;
            default:
                targetAttackPosition = basePosition; // No offset for invalid index
                if (isDebugMode) Debug.LogWarning($"Invalid attack position index: {index}");
                break;
        }
    }

    // New method to reset all attack animations
    private void ResetAttackAnimations()
    {
        animator.SetBool("isAttackingDown", false);
        animator.SetBool("isAttackingUp", false);
        animator.SetBool("isAttackingLeft", false);
        animator.SetBool("isAttackingRight", false);
    }

    // Updated AttackPlayerFalse method to also reset attack animations
    private void AttackPlayerFalse()
    {
        // Don't force-reset attack while we're already in a committed attack/recovery flow.
        if (isPerformingAttack || isMovingToAttackPosition || attackTimerRunning || state == State.Recovering)
        {
            return;
        }

        // If the enemy is still holding the correct attack position beside the player,
        // keep the attack state latched instead of flicking off for one frame.
        if (ShouldHoldAttackAnimation())
        {
            if (animator != null)
            {
                animator.SetBool("isAttacking", true);
            }
            return;
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            ResetAttackAnimations(); // Reset directional attack animations
            if (isDebugMode) Debug.Log("Setting isAttacking to false");
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (isDebugMode) Debug.Log("Animator state: " + GetStateNameFromHash(stateInfo.fullPathHash));

            // Reset attack position flag when attack animation stops
            hasChosenAttackPosition = false;
        }
        else
        {
            if (isDebugMode) Debug.LogError("Animator component is null.");
        }

        // Return to chasing if player still detected, otherwise roaming
        state = playerDetected ? State.Chasing : State.Roaming;
        // Ensure moving-to-attack is cancelled
        isMovingToAttackPosition = false;
    }

    // Visualize detection radius and attack range in the Scene view
    private void OnDrawGizmosSelected()
    {
        RecalculateAttackRanges();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Detection radius        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackEnterRange); // Attack enter range
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackExitRange); // Attack exit range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance); // Minimum attack range
    }

    private void RecalculateAttackRanges()
    {
        float minValidAttackRange = Mathf.Max(minAttackDistance, 0.05f);
        float stopDistanceBasedRange = Mathf.Max(attackStopDistance + 0.05f, minValidAttackRange);
        attackEnterRange = Mathf.Max(maxAttackRange, stopDistanceBasedRange);
        attackExitRange = Mathf.Max(attackEnterRange + Mathf.Max(attackRangeExitBuffer, 0.05f), attackEnterRange * 1.2f);
    }

    private bool TryResolvePlayerReferences()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        if (player == null)
        {
            playerTransform = null;
            return false;
        }

        playerTransform = player.transform;

        // New explicit player-side attack markers (preferred)
        attackFromLeft = FindAttackMarker("AttackFromLeft");
        attackFromRight = FindAttackMarker("AttackFromRight");
        attackFromUp = FindAttackMarker("AttackFromUp");
        attackFromDown = FindAttackMarker("AttackFromDown");

        // Legacy parent container fallback
        enemyAttackPosition = playerTransform.Find("EnemyAttackPosition");
        return true;
    }

    private void EnsurePatrolPointsAvailable()
    {
        if (TryDiscoverPatrolPointsInScene())
        {
            return;
        }

        if (HasValidPatrolPoints())
        {
            return;
        }

        if (!autoCreatePatrolPointsIfMissing)
        {
            return;
        }

        if (fallbackPatrolRoot == null)
        {
            fallbackPatrolRoot = new GameObject("GeneratedPatrolPoints").transform;
            fallbackPatrolRoot.SetParent(null, true);
        }
        else if (fallbackPatrolRoot.parent == transform)
        {
            fallbackPatrolRoot.SetParent(null, true);
        }

        if (fallbackPatrolPointA == null)
        {
            fallbackPatrolPointA = new GameObject("PointA").transform;
            fallbackPatrolPointA.SetParent(fallbackPatrolRoot, false);
        }

        if (fallbackPatrolPointB == null)
        {
            fallbackPatrolPointB = new GameObject("PointB").transform;
            fallbackPatrolPointB.SetParent(fallbackPatrolRoot, false);
        }

        Vector3 desiredA;
        Vector3 desiredB;
        bool generatedWithAStar = TryGeneratePatrolEndpointsWithAStar(out desiredA, out desiredB);

        if (!generatedWithAStar)
        {
            float halfDistance = Mathf.Max(0.2f, fallbackPatrolHalfDistance);
            desiredA = transform.position + (Vector3.left * halfDistance);
            desiredB = transform.position + (Vector3.right * halfDistance);
        }

        fallbackPatrolPointA.position = ClampToPatrolArea(desiredA);
        fallbackPatrolPointB.position = ClampToPatrolArea(desiredB);
        patrolPoints = new Transform[] { fallbackPatrolPointA, fallbackPatrolPointB };
    }

    private bool TryDiscoverPatrolPointsInScene()
    {
        if (!autoDiscoverPatrolPointsInScene)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(patrolPointTag))
        {
            return TryDiscoverPatrolPointsFromParentObject();
        }

        if (TryDiscoverPatrolPointsFromParentObject())
        {
            return true;
        }

        GameObject[] taggedPatrolObjects;
        try
        {
            taggedPatrolObjects = GameObject.FindGameObjectsWithTag(patrolPointTag);
        }
        catch (UnityException)
        {
            if (isDebugMode)
            {
                Debug.LogWarning($"[EnemyAI] Patrol tag '{patrolPointTag}' does not exist. Skipping scene patrol discovery.");
            }
            return false;
        }

        if (taggedPatrolObjects == null || taggedPatrolObjects.Length == 0)
        {
            return false;
        }

        float searchRadiusSqr = patrolPointSearchRadius > 0f ? patrolPointSearchRadius * patrolPointSearchRadius : -1f;

        List<Transform> candidatePoints = taggedPatrolObjects
            .Where(o => o != null && o.scene == gameObject.scene)
            .Select(o => o.transform)
            .Where(t =>
            {
                if (t == null) return false;
                if (searchRadiusSqr > 0f)
                {
                    float sqrDistance = ((Vector2)(t.position - transform.position)).sqrMagnitude;
                    if (sqrDistance > searchRadiusSqr) return false;
                }

                if (patrolAreaCollider != null && !patrolAreaCollider.OverlapPoint(t.position))
                {
                    return false;
                }

                return true;
            })
            .ToList();

        Transform[] discoveredPoints = SelectPatrolPointsForCoverage(candidatePoints);

        if (discoveredPoints.Length < 2)
        {
            return false;
        }

        patrolPoints = discoveredPoints;
        if (isDebugMode)
        {
            Debug.Log($"[EnemyAI] Auto-discovered {patrolPoints.Length} patrol points using tag '{patrolPointTag}'.");
        }
        return true;
    }

    private bool TryDiscoverPatrolPointsFromParentObject()
    {
        if (string.IsNullOrWhiteSpace(checkpointParentObjectName))
        {
            return false;
        }

        GameObject parentObject = GameObject.Find(checkpointParentObjectName);
        if (parentObject == null || parentObject.scene != gameObject.scene)
        {
            return false;
        }

        List<Transform> candidatePoints = parentObject.transform
            .Cast<Transform>()
            .Where(t => t != null)
            .Where(t =>
            {
                if (patrolAreaCollider != null && !patrolAreaCollider.OverlapPoint(t.position))
                {
                    return false;
                }

                return true;
            })
            .ToList();

        Transform[] discoveredPoints = SelectPatrolPointsForCoverage(candidatePoints);

        if (discoveredPoints.Length < 2)
        {
            return false;
        }

        patrolPoints = discoveredPoints;
        if (isDebugMode)
        {
            Debug.Log($"[EnemyAI] Auto-discovered {patrolPoints.Length} patrol points from parent '{checkpointParentObjectName}'.");
        }

        return true;
    }

    private Transform[] SelectPatrolPointsForCoverage(List<Transform> candidatePoints)
    {
        if (candidatePoints == null || candidatePoints.Count == 0)
        {
            return new Transform[0];
        }

        int desiredCount = maxDiscoveredPatrolPoints <= 0
            ? candidatePoints.Count
            : Mathf.Clamp(maxDiscoveredPatrolPoints, 2, candidatePoints.Count);

        if (desiredCount >= candidatePoints.Count)
        {
            return candidatePoints.ToArray();
        }

        // Seed near the enemy, then spread out to cover more of the patrol network.
        List<Transform> selectedPoints = new List<Transform>(desiredCount);
        Transform seedPoint = candidatePoints
            .OrderBy(t => ((Vector2)(t.position - transform.position)).sqrMagnitude)
            .FirstOrDefault();

        if (seedPoint == null)
        {
            return new Transform[0];
        }

        selectedPoints.Add(seedPoint);

        while (selectedPoints.Count < desiredCount)
        {
            Transform farthestPoint = null;
            float farthestMinDistance = float.MinValue;

            for (int i = 0; i < candidatePoints.Count; i++)
            {
                Transform candidate = candidatePoints[i];
                if (candidate == null || selectedPoints.Contains(candidate))
                {
                    continue;
                }

                float minDistanceToSelected = float.MaxValue;
                for (int j = 0; j < selectedPoints.Count; j++)
                {
                    float distance = ((Vector2)(candidate.position - selectedPoints[j].position)).sqrMagnitude;
                    if (distance < minDistanceToSelected)
                    {
                        minDistanceToSelected = distance;
                    }
                }

                if (minDistanceToSelected > farthestMinDistance)
                {
                    farthestMinDistance = minDistanceToSelected;
                    farthestPoint = candidate;
                }
            }

            if (farthestPoint == null)
            {
                break;
            }

            selectedPoints.Add(farthestPoint);
        }

        return selectedPoints.ToArray();
    }

    private Vector3 GetPatrolPointWorldPosition(int patrolIndex)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return transform.position;
        }

        int clampedIndex = Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1);
        Transform point = patrolPoints[clampedIndex];
        if (point == null)
        {
            return transform.position;
        }

        return ClampToPatrolArea(point.position);
    }

    private Vector3 ClampToPatrolArea(Vector3 worldPosition)
    {
        if (patrolAreaCollider == null)
        {
            return worldPosition;
        }

        Vector2 clampedPoint = patrolAreaCollider.ClosestPoint(worldPosition);
        return new Vector3(clampedPoint.x, clampedPoint.y, worldPosition.z);
    }

    private bool HasValidPatrolPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGeneratePatrolEndpointsWithAStar(out Vector3 pointA, out Vector3 pointB)
    {
        pointA = transform.position;
        pointB = transform.position;

        if (!useAStarForGeneratedPatrolPoints || NPCManager.Instance == null)
        {
            return false;
        }

        int maxCells = Mathf.Max(1, generatedPatrolSearchCells);
        Vector2Int startGrid = GetGridTargetFromWorld(transform.position);

        bool hasLeft = TryFindReachableInDirection(startGrid, Vector2Int.left, maxCells, out Vector2Int leftGrid, out int leftDistance);
        bool hasRight = TryFindReachableInDirection(startGrid, Vector2Int.right, maxCells, out Vector2Int rightGrid, out int rightDistance);
        bool hasUp = TryFindReachableInDirection(startGrid, Vector2Int.up, maxCells, out Vector2Int upGrid, out int upDistance);
        bool hasDown = TryFindReachableInDirection(startGrid, Vector2Int.down, maxCells, out Vector2Int downGrid, out int downDistance);

        bool hasHorizontalPair = hasLeft && hasRight;
        bool hasVerticalPair = hasUp && hasDown;

        if (hasHorizontalPair && (!hasVerticalPair || (leftDistance + rightDistance) >= (upDistance + downDistance)))
        {
            pointA = GridToWorldCenter(leftGrid);
            pointB = GridToWorldCenter(rightGrid);
            return true;
        }

        if (hasVerticalPair)
        {
            pointA = GridToWorldCenter(downGrid);
            pointB = GridToWorldCenter(upGrid);
            return true;
        }

        // If only one side is reachable, pair it with the start position.
        if (hasLeft)
        {
            pointA = GridToWorldCenter(leftGrid);
            pointB = GridToWorldCenter(startGrid);
            return true;
        }

        if (hasRight)
        {
            pointA = GridToWorldCenter(startGrid);
            pointB = GridToWorldCenter(rightGrid);
            return true;
        }

        if (hasDown)
        {
            pointA = GridToWorldCenter(downGrid);
            pointB = GridToWorldCenter(startGrid);
            return true;
        }

        if (hasUp)
        {
            pointA = GridToWorldCenter(startGrid);
            pointB = GridToWorldCenter(upGrid);
            return true;
        }

        return false;
    }

    private bool TryFindReachableInDirection(Vector2Int startGrid, Vector2Int direction, int maxCells, out Vector2Int bestGrid, out int bestDistance)
    {
        bestGrid = startGrid;
        bestDistance = 0;

        // Collect every valid candidate along with its actual A* path length.
        List<Vector2Int> candidateCells = new List<Vector2Int>();
        List<int> candidateSteps = new List<int>();

        for (int step = 1; step <= maxCells; step++)
        {
            Vector2Int candidateGrid = startGrid + (direction * step);

            if (!IsGridCellInCurrentSceneBounds(candidateGrid))
                continue;

            if (IsGridCellBlockedForNPC(candidateGrid))
                continue;

            Vector3 candidateWorld = GridToWorldCenter(candidateGrid);

            if (patrolAreaCollider != null && !patrolAreaCollider.OverlapPoint(candidateWorld))
                continue;

            int pathSteps = GetAStarPathSteps(startGrid, candidateGrid);
            if (pathSteps > 0)
            {
                candidateCells.Add(candidateGrid);
                candidateSteps.Add(pathSteps);
            }
        }

        if (candidateCells.Count == 0)
            return false;

        // Prefer candidates whose A* path is at least half of the longest found,
        // so the chosen point is genuinely far rather than always the nearest cell.
        int maxSteps = 0;
        for (int i = 0; i < candidateSteps.Count; i++)
            if (candidateSteps[i] > maxSteps) maxSteps = candidateSteps[i];

        int minAcceptable = Mathf.Max(1, maxSteps / 2);

        List<Vector2Int> preferred = new List<Vector2Int>();
        List<int> preferredSteps = new List<int>();
        for (int i = 0; i < candidateCells.Count; i++)
        {
            if (candidateSteps[i] >= minAcceptable)
            {
                preferred.Add(candidateCells[i]);
                preferredSteps.Add(candidateSteps[i]);
            }
        }

        // Fall back to all candidates if the preferred pool is empty.
        List<Vector2Int> pool = preferred.Count > 0 ? preferred : candidateCells;
        List<int> poolSteps = preferred.Count > 0 ? preferredSteps : candidateSteps;

        int pick = Random.Range(0, pool.Count);
        bestGrid = pool[pick];
        bestDistance = poolSteps[pick];
        return true;
    }

    // Returns the number of A* path steps from startGrid to targetGrid,
    // or -1 if no path exists or the target is blocked.
    private int GetAStarPathSteps(Vector2Int startGrid, Vector2Int targetGrid)
    {
        if (NPCManager.Instance == null)
            return -1;

        if (IsGridCellBlockedForNPC(targetGrid))
            return -1;

        Stack<NPCMovementStep> tempPath = new Stack<NPCMovementStep>();
        bool found = NPCManager.Instance.BuildPath(npcCurrentScene, startGrid, targetGrid, tempPath);
        return found ? tempPath.Count : -1;
    }

    private bool IsGridCellInCurrentSceneBounds(Vector2Int gridPosition)
    {
        if (GridPropertiesManager.Instance == null)
        {
            return true;
        }

        if (!GridPropertiesManager.Instance.GetGridDimensions(npcCurrentScene, out Vector2Int gridDimensions, out Vector2Int gridOrigin))
        {
            return true;
        }

        int maxX = gridOrigin.x + gridDimensions.x - 1;
        int maxY = gridOrigin.y + gridDimensions.y - 1;
        return gridPosition.x >= gridOrigin.x && gridPosition.x <= maxX && gridPosition.y >= gridOrigin.y && gridPosition.y <= maxY;
    }

    private bool IsGridCellBlockedForNPC(Vector2Int gridPosition)
    {
        if (GridPropertiesManager.Instance == null)
        {
            return false;
        }

        GridPropertyDetails details = GridPropertiesManager.Instance.GetGridPropertyDetails(gridPosition.x, gridPosition.y);
        if (details == null)
        {
            // Treat missing grid data as blocked to avoid generating out-of-map patrol points.
            return true;
        }

        return details.isNPCObstacle;
    }

    private Vector3 GridToWorldCenter(Vector2Int gridPosition)
    {
        if (sceneGrid == null)
        {
            sceneGrid = FindObjectOfType<Grid>();
        }

        if (sceneGrid != null)
        {
            return sceneGrid.GetCellCenterWorld(new Vector3Int(gridPosition.x, gridPosition.y, 0));
        }

        return new Vector3(gridPosition.x, gridPosition.y, transform.position.z);
    }

    private Transform FindAttackMarker(string markerName)
    {
        if (player == null) return null;

        // Search entire player hierarchy recursively so markers can be nested anywhere.
        Transform marker = FindDeepChild(player.transform, markerName);

        if (marker != null)
        {
            Debug.Log($"[EnemyAI] Found attack marker '{markerName}' at {marker.position} (path: {GetTransformPath(marker)})");
        }
        else
        {
            Debug.LogWarning($"[EnemyAI] Attack marker '{markerName}' NOT found anywhere under player '{player.name}'. Enemy will use legacy fallback and may always approach from top.");
        }

        return marker;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindDeepChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private string GetTransformPath(Transform t)
    {
        string path = t.name;
        Transform current = t.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    private Transform GetAttackPositionTransform(int attackIndex)
    {
        // Animation index -> where enemy should stand relative to player.
        // 0 = attack down  => enemy above player
        // 1 = attack up    => enemy below player
        // 2 = attack right => enemy left of player
        // 3 = attack left  => enemy right of player
        switch (attackIndex)
        {
            case 0:
                if (attackFromUp != null) return attackFromUp;
                break;
            case 1:
                if (attackFromDown != null) return attackFromDown;
                break;
            case 2:
                if (attackFromLeft != null) return attackFromLeft;
                break;
            case 3:
                if (attackFromRight != null) return attackFromRight;
                break;
        }

        // Backward-compatible fallback: indexed children in EnemyAttackPosition parent.
        if (enemyAttackPosition != null && enemyAttackPosition.childCount > 0)
        {
            int clampedIndex = Mathf.Clamp(attackIndex, 0, enemyAttackPosition.childCount - 1);
            return enemyAttackPosition.GetChild(clampedIndex);
        }

        return null;
    }

    private void ResetMovementAnimation()
    {
        // Reset all movement animation parameters
        animator.SetBool(Settings.walkRight, false);
        animator.SetBool(Settings.walkLeft, false);
        animator.SetBool(Settings.walkUp, false);
        animator.SetBool(Settings.walkDown, false);

        animator.SetBool(Settings.idleRight, false);
        animator.SetBool(Settings.idleLeft, false);
        animator.SetBool(Settings.idleUp, false);
        animator.SetBool(Settings.idleDown, false);
    }
    public void AttackPlayerByAnimation()
    {
        if (player == null)
        {
            return;
        }

        Character targetCharacter = player.GetComponent<Character>();
        if (targetCharacter != null)
        {
            targetCharacter.TakeDamage(attackDamage);
        }
    }

    // Coroutine that pauses movement while the attack animation plays, then resumes behaviour
    private IEnumerator AttackCooldownRoutine()
    {
        if (attackTimerRunning) yield break;
        attackTimerRunning = true;

        // Ensure movement/pathing is stopped during attack
        isPerformingAttack = true;
        if (npcMovement != null) npcMovement.CancelNPCMovement();
        if (npcPath != null) npcPath.ClearPath();

        if (animator != null)
        {
            BeginAttackAnimation();
        }

        // Let animator process the transition, then resolve clip-driven duration.
        yield return null;

        float resolvedAttackDuration = Mathf.Max(attackDuration, GetCurrentAttackClipDuration());
        if (isDebugMode) Debug.Log($"Attack started - pausing movement for {resolvedAttackDuration:F2}s");
        yield return new WaitForSeconds(resolvedAttackDuration);

        hasChosenAttackPosition = false;
        isPerformingAttack = false;
        isMovingToAttackPosition = false;

        // Enter a short recovery period after attack before resuming chase/roam
        state = State.Recovering;
        if (isDebugMode) Debug.Log($"Entering recovery for {recoveryDuration}s");
        yield return new WaitForSeconds(recoveryDuration);

        // Decide next state after recovery
        if (playerDetected)
        {
            state = State.Chasing;
            if (isDebugMode) Debug.Log("Recovery ended - switching to Chasing");
        }
        else
        {
            state = State.Roaming;
            if (isDebugMode) Debug.Log("Recovery ended - switching to Roaming");
            StartCoroutine(RoamingRoutine());
        }

        bool shouldHoldAttackAnimation = ShouldHoldAttackAnimation();
        if (animator != null && !shouldHoldAttackAnimation)
        {
            animator.SetBool("isAttacking", false);
            ResetAttackAnimations();
        }

        attackTimerRunning = false;
        attackCoroutine = null;
        if (isDebugMode) Debug.Log("Attack ended - resuming behaviour");
    }

    private float GetCurrentAttackClipDuration()
    {
        if (animator == null)
        {
            return attackDuration;
        }

        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips != null && clips.Length > 0 && clips[0].clip != null)
        {
            float speed = Mathf.Abs(animator.speed);
            if (speed < 0.01f)
            {
                speed = 1f;
            }

            return clips[0].clip.length / speed;
        }

        return attackDuration;
    }

    private void BeginAttackAnimation()
    {
        if (animator == null)
        {
            return;
        }

        // Clear locomotion/idle parameters so they can't override the attack state.
        ResetMovementAnimation();

        if (lockedAttackIndex >= 0)
        {
            SetAttackAnimationByIndex(lockedAttackIndex);
            if (isDebugMode) Debug.Log($"[EnemyAI] Applying attack direction at swing start. index={lockedAttackIndex}");
        }

        animator.SetBool("isAttacking", true);
    }

    private bool ShouldHoldAttackAnimation()
    {
        if (!playerDetected || lockedAttackIndex < 0)
        {
            return false;
        }

        if (!TryGetAttackTargetWorldPosition(lockedAttackIndex, out Vector3 attackTargetWorldPos))
        {
            return false;
        }

        return Vector3.Distance(transform.position, attackTargetWorldPos) <= attackMarkerArrivalDistance + 0.05f;
    }

    // New helper to convert a direction vector to attack index
    private int GetAttackIndexFromDirection(Vector2 dir)
    {
        // If horizontal component is dominant -> horizontal attack
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? 2 : 3; // right : left
        }
        else
        {
            return dir.y > 0 ? 1 : 0; // up : down
        }
    }

    private int GetAttackIndexFromWalkingDirection(Vector2 fallbackDir)
    {
        Vector2 dirToUse = lastWalkingDirection;
        if (dirToUse.sqrMagnitude < 0.0001f)
        {
            dirToUse = fallbackDir;
        }

        // Walking direction tells us from which side enemy approached:
        // moving right => enemy should stand left of player, and vice versa.
        if (Mathf.Abs(dirToUse.x) > Mathf.Abs(dirToUse.y))
        {
            return dirToUse.x > 0f ? 2 : 3;
        }

        return dirToUse.y > 0f ? 1 : 0;
    }

    private int GetAttackIndexFromApproachDirection(Vector2 fallbackDir)
    {
        Vector2 dirToUse = lastWalkingDirection;
        if (dirToUse.sqrMagnitude < 0.0001f)
        {
            dirToUse = fallbackDir;
        }

        float absX = Mathf.Abs(dirToUse.x);
        float absY = Mathf.Abs(dirToUse.y);

        // If we have meaningful horizontal approach velocity, commit to left/right side selection.
        if (absX >= horizontalApproachThreshold)
        {
            return dirToUse.x > 0f ? 2 : 3;
        }

        // Otherwise use vertical approach.
        if (absY >= horizontalApproachThreshold)
        {
            return dirToUse.y > 0f ? 1 : 0;
        }

        // Final fallback: choose based on enemy side relative to player.
        Vector2 sideOffset = (Vector2)transform.position - (Vector2)playerTransform.position;
        if (Mathf.Abs(sideOffset.x) >= Mathf.Abs(sideOffset.y))
        {
            return sideOffset.x < 0f ? 2 : 3;
        }

        return sideOffset.y > 0f ? 0 : 1;
    }

    private int GetClosestAttackIndexToEnemy()
    {
        int closestIndex = -1;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            if (!TryGetAttackTargetWorldPosition(i, out Vector3 markerWorldPos)) continue;

            float sqrDistance = ((Vector2)(transform.position - markerWorldPos)).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestIndex = i;
            }
        }

        if (closestIndex >= 0)
        {
            return closestIndex;
        }

        return GetAttackIndexFromRelativePlayerSide();
    }

    private Vector2Int GetGridTargetFromWorld(Vector3 worldPosition)
    {
        if (sceneGrid == null)
        {
            sceneGrid = FindObjectOfType<Grid>();
        }

        if (sceneGrid != null)
        {
            Vector3Int gridCell = sceneGrid.WorldToCell(worldPosition);
            return new Vector2Int(gridCell.x, gridCell.y);
        }

        return new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
    }

    private Vector2Int AlignGridTargetToAttackAxis(int attackIndex, Vector2Int markerGridTarget)
    {
        if (playerTransform == null)
        {
            return markerGridTarget;
        }

        Vector2Int playerGridTarget = GetGridTargetFromWorld(playerTransform.position);

        // Keep side attacks on player's row and vertical attacks on player's column.
        if (attackIndex == 2 || attackIndex == 3)
        {
            markerGridTarget.y = playerGridTarget.y;
        }
        else if (attackIndex == 0 || attackIndex == 1)
        {
            markerGridTarget.x = playerGridTarget.x;
        }

        return markerGridTarget;
    }

    private int GetAttackIndexFromRelativePlayerSide()
    {
        Vector2 sideOffset = (Vector2)transform.position - (Vector2)playerTransform.position;
        float absSideX = Mathf.Abs(sideOffset.x);
        float absSideY = Mathf.Abs(sideOffset.y);

        if (lockedAttackAxis == AttackAxis.Horizontal)
        {
            return sideOffset.x < 0f ? 2 : 3;
        }

        if (lockedAttackAxis == AttackAxis.Vertical)
        {
            return sideOffset.y > 0f ? 0 : 1;
        }

        // Slight horizontal bias avoids switching to top/bottom when near player center.
        if (absSideX >= absSideY * 0.7f)
        {
            return sideOffset.x < 0f ? 2 : 3;
        }

        return sideOffset.y > 0f ? 0 : 1;
    }

    private bool TryGetAttackTargetWorldPosition(int attackIndex, out Vector3 worldPosition)
    {
        worldPosition = transform.position;

        Transform selectedAttackPosition = GetAttackPositionTransform(attackIndex);
        if (selectedAttackPosition == null)
        {
            return false;
        }

        Vector3 basePosition = selectedAttackPosition.position;
        switch (attackIndex)
        {
            case 0:
                worldPosition = basePosition;
                return true;
            case 1:
                worldPosition = basePosition;
                return true;
            case 2:
                worldPosition = basePosition;
                return true;
            case 3:
                worldPosition = basePosition;
                return true;
            default:
                worldPosition = basePosition;
                return true;
        }
    }

    private int GetAttackIndexForCurrentPosition(Vector2 fallbackDir)
    {
        Transform[] markerByIndex = new Transform[4]
        {
            attackFromUp,    // index 0 -> down attack, enemy stands above player
            attackFromDown,  // index 1 -> up attack, enemy stands below player
            attackFromLeft,  // index 2 -> right attack, enemy stands left of player
            attackFromRight  // index 3 -> left attack, enemy stands right of player
        };

        int nearestIndex = -1;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < markerByIndex.Length; i++)
        {
            Transform marker = markerByIndex[i];
            if (marker == null) continue;

            float sqrDistance = ((Vector2)(transform.position - marker.position)).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestIndex = i;
            }
        }

        if (nearestIndex >= 0)
        {
            return nearestIndex;
        }

        return GetAttackIndexFromDirection(fallbackDir);
    }

    private void TrackWalkingDirection()
    {
        Vector2 frameDelta = (Vector2)(transform.position - previousFramePosition);
        if (frameDelta.sqrMagnitude > 0.0004f)
        {
            lastWalkingDirection = frameDelta.normalized;
        }

        previousFramePosition = transform.position;
    }

    private void LogAttackSideDebug(float distanceToPlayer)
    {
        if (!isDebugMode || playerTransform == null) return;

        sideDebugLogTimer -= Time.deltaTime;
        if (sideDebugLogTimer > 0f) return;

        sideDebugLogTimer = SideDebugLogInterval;

        Vector2 sideOffset = (Vector2)transform.position - (Vector2)playerTransform.position;
        Debug.Log($"[EnemyAI SideDebug] lock={lockedAttackIndex} walkDir={lastWalkingDirection} sideOffset={sideOffset} inRange={isInAttackRange} dPlayer={distanceToPlayer:F2}");
    }
    public void BuildPath(NPCScheduleEvent scheduleEvent)
    {
        if (npcPath != null)
        {
            npcPath.BuildPath(scheduleEvent);
        }
        else if (isDebugMode)
        {
            Debug.LogWarning("E_EnemyAI.BuildPath called but npcPath is null.");
        }
    }
}