using UnityEngine;
using Newtonsoft.Json;

public abstract class DirectionalSpriteObject : SpriteObject
{
    public DirectionalSpriteObject(int spriteCount, Sprite north, Sprite south, Sprite east, Sprite west, Direction direction, Vector3Int position, string name, Vector3Int ObjectDimension, bool blocking)
    : base(spriteCount, null, position, name, ObjectDimension, blocking)
    {
        Direction = direction;

        switch (direction)
        {
            case Direction.North:
                Sprite = north;
                break;
            case Direction.South:
                Sprite = south;
                break;
            case Direction.East:
                Sprite = east;
                break;
            case Direction.West:
                Sprite = west;
                break;
        }
    }

    [JsonProperty]
    public Direction Direction { get; }
}
