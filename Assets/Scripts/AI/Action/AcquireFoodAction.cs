using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Sprite_Object;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="AcquireFoodAction"/> class is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to obtain food. Will be replaced when the inventory system is more fleshed out.
    /// </summary>
    public class AcquireFoodAction : ActorAction
    {
        private readonly IInteractable _interactable;

        private float _tick;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcquireFoodAction"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> that is getting food.</param>
        /// <param name="interactable">The <see cref="IInteractable"/> from which <c>actor</c> is getting food.</param>
        public AcquireFoodAction(Actor actor, IInteractable interactable) : base(actor)
        {
            _interactable = interactable;
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <inheritdoc/>
        public override int Complete()
        {
            return _tick > 2 ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Actor.HasFood = true;
            Actor.Pawn.CurrentStep = new WaitStep(Actor.Pawn, Utility.Utility.VectorToDirection(_interactable.WorldPosition - Actor.Pawn.WorldPosition), true);
        }

        /// <inheritdoc/>
        public override void Perform() 
        {
            _tick += Time.deltaTime;
        }
    }
}
