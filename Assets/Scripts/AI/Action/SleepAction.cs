using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="SleepAction"/> class is a <see cref="TaskAction"/> for when a <see cref="AdventurerPawn"/> is sleeping.
    /// </summary>
    public class SleepAction : ActorAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SleepAction"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> that is sleeping.</param>
        public SleepAction(Actor actor) : base(actor)
        {
        }

        /// <inheritdoc/>
        public override bool CanSpeak => false;

        /// <inheritdoc/>
        public override bool CanListen => false;

        /// <inheritdoc/>
        public override int Complete()
        {
            if (Actor.Stats.Stance != Stance.Lay)
                return -1;

            return Actor.Stats.Sleep >= 10 ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            Actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 5);
        }
    }
}
