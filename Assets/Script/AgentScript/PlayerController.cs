using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : AgentController
{
    public static PlayerController instance;

    [SerializeField]
    private float timeToTranslate = 0.25f;

    [SerializeField]
    private float timeWaitToRecalculatePath = 0.25f;

    Stack<PathElement> pathStack = new Stack<PathElement>();
    PathElement currentPathElement;
    Tween MoveAgentTween;
    Vector3 Destination;

    // Start is called before the first frame update
    private bool refreshPath = false;

    void Awake()
    {
        instance = this;
        PathfinderManager.instance.AddAgentToRequestDictionary(this);
    }

    private void Start()
    {
        currentPathElement = new PathElement(null, transform.parent.position);
        if (PathfinderJob.Instance.agentsAreObstacle)
        {
            LevelManager.instance.OnAgentMove += OnAgentMove;
        }
    }

    private void OnDestroy()
    {
        if (PathfinderJob.Instance.agentsAreObstacle)
        {
            LevelManager.instance.OnAgentMove -= OnAgentMove;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {

            if (currentPathElement.indicator != null) 
            {
                currentPathElement.indicator.RemoveIndicator(this);
            }

            for (int i = pathStack.Count; i > 0; i--)
            {
                PathElement tempPathElement = pathStack.Pop();
                if (tempPathElement.indicator != null) tempPathElement.indicator.RemoveIndicator(this);
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
            {
                PathfinderManager.instance.RemoveDestinationIndicator(Destination);
                Destination = hit.transform.position;
                pathStack = PathfinderManager.instance.RequestPathToLocation(this, Destination);
            }

            MoveAgentTween.Kill();
            ExecutePath();
        }
    }

    private void ExecutePath()
    {
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
            MoveAgentTween = transform.parent.DOMove(currentPathElement.location, timeToTranslate).OnComplete(OnMoveComplete).SetEase(Ease.Linear);
        }
        else 
        {
            PathfinderManager.instance.RemoveDestinationIndicator(Destination);
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
