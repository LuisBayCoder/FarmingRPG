using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    public float MoveSpeed // Expose it via a property
    {
        get { return moveSpeed; }
        set { moveSpeed = value; } // Optional setter
    }
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private E_EnemyAI e_EnemyAI;
    private NPCMovement npcMovement;
    private NPCPath npcPath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        e_EnemyAI = GetComponent<E_EnemyAI>();
        npcMovement = GetComponent<NPCMovement>();
        npcPath = GetComponent<NPCPath>();
    }

    private void FixedUpdate()
    {
        if (e_EnemyAI.playerDetected == true) return;

        // Let NPCMovement own Rigidbody movement while following A* paths.
        bool npcPathActive = npcPath != null && npcPath.npcMovementStepStack != null && npcPath.npcMovementStepStack.Count > 0;
        bool npcStepMoving = npcMovement != null && npcMovement.npcIsMoving;
        if (npcPathActive || npcStepMoving)
        {
            return;
        }

        if (moveDir.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = targetPosition;
    }
}
