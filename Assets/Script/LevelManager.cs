using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Collections;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public delegate void OnAgentMoveEvent(AgentController source);
    public event OnAgentMoveEvent OnAgentMove;

    [SerializeField]
    int nbNPC = 0;

    [SerializeField]
    string levelName = "BigLevel";

    [Header("Prefabs")]
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    GameObject NPCPrefab;

    [SerializeField]
    GameObject floorTilesPrefab;

    [SerializeField]
    GameObject wallTilesPrefab;

    [SerializeField]
    GameObject floorTilesW2Prefab;
    
    [SerializeField]
    GameObject floorTilesW3Prefab;

    int floorSizeX = 10;
    int floorSizeZ = 10;
    GridPosition playerPosition;

    List<GridPosition> FloorW2Positions = new List<GridPosition>();
    List<GridPosition> FloorW3Positions = new List<GridPosition>();
    List<GridPosition> interiorWallCoordinate = new List<GridPosition>();

    [HideInInspector]
    public HashSet<Vector3> NPCTakenPosition = new HashSet<Vector3>();

    [HideInInspector]
    public HashSet<Vector3> agentCoordinates = new HashSet<Vector3>();

    [HideInInspector]
    public HashSet<Vector3> wallCoordinates = new HashSet<Vector3>();

    [HideInInspector]
    public HashSet<Vector3> floorW2Coordinates = new HashSet<Vector3>();

    [HideInInspector]
    public HashSet<Vector3> floorW3Coordinates = new HashSet<Vector3>();

    [HideInInspector]
    public List<GameObject> ennemiObjects = new List<GameObject>();

    [HideInInspector]
    public List<GameObject> tokenObjects = new List<GameObject>();

    [HideInInspector]
    public List<Vector3> allFloorPositions = new List<Vector3>();

    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {

    }

    IEnumerator Start()
    {
        while (GameManager.instance == null) 
        {
            yield return null;
        }

        //gather the position of each element from the Level.txt file
        #if UNITY_EDITOR
            string path = "Assets/StreamingAssets/"+levelName+".txt";
        #else
            string path = Application.dataPath + "/StreamingAssets/Level.txt";
        #endif

        StreamReader reader = new StreamReader(path);
        string levelString = reader.ReadToEnd();
        reader.Close();

        string[] levelStringSplit = levelString.Split('\n');

        floorSizeX = levelStringSplit[0].Length;
        floorSizeZ = levelStringSplit.Length;

        int k = 0;
        for (int i = (levelStringSplit.Length-1); i>=0; i--) 
        {
            for (int j=0; j<levelStringSplit[i].Length; j++) 
            {

                if (levelStringSplit[i][j] == 'W') 
                {
                    interiorWallCoordinate.Add(new GridPosition(j + 1, k +1));
                }

                if (levelStringSplit[i][j] == 'P')
                {
                    playerPosition = new GridPosition(j+1, k + 1);
                }

                if (levelStringSplit[i][j] == 'G')
                {
                    FloorW2Positions.Add(new GridPosition(j + 1, k + 1));
                    floorW2Coordinates.Add(new Vector3(j + 1, 0, k + 1));
                }

                if (levelStringSplit[i][j] == 'H')
                {
                    FloorW3Positions.Add(new GridPosition(j + 1, k + 1));
                    floorW3Coordinates.Add(new Vector3(j + 1, 0, k + 1));
                }

            }
            k++;
        }
        
        //Floor and exterior wall
        for (int i = 0; i < (floorSizeX + 1); i++)
        {
            for (int j = 0; j < (floorSizeZ + 2); j++)
            {
                Vector3 floorPosition = new Vector3(i, 0, j);

                if (floorW2Coordinates.Contains(floorPosition))
                {
                    Instantiate(floorTilesW2Prefab, floorPosition, Quaternion.identity);
                }
                else if (floorW3Coordinates.Contains(floorPosition))
                {
                    Instantiate(floorTilesW3Prefab, floorPosition, Quaternion.identity);
                }
                else 
                {
                    Instantiate(floorTilesPrefab, floorPosition, Quaternion.identity);
                }

                if (i == 0 || i == (floorSizeX) || j == 0 || j == (floorSizeZ+1))
                {
                    Instantiate(wallTilesPrefab, new Vector3(i, 0, j), Quaternion.identity);
                    AddWallCoordinates(new Vector3(i, 0, j));

                }
                else 
                {
                    allFloorPositions.Add(floorPosition);
                }
            }
        }

        //Wall
        for (int i = 0; i < interiorWallCoordinate.Count; i++)
        {
            Instantiate(wallTilesPrefab, new Vector3(interiorWallCoordinate[i].x, 0, interiorWallCoordinate[i].z), Quaternion.identity);
            AddWallCoordinates(new Vector3(interiorWallCoordinate[i].x, 0, interiorWallCoordinate[i].z));
            allFloorPositions.Remove(new Vector3(interiorWallCoordinate[i].x, 0, interiorWallCoordinate[i].z));
        }

        //Player
        AddAgentCoordinates(new Vector3(playerPosition.x, 0, playerPosition.z));
        
        NPCTakenPosition.Add(new Vector3(playerPosition.x, 0, playerPosition.z));
        Instantiate(playerPrefab, new Vector3(playerPosition.x, 0, playerPosition.z), Quaternion.identity);

        //NPC
        for (int i = 0; i< nbNPC; i++)
        {
            Vector3 CurrentNPCPosition = allFloorPositions[UnityEngine.Random.Range(0, allFloorPositions.Count)];
            while (NPCTakenPosition.Contains(CurrentNPCPosition)) 
            {
                CurrentNPCPosition = allFloorPositions[UnityEngine.Random.Range(0, allFloorPositions.Count)];
            }
            
            Instantiate(NPCPrefab, CurrentNPCPosition, Quaternion.identity);
            AddAgentCoordinates(CurrentNPCPosition);
        }

        //send signal to adjust camera
        GameManager.instance.AdjustCamera(floorSizeX, floorSizeZ);
    }

    public int GetFloorCost(Vector3 floorPosition) 
    {
        if (floorW2Coordinates.Contains(floorPosition))
        {
            return Pathfinder.Instance.W2Weight;
        }
        else if (floorW3Coordinates.Contains(floorPosition))
        {
            return Pathfinder.Instance.W3Weight;
        }
        else
        {
            return 1;
        }
    }

    public void CallOnAgentMoveEvent(AgentController source) 
    {
        if (OnAgentMove != null) OnAgentMove(source);
    }

    public void AddWallCoordinates(Vector3 coordinate) 
    {
      wallCoordinates.Add(coordinate);
    }

    public void RemoveWallCoordinates(Vector3 coordinate)
    {
      wallCoordinates.Remove(coordinate);
    }

    public void AddAgentCoordinates(Vector3 coordinate)
    {
        agentCoordinates.Add(coordinate);
    }

    public void RemoveAgentCoordinates(Vector3 coordinate)
    {
        agentCoordinates.Remove(coordinate);
    }
}

[Serializable]
public struct GridPosition
{
    public int x;
    public int z;

    public GridPosition(int _x, int _z) 
    {
        x = _x;
        z = _z;
    }
}

