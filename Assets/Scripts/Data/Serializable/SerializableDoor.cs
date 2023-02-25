using UnityEngine;
/// <summary>
/// The <see cref="SerializableDoor"/> class is a serializable version of the <see cref="DoorConnector"/> class used for data persistence.
/// </summary>
[System.Serializable]
public struct SerializableDoor
{
    public MapAlignment Alignment;
    public Vector3Int Position;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableDoor"/> class based on a <see cref="DoorConnector"/>.
    /// </summary>
    /// <param name="door">The <see cref="DoorConnector"/> being serialized and saved.</param>
    public SerializableDoor(DoorConnector door)
    {
        (Position, Alignment) = door.WallNode.WallSprite.GetPosition;
    }
}
