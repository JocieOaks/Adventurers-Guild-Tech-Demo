using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// The <see cref="Corner"/> class is a unity component for the <see cref="Sprite"/>s at the corners of two or more <see cref="WallSprite"/>s.
    /// </summary>
    public class Corner : MonoBehaviour
    {
        private static readonly List<int> s_ignoreIndices = new() { 1, 2, 4, 5, 8, 10 };
        private readonly AccentMaterial _material = AccentMaterial.Stone;
        private Vector3Int _position;
        private int _spriteIndex;
        private SpriteRenderer _spriteRenderer;
        private WallBlocker East => Map.Map.Instance.GetWall(MapAlignment.XEdge, _position);
        private WallBlocker North => Map.Map.Instance.GetWall(MapAlignment.YEdge, _position);
        private WallBlocker South => Map.Map.Instance.GetWall(MapAlignment.YEdge, _position + Vector3Int.down);
        private WallBlocker West => Map.Map.Instance.GetWall(MapAlignment.XEdge, _position + Vector3Int.left);

        /// <summary>
        /// Creates a corner at the given position, if there should be one there.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the <see cref="Corner"/>.</param>
        /// <param name="corner">An out parameter that will be set to the newly created corner.</param>
        /// <returns>Returns true if a corner was successfully made.</returns>
        public static bool TryMakeCorner(Vector3Int position, out Corner corner)
        {
            int index = GetSpriteIndex(position);
            if (index == -1)
            {
                corner = null;
                return false;
            }

            corner = Instantiate(Graphics.Instance.SpritePrefab).AddComponent<Corner>();
            corner.gameObject.name = "Corner";

            corner._position = position;

            corner.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position, MapAlignment.Corner);
            corner._spriteRenderer.sortingOrder = Utility.Utility.GetSortOrder(position) + 3;

            corner.ConfigureCorner(index);
            return true;
        }

        /// <summary>
        /// Evaluates the sprite index for the <see cref="Corner"/> and then calls <see cref="ConfigureCorner"/>.
        /// </summary>
        public void ConfigureCorner()
        {
            ConfigureCorner(GetSpriteIndex(_position));
        }

        /// <summary>
        /// Determines the type of corner that should be set at the given position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the <see cref="Corner"/>.</param>
        /// <returns>Returns the index of the sprite for the <see cref="Corner"/> at <c>position</c> or -1 if no corner is needed.</returns>
        private static int GetSpriteIndex(Vector3Int position)
        {
            int index = 0;
            index += Map.Map.Instance.GetWall(MapAlignment.XEdge, position) != null ? 1 : 0;
            index += Map.Map.Instance.GetWall(MapAlignment.YEdge, position + Vector3Int.down) != null ? 2 : 0;
            index += Map.Map.Instance.GetWall(MapAlignment.XEdge, position + Vector3Int.left) != null ? 4 : 0;
            index += Map.Map.Instance.GetWall(MapAlignment.YEdge, position) != null ? 8 : 0;


            if (s_ignoreIndices.Any(x => x == index))
            {
                return -1;
            }
            index -= index switch
            {
                >= 10 => 7,
                >= 8 => 6,
                >= 5 => 5,
                _ => 3,
            };
            if (index < 0)
            {
                return -1;
            }

            return index;


        }

        [UsedImplicitly]
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Graphics.UpdatedGraphics += SetCornerMode;
            Graphics.LevelChangedLate += OnLevelChange;
        }

        /// <summary>
        /// Sets up the corner, destroying the corner if no corner is needed, and masking the adjacent <see cref="WallSprite"/>s if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void ConfigureCorner(int index)
        {
            _spriteIndex = index;

            switch (_spriteIndex)
            {
                case -1:
                    Graphics.UpdatedGraphics -= SetCornerMode;
                    Graphics.LevelChangedLate -= OnLevelChange;

                    West?.WallSprite.MaskCorner(0);
                    South?.WallSprite.MaskCorner(0);

                    Destroy(gameObject);
                    return;
                case 1:
                case 7:
                case 8:
                    West.WallSprite.MaskCorner(1);
                    South.WallSprite.MaskCorner(-1);
                    break;
                case 2:
                    West.WallSprite.MaskCorner(-1);
                    South.WallSprite.MaskCorner(1);
                    break;
                default:
                    West?.WallSprite.MaskCorner(0);
                    South?.WallSprite.MaskCorner(0);
                    break;
            }
        }

        /// <summary>
        /// Sets whether the <see cref="Corner"/> should be a full sprite or just a base sprite.
        /// </summary>
        private void SetCornerMode()
        {
            if (GameManager.Instance.IsOnLevel(_position.z) == 0)
            {
                if (Graphics.Instance.Mode == WallDisplayMode.Open)
                {
                    bool? xPos = East?.WallSprite.IsFullWall;
                    bool? yNeg = South?.WallSprite.IsFullWall;
                    bool? xNeg = West?.WallSprite.IsFullWall;
                    bool? yPos = North?.WallSprite.IsFullWall;
                    bool fullCorner = (xPos ?? false) || (yNeg ?? false) || (xNeg ?? false) || (yPos ?? false);
                    _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _material, fullCorner];

                    if (fullCorner)
                    {
                        if (!xPos ?? false)
                            East.WallSprite.SetEdge();
                        if (!yPos ?? false)
                            North.WallSprite.SetEdge();
                        if (!yNeg ?? false)
                            South.WallSprite.SetEdge();
                        if (!xNeg ?? false)
                            West.WallSprite.SetEdge();
                    }
                }
                else
                {
                    _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _material, Graphics.Instance.Mode == WallDisplayMode.Full];
                }
            }
        }

        /// <summary>
        /// Called whenever the level changes, to determine how the <see cref="Corner"/> should be rendered.
        /// </summary>
        private void OnLevelChange()
        {
            int level = GameManager.Instance.IsOnLevel(_position.z);
            if (level > 0)
                _spriteRenderer.enabled = false;
            else
            {
                _spriteRenderer.enabled = true;
                if (level == 0)
                {
                    SetCornerMode();
                }
                else
                {
                    _spriteRenderer.sprite = Graphics.CornerSprites[_spriteIndex, _material, true];
                }
            }
        }
    }
}