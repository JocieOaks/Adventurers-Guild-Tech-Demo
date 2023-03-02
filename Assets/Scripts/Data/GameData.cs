using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/* 
Based on the Save Load System by Trevor Mock
https://github.com/trevermock/save-load-system
*/

/// <summary>
/// The different possible Nodes that could be connected to a <see cref="RoomNode"/>.
/// </summary>
enum NodeType
{
    Null,
    RoomNode,
    Wall
}

/// <summary>
/// The <see cref="GameData"/> class contains all the serialized data for the game to be stored in a save file.
/// </summary>
[System.Serializable]
public class GameData
{
    public List<SerializableDoor> Doors;
    public int Layers;
    public int MapHeight;
    public int MapLength;
    public int MapWidth;
    [JsonIgnore] public List<string> Names;
    [JsonIgnore] public List<QuestData> Quests;
    public List<SpriteObject> SpriteObjects;
    public List<SerializableStair> Stairs;
    public SerializableNode[] Map;
}
