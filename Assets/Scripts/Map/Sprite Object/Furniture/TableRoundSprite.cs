using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object.Furniture
{
    /// <summary>
    /// The <see cref="TableRoundSprite"/> class is a <see cref="SpriteObject"/> for round table furniture.
    /// </summary>
    [System.Serializable]
    public class TableRoundSprite : SpriteObject
    {
        private static readonly Sprite[] s_sprites = new Sprite[] { Graphics.Instance.TableRound[0] };

        // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all TableRounds.
        private static bool[,] s_pixels;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableRoundSprite"/> class.
        /// </summary>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="TableRoundSprite"/>.</param>
        [JsonConstructor]
        public TableRoundSprite(Vector3Int worldPosition)
            : base(3, s_sprites, Direction.Undirected, worldPosition, "Round Table", new Vector3Int(1, 1, 2), true)
        {
            SpriteRenderers[1].sprite = Graphics.Instance.TableRound[1];
            SpriteRenderers[1].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + Vector3Int.up);
            SpriteRenderers[2].sprite = Graphics.Instance.TableRound[2];
            SpriteRenderers[2].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + Vector3Int.right);
        }

        /// <value>The 3D dimensions of a <see cref="TableRoundSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(3, 3, 2);

        /// <value>he 3D dimensions of the <see cref="SpriteObject"/> in terms of <see cref="Map"/> coordinates. 
        /// Normally should be equivalent to <see cref="ObjectDimensions"/> but can be publicly accessed without knowing the <see cref="SpriteObject"/>'s type.</value>
        [JsonIgnore]
        public override Vector3Int Dimensions => ObjectDimensions;

        /// <inheritdoc/>
        public override Vector3Int NearestCornerPosition => WorldPosition;

        /// <inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (s_pixels == default)
                {
                    BuildPixelArray(Graphics.Instance.TableRound, ref s_pixels);
                }

                yield return s_pixels;
            }
        }

        /// <inheritdoc/>
        [JsonProperty]
        protected override string ObjectType { get; } = "TableRound";

        /// <summary>
        /// Checks if a new <see cref="TableRoundSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true a <see cref="TableRoundSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="TableRoundSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="TableRoundSprite"/>.</param>
        public static void CreateTableRound(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new TableRoundSprite(position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="TableRoundSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;
                highlight.sprite = Graphics.Instance.TableRound[0];
                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        }

        /// <inheritdoc/>
        public override float SpeedMultiplier(Vector3Int nodePosition)
        {
            Vector3Int vector = nodePosition - WorldPosition;
            if (vector == Vector3Int.one)
                return 0f;
            else if (
                vector.x <= 0 && vector.x < Dimensions.x &&
                vector.y <= 0 && vector.y < Dimensions.y &&
                vector.z <= 0 && vector.z < Dimensions.z
            )
                return 0;
            else return 1;
        }
    }
}
