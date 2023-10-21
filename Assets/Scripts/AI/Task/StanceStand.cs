using System.Collections.Generic;
using Assets.Scripts.AI.Action;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="StanceStand"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> transition into <see cref="Stance.Stand"/>.
    /// </summary>
    public class StanceStand : Task, INestingTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StanceStand"/> class.
        /// </summary>
        public StanceStand() : base(null, false, null, null) { }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            worldState.PrimaryActor.Stance = Stance.Stand;
            return worldState;
        }

        /// <inheritdoc/>
        public override bool ConditionsMet(WorldState worldState)
        {
            return base.ConditionsMet(worldState) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceLay;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        {
            yield break;
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return 1;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            if (worldState.PrimaryActor.Stance == Stance.Lay)
                return -1 * (10 -worldState.PrimaryActor.Sleep);
            else
                return -0.5f * (10 - worldState.PrimaryActor.Sleep);
        }
    }
}
