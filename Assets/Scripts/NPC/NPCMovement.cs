using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCPath))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class NPCMovement : MonoBehaviour
{
    public SceneName npcCurrentScene;
    [HideInInspector] public SceneName npcTargetScene;
    [HideInInspector] public Vector3Int npcCurrentGridPosition;
    [HideInInspector] public Vector3Int npcTargetGridPosition;
    [HideInInspector] public Vector3 npcTargetWorldPosition;
    public Direction npcFacingDirectionAtDestination;

    private SceneName npcPreviousMovementStepScene;
    private Vector3Int npcNextGridPosition;
    private Vector3 npcNextWorldPosition;

    [Header("NPC Movement")]
    public float npcNormalSpeed = 2f;

    [SerializeField] private float npcMinSpeed = 1f;
    [SerializeField] private float npcMaxSpeed = 3f;
    public bool npcIsMoving = false;
    private bool paused = false;

    [HideInInspector] public AnimationClip npcTargetAnimationClip;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private Grid grid;
    private Rigidbody2D rigidBody2D;
    [SerializeField] private BoxCollider2D boxCollider2D;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private NPCPath npcPath;
    private bool npcInitialised = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool npcActiveInScene = false;
    [SerializeField] private Component[] componentsToToggle; // Array of components to enable/disable
    private bool sceneLoaded = false;

    private Coroutine moveToGridPositionRoutine;

    public bool debugMode = false; // Set to true to show debug logs

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloaded;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloaded;
    }

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        npcPath = GetComponent<NPCPath>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            if (debugMode) Debug.LogError("Animator component not found in Awake.");
        }
        else
        {
            if (debugMode) Debug.Log("Animator component found in Awake.");
            if (animator.runtimeAnimatorController == null)
            {
                if (debugMode) Debug.LogError("Animator Controller is not assigned in Awake.");
            }
            else
            {
                if (debugMode) Debug.Log("Animator Controller is assigned in Awake.");
            }
        }

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        
        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetIdleAnimation();

        if (animator == null)
        {
            if (debugMode) Debug.LogError("Animator component not found in Start.");
        }
        else
        {
            if (debugMode) Debug.Log("Animator component found in Start.");
            if (animator.runtimeAnimatorController == null)
            {
                if (debugMode) Debug.LogError("Animator Controller is not assigned in Start.");
            }
            else
            {
                if (debugMode) Debug.Log("Animator Controller is assigned in Start.");
            }
        }
    }

    private void FixedUpdate()
    {
        if(debugMode) Debug.Log("FixedUpdate" + sceneLoaded + " " + paused);
        if (sceneLoaded && !paused)
        {
            if (!npcIsMoving)
            {
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;


                if(debugMode) Debug.Log("npcPath.npcMovementStepStack.Count: " + npcPath.npcMovementStepStack.Count);
                if (npcPath.npcMovementStepStack.Count > 0)
                {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();
                    if (debugMode) Debug.Log(npcMovementStep.sceneName + " " + npcMovementStep.hour + ":" + npcMovementStep.minute + ":" + npcMovementStep.second + " " + npcMovementStep.gridCoordinate);
                    npcCurrentScene = npcMovementStep.sceneName;

                    if (npcCurrentScene != npcPreviousMovementStepScene)
                    {
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);
                        npcPreviousMovementStepScene = npcCurrentScene;
                        npcPath.UpdateTimesOnPath();
                    }

                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
                    {
                        SetNPCActiveInScene();
                        npcMovementStep = npcPath.npcMovementStepStack.Pop();
                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        MoveToGridPosition(npcNextGridPosition);
                    }
                    else
                    {
                        SetNPCInactiveInScene();
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);

                        TimeSpan gameTime = TimeManager.Instance.GetGameTime();
                        npcMovementStep = npcPath.npcMovementStepStack.Peek();

                        if (new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second) < gameTime)
                        {
                            npcPath.npcMovementStepStack.Pop();
                            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                            npcNextGridPosition = npcCurrentGridPosition;
                            transform.position = GetWorldPosition(npcCurrentGridPosition);
                        }
                    }
                }
                else
                {
                    ResetMoveAnimation();
                    SetNPCFacingDirection();
                    SetNPCEventAnimation();
                }
            }
        }
    }

    public void SetScheduleEventDetails(NPCScheduleEvent npcScheduleEvent)
    {
        npcTargetScene = npcScheduleEvent.toSceneName;
        npcTargetGridPosition = (Vector3Int)npcScheduleEvent.toGridCoordinate;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
        npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination;
        npcTargetAnimationClip = npcScheduleEvent.animationAtDestination;
        ClearNPCEventAnimation();
    }

    private void SetNPCEventAnimation()
    {
        if (npcTargetAnimationClip != null)
        {
            ResetIdleAnimation();
            animatorOverrideController[blankAnimation] = npcTargetAnimationClip;
            animator.SetBool(Settings.eventAnimation, true);
        }
        else
        {
            animatorOverrideController[blankAnimation] = blankAnimation;
            animator.SetBool(Settings.eventAnimation, false);
        }
    }

    public void ClearNPCEventAnimation()
    {
        animatorOverrideController[blankAnimation] = blankAnimation;
        animator.SetBool(Settings.eventAnimation, false);
        transform.rotation = Quaternion.identity;
    }

    private void SetNPCFacingDirection()
    {
        ResetIdleAnimation();

        switch (npcFacingDirectionAtDestination)
        {
            case Direction.up:
                animator.SetBool(Settings.idleUp, true);
                break;
            case Direction.down:
                animator.SetBool(Settings.idleDown, true);
                break;
            case Direction.left:
                animator.SetBool(Settings.idleLeft, true);
                break;
            case Direction.right:
                animator.SetBool(Settings.idleRight, true);
                break;
            case Direction.none:
                break;
        }
    }

    public void SetNPCActiveInScene()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;

        // Enable all components in the array
        foreach (Component component in componentsToToggle)
        {
            if (component is Behaviour behaviour)
            {
                behaviour.enabled = true;
            }
            else if (component is Renderer renderer)
            {
                renderer.enabled = true;
            }
            else if (component is Collider2D collider)
            {
                collider.enabled = true;
            }
            else
            {
                component.gameObject.SetActive(true);
            }
        }

        npcActiveInScene = true;
    }

    public void SetNPCInactiveInScene()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;

        // Disable all components in the array
        foreach (Component component in componentsToToggle)
        {
            if (component is Behaviour behaviour)
            {
                behaviour.enabled = false;
            }
            else if (component is Renderer renderer)
            {
                renderer.enabled = false;
            }
            else if (component is Collider2D collider)
            {
                collider.enabled = false;
            }
            else
            {
                component.gameObject.SetActive(false);
            }
        }

        npcActiveInScene = false;
    }

    public void EnemyAfterSceneLoad()
    {
        AfterSceneLoad();
    }

    private void AfterSceneLoad()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        if (!npcInitialised)
        {
            InitialiseNPC();
            npcInitialised = true;
        }

        sceneLoaded = true;
    }

    private void BeforeSceneUnloaded()
    {
        sceneLoaded = false;
    }

    private Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        if (grid != null)
        {
            return grid.WorldToCell(worldPosition);
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    private Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        if (grid != null)
        {
            return grid.CellToWorld(gridPosition) + new Vector3(Settings.gridCellSize / 2f, Settings.gridCellSize / 2f, 0);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void MoveToGridPosition(Vector3Int gridPosition)
    {
        if (moveToGridPositionRoutine != null)
        {
            StopCoroutine(moveToGridPositionRoutine);
        }

        moveToGridPositionRoutine = StartCoroutine(MoveRoutine(gridPosition));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPosition)
    {
        npcIsMoving = true;
        SetMoveAnimation(gridPosition);
        npcNextWorldPosition = GetWorldPosition(gridPosition);
        float npcCalculatedSpeed = npcNormalSpeed;

        while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize)
        {
            Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position);
            Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);
            rigidBody2D.MovePosition(rigidBody2D.position + move);
            yield return waitForFixedUpdate;
        }

        rigidBody2D.position = npcNextWorldPosition;
        npcCurrentGridPosition = gridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }

    private void SetMoveAnimation(Vector3Int gridPosition)
    {
        ResetIdleAnimation();
        ResetMoveAnimation();
        Vector3 toWorldPosition = GetWorldPosition(gridPosition);
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y))
        {
            if (directionVector.x > 0)
            {
                animator.SetBool(Settings.walkRight, true);
            }
            else
            {
                animator.SetBool(Settings.walkLeft, true);
            }
        }
        else
        {
            if (directionVector.y > 0)
            {
                animator.SetBool(Settings.walkUp, true);
            }
            else
            {
                animator.SetBool(Settings.walkDown, true);
            }
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool(Settings.idleDown, true);
    }

    private void ResetMoveAnimation()
    {
        animator.SetBool(Settings.walkRight, false);
        animator.SetBool(Settings.walkLeft, false);
        animator.SetBool(Settings.walkUp, false);
        animator.SetBool(Settings.walkDown, false);
    }

    private void ResetIdleAnimation()
    {
        animator.SetBool(Settings.idleRight, false);
        animator.SetBool(Settings.idleLeft, false);
        animator.SetBool(Settings.idleUp, false);
        animator.SetBool(Settings.idleDown, false);
    }

    public void MoveToNextScheduleEvent(NPCScheduleEvent nextEvent)
    {
        SetScheduleEventDetails(nextEvent);
        MoveToGridPosition(npcTargetGridPosition);
    }

    public void CancelNPCMovement()
    {
        if (moveToGridPositionRoutine != null)
        {
            StopCoroutine(moveToGridPositionRoutine);
            moveToGridPositionRoutine = null;
            npcIsMoving = false;
        }
    }

    public void InitialiseNPC()
    {
        npcCurrentGridPosition = GetGridPosition(transform.position);
        npcNextGridPosition = npcCurrentGridPosition;
        SetIdleAnimation();
        SetNPCActiveInScene();
    }

    public void Pause()
    {
        paused = true;
        CancelNPCMovement();

        // Set NPC to idle down animation when paused
        ResetIdleAnimation();
        animator.SetBool(Settings.idleDown, true);
    }

    public void Unpause()
    {
        paused = false;
    }
}
