using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [SerializeField]
    GameObject cameraObject;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void AdjustCamera(int floorSizeX, int floorSizeZ)
    {
        //Adjust the camera position with the size of the grid. Only work for 1920X1080 and possibly other 16:9 resolution
        //check if the sizeX or the sizeZ is even or odd in order to adjust the position to the center
        float adjusterX = floorSizeX % 2 == 0 ? 0.5f : 0f;
        float adjusterZ = floorSizeZ % 2 == 0 ? 0.5f : 0f;

        //if the size in X or Z is above a certain threshold, the camera should go up in order to see more of the map
        float yCameraPosX = 12f;
        float yCameraPosZ = 12f;

        if (floorSizeX > 20)
        {
            yCameraPosX = 12f + (floorSizeX - 20) * 0.70f;
        }

        if (floorSizeZ > 10)
        {
            yCameraPosZ = 12f + (floorSizeZ - 10) * 0.95f;
        }

        //set the camera position
        cameraObject.transform.position = new Vector3
            (
                Mathf.Ceil((float)floorSizeX / 2) + adjusterX,
                Mathf.Max(yCameraPosX, yCameraPosZ),
                Mathf.Ceil((float)floorSizeZ / 2) + adjusterZ
            );
    }
}
