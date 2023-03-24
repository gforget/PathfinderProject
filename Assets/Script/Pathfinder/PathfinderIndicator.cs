using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderIndicator : MonoBehaviour
{
    [HideInInspector]
    public HashSet<AgentController> attachedAgentsController = new HashSet<AgentController>();

    public void AddAgentController(AgentController agentController) 
    {
        attachedAgentsController.Add(agentController);
    }

    public void RemoveIndicator(AgentController agentController) 
    {
        attachedAgentsController.Remove(agentController);
        if (attachedAgentsController.Count == 0) 
        {
            PathfinderJob.Instance.DeletePathIndicatorReference(transform.position);
            Destroy(gameObject);
        }
    }
}
