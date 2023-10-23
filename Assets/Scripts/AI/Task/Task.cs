using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.AI.Social;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="Task"/> class is the base class for all long term AI behaviors, consisting of potential multiple <see cref="TaskAction"/>s.
    /// </summary>
    public abstract class Task : ITask
    {
        //The following fields are used as flags for the basic conditions for a Task to be performed. The flags are nullable which means that the Task can be performed whether the condition is true or false.
        protected bool? Sitting, Standing, Laying, Conversing;

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class, and sets the conditional flags.
        /// </summary>
        /// <param name="sitting">Determines whether a <see cref="Task"/> should be performed while the <see cref="AdventurerPawn"/> is in <see cref="Stance.Sit"/>. Can be null.</param>
        /// <param name="standing">Determines whether a <see cref="Task"/> should be performed while the <see cref="AdventurerPawn"/> is in <see cref="Stance.Stand"/>. Can be null.</param>
        /// <param name="laying">Determines whether a <see cref="Task"/> should be performed while the <see cref="AdventurerPawn"/> is in <see cref="Stance.Lay"/>. Can be null.</param>
        /// <param name="conversing">Determines whether a <see cref="Task"/> should be performed while the <see cref="AdventurerPawn"/> is in a <see cref="Conversation"/>. Can be null.</param>
        protected Task(bool? sitting, bool? standing, bool? laying, bool? conversing)
        {
            Sitting = sitting;
            Standing = standing;
            Laying = laying;
            Conversing = conversing;
        }


        /// <inheritdoc/>
        public abstract WorldState ChangeWorldState(WorldState worldState);

        /// <inheritdoc/>
        public virtual bool ConditionsMet(WorldState worldState)
        {
            Stance stance = worldState.PrimaryActor.Stance;
            if (Sitting.HasValue && Sitting.Value != (stance == Stance.Sit))
                return false;
            if (Standing.HasValue && Standing.Value != (stance == Stance.Stand))
                return false;
            if (Laying.HasValue && Laying.Value != (stance == Stance.Lay))
                return false;
            if (Conversing.HasValue && Conversing.Value != (worldState.Conversation != null))
                return false;
            return true;
        }

        /// <inheritdoc/>
        public abstract IEnumerable<TaskAction> GetActions(Actor.Actor actor);

        /// <inheritdoc/>
        public abstract float Time(WorldState worldState);

        /// <inheritdoc/>
        public abstract float Utility(WorldState worldState);

        /// <summary>
        /// Static method used by some <see cref="ConditionsMet(WorldState)"/> to check if any <see cref="IInteractable"/>s are available to perform a <see cref="Task"/>.
        /// </summary>
        /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
        /// <param name="interactables">A list of <see cref="IInteractable"/>s that can potentially be used to perform a <see cref="Task"/>.</param>
        /// <returns></returns>
        protected static bool InteractablesCondition(WorldState worldState, List<IInteractable> interactables)
        {
            return interactables.Any(x =>
                {
                    foreach (RoomNode interactionPoint in x.InteractionPoints)
                    {
                        if (interactionPoint.Traversable && Sector.SameSector(x, worldState.PrimaryActor.RoomNode) && (worldState.Conversation == null || worldState.Conversation.InRadius(x.WorldPosition)))
                            return true;
                    }
                    return false;
                }
            );
        }
    }
}