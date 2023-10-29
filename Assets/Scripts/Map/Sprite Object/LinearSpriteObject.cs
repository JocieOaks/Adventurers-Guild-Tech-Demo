using System;
using Assets.Scripts.AI;
using Assets.Scripts.Map.Node;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="LinearSpriteObject"/> class is the base class for <see cref="SpriteObject"/>s where there are typically multiple copies of the same <see cref="SpriteObject"/> lined up in a row.
    /// <see cref="LinearSpriteObject"/> extends <see cref="SpriteObject"/> to allow the player to create multiple <see cref="LinearSpriteObject"/>s at the same time.
    /// </summary>
    public abstract class LinearSpriteObject : SpriteObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSpriteObject"/> class.
        /// </summary>
        /// <param name="spriteCount">The number of individual sprites that make up the <see cref="LinearSpriteObject"/>.</param>
        /// <param name="sprites">The initial <see cref="UnityEngine.Sprite"/> to set, depending on the direction. Has only one sprite if undirected.
        /// This is generally the forward most sprite for the <see cref="SpriteObject"/>.</param>
        /// <param name="direction">The direction the <see cref="LinearSpriteObject"/> is facing.</param>
        /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="LinearSpriteObject"/>. For <see cref="LinearSpriteObject"/>s that extend over multiple <see cref="RoomNode"/>s, 
        /// this is the coordinate of the forward most <see cref="RoomNode"/> the object occupies.</param>
        /// <param name="name">The name of the <see cref="LinearSpriteObject"/>.</param>
        /// <param name="dimensions">The 3D dimensions of the <see cref="LinearSpriteObject"/> in <see cref="Map"/> coordinates.</param>
        /// <param name="blocking">If true, the <see cref="RoomNode"/>s the <see cref="LinearSpriteObject"/> occupies are blocked and thus cannot be traversed by a <see cref="AdventurerPawn"/>.</param>
        protected LinearSpriteObject(int spriteCount, Sprite[] sprites, Direction direction, Vector3Int position, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, sprites, direction, position, name, dimensions, blocking)
        {
            Alignment = Utility.Utility.DirectionToEdgeAlignment(direction);

            BuildFunctions.ConfirmingObjects += OnConfirmingObjects;
            BuildFunctions.CheckingLineConstraints += OnCheckingConstraints;
        }

        /// <value>The <see cref="MapAlignment"/> of the <see cref="LinearSpriteObject"/>.</value>
        [JsonProperty]
        public sealed override MapAlignment Alignment { get; }

        /// <summary>
        /// Called when the created <see cref="LinearSpriteObject"/>s are confirmed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnConfirmingObjects(object sender, EventArgs eventArgs)
        {
            Confirm();
        }

        protected virtual void Confirm()
        {
            BuildFunctions.CheckingLineConstraints -= OnCheckingConstraints;
            BuildFunctions.ConfirmingObjects -= OnConfirmingObjects;
        }

        /// <summary>
        /// Called when constraints are checked. Destroys the <see cref="LinearSpriteObject"/> if it isn't within the constraints.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="lineEventArgs"></param>
        private void OnCheckingConstraints(object sender, LineEventArgs lineEventArgs)
        {
            if (Alignment == MapAlignment.XEdge && (WorldPosition.x < lineEventArgs.Start || WorldPosition.x > lineEventArgs.End) || Alignment == MapAlignment.YEdge && (WorldPosition.y < lineEventArgs.Start || WorldPosition.y > lineEventArgs.End))
            {
                BuildFunctions.ConfirmingObjects -= OnConfirmingObjects;
                BuildFunctions.CheckingLineConstraints -= OnCheckingConstraints;

                Destroy();
            }
        }
    }
}