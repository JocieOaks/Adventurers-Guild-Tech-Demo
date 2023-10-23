using Assets.Scripts.AI.Actor;

namespace Assets.Scripts.AI.Step
{
    /// <summary>
    /// The <see cref="TaskStep"/> class is the base class for small single steps for a <see cref="AI.Actor.Pawn"/> to take.
    /// <see cref="TaskStep"/> is also responsible for controlling <see cref="AI.Actor.Pawn"/> animations.
    /// </summary>
    public abstract class TaskStep
    {

        protected const float BREATH_TIME = 0.125f;
        protected const float STEP_TIME = 0.5f;
        protected Pawn Pawn;
        protected int Frame = 0;
        protected float Period = 0f;
        private bool _finished;

        /// <summary>
        /// Initializes a new instance of <see cref="TaskStep"/>.
        /// </summary>
        /// <param name="pawn">The <see cref="AI.Actor.Pawn"/> performing the <see cref="TaskStep"/>.</param>
        protected TaskStep(Pawn pawn)
        {
            Pawn = pawn;
        }

        /// <value>Evaluates whether the <see cref="TaskStep"/> has finished.</value>
        protected abstract bool Complete { get; }

        /// <summary>
        /// Evaluates whether the <see cref="TaskStep"/> has finished. Calls <see cref="Finish"/> the first time it returns true.
        /// </summary>
        /// <returns>True if the <see cref="TaskStep"/> has finished.</returns>
        public bool IsComplete()
        {
            if(Complete)
            {
                if (!_finished)
                {
                    Finish();
                    _finished= true;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called every Unity update.
        /// </summary>
        public abstract void Perform();

        /// <summary>
        /// Forces the <see cref="TaskStep"/> to run <see cref="Finish"/>, even if the <see cref="IsComplete"/> is not true.
        /// </summary>
        public void ForceFinish()
        {
            if(!_finished)
            {
                Finish();
                _finished = true;
            }
        }

        /// <summary>
        /// Called once the <see cref="TaskStep"/> has completed.
        /// </summary>
        protected virtual void Finish() { }
    }
}