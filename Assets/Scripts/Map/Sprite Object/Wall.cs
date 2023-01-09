using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Wall : LinearSpriteObject
{
    static readonly Vector2[] _colliderXBase = new Vector2[]
    {
            new Vector2(2f/3, 13.5f/6),
            new Vector2(-1.5f, 7f/6),
            new Vector2(-1.5f, -1),
            new Vector2(-2f/3, -8.5f/6),
            new Vector2(8f/6, -2.5f/6),
            new Vector2(8f/6, 11.5f/6)
    };

    static readonly Vector2[] _colliderXFull = new Vector2[]
    {
            new Vector2(2f/3, 13.5f/6 + 12 * 5f/6),
            new Vector2(-1.5f, 7f/6 + 12 * 5f/6),
            new Vector2(-1.5f, -1),
            new Vector2(-2f/3, -8.5f/6),
            new Vector2(8f/6, -2.5f/6),
            new Vector2(8f/6, 11.5f/6 + 12 * 5f/6)
    };

    static readonly Vector2[] _colliderYBase = new Vector2[]
    {
            new Vector2(-8f/6, 11.5f/6),
            new Vector2(-8f/6, -2.5f/6),
            new Vector2(2f/3, -8.5f/6),
            new Vector2(1.5f, -1),
            new Vector2(1.5f, 7f/6),
            new Vector2(-2f/3, 13.5f/6)
    };

    static readonly Vector2[] _colliderYFull = new Vector2[]
    {
            new Vector2(-8f/6, 11.5f/6 + 12 * 5f/6),
            new Vector2(-8f/6, -2.5f/6),
            new Vector2(2f/3, -8.5f/6),
            new Vector2(1.5f, -1),
            new Vector2(1.5f, 7f/6 + 12 * 5f/6),
            new Vector2(-2f/3, 13.5f/6 + 12 * 5f/6)
    };

    bool[,] _fullDoorMask;
    bool[,] _baseDoorMask;
    

    void MakeDoorMask()
    {
        _fullDoorMask = new bool[9 + _height * 12, 17];

        Vector2 pivot = SpriteRenderer.sprite.pivot;

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < 21; j++)
            {
                for (int k = 0; k < 17; k++)
                {
                    if (Alignment == MapAlignment.XEdge ? PixelsX[j, k] : PixelsY[j, k])
                    {
                        _fullDoorMask[j + i * 12, k] = true;
                    }
                }
            }
        }

        if (Alignment == MapAlignment.XEdge)
            _baseDoorMask = PixelsX.Clone() as bool[,];
        else
            _baseDoorMask = PixelsY.Clone() as bool[,];

        Vector3 maskPosition = _doorMask.transform.localPosition;
        int xOffsetMask = (int)(pivot.x + maskPosition.x * 6 - _doorMask.sprite.pivot.x);
        int yOffsetMask = (int)(pivot.y + maskPosition.y * 6 - _doorMask.sprite.pivot.y);

        Sprite doorFull = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, true];
        Sprite doorBase = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, false];

        int xOffsetDoor = (int)(pivot.x - doorFull.pivot.x);
        int yOffsetDoor = (int)(pivot.y - doorFull.pivot.y);



        for (int i = Mathf.Max(0, yOffsetMask); i < 9 + _height * 12; i++)
        {
            for(int j = Mathf.Max(0, xOffsetMask); j < 17; j++)
            {
                if (j - xOffsetMask < _doorMask.sprite.texture.width && i - yOffsetMask < _doorMask.sprite.texture.height && _doorMask.sprite.texture.GetPixel(j - xOffsetMask, i - yOffsetMask).a > 0.5f)
                {
                    if (doorFull.texture.GetPixel(j - xOffsetDoor, i - yOffsetDoor).a < 0.5f)
                    {
                        _fullDoorMask[i,j] = false;
                    }
                    if(doorBase == null || doorBase.texture.GetPixel(j - xOffsetDoor, i - yOffsetDoor).a < 0.5f)
                    { 
                        if (i < 21)
                        {
                            _baseDoorMask[i,j] = false;
                        }
                    }
                }
            }
        }
    }

    SpriteMask _cornerMask;
    SpriteMask _doorMask;
    AccentMaterial _doorMaterial;
    SpriteRenderer _doorSprite;
    DoorSpriteType _doorSpriteType;
    int _height;
    bool _highlightDoor;
    bool _isFullWall;
    Wall _nextDoor;
    SortingGroup _sortingGroup;
    WallNode _wall;
    WallMaterial _wallMaterial;
    /// <summary>
    /// Sets up a <see cref="Wall"/> that does not have a corresponding <see cref="WallNode"/>
    /// </summary>
    /// <param name="position">The position of the world sprite in the <see cref="Map"/> coordinate system.</param>
    /// <param name="alignment">Vertical or horizontal alignment of the wall.</param>
    /// <param name="height">Height of the wall.</param>
    /// <param name="wallMaterial"><see cref="WallMaterial"/> of the wall.</param>
    public Wall(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial) :
        base(height, Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], position, alignment, "Wall", new Vector3Int(alignment == MapAlignment.XEdge ? 1: 0, alignment == MapAlignment.YEdge ? 1 : 0, height), false)
    {
        _wallMaterial = wallMaterial;

        _height = height;


        Transform.position = Map.MapCoordinatesToSceneCoordinates(position, alignment);


        _sortingGroup = GameObject.AddComponent<SortingGroup>();
        _sortingGroup.sortingOrder = Graphics.GetSortOrder(position) + 1;
        SpriteRenderer.color = Graphics.Instance.HighlightColor;
        SpriteRenderer.sortingLayerName = "Wall";


        for (int i = 1; i < _height; i++)
        {
            SpriteRenderer current = _spriteRenderer[i];

            current.transform.localPosition = Vector3Int.up * i * 2;
            current.name = "Wall";
            current.sortingOrder = i;
            current.sortingLayerName = "Wall";
            current.sprite = Graphics.WallSprites[GetSpriteType(position, i, alignment), wallMaterial];
            current.color = Graphics.Instance.HighlightColor;
        }

        Graphics.UpdatingGraphics += SetWallMode;
    }

    public Wall(int x, int y, int z, MapAlignment alignment, int height, WallMaterial wallMaterial) : this(new Vector3Int(x, y, z), alignment, height, wallMaterial)
    {
    }

    public Wall(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial, WallNode wall) : this(position, alignment, height, wallMaterial)
    {
        _wall = wall;
        Confirm();
    }

    public static AccentMaterial DoorMaterial { get; set; } = AccentMaterial.Stone;
    public static int WallHeight { get; set; } = 6;

    public static WallMaterial WallMaterial { get; set; } = WallMaterial.Brick;
    public (Vector3Int, MapAlignment) GetPosition => (WorldPosition, Alignment);

    public bool IsDoor => _doorSprite != null && _doorSprite.enabled && !_highlightDoor;

    public bool IsFullWall
    {
        get => _isFullWall;
        private set
        {
            _isFullWall = value;
            if (_isFullWall)
            {
                for (int i = 1; i < _height; i++)
                    _spriteRenderer[i].enabled = true;

                if (_cornerMask != null)
                    _cornerMask.transform.localPosition = Vector3.up * 2 * (_height - 1);
            }
            else
            {
                for (int i = 1; i < _height; i++)
                    _spriteRenderer[i].enabled = false;

                if (_cornerMask != null)
                    _cornerMask.transform.localPosition = Vector3.zero;
            }

            if (IsDoor)
            {
                DoorSprite.sprite = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, _isFullWall];
            }
        }
    }

    SpriteRenderer DoorSprite
    {
        get
        {
            if (_doorSprite == null)
            {
                _doorSprite = Object.Instantiate(Graphics.Instance.SpritePrefab, Transform).GetComponent<SpriteRenderer>();
                _doorSprite.name = "Door";
                _doorSprite.maskInteraction = SpriteMaskInteraction.None;
            }

            return _doorSprite;
        }
    }

    int X => WorldPosition.x;

    int Y => WorldPosition.y;

    int Z => WorldPosition.z;

    static bool[,] _pixelsX;
    static bool[,] _pixelsY;

    static bool[,] PixelsX
    {
        get
        {
            if (_pixelsX == null)
                BuildPixelArray(Graphics.WallSprites[0, WallMaterial.Brick], ref _pixelsX);
            return _pixelsX;
        }
    }
    static bool[,] PixelsY
    {
        get
        {
            if (_pixelsY == null)
                BuildPixelArray(Graphics.WallSprites[6, WallMaterial.Brick], ref _pixelsY);
            return _pixelsY;
        }
    }

    public override Vector3 OffsetVector => 2 * Vector3.up;
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if(IsDoor)
            {
                if (_isFullWall)
                {
                    yield return _fullDoorMask;
                }
                else
                {
                    yield return _baseDoorMask;
                }
            }
            else if(Alignment == MapAlignment.XEdge)
            {
                for(int i = 0; i < (_isFullWall ? _height : 1); i++)
                    yield return PixelsX;
            }
            else
            {
                for (int i = 0; i < (_isFullWall ? _height : 1); i++)
                    yield return PixelsY;
            }
        }
    }

    public static bool CheckDoor(Vector3Int position, MapAlignment alignment)
    {
        return Map.Instance.CanPlaceDoor(position, alignment);
    }

    public static bool CheckObject(Vector3Int position, MapAlignment alignment)
    {
        return Map.Instance.CanPlaceWall(position, alignment);
    }

    public static void CreateDoor(Vector3Int position, MapAlignment alignment)
    {
        Map.Instance.GetWall(alignment, position).WallSprite.AddDoor(3, DoorMaterial);
    }

    public static void CreateWall(int x, int y, int z, MapAlignment alignment)
    {
        new Wall(x, y, z, alignment, WallHeight, WallMaterial);
    }

    public static void PlaceDoorHighlight(Wall wall)
    {
        Graphics.Instance.ResetSprite();
        (Vector3Int position, MapAlignment alignment) = wall.GetPosition;
        if (Map.Instance.CanPlaceDoor(position, alignment))
        {
            wall.AddDoor(3, DoorMaterial, true);
        }
    }

    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position, MapAlignment alignment)
    {
        if (CheckObject(position, alignment))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.WallSprites[alignment == MapAlignment.XEdge ? WallSpriteType.X11 : WallSpriteType.Y11, WallMaterial];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position, alignment);
            highlight.sortingOrder = Graphics.GetSortOrder(position) + 1;
        }
        else
            highlight.enabled = false;
    }
    /// <summary>
    /// Adds a door sprite overlaying the wall at the given door position.
    /// </summary>
    /// <param name="doorPosition">Position where the door originates.</param>
    /// <param name="highlight">Determines if the door is there permanently, or is just a temporary highlight.</param>
    /// <exception cref="System.ArgumentException">Throws exception if the door position does not overlap with the wall.</exception>
    public void AddDoor(int width, AccentMaterial material, bool highlight = false)
    {
        int start = -width / 2;
        int end = width + start - 1;

        if (Alignment == MapAlignment.XEdge)
        {
            Wall prev = Map.Instance.GetWall(Alignment, X + end, Y, Z).WallSprite;
            for (int i = start; i <= end; i++)
            {
                Wall current = Map.Instance.GetWall(Alignment, X + i, Y, Z).WallSprite;
                current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorXLeft : i == end ? DoorSpriteType.DoorXRight : DoorSpriteType.DoorXMid, material, new Vector3(-2, -1) * i, highlight);
                prev = current;
            }
        }
        else
        {
            Wall prev = Map.Instance.GetWall(Alignment, X, Y + end, Z).WallSprite;
            for (int i = start; i <= end; i++)
            {
                Wall current = Map.Instance.GetWall(Alignment, X, Y + i, Z).WallSprite;
                current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorYRight : i == end ? DoorSpriteType.DoorYLeft : DoorSpriteType.DoorYMid, material, new Vector3(2, -1) * i, highlight);
                prev = current;
            }
        }
    }

    public override void Destroy()
    {
        Graphics.UpdatingGraphics -= SetWallMode;
        Graphics.ResetingSprite -= ResetSprite;
        Graphics.LevelChanged -= SetLevel;
        if (Alignment == MapAlignment.XEdge)
        {
            Map.Instance.GetCorner(X + 1, Y, Z)?.UpdateCorner();
        }
        else
        {
            Map.Instance.GetCorner(X, Y + 1, Z)?.UpdateCorner();
        }

        Map.Instance.GetCorner(X, Y, Z)?.UpdateCorner();

        Object.Destroy(GameObject);

        Map.Instance.RemoveWall(WorldPosition, Alignment);
    }

    /// <summary>
    /// Sets the <see cref="Wall"/> to a given, temporary highlight color. If the <see cref="Wall"/> is a door, the door frame is highlighted instead.
    /// </summary>
    /// <param name="color">Color used to highlight the wall.</param>
    public override void Highlight(Color color)
    {
        if (IsDoor)
        {
            Wall nextDoor = this;
            do
            {
                nextDoor = nextDoor._nextDoor;
                nextDoor.HighlightDoor(color);
            } while (nextDoor != this);
        }
        else
        {
            base.Highlight(color);
        }
    }

    /// <summary>
    /// Enable or disable the mask for the corner intersection between two walls.
    /// </summary>
    /// <param name="mask">Determines whether to enable or disable the mask.</param>
    public void MaskCorner(bool mask)
    {
        if (_cornerMask == null)
        {
            if (!mask)
                return;

            if (Alignment == MapAlignment.XEdge)
                _cornerMask = Object.Instantiate(Graphics.Instance.CornerMaskX, Transform);
            else
                _cornerMask = Object.Instantiate(Graphics.Instance.CornerMaskY, Transform);

            if (IsFullWall)
                _cornerMask.transform.localPosition = Vector3.up * 2 * (_height - 1);
        }

        _cornerMask.enabled = mask;
        _sortingGroup.sortingOrder += mask ? 1 : -1;
    }

    public void RemoveDoor()
    {
        Wall nextDoor = this;
        Wall current;
        do
        {
            current = nextDoor;
            nextDoor = current._nextDoor;
            current.RemoveDoorSprite();
        } while (nextDoor != this);

        Map.Instance.RemoveDoor(WorldPosition, Alignment);
    }

    public void SetEdge()
    {
        IsFullWall = true;
    }
    protected override void Confirm()
    {
        if (_wall == null)
            _wall = new WallNode(this, WorldPosition, Alignment);

        for (int i = 0; i < _height; i++)
        {
            _spriteRenderer[i].color = Color.white;
        }

        if (Alignment == MapAlignment.XEdge)
        {
            Map.Instance.GetCorner(X + 1, Y, Z).SetCorner(WorldPosition + Vector3Int.right, _wallMaterial);
        }
        else
        {
            Map.Instance.GetCorner(X, Y + 1, Z).SetCorner(WorldPosition + Vector3Int.up, _wallMaterial);
        }

        Map.Instance.GetCorner(X, Y, Z).SetCorner(WorldPosition, _wallMaterial);

        base.Confirm();
    }

    protected override void ResetSprite()
    {
        for (int i = 0; i < _height; i++)
        {
            _spriteRenderer[i].sprite = Graphics.WallSprites[GetSpriteType(WorldPosition, i, Alignment), _wallMaterial];
            _spriteRenderer[i].color = Color.white;
        }

        if (_highlightDoor)
        {
            RemoveDoorSprite();
            _highlightDoor = false;
        }
        else if (IsDoor)
        {
            _doorSprite.color = Color.white;
        }

        Graphics.ResetingSprite -= ResetSprite;
    }

    protected override void SetLevel()
    {
        int level = GameManager.Instance.IsOnLevel(Z);
        if (level > 0)
            for (int i = 0; i < _height; i++)
            {
                _spriteRenderer[i].enabled = false;
                if (_doorSprite != null)
                    _doorSprite.enabled = false;
            }
        else
        {
            for (int i = 0; i < _height; i++)
                _spriteRenderer[i].enabled = true;

            if (_doorSprite != null)
                _doorSprite.enabled = true;

            if (level == 0)
            {
                SetWallMode();
            }
            else
            {
                IsFullWall = true;

                Collider.enabled = false;
                Collider.SetPath(0, _isFullWall ?
                    (Alignment == MapAlignment.XEdge ? _colliderXFull : _colliderYFull) :
                    (Alignment == MapAlignment.XEdge ? _colliderXBase : _colliderYBase));
                Collider.enabled = true;
            }
        }
    }

    static int GetSpriteType(Vector3Int _position, int height, MapAlignment alignment)
    {
        int mod = alignment == MapAlignment.XEdge ? _position.x % 3 : _position.y % 3;
        int shift = (height / 2) * (2 * (height % 2) - 1) % 3;
        int offset = (alignment == MapAlignment.YEdge ? 6 : 0) + 3 * (height % 2);
        if (shift < 0)
            shift += 3;

        return (mod + shift) % 3 + offset;
    }

    void AddDoorSprite(Wall nextDoor, DoorSpriteType doorSpriteType, AccentMaterial material, Vector3 maskPosition, bool highlight)
    {
        DoorSprite.enabled = true;

        _nextDoor = nextDoor;

        _doorSpriteType = doorSpriteType;
        _doorMaterial = material;
        _doorSprite.sprite = Graphics.DoorSprites[doorSpriteType, material, IsFullWall];

        _doorMask = Object.Instantiate(Alignment == MapAlignment.XEdge ? Graphics.Instance.DoorMaskX : Graphics.Instance.DoorMaskY, Transform);
        _doorMask.transform.localPosition = maskPosition;

        if (highlight)
        {
            DoorSprite.color = Graphics.Instance.HighlightColor;
            Graphics.ResetingSprite += ResetSprite;
            _highlightDoor = true;
        }
        else
        {
            DoorSprite.color = Color.white;
            _highlightDoor = false;
        }

        MakeDoorMask();
    }
    void HighlightDoor(Color color)
    {
        _doorSprite.color = color;
        Graphics.ResetingSprite += ResetSprite;
    }

    void RemoveDoorSprite()
    {
        Object.Destroy(_doorSprite);
        Object.Destroy(_doorMask.gameObject);
        _doorSprite = null;
        _doorMask = null;

        _nextDoor = null;
    }
    void SetWallMode()
    {
        if (GameManager.Instance.IsOnLevel(Z) == 0)
        {
            if (Graphics.Instance.Mode == WallMode.Open)
            {
                if (Map.Instance[X, Y, Z] == null || Map.Instance[X, Y, Z].Room is Layer)
                {
                    IsFullWall = true;
                }
                else
                {
                    IsFullWall = false;
                }
            }
            else
            {
                IsFullWall = Graphics.Instance.Mode == WallMode.Full;
            }

            Collider.enabled = false;
            Collider.SetPath(0, _isFullWall ?
                (Alignment == MapAlignment.XEdge ? _colliderXFull : _colliderYFull) :
                (Alignment == MapAlignment.XEdge ? _colliderXBase : _colliderYBase));
            Collider.enabled = true;
        }
    }
}
