using System.Collections.Generic;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Node;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="StairSprite"/> class is the <see cref="SpriteObject"/> that corresponds with <see cref="StairNode"/>.
    /// </summary>
    public class StairSprite : AreaSpriteObject, IDirected
    {
        // Initialized the first time GetMaskPixels is called for each given direction, _pixelsCube, _pixelsEast, _pixelsNorth, _pixelsSouth, and _pixelsWest are the sprite mask for all Stairs.
        private static bool[,] s_pixelsCube;
        private static bool[,] s_pixelsEast;
        private static bool[,] s_pixelsNorth;
        private static bool[,] s_pixelsSouth;
        private static bool[,] s_pixelsWest;
        private static readonly Sprite[] s_sprites = new Sprite[] { Graphics.Instance.StairsNorth, Graphics.Instance.StairsEast, Graphics.Instance.StairsSouth, Graphics.Instance.StairsWest };
        private StairNode _stair;

        /// <summary>
        /// Initializes a new instance of the <see cref="StairSprite"/> class.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> the <see cref="StairSprite"/> is facing.</param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="StairSprite"/>.</param>
        /// <param name="z">The elevation relative to the <see cref="Room"/> the <see cref="StairSprite"/> is in.</param>
        public StairSprite(Direction direction, Vector3Int worldPosition, int z) : base(z + 1, s_sprites, direction, worldPosition, "Stair", new Vector3Int(1,1,z), false)
        {
            Direction = direction;

            SpriteRenderer.sortingOrder = 0;
            SpriteRenderer.enabled = GameManager.Instance.IsOnLevel(WorldPosition.z) <= 0;

            var sortingGroup = GameObject.AddComponent<SortingGroup>();
            sortingGroup.sortingOrder = Utility.Utility.GetSortOrder(worldPosition + z * Vector3Int.back);

            for (int i = 1; i < z + 1; i++)
            {
                SpriteRenderer current = SpriteRenderers[i];

                current.transform.localPosition = Vector3Int.down * i * 2;
                current.name = "Stair";
                current.sortingOrder = -i;
                current.sprite = Graphics.Instance.Cube;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StairSprite"/> class for a <see cref="StairNode"/> that has already been initialized.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> the <see cref="StairSprite"/> is facing.</param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="StairSprite"/>.</param>
        /// <param name="z">The elevation relative to the <see cref="Room"/> the <see cref="StairSprite"/> is in.</param>
        /// <param name="stair">The <see cref="StairNode"/> this <see cref="StairSprite"/> corresponds to.</param>
        public StairSprite(Direction direction, Vector3Int worldPosition, int z, StairNode stair) : this(direction, worldPosition, z)
        {
            _stair = stair;
            OnConfirmingObjects();
        }

        /// <value>The 3D dimensions of a <see cref="StairSprite"/> in terms of <see cref="Map"/> coordinates.</value>
        public new static Vector3Int ObjectDimensions => Vector3Int.one;

        /// <inheritdoc/>
        public Direction Direction { get; }

        /// <inheritdoc/>
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                switch (Direction)
                {
                    case Direction.North:

                        if (s_pixelsNorth == null)
                        {
                            BuildPixelArray(Graphics.Instance.StairsNorth, ref s_pixelsNorth);
                        }

                        yield return s_pixelsNorth;
                        break;
                    case Direction.South:

                        if (s_pixelsSouth == null)
                        {
                            BuildPixelArray(Graphics.Instance.StairsSouth, ref s_pixelsSouth);
                        }

                        yield return s_pixelsSouth;
                        break;
                    case Direction.East:

                        if (s_pixelsEast == null)
                        {
                            BuildPixelArray(Graphics.Instance.StairsEast, ref s_pixelsEast);
                        }

                        yield return s_pixelsEast;
                        break;
                    case Direction.West:

                        if (s_pixelsWest == null)
                        {
                            BuildPixelArray(Graphics.Instance.StairsWest, ref s_pixelsWest);
                        }

                        yield return s_pixelsWest;
                        break;
                }

                if (s_pixelsCube == null)
                {
                    BuildPixelArray(Graphics.Instance.Cube, ref s_pixelsCube);
                }

                for (int i = 1; i < SpriteRenderers.Length; i++)
                {
                    yield return s_pixelsCube;
                }
            }
        }

        /// <inheritdoc/>
        public override Vector3Int NearestCornerPosition => new(WorldPosition.x, WorldPosition.y, Room.Origin.z);

        /// <inheritdoc/>
        public override Vector3 OffsetVector => Vector3.down * 2;

        /// <summary>
        /// Checks if a new <see cref="StairSprite"/> can be created at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true if a <see cref="StairSprite"/> can be created at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.CanPlaceObject(position, ObjectDimensions) && GameManager.Instance.IsOnLevel(position.z) <= 0;
        }

        /// <summary>
        /// Initializes a new <see cref="StairSprite"/> at the given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to create the new <see cref="StairSprite"/>.</param>
        public static void CreateStair(Vector3Int position)
        {
            Direction direction = BuildFunctions.Direction;
            if(Map.Instance[position].TryGetNodeAs(~direction, out StairNode stairNode, false))
                position.z = stairNode.WorldPosition.z + 1;

            if (!CheckObject(position))
                return;

            Layer layer = Map.Instance[position.z];

            int z = position.z - layer.Origin.z;

            // ReSharper disable once ObjectCreationAsStatement
            new StairSprite(direction, position, z);
        }

        /// <summary>
        /// Places a highlight object with a <see cref="StairSprite"/> <see cref="Sprite"/> at the given position.
        /// </summary>
        /// <param name="highlight">The highlight game object that is being placed.</param>
        /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
        public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
        {
            Direction direction = BuildFunctions.Direction;
            if (Map.Instance[position].TryGetNodeAs(~direction, out StairNode stairNode, false))
                position.z = stairNode.WorldPosition.z + 1;

            if (CheckObject(position))
            {
                highlight.enabled = true;

                switch (BuildFunctions.Direction)
                {
                    case Direction.North:
                        highlight.sprite = Graphics.Instance.StairsNorth;
                        break;
                    case Direction.South:
                        highlight.sprite = Graphics.Instance.StairsSouth;
                        break;
                    case Direction.East:
                        highlight.sprite = Graphics.Instance.StairsEast;
                        break;
                    case Direction.West:
                        highlight.sprite = Graphics.Instance.StairsWest;
                        break;
                }

                highlight.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
                highlight.sortingOrder = Utility.Utility.GetSortOrder(position);
            }
            else
                highlight.enabled = false;
        
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            RoomNode roomNode = Map.Instance[WorldPosition];
            (roomNode as StairNode)?.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// Called when the created <see cref="AreaSpriteObject"/>s are confirmed.
        /// Creates a new <see cref="StairNode"/> at the given <see cref="Map"/> position if one is not already present.
        /// </summary>
        protected sealed override void OnConfirmingObjects()
        {
            RoomNode roomNode = Map.Instance[WorldPosition];

            _stair ??= new StairNode(this, roomNode, WorldPosition.z, Direction);

            base.OnConfirmingObjects();
        }
    }
}
