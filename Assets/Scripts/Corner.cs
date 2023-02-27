using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Corner : MonoBehaviour
{
    static readonly List<int> ignoreIndeces = new() { 1, 2, 4, 5, 8, 10 };
    readonly WallMaterial _wallMaterial = WallMaterial.Brick;
    Vector3Int _position;
    int _spriteIndex;
    SpriteRenderer _spriteRenderer;
    WallBlocker East => Map.Instance.GetWall(MapAlignment.XEdge, _position);
    WallBlocker North => Map.Instance.GetWall(MapAlignment.YEdge, _position);
    WallBlocker South => Map.Instance.GetWall(MapAlignment.YEdge, _position + Vector3Int.down);
    WallBlocker West => Map.Instance.GetWall(MapAlignment.XEdge, _position + Vector3Int.left);

    public static bool TryMakeCorner(Vector3Int position, out Corner corner)
    {
        int index = GetSpriteIndex(position);
        if (index == -1)
        {
            corner = null;
            return false;
        }

        corner = Instantiate(Graphics.Instance.SpritePrefab).AddComponent<Corner>();
        corner.gameObject.name = "Corner";

        corner._position = position;

        corner.transform.position = Map.MapCoordinatesToSceneCoordinates(position, MapAlignment.Corner);
        corner._spriteRenderer.sortingOrder = Graphics.GetSortOrder(position) + 3;

        corner.ConfigureCorner(index);
        return true;
    }

    public void ConfigureCorner()
    {
        ConfigureCorner(GetSpriteIndex(_position));
    }

    static int GetSpriteIndex(Vector3Int position)
    {
        int index = 0;
        index += Map.Instance.GetWall(MapAlignment.XEdge, position) != null ? 1 : 0;
        index += Map.Instance.GetWall(MapAlignment.YEdge, position + Vector3Int.down) != null ? 2 : 0;
        index += Map.Instance.GetWall(MapAlignment.XEdge, position + Vector3Int.left) != null ? 4 : 0;
        index += Map.Instance.GetWall(MapAlignment.YEdge, position) != null ? 8 : 0;


        if (ignoreIndeces.Any(x => x == index))
        {
            return -1;
        }
        index -= index switch
        {
            int n when n >= 10 => 7,
            int n when n >= 8 => 6,
            int n when n >= 5 => 5,
            _ => 3,
        };
        if (index < 0)
        {
            return -1;
        }

        return index;


    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Graphics.UpdatedGraphics += SetCornerMode;
        Graphics.LevelChangedLate += SetLevel;
    }

    void ConfigureCorner(int index)
    {
        _spriteIndex = index;

        if (_spriteIndex == -1)
        {
            Graphics.UpdatedGraphics -= SetCornerMode;
            Graphics.LevelChangedLate -= SetLevel;

            West?.WallSprite.MaskCorner(false);
            South?.WallSprite.MaskCorner(false);

            Destroy(gameObject);
            return;
        }

        if (_spriteIndex == 1 || _spriteIndex == 7 || _spriteIndex == 8)
        {
            West.WallSprite.MaskCorner(true);
            South.WallSprite.MaskCorner(false);
        }
        else if (_spriteIndex == 2)
        {
            West.WallSprite.MaskCorner(false);
            South.WallSprite.MaskCorner(true);
        }
        else
        {
            West?.WallSprite.MaskCorner(false);
            South?.WallSprite.MaskCorner(false);
        }
    }

    void SetCornerMode()
    {
        if (GameManager.Instance.IsOnLevel(_position.z) == 0)
        {
            if (Graphics.Instance.Mode == WallMode.Open)
            {
                bool? xPos = East?.WallSprite.IsFullWall;
                bool? yNeg = South?.WallSprite.IsFullWall;
                bool? xNeg = West?.WallSprite.IsFullWall;
                bool? yPos = North?.WallSprite.IsFullWall;
                bool fullCorner = (xPos ?? false) || (yNeg ?? false) || (xNeg ?? false) || (yPos ?? false);
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _wallMaterial, fullCorner];

                if (fullCorner)
                {
                    if (!xPos ?? false)
                        East.WallSprite.SetEdge();
                    if (!yPos ?? false)
                        North.WallSprite.SetEdge();
                    if (!yNeg ?? false)
                        South.WallSprite.SetEdge();
                    if (!xNeg ?? false)
                        West.WallSprite.SetEdge();
                }
            }
            else
            {
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _wallMaterial, Graphics.Instance.Mode == WallMode.Full];
            }
        }
    }

    void SetLevel()
    {
        int level = GameManager.Instance.IsOnLevel(_position.z);
        if (level > 0)
            _spriteRenderer.enabled = false;
        else
        {
            _spriteRenderer.enabled = true;
            if (level == 0)
            {
                SetCornerMode();
            }
            else
            {
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _wallMaterial, true];
            }
        }
    }
}