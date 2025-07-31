using System.Collections;
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
    [SerializeField] private float attackPositionMoveSpeed = 5f; // Speed for moving to attack position

    private GameObject player; // Reference to the player GameObject
    //private Vector2Int playerGridPosition; // Player's grid position for pathfinding  
    private Transform playerTransform;
    private Transform enemyAttackPosition; // The position where the enemy attacks from
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

    [SerializeField] private NPCMovement npcMovement; // Reference to the NPCMovement script

    [SerializeField] private Vector2 attackOffsetDown = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetUp = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetRight = Vector2.zero;
    [SerializeField] private Vector2 attackOffsetLeft = Vector2.zero;

    private enum State
    {
        Roaming,
        Chasing,
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
        // Find player in the scene
        player = GameObject.FindWithTag("Player");
        //get the child gameobject of the player named EnemyAttackPosition
        enemyAttackPosition = player.transform.Find("EnemyAttackPosition");

        playerTransform = player.transform;

        GetComponent<NPCMovement>().EnemyAfterSceneLoad();

        npcPath = GetComponent<NPCPath>();

        animator.SetBool("isAttacking", false); // Ensure attack animation is not playing at start

        StartResetAnimation();

        enemyCollider = GetComponent<Collider2D>();

        // Start periodic checks
        InvokeRepeating(nameof(CheckForCollision), checkInterval, checkInterval);
    }

    private void StartResetAnimation()
    {

        ResetMovementAnimation(); // Reset all movement animations

        state = State.Roaming; // Set initial state to Roaming

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

            Vector2 roamPosition = GetRoamingPosition();

            // Determine and set movement direction before moving
            SetMovementAnimation(roamPosition);

            enemyPathfinding.MoveTo(roamPosition);
            // Wait for a short duration to simulate roaming between roamDuration and 2f
            yield return new WaitForSeconds(Random.Range(roamDuration, 2f));
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized;
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
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            if (isDebugMode) Debug.Log("Player detected within detection radius."); state = State.Chasing;
            // Set playerDetected to true
            playerDetected = true;
            // If the enemy is within the max attack distance, stop updating the path
            if (distanceToPlayer <= maxAttackRange)
            {
                isInAttackRange = true;
                targetPosition = playerTransform; // Set the target to the player's position
                npcMovement.CancelNPCMovement(); // Stop moving once attacking
            }
            else
            {
                isInAttackRange = false;

                if (targetPosition == null || Vector3.Distance(transform.position, targetPosition.position) > maxAttackRange)
                {
                    UpdatePathToPlayer(); // Update the path to the player
                }
            }
        }
        else
        {
            // Reset player detection when out of range
            if (playerDetected == false) return;
            if (isDebugMode) Debug.Log("Player out of detection radius.");
            playerDetected = false;
            isInAttackRange = false;
            hasChosenAttackPosition = false; // Reset attack position flag when player leaves detection range
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
            transform.position = Vector3.MoveTowards(transform.position, targetAttackPosition, attackPositionMoveSpeed * Time.deltaTime);

            // Check if we've reached the target position
            if (Vector3.Distance(transform.position, targetAttackPosition) < 0.01f)
            {
                transform.position = targetAttackPosition; // Snap to exact position
                isMovingToAttackPosition = false;
                if (isDebugMode) Debug.Log("Enemy reached attack position");
            }
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
        // Exit if the enemy is in attack range
        if (isInAttackRange)
        {
            return;
        }

        pathUpdateTimer -= Time.deltaTime;

        // Only update the path if the distance to the target is significant
        if (pathUpdateTimer <= 0f)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition != null ? targetPosition.position : playerTransform.position);


            // Update the path only if the enemy is far from the target
            if (distanceToTarget >= pathFindingStopDistance) // Adding a buffer
            {
                pathUpdateTimer = pathUpdateDelay; // Reset the timer
                targetPosition = playerTransform; // Set the target to the player's position

                if (targetPosition != null)
                {
                    Vector2Int gridPosition = new Vector2Int(
                        Mathf.RoundToInt(targetPosition.position.x),
                        Mathf.RoundToInt(targetPosition.position.y)
                    );

                    finishPosition = gridPosition;

                    NPCScheduleEvent enemyChaseEvent = new NPCScheduleEvent(
                        0, 0, 0, 0, Weather.none, Season.none, npcCurrentScene,
                        new GridCoordinate(finishPosition.x, finishPosition.y), null
                    );

                    npcPath.BuildPath(enemyChaseEvent); // Build the path using A*
                    if (isDebugMode) Debug.Log("Path to player updated." + finishPosition);
                }
            }
        }
    }

    // Trigger attack animation when the enemy is in range
    private void AttackPlayer()
    {
        // Only choose attack position if we haven't already chosen one
        if (!hasChosenAttackPosition)
        {
            if (animator != null)
            {
                animator.SetBool("isAttacking", true); // Play attack animation
                if (isDebugMode) Debug.Log("Setting isAttacking to true");
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (isDebugMode) Debug.Log("Animator state: " + GetStateNameFromHash(stateInfo.fullPathHash));
            }
            else
            {
                if (isDebugMode) Debug.LogError("Animator component is null.");
            }

            npcPath.ClearPath(); // Stop moving once the enemy attacks

            // Choose a random attack position from the 4 child objects
            if (enemyAttackPosition != null && enemyAttackPosition.childCount > 0)
            {
                int randomIndex = Random.Range(0, enemyAttackPosition.childCount);

                // Move to attack position and set animation (now handles offset internally)
                isMovingToAttackPosition = true;
                hasChosenAttackPosition = true; // Mark that we've chosen an attack position

                // Set attack animation based on random index (this now also sets targetAttackPosition with offset)
                SetAttackAnimationByIndex(randomIndex);

                if (isDebugMode) Debug.Log($"Enemy moving to attack position {randomIndex} with offset");
            }
        }
    }

    // New method to set attack animation based on attack position index
    private void SetAttackAnimationByIndex(int index)
    {
        // Reset all attack animations first
        ResetAttackAnimations();

        // Get the base attack position
        Transform selectedAttackPosition = enemyAttackPosition.GetChild(index);
        Vector3 basePosition = selectedAttackPosition.position;

        // Set the appropriate attack animation and apply offset based on index
        switch (index)
        {
            case 0: // Down attack
                animator.SetBool("isAttackingDown", true);
                targetAttackPosition = basePosition + (Vector3)attackOffsetDown;
                if (isDebugMode) Debug.Log("Setting attack animation: Down with offset: " + attackOffsetDown);
                break;
            case 1: // Up attack
                animator.SetBool("isAttackingUp", true);
                targetAttackPosition = basePosition + (Vector3)attackOffsetUp;
                if (isDebugMode) Debug.Log("Setting attack animation: Up with offset: " + attackOffsetUp);
                break;
            case 2: // Right attack
                animator.SetBool("isAttackingRight", true);
                targetAttackPosition = basePosition + (Vector3)attackOffsetRight;
                if (isDebugMode) Debug.Log("Setting attack animation: Right with offset: " + attackOffsetRight);
                break;
            case 3: // Left attack
                animator.SetBool("isAttackingLeft", true);
                targetAttackPosition = basePosition + (Vector3)attackOffsetLeft;
                if (isDebugMode) Debug.Log("Setting attack animation: Left with offset: " + attackOffsetLeft);
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
    }

    // Visualize detection radius and attack range in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Detection radius        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange); // Maximum attack range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance); // Minimum attack range
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
        player.GetComponent<Character>().TakeDamage(attackDamage);
    }
}