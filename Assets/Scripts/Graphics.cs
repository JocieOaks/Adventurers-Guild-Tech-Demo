using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Scripts.AI;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Sprite_Object;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts
{
    /// <summary>
    /// Designates the material used for an accent object and determines which <see cref="Sprite"/> is used for it.
    /// </summary>
    public enum AccentMaterial
    {
        Stone = 0
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
    /// Designates the material used for a <see cref="WallSprite"/> and determines which <see cref="Sprite"/> is used for it.
    /// </summary>
    public enum WallMaterial
    {
        Brick = 2,
        Plaster = 1,
        StoneBrick = 0
    }
    /// <summary>
    /// The <see cref="Graphics"/> class is a singleton that controls graphical aspects of the game, and holds reference to the <see cref="Sprite"/>s used by the game.
    /// </summary>
    public class Graphics : MonoBehaviour
    {
        public SpriteRenderer Highlight;

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

        public Texture2D PawnGradientGreyscale;
        public Texture2D PawnGradientHair;
        public Texture2D PawnGradientHorns;
        public Texture2D PawnGradientSkin;

        public AdventurerPawn PawnPrefab;

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

        private static Color[] s_pawnGradientGreyscale;
        private static Color[] s_pawnGradientHair;
        private static Color[] s_pawnGradientHorns;
        private static Color[] s_pawnGradientSkin;
        private static int s_pawnSpriteSheetArrayLength;
        private static int s_pawnSpriteSheetHeight;
        private static int s_pawnSpriteSheetWidth;
        private static Color[] s_pawnTextureBeard;
        private static Color[] s_pawnTextureBodyMuscular;
        private static Color[] s_pawnTextureEars;
        private static Color[] s_pawnTextureHair;
        private static Color[] s_pawnTextureHairFront;
        private static Color[] s_pawnTextureHead;
        private static Color[] s_pawnTextureHornsBack;
        private static Color[] s_pawnTextureHornsFront;
        private static Color[] s_pawnTextureOrcTeeth;
        private readonly Dictionary<Vector3Int, Corner> _cornerDictionary = new ();

        private WallDisplayMode _mode = WallDisplayMode.Open;

        public static event Action LevelChanged;

        public static event Action LevelChangedLate;
        public static event Action ResettingSprite;

        public static event Action UpdatedGraphics;

        public static event Action UpdatingGraphics;

        /// <value>The <see cref="SpriteSheet"/> containing all the <see cref="Sprite"/>s for <see cref="Corner"/>s.</value>
        public static SpriteSheet CornerSprites { get; private set; }

        /// <value>The <see cref="SpriteSheet"/> containing all the <see cref="Sprite"/>s for doors.</value>
        public static SpriteSheet DoorSprites { get; private set; }

        /// <value>Gives access to the <see cref="Graphics"/> singleton instance.</value>
        public static Graphics Instance { get; private set; }

        /// <value>Signifies if <see cref="Graphics"/> has finished its initial setup.</value>
        public static bool Ready { get; private set; }

        /// <value>The <see cref="SpriteSheet2"/> containing all the <see cref="Sprite"/>s for <see cref="WallSprite"/>s.</value>
        public static SpriteSheet2 WallSprites { get; private set; }

        /// <value>The <see cref="Queue{T}"/> of <see cref="Map"/> positions to be checked for if a <see cref="Corner"/> needs to be 
        /// placed, removed, or modified due to the changing of <see cref="WallSprite"/>s.</value>
        public Queue<Vector3Int> CornerQueue { get; } = new();

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

        /// <summary>
        /// Initializes and schedules a new <see cref="BuildSpriteJob"/> to create a sprite sheet for a character's appearance.
        /// </summary>
        /// <param name="appearance">An <see cref="ActorAppearance"/> defining how the sprite sheet should look.</param>
        /// <param name="pixels">An out <see cref="NativeArray{T}"/> of <see cref="Color"/> containing the pixel data for the sprite sheet once it's completed.</param>
        /// <returns>Returns the scheduled <see cref="JobHandle"/> for the <see cref="BuildSpriteJob"/>.</returns>
        public JobHandle BuildSprites(ActorAppearance appearance, out NativeArray<Color> pixels)
        {
            pixels = new NativeArray<Color>(s_pawnSpriteSheetArrayLength, Allocator.Persistent);
            BuildSpriteJob buildSpriteJob = new(appearance, pixels);
            return buildSpriteJob.Schedule();
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
            Highlight.enabled = false;
            ResettingSprite?.Invoke();
        }

        /// <summary>
        /// Highlights the given <see cref="SpriteObject"/> to be demolished.
        /// </summary>
        /// <param name="spriteObject">The <see cref="SpriteObject"/> potentially being demolished.</param>
        public void HighlightDemolish(SpriteObject spriteObject)
        {
            ResettingSprite?.Invoke();

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
            ResettingSprite?.Invoke();
        }

        /// <summary>
        /// Calls all <see cref="SpriteObject"/>s and those subscribed to the level change events.
        /// </summary>
        public void SetLevel()
        {
            LevelChanged?.Invoke();
            LevelChangedLate?.Invoke();
        }

        public Sprite[] SliceSprites(Color[] pixels)
        {
            Texture2D copied = new(s_pawnSpriteSheetWidth, s_pawnSpriteSheetHeight)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            copied.SetPixels(pixels);
            copied.Apply();

            var sprites = new Sprite[40];
            for (var i = 0; i < 40; i++)
                sprites[i] = Sprite.Create(copied,
                    new Rect(64 * (i % 5),
                        64 * (i / 5),
                        64,
                        64),
                    new Vector2(0.5f,
                        5f / 64),
                    6);
            return sprites;
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

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                StartCoroutine(Startup());
            }
            else
                Destroy(this);
        }

        /// <summary>
        /// Checks all the positions in <see cref="CornerQueue"/> and places or modifies the <see cref="Corner"/>s at those positions, if necessary.
        /// </summary>
        private void SetCorners()
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
                    _cornerDictionary[position] = corner;
                }
            }
        }

        /// <summary>
        /// Called when the instance is first created at the start of the game.
        /// </summary>
        /// <returns>Yield returns <see cref="WaitUntil"/> objects to wait for the <see cref="Map.Instance"/>.</returns>
        private IEnumerator Startup()
        {
            yield return new WaitUntil(() => Map.Map.Instance != null);

            Highlight = Instantiate(SpritePrefab).GetComponent<SpriteRenderer>();

            Highlight.color = HighlightColor;

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

            s_pawnGradientGreyscale = PawnGradientGreyscale.GetPixels();

            s_pawnGradientHair = PawnGradientHair.GetPixels();

            s_pawnGradientHorns = PawnGradientHorns.GetPixels();

            s_pawnGradientSkin = PawnGradientSkin.GetPixels();

            s_pawnTextureBeard = PawnTextureBeard.GetPixels();

            PawnTextureBodyHairMuscular.GetPixels();

            PawnTextureBodyHairThick.GetPixels();

            s_pawnTextureBodyMuscular = PawnTextureBodyMuscular.GetPixels();

            PawnTextureBodyThick.GetPixels();

            PawnTextureChestHairMuscular.GetPixels();

            PawnTextureChestHairThick.GetPixels();

            s_pawnTextureEars = PawnTextureEars.GetPixels();

            s_pawnTextureHair = PawnTextureHair.GetPixels();

            s_pawnTextureHairFront = PawnTextureHairFront.GetPixels();

            s_pawnTextureHead = PawnTextureHead.GetPixels();

            s_pawnTextureHornsBack = PawnTextureHornsBack.GetPixels();

            s_pawnTextureHornsFront = PawnTextureHornsFront.GetPixels();

            s_pawnTextureOrcTeeth = PawnTextureOrcTeeth.GetPixels();

            s_pawnSpriteSheetWidth = PawnTextureBodyMuscular.width;

            s_pawnSpriteSheetHeight = PawnTextureBodyMuscular.height;

            s_pawnSpriteSheetArrayLength = s_pawnTextureBodyMuscular.Length;

            Ready = true;
        }

        /// <summary>
        /// The <see cref="BuildSpriteJob"/> struct is an <see cref="IJob"/> that creates a sprite sheet as an array of pixels, using a worker thread.
        /// </summary>
        private struct BuildSpriteJob : IJob
        {
            private readonly bool _narrow, _thick, _tusks;
            private NativeArray<Color> _pixels;
            private readonly int _skinColor, _hairColor, _hornsColor, _ears, _hairType, _beardType, _horns, _bodyHair;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildSpriteJob"/> struct.
            /// </summary>
            /// <param name="appearance">An <see cref="ActorAppearance"/> defining how the sprite sheet should look.</param>
            /// <param name="pixels">A <see cref="NativeArray{T}"/> of <see cref="Color"/> containing the pixel data for the sprite sheet once it's completed.</param>
            public BuildSpriteJob(ActorAppearance appearance, NativeArray<Color> pixels)
            {
                _skinColor = appearance.SkinColor;
                _hairColor = appearance.HairColor;
                _hornsColor = appearance.HornsColor;
                _narrow = appearance.Narrow;
                _thick = appearance.Thick;
                _ears = appearance.Ears;
                _tusks = appearance.Tusks;
                _hairType = appearance.HairType;
                _beardType = appearance.BeardType;
                _horns = appearance.Horns;
                _bodyHair = appearance.BodyHair;
                _pixels = pixels;
            }

            /// <inheritdoc/>
            public void Execute()
            {
                ListDictionary skinColorMapping = new();
                ListDictionary hairColorMapping = new();
                ListDictionary hornColorMapping = new();

                //horns - 14
                //hair - 15
                //skin - 21

                for (int i = 0; i < 5; i++)
                {
                    skinColorMapping[s_pawnGradientGreyscale[i]] = s_pawnGradientSkin[_skinColor * 5 + i];
                }

                for (int i = 0; i < 4; i++)
                {
                    hairColorMapping[s_pawnGradientGreyscale[i + 1]] = s_pawnGradientHair[_hairColor * 4 + i];
                }

                for (int i = 0; i < 4; i++)
                {
                    hornColorMapping[s_pawnGradientGreyscale[i + 1]] = s_pawnGradientHorns[_hornsColor * 4 + i];
                }


                for (int i = 0; i < s_pawnSpriteSheetArrayLength; i++)
                {
                    Color bodyPixel = Color.clear;
                    //Temp change for testing new sprites
                    bodyPixel = s_pawnTextureBodyMuscular[i];
                    if (skinColorMapping.Contains(bodyPixel))
                        bodyPixel = (Color)skinColorMapping[bodyPixel];
                    /*if (_bodyHair == 1)
                    bodyPixel = _thick ? _pawnTextureBodyHairThick[i] : _pawnTextureBodyHairMuscular[i];
                else if (_bodyHair == 2)
                    bodyPixel = _thick ? _pawnTextureChestHairThick[i] : _pawnTextureChestHairMuscular[i];

                if (_bodyHair == 0 || bodyPixel.a < 0.5f)
                {
                    bodyPixel = _thick ? _pawnTextureBodyThick[i] : _pawnTextureBodyMuscular[i];
                    if (skinColorMapping.Contains(bodyPixel))
                        bodyPixel = (Color)skinColorMapping[bodyPixel];
                }
                else
                {
                    if (hairColorMapping.Contains(bodyPixel))
                        bodyPixel = (Color)hairColorMapping[bodyPixel];
                }*/
                    _pixels[i] = bodyPixel;
                }

                /*for (int i = 0; i < headTable.Length; i++)
            {
                (int x, int y, int headDirection, bool flipped) = headTable[i];
                x += 64 * (i % 4);
                y += 64 * (i / 4);

                for (int j = 0; j < 16; j++)
                {
                    for (int k = 0; k < 16; k++)
                    {
                        Color headPixel = GetHeadPixel(j, k, headDirection, skinColorMapping, hairColorMapping, hornColorMapping);

                        if (headPixel.a > 0.5f)
                        {
                            if (!flipped)
                                _pixels[(y + k) * 256 + x + j] = headPixel;
                            else
                                _pixels[(y + k) * 256 + x + 15 - j] = headPixel;
                        }
                    }
                }
            }*/
            }

            /// <summary>
            /// Determines the color of a pixel at a given position, based on the 16x16 sprite of an <see cref="Actor"/>'s head.
            /// </summary>
            /// <param name="x">The x position of the pixel within the 16x16 square.</param>
            /// <param name="y">The y position of the pixel within the 16x16 square.</param>
            /// <param name="headDirection">The direction the head is facing.</param>
            /// <param name="skinColorMapping">A <see cref="ListDictionary"/> containing the <see cref="Actor"/>'s skin colors.</param>
            /// <param name="hairColorMapping">A <see cref="ListDictionary"/> containing the <see cref="Actor"/>'s hair colors.</param>
            /// <param name="hornColorMapping">A <see cref="ListDictionary"/> containing the <see cref="Actor"/>'s horn colors.</param>
            /// <returns></returns>
            private Color GetHeadPixel(int x, int y, int headDirection, ListDictionary skinColorMapping, ListDictionary hairColorMapping, ListDictionary hornColorMapping)
            {
                const int hairoptions = 5;
                const int hornoptions = 4;
                const int beardoptions = 4;

                Color headPixel;
                if (_hairType != 5)
                {
                    headPixel = s_pawnTextureHairFront[(y + 16 * headDirection) * (16 * hairoptions) + x + _hairType * 16];
                    if (hairColorMapping.Contains(headPixel))
                    {
                        headPixel = (Color)hairColorMapping[headPixel];
                        return headPixel;
                    }
                }

                if (_horns != 4)
                {
                    headPixel = s_pawnTextureHornsFront[(y + 16 * headDirection) * (16 * hornoptions) + x + _horns * 16];
                    if (hornColorMapping.Contains(headPixel))
                    {
                        headPixel = (Color)hornColorMapping[headPixel];
                        return headPixel;
                    }
                }

                if (_tusks)
                {
                    headPixel = s_pawnTextureOrcTeeth[32 * (y + 16 * headDirection) + x + (_narrow ? 16 : 0)];
                    if (skinColorMapping.Contains(headPixel))
                        headPixel = (Color)skinColorMapping[headPixel];

                    if (headPixel.a > 0.5f)
                        return headPixel;
                }

                if (_ears != 2)
                {
                    headPixel = s_pawnTextureEars[32 * (y + 16 * headDirection) + x + _ears * 16];
                    if (skinColorMapping.Contains(headPixel))
                    {
                        headPixel = (Color)skinColorMapping[headPixel];
                        return headPixel;
                    }
                }
                if (_hairType != 5)
                {
                    headPixel = s_pawnTextureHair[(y + 16 * headDirection) * (16 * hairoptions) + x + _hairType * 16];
                    if (hairColorMapping.Contains(headPixel))
                    {
                        headPixel = (Color)hairColorMapping[headPixel];

                        if (_hairType == 0)
                        {
                            Color skullPixel = s_pawnTextureHead[32 * (y + 16 * headDirection) + x + (_narrow ? 16 : 0)];
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

                if (_beardType != 4)
                {
                    headPixel = s_pawnTextureBeard[(y + 16 * headDirection) * (16 * beardoptions) + x + _beardType * 16];
                    Color skullPixel = s_pawnTextureHead[32 * (y + 16 * headDirection) + x + (_narrow ? 16 : 0)];
                    if (hairColorMapping.Contains(headPixel) && skinColorMapping.Contains(skullPixel))
                    {
                        headPixel = (Color)hairColorMapping[headPixel];
                        if (_beardType < 2)
                        {
                            skullPixel = (Color)skinColorMapping[skullPixel];

                            headPixel = new Color(headPixel.r / 2f + skullPixel.r / 2f, headPixel.g / 2f + skullPixel.g / 2f, headPixel.b / 2f + skullPixel.b / 2f);
                        }

                        return headPixel;

                    }
                }

                if (_horns != 4)
                {
                    headPixel = s_pawnTextureHornsBack[(y + 16 * headDirection) * (16 * hornoptions) + x + _horns * 16];
                    if (hornColorMapping.Contains(headPixel))
                    {
                        headPixel = (Color)hornColorMapping[headPixel];
                        return headPixel;
                    }
                }

                headPixel = s_pawnTextureHead[32 * (y + 16 * headDirection) + x + (_narrow ? 16 : 0)];
                if (skinColorMapping.Contains(headPixel))
                {
                    headPixel = (Color)skinColorMapping[headPixel];
                }
                return headPixel;
            }
        }

        /// <summary>
        /// The <see cref="SpriteSheet"/> class is a 3D array of <see cref="Sprite"/>s.
        /// </summary>
        public class SpriteSheet
        {
            private readonly Sprite[,,] _sprites;

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
            /// <param name="wallSpriteType">The <see cref="WallSpriteType"/> of the <see cref="Sprite"/>.</param>
            /// <param name="wallMaterial">The <see cref="WallMaterial"/> of the <see cref="Sprite"/>.</param>
            /// <param name="isFullWall">Determines if the <see cref="Sprite"/> is for a full wall or a base wall.</param>
            /// <returns>The <see cref="Sprite"/> with the given features.</returns>
            public Sprite this[WallSpriteType wallSpriteType, WallMaterial wallMaterial, bool isFullWall] => _sprites[(int)wallSpriteType, (int)wallMaterial, isFullWall ? 0 : 1];

            /// <summary>
            /// Indexer for the <see cref="SpriteSheet"/>
            /// </summary>
            /// <param name="doorSpriteType">The <see cref="DoorSpriteType"/> of the <see cref="Sprite"/>.</param>
            /// <param name="material">The <see cref="AccentMaterial"/> of the <see cref="Sprite"/>.</param>
            /// <param name="isFullWall">Determines if the <see cref="Sprite"/> is for a full wall or a base wall.</param>
            /// <returns>The <see cref="Sprite"/> with the given features.</returns>
            public Sprite this[DoorSpriteType doorSpriteType, AccentMaterial material, bool isFullWall] => _sprites[(int)doorSpriteType, (int)material, isFullWall ? 0 : 1];

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
                set => _sprites[i, j, k] = value;
            }
        }

        /// <summary>
        /// The <see cref="SpriteSheet2"/> class is a 2D array of <see cref="Sprite"/>s.
        /// </summary>
        public class SpriteSheet2
        {
            private readonly Sprite[,] _sprites;

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
            /// <param name="wallSpriteType">The <see cref="WallSpriteType"/> of the <see cref="Sprite"/>.</param>
            /// <param name="wallMaterial">The <see cref="WallMaterial"/> of the <see cref="Sprite"/>.</param>
            /// <returns>The <see cref="Sprite"/> with the given features.</returns>
            public Sprite this[WallSpriteType wallSpriteType, WallMaterial wallMaterial] => _sprites[(int)wallMaterial, (int)wallSpriteType];

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
                set => _sprites[j, i] = value;
            }
        }
    }
}