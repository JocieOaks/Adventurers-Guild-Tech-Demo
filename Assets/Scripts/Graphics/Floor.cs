using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : AreaSpriteObject
{
    public static int FloorSpriteIndex { private get; set; } = 0;

    public static void CreateFloor(Vector3Int position)
    {
        Floor floor = Map.Instance[position].Floor;
        floor.Sprite = Graphics.Instance.FloorSprites[FloorSpriteIndex];
        floor.SpriteRenderer.color = Graphics.Instance.HighlightColor;
        Graphics.ConfirmingObject += floor.Confirm;
        Graphics.CheckingAreaConstraints += floor.DestroyIfOutOfRange;
    }

    public static void PlaceHighlight(SpriteRenderer highlight,Vector3Int position)
    {
        Graphics.Instance.ResetSprite();
        if (CheckObject(position))
        {
            RoomNode node = Map.Instance[position];
            node.Floor.Highlight(Graphics.Instance.HighlightColor, FloorSpriteIndex);
        }
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.IsSupported(position, MapAlignment.Center);
    }


    int _spriteIndex;
    bool _enabled = false;

    public int SpriteIndex
    {
        get { return _spriteIndex; }
        set
        {
            _spriteIndex = value;
            Sprite = Graphics.Instance.FloorSprites[value];

            if (!_enabled)
            {
                _enabled = true;
                if (GameManager.Instance.IsOnLevel(WorldPosition.z) > 0)
                    SpriteRenderer.enabled = false;

                Graphics.LevelChanging += SetLevel;
            }
        }
    }

    public Floor(Vector3Int position) : base(1,null,position, "Floor", new Vector3Int(1, 1, 0), false)
    {
        if (position == Vector3Int.back)
            return;

        SpriteRenderer.sortingOrder = Graphics.GetSortOrder(position) - 4;
        SpriteRenderer.color = Color.white;

        Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
    }

    protected override void Confirm()
    {
        Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
        SpriteIndex = FloorSpriteIndex;
        SpriteRenderer.color = Color.white;
        Enabled = true;
    }

    public void Highlight(Color color, int spriteIndex)
    {
        SpriteRenderer.color = color;
        Sprite = Graphics.Instance.FloorSprites[spriteIndex];
        Graphics.ResetingSprite += ResetSprite;
    }

    protected override void ResetSprite()
    {

        SpriteRenderer.color = Color.white;
        if (!Enabled)
        {
            Sprite = null;
        }
        else
        {
            Sprite = Graphics.Instance.FloorSprites[SpriteIndex];
        }

        Graphics.ResetingSprite -= ResetSprite;
    }

    public override void Destroy()
    {
        if (WorldPosition.z > 0)
            Enabled = false;
        else
            SpriteIndex = 1;
    }

    protected override void DestroyIfOutOfRange(Vector3Int start, Vector3Int end)
    {
        int minX = start.x < end.x ? start.x : end.x;
        int maxX = start.x > end.x ? start.x : end.x;
        int minY = start.y < end.y ? start.y : end.y;
        int maxY = start.y > end.y ? start.y : end.y;

        if (WorldPosition.x < minX || WorldPosition.y < minY || WorldPosition.x > maxX || WorldPosition.y > maxY)
        {
            Graphics.ConfirmingObject -= Confirm;
            Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;

            ResetSprite();
        }
    }

    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;
            if (value)
            {
                Sprite = Graphics.Instance.FloorSprites[_spriteIndex];
                Graphics.LevelChanging += SetLevel;

            }
            else
            {
                Sprite = null;
                Graphics.LevelChanging -= SetLevel;
            }
        }
    }
}
