using System;
using System.Collections;
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
    private bool npcIsMoving = false;

    [HideInInspector] public AnimationClip npcTargetAnimationClip;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private Grid grid;
    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider2D;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private int lastMoveAnimationParameter;
    private NPCPath npcPath;
    private bool npcInitialised = false;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool npcActiveInScene = false;

    private bool sceneLoaded = false;

    private Coroutine moveToGridPositionRoutine;
    private bool npcStopped = false; // New flag to track if NPC is stopped

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

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        // Initialise target world position, target grid position & target scene to current
        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetIdleAnimation();
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            if (npcStopped)
            {
                // Set idle down animation if the NPC is stopped
                SetIdleDownAnimation();
                return;
            }

            if (npcIsMoving == false)
            {
                // set npc current and next grid position - to take into account the npc might be animating
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.npcMovementStepStack.Count > 0)
                {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();
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
                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
                    }
                    else
                    {
                        SetNPCInactiveInScene();
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);
                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                        TimeSpan gameTime = TimeManager.Instance.GetGameTime();
                        if (npcMovementStepTime < gameTime)
                        {
                            npcMovementStep = npcPath.npcMovementStepStack.Pop();
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
            default:
                break;
        }
    }

    public void SetNPCActiveInScene()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
        npcActiveInScene = true;
    }

    public void SetNPCInactiveInScene()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        npcActiveInScene = false;
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

    public Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        Vector3 worldPosition = grid.CellToWorld(gridPosition);
        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f, worldPosition.z);
    }

    public void CancelNPCMovement()
    {
        npcPath.ClearPath();
        npcNextGridPosition = Vector3Int.zero;
        npcNextWorldPosition = Vector3.zero;
        npcIsMoving = false;

        if (moveToGridPositionRoutine != null)
        {
            StopCoroutine(moveToGridPositionRoutine);
        }

        // Reset move animation
        ResetMoveAnimation();

        // Clear event animation
        ClearNPCEventAnimation();
        npcTargetAnimationClip = null;

        // Reset idle animation
        ResetIdleAnimation();

        // Set idle down animation
        SetIdleDownAnimation();

        // Set the NPC stopped flag to true
        npcStopped = true;
    }

    private void SetIdleDownAnimation()
    {
        ResetIdleAnimation();
        animator.SetBool(Settings.idleDown, true);
    }

    private void InitialiseNPC()
    {
        // Active in scene
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
        {
            SetNPCActiveInScene();
        }
        else
        {
            SetNPCInactiveInScene();
        }

        npcPreviousMovementStepScene = npcCurrentScene;

        // Get NPC Current Grid Position
        npcCurrentGridPosition = GetGridPosition(transform.position);

        // Set Next Grid Position and Target Grid Position to current Grid Position
        npcNextGridPosition = npcCurrentGridPosition;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        // Get NPC WorldPosition
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
    }

    private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        moveToGridPositionRoutine = StartCoroutine(MoveToGridPositionRoutine(gridPosition, npcMovementStepTime, gameTime));
    }

    private IEnumerator MoveToGridPositionRoutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        npcIsMoving = true;

        SetMoveAnimation(gridPosition);

        npcNextWorldPosition = GetWorldPosition(gridPosition);

        // If movement step time is in the future, otherwise skip and move NPC immediately to position
        if (npcMovementStepTime > gameTime)
        {
            //calculate time difference in seconds
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);

            // Calculate speed
            float npcCalculatedSpeed = Mathf.Max(npcMinSpeed, Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond);

            //// If speed is at least npc min speed and less than npc max speed  then process, otherwise skip and move NPC immediately to position
            if (npcCalculatedSpeed <= npcMaxSpeed)
            {
                while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize)
                {
                    Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position);
                    Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);

                    rigidBody2D.MovePosition(rigidBody2D.position + move);

                    yield return waitForFixedUpdate;
                }
            }
        }

        rigidBody2D.position = npcNextWorldPosition;
        npcCurrentGridPosition = gridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }

    private void SetMoveAnimation(Vector3Int gridPosition)
    {
        // Reset idle animation
        ResetIdleAnimation();

        // Reset move animation
        ResetMoveAnimation();

        // get world position
        Vector3 toWorldPosition = GetWorldPosition(gridPosition);

        // get vector
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y))
        {
            // Use left/right animation
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
            //Use up/down animation
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

    public void OnConversationEnd()
    {
        // Resume NPC movement after conversation ends
        ResumeNPCMovement();
    }

    public void ResumeNPCMovement()
    {
        // Clear the NPC stopped flag
        npcStopped = false;

        // Set idle animation to down to ensure proper state before resuming
        SetIdleDownAnimation();

        // Check if there are any movement steps remaining
        if (npcPath.npcMovementStepStack.Count > 0)
        {
            NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();
            npcCurrentScene = npcMovementStep.sceneName;

            if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
            {
                SetNPCActiveInScene();

                npcMovementStep = npcPath.npcMovementStepStack.Pop();

                npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;

                TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);

                MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
            }
            else
            {
                SetNPCInactiveInScene();

                npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                npcNextGridPosition = npcCurrentGridPosition;
                transform.position = GetWorldPosition(npcCurrentGridPosition);

                TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                TimeSpan gameTime = TimeManager.Instance.GetGameTime();

                if (npcMovementStepTime < gameTime)
                {
                    npcMovementStep = npcPath.npcMovementStepStack.Pop();

                    npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                    npcNextGridPosition = npcCurrentGridPosition;
                    transform.position = GetWorldPosition(npcCurrentGridPosition);
                }
            }
        }
    }
}
