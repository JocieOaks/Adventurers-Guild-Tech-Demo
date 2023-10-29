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
    /// The <see cref="BedSprite"/> class is a <see cref="SpriteObject"/> for bed furniture.
    /// </summary>
    [System.Serializable]
    public class BedSprite : SpriteObject, IOccupied
    {
        // Initialized the first time GetMaskPixels is called, _pixels is the sprite mask for all Beds.
        private static bool[,] s_pixels;
        private static readonly Sprite[] s_sprites = { Graphics.Instance.BedSprite[1] };

        private List<RoomNode> _interactionPoints;



        /// <summary>
        /// Initializes a new instance of the <see cref="BedSprite"/> class.
        /// </summary>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="BedSprite"/>.</param>
        [JsonConstructor]
        public BedSprite(Vector3Int worldPosition) :
            base(5, s_sprites, Direction.Undirected, worldPosition, "Bed", ObjectDimensions, true)
        {
            LayDestination.AddLayingObject(this);
            SpriteRenderers[1].sprite = Graphics.Instance.BedSprite[0];
            SpriteRenderers[1].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + Vector3Int.up);

            for (int i = 2; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].sprite = Graphics.Instance.BedSprite[i];
                SpriteRenderers[i].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + Vector3Int.right * (i - 1));
            }

        }

        /// <value>The 3D dimensions of a <see cref="BedSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(4, 2, 1);

        /// <inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (s_pixels == default)
                {
                    BuildPixelArray(Graphics.Instance.BedSprite, ref s_pixels);
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
                    for (int i = -2; i < 6; i++)
                    {
                        for (int j = -2; j < 4; j++)
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
    
        /// <value>The <see cref="AdventurerPawn"/> that owns this <see cref="BedSprite"/>.</value>
        public AdventurerPawn Owner { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        protected override string ObjectType { get; } = "Bed";

        /// <summary>
        /// Checks if a new <see cref="BedSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true a <see cref="BedSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="BedSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="BedSprite"/>.</param>
        public static void CreateBed(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new BedSprite(position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="BedSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;
                highlight.flipX = false;
                highlight.sprite = Graphics.Instance.BedSprite[1];
                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            LayDestination.RemoveLayingObject(this);
            base.Destroy();
        }

        /// <inheritdoc/>
        public void Enter(Pawn pawn)
        {
            pawn.transform.Rotate(0, 0, -55);
            pawn.ForcePosition(WorldPosition + Vector3Int.up);
            pawn.Occupying = this;
            Occupant = pawn;
            LayDestination.RemoveLayingObject(this);
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
            pawn.transform.Rotate(0, 0, 55);
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
            LayDestination.AddLayingObject(this);
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
        protected override void OnMapChanging()
        {
            _interactionPoints = null;
            ReserveInteractionPoints();
        }

        /// <inheritdoc/>
        public void StartPlayerInteraction()
        {
            PlayerPawn.Instance.SetTask(new StanceLay(this));
        }

        /// <inheritdoc/>
        public void EndPlayerInteraction()
        {
            Exit(PlayerPawn.Instance);
        }
    }
}
