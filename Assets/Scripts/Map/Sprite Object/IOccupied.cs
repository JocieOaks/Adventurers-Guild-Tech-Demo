using Assets.Scripts.AI;
using Assets.Scripts.AI.Actor;
using UnityEngine;

namespace Assets.Scripts.Map.Sprite_Object
{
    public interface IOccupied : IPlayerInteractable
    {
        /// <value>The <see cref="AdventurerPawn"/> occupying the <see cref="IOccupied"/>. Null if empty.</value>
        public Pawn Occupant { get; set; }
        /// <value>Returns true if the <see cref="IOccupied"/> has an <see cref="AdventurerPawn"/> occupying it.</value>
        public bool Occupied => Occupant != null;

        /// <summary>
        /// Called when a given <see cref="AdventurerPawn"/> begins occupying the <see cref="IOccupied"/>.
        /// </summary>
        /// <param name="pawn"><see cref="AdventurerPawn"/> occupying the <see cref="IOccupied"/>.</param>
        public void Enter(Pawn pawn);

        /// <summary>
        /// Called when a given <see cref="AdventurerPawn"/> stops occupying the <see cref="IOccupied"/>. Checks to ensure
        /// that the given <see cref="AdventurerPawn"/> is actually occupying the <see cref="IOccupied"/>.
        /// </summary>
        /// <param name="pawn"><see cref="AdventurerPawn"/> exiting the <see cref="IOccupied"/>.</param>
        /// <param name="exitTo">If not left as default, has <c>pawn</c> move to the specified position.</param>
        public void Exit(Pawn pawn, Vector3Int exitTo = default);
    }
}
