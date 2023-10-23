using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Planning;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="WaitTask"/> class is a <see cref="Task"/> for a <see cref="AdventurerPawn"/> to wait a specified amount of time.
    /// </summary>
    public class WaitTask : Task
    {
        private readonly float _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTask"/> class.
        /// </summary>
        /// <param name="time">The time in seconds that a <see cref="AdventurerPawn"/> should wait.</param>
        public WaitTask(float time) : base(null, null, null, null)
        {
            _time = time;
        }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            return worldState;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor.Actor actor)
        {
            yield return new WaitAction(_time, actor.Pawn);
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return _time;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            return 0;
        }
    }
}
