using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Goal;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Map.Node;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object.Furniture
{
    /// <summary>
    /// The <see cref="StoolSprite"/> class is a <see cref="SpriteObject"/> for stool furniture.
    /// </summary>
    [System.Serializable]
    public class StoolSprite : SpriteObject, IOccupied
    {
        private static readonly Sprite[] s_sprites = { Graphics.Instance.Stool };

        // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all Stools.
        private static bool[,] s_pixels;
        private List<RoomNode> _interactionPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoolSprite"/> class.
        /// </summary>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="StoolSprite"/>.</param>
        [JsonConstructor]
        public StoolSprite(Vector3Int worldPosition)
            : base(1, s_sprites, Direction.Undirected, worldPosition, "Stool", ObjectDimensions, true)
        {
            SitDestination.AddSittingObject(this);
        }

        /// <value>The 3D dimensions of a <see cref="StoolSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(1, 1, 2);

        /// <inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (s_pixels == default)
                {
                    BuildPixelArray(Graphics.Instance.Stool, ref s_pixels);
                }

                yield return s_pixels;
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
                    for (int i = -2; i < 2; i++)
                    {
                        for (int j = -2; j < 2; j++)
                        {
                            RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                            if (roomNode.Traversable)
                                _interactionPoints.Add(roomNode);
                        }
                    }
                }


                return _interactionPoints;
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
        protected override string ObjectType { get; } = "Stool";

        /// <summary>
        /// Checks if a new <see cref="StoolSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true a <see cref="StoolSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="StoolSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="StoolSprite"/>.</param>
        public static void CreateStool(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new StoolSprite(position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="StoolSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;
                highlight.sprite = Graphics.Instance.Stool;
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
            pawn.ForcePosition(WorldPosition + Vector3Int.back);
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
                //Emergency option if there's no interaction points to move to.
                pawn.ForcePosition(roomNode?.WorldPosition ?? Vector3Int.one);
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
            _interactionPoints = null;
            ReserveInteractionPoints();
        }
    }
}
