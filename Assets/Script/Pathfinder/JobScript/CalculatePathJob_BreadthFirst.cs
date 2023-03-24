using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

internal struct CalculatePathJob_BreadthFirst : IJobParallelFor
{
    public NativeArray<PathData> pathDataArray;

    [ReadOnly] public NativeArray<Vector3> movementAllowed;
    [ReadOnly] public Vector3 current;
    [ReadOnly] public Vector3 destination;
    [ReadOnly] public bool agentsAreObstacle;

    [ReadOnly] public NativeParallelHashSet<Vector3> wallCoordinates;
    [ReadOnly] public NativeParallelHashSet<Vector3> agentCoordinates;
    [ReadOnly] public NativeParallelHashMap<Vector3, Vector3> came_from;
    
    private Vector3 next;
    // The code actually running on the job
    public void Execute(int index)
    {
        //cannot pass data directly via pathDataArray[index] for some reason, this is not just for readability

        PathData data = pathDataArray[index];
        next = current + movementAllowed[index];
        data.next = next;
        
        if (!wallCoordinates.Contains(next)
            && ValidateAgentAsObstacle(next)
            && !came_from.ContainsKey(next))
        {
            data.NextIsValid = true;
            if (next == destination)
            {
                data.PathFound = true;
            }
        }

        pathDataArray[index] = data;
    }

    private bool ValidateAgentAsObstacle(Vector3 next)
    {
        if (agentsAreObstacle)
        {
            return !agentCoordinates.Contains(next);
        }
        else
        {
            return true;
        }
    }
}
