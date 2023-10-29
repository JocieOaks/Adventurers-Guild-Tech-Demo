using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="AreaSpriteObject"/> class is the base class for <see cref="SpriteObject"/>s where there are typically multiple copies of the same <see cref="SpriteObject"/> within an area.
    /// <see cref="AreaSpriteObject"/> extends <see cref="SpriteObject"/> to allow the player to create multiple <see cref="AreaSpriteObject"/>s at the same time.
    /// </summary>
    public abstract class AreaSpriteObject : SpriteObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AreaSpriteObject"/> class.
        /// </summary>
        /// <param name="spriteCount">The number of individual sprites that make up the <see cref="AreaSpriteObject"/>.</param>
        /// <param name="sprites">The initial <see cref="UnityEngine.Sprite"/> to set, depending on the direction. Has only one sprite if undirected.
        /// This is generally the forward most sprite for the <see cref="AreaSpriteObject"/>.</param>
        /// <param name="direction">The direction the <see cref="AreaSpriteObject"/> is facing.</param>
        /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="AreaSpriteObject"/>. For <see cref="AreaSpriteObject"/>s that extend over multiple <see cref="RoomNode"/>s, 
        /// this is the coordinate of the forward most <see cref="RoomNode"/> the object occupies.</param>
        /// <param name="name">The name of the <see cref="AreaSpriteObject"/>.</param>
        /// <param name="dimensions">The 3D dimensions of the <see cref="AreaSpriteObject"/> in <see cref="Map"/> coordinates.</param>
        /// <param name="blocking">If true, the <see cref="RoomNode"/>s the <see cref="AreaSpriteObject"/> occupies are blocked and thus cannot be traversed by a <see cref="AdventurerPawn"/>.</param>
        protected AreaSpriteObject(int spriteCount, Sprite[] sprites, Direction direction, Vector3Int position, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, sprites, direction, position, name, dimensions, blocking)
        {
            SpriteRenderer.color = Graphics.Instance.HighlightColor;
            BuildFunctions.ConfirmingObjects += OnConfirmingObjects;
            BuildFunctions.CheckingAreaConstraints += OnCheckingConstraints;
        }

        /// <summary>
        /// Called when the created <see cref="AreaSpriteObject"/>s are confirmed.
        /// </summary>
        protected virtual void OnConfirmingObjects()
        {
            SpriteRenderer.color = Color.white;
            BuildFunctions.CheckingAreaConstraints -= OnCheckingConstraints;
            BuildFunctions.ConfirmingObjects -= OnConfirmingObjects;
        }

        /// <summary>
        /// Called when constraints are checked. Destroys the <see cref="AreaSpriteObject"/> if it isn't within the constraints.
        /// </summary>
        /// <param name="start">The position of first corner of the area.</param>
        /// <param name="end">The position of the second corner of the area.</param>
        protected virtual void OnCheckingConstraints(Vector3Int start, Vector3Int end)
        {
            int minX = start.x < end.x ? start.x : end.x;
            int maxX = start.x > end.x ? start.x : end.x;
            int minY = start.y < end.y ? start.y : end.y;
            int maxY = start.y > end.y ? start.y : end.y;

            if (WorldPosition.x < minX || WorldPosition.y < minY || WorldPosition.x > maxX || WorldPosition.y > maxY)
            {
                BuildFunctions.ConfirmingObjects -= OnConfirmingObjects;
                BuildFunctions.CheckingAreaConstraints -= OnCheckingConstraints;

                Destroy();
            }
        }
    }
}
