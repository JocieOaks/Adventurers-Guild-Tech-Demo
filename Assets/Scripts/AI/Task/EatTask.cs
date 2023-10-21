using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using UnityEngine;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="EatTask"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> eat food.
    /// </summary>
    internal class EatTask : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EatTask"/> class.
        /// </summary>
        public EatTask() : base(null, null, null, null) { }

        public override WorldState ChangeWorldState(WorldState worldState)
        {
            worldState.PrimaryActor.Hunger = 10;
            worldState.PrimaryActor.HasFood = false;
            return worldState;
        }

        /// <inheritdoc/>
        public override bool ConditionsMet(WorldState worldState)
        {
            return worldState.PrimaryActor.HasFood;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        {
            //Debug.Log(actor.Stats.Name + " Eat " + actor.Stats.Hunger);
            yield return new EatAction(actor);
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return Mathf.Max(5, (10 - worldState.PrimaryActor.Hunger) * 1.5f);
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            if (worldState.PrimaryActor.Stance == Stance.Stand)
                return -5 * Time(worldState);
            else if (worldState.PrimaryActor.Stance == Stance.Lay)
                return -10 * Time(worldState);
            return 0;
        }
    }
}
