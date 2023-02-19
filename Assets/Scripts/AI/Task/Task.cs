using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The <see cref="Task"/> class is the base class for all long term AI behaviours, consisting of potentiall multiple <see cref="TaskAction"/>s.
/// </summary>
public abstract class Task
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


    /// <summary>
    /// Creates an estimate of the <see cref="WorldState"/> for after the <see cref="Task"/> has been performed.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Creates a new prediction, based on the previously predicted <see cref="WorldState"/> for the new <see cref="WorldState"/> after the <see cref="Task"/> is performed.</returns>
    public abstract WorldState ChangeWorldState(WorldState worldState);

    /// <summary>
    /// Checks the <see cref="WorldState"/> to see if the <see cref="Task"/>'s conditions are met.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns true if the conditions are met by <c>worldState</c>.</returns>
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

    /// <summary>
    /// Creates a list of <see cref="TaskAction"/>s to be performed by a <see cref="Actor"/> in order to complete the <see cref="Task"/>.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> performing the <see cref="Task"/>.</param>
    /// <returns>Enumerates over the <see cref="TaskAction"/>s to perform the <see cref="Task"/></returns>
    public abstract IEnumerable<TaskAction> GetActions(Actor actor);

    /// <summary>
    /// Gives the estimated time for the <see cref="Task"/> to be completed.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns the expected time it will take to perform the <see cref="Task"/> while in the given <see cref="WorldState"/>.</returns>
    public abstract float Time(WorldState worldState);

    /// <summary>
    /// Gives the utility score of the <see cref="Task"/>.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns the expected utility for performing the <see cref="Task"/> while in the given <see cref="WorldState"/>.</returns>
    public abstract float Utility(WorldState worldState);

    /// <summary>
    /// Static method used by some <see cref="ConditionsMet(WorldState)"/> to check if any <see cref="IInteractable"/>s are available to perform a <see cref="Task"/>.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <param name="interactables">A list of <see cref="IInteractable"/>s that can potentially be used to perform a <see cref="Task"/>.</param>
    /// <returns></returns>
    protected static bool InteractablesCondition(WorldState worldState, List<IInteractable> interactables)
    {

        return interactables.Any(x => x.InteractionPoints.Any(y => y.Traversable)) && 
            (worldState.Conversation == null || interactables.Any(x => worldState.Conversation.InRadius(x.WorldPosition))) && 
            interactables.Any(x => x.InteractionPoints.Any(y => y.Sector == Map.Instance[worldState.PrimaryActor.Position].Sector));
    }
}