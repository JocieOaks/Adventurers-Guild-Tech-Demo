using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// The <see cref="Corner"/> class is a unity component for the <see cref="Sprite"/>s at the corners of two or more <see cref="WallSprite"/>s.
/// </summary>
public class Corner : MonoBehaviour
{
    static readonly List<int> ignoreIndeces = new() { 1, 2, 4, 5, 8, 10 };
    readonly AccentMaterial material = AccentMaterial.Stone;
    Vector3Int _position;
    int _spriteIndex;
    SpriteRenderer _spriteRenderer;
    WallBlocker East => Map.Instance.GetWall(MapAlignment.XEdge, _position);
    WallBlocker North => Map.Instance.GetWall(MapAlignment.YEdge, _position);
    WallBlocker South => Map.Instance.GetWall(MapAlignment.YEdge, _position + Vector3Int.down);
    WallBlocker West => Map.Instance.GetWall(MapAlignment.XEdge, _position + Vector3Int.left);

    /// <summary>
    /// Creates a corner at the given position, if there should be one there.
    /// </summary>
    /// <param name="position">The <see cref="Map"/> position of the <see cref="Corner"/>.</param>
    /// <param name="corner">An out parameter that will be set to the newly created corner.</param>
    /// <returns>Returns true if a corner was successfully made.</returns>
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

        corner.transform.position = Utility.MapCoordinatesToSceneCoordinates(position, MapAlignment.Corner);
        corner._spriteRenderer.sortingOrder = Utility.GetSortOrder(position) + 3;

        corner.ConfigureCorner(index);
        return true;
    }

    /// <summary>
    /// Evaluates the sprite index for the <see cref="Corner"/> and then calls <see cref="ConfigureCorner"/>.
    /// </summary>
    public void ConfigureCorner()
    {
        ConfigureCorner(GetSpriteIndex(_position));
    }

    /// <summary>
    /// Determines the type of corner that should be set at the given position.
    /// </summary>
    /// <param name="position">The <see cref="Map"/> position of the <see cref="Corner"/>.</param>
    /// <returns>Returns the index of the sprite for the <see cref="Corner"/> at <c>position</c> or -1 if no corner is needed.</returns>
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

    /// <inheritdoc/>
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Graphics.UpdatedGraphics += SetCornerMode;
        Graphics.LevelChangedLate += OnLevelChange;
    }

    /// <summary>
    /// Sets up the corner, destroying the corner if no corner is needed, and masking the adjacent <see cref="WallSprite"/>s if neccessary.
    /// </summary>
    /// <param name="index"></param>
    void ConfigureCorner(int index)
    {
        _spriteIndex = index;

        if (_spriteIndex == -1)
        {
            Graphics.UpdatedGraphics -= SetCornerMode;
            Graphics.LevelChangedLate -= OnLevelChange;

            West?.WallSprite.MaskCorner(0);
            South?.WallSprite.MaskCorner(0);

            Destroy(gameObject);
            return;
        }

        if (_spriteIndex == 1 || _spriteIndex == 7 || _spriteIndex == 8)
        {
            West.WallSprite.MaskCorner(1);
            South.WallSprite.MaskCorner(-1);
        }
        else if (_spriteIndex == 2)
        {
            West.WallSprite.MaskCorner(-1);
            South.WallSprite.MaskCorner(1);
        }
        else
        {
            West?.WallSprite.MaskCorner(0);
            South?.WallSprite.MaskCorner(0);
        }
    }

    /// <summary>
    /// Sets whether the <see cref="Corner"/> should be a full sprite or just a base sprite.
    /// </summary>
    void SetCornerMode()
    {
        if (GameManager.Instance.IsOnLevel(_position.z) == 0)
        {
            if (Graphics.Instance.Mode == WallDisplayMode.Open)
            {
                bool? xPos = East?.WallSprite.IsFullWall;
                bool? yNeg = South?.WallSprite.IsFullWall;
                bool? xNeg = West?.WallSprite.IsFullWall;
                bool? yPos = North?.WallSprite.IsFullWall;
                bool fullCorner = (xPos ?? false) || (yNeg ?? false) || (xNeg ?? false) || (yPos ?? false);
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, material, fullCorner];

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
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, material, Graphics.Instance.Mode == WallDisplayMode.Full];
            }
        }
    }

    /// <summary>
    /// Called whenever the level changes, to determine how the <see cref="Corner"/> should be rendered.
    /// </summary>
    void OnLevelChange()
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
                _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, material, true];
            }
        }
    }
}