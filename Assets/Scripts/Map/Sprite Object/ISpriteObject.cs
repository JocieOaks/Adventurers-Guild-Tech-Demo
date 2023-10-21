using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.Data;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="ISpriteObject"/> interface is the interface that corresponds to the <see cref="SpriteObject"/> base class. 
    /// It is used for other interfaces that extend <see cref="SpriteObject"/> such as <see cref="IInteractable"/>.
    /// </summary>
    public interface ISpriteObject : IDataPersistence, IWorldPosition
    {
        /// <value>The pixels blocked by the <see cref="SpriteObject"/>'s sprites as an array of bool. Used by <see cref="AdventurerPawn"/> to construct a <see cref="SpriteMask"/> for all objects in front of it.</value>
        IEnumerable<bool[,]> GetMaskPixels { get; }

        /// <value>Gives the offset for <see cref="GetMaskPixels"/> for <see cref="AdventurerPawn"/> when constructing a <see cref="SpriteMask"/> composed of multiple copies of the same <see cref="UnityEngine.Sprite"/>.</value>
        Vector3 OffsetVector { get; }

        /// <value>Gives the <see cref="UnityEngine.SpriteRenderer"/> for the forward most sprite of the <see cref="SpriteObject"/>.</value>
        SpriteRenderer SpriteRenderer { get; }

        /// <summary>
        /// Destroys the <see cref="SpriteObject"/> and all of it's <see cref="UnityEngine.SpriteRenderer"/>s.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Sets the <see cref="SpriteObject"/> to a specific highlight color.
        /// </summary>
        /// <param name="color">The color to set the <see cref="UnityEngine.SpriteRenderer"/>s.</param>
        void Highlight(Color color);

        /// <summary>
        /// Calculates the effect on a <see cref="Pawn"/>'s speed from trying to move through the <see cref="RoomNode"/> at the given position. 
        /// </summary>
        /// <param name="nodePosition">The position of the <see cref="RoomNode"/> being evaluated.</param>
        /// <returns></returns>
        float SpeedMultiplier(Vector3Int nodePosition);
    }
}