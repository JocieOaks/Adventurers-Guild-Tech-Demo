using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// The <see cref="Task"/> class is the base class for all long term AI behaviours, consisting of potentiall multiple <see cref="TaskAction"/>s.
/// </summary>
public abstract class Task : ITask
{
    //The following fields are used as flags for the basic conditions for a Task to be performed. The flags are nullible which means that the Task can be performed whether the condition is true or false.
    protected bool? _sitting, _standing, _laying, _conversing;

    /// <summary>
    /// Initializes a new instance of the <see cref="Task"/> class, and sets the conditional flags.
    /// </summary>
    /// <param name="sitting">Determines whether a <see cref="Task"/> should be performed while the <see cref="Pawn"/> is in <see cref="Stance.Sit"/>. Can be null.</param>
    /// <param name="standing">Determines whether a <see cref="Task"/> should be performed while the <see cref="Pawn"/> is in <see cref="Stance.Stand"/>. Can be null.</param>
    /// <param name="laying">Determines whether a <see cref="Task"/> should be performed while the <see cref="Pawn"/> is in <see cref="Stance.Lay"/>. Can be null.</param>
    /// <param name="conversing">Determines whether a <see cref="Task"/> should be performed while the <see cref="Pawn"/> is in a <see cref="Conversation"/>. Can be null.</param>
    protected Task(bool? sitting, bool? standing, bool? laying, bool? conversing)
    {
        _sitting = sitting;
        _standing = standing;
        _laying = laying;
        _conversing = conversing;
    }


    /// <inheritdoc/>
    public abstract WorldState ChangeWorldState(WorldState worldState);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public abstract IEnumerable<TaskAction> GetActions(Actor actor);

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