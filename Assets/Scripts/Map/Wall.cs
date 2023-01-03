using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using UnityEditor;

public class Wall : INode
{
    MapAlignment _alignment;
    WallSprite _wallSprite;
    public Wall(WallSprite wallSprite, Vector3Int worldPosition, MapAlignment alignment)
    {
        _wallSprite = wallSprite;
        WorldPosition = worldPosition;
        _alignment = alignment;
        Map.Instance.SetWall(alignment, WorldPosition, this);
    }

    public Wall(Vector3Int worldPosition, MapAlignment alignment)
    {
        _wallSprite = new WallSprite(worldPosition, alignment, 6, WallMaterial.Brick, this);
        WorldPosition = worldPosition;
        _alignment = alignment;
    }

    public bool Traversible
    {
        get => false;
        set { }
    }

    public WallSprite WallSprite => _wallSprite;
    public Vector3Int WorldPosition { get; private set; }
}

public class WallSprite : LinearSpriteObject
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

    SpriteRenderer _accent;
    SpriteMask _cornerMask;
    SpriteMask _doorMask;
    AccentMaterial _doorMaterial;
    SpriteRenderer _doorSprite;
    DoorSpriteType _doorSpriteType;
    int _height;
    bool _highlightDoor;
    bool _isFullWall;
    WallSprite _nextDoor;
    SortingGroup _sortingGroup;
    Wall _wall;
    WallMaterial _wallMaterial;
    /// <summary>
    /// Sets up a <see cref="WallSprite"/> that does not have a corresponding <see cref="Wall"/>
    /// </summary>
    /// <param name="position">The position of the world sprite in the <see cref="Map"/> coordinate system.</param>
    /// <param name="alignment">Vertical or horizontal alignment of the wall.</param>
    /// <param name="height">Height of the wall.</param>
    /// <param name="wallMaterial"><see cref="WallMaterial"/> of the wall.</param>
    public WallSprite(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial) :
        base(height, Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], position, alignment, "Wall", Vector3Int.zero, false)
    {
        _wallMaterial = wallMaterial;

        _height = height;


        Transform.position = Map.MapCoordinatesToSceneCoordinates(alignment, position);


        _sortingGroup = GameObject.AddComponent<SortingGroup>();
        _sortingGroup.sortingOrder = Graphics.GetSortOrder(position) + 1;
        SpriteRenderer.color = Graphics.Instance.HighlightColor;
        SpriteRenderer.sortingLayerName = "Wall";


        for (int i = 1; i < _height; i++)
        {

            _spriteRenderer[i] = Object.Instantiate(Graphics.Instance.SpriteObject, Transform).GetComponent<SpriteRenderer>();
            SpriteRenderer current = _spriteRenderer[i];

            current.transform.localPosition = Vector3Int.up * i * 2;
            current.name = "Wall";
            current.sortingOrder = i;
            SpriteRenderer.sortingLayerName = "Wall";
            current.sprite = Graphics.WallSprites[GetSpriteType(position, i, alignment), wallMaterial];
            current.color = Graphics.Instance.HighlightColor;
        }

        Graphics.UpdatingGraphics += SetWallMode;
    }

    public WallSprite(int x, int y, int z, MapAlignment alignment, int height, WallMaterial wallMaterial) : this(new Vector3Int(x, y, z), alignment, height, wallMaterial)
    {
    }

    public WallSprite(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial, Wall wall) : this(position, alignment, height, wallMaterial)
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
                _doorSprite = Object.Instantiate(Graphics.Instance.SpriteObject, Transform).GetComponent<SpriteRenderer>();
                _doorSprite.name = "Door";
                _doorSprite.maskInteraction = SpriteMaskInteraction.None;
            }

            return _doorSprite;
        }
    }

    int X => WorldPosition.x;

    int Y => WorldPosition.y;

    int Z => WorldPosition.z;

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
        new WallSprite(x, y, z, alignment, WallHeight, WallMaterial);
    }

    public static void PlaceDoorHighlight(WallSprite wall)
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
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(alignment, position);
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
            WallSprite prev = Map.Instance.GetWall(Alignment, X + end, Y, Z).WallSprite;
            for (int i = start; i <= end; i++)
            {
                WallSprite current = Map.Instance.GetWall(Alignment, X + i, Y, Z).WallSprite;
                current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorXLeft : i == end ? DoorSpriteType.DoorXRight : DoorSpriteType.DoorXMid, material, new Vector3(-2, -1) * i, highlight);
                prev = current;
            }
        }
        else
        {
            WallSprite prev = Map.Instance.GetWall(Alignment, X, Y + end, Z).WallSprite;
            for (int i = start; i <= end; i++)
            {
                WallSprite current = Map.Instance.GetWall(Alignment, X, Y + i, Z).WallSprite;
                current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorYRight : i == end ? DoorSpriteType.DoorYLeft : DoorSpriteType.DoorYMid, material, new Vector3(2, -1) * i, highlight);
                prev = current;
            }
        }
    }

    public override void Destroy()
    {
        Graphics.UpdatingGraphics -= SetWallMode;
        Graphics.ResetingSprite -= ResetSprite;
        Graphics.EnablingCollider -= EnableCollider;
        Graphics.LevelChanging -= SetLevel;
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

    public void EnableCollider(bool enabled)
    {
        if(GameManager.Instance.IsOnLevel(Z) == 0)
            Collider.enabled = enabled;
    }

    /// <summary>
    /// Sets the <see cref="WallSprite"/> to a given, temporary highlight color. If the <see cref="WallSprite"/> is a door, the door frame is highlighted instead.
    /// </summary>
    /// <param name="color">Color used to highlight the wall.</param>
    public override void Highlight(Color color)
    {
        if (IsDoor)
        {
            WallSprite nextDoor = this;
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
        WallSprite nextDoor = this;
        WallSprite current;
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
            _wall = new Wall(this, WorldPosition, Alignment);

        Graphics.EnablingCollider += EnableCollider;

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
            }
        }

        if (Collider != null)
            Collider.enabled = level == 0;

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

    void AddDoorSprite(WallSprite nextDoor, DoorSpriteType doorSpriteType, AccentMaterial material, Vector3 maskPosition, bool highlight)
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
        }
    }
}
