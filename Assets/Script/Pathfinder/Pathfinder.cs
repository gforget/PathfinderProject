using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    //All code based on this article : https://www.redblobgames.com/pathfinding/a-star/introduction.html

    public static Pathfinder Instance;

    [HideInInspector] public bool showPath = true;
    [HideInInspector] public bool agentsAreObstacle = true;
    [HideInInspector] public bool pathAreWeighted = true;
    [HideInInspector] public int pathWeightMax = 6;
    [HideInInspector] public int pathWeightDecay = 2;
    [HideInInspector] public GlobalEnum.PathfinderAlgorithm pickedAlgorithm;
    [HideInInspector] public GlobalEnum.DefaultMovementType defaultMovementType;
    [HideInInspector] public int W2Weight = 6;
    [HideInInspector] public int W3Weight = 12;
    [HideInInspector] public GameObject pathIndicatorPrefab;
    [HideInInspector] public GameObject destinationIndicatorPrefab;

    private List<GameObject> PathIndicatorsReference = new List<GameObject>();
    private Vector3[] MovementAllowed_4Direction = new Vector3[4]
{
        Vector3.forward,
        Vector3.forward*-1,
        Vector3.right,
        Vector3.right*-1
    };

    private Vector3[] MovementAllowed_8Direction = new Vector3[8]
    {
        Vector3.forward,
        Vector3.forward*-1,
        Vector3.right,
        Vector3.right*-1,
        new Vector3(1f, 0f, 1f),
        new Vector3(1f, 0f, -1f),
        new Vector3(-1f, 0f, -1f),
        new Vector3(-1f, 0f, 1f)
    };

    private Vector3[] defaultMovementAllowed;

    private Dictionary<Vector3, PathfinderIndicator> allPathIndicators = new Dictionary<Vector3, PathfinderIndicator>();
    int currentPathWeight;

    private Dictionary<Vector3, DestinationIndicator> allDestinationIndicator = new Dictionary<Vector3, DestinationIndicator>();

    private void Awake()
    {
        Instance = this;
    }

    public void DefaultMovementUpdate() 
    {
        switch (defaultMovementType)
        {
            case GlobalEnum.DefaultMovementType._4Directions:
                defaultMovementAllowed = MovementAllowed_4Direction;
                break;

            case GlobalEnum.DefaultMovementType._8Directions:
                defaultMovementAllowed = MovementAllowed_8Direction;
                break;
        }
    }

    public Stack<PathElement> RequestPathToLocation(AgentController agentController, Vector3 Location, Vector3[] movementAllowed = null)
    {
        if (!allDestinationIndicator.ContainsKey(Location))
        {
            CreateDestinationIndicator(Location, agentController);
        }

        if (allDestinationIndicator[Location].DestinationOwner == agentController) 
        { 
            switch (pickedAlgorithm)
            {
                case GlobalEnum.PathfinderAlgorithm.BreadthFirst:
                    return Breadth_First(agentController, Location, movementAllowed);

                case GlobalEnum.PathfinderAlgorithm.Dijkstra:
                    return Dijkstra(agentController, Location, movementAllowed);

                case GlobalEnum.PathfinderAlgorithm.AStar:
                    return AStar(agentController, Location, movementAllowed);

                default:
                    Debug.Log("Unknown algorithm");
                    return new Stack<PathElement>();
            }
        }
        else 
        {
            Debug.LogWarning("Destination already taken by another agent");
        }

        return new Stack<PathElement>();
    }

    private Stack<PathElement> Breadth_First(AgentController agentController, Vector3 Location, Vector3[] movementAllowed = null) 
    {
        if (movementAllowed == null)
        {
            movementAllowed = defaultMovementAllowed;
        }

        Vector3 destination = Location;

        //By rounding the coordinate, you find the closest tile position mathematicaly since every tile has a round distance of 1 to each other
        Vector3 initialPosition = new Vector3(
        Mathf.Round(agentController.transform.parent.position.x),
        Mathf.Round(agentController.transform.parent.position.y),
        Mathf.Round(agentController.transform.parent.position.z));

        Queue<Vector3> frontier = new Queue<Vector3>();
        frontier.Enqueue(initialPosition);

        Dictionary<Vector3, Vector3> came_from = new Dictionary<Vector3, Vector3>();
        came_from[initialPosition] = Vector3.zero;

        bool PathFound = false;
        int iteration = 0;

        while (frontier.Count > 0)
        {
            Vector3 current = frontier.Dequeue();
            for (int i = 0; i < movementAllowed.Length; i++)
            {
                iteration++;
                Vector3 next = current + movementAllowed[i];
                if (DetectObstacle(current, next, i) &&
                   !came_from.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    came_from[next] = current;

                    if (next == destination)
                    {
                        PathFound = true;
                        break;
                    }
                }
            }

            if (PathFound)
            {
                break;
            }
        }

        Stack<PathElement> pathStack = new Stack<PathElement>();
        if (PathFound)
        {
            PathfinderIndicator pathIndicator = null;
            Vector3 current = destination;

            while (current != initialPosition)
            {
                //Show path
                if (showPath)
                {
                    pathIndicator = CreatePathIndicator(current, agentController);
                }

                pathStack.Push(new PathElement(pathIndicator, current));
                current = came_from[current];
            }

            //Debug.Log("number of iteration before finding a path : " + iteration);
        }

        return pathStack;
    }

    private Stack<PathElement> Dijkstra(AgentController agentController, Vector3 Location, Vector3[] movementAllowed = null)
    {
        if (movementAllowed == null)
        {
            movementAllowed = defaultMovementAllowed;
        }

        Vector3 destination = Location;

        //By rounding the coordinate, you find the closest tile position mathematicaly since every tile has a round distance of 1 to each other
        Vector3 initialPosition = new Vector3(
        Mathf.Round(agentController.transform.parent.position.x),
        Mathf.Round(agentController.transform.parent.position.y),
        Mathf.Round(agentController.transform.parent.position.z));

        PriorityQueue<Vector3, int> frontier = new PriorityQueue<Vector3, int>();
        frontier.Enqueue(initialPosition, 0);

        Dictionary<Vector3, Vector3> came_from = new Dictionary<Vector3, Vector3>();
        came_from[initialPosition] = Vector3.zero;
        
        Dictionary<Vector3, int> cost_so_far = new Dictionary<Vector3, int>();
        cost_so_far[initialPosition] = 0;

        bool PathFound = false;
        int iteration = 0;

        while (!frontier.isEmpty())
        {
            Vector3 current = frontier.Dequeue();
            for (int i = 0; i < movementAllowed.Length; i++)
            {
                iteration++;
                Vector3 next = current + movementAllowed[i];
                int new_cost = cost_so_far[current] + LevelManager.instance.GetFloorCost(current) + AddPathWeight(current, agentController); ;

                if (
                    DetectObstacle(current, next, i) &&
                    (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
                    )
                {
                    cost_so_far[next] = new_cost;
                    frontier.Enqueue(next, new_cost);
                    came_from[next] = current;

                    if (next == destination)
                    {
                        PathFound = true;
                        break;
                    }
                }
            }

            if (PathFound)
            {
                break;
            }
        }

        Stack<PathElement> pathStack = new Stack<PathElement>();
        if (PathFound)
        {
            PathfinderIndicator pathIndicator = null;
            Vector3 current = destination;

            while (current != initialPosition)
            {
                //Show path
                if (showPath)
                {
                    pathIndicator = CreatePathIndicator(current, agentController);
                }

                pathStack.Push(new PathElement(pathIndicator, current));
                current = came_from[current];
            }

            //Debug.Log("number of iteration before finding a path : " + iteration);
        }

        return pathStack;
    }
    private Stack<PathElement> AStar(AgentController agentController, Vector3 Location, Vector3[] movementAllowed = null)
    {
        if (movementAllowed == null)
        {
            movementAllowed = defaultMovementAllowed;
        }

        Vector3 destination = Location;

        //By rounding the coordinate, you find the closest tile position mathematicaly since every tile has a round distance of 1 to each other
        Vector3 initialPosition = new Vector3(
            Mathf.Round(agentController.transform.parent.position.x), 
            Mathf.Round(agentController.transform.parent.position.y), 
            Mathf.Round(agentController.transform.parent.position.z));

        PriorityQueue<Vector3, int> frontier = new PriorityQueue<Vector3, int>();
        frontier.Enqueue(initialPosition, 0);

        Dictionary<Vector3, Vector3> came_from = new Dictionary<Vector3, Vector3>();
        came_from[initialPosition] = Vector3.zero;

        Dictionary<Vector3, int> cost_so_far = new Dictionary<Vector3, int>();
        cost_so_far[initialPosition] = 0;

        bool PathFound = false;
        int iteration = 0;
        
        while (!frontier.isEmpty())
        {
            Vector3 current = frontier.Dequeue();
            for (int i = 0; i < movementAllowed.Length; i++)
            {
                iteration++;
                Vector3 next = current + movementAllowed[i];
                int new_cost = cost_so_far[current] + LevelManager.instance.GetFloorCost(current) + AddPathWeight(current, agentController);

                if (
                    DetectObstacle(current, next, i)
                    &&
                    (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next]) 
                    )
                {
                    cost_so_far[next] = new_cost;

                    //Add the heuristic so the search move toward the goal instead of all direction
                    frontier.Enqueue(next, new_cost + Heuristic(destination, next));
                    came_from[next] = current;

                    if (next == destination)
                    {
                        PathFound = true;
                        break;
                    }
                }
            }

            if (PathFound) 
            {
                break;
            }

            currentPathWeight -= pathWeightDecay;
        }

        Stack<PathElement> pathStack = new Stack<PathElement>();
        if (PathFound)
        {
            PathfinderIndicator pathIndicator = null;
            Vector3 current = destination;

            while (current != initialPosition)
            {
                //Show path
                if (showPath)
                {
                   pathIndicator = CreatePathIndicator(current, agentController);
                }

                //Debug.Log("number of iteration before finding a path : " + iteration);
                pathStack.Push(new PathElement(pathIndicator, current));
                current = came_from[current];
            }
        }

        return pathStack;
    }

    private int Heuristic(Vector3 goal, Vector3 next) 
    {

            return (int)(Mathf.Abs(goal.x - next.x) + Mathf.Abs(goal.z - next.z));
    }

    private bool DetectObstacle(Vector3 current, Vector3 next, int directionIndex) 
    {
        bool canPass = false;
        canPass = !LevelManager.instance.wallCoordinates.Contains(next) && ValidateAgentAsObstacle(next);

        //if movement in diagonal, check if there is obstacle on the side of the destination
        if (PathfinderManager.instance.defaultMovementType == GlobalEnum.DefaultMovementType._8Directions && directionIndex > 3 && canPass)
        {
            Vector3 evalPos1 = current + new Vector3(defaultMovementAllowed[directionIndex].x, 0, 0);
            Vector3 evalPos2 = current + new Vector3(0, 0, defaultMovementAllowed[directionIndex].z);

            canPass = !LevelManager.instance.wallCoordinates.Contains(evalPos1) && ValidateAgentAsObstacle(evalPos1);
            if (canPass)
            {
                canPass = !LevelManager.instance.wallCoordinates.Contains(evalPos2) && ValidateAgentAsObstacle(evalPos2);
            }
        }

        return canPass;
    }

    private PathfinderIndicator CreatePathIndicator(Vector3 destination, AgentController agentController)
    {
        GameObject PathIndicatorObj = Instantiate(pathIndicatorPrefab, destination, Quaternion.identity);
        PathIndicatorsReference.Add(PathIndicatorObj);
        PathfinderIndicator indicatorRef = PathIndicatorObj.GetComponent<PathfinderIndicator>();
        indicatorRef.AddAgentController(agentController);
        if (!allPathIndicators.ContainsKey(destination)) allPathIndicators.Add(destination, indicatorRef);

        return PathIndicatorObj.GetComponent<PathfinderIndicator>();
    }

    private GameObject CreateDestinationIndicator(Vector3 destination, AgentController agentOwner) 
    {
        GameObject DestinationIndicatorObj = Instantiate(destinationIndicatorPrefab, destination, Quaternion.identity);
        DestinationIndicator indicatorRef = DestinationIndicatorObj.GetComponent<DestinationIndicator>();
        indicatorRef.DestinationOwner = agentOwner;

        allDestinationIndicator.Add(destination, indicatorRef);

        return DestinationIndicatorObj;
    }

    public void RemoveDestinationIndicator(Vector3 destination) 
    {
        if (allDestinationIndicator.ContainsKey(destination)) 
        {
            Destroy(allDestinationIndicator[destination].gameObject);
        }

        allDestinationIndicator.Remove(destination);
    }

    private bool ValidateAgentAsObstacle(Vector3 next) 
    {
        if (agentsAreObstacle)
        {
            return !LevelManager.instance.agentCoordinates.Contains(next);
        }
        else 
        {
            return true;
        }
    }

    private int AddPathWeight(Vector3 tileLocation, AgentController agentController) 
    {
        if (pathAreWeighted)
        {
            if (allPathIndicators.ContainsKey(tileLocation))
            {
                if (!allPathIndicators[tileLocation].attachedAgentsController.Contains(agentController) && currentPathWeight > 0)
                {
                    return currentPathWeight;
                }
            }
        }

        return 0;
    }

    public void DeletePathIndicatorReference(Vector3 tileLocation) 
    {
        allPathIndicators.Remove(tileLocation);
    }
}

public struct PathElement 
{
    public PathfinderIndicator indicator;
    public Vector3 location;

    public PathElement(PathfinderIndicator _indicator, Vector3 _location) 
    {
        indicator = _indicator;
        location = _location;
    }
}
