using UnityEngine;

public abstract class AreaSpriteObject : SpriteObject
{

    public AreaSpriteObject(int spriteCount, Sprite sprite, Vector3Int position, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, sprite, position, name, dimensions, blocking)
    {
        SpriteRenderer.color = Graphics.Instance.HighlightColor;
        Graphics.ConfirmingObject += Confirm;
        Graphics.CheckingAreaConstraints += DestroyIfOutOfRange;
    }

    protected virtual void Confirm()
    {
        SpriteRenderer.color = Color.white;
        Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
    }

    protected virtual void DestroyIfOutOfRange(Vector3Int start, Vector3Int end)
    {
        int minX = start.x < end.x ? start.x : end.x;
        int maxX = start.x > end.x ? start.x : end.x;
        int minY = start.y < end.y ? start.y : end.y;
        int maxY = start.y > end.y ? start.y : end.y;

        if (WorldPosition.x < minX || WorldPosition.y < minY || WorldPosition.x > maxX || WorldPosition.y > maxY)
        {
            Graphics.ConfirmingObject -= Confirm;
            Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;

            Destroy();
        }
    }
}
