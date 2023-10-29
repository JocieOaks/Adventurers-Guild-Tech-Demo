using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI;
using Assets.Scripts.Map.Node;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// Enum referring to the <see cref="Sprite"/> to be used for a portion of a door.
    /// </summary>
    public enum DoorSpriteType
    {
        DoorXLeft = 0,
        DoorXMid = 1,
        DoorXRight = 2,
        DoorYRight = 3,
        DoorYMid = 4,
        DoorYLeft = 5
    }

    /// <summary>
    /// Enum referring to the <see cref="Sprite"/> to be used for a portion of a <see cref="WallSprite"/>.
    /// </summary>
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

    /// <summary>
    /// The <see cref="WallSprite"/> class is the <see cref="SpriteObject"/> that corresponds to <see cref="WallBlocker"/>.
    /// </summary>
    public class WallSprite : LinearSpriteObject
    {
        private static readonly Vector2[] s_colliderXBase = {
            new(2f/3, 13.5f/6),
            new(-1.5f, 7f/6),
            new(-1.5f, -1),
            new(-2f/3, -8.5f/6),
            new(8f/6, -2.5f/6),
            new(8f/6, 11.5f/6)
        };

        private static readonly Vector2[] s_colliderXFull = {
            new(2f/3, 13.5f/6 + 12 * 5f/6),
            new(-1.5f, 7f/6 + 12 * 5f/6),
            new(-1.5f, -1),
            new(-2f/3, -8.5f/6),
            new(8f/6, -2.5f/6),
            new(8f/6, 11.5f/6 + 12 * 5f/6)
        };

        private static readonly Vector2[] s_colliderYBase = {
            new(-8f/6, 11.5f/6),
            new(-8f/6, -2.5f/6),
            new(2f/3, -8.5f/6),
            new(1.5f, -1),
            new(1.5f, 7f/6),
            new(-2f/3, 13.5f/6)
        };

        private static readonly Vector2[] s_colliderYFull = {
            new(-8f/6, 11.5f/6 + 12 * 5f/6),
            new(-8f/6, -2.5f/6),
            new(2f/3, -8.5f/6),
            new(1.5f, -1),
            new(1.5f, 7f/6 + 12 * 5f/6),
            new(-2f/3, 13.5f/6 + 12 * 5f/6)
        };

        private static readonly WallMaterial[] s_dontInverseShift = { WallMaterial.StoneBrick };
        private static bool[,] s_pixelsX;
        private static bool[,] s_pixelsY;

        private readonly int _height;
        private readonly SortingGroup _sortingGroup;
        private readonly WallMaterial _wallMaterial;
        private bool[,] _baseDoorMask;
        private SpriteMask _cornerMask;
        private SpriteMask _doorMask;
        private AccentMaterial _doorMaterial;
        private SpriteRenderer _doorSprite;
        private DoorSpriteType _doorSpriteType;
        private bool[,] _fullDoorMask;
        private bool _highlightDoor;

        private bool _isFullWall;

        private WallSprite _nextDoorWall;
        private WallBlocker _wall;

        /// <summary>
        /// Initializes a new instance of <see cref="WallSprite"/> that does not have a corresponding <see cref="WallBlocker"/>.
        /// </summary>
        /// <param name="position">The position of the world sprite in the <see cref="Map"/> coordinate system.</param>
        /// <param name="alignment">Vertical or horizontal alignment of the wall.</param>
        /// <param name="height">Height of the wall.</param>
        /// <param name="wallMaterial"><see cref="Scripts.WallMaterial"/> of the wall.</param>
        public WallSprite(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial) :
            base(height, new[] { Graphics.WallSprites[GetSpriteType(position, 0, alignment, wallMaterial), wallMaterial], Graphics.WallSprites[GetSpriteType(position, 0, alignment, wallMaterial), wallMaterial] }, alignment == MapAlignment.XEdge ? Direction.North : Direction.East, position, "Wall", new Vector3Int(alignment == MapAlignment.XEdge ? 1 : 0, alignment == MapAlignment.YEdge ? 1 : 0, height), false)
        {
            _wallMaterial = wallMaterial;

            _height = height;


            Transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position, alignment);


            _sortingGroup = GameObject.AddComponent<SortingGroup>();
            _sortingGroup.sortingOrder = Utility.Utility.GetSortOrder(position) + 1;
            SpriteRenderer.color = Graphics.Instance.HighlightColor;
            SpriteRenderer.sortingLayerName = "Wall";


            for (int i = 1; i < _height; i++)
            {
                SpriteRenderer current = SpriteRenderers[i];

                current.transform.localPosition = Vector3Int.up * i * 2;
                current.name = "Wall";
                current.sortingOrder = i;
                current.sortingLayerName = "Wall";
                current.sprite = Graphics.WallSprites[GetSpriteType(position, i, alignment, _wallMaterial), wallMaterial];
                current.color = Graphics.Instance.HighlightColor;
            }

            Graphics.UpdatingGraphics += WhenUpdatingGraphics;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="WallSprite"/> that already has a corresponding <see cref="WallBlocker"/>.
        /// </summary>
        /// <param name="position">The position of the world sprite in the <see cref="Map"/> coordinate system.</param>
        /// <param name="alignment">Vertical or horizontal alignment of the wall.</param>
        /// <param name="height">Height of the wall.</param>
        /// <param name="wallMaterial"><see cref="Scripts.WallMaterial"/> of the wall.</param>
        /// <param name="wall">The <see cref="WallBlocker"/> that this <see cref="WallSprite"/> corresponds to.</param>
        public WallSprite(Vector3Int position, MapAlignment alignment, int height, WallMaterial wallMaterial, WallBlocker wall) : this(position, alignment, height, wallMaterial)
        {
            _wall = wall;
            Confirm();
        }

        /// <value>The <see cref="AccentMaterial"/> for a door, if the <see cref="WallSprite"/> has one.</value>
        public static AccentMaterial DoorMaterial { get; private set; } = AccentMaterial.Stone;

        /// <value>The height of the <see cref="WallSprite"/>.</value>
        public static int WallHeight { get; private set; } = 6;

        /// <value>The <see cref="Scripts.WallMaterial"/> the <see cref="WallSprite"/> is made of, determining its <see cref="Sprite"/>s.</value>
        public static WallMaterial WallMaterial { get; private set; } = WallMaterial.Brick;

        /// <inheritdoc/>
        public override Vector3Int Dimensions => IsDoor && (_doorSpriteType == DoorSpriteType.DoorXMid || _doorSpriteType == DoorSpriteType.DoorYMid) ? base.Dimensions - new Vector3Int(0, 05) : base.Dimensions;

        /// <inheritdoc/>
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (IsDoor)
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
                else if (Alignment == MapAlignment.XEdge)
                {
                    for (int i = 0; i < (_isFullWall ? _height : 1); i++)
                        yield return PixelsX;
                }
                else
                {
                    for (int i = 0; i < (_isFullWall ? _height : 1); i++)
                        yield return PixelsY;
                }
            }
        }

        /// <value>Returns the position in <see cref="Map"/> coordinates and <see cref="MapAlignment"/> of the <see cref="WallSprite"/>.</value>
        public (Vector3Int, MapAlignment) GetPosition => (WorldPosition, Alignment);

        /// <value>True if the <see cref="WallSprite"/> is part of a doorway.</value>
        public bool IsDoor => _doorSprite != null && _doorSprite.enabled && !_highlightDoor;

        /// <value>Represents whether in the current viewing mode, all of the <see cref="WallSprite"/>'s <see cref="Sprite"/>s should be visible or just the base.</value>
        public bool IsFullWall
        {
            get => _isFullWall;
            private set
            {
                _isFullWall = value;
                if (_isFullWall)
                {
                    for (int i = 1; i < _height; i++)
                        SpriteRenderers[i].enabled = true;

                    if (_cornerMask != null)
                        _cornerMask.transform.localPosition = (_height - 1) * 2 * Vector3.up;
                }
                else
                {
                    for (int i = 1; i < _height; i++)
                        SpriteRenderers[i].enabled = false;

                    if (_cornerMask != null)
                        _cornerMask.transform.localPosition = Vector3.zero;
                }

                if (IsDoor)
                {
                    DoorSprite.sprite = Graphics.DoorSprites[_doorSpriteType, _doorMaterial, _isFullWall];
                }
            }
        }

        /// <inheritdoc/>
        public override Vector3Int NearestCornerPosition => IsDoor && (_doorSpriteType == DoorSpriteType.DoorXMid || _doorSpriteType == DoorSpriteType.DoorYMid)? WorldPosition + new Vector3Int(0,0,5) : WorldPosition;
        /// <inheritdoc/>
        public override Vector3 OffsetVector => 2 * Vector3.up;

        /// <value>Initializes the sprite mask pixels for a <see cref="WallSprite"/> in the <see cref="MapAlignment.XEdge"/> alignment if they are not already initialized, and returns them.</value>
        private static bool[,] PixelsX
        {
            get
            {
                if (s_pixelsX == null)
                    BuildPixelArray(Graphics.WallSprites[0, WallMaterial.Brick], ref s_pixelsX);
                return s_pixelsX;
            }
        }

        /// <value>Initializes the sprite mask pixels for a <see cref="WallSprite"/> in the <see cref="MapAlignment.YEdge"/> alignment if they are not already initialized, and returns them.</value>
        private static bool[,] PixelsY
        {
            get
            {
                if (s_pixelsY == null)
                    BuildPixelArray(Graphics.WallSprites[6, WallMaterial.Brick], ref s_pixelsY);
                return s_pixelsY;
            }
        }

        /// <value>The <see cref="SpriteRenderer"/> for a door that the <see cref="WallSprite"/> is a part of.</value>
        private SpriteRenderer DoorSprite
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

        /// <summary>
        /// Checks if a new <see cref="DoorConnector"/> can be placed at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> to check.</param>
        /// <returns>Returns true if a <see cref="DoorConnector"/> can be created at <c>position</c>.</returns>
        public static bool CheckDoor(Vector3Int position, MapAlignment alignment)
        {
            return Map.Instance.CanPlaceDoor(position, alignment);
        }

        /// <summary>
        /// Checks if a new <see cref="WallSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true if a <see cref="WallSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceWall(position, Utility.Utility.DirectionToEdgeAlignment(BuildFunctions.Direction));
        }

        /// <summary>
        /// Initializes a new <see cref="DoorConnector"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="DoorConnector"/>.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> to create the new <see cref="DoorConnector"/>.</param>
        public static void CreateDoor(Vector3Int position, MapAlignment alignment)
        {
            Map.Instance.GetWall(alignment, position).WallSprite.AddDoor(3, DoorMaterial);
        }

        /// <summary>
        /// Initializes a new <see cref="WallSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="WallSprite"/>.</param>
        public static void CreateWall(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new WallSprite(position, Utility.Utility.DirectionToEdgeAlignment(BuildFunctions.Direction), WallHeight, WallMaterial);
        }

        /// <summary>
        /// Creates a highlight of a door on the given <see cref="WallSprite"/>.
        /// </summary>
        /// <param name="wall">The <see cref="WallSprite"/> on which the door is centered.</param>
        public static void PlaceDoorHighlight(WallSprite wall)
        {
            Graphics.Instance.ResetSprite();
            (Vector3Int position, MapAlignment alignment) = wall.GetPosition;
            if (Map.Instance.CanPlaceDoor(position, alignment))
            {
                wall.AddDoor(3, DoorMaterial, true);
            }
        }

        /// <summary>
        /// Places a highlight object with a <see cref="WallSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>///
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                MapAlignment alignment = Utility.Utility.DirectionToEdgeAlignment(BuildFunctions.Direction);
                highlight.enabled = true;
                highlight.sprite = Graphics.WallSprites[alignment == MapAlignment.XEdge ? WallSpriteType.X11 : WallSpriteType.Y11, WallMaterial];
                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position, alignment);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position) + 1;
            }
            else
                highlight.enabled = false;
        }

        /// <summary>
        /// Adds a door sprite overlaying the wall at the given door position.
        /// </summary>
        /// <param name="width">The number of <see cref="WallSprite"/>s over which the door covers.</param>
        /// <param name="material">The <see cref="AccentMaterial"/> that the door should be made of.</param>
        /// <param name="highlight">Determines if the door is there permanently, or is just a temporary highlight. Defaults to false.</param>
        /// <exception cref="System.ArgumentException">Throws exception if the door position does not overlap with the wall.</exception>
        public void AddDoor(int width, AccentMaterial material, bool highlight = false)
        {
            int start = -width / 2;
            int end = width + start - 1;

            if (Alignment == MapAlignment.XEdge)
            {
                WallSprite prev = Map.Instance.GetWall(Alignment, WorldPosition + end * Vector3Int.right).WallSprite;
                for (int i = start; i <= end; i++)
                {
                    WallSprite current = Map.Instance.GetWall(Alignment, WorldPosition + i * Vector3Int.right).WallSprite;
                    current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorXLeft : i == end ? DoorSpriteType.DoorXRight : DoorSpriteType.DoorXMid, material, new Vector3(-2, -1) * i, highlight);
                    prev = current;
                }
            }
            else
            {
                WallSprite prev = Map.Instance.GetWall(Alignment, WorldPosition + end * Vector3Int.up).WallSprite;
                for (int i = start; i <= end; i++)
                {
                    WallSprite current = Map.Instance.GetWall(Alignment, WorldPosition + i * Vector3Int.up).WallSprite;
                    current.AddDoorSprite(prev, i == start ? DoorSpriteType.DoorYRight : i == end ? DoorSpriteType.DoorYLeft : DoorSpriteType.DoorYMid, material, new Vector3(2, -1) * i, highlight);
                    prev = current;
                }
            }
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            Graphics.UpdatingGraphics -= WhenUpdatingGraphics;
            Graphics.ResettingSprite -= WhenResettingSprite;
            Graphics.LevelChanged -= WhenLevelChanged;

            Graphics.Instance.CornerQueue.Enqueue(WorldPosition + (Alignment == MapAlignment.XEdge ? Vector3Int.right : Vector3Int.up));

            Graphics.Instance.CornerQueue.Enqueue(WorldPosition);

            Object.Destroy(GameObject);

            if(_wall != null)
                Map.Instance.RemoveWall(_wall);
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
                    nextDoor = nextDoor._nextDoorWall;
                    nextDoor.HighlightDoor(color);
                } while (nextDoor != this);
            }
            else
            {
                base.Highlight(color);
            }
        }

        /// <summary>
        /// Enable or disable the <see cref="SpriteMask"/> for the corner intersection between two <see cref="WallSprite"/>s.
        /// </summary>
        /// <param name="masking">Determines whether to enable or disable the <see cref="SpriteMask"/>.</param>
        public void MaskCorner(int masking)
        {
        

            if (_cornerMask == null)
            {
                if (masking == 0)
                    return;
                else if (masking == -1)
                {
                    _sortingGroup.sortingOrder = Utility.Utility.GetSortOrder(WorldPosition);
                    return;
                }

                _cornerMask = Object.Instantiate(Alignment == MapAlignment.XEdge ? Graphics.Instance.CornerMaskX : Graphics.Instance.CornerMaskY, Transform);

                if (IsFullWall)
                    _cornerMask.transform.localPosition = (_height - 1) * 2 * Vector3.up;
            }

            _cornerMask.enabled = masking == 1;
            _sortingGroup.sortingOrder = Utility.Utility.GetSortOrder(WorldPosition) + (masking >= 0 ? 1 : 0);
        }

        /// <summary>
        /// Removes the door for this <see cref="WallSprite"/>, and all the other <see cref="WallSprite"/>s the door occupies.
        /// </summary>
        public void RemoveDoor()
        {
            WallSprite nextDoor = this;
            do
            {
                WallSprite current = nextDoor;
                nextDoor = current._nextDoorWall;
                current.RemoveDoorSprite();
            } while (nextDoor != this);

            Map.Instance.RemoveDoor(WorldPosition, Alignment);
        }

        /// <summary>
        /// Called when the <see cref="WallSprite"/> is at a corner with another <see cref="WallSprite"/> that is a full wall,
        /// and <see cref="Graphics.Mode"/> is <see cref="WallDisplayMode.Open"/>.
        /// </summary>
        public void SetEdge()
        {
            IsFullWall = true;
        }

        /// <summary>
        /// Called when the created <see cref="LinearSpriteObject"/>s are confirmed.
        /// Creates a new <see cref="WallBlocker"/> at the given <see cref="Map"/> position if one is not already present.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected sealed override void Confirm()
        {
            _wall ??= new WallBlocker(this, WorldPosition, Alignment);

            for (int i = 0; i < _height; i++)
            {
                SpriteRenderers[i].color = Color.white;
            }

            Graphics.Instance.CornerQueue.Enqueue(WorldPosition + (Alignment == MapAlignment.XEdge ? Vector3Int.right : Vector3Int.up));

            Graphics.Instance.CornerQueue.Enqueue(WorldPosition);

            base.Confirm();
        }

        /// <inheritdoc/>
        protected override void WhenLevelChanged(object sender, EventArgs eventArgs)
        {
            int level = GameManager.Instance.IsOnLevel(WorldPosition.z);
            if (level > 0)
                for (int i = 0; i < _height; i++)
                {
                    SpriteRenderers[i].enabled = false;
                    if (_doorSprite != null)
                        _doorSprite.enabled = false;
                }
            else
            {
                for (int i = 0; i < _height; i++)
                    SpriteRenderers[i].enabled = true;

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
                        (Alignment == MapAlignment.XEdge ? s_colliderXFull : s_colliderYFull) :
                        (Alignment == MapAlignment.XEdge ? s_colliderXBase : s_colliderYBase));
                    Collider.enabled = true;
                }
            }
        }

        /// <inheritdoc/>
        protected override void ResetSprite()
        {
            for (int i = 0; i < _height; i++)
            {
                SpriteRenderers[i].sprite = Graphics.WallSprites[GetSpriteType(WorldPosition, i, Alignment, _wallMaterial), _wallMaterial];
                SpriteRenderers[i].color = Color.white;
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

            Graphics.ResettingSprite -= WhenResettingSprite;
        }

        /// <summary>
        /// Get the index for a <see cref="WallSprite"/>'s <see cref="Sprite"/>
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the <see cref="WallSprite"/>.</param>
        /// <param name="height">The height of the <see cref="Sprite"/> on this <see cref="WallSprite"/></param>
        /// <param name="alignment">The alignment of the <see cref="WallSprite"/>.</param>
        /// <param name="material">The <see cref="WallMaterial"/> of the <see cref="WallSprite"/>.</param>
        /// <returns>Returns the index of the <see cref="Sprite"/> in a <see cref="WallSprite"/> at a given location.</returns>
        private static int GetSpriteType(Vector3Int position, int height, MapAlignment alignment, WallMaterial material)
        {
            int mod = alignment == MapAlignment.XEdge ? position.x % 3 : position.y % 3;
            //Inverse shift makes the wall pattern more random by shifting the bands in opposite directions, but some walls require the bands to remain aligned.
            int shift;
            if (s_dontInverseShift.FirstOrDefault(x => x == material) == default)
                shift = (height / 2) * (2 * (height % 2) - 1) % 3;
            else
                shift = (height / 2) % 3;
            int offset = (alignment == MapAlignment.YEdge ? 6 : 0) + 3 * (height % 2);
            if (shift < 0)
                shift += 3;

            return (mod + shift) % 3 + offset;
        }

        /// <summary>
        /// Adds a <see cref="Sprite"/> for a door set in this <see cref="WallSprite"/>.
        /// </summary>
        /// <param name="nextDoorWall">Reference to another <see cref="WallSprite"/> that is part of this door. 
        /// Used to be able to access all the <see cref="WallSprite"/>s that are part of this door, from any individual <see cref="WallSprite"/>.</param>
        /// <param name="doorSpriteType">The <see cref="DoorSpriteType"/> for this <see cref="WallSprite"/>.</param>
        /// <param name="material">The <see cref="AccentMaterial"/> for the door.</param>
        /// <param name="maskPosition">The position of the doors <see cref="SpriteMask"/> relative to <see cref="Transform"/> in scene coordinates.</param>
        /// <param name="highlight">Determines if the door is there permanently, or is just a temporary highlight.</param>
        private void AddDoorSprite(WallSprite nextDoorWall, DoorSpriteType doorSpriteType, AccentMaterial material, Vector3 maskPosition, bool highlight)
        {
            DoorSprite.enabled = true;

            _nextDoorWall = nextDoorWall;

            _doorSpriteType = doorSpriteType;
            _doorMaterial = material;
            _doorSprite.sprite = Graphics.DoorSprites[doorSpriteType, material, IsFullWall];

            _doorMask = Object.Instantiate(Alignment == MapAlignment.XEdge ? Graphics.Instance.DoorMaskX : Graphics.Instance.DoorMaskY, Transform);
            _doorMask.transform.localPosition = maskPosition;

            if (highlight)
            {
                DoorSprite.color = Graphics.Instance.HighlightColor;
                Graphics.ResettingSprite += WhenResettingSprite;
                _highlightDoor = true;
            }
            else
            {
                DoorSprite.color = Color.white;
                _highlightDoor = false;
            }

            MakeDoorMask();
        }

        /// <summary>
        /// Highlights the door on this <see cref="WallSprite"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> of the highlight.</param>
        private void HighlightDoor(Color color)
        {
            _doorSprite.color = color;
            Graphics.ResettingSprite += WhenResettingSprite;
        }

        /// <summary>
        /// Creates the pixel array for a <see cref="AdventurerPawn"/>'s <see cref="SpriteMask"/> when this wall is a door.
        /// </summary>
        private void MakeDoorMask()
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
                                _baseDoorMask![i,j] = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Destroys the <see cref="SpriteRenderer"/> for a door on this <see cref="WallSprite"/>.
        /// </summary>
        private void RemoveDoorSprite()
        {
            Object.Destroy(_doorSprite);
            Object.Destroy(_doorMask.gameObject);
            _doorSprite = null;
            _doorMask = null;

            _nextDoorWall = null;
        }

        private void WhenUpdatingGraphics(object sender, EventArgs eventArgs)
        {
            SetWallMode();
        }

        /// <summary>
        /// Determines if the <see cref="WallSprite"/> is a full wall, based on the <see cref="Graphics.Mode"/>.
        /// </summary>
        private void SetWallMode()
        {
            if (GameManager.Instance.IsOnLevel(WorldPosition.z) == 0)
            {
                if (Graphics.Instance.Mode == WallDisplayMode.Open)
                {
                    if (Map.Instance[WorldPosition] == null || Map.Instance[WorldPosition].Room is Layer)
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
                    IsFullWall = Graphics.Instance.Mode == WallDisplayMode.Full;
                }

                Collider.enabled = false;
                Collider.SetPath(0, _isFullWall ?
                    (Alignment == MapAlignment.XEdge ? s_colliderXFull : s_colliderYFull) :
                    (Alignment == MapAlignment.XEdge ? s_colliderXBase : s_colliderYBase));
                Collider.enabled = true;
            }
        }
    }
}