using System.Collections.Generic;
using Assets.Scripts.AI.Navigation.Goal;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Node;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object.Furniture
{
    /// <summary>
    /// The <see cref="BarSprite"/> class is a <see cref="SpriteObject"/> for bar furniture.
    /// </summary>
    [System.Serializable]
    public class BarSprite : LinearSpriteObject, IInteractable, IDirected
    {

        // Initialized the first time GetMaskPixels is called, _pixelsX and _pixelsY are the sprite mask for all Bars.
        private static bool[,] s_pixelsX;
        private static bool[,] s_pixelsY;
        private static readonly Sprite[] s_sprites = { Graphics.Instance.BarX[0], Graphics.Instance.BarY[0], Graphics.Instance.BarX[0], Graphics.Instance.BarY[0] };

        private List<RoomNode> _interactionPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarSprite"/> class.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> the <see cref="BarSprite"/> is facing.</param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="BarSprite"/>.</param>
        [JsonConstructor]
        public BarSprite(Direction direction, Vector3Int worldPosition)
            : base(2, s_sprites, direction, worldPosition, "Bar", ObjectDimensions, true)
        {
            FoodGoal.AddFoodSource(this);
            Direction = direction;
            SpriteRenderers[1].sprite = Alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[1] : Graphics.Instance.BarY[1];
            SpriteRenderers[1].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + (Alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left));
        }

        /// <value>The 3D dimensions of a <see cref="BarSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(1, 2, 2);

        /// <inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (Alignment == MapAlignment.XEdge)
                {
                    if (s_pixelsX == default)
                    {
                        BuildPixelArray(Graphics.Instance.BarX, ref s_pixelsX);
                    }

                    yield return s_pixelsX;
                }
                else
                {
                    if (s_pixelsY == default)
                    {
                        BuildPixelArray(Graphics.Instance.BarY, ref s_pixelsY);
                    }

                    yield return s_pixelsY;
                }
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public IEnumerable<RoomNode> InteractionPoints
        {
            get
            {
                if (_interactionPoints == null)
                {
                    _interactionPoints = new List<RoomNode>();

                    if (Alignment == MapAlignment.XEdge)
                    {
                        int i = 0;
                        while (Map.Instance[WorldPosition + Vector3Int.right * i].Occupant is BarSprite)
                        {
                            RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.right * i + 2 * Vector3Int.down];
                            if (roomNode.Traversable)
                                _interactionPoints.Add(roomNode);
                            i++;
                        }
                        i = 1;
                        while (Map.Instance[WorldPosition + Vector3Int.left * i].Occupant is BarSprite)
                        {
                            RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.left * i + 2 * Vector3Int.down];
                            if (roomNode.Traversable)
                                _interactionPoints.Add(roomNode);
                            i++;
                        }
                    }
                    else
                    {
                        int i = 0;
                        while (Map.Instance[WorldPosition + Vector3Int.up * i].Occupant is BarSprite)
                        {
                            RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.up * i + 2 * Vector3Int.left];
                            if (roomNode.Traversable)
                                _interactionPoints.Add(roomNode);
                            i++;
                        }
                        i = 1;
                        while (Map.Instance[WorldPosition + Vector3Int.down * i].Occupant is BarSprite)
                        {
                            RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.down * i + 2 * Vector3Int.left];
                            if (roomNode.Traversable)
                                _interactionPoints.Add(roomNode);
                            i++;
                        }
                    }
                }

                return _interactionPoints;
            }
        }

        [JsonProperty]
        public Direction Direction { get; }

        /// <inheritdoc/>
        [JsonProperty]
        protected override string ObjectType { get; } = "Bar";

        /// <summary>
        /// Checks if a new <see cref="BarSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true if a <see cref="BarSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            Vector3Int dimensions = default;
            switch (BuildFunctions.Direction)
            {
                case Direction.North:
                case Direction.South:
                    dimensions = ObjectDimensions;
                    break;

                case Direction.East:
                case Direction.West:
                    dimensions = new Vector3Int(ObjectDimensions.y, ObjectDimensions.x, ObjectDimensions.z);
                    break;
            }
            return Map.Instance.CanPlaceObject(position, dimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="BarSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="BarSprite"/>.</param>
        public static void CreateBar(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new BarSprite(BuildFunctions.Direction, position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="BarSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>///
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;

                highlight.sprite = Utility.Utility.DirectionToEdgeAlignment(BuildFunctions.Direction) == MapAlignment.XEdge ? Graphics.Instance.BarX[0] : Graphics.Instance.BarY[0];

                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            FoodGoal.RemoveFoodSource(this);
            base.Destroy();
        }

        /// <inheritdoc/>
        public void ReserveInteractionPoints()
        { }

        /// <inheritdoc/>
        protected override void OnMapChanging()
        {
            _interactionPoints = null;
        }
    }
}