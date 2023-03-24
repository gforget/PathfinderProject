using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationIndicator : MonoBehaviour
{

    public AgentController DestinationOwner
    {
        set 
        {
            _DestinationOwner = value;
            if (_DestinationOwner is PlayerController)
            {
                meshRenderer.material = PlayerMaterial;
            }
            else
            {
                meshRenderer.material = NPCMaterial;
            }
        }

        get 
        {
            return _DestinationOwner;
        }
    }

    private AgentController _DestinationOwner;

    [SerializeField]
    private Material PlayerMaterial;

    [SerializeField]
    private Material NPCMaterial;

    private MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
    }
}
