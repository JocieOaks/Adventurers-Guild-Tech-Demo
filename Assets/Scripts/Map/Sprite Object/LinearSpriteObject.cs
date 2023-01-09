using UnityEngine;
using Newtonsoft.Json;

public abstract class LinearSpriteObject : SpriteObject
{
    public LinearSpriteObject(int spriteCount, Sprite xSprite, Sprite ySprite, Vector3Int position, MapAlignment alignment, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, null, position, name, dimensions, blocking)
    {
        Alignment = alignment;
        Sprite = alignment == MapAlignment.XEdge ? xSprite : ySprite;
        Graphics.ConfirmingObject += Confirm;
        Graphics.CheckingLineConstraints += DestroyIfOutOfRange;
    }

    [JsonProperty]
    public MapAlignment Alignment { get; }
    protected virtual void Confirm()
    {
        Graphics.CheckingLineConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
    }

    void DestroyIfOutOfRange(int start, int end)
    {
        if (Alignment == MapAlignment.XEdge && (WorldPosition.x < start || WorldPosition.x > end) || Alignment == MapAlignment.YEdge && (WorldPosition.y < start || WorldPosition.y > end))
        {
            Graphics.ConfirmingObject -= Confirm;
            Graphics.CheckingLineConstraints -= DestroyIfOutOfRange;

            Destroy();
        }
    }
}
