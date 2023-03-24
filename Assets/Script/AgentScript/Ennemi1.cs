using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ennemi1 : AgentController
{
    [SerializeField]
    private float timeToTranslate = 0.5f;

    private float timer = 0f;

    Stack<PathElement> pathStack = new Stack<PathElement>();
    Vector3[] movementAllowed = new Vector3[4]
{
        Vector3.forward,
        Vector3.forward*-1,
        Vector3.right,
        Vector3.right*-1
    };

    IEnumerator Start()
    {
        while (Pathfinder.Instance == null || PlayerController.instance == null) 
        {
            yield return null;
        }

        pathStack = Pathfinder.Instance.RequestPathToLocation(this, PlayerController.instance.transform.parent.position, movementAllowed);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToTranslate) 
        {
            //go to the player, but if no path was found use a random destination
            if (pathStack.Count > 0)
            {
                transform.parent.position = pathStack.Pop().location;
            }
            else 
            {
                Vector3 randomDestination = transform.parent.position + movementAllowed[Random.Range(0, 4)];
                if (!LevelManager.instance.wallCoordinates.Contains(randomDestination))
                {
                    transform.parent.position = randomDestination;
                }
            }
            timer = 0;
        } 
    }
}
