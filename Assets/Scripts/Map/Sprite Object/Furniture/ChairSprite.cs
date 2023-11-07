using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.AI.Step;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Map.Node;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object.Furniture
{
    /// <summary>
    /// The <see cref="ChairSprite"/> class is a <see cref="SpriteObject"/> for chair furniture.
    /// </summary>
    [System.Serializable]
    public class ChairSprite : SpriteObject, IOccupied, IDirected
    {
        private static readonly Sprite[] s_sprites = { Graphics.Instance.ChairNorth, Graphics.Instance.ChairEast, Graphics.Instance.ChairSouth, Graphics.Instance.ChairWest };

        // Initialized the first time GetMaskPixels is called for each given direction., _pixelsEast, _pixelsNorth, _pixelsSouth, and _pixelsWest are the sprite mask for all Chairs.
        private static bool[,] s_pixelsEast;
        private static bool[,] s_pixelsNorth;
        private static bool[,] s_pixelsSouth;
        private static bool[,] s_pixelsWest;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChairSprite"/> class.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> the <see cref="ChairSprite"/> is facing.</param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="ChairSprite"/>.</param>
        [JsonConstructor]
        public ChairSprite(Direction direction, Vector3Int worldPosition)
            : base(1,  s_sprites, direction, worldPosition, "Chair", ObjectDimensions, true)
        {
            Direction = direction;
            SitDestination.AddSittingObject(this);
        }

        /// <value>The 3D dimensions of a <see cref="ChairSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(1, 1, 2);

        /// <inheritdoc/>
        public Direction Direction { get; private set; }

        ///<inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                switch (Direction)
                {
                    case Direction.North:
                        if (s_pixelsNorth == default)
                        {
                            BuildPixelArray(Graphics.Instance.ChairNorth, ref s_pixelsNorth);
                        }
                        yield return s_pixelsNorth;
                        yield break;
                    case Direction.South:
                        if (s_pixelsSouth == default)
                        {
                            BuildPixelArray(Graphics.Instance.ChairSouth, ref s_pixelsSouth);
                        }
                        yield return s_pixelsSouth;
                        yield break;
                    case Direction.East:
                        if (s_pixelsEast == default)
                        {
                            BuildPixelArray(Graphics.Instance.ChairEast, ref s_pixelsEast);
                        }
                        yield return s_pixelsEast;
                        yield break;

                    default:
                        if (s_pixelsWest == default)
                        {
                            BuildPixelArray(Graphics.Instance.ChairWest, ref s_pixelsWest);
                        }
                        yield return s_pixelsWest;
                        yield break;
                }
            }
        }


        /// <inheritdoc/>
        [JsonIgnore]
        public IEnumerable<RoomNode> InteractionPoints
        {
            get
            {
                yield return Node;
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public Pawn Occupant { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public bool Occupied => Occupant != null;

        /// <inheritdoc/>
        [JsonProperty]
        protected override string ObjectType { get; } = "Chair";

        /// <summary>
        /// Checks if a new <see cref="ChairSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true a <see cref="ChairSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="ChairSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="ChairSprite"/>.</param>
        public static void CreateChair(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ChairSprite(BuildFunctions.Direction, position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="ChairSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;

                highlight.sprite = BuildFunctions.Direction switch
                {
                    Direction.North => Graphics.Instance.ChairNorth,
                    Direction.South => Graphics.Instance.ChairSouth,
                    Direction.East => Graphics.Instance.ChairEast,
                    Direction.West => Graphics.Instance.ChairWest,
                    _ => highlight.sprite
                };

                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            SitDestination.RemoveSittingObject(this);
            base.Destroy();
        }

        /// <inheritdoc/>
        public void EndPlayerInteraction()
        {
            Exit(PlayerPawn.Instance);
        }

        /// <inheritdoc/>
        public void Enter(Pawn pawn)
        {
            pawn.ForcePosition(WorldPosition);
            pawn.Occupying = this;
            Occupant = pawn;
            SitDestination.RemoveSittingObject(this);
        }

        /// <inheritdoc/>
        public void Exit(Pawn pawn, Vector3Int exitTo = default)
        {
            if (pawn == Occupant)
            {
                Occupant = null;
            }
            if (pawn.Occupying == this)
            {
                pawn.Occupying = null;
            }
            if (exitTo != default)
            {
                pawn.ForcePosition(exitTo);
            }
            else
            {
                RoomNode roomNode = InteractionPoints.FirstOrDefault(x => x.Traversable);
                if (roomNode == default)
                {
                    //Emergency option if there's no interaction points to move to.
                    pawn.ForcePosition(Vector3Int.one);
                }
                else
                {
                    pawn.ForcePosition(roomNode);
                }
            }
            SitDestination.AddSittingObject(this);
        }

        /// <inheritdoc/>
        public void ReserveInteractionPoints()
        {
            foreach (RoomNode roomNode in InteractionPoints)
            {
                roomNode.Reserved = true;
            }
        }

        /// <inheritdoc/>
        public override float SpeedMultiplier(Vector3Int nodePosition)
        {
            if (nodePosition == WorldPosition)
                return 0.5f;
            else return 1;
        }

        /// <inheritdoc/>
        public void StartPlayerInteraction()
        {
            PlayerPawn.Instance.SetTask(new StanceSit(this));
        }

        /// <inheritdoc/>
        protected override void OnMapChanging()
        {
            ReserveInteractionPoints();
        }
    }
}
