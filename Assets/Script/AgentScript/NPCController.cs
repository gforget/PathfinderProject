using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NPCController : AgentController
{
    [SerializeField]
    private float timeToTranslate = 0.25f;

    [SerializeField]
    private float timeWaitToRecalculatePath = 0.25f;

    Stack<PathElement> pathStack = new Stack<PathElement>();
    PathElement currentPathElement;
    Vector3 Destination;
    
    private bool refreshPath = false;

    // Start is called before the first frame update
    private void Awake()
    {
        PathfinderManager.instance.AddAgentToRequestDictionary(this);
    }

    void Start()
    {
        currentPathElement = new PathElement(null, transform.parent.position);
        if (PathfinderManager.instance.agentsAreObstacle) 
        {
            LevelManager.instance.OnAgentMove += OnAgentMove;
        }

        pathStack = RequestRandomPath();
        ExecutePath();
    }

    private void OnDestroy()
    {
        if (PathfinderManager.instance.agentsAreObstacle)
        {
            LevelManager.instance.OnAgentMove -= OnAgentMove;
        }
    }

    private Stack<PathElement> RequestRandomPath() 
    {
        //try getting a path until you get one that work
        Stack<PathElement> pathStack = new Stack<PathElement>();
        while (pathStack.Count == 0)
        {
            PathfinderManager.instance.RemoveDestinationIndicator(Destination);
            Destination = LevelManager.instance.allFloorPositions[Random.Range(0, LevelManager.instance.allFloorPositions.Count)];
            pathStack = PathfinderManager.instance.RequestPathToLocation(this, Destination);
        }

        return pathStack;
    }

    private void ExecutePath()
    {
        if (currentPathElement.location == Destination)
        {
            refreshPath = false;
            pathStack = RequestRandomPath();
        }

        if (refreshPath) 
        {
            for (int i = pathStack.Count; i > 0; i--)
            {
                PathElement tempPathElement = pathStack.Pop();
                if (tempPathElement.indicator != null) tempPathElement.indicator.RemoveIndicator(this);
            }

            pathStack = PathfinderManager.instance.RequestPathToLocation(this, Destination);
            if (pathStack.Count == 0)
            {
                Invoke("ExecutePath", timeWaitToRecalculatePath);
            }
            else
            {
                CancelInvoke("ExecutePath");
                refreshPath = false;
            }
        }

        if (pathStack.Count > 0)
        {
            LevelManager.instance.RemoveAgentCoordinates(currentPathElement.location);
            currentPathElement = pathStack.Pop();
            LevelManager.instance.AddAgentCoordinates(currentPathElement.location);
            LevelManager.instance.CallOnAgentMoveEvent(this);
            transform.parent.DOMove(currentPathElement.location, timeToTranslate).OnComplete(OnMoveComplete).SetEase(Ease.Linear);
        }
    }

    private void OnMoveComplete()
    {
        if (currentPathElement.indicator != null) currentPathElement.indicator.RemoveIndicator(this);
        ExecutePath();
    }

    private void OnAgentMove(AgentController source) 
    {
        if (source != this) 
        {
            refreshPath = true;
        }
    }
}
