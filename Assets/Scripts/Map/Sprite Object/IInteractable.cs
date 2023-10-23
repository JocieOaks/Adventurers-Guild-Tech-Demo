using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// Interface for <see cref="SpriteObject"/>s that can be interacted with by <see cref="AdventurerPawn"/>s.
    /// </summary>
    public interface IInteractable : ISpriteObject
    {
        /// <value>The <see cref="List{T}"/> of all <see cref="RoomNode"/>s from which a <see cref="AdventurerPawn"/> can interact with this <see cref="IInteractable"/>.</value>
        public IEnumerable<RoomNode> InteractionPoints { get; }

        /// <summary>
        /// Sets all <see cref="InteractionPoints"/> to be reserved, so that a <see cref="AdventurerPawn"/> cannot block them.
        /// </summary>
        public void ReserveInteractionPoints();
    }
}