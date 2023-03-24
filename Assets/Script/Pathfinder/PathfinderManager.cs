using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathfinderJob))]
[RequireComponent(typeof(Pathfinder))]
public class PathfinderManager : MonoBehaviour
{
    public static PathfinderManager instance;
    public bool UseJobSystem = false;

    [SerializeField] bool showPath = true;
    public  bool agentsAreObstacle = true;
    [SerializeField] bool pathAreWeighted = true;
    [SerializeField] int pathWeightMax = 24;
    [SerializeField] int pathWeightDecay = 0;
    [SerializeField] GlobalEnum.PathfinderAlgorithm pickedAlgorithm = 0;
    [SerializeField] GlobalEnum.DefaultMovementType defaultMovementType = 0;
    [SerializeField] int W2Weight = 6;
    [SerializeField] int W3Weight = 12;
    [SerializeField] GameObject pathIndicatorPrefab;
    [SerializeField] GameObject destinationIndicatorPrefab;

    private PathfinderJob pathfinderJob;
    private Pathfinder pathfinder;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        pathfinderJob = GetComponent<PathfinderJob>();
        pathfinderJob.showPath = showPath;
        pathfinderJob.agentsAreObstacle = agentsAreObstacle;
        pathfinderJob.pathAreWeighted = pathAreWeighted;
        pathfinderJob.pathWeightMax = pathWeightMax;
        pathfinderJob.pathWeightDecay = pathWeightDecay;
        pathfinderJob.pickedAlgorithm = pickedAlgorithm;
        pathfinderJob.defaultMovementType = defaultMovementType;
        pathfinderJob.W2Weight = W2Weight;
        pathfinderJob.W3Weight = W3Weight;
        pathfinderJob.pathIndicatorPrefab = pathIndicatorPrefab;
        pathfinderJob.destinationIndicatorPrefab = destinationIndicatorPrefab;
        pathfinderJob.DefaultMovementUpdate();

        pathfinder = GetComponent<Pathfinder>();
        pathfinder.showPath = showPath;
        pathfinder.agentsAreObstacle = agentsAreObstacle;
        pathfinder.pathAreWeighted = pathAreWeighted;
        pathfinder.pathWeightMax = pathWeightMax;
        pathfinder.pathWeightDecay = pathWeightDecay;
        pathfinder.pickedAlgorithm = pickedAlgorithm;
        pathfinder.defaultMovementType = defaultMovementType;
        pathfinder.W2Weight = W2Weight;
        pathfinder.W3Weight = W3Weight;
        pathfinder.pathIndicatorPrefab = pathIndicatorPrefab;
        pathfinder.destinationIndicatorPrefab = destinationIndicatorPrefab;
        pathfinder.DefaultMovementUpdate();
    }

    int nbRequestThisFrame = 0;
    Dictionary<AgentController, PathRequest> RequestDictionary = new Dictionary<AgentController, PathRequest>();
    public void AddAgentToRequestDictionary(AgentController agent) 
    {
        RequestDictionary.Add(agent, new PathRequest());
        //Debug.Log(RequestDictionary.Count);
    }

    private void Update()
    {
        if (nbRequestThisFrame != 0) 
        {
            Debug.Log("execute request : " + nbRequestThisFrame);
        }

        nbRequestThisFrame = 0;
    }

    public Stack<PathElement> RequestPathToLocation(AgentController agentController, Vector3 Location, Vector3[] movementAllowed = null)
    {
        nbRequestThisFrame++;
        if (UseJobSystem)
        {
            return pathfinderJob.RequestPathToLocation(agentController, Location, movementAllowed);
        }
        else 
        {
            return pathfinder.RequestPathToLocation(agentController, Location, movementAllowed);
        }
    }

    public void RemoveDestinationIndicator(Vector3 destination) 
    {
        if (UseJobSystem)
        {
            pathfinderJob.RemoveDestinationIndicator(destination);
        }
        else
        {
            pathfinder.RemoveDestinationIndicator(destination);
        }
    }
}

public struct PathRequest 
{
    public Vector3 location;
    public Vector3[] movementAllowed;

    PathRequest(Vector3 _location, Vector3[] _movementAllowed) 
    {
        location = _location;
        movementAllowed = _movementAllowed;
    }
}
