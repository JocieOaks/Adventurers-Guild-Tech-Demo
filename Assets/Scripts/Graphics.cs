using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Rendering;

public enum AccentMaterial
{
    Stone = 0
}

public enum AccentSpriteType
{
    BaseBoardX,
    BaseBoardY,
}

public enum BuildMode
{
    None,
    Point,
    Line,
    Area,
    Door,
    Demolish
}

public enum DoorSpriteType
{
    DoorXLeft = 0,
    DoorXMid = 1,
    DoorXRight = 2,
    DoorYRight = 3,
    DoorYMid = 4,
    DoorYLeft = 5
}

public enum WallMaterial
{
    Brick = 0
}

public enum WallMode
{
    Full = 0,
    Base = 1,
    Open = 2
}

public enum WallSpriteType
{
    None = -1,
    X11 = 0,
    X12 = 1,
    X13 = 2,
    X21 = 3,
    X22 = 4,
    X23 = 5,
    Y11 = 6,
    Y12 = 7,
    Y13 = 8,
    Y21 = 9,
    Y22 = 10,
    Y23 = 11
}
public static class BuildFunctions
{
    public delegate bool CheckLineDelegate(Vector3Int position, MapAlignment alignment);

    public delegate bool CheckPointDelegate(Vector3Int position);
    public static BuildMode BuildMode { get; set; }
    public static CheckLineDelegate CheckLine { get; set; }
    public static CheckPointDelegate CheckPoint { get; set; }
    public static Action<int, int, int, MapAlignment> CreateLine { get; set; }
    public static Action<Vector3Int> CreatePoint { get; set; }
    public static Direction Direction { get; set; }
    public static Action<SpriteRenderer, Vector3Int, MapAlignment> HighlightLine { get; set; }
    public static Action<SpriteRenderer, Vector3Int> HighlightPoint { get; set; }
}

public class Graphics : MonoBehaviour
{
    public SpriteRenderer _highlight;

    public Sprite[] BarX;

    public Sprite[] BarY;

    public Sprite[] BedSprite;

    public Sprite ChairEast;

    public Sprite ChairNorth;

    public Sprite ChairSouth;

    public Sprite ChairWest;

    public Sprite[] Commentary;

    public SpriteMask CornerMaskX;

    public SpriteMask CornerMaskY;

    public Sprite Cube;

    public SpriteMask DoorMaskX;

    public SpriteMask DoorMaskY;

    public Sprite[] FloorSprites;

    public Pawn PawnPrefab;

    public Texture2D PawnGradientGreyscale;

    public Texture2D PawnGradientHair;

    public Texture2D PawnGradientHorns;

    public Texture2D PawnGradientSkin;

    public Texture2D PawnTextureBeard;

    public Texture2D PawnTextureBodyHairMuscular;

    public Texture2D PawnTextureBodyHairThick;

    public Texture2D PawnTextureBodyMuscular;

    public Texture2D PawnTextureBodyThick;

    public Texture2D PawnTextureChestHairMuscular;

    public Texture2D PawnTextureChestHairThick;

    public Texture2D PawnTextureEars;

    public Texture2D PawnTextureHair;

    public Texture2D PawnTextureHairFront;

    public Texture2D PawnTextureHead;

    public Texture2D PawnTextureHornsBack;

    public Texture2D PawnTextureHornsFront;

    public Texture2D PawnTextureOrcTeeth;

    public SpriteRenderer SpeechBubble;

    public GameObject SpritePrefab;

    public Sprite StairsNorth;

    public Sprite StairsSouth;

    public Sprite StairsEast;

    public Sprite StairsWest;

    public Sprite Stool;

    public Sprite[] TableRound;

    public Sprite[] TableSquare;

    public Sprite[] UnsortedCornerSprites;

    public Sprite[] UnsortedDoorSprites;

    public Sprite[] UnsortedWallSprites;

    public Sprite Wave;

    public SortingGroup SortingObject;

    static Graphics _instance;

    static Color[] _pawnGradientGreyscale;

    static (int xOffset, int yOffset, int headDirection, bool flipped)[] headTable = new (int, int, int, bool)[] {
            (24, 48, 0, false), (24, 49, 0, false), (24, 48, 0, false), (24, 49, 0, false),
            (24, 47, 2, true), (24, 48, 2, true), (24, 47, 2, true), (24, 48, 2, true),
            (24, 47, 2, false), (24, 48, 2, false), (24, 47, 2, false), (24, 48, 2, false),
            (24, 48, 3, false), (24, 49, 3, false), (25, 48, 3, false), (25, 49, 3, false),
            (23, 47, 4, true), (23, 48, 4, true), (23, 47, 4, true), (23, 48, 4, true),
            (25, 47, 4, false), (25, 48, 4, false), (25, 47, 4, false), (25, 48, 4, false),
            (25, 47, 4, false), (25, 47, 4, false), (25, 47, 4, false), (25, 47, 4, false),
            (25, 47, 4, false), (25, 47, 4, false), (23, 47, 4, true), (23, 47, 4, true),
            (23, 47, 4, true), (23, 47, 4, true), (23, 47, 4, true), (23, 47, 4, true),
            (26, 48, 1, true), (26, 49, 1, true), (26, 48, 1, true), (26, 49, 1, true),
            (22, 48, 1, false), (22, 49, 1, false), (22, 48, 1, false), (22, 49, 1, false),
            (22, 47, 1, false), (26, 47, 1, true), (26, 39, 1, false), (22, 39, 1, true) };


    int _lineEnd;

    int _lineStart;

    Map _map;

    WallMode _mode = WallMode.Open;

    Vector3Int areaEnd;

    Vector3Int areaStart;

    static Color[] _pawnGradientHair;

    static Color[] _pawnGradientHorns;

    static Color[] _pawnGradientSkin;

    static Color[] _pawnTextureBeard;

    static Color[] _pawnTextureBodyHairMuscular;

    static Color[] _pawnTextureBodyHairThick;

    static Color[] _pawnTextureBodyMuscular;

    static Color[] _pawnTextureBodyThick;

    static Color[] _pawnTextureChestHairMuscular;

    static Color[] _pawnTextureChestHairThick;

    static Color[] _pawnTextureEars;

    static Color[] _pawnTextureHair;

    static Color[] _pawnTextureHairFront;

    static Color[] _pawnTextureHead;

    static Color[] _pawnTextureHornsBack;

    static Color[] _pawnTextureHornsFront;

    static Color[] _pawnTextureOrcTeeth;

    public static event Action<Vector3Int, Vector3Int> CheckingAreaConstraints;

    public static event Action<int, int> CheckingLineConstraints;
    public static event Action ConfirmingObject;

    public static event Action LevelChangedLate;

    public static event Action LevelChanged;

    public static event Action ResetingSprite;

    public static event Action UpdatedGraphics;

    public static event Action UpdatingGraphics;
    public static BuildMode BuildMode { get; set; }
    public static SpriteSheet CornerSprites { get; private set; }
    public static SpriteSheet DoorSprites { get; private set; }
    public static Graphics Instance => _instance;
    public static bool Ready { get; private set; } = false;
    public static SpriteSheet2 WallSprites { get; private set; }
    public Color DemolishColor { get; } =  new Color(255, 0, 0, 0.5f);
    public Color HighlightColor { get; } = new Color(0, 255, 245, 0.5f);
    public WallMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            UpdateGraphics();
        }
    }

    public static int GetSortOrder(Vector3Int position)
    {
        return 1 - 2 * position.x - 2 * position.y + 2 * position.z;
    }

    public Sprite[] BuildSprites(int skinColor, int hairColor, int hornsColor, bool narrow, bool thick, int ears, bool orc, int hairType, int beardType, int horns, int bodyHair)
    {
        Texture2D copied = new Texture2D(PawnTextureBodyThick.width, PawnTextureBodyThick.height);
        copied.filterMode = FilterMode.Point;
        copied.wrapMode = TextureWrapMode.Clamp;
        Sprite[] sprites = new Sprite[48];

        ListDictionary skinColorMapping = new ListDictionary();
        ListDictionary hairColorMapping = new ListDictionary();
        ListDictionary hornColorMapping = new ListDictionary();

        int sheetWidth = PawnTextureBodyThick.width;

        //horns - 14
        //hair - 15
        //skin - 21

        for (int i = 0; i < 5; i++)
        {
            skinColorMapping[_pawnGradientGreyscale[i]] = _pawnGradientSkin[skinColor * 5 + i];
        }

        for (int i = 0; i < 4; i++)
        {
            hairColorMapping[_pawnGradientGreyscale[i + 1]] = _pawnGradientHair[hairColor * 4 + i];
        }

        for (int i = 0; i < 4; i++)
        {
            hornColorMapping[_pawnGradientGreyscale[i + 1]] = _pawnGradientHorns[hornsColor * 4 + i];
        }


        for (int i = 0; i < _pawnTextureBodyThick.Length; i++)
        {
            Color bodyPixel = Color.clear;
            if (bodyHair == 1)
                bodyPixel = thick ? _pawnTextureBodyHairThick[i] : _pawnTextureBodyHairMuscular[i];
            else if (bodyHair == 2)
                bodyPixel = thick ? _pawnTextureChestHairThick[i] : _pawnTextureChestHairMuscular[i];

            if (bodyHair == 0 || bodyPixel.a < 0.5f)
            {
                bodyPixel = thick ? _pawnTextureBodyThick[i] : _pawnTextureBodyMuscular[i];
                if (skinColorMapping.Contains(bodyPixel))
                    bodyPixel = (Color)skinColorMapping[bodyPixel];
            }
            else
            {
                if (hairColorMapping.Contains(bodyPixel))
                    bodyPixel = (Color)hairColorMapping[bodyPixel];
            }
            copied.SetPixel(i % sheetWidth, i / sheetWidth, bodyPixel);
            
        }

        for (int i = 0; i < headTable.Length; i++)
        {
            (int x, int y, int headDirection, bool flipped) = headTable[i];
            x += 64 * (i % 4);
            y += 64 * (i / 4);

            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    Color headPixel = GetHeadPixel(j, k, headDirection);

                    if (headPixel.a > 0.5f)
                    {
                        if (!flipped)
                            copied.SetPixel(x + j, y + k, headPixel);
                        else
                            copied.SetPixel(x + 15 - j, y + k, headPixel);
                    }
                }
            }
        }

        copied.Apply();
        for (int i = 0; i < headTable.Length; i++)
            sprites[i] = Sprite.Create(copied, new Rect(64 * (i % 4), 64 * (i / 4), 64, 64), new Vector2(0.5f, 5f/64), 6);

        return sprites;

        Color GetHeadPixel(int x, int y, int headDirection)
        {
            const int HAIROPTIONS = 5;
            const int HORNOPTIONS = 4;
            const int BEARDOPTIONS = 4;

            Color headPixel;
            if (hairType != 5)
            {
                headPixel = _pawnTextureHairFront[(y + 16 * headDirection) * (16 * HAIROPTIONS) + x + hairType * 16];
                if (hairColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)hairColorMapping[headPixel];
                    return headPixel;
                }
            }

            if (horns != 4)
            {
                headPixel = _pawnTextureHornsFront[(y + 16 * headDirection) * (16 * HORNOPTIONS) + x + horns * 16];
                if (hornColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)hornColorMapping[headPixel];
                    return headPixel;
                }
            }

            if (orc)
            {
                headPixel = _pawnTextureOrcTeeth[32 * (y + 16 * headDirection) + x + (narrow ? 16 : 0)];
                if (skinColorMapping.Contains(headPixel))
                    headPixel = (Color)skinColorMapping[headPixel];

                if (headPixel.a > 0.5f)
                    return headPixel;
            }

            if (ears != 2)
            {
                headPixel = _pawnTextureEars[32 * (y + 16 * headDirection) + x + ears * 16];
                if (skinColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)skinColorMapping[headPixel];
                    return headPixel;
                }
            }
            if (hairType != 5)
            {
                headPixel = _pawnTextureHair[(y + 16 * headDirection) * (16 * HAIROPTIONS) + x + hairType * 16];
                if (hairColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)hairColorMapping[headPixel];

                    if (hairType == 0)
                    {
                        Color skullPixel = _pawnTextureHead[32 * (y + 16 * headDirection) + x + (narrow ? 16 : 0)];
                        if (skinColorMapping.Contains(skullPixel))
                        {
                            skullPixel = (Color)skinColorMapping[skullPixel];

                            headPixel = new Color(headPixel.r / 2f + skullPixel.r / 2f, headPixel.g / 2f + skullPixel.g / 2f, headPixel.b / 2f + skullPixel.b / 2f);
                            return headPixel;
                        }
                    }
                    else
                        return headPixel;
                }
            }

            if (beardType != 4)
            {
                headPixel = _pawnTextureBeard[(y + 16 * headDirection) * (16 * BEARDOPTIONS) + x + beardType * 16];
                Color skullPixel = _pawnTextureHead[32 * (y + 16 * headDirection) + x + (narrow ? 16 : 0)];
                if (hairColorMapping.Contains(headPixel) && skinColorMapping.Contains(skullPixel))
                {
                    headPixel = (Color)hairColorMapping[headPixel];
                    if (beardType < 2)
                    {
                        skullPixel = (Color)skinColorMapping[skullPixel];

                        headPixel = new Color(headPixel.r / 2f + skullPixel.r / 2f, headPixel.g / 2f + skullPixel.g / 2f, headPixel.b / 2f + skullPixel.b / 2f);
                    }

                    return headPixel;

                }
            }

            if (horns != 4)
            {
                headPixel = _pawnTextureHornsBack[(y + 16 * headDirection) * (16 * HORNOPTIONS) + x + horns * 16];
                if (hornColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)hornColorMapping[headPixel];
                    return headPixel;
                }
            }

            headPixel = _pawnTextureHead[32 * (y + 16 * headDirection) + x + (narrow ? 16 : 0)];
            if (skinColorMapping.Contains(headPixel))
            {
                headPixel = (Color)skinColorMapping[headPixel];
            }
            return headPixel;
        }
    }

    public void Confirm()
    {
        ConfirmingObject?.Invoke();
        _lineStart = -1;
        _lineEnd = -1;
    }

    public void CycleMode()
    {
        switch (_mode)
        {
            case WallMode.Full:
                Mode = WallMode.Open;
                break;
            case WallMode.Open:
                Mode = WallMode.Base;
                break;
            case WallMode.Base:
                Mode = WallMode.Full;
                break;
        }
    }

    public void Demolish(SpriteObject spriteObject)
    {
        if (spriteObject is Wall wall && wall.IsDoor)
        {
            wall.RemoveDoor();
        }
        else
        {
            spriteObject.Destroy();
        }
    }

    public void HideHighlight()
    {
        _highlight.enabled = false;
        ResetingSprite?.Invoke();
    }

    public void HighlightDemolish(SpriteObject spriteObject)
    {
        ResetingSprite?.Invoke();

        spriteObject.Highlight(DemolishColor);
    }

    public void PlaceArea(Vector3Int endPosition)
    {
        if (areaEnd != endPosition)
        {
            int minX = areaStart.x < areaEnd.x ? areaStart.x : areaEnd.x;
            int maxX = areaStart.x > areaEnd.x ? areaStart.x : areaEnd.x;
            int minY = areaStart.y < areaEnd.y ? areaStart.y : areaEnd.y;
            int maxY = areaStart.y > areaEnd.y ? areaStart.y : areaEnd.y;

            for (int i = minX < endPosition.x ? minX : endPosition.x; i <= (maxX > endPosition.x ? maxX : endPosition.x); i++)
            {
                for (int j = minY < endPosition.y ? minY : endPosition.y; j <= (maxY > endPosition.y ? maxY : endPosition.y); j++)
                {
                    Vector3Int position = new Vector3Int(i, j, endPosition.z);
                    if ((i < minX || i > maxX || j < minY || j > maxY) && BuildFunctions.CheckPoint(position))
                    {
                        BuildFunctions.CreatePoint(position);
                    }
                }
            }

            areaEnd = endPosition;
            CheckingAreaConstraints?.Invoke(areaStart, areaEnd);
        }
    }

    public void PlaceArea(Vector3Int startPosition, Vector3Int endPosition)
    {
        areaStart = startPosition;
        areaEnd = endPosition;

        int minX = areaStart.x < areaEnd.x ? areaStart.x : areaEnd.x;
        int maxX = areaStart.x > areaEnd.x ? areaStart.x : areaEnd.x;
        int minY = areaStart.y < areaEnd.y ? areaStart.y : areaEnd.y;
        int maxY = areaStart.y > areaEnd.y ? areaStart.y : areaEnd.y;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                Vector3Int position = new Vector3Int(i, j, endPosition.z);
                if (BuildFunctions.CheckPoint(position))
                {
                    BuildFunctions.CreatePoint(position);
                }
            }
        }

    }

    public void PlaceDoor(Vector3Int position, MapAlignment alignment, AccentMaterial material)
    {
        if (Wall.CheckDoor(position, alignment))
        {
            Wall.CreateDoor(position, alignment);
        }
    }

    public void PlaceFloor(Room room, int floorType, bool overrideFloor)
    {
        foreach (RoomNode node in room.RoomNodeIterator())
        {
            Vector3Int position = room.GetWorldPosition(node);
            if (_map.IsSupported(position, MapAlignment.Center))
                if (!_map[position].Floor.Enabled || overrideFloor)
                    _map[position].Floor.SpriteIndex = floorType;
        }
    }
    public void PlaceLine(Vector3Int startPosition, int end, MapAlignment alignment)
    {
        int startX = startPosition.x;
        int startY = startPosition.y;
        int z = startPosition.z;

        if ((alignment == MapAlignment.XEdge ? _lineStart != startX : _lineStart != startY) || _lineEnd != end)
        {
            if (alignment == MapAlignment.XEdge)
            {
                for (int i = startX < end ? startX : end; i <= (startX < end ? end : startX); i++)
                {
                    if ((i < _lineStart || i > _lineEnd) && BuildFunctions.CheckLine(new Vector3Int(i, startY, z) ,alignment))
                    {
                        BuildFunctions.CreateLine(i, startY, z, alignment);
                    }
                }

                _lineStart = startX < end ? startX : end;
                _lineEnd = startX < end ? end : startX;
            }
            else
            {
                for (int i = startY < end ? startY : end; i <= (startY < end ? end : startY); i++)
                {
                    if ((i < _lineStart || i > _lineEnd) && BuildFunctions.CheckLine(new Vector3Int(startX, i, z), alignment))
                    {
                        BuildFunctions.CreateLine(startX, i, z, alignment);
                    }
                }

                _lineStart = startY < end ? startY : end;
                _lineEnd = startY < end ? end : startY;
            }


        }

        CheckingLineConstraints?.Invoke(_lineStart, _lineEnd);
    }
    public void ResetSprite()
    {
        ResetingSprite?.Invoke();
    }

    public void SetLevel()
    {
        LevelChanged?.Invoke();
        LevelChangedLate?.Invoke();
    }

    public void UpdateGraphics()
    {
        UpdatingGraphics?.Invoke();
        UpdatedGraphics?.Invoke();
    }
    /*void ConfigureCorners()
    {
        foreach(Vector3Int position in _cornersToBeConfigured)
        {
            int index = Corner.GetSpriteIndex(position);
            if(index == -1)
            {

            }
            else
            {
                Corner corner = Map.Instance.GetCorner(position);
            }
        }
    }*/   

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            StartCoroutine(Startup());
        }
        else
            Destroy(this);
    }

    IEnumerator Startup()
    {
        yield return new WaitUntil(() => Map.Instance != null);

        _map = Map.Instance;

        _highlight = Instantiate(SpritePrefab).GetComponent<SpriteRenderer>();

        _highlight.color = HighlightColor;

        CornerSprites = new SpriteSheet(9, 1);
        WallSprites = new SpriteSheet2(14, 1);
        DoorSprites = new SpriteSheet(8, 1);

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    CornerSprites[i, j, k] = UnsortedCornerSprites[i * 2 + k];
                }
            }
        }

        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                WallSprites[i, j] = UnsortedWallSprites[i];
            }
        }

        for(int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    DoorSprites[i, j, k] = UnsortedDoorSprites[2 * i + k];
                }
            }
        }

        _pawnGradientGreyscale = PawnGradientGreyscale.GetPixels();

        _pawnGradientHair = PawnGradientHair.GetPixels();

        _pawnGradientHorns = PawnGradientHorns.GetPixels();

        _pawnGradientSkin = PawnGradientSkin.GetPixels();

        _pawnTextureBeard = PawnTextureBeard.GetPixels();

        _pawnTextureBodyHairMuscular = PawnTextureBodyHairMuscular.GetPixels();

        _pawnTextureBodyHairThick = PawnTextureBodyHairThick.GetPixels();

        _pawnTextureBodyMuscular = PawnTextureBodyMuscular.GetPixels();

        _pawnTextureBodyThick = PawnTextureBodyThick.GetPixels();

        _pawnTextureChestHairMuscular = PawnTextureChestHairMuscular.GetPixels();

        _pawnTextureChestHairThick = PawnTextureChestHairThick.GetPixels();

        _pawnTextureEars = PawnTextureEars.GetPixels();

        _pawnTextureHair = PawnTextureHair.GetPixels();

        _pawnTextureHairFront = PawnTextureHairFront.GetPixels();

        _pawnTextureHead = PawnTextureHead.GetPixels();

        _pawnTextureHornsBack = PawnTextureHornsBack.GetPixels();

        _pawnTextureHornsFront = PawnTextureHornsFront.GetPixels();

        _pawnTextureOrcTeeth = PawnTextureOrcTeeth.GetPixels();

        Ready = true;
    }

    public class Corner : MonoBehaviour
    {
        static List<int> ignoreIndeces = new List<int>() { 1, 2, 4, 5, 8, 10 };
        static List<int> maskedIndeces = new List<int>() { 1, 2, 7, 8 };
        bool _configuring = false;
        Vector3Int _position;
        int _spriteIndex;
        SpriteRenderer _spriteRenderer;
        WallMaterial _wallMaterial;
        int _x, _y, _z;

        public static int GetSpriteIndex(Vector3Int position)
        {
            int index = 0;
            index += Instance._map.GetWall(MapAlignment.XEdge, position) != null ? 1 : 0;
            index += Instance._map.GetWall(MapAlignment.YEdge, position + Vector3Int.down) != null ? 2 : 0;
            index += Instance._map.GetWall(MapAlignment.XEdge, position + Vector3Int.left) != null ? 4 : 0;
            index += Instance._map.GetWall(MapAlignment.YEdge, position) != null ? 8 : 0;


            if (ignoreIndeces.Any(x => x == index))
            {
                
                return -1;
            }
            switch (index)
            {
                case int n when n >= 10:
                    index -= 7;
                    break;
                case int n when n >= 8:
                    index -= 6;
                    break;
                case int n when n >= 5:
                    index -= 5;
                    break;
                default:
                    index -= 3;
                    break;
            }
            if(index < 0)
            {
                return -1;
            }

            return index;

            
        }

        public void SetCorner(Vector3Int position, WallMaterial wallMaterial)
        {
            _x = position.x;
            _y = position.y;
            _z = position.z;

            _position = position;
            _wallMaterial = wallMaterial;

            transform.position = Map.MapCoordinatesToSceneCoordinates(position, MapAlignment.Corner);
            _spriteRenderer.sortingOrder = GetSortOrder(position) + 3;

            if (!_configuring)
            {
                UpdatingGraphics += ConfigureCorner;
                _configuring = true;
            }
        }

        public void UpdateCorner()
        {
            ConfigureCorner();
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdatedGraphics += SetCornerMode;
            LevelChangedLate += SetLevel;
        }

        void ConfigureCorner()
        {
            UpdatingGraphics -= ConfigureCorner;
            _configuring = false;

             
            _spriteIndex = GetSpriteIndex(_position);

            if(_spriteIndex == -1)
            {
                UpdatedGraphics -= SetCornerMode;
                LevelChangedLate -= SetLevel;
                //Map.Instance.GetCorner(_x, _y, _z).enabled = false;

                Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z)?.WallSprite.MaskCorner(false);
                Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z)?.WallSprite.MaskCorner(false);

                Destroy(gameObject);
                return;
            }

            if (_spriteIndex == 1 || _spriteIndex == 7 || _spriteIndex == 8)
            {
                Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z).WallSprite.MaskCorner(true);
                Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z).WallSprite.MaskCorner(false);
            }
            else if(_spriteIndex == 2)
            {
                Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z).WallSprite.MaskCorner(false);
                Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z).WallSprite.MaskCorner(true);
            }
            else
            {
                Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z)?.WallSprite.MaskCorner(false);
                Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z)?.WallSprite.MaskCorner(false);
            }
        }

        void SetCornerMode()
        {
            if (GameManager.Instance.IsOnLevel(_z) == 0)
            {
                if (Instance.Mode == WallMode.Open)
                {
                    bool? xPos = Instance._map.GetWall(MapAlignment.XEdge, _x, _y, _z)?.WallSprite.IsFullWall;
                    bool? yNeg = Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z)?.WallSprite.IsFullWall;
                    bool? xNeg = Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z)?.WallSprite.IsFullWall;
                    bool? yPos = Instance._map.GetWall(MapAlignment.YEdge, _x, _y, _z)?.WallSprite.IsFullWall;
                    bool fullCorner = (xPos ?? false) || (yNeg ?? false) || (xNeg ?? false) || (yPos ?? false);
                    _spriteRenderer.sprite = CornerSprites[_spriteIndex, _wallMaterial, fullCorner];

                    if (fullCorner)
                    {
                        if (!xPos ?? false)
                            Instance._map.GetWall(MapAlignment.XEdge, _x, _y, _z).WallSprite.SetEdge();
                        if (!yPos ?? false)
                            Instance._map.GetWall(MapAlignment.YEdge, _x, _y, _z).WallSprite.SetEdge();
                        if (!yNeg ?? false)
                            Instance._map.GetWall(MapAlignment.YEdge, _x, _y - 1, _z).WallSprite.SetEdge();
                        if (!xNeg ?? false)
                            Instance._map.GetWall(MapAlignment.XEdge, _x - 1, _y, _z).WallSprite.SetEdge();
                    }
                }
                else
                {
                    _spriteRenderer.sprite = CornerSprites[_spriteIndex, _wallMaterial, Instance.Mode == WallMode.Full];
                }
            }
        }

        void SetLevel()
        {
            int level = GameManager.Instance.IsOnLevel(_z);
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
                    _spriteRenderer.sprite = CornerSprites[_spriteIndex, _wallMaterial, true];
                }
            }
        }
    }

    public class SpriteSheet
    {
        Sprite[,,] _sprites;

        public SpriteSheet(int i, int j)
        {
            _sprites = new Sprite[i, j, 2];
        }

        public Sprite this[WallSpriteType WallSpriteType, WallMaterial wallMaterial, bool isFullWall] => _sprites[(int)WallSpriteType, (int)wallMaterial, isFullWall ? 0 : 1];

        public Sprite this[DoorSpriteType DoorSpriteType, AccentMaterial material, bool isFullWall] => _sprites[(int)DoorSpriteType, (int)material, isFullWall ? 0 : 1];
        public Sprite this[int spriteIndex, WallMaterial wallMaterial, bool isFullWall] => _sprites[spriteIndex, (int)wallMaterial, isFullWall ? 0 : 1];

        public Sprite this[int i, int j, int k]
        {
            set
            {
                _sprites[i, j, k] = value;
            }
        }
    }

    public class SpriteSheet2
    {
        Sprite[,] _sprites;

        public SpriteSheet2(int i, int j)
        {
            _sprites = new Sprite[i, j];
        }

        public Sprite this[WallSpriteType WallSpriteType, WallMaterial wallMaterial] => _sprites[(int)WallSpriteType, (int)wallMaterial];

        public Sprite this[int spriteIndex, WallMaterial wallMaterial] => _sprites[spriteIndex, (int)wallMaterial];

        public Sprite this[int i, int j]
        {
            set
            {
                _sprites[i, j] = value;
            }
        }
    }
}