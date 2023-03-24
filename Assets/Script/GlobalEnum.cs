using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalEnum
{
    public enum Layer 
    { 
      Ennemi = 6,
      Token = 7,
      Wall = 8,
      Player = 9,
      Portal = 10
    }

    public enum DefaultMovementType
    {
        _4Directions = 0,
        _8Directions = 1,
        _Custom = 2
    }

    public enum PathfinderAlgorithm
    {
        BreadthFirst = 0,
        Dijkstra = 1,
        AStar = 2
    }
}
