using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

    Sprite _fullDoorMask;
    Sprite _baseDoorMask;
    

    void MakeDoorMask()
    {
        Texture2D fullTexture = new Texture2D(17, 9 + _height * 12, TextureFormat.ARGB32, false);
        fullTexture.filterMode = FilterMode.Point;
        fullTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < 17; i++)
        {
            for(int j = 0; j < 9 + _height * 12; j++)
            {
                fullTexture.SetPixel(i, j, Color.clear);
            }
        }
        Texture2D baseTexture = new Texture2D(17,21, TextureFormat.ARGB32, false);
        baseTexture.filterMode = FilterMode.Point;
        baseTexture.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < 17; i++)
        {
            for (int j = 0; j < 21; j++)
            {
                baseTexture.SetPixel(i, j, Color.clear);
            }
        }
        Vector2 pivot = SpriteRenderer.sprite.pivot;

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < 17; j++)
            {
                for (int k = 0; k < 21; k++)
                {
                    if (_spriteRenderer[i].sprite.texture.GetPixel(j,k).a > 0.5f)
                    {
                        fullTexture.SetPixel(j, k + i * 12, Color.black);
                    }
                }
            }
        }

        for (int j = 0; j < 17; j++)
        {
            for (int k = 0; k < 21; k++)
            {
                if (SpriteRenderer.sprite.texture.GetPixel(j, k).a > 0.5f)
                {
                    baseTexture.SetPixel(j, k, Color.black);
                }
            }
        }

        Vector3 maskPosition = _doorMask.transform.localPosition;
        int xOffsetMask = (int)(pivot.x + maskPosition.x * 6 - _doorMask.sprite.pivot.x);
        int yOffsetMask = (int)(pivot.y + maskPosition.y * 6 - _doorMask.sprite.pivot.y);

        Sprite doorFull = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, true];
        Sprite doorBase = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, false];

        int xOffsetDoor = (int)(pivot.x - doorFull.pivot.x);
        int yOffsetDoor = (int)(pivot.y - doorFull.pivot.y);



        for (int i = Mathf.Max(0, xOffsetMask); i < fullTexture.width; i++)
        {
            for(int j = Mathf.Max(0, yOffsetMask); j < fullTexture.height; j++)
            {
                if (i - xOffsetMask < _doorMask.sprite.texture.width && j - yOffsetMask < _doorMask.sprite.texture.height && _doorMask.sprite.texture.GetPixel(i - xOffsetMask, j - yOffsetMask).a > 0.5f)
                {
                    if (doorFull.texture.GetPixel(i - xOffsetDoor, j - yOffsetDoor).a < 0.5f)
                    {
                        fullTexture.SetPixel(i, j, Color.clear);
                    }
                    if(doorBase == null || doorBase.texture.GetPixel(i - xOffsetDoor, j - yOffsetDoor).a < 0.5f)
                    { 
                        if (j < baseTexture.height)
                        {
                            baseTexture.SetPixel(i, j, Color.clear);
                        }
                    }
                }
            }
        }

        fullTexture.Apply();
        baseTexture.Apply();
        Vector2 fullPivot = new Vector2(pivot.x / 17, pivot.y / fullTexture.height);
        Vector2 basePivot = new Vector2(pivot.x / 17, pivot.y / 21);

        _fullDoorMask = Sprite.Create(fullTexture, new Rect(0, 0, 17, fullTexture.height), fullPivot, 6);
        _baseDoorMask = Sprite.Create(baseTexture, new Rect(0, 0, 17, 21), basePivot, 6);

    }

    class DoorMask : MonoBehaviour
    {
        SpriteMask _mask;
        WallSprite _wallSprite;

        public void SetMask(WallSprite wallSprite)
        {
            _mask = gameObject.AddComponent<SpriteMask>();
            _wallSprite = wallSprite;

            OnGraphicsUpdated();

            transform.position = wallSprite.SpriteRenderer.transform.position;

            Graphics.UpdatedGraphics += OnGraphicsUpdated;
            Graphics.LevelChanged += OnGraphicsUpdated;
        }

        void OnGraphicsUpdated()
        {
            if (_wallSprite.IsFullWall)
            {
                _mask.sprite = _wallSprite._fullDoorMask;
            }
            else
            {
                _mask.sprite = _wallSprite._baseDoorMask;
            }

            _mask.enabled = _wallSprite.SpriteRenderer.enabled;
        }

        private void OnDestroy()
        {
            Graphics.UpdatedGraphics -= OnGraphicsUpdated;
            Graphics.LevelChanged -= OnGraphicsUpdated;
        }
    }

    public override SpriteMask[] GetSpriteMask(Transform parent)
    {

        if(IsDoor)
        {
            SpriteMask[] masks = new SpriteMask[1];
            DoorMask mask = new GameObject("Door Mask").AddComponent<DoorMask>();
            mask.transform.SetParent(parent);
            mask.SetMask(this);
            masks[0] = mask.GetComponent<SpriteMask>();
            
            return masks;
        }

        return base.GetSpriteMask(parent);
    }

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
        base(height, Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], Graphics.WallSprites[GetSpriteType(position, 0, alignment), wallMaterial], position, alignment, "Wall", new Vector3Int(alignment == MapAlignment.XEdge ? 1: 0, alignment == MapAlignment.YEdge ? 1 : 0, height), false)
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
