# PathfinderProject 
Simple unity project to test grid base Pathfinding Algorithm using a simple level generator file.</br>
Unity version 20202.3.26f1

## Implemented Algorithm
1. Breadth First
2. Dijkstra
3. A* - Manhattan heuristic

## How to use the project
1. Open the MainScene file in the Scenes folder
2. Press play to see the basic level with a controllable agent (teal cube) and a bot agent (purple cube)
![Screenshot of how the project look when activated](/Screenshot/MainLevel.png)

## Level Editor
Simply create a .txt file where each letter determine the type of tiles should place in the level. For example, the following folder create the level seen above: </BR></BR>
![Screenshot of a level seen in visual studio](/Screenshot/levelExample.png)

Once the file is created, put it in the StreamingAssets folder. In the MainScene scene, use the level name in the LevelManager component 'Level Name' Field. </BR></BR>
![Screenshot of how the level manager component](/Screenshot/LevelManager.png)

<p>
The letter use in the files are as followed :</BR>
W = wall</BR>
P = Player Position</BR>
F = Floor</BR>
G = FloorW2*</BR>
H = FloorW3*</BR>
</p>
*: W mean 'Weight', meaning those floor position can be cross, but should be avoided as much as possible (this could represent a fire zone for example)

## Main components in 'Main Scene'
### Level Manager
![Screenshot of how the level manager component](/Screenshot/LevelManager.png)</BR>

<b>Nb NPC</b></BR>
The number of NPC to place in the level that will randomly select a valid position to go to </BR></BR>
<b>Level Name</b></BR>
The name of the level file you want to use

### PathfinderManager
![Screenshot of how the level manager component](/Screenshot/PathfinderManager.png)</BR>

<b>Use Job System</b></BR>
Activate the Jobified version of the algorithm. Still a work in progress, only work with A*</BR></BR>

<b>Show Path</b></BR>
Activate so the tiles become green to show where the path the agent is following</BR></BR>

<b>Agents Are Obstacle</b></BR>
Activate so that other agent will be considered obstacle to circumvent too</BR></BR>

<b>Path are Weighted</b></BR>
Activate so tiles who will be used by agent are weighted so other agent will try not walking in the path of other agent</BR></BR>

<b>Path Weight Max</b></BR>
The weight of the path used by agents if the 'Path are Weighted' option is activated. The higher the number, the more likely the other agent will try to avoid it</BR></BR>

<b>Path Weight Decay</b></BR>
The weight of the path decay the farther the tiles is from the agent. 0 mean no decay.</BR></BR>

<b>Picked Algorithm</b></BR>
The pathfinding algorithm used. Current algorithm are : Breadth First, Dijkstra and A*</BR></BR>

<b>Default Movement Type</b></BR>
Choose if the agents can only use lateral direction (4 direction), or all direction (8 directions)</BR></BR>

<b>W2 Weight and W3 Weight</b></BR>
The weight of W2 (yellow) and W3(red) floor tiles</BR></BR>
