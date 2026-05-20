using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(NPCMovement))]
public class NPCPath : MonoBehaviour
{
    public Stack<NPCMovementStep> npcMovementStepStack;

    private NPCMovement npcMovement;
    public bool debugMode = false; // Set to true to show debug logs

    [Header("Path Debug")]
    [SerializeField] private bool drawAStarPathGizmos = true;
    [SerializeField] private bool drawOnlyWhenSelected = false;
    [SerializeField] private Color pathLineColor = new Color(0.2f, 1f, 1f, 0.95f);
    [SerializeField] private Color pathNodeColor = new Color(1f, 0.9f, 0.2f, 0.95f);
    [SerializeField] private float pathNodeRadius = 0.08f;

    private Grid sceneGrid;

    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();

        //create a new stack of NPCMovementStep objects
        npcMovementStepStack = new Stack<NPCMovementStep>();
    }

    public void ClearPath()
    {
        npcMovementStepStack.Clear();
    }

    public void BuildPath(NPCScheduleEvent npcScheduleEvent)
    {
        // Check if there is no schedule event
        if (npcScheduleEvent == null)
        {
            Debug.LogWarning("No schedule event provided for NPC.");
            return;
        }

        ClearPath();

        // If schedule event is for the same scene as the current NPC scene
        if (npcScheduleEvent.toSceneName == npcMovement.npcCurrentScene)
        {
            Vector2Int npcCurrentGridPosition = (Vector2Int)npcMovement.npcCurrentGridPosition;

            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            // Build path and add movement steps to movement step stack
            //this is the line that calls the NPCManager's BuildPath method
            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, npcTargetGridPosition, npcMovementStepStack);
            if(debugMode) Debug.Log("NPCPath: BuildPath: npcMovementStepStack.Count: " + npcMovementStepStack.Count + " for " + npcScheduleEvent.toSceneName + " from " + npcMovement.npcCurrentScene);
        }
        // else if the schedule event is for a location in another scene
        else if (npcScheduleEvent.toSceneName != npcMovement.npcCurrentScene)
        {
            SceneRoute sceneRoute;

            // Get scene route matchingSchedule
            sceneRoute = NPCManager.Instance.GetSceneRoute(npcMovement.npcCurrentScene.ToString(), npcScheduleEvent.toSceneName.ToString());

            // Has a valid scene route been found?
            if (sceneRoute != null)
            {
                // Loop through scene paths in reverse order

                for (int i = sceneRoute.scenePathList.Count - 1; i >= 0; i--)
                {
                    int toGridX, toGridY, fromGridX, fromGridY;

                    ScenePath scenePath = sceneRoute.scenePathList[i];

                    // Check if this is the final destination
                    if (scenePath.toGridCell.x >= Settings.maxGridWidth || scenePath.toGridCell.y >= Settings.maxGridHeight)
                    {
                        // If so use final destination grid cell
                        toGridX = npcScheduleEvent.toGridCoordinate.x;
                        toGridY = npcScheduleEvent.toGridCoordinate.y;
                    }
                    else
                    {
                        // else use scene path to position
                        toGridX = scenePath.toGridCell.x;
                        toGridY = scenePath.toGridCell.y;
                    }

                    // Check if this is the starting position
                    if (scenePath.fromGridCell.x >= Settings.maxGridWidth || scenePath.fromGridCell.y >= Settings.maxGridHeight)
                    {
                        // if so use npc position
                        fromGridX = npcMovement.npcCurrentGridPosition.x;
                        fromGridY = npcMovement.npcCurrentGridPosition.y;
                    }
                    else
                    {
                        // else use scene path from position
                        fromGridX = scenePath.fromGridCell.x;
                        fromGridY = scenePath.fromGridCell.y;
                    }

                    Vector2Int fromGridPosition = new Vector2Int(fromGridX, fromGridY);
                    Vector2Int toGridPosition = new Vector2Int(toGridX, toGridY);

                    // Build path and add movement steps to movement step stack
                    NPCManager.Instance.BuildPath(scenePath.sceneName, fromGridPosition, toGridPosition, npcMovementStepStack);
                }
            }
        }

        // If stack count >1, update times and then pop off 1st item which is the starting position
        if (npcMovementStepStack.Count > 1)
        {
            UpdateTimesOnPath();
            npcMovementStepStack.Pop(); // discard starting step

            // Set schedule event details in NPC movement
            npcMovement.SetScheduleEventDetails(npcScheduleEvent);
        }
    }

    public void UpdateTimesOnPath()
    {
        // Get current game time
        TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();

        NPCMovementStep previousNPCMovementStep = null;

        foreach (NPCMovementStep npcMovementStep in npcMovementStepStack)
        {
            if (previousNPCMovementStep == null)
                previousNPCMovementStep = npcMovementStep;

            npcMovementStep.hour = currentGameTime.Hours;
            npcMovementStep.minute = currentGameTime.Minutes;
            npcMovementStep.second = currentGameTime.Seconds;

            TimeSpan movementTimeStep;

            // if diagonal
            if (MovementIsDiagonal(npcMovementStep, previousNPCMovementStep))
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }
            else
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }

            currentGameTime = currentGameTime.Add(movementTimeStep);

            previousNPCMovementStep = npcMovementStep;
        }
    }

    private bool MovementIsDiagonal(NPCMovementStep npcMovementStep, NPCMovementStep previousNPCMovementStep)
    {
        return npcMovementStep.gridCoordinate.x != previousNPCMovementStep.gridCoordinate.x && npcMovementStep.gridCoordinate.y != previousNPCMovementStep.gridCoordinate.y;
    }

    public void Pause()
    {
        npcMovement.Pause();
    }

    public void Unpause()
    {
        npcMovement.Unpause();
    }

    private void OnDrawGizmos()
    {
        if (!drawOnlyWhenSelected)
        {
            DrawAStarPathGizmos();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawAStarPathGizmos();
    }

    private void DrawAStarPathGizmos()
    {
        if (!drawAStarPathGizmos || npcMovementStepStack == null || npcMovementStepStack.Count == 0)
        {
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        List<NPCMovementStep> pathSteps = npcMovementStepStack.ToList();

        Vector3 previousPoint = transform.position;
        bool hasPreviousPoint = true;

        Gizmos.color = pathLineColor;

        for (int i = 0; i < pathSteps.Count; i++)
        {
            NPCMovementStep step = pathSteps[i];
            if (step == null)
            {
                continue;
            }

            if (!string.Equals(step.sceneName.ToString(), activeSceneName, StringComparison.Ordinal))
            {
                continue;
            }

            Vector3 stepWorldPosition = GridToWorld(step.gridCoordinate);

            if (hasPreviousPoint)
            {
                Gizmos.DrawLine(previousPoint, stepWorldPosition);
            }

            Gizmos.color = pathNodeColor;
            Gizmos.DrawSphere(stepWorldPosition, Mathf.Max(0.01f, pathNodeRadius));
            Gizmos.color = pathLineColor;

            previousPoint = stepWorldPosition;
            hasPreviousPoint = true;
        }
    }

    private Vector3 GridToWorld(Vector2Int gridCoordinate)
    {
        if (sceneGrid == null)
        {
            sceneGrid = FindObjectOfType<Grid>();
        }

        if (sceneGrid == null)
        {
            return new Vector3(gridCoordinate.x, gridCoordinate.y, transform.position.z);
        }

        Vector3 center = sceneGrid.GetCellCenterWorld(new Vector3Int(gridCoordinate.x, gridCoordinate.y, 0));
        center.z = transform.position.z;
        return center;
    }
}