using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Cinemachine;

public enum Dir
{
  UP,
  RIGHT,
  DOWN,
  LEFT,
}

public class Room
{
  public string Id { private set; get; }
  public string RawId { private set; get; }
  private Dictionary<Dir, Room> adjacentRooms = new Dictionary<Dir, Room>();
  private HashSet<char> pathways = new HashSet<char>();
  private SceneGenerator sceneGenerator;
  public bool Visited = false;

  public Room(string rawId, SceneGenerator _sceneGenerator)
  {
    RawId = rawId;
    Id = rawId.Substring(0, 1);
    sceneGenerator = _sceneGenerator;

    string rest = rawId.Substring(1);
    foreach (char c in rest)
    {
      pathways.Add(c);
    }
  }

  public void SetAdjacent(Dir dir, Room room)
  {
    adjacentRooms[dir] = room;
  }

  public bool HasPathway(char pathway)
  {
    return pathways.Contains(pathway);
  }

  public Room GetAdjacent(Dir dir)
  {
    if (adjacentRooms.ContainsKey(dir))
    {
      return adjacentRooms[dir];
    }

    return null;
  }

  public bool HasMatchingPathway(Room other)
  {
    foreach (char pathway in pathways)
    {
      if (other.HasPathway(pathway))
      {
        return true;
      }
    }

    return false;
  }

  public override string ToString()
  {
    string up = GetAdjacent(Dir.UP) != null ? $"Up: {GetAdjacent(Dir.UP).Id}" : "Up: None";
    string right = GetAdjacent(Dir.RIGHT) != null ? $"Right: {GetAdjacent(Dir.RIGHT).Id}" : "Right: None";
    string down = GetAdjacent(Dir.DOWN) != null ? $"Down: {GetAdjacent(Dir.DOWN).Id}" : "Down: None";
    string left = GetAdjacent(Dir.LEFT) != null ? $"Left: {GetAdjacent(Dir.LEFT).Id}" : "Left: None";

    string pathwaysStr = "";
    foreach (char pathway in pathways)
    {
      pathwaysStr += pathway;
    }
    if (pathwaysStr.Length == 0)
    {
      pathwaysStr = "None";
    }

    return $"Id: {Id}\n\nAdjacent rooms:\n  {up}\n  {right}\n  {down}\n  {left}\n\nPathways: {pathwaysStr}";
  }
}

public class CompoundRoom
{
  public string Id { private set; get; }
  public List<Room> Rooms = new List<Room>();
  private SceneGenerator sceneGenerator;
  private Scene currScene;

  public CompoundRoom(string id, SceneGenerator _sceneGenerator)
  {
    Id = id;
    sceneGenerator = _sceneGenerator;
  }

  public void AddRoom(Room room)
  {
    if (room.Id != Id)
    {
      return;
    }

    Rooms.Add(room);
  }

  public void GenerateScene()
  {
    currScene = sceneGenerator.GenerateScene();
    PlaceOuterTilesForCompoundRoom();
    ExcavateInnerTilesForCompoundRoom();
    sceneGenerator.ResetTilemapCollider();
    sceneGenerator.SaveSceneWithName(currScene, $"{sceneGenerator.SceneNamePrefix}{Id}");
  }

  private void SetAllUnvisited()
  {
    foreach (Room room in Rooms)
    {
      room.Visited = false;
    }
  }

  private void PlaceOuterTilesForCompoundRoom()
  {
    SetAllUnvisited();
    // First room in array is always the top left (0,0) coord
    // This is due to how we traversed the raw array when we first
    // collected rooms
    Room topLeft = Rooms[0];
    Traverse(topLeft, new Vector2Int(0, 0), true, false);
    SetAllUnvisited();
  }

  private void ExcavateInnerTilesForCompoundRoom()
  {
    SetAllUnvisited();
    // First room in array is always the top left (0,0) coord
    // This is due to how we traversed the raw array when we first
    // collected rooms
    Room topLeft = Rooms[0];
    Traverse(topLeft, new Vector2Int(0, 0), false, true);
    SetAllUnvisited();
  }

  private void Traverse(Room room, Vector2Int coords, bool outer, bool addSceneTransitions)
  {
    room.Visited = true;

    if (outer)
    {
      sceneGenerator.PlaceOuterRoom(coords);
    }
    else
    {
      sceneGenerator.ExcavateInnerRoom(coords);
    }

    if (addSceneTransitions)
    {
      AddSceneTransitions(room, coords);
    }

    TraverseExcavation(room, coords, outer, addSceneTransitions);
  }

  private void AddSceneTransitions(Room room, Vector2Int coords)
  {
    Room up = room.GetAdjacent(Dir.UP);
    if (up != null && room.HasMatchingPathway(up))
    {
      sceneGenerator.AddSceneTransition(currScene, $"{sceneGenerator.SceneNamePrefix}{up.Id}", $"Transition to compound room {up.Id}", new Vector2(coords.x, coords.y + (sceneGenerator.RoomUnitHeight / 2) - 1));
    }

    Room right = room.GetAdjacent(Dir.RIGHT);
    if (right != null && room.HasMatchingPathway(right))
    {
      sceneGenerator.AddSceneTransition(currScene, $"{sceneGenerator.SceneNamePrefix}{right.Id}", $"Transition to compound room {right.Id}", new Vector2(coords.x + (sceneGenerator.RoomUnitWidth / 2) - 1, coords.y));
    }

    Room down = room.GetAdjacent(Dir.DOWN);
    if (down != null && room.HasMatchingPathway(down))
    {
      sceneGenerator.AddSceneTransition(currScene, $"{sceneGenerator.SceneNamePrefix}{down.Id}", $"Transition to compound room {down.Id}", new Vector2(coords.x, coords.y - (sceneGenerator.RoomUnitHeight / 2) + 1));
    }

    Room left = room.GetAdjacent(Dir.LEFT);
    if (left != null && room.HasMatchingPathway(left))
    {
      sceneGenerator.AddSceneTransition(currScene, $"{sceneGenerator.SceneNamePrefix}{left.Id}", $"Transition to compound room {left.Id}", new Vector2(coords.x - (sceneGenerator.RoomUnitWidth / 2) + 1, coords.y));
    }
  }

  private void TraverseExcavation(Room room, Vector2Int coords, bool outer, bool addSceneTransitions)
  {
    Room up = room.GetAdjacent(Dir.UP);
    if (up != null && !up.Visited && up.Id == room.Id)
    {
      Vector2Int newCoords = new Vector2Int(coords.x, coords.y + sceneGenerator.RoomUnitHeight);
      Traverse(up, newCoords, outer, addSceneTransitions);
    }

    Room right = room.GetAdjacent(Dir.RIGHT);
    if (right != null && !right.Visited && right.Id == room.Id)
    {
      Vector2Int newCoords = new Vector2Int(coords.x + sceneGenerator.RoomUnitWidth, coords.y);
      Traverse(right, newCoords, outer, addSceneTransitions);
    }

    Room down = room.GetAdjacent(Dir.DOWN);
    if (down != null && !down.Visited && down.Id == room.Id)
    {
      Vector2Int newCoords = new Vector2Int(coords.x, coords.y + -sceneGenerator.RoomUnitHeight);
      Traverse(down, newCoords, outer, addSceneTransitions);
    }

    Room left = room.GetAdjacent(Dir.LEFT);
    if (left != null && !left.Visited && left.Id == room.Id)
    {
      Vector2Int newCoords = new Vector2Int(coords.x + -sceneGenerator.RoomUnitWidth, coords.y);
      Traverse(left, newCoords, outer, addSceneTransitions);
    }
  }

  public override string ToString()
  {
    return $"Id: {Id}\nNum rooms: {Rooms.Count}";
  }
}

public class RoomGatherer
{
  private string[,] rawIds;
  private SceneGenerator sceneGenerator;
  public Dictionary<string, CompoundRoom> CompoundRooms = new Dictionary<string, CompoundRoom>();

  private void AddToOrCreate(Room room)
  {
    // If the compound room has already been created, add the room to the compound room
    if (CompoundRooms.ContainsKey(room.Id))
    {
      CompoundRooms[room.Id].AddRoom(room);
    }
    // If the compound room has yet to be created, create the compound room and add the room
    else
    {
      CompoundRoom compoundRoom = new CompoundRoom(room.Id, sceneGenerator);
      compoundRoom.AddRoom(room);
      CompoundRooms[room.Id] = compoundRoom;
    }
  }

  public void Gather()
  {
    int iDim = rawIds.GetLength(0);
    int jDim = rawIds.GetLength(1);

    // First, iterate through the 2D array of raw ids and create
    // a second 2D array of Room objects while keeping respective spots
    // in the arrays
    Room[,] rooms = new Room[iDim, jDim];

    for (int i = 0; i < iDim; i++)
    {
      for (int j = 0; j < jDim; j++)
      {
        string rawId = rawIds[i, j];
        Room room = rawId == "0" ? null : new Room(rawId, sceneGenerator);
        rooms[i, j] = room;
      }
    }

    // Go through again and assign room adjacency
    for (int i = 0; i < iDim; i++)
    {
      for (int j = 0; j < jDim; j++)
      {
        Room room = rooms[i, j];
        if (room != null)
        {
          Room up = rooms[i - 1, j];
          if (up != null) room.SetAdjacent(Dir.UP, up);

          Room right = rooms[i, j + 1];
          if (right != null) room.SetAdjacent(Dir.RIGHT, right);

          Room down = rooms[i + 1, j];
          if (down != null) room.SetAdjacent(Dir.DOWN, down);

          Room left = rooms[i, j - 1];
          if (left != null) room.SetAdjacent(Dir.LEFT, left);

          AddToOrCreate(room);
        }
      }
    }
  }

  public RoomGatherer(string[,] _rawIds, SceneGenerator _sceneGenerator)
  {
    rawIds = _rawIds;
    sceneGenerator = _sceneGenerator;
  }

  public override string ToString()
  {
    string resultStr = "";
    foreach (CompoundRoom compoundRoom in CompoundRooms.Values)
    {
      resultStr += $"{compoundRoom.ToString()}\n";
    }
    if (resultStr.Length == 0)
    {
      resultStr = "No compound rooms found!";
    }

    return resultStr;
  }
}

[CreateAssetMenu(menuName = "ScriptableObjects/SceneGenerator")]
public class SceneGenerator : ScriptableObject
{
  public int RoomUnitWidth = 20;
  public int RoomUnitHeight = 20;
  public string SceneNamePrefix = "Compound Room ";
  public TileBase BoundaryTile;
  public GameObject SceneTransitionPrefab;
  public Tilemap tileMap { private set; get; }

  private string[,] rooms = new string[10, 10] {
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "1",   "1ab", "2a",  "2",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "3",   "3b",  "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "4a",  "3a",  "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
      {"0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0",   "0"},
  };

  [ContextMenu("Generate from room mapping")]
  public void GenerateAllScenes()
  {
    RoomGatherer gatherer = new RoomGatherer(rooms, this);
    gatherer.Gather();

    foreach (CompoundRoom compoundRoom in gatherer.CompoundRooms.Values)
    {
      compoundRoom.GenerateScene();
    }
  }

  public Scene GenerateScene()
  {
    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
    MoveAllRootGameObjectsToScene(newScene);
    MoveGridToScene(newScene);
    return newScene;
  }

  // EXCEPT FOR THE GRID!!!!!!
  public void MoveAllRootGameObjectsToScene(Scene scene)
  {
    List<GameObject> gos = new List<GameObject>();
    SceneManager.GetSceneByName("Base").GetRootGameObjects(gos);

    CinemachineVirtualCamera vCam = null;
    GameObject playerClone = null;
    foreach (GameObject go in gos)
    {
      // Skip the grid.... we could move this too but the code is strctured weirdly for legacy reasons
      // so dont lol.  Also dont move the generator
      if (go.name == "Grid" || go.name == "Scene Generator") continue;

      GameObject clone = Instantiate(go, go.transform.position, Quaternion.identity);
      clone.name = go.name;
      EditorSceneManager.MoveGameObjectToScene(clone, scene);

      // Save these for later to reconstruct references....
      if (clone.CompareTag("Player"))
      {
        playerClone = clone;
      }

      if (clone.CompareTag("MainVirtualCamera"))
      {
        vCam = clone.GetComponent<CinemachineVirtualCamera>();
      }
    }

    // Reconstruct the virtual cam follow to the newly cloned player object if possible.
    // Otherwise the vcam follow reference is to the old player object which can not be saved
    // cross-scene.
    if (vCam != null && playerClone != null)
    {
      vCam.m_Follow = playerClone.transform;
    }
  }

  public void MoveGridToScene(Scene scene)
  {
    GameObject grid = GameObject.Find("Grid");
    GameObject gridClone = Instantiate(grid, new Vector3(0, 0, 0), Quaternion.identity);
    gridClone.name = "Grid (Generated)";
    tileMap = gridClone.transform.GetChild(0).GetComponent<Tilemap>();
    tileMap.ClearAllTiles();
    EditorSceneManager.MoveGameObjectToScene(gridClone, scene);
  }

  public void AddSceneTransition(Scene currScene, string toSceneName, string name, Vector2 location)
  {
    GameObject transitionObject = Instantiate(SceneTransitionPrefab);
    EditorSceneManager.MoveGameObjectToScene(transitionObject, currScene);
    transitionObject.transform.position = new Vector3(location.x, location.y, 0);
    transitionObject.name = name;
    SceneTransitionTrigger transitionScene = transitionObject.GetComponent<SceneTransitionTrigger>();
    transitionScene.ToSceneName = toSceneName;
  }

  public void SaveSceneWithName(Scene scene, string name)
  {
    scene.name = name;
    EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Generated/{name}.unity", true);
    EditorSceneManager.CloseScene(scene, true);
  }

  public void PlaceOuterRoom(Vector2Int centerCoords)
  {
    // Create a box of tiles that is DOUBLE the provided bounds
    for (int i = centerCoords.x - RoomUnitWidth; i < centerCoords.x + RoomUnitWidth; i++)
    {
      for (int j = centerCoords.y - RoomUnitHeight; j < centerCoords.y + RoomUnitHeight; j++)
      {
        tileMap.SetTile(new Vector3Int(i, j, 0), BoundaryTile);
      }
    }
  }

  public void ExcavateInnerRoom(Vector2Int centerCoords)
  {
    // Excavate out the actual room size
    for (int i = centerCoords.x - (RoomUnitWidth / 2); i < centerCoords.x + (RoomUnitWidth / 2); i++)
    {
      for (int j = centerCoords.y - (RoomUnitHeight / 2); j < centerCoords.y + (RoomUnitHeight / 2); j++)
      {
        tileMap.SetTile(new Vector3Int(i, j, 0), null);
      }
    }
  }

  public void ResetTilemapCollider()
  {
    GameObject go = tileMap.gameObject;
    TilemapCollider2D collider = go.GetComponent<TilemapCollider2D>();
    collider.ProcessTilemapChanges();
  }
}