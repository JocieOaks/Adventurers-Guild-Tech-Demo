using System;
using System.Collections.Generic;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="FloorSprite"/> class is a <see cref="SpriteObject"/> for flooring.
    /// It corresponds to <see cref="RoomNode"/>s and every <see cref="RoomNode"/> has a <see cref="FloorSprite"/>.
    /// </summary>
    public class FloorSprite : AreaSpriteObject
    {
        // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all Floors.
        private static bool[,] s_pixels;
        private static readonly Sprite[] s_sprites = { null };
        private bool _enabled;
        private int _spriteIndex;
        public FloorSprite(Vector3Int position) : base(1, s_sprites, Direction.Undirected, position, "Floor", new Vector3Int(1, 1, 0), false)
        {
            if (position == Vector3Int.back)
                return;

            SpriteRenderer.sortingOrder = Utility.Utility.GetSortOrder(position) - 4;
            Confirm();
        }

        /// <value>The current index for any <see cref="FloorSprite"/>s placed.</value>
        public static int FloorSpriteIndex { private get; set; } = 0;

        /// <value>Sets whether the <see cref="FloorSprite"/> is active or not.
        /// Unlike other <see cref="SpriteObject"/>s, the <see cref="FloorSprite"/> class is created for every <see cref="RoomNode"/>,
        /// even if their is no actual floor in that location.</value>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (value)
                {
                    Sprite = Graphics.Instance.FloorSprites[_spriteIndex];
                    Graphics.LevelChanged += WhenLevelChanged;

                }
                else
                {
                    Sprite = null;
                    Graphics.LevelChanged -= WhenLevelChanged;
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<bool[,]> GetMaskPixels
        {
            get
            {
                if (s_pixels == null)
                {
                    BuildPixelArray(Graphics.Instance.FloorSprites[0], ref s_pixels);
                }
                if (Enabled)
                    yield return s_pixels;
            }
        }

        /// <value>The sprite index for the <see cref="FloorSprite"/>. When set, enables the <see cref="FloorSprite"/>.</value>
        public int SpriteIndex
        {
            get => _spriteIndex;
            set
            {
                _spriteIndex = value;
                Sprite = Graphics.Instance.FloorSprites[value];

                if (_enabled) return;

                _enabled = true;
                if (GameManager.Instance.IsOnLevel(WorldPosition.z) > 0)
                    SpriteRenderer.enabled = false;

                Graphics.LevelChanged += WhenLevelChanged;
            }
        }

        /// <summary>
        /// Checks if the <see cref="FloorSprite"/> can be enabled at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="position"><see cref="Map"/> position to check.</param>
        /// <returns>Returns true if <see cref="FloorSprite"/> is supported at <c>position</c>.</returns>
        public static bool CheckObject(Vector3Int position)
        {
            return Map.Instance.IsSupported(position, MapAlignment.Center);
        }

        /// <summary>
        /// Enables the <see cref="FloorSprite"/> at the given <see cref="Map"/> position, but does not confirm it until <see cref="WhenConfirmingObjects"/> is called.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position where the <see cref="FloorSprite"/> should be enabled.</param>
        public static void CreateFloor(Vector3Int position)
        {
            FloorSprite floor = Map.Instance[position].Floor;
            floor.Sprite = Graphics.Instance.FloorSprites[FloorSpriteIndex];
            floor.SpriteRenderer.color = Graphics.Instance.HighlightColor;
            BuildFunctions.ConfirmingObjects += floor.WhenConfirmingObjects;
            BuildFunctions.CheckingAreaConstraints += floor.WhenCheckingConstraints;
        }

        /// <summary>
        /// Highlights the <see cref="FloorSprite"/> at a given <see cref="Map"/> position.
        /// </summary>
        /// <param name="highlight">Not actually used, but present for delegate pattern matching.</param>
        /// <param name="position"><see cref="Map"/> position of <see cref="FloorSprite"/> to be highlighted.</param>
        public static void PlaceHighlight(SpriteRenderer highlight,Vector3Int position)
        {
            Graphics.Instance.ResetSprite();
            if (CheckObject(position))
            {
                RoomNode node = Map.Instance[position];
                node.Floor.Highlight(Graphics.Instance.HighlightColor, FloorSpriteIndex);
            }
        }

        /// <summary>
        /// Sets the <see cref="FloorSprite"/> to be disabled, unless the <see cref="FloorSprite"/> is at ground level, in which the sprite is set to ground.
        /// </summary>
        public override void Destroy()
        {
            if (WorldPosition.z > 0)
                Enabled = false;
            else
                SpriteIndex = 1;
        }

        /// <summary>
        /// Highlights this <see cref="FloorSprite"/>.
        /// </summary>
        /// <param name="color"><see cref="Color"/> to set the <see cref="SpriteRenderer"/> to.</param>
        /// <param name="spriteIndex">Sprite index to set this <see cref="FloorSprite"/> to.</param>
        public void Highlight(Color color, int spriteIndex)
        {
            SpriteRenderer.color = color;
            Sprite = Graphics.Instance.FloorSprites[spriteIndex];
            Graphics.ResettingSprite += WhenResettingSprite;
        }

        /// <inheritdoc/>
        protected override void WhenCheckingConstraints(object sender, AreaEventArgs areaEventArgs)
        {
            int minX = areaEventArgs.Start.x < areaEventArgs.End.x ? areaEventArgs.Start.x : areaEventArgs.End.x;
            int maxX = areaEventArgs.Start.x > areaEventArgs.End.x ? areaEventArgs.Start.x : areaEventArgs.End.x;
            int minY = areaEventArgs.Start.y < areaEventArgs.End.y ? areaEventArgs.Start.y : areaEventArgs.End.y;
            int maxY = areaEventArgs.Start.y > areaEventArgs.End.y ? areaEventArgs.Start.y : areaEventArgs.End.y;

            if (WorldPosition.x < minX || WorldPosition.y < minY || WorldPosition.x > maxX || WorldPosition.y > maxY)
            {
                BuildFunctions.ConfirmingObjects -= WhenConfirmingObjects;
                BuildFunctions.CheckingAreaConstraints -= WhenCheckingConstraints;

                ResetSprite();
            }
        }

        /// <inheritdoc/>
        protected sealed override void Confirm()
        {
            SpriteIndex = FloorSpriteIndex;
            Enabled = true;
            base.Confirm();
        }

        /// <inheritdoc/>
        protected override void ResetSprite()
        {

            SpriteRenderer.color = Color.white;
            Sprite = !Enabled ? null : Graphics.Instance.FloorSprites[SpriteIndex];

            Graphics.ResettingSprite -= WhenResettingSprite;
        }
    }
}
