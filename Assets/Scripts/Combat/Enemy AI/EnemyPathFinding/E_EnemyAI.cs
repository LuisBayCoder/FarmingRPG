using System.Collections;
using UnityEngine;
using System.Linq;

public class E_EnemyAI : MonoBehaviour
{   // [SerializeField] private Transform[] attackPositions; // The 4 positions around the player (left, right, up, down)
    [SerializeField] private float detectionRadius = 5f; // Detection radius
    [SerializeField] private float minAttackDistance = 0.1f; // Minimum distance to trigger attack
    [SerializeField] private float maxChaseRange = 0.3f; // Maximum distance to trigger attack
    [SerializeField] private float stopDistance = 1.75f;
    [SerializeField] private float pathUpdateDelay = 0.5f; // Time between path recalculations
    [SerializeField] private NPCPath npcPath = null; // A* pathfinding script
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private bool isDebugMode = false; // Debug mode flag

    private Transform player;
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

    private enum State
    {
        Roaming,
        Chasing,
        Avoiding
    }

    private State state; // Current state of the enemy
    private EnemyPathfinding enemyPathfinding;
    private Vector3 originalTargetPosition; // Store the target position before avoiding

    private void Awake()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            if (isDebugMode) Debug.LogError("Animator component not found in Awake.");
        }
        else
        {
            if (isDebugMode) Debug.Log("Animator component found in Awake.");
        }

        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }

    private void Start()
    {
        // Find player in the scene
        player = GameObject.FindGameObjectWithTag("Player").transform; 

        GetComponent<NPCMovement>().EnemyAfterSceneLoad();

        npcPath = GetComponent<NPCPath>();

        StartCoroutine(RoamingRoutine());

        enemyCollider = GetComponent<Collider2D>();

        // Start periodic checks
        InvokeRepeating(nameof(CheckForCollision), checkInterval, checkInterval);
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
            Vector2 roamPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamPosition);
            yield return new WaitForSeconds(2f);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized; 
    }

    private void Update()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            if (isDebugMode) Debug.Log("Player detected within detection radius.");            state = State.Chasing; 
            // Set playerDetected to true
            playerDetected = true;
            // If the enemy is within the max attack distance, stop updating the path
            if (distanceToPlayer <= maxChaseRange)
            {
                isInAttackRange = true;
                targetPosition = player; // Set the target to the player's position
            }
            else            {
                isInAttackRange = false;

                if (targetPosition == null || Vector3.Distance(transform.position, targetPosition.position) > maxChaseRange)
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
                npcPath.ClearPath(); // Stop moving once attacking
                targetPosition = null; // Clear the target to stop movement
            }
            else
            {
                AttackPlayerFalse();
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
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition != null ? targetPosition.position : player.position);


            // Update the path only if the enemy is far from the target
            if (distanceToTarget >= stopDistance) // Adding a buffer
            {
                pathUpdateTimer = pathUpdateDelay; // Reset the timer
                targetPosition = player; // Set the target to the player's position

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
        DeterminePlayerDirection(); // Determine player direction for the attack animation
    }

    // This method is called by the animation event when the enemy is attacking
    //even if the player is out of range it will get hit. Fix needed.
    public void AttackPlayerByAnimation()
    {
        // Deal damage to the player
        Character playerCharacter = player.GetComponent<Character>();
        if (playerCharacter != null)
        {
            playerCharacter.TakeDamage(attackDamage); // Deal 10 damage (you can adjust the damage amount as needed)
        }
    }

    private void AttackPlayerFalse()
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            if (isDebugMode) Debug.Log("Setting isAttacking to false");
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (isDebugMode) Debug.Log("Animator state: " + GetStateNameFromHash(stateInfo.fullPathHash));
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
        Gizmos.DrawWireSphere(transform.position, maxChaseRange); // Maximum attack range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance); // Minimum attack range
    }

    private void DeterminePlayerDirection()
    {
        Vector3 directionToPlayer = player.position - transform.position;

        // Check the horizontal and vertical direction to determine the quadrant
        if (directionToPlayer.x > 0 && directionToPlayer.y > 0)
        {
            if (isDebugMode) Debug.Log("Player is above and to the right");
            // Handle logic when the player is above and to the right
            animator.SetBool("isAttackingLeft", false);
            animator.SetBool("isAttackingRight", true);
        }
        else if (directionToPlayer.x < 0 && directionToPlayer.y > 0)
        {
            if (isDebugMode) Debug.Log("Player is above and to the left");
            // Handle logic when the player is above and to the left
            animator.SetBool("isAttackingRight", false);
            animator.SetBool("isAttackingLeft", true);
        }
        else if (directionToPlayer.x > 0 && directionToPlayer.y < 0)
        {
            if (isDebugMode) Debug.Log("Player is below and to the right");
            // Handle logic when the player is below and to the right
            animator.SetBool("isAttackingLeft", false);
            animator.SetBool("isAttackingRight", true);
        }
        else if (directionToPlayer.x < 0 && directionToPlayer.y < 0)
        {
            if (isDebugMode) Debug.Log("Player is below and to the left");
            // Handle logic when the player is below and to the left
            animator.SetBool("isAttackingRight", false);
            animator.SetBool("isAttackingLeft", true);
        }
        else if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.y))
        {
            // The player is directly to the left or right
            if (directionToPlayer.x > 0)
            {
                if (isDebugMode) Debug.Log("Player is directly to the right");
                // Handle logic when the player is directly to the right
                animator.SetBool("isAttackingLeft", false);
                animator.SetBool("isAttackingRight", true);
            }
            else
            {
                if (isDebugMode) Debug.Log("Player is directly to the left");
                // Handle logic when the player is directly to the left
                animator.SetBool("isAttackingRight", false);
                animator.SetBool("isAttackingLeft", true);
            }
        }
        else
        {
            // The player is directly above or below
            if (directionToPlayer.y > 0)
            {
                if (isDebugMode) Debug.Log("Player is directly above");
                // Handle logic when the player is directly above
            }
            else
            {
                if (isDebugMode) Debug.Log("Player is directly below");
                // Handle logic when the player is directly below
            }
        }
    }
}