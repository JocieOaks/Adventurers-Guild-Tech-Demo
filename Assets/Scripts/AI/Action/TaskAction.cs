using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Step;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="TaskAction"/> class is the base class for actions for a <see cref="AI.Actor.Pawn"/> consisting of potentially multiple <see cref="TaskStep"/>s.
    /// </summary>
    public abstract class TaskAction
    {
        protected Pawn Pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAction"/> class.
        /// </summary>
        /// <param name="pawn">The <see cref="AI.Actor.Pawn"/> performing the <see cref="TaskAction"/>.</param>
        protected TaskAction(Pawn pawn)
        {
            Pawn = pawn;
        }

        /// <value>Determines whether a <see cref="AI.Actor.Pawn"/> can be spoken to while performing this <see cref="TaskAction"/>.</value>
        public abstract bool CanListen { get; }

        /// <value>Determines whether a <see cref="AI.Actor.Pawn"/> can speak while performing this <see cref="TaskAction"/>.</value>
        public abstract bool CanSpeak { get; }

        /// <summary>
        /// Checks if the <see cref="TaskAction"/> has been completed.
        /// </summary>
        /// <returns> Returns 1 if the <see cref="TaskAction"/> is complete, and -1 if the <see cref="TaskAction"/> cannot be completed.</returns>
        public abstract int Complete();

        /// <summary>
        /// Called when the <see cref="TaskAction"/> first begins.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called each Update by the <see cref="AI.Actor.Pawn"/> performing the <see cref="TaskAction"/>.
        /// </summary>
        public abstract void Perform();
    }
}