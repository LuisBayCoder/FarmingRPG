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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        e_EnemyAI = GetComponent<E_EnemyAI>();
    }

    private void FixedUpdate()
    {
        if (e_EnemyAI.playerDetected == true) return;
        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = targetPosition;
    }
}
