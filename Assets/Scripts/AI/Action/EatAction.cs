using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="EatAction"/> class is a <see cref="TaskAction"/> for eating.
    /// </summary>
    public class EatAction : ActorAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EatAction"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> that is eating.</param>
        public EatAction(Actor actor) : base(actor) {}

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <inheritdoc/>
        public override int Complete()
        {
            if (!Actor.Stats.HasFood)
                return -1;
            if(Actor.Stats.Hunger >= 10)
            {
                Actor.HasFood = false;

                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public override void Initialize() {}

        /// <inheritdoc/>
        public override void Perform()
        {
            Actor.ChangeNeeds(Needs.Hunger, Time.deltaTime / 1.5f);
        }
    }
}
