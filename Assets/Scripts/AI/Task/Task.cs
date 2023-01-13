using System.Collections.Generic;
using System.Linq;

public abstract class Task
{
    protected abstract bool? _sitting { get; }
    protected abstract bool? _standing { get; }
    protected abstract bool? _laying { get; }
    protected abstract bool? _conversing { get; }


    /// <summary>
    /// Checks the worldState to see if the Tasks conditions (if it has any) are met.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>Returns true if conditions are met by the worldState.</returns>
    public virtual bool ConditionsMet(WorldState worldState)
    {
        Stance stance = worldState.PrimaryActor.Stance;
        if (_sitting.HasValue && _sitting.Value != (stance == Stance.Sit))
            return false;
        if (_standing.HasValue && _standing.Value != (stance == Stance.Stand))
            return false;
        if (_laying.HasValue && _laying.Value != (stance == Stance.Lay))
            return false;
        if (_conversing.HasValue && _conversing.Value != (worldState.Conversation != null))
            return false;
        return true;
    }

    protected static bool InteractablesCondition(WorldState worldState, List<IInteractable> interactables)
    {

        return interactables.Any(x => x.InteractionPoints.Any(y => y.Traversible)) && (worldState.Conversation == null || interactables.Any(x => worldState.Conversation.InRadius(x.WorldPosition)));
    }

    /// <summary>
    /// Gives the Utility value of a task.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>Returns the utility of success and failure. If there is no failure state, both values will be the same.</returns>
    public abstract float Utility(WorldState worldState);

    /// <summary>
    /// Gives the estimated time for the Task to be completed.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>The estimated time for the Task to be completed.</returns>
    public abstract float Time(WorldState worldState);

    /// <summary>
    /// Creates an estimate of the worldState for after the Task has been performed.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>A new world state based on the expected results of performing the task.</returns>
    public abstract WorldState ChangeWorldState(WorldState worldState);

    public abstract IEnumerable<TaskAction> GetActions(Actor actor);
}