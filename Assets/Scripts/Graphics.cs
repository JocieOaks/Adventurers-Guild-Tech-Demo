using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System;
using UnityEngine.Rendering;

/// <summary>
/// Designates the material used for an accent object and determines which <see cref="Sprite"/> is used for it.
/// </summary>
public enum AccentMaterial
{
    Stone = 0
}

/// <summary>
/// Designates the material used for a <see cref="WallSprite"/> and determines which <see cref="Sprite"/> is used for it.
/// </summary>
public enum WallMaterial
{
    Brick = 2,
    Plaster = 1,
    StoneBrick = 0
}

/// <summary>
/// Enum used to represent how <see cref="WallSprite"/>s should appear.
/// </summary>
public enum WallDisplayMode
{
    /// <value>Walls should be fully shown.</value>
    Full = 0,
    /// <value>Walls should only show the base of the <see cref="WallSprite"/>.</value>
    Base = 1,
    /// <value>Walls should be fully visible if the do not border a <see cref="Room"/> behind them. Otherwise they should only show the base.</value>
    Open = 2
}

/// <summary>
/// The <see cref="Graphics"/> class is a singleton that controls graphical aspects of the game, and holds reference to the <see cref="Sprite"/>s used by the game.
/// </summary>
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

    public Sprite Marker;

    public Texture2D PawnGradientGreyscale;

    public Texture2D PawnGradientHair;

    public Texture2D PawnGradientHorns;

    public Texture2D PawnGradientSkin;

    public Pawn PawnPrefab;

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

    public SortingGroup SortingObject;
    public SpriteRenderer SpeechBubble;

    public GameObject SpritePrefab;

    public Sprite StairsEast;
    public Sprite StairsNorth;

    public Sprite StairsSouth;
    public Sprite StairsWest;

    public Sprite Stool;

    public Sprite[] TableRound;

    public Sprite[] TableSquare;

    public Sprite[] UnsortedCornerSprites;

    public Sprite[] UnsortedDoorSprites;

    public Sprite[] UnsortedWallSprites;

    public Sprite Wave;
    static readonly (int xOffset, int yOffset, int headDirection, bool flipped)[] headTable = new (int, int, int, bool)[] {
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

    static Graphics _instance;

    static Color[] _pawnGradientGreyscale;
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
    readonly Dictionary<Vector3Int, Corner> _cornerDictionary = new ();

    WallDisplayMode _mode = WallDisplayMode.Open;

    public static event Action LevelChanged;

    public static event Action LevelChangedLate;
    public static event Action ResetingSprite;

    public static event Action UpdatedGraphics;

    public static event Action UpdatingGraphics;

    /// <value>The <see cref="SpriteSheet"/> containing all the <see cref="Sprite"/>s for <see cref="Corner"/>s.</value>
    public static SpriteSheet CornerSprites { get; private set; }

    /// <value>The <see cref="SpriteSheet"/> containing all the <see cref="Sprite"/>s for doors.</value>
    public static SpriteSheet DoorSprites { get; private set; }

    /// <value>Gives access to the <see cref="Graphics"/> singleton instance.</value>
    public static Graphics Instance => _instance;

    /// <value>Signifies if <see cref="Graphics"/> has finished its initial setup.</value>
    public static bool Ready { get; private set; } = false;

    /// <value>The <see cref="SpriteSheet2"/> containing all the <see cref="Sprite"/>s for <see cref="WallSprite"/>s.</value>
    public static SpriteSheet2 WallSprites { get; private set; }

    /// <value>The <see cref="Queue{T}"/> of <see cref="Map"/> positions to be checked for if a <see cref="Corner"/> needs to be 
    /// placed, removed, or modified due to the changing of <see cref="WallSprite"/>s.</value>
    public Queue<Vector3Int> CornerQueue { get; } = new Queue<Vector3Int>();

    /// <value>The <see cref="Color"/> used to highlight a <see cref="SpriteObject"/> to be destroyed.</value>
    public Color DemolishColor => new(255, 0, 0, 0.5f);

    /// <value>The <see cref="Color"/> used to highlight a <see cref="SpriteObject"/> to be built or changed.</value>
    public Color HighlightColor => new(0, 255, 245, 0.5f);

    /// <value>The <see cref="WallDisplayMode"/> for how <see cref="WallSprite"/>s should be presented.</value>
    public WallDisplayMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            UpdateGraphics();
        }
    }

    public Sprite[] BuildSprites(int skinColor, int hairColor, int hornsColor, bool narrow, bool thick, int ears, bool orc, int hairType, int beardType, int horns, int bodyHair)
    {
        Texture2D copied = new(PawnTextureBodyThick.width, PawnTextureBodyThick.height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Sprite[] sprites = new Sprite[48];

        ListDictionary skinColorMapping = new();
        ListDictionary hairColorMapping = new();
        ListDictionary hornColorMapping = new();

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

    /// <summary>
    /// Changes the current <see cref="WallDisplayMode"/>.
    /// </summary>
    public void CycleWallDisplayMode()
    {
        switch (_mode)
        {
            case WallDisplayMode.Full:
                Mode = WallDisplayMode.Open;
                break;
            case WallDisplayMode.Open:
                Mode = WallDisplayMode.Base;
                break;
            case WallDisplayMode.Base:
                Mode = WallDisplayMode.Full;
                break;
        }
    }

    /// <summary>
    /// Hides the highlight <see cref="SpriteRenderer"/> from being displayed.
    /// </summary>
    public void HideHighlight()
    {
        _highlight.enabled = false;
        ResetingSprite?.Invoke();
    }

    /// <summary>
    /// Highlights the given <see cref="SpriteObject"/> to be demolished.
    /// </summary>
    /// <param name="spriteObject">The <see cref="SpriteObject"/> potentially being demolished.</param>
    public void HighlightDemolish(SpriteObject spriteObject)
    {
        ResetingSprite?.Invoke();

        spriteObject.Highlight(DemolishColor);
    }

    /// <summary>
    /// Evaluates if the given position has a <see cref="Corner"/>.
    /// </summary>
    /// <param name="position">The <see cref="Map"/> coordinates being evaluated.</param>
    /// <returns>Returns true if there is a <see cref="Corner"/> at <c>position</c>.</returns>
    public bool IsCorner(Vector3Int position)
    {
        return _cornerDictionary.TryGetValue(position, out Corner corner) && corner != null;
    }

    /// <summary>
    /// Reset's all highlighted <see cref="SpriteObject"/>s to their original state.
    /// </summary>
    public void ResetSprite()
    {
        ResetingSprite?.Invoke();
    }

    /// <summary>
    /// Calls all <see cref="SpriteObject"/>s and those subscribed to the level change events.
    /// </summary>
    public void SetLevel()
    {
        LevelChanged?.Invoke();
        LevelChangedLate?.Invoke();
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Checks all the positions in <see cref="CornerQueue"/> and places or modifies the <see cref="Corner"/>s at those positions, if necessary.
    /// </summary>
    void SetCorners()
    {
        while (CornerQueue.Count > 0)
        {
            Vector3Int position = CornerQueue.Dequeue();

            if (_cornerDictionary.TryGetValue(position, out Corner corner) && corner != null)
            {
                corner.ConfigureCorner();
            }
            else if (Corner.TryMakeCorner(position, out corner))
            {
                _cornerDictionary[position] =  corner;
            }
        }
    }

    /// <summary>
    /// Called when the instance is first created at the start of the game.
    /// </summary>
    /// <returns>Yield returns <see cref="WaitUntil"/> objects to wait for the <see cref="Map.Instance"/>.</returns>
    IEnumerator Startup()
    {
        yield return new WaitUntil(() => Map.Instance != null);

        _highlight = Instantiate(SpritePrefab).GetComponent<SpriteRenderer>();

        _highlight.color = HighlightColor;

        CornerSprites = new SpriteSheet(9, 1);
        WallSprites = new SpriteSheet2(14, 3);
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
            for (int j = 0; j < 3; j++)
            {
                WallSprites[i, j] = UnsortedWallSprites[j * 12 + i];
            }
        }

        for (int i = 0; i < 6; i++)
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

    /// <summary>
    /// Calls the update graphics events.
    /// </summary>
    public void UpdateGraphics()
    {
        UpdatingGraphics?.Invoke();
        SetCorners();
        UpdatedGraphics?.Invoke();
    }  

    /// <summary>
    /// The <see cref="SpriteSheet"/> class is a 3D array of <see cref="Sprite"/>s.
    /// </summary>
    public class SpriteSheet
    {
        readonly Sprite[,,] _sprites;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteSheet"/> class.
        /// </summary>
        /// <param name="types">The number of types of <see cref="Sprite"/>s there can be.</param>
        /// <param name="materials">The number of materials the <see cref="Sprite"/>s can be made of.</param>
        public SpriteSheet(int types, int materials)
        {
            _sprites = new Sprite[types, materials, 2];
        }

        /// <summary>
        /// Indexer for the <see cref="SpriteSheet"/>
        /// </summary>
        /// <param name="WallSpriteType">The <see cref="WallSpriteType"/> of the <see cref="Sprite"/>.</param>
        /// <param name="wallMaterial">The <see cref="WallMaterial"/> of the <see cref="Sprite"/>.</param>
        /// <param name="isFullWall">Determines if the <see cref="Sprite"/> is for a full wall or a base wall.</param>
        /// <returns>The <see cref="Sprite"/> with the given features.</returns>
        public Sprite this[WallSpriteType WallSpriteType, WallMaterial wallMaterial, bool isFullWall] => _sprites[(int)WallSpriteType, (int)wallMaterial, isFullWall ? 0 : 1];

        /// <summary>
        /// Indexer for the <see cref="SpriteSheet"/>
        /// </summary>
        /// <param name="DoorSpriteType">The <see cref="DoorSpriteType"/> of the <see cref="Sprite"/>.</param>
        /// <param name="material">The <see cref="AccentMaterial"/> of the <see cref="Sprite"/>.</param>
        /// <param name="isFullWall">Determines if the <see cref="Sprite"/> is for a full wall or a base wall.</param>
        /// <returns>The <see cref="Sprite"/> with the given features.</returns>
        public Sprite this[DoorSpriteType DoorSpriteType, AccentMaterial material, bool isFullWall] => _sprites[(int)DoorSpriteType, (int)material, isFullWall ? 0 : 1];

        /// <summary>
        /// Indexer for the <see cref="SpriteSheet"/>
        /// </summary>
        /// <param name="spriteIndex">The index of the <see cref="Sprite"/>.</param>
        /// <param name="material">The <see cref="AccentMaterial"/> of the <see cref="Sprite"/>.</param>
        /// <param name="isFullWall">Determines if the <see cref="Sprite"/> is for a full wall or a base wall.</param>
        /// <returns>The <see cref="Sprite"/> with the given features.</returns>
        public Sprite this[int spriteIndex, AccentMaterial material, bool isFullWall] => _sprites[spriteIndex, (int)material, isFullWall ? 0 : 1];

        /// <summary>
        /// Indexer to populate the <see cref="SpriteSheet"/>.
        /// </summary>
        public Sprite this[int i, int j, int k]
        {
            set
            {
                _sprites[i, j, k] = value;
            }
        }
    }

    /// <summary>
    /// The <see cref="SpriteSheet2"/> class is a 2D array of <see cref="Sprite"/>s.
    /// </summary>
    public class SpriteSheet2
    {
        readonly Sprite[,] _sprites;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteSheet2"/> class.
        /// </summary>
        /// <param name="types">The number of types of <see cref="Sprite"/>s there can be.</param>
        /// <param name="materials">The number of materials the <see cref="Sprite"/>s can be made of.</param>
        public SpriteSheet2(int types, int materials)
        {
            _sprites = new Sprite[materials, types];
        }

        /// <summary>
        /// Indexer for the <see cref="SpriteSheet2"/>.
        /// </summary>
        /// <param name="WallSpriteType">The <see cref="WallSpriteType"/> of the <see cref="Sprite"/>.</param>
        /// <param name="wallMaterial">The <see cref="WallMaterial"/> of the <see cref="Sprite"/>.</param>
        /// <returns>The <see cref="Sprite"/> with the given features.</returns>
        public Sprite this[WallSpriteType WallSpriteType, WallMaterial wallMaterial] => _sprites[(int)wallMaterial, (int)WallSpriteType];

        /// <summary>
        /// Indexer for the <see cref="SpriteSheet2"/>
        /// </summary>
        /// <param name="spriteIndex">The index of the <see cref="Sprite"/>.</param>
        /// <param name="wallMaterial">The <see cref="WallMaterial"/> of the <see cref="Sprite"/>.</param>
        /// <returns>The <see cref="Sprite"/> with the given features.</returns>
        public Sprite this[int spriteIndex, WallMaterial wallMaterial] => _sprites[(int)wallMaterial, spriteIndex];

        /// <summary>
        /// Indexer to populate the <see cref="SpriteSheet2"/>.
        /// </summary>
        public Sprite this[int i, int j]
        {
            set
            {
                _sprites[j, i] = value;
            }
        }
    }
}