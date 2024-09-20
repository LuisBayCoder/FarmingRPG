using UnityEngine;

public class E_EnemyAI : MonoBehaviour
{
    [SerializeField] private NPCPath npcPath = null;
    [SerializeField] private bool moveEnemy = false;
    [SerializeField] private SceneName sceneName = SceneName.Scene1_Farm;
    [SerializeField] private Vector2Int finishPosition; // The target destination (player position)
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip chaseAnimationClip = null;
    [SerializeField] private AnimationClip attackAnimationClip = null; // Add attack animation clip
    [SerializeField] private float detectionRadius = 5f; // Radius within which the player will be detected
    [SerializeField] private float attackRange = 1.5f; // Range to trigger the attack
    private NPCMovement npcMovement;
    private Transform player;
    private Animator animator;

    private void Start()
    {
        npcMovement = npcPath.GetComponent<NPCMovement>();
        npcMovement.npcFacingDirectionAtDestination = Direction.down;
        npcMovement.npcTargetAnimationClip = idleDownAnimationClip;

        // Find player in the scene
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the Animator component
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // Trigger enemy to chase the player
            ChasePlayer();
        }

        // Check if enemy is within attack range
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }

        // If moveEnemy is true, trigger the pathfinding towards the player
        if (moveEnemy)
        {
            moveEnemy = false;

            // Get the player's grid position
            Vector2Int playerGridPosition = new Vector2Int(Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.y));
            finishPosition = playerGridPosition;

            // Create an NPC event to start the chase
            NPCScheduleEvent enemyChaseEvent = new NPCScheduleEvent(0, 0, 0, 0, Weather.none, Season.none, sceneName, new GridCoordinate(finishPosition.x, finishPosition.y), chaseAnimationClip);

            // Build the path towards the player using A*
            npcPath.BuildPath(enemyChaseEvent);
        }
    }

    // Call this to trigger enemy movement
    public void ChasePlayer()
    {
        moveEnemy = true;
    }

    // Call this to attack the player
    public void AttackPlayer()
    {
        npcMovement.CancelNPCMovement(); // Stop the enemy from moving
        animator.Play(attackAnimationClip.name); // Play the attack animation
    }

    // Optional: visualize the detection radius in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

