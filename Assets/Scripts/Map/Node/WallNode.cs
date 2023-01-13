using UnityEngine;
public class WallNode : INode
{
    MapAlignment _alignment;
    Wall _wallSprite;
    public WallNode(Wall wallSprite, Vector3Int worldPosition, MapAlignment alignment)
    {
        _wallSprite = wallSprite;
        WorldPosition = worldPosition;
        _alignment = alignment;
        Map.Instance.SetWall(alignment, WorldPosition, this);
    }

    public WallNode(Vector3Int worldPosition, MapAlignment alignment)
    {
        _wallSprite = new Wall(worldPosition, alignment, 6, WallMaterial.Brick, this);
        WorldPosition = worldPosition;
        _alignment = alignment;
    }

    public bool Traversible
    {
        get => false;
        set { }
    }

    public Wall WallSprite => _wallSprite;
    public Vector3Int WorldPosition { get; private set; }

    public Room Room => null;

    public INode Node => this;

    public bool HasNavigatedTo(RoomNode node)
    {
        return Map.Instance[WorldPosition] == node || Map.Instance[WorldPosition + (_alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left)] == node;
    }
}