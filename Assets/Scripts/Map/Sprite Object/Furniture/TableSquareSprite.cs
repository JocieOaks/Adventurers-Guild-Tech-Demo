using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object.Furniture
{
    /// <summary>
    /// The <see cref="TableSquareSprite"/> class is a <see cref="SpriteObject"/> for square table furniture.
    /// </summary>
    [System.Serializable]
    public class TableSquareSprite : SpriteObject
    {
        // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all TableSquares.
        private static bool[,] s_pixels;
        private static readonly Sprite[] s_sprites = { Graphics.Instance.TableSquare[0] };

        /// <summary>
        /// Initializes a new instance of the <see cref="TableSquareSprite"/> class.
        /// </summary>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="TableSquareSprite"/>.</param>
        [JsonConstructor]
        public TableSquareSprite(Vector3Int worldPosition)
            : base(3, s_sprites, Direction.Undirected, worldPosition, "Square Table", ObjectDimensions, true)
        {
            SpriteRenderers[1].sprite = Graphics.Instance.TableSquare[1];
            SpriteRenderers[1].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + 2 * Vector3Int.up);
            SpriteRenderers[2].sprite = Graphics.Instance.TableSquare[2];
            SpriteRenderers[2].sortingOrder = Utility.Utility.GetSortOrder(WorldPosition + 2 * Vector3Int.right);
        }

        /// <value>The 3D dimensions of a <see cref="TableSquareSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions { get; } = new(3, 3, 2);

        /// <inheritdoc/>
        [JsonIgnore]
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (s_pixels == default)
                {
                    BuildPixelArray(Graphics.Instance.TableSquare, ref s_pixels);
                }

                yield return s_pixels;
            }
        }

        /// <inheritdoc/>
        [JsonProperty]
        protected override string ObjectType { get; } = "TableSquare";

        /// <summary>
        /// Checks if a new <see cref="TableSquareSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true a <see cref="TableSquareSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions);
        }

        /// <summary>
        /// Initializes a new <see cref="TableSquareSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="TableSquareSprite"/>.</param>
        public static void CreateTableSquare(Vector3Int position)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new TableSquareSprite(position);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="TableSquareSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            if (CheckObject(position))
            {
                highlight.enabled = true;
                highlight.sprite = Graphics.Instance.TableSquare[0];
                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        }
    }
}
