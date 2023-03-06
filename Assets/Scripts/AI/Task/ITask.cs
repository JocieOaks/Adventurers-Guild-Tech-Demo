using System.Collections.Generic;

/// <summary>
/// The <see cref="ITask"/> interface is the interface that corrseponds to the <see cref="Task"/> base class. 
/// It is used for other interfaces that extend <see cref="Task"/> such as <see cref="IRiskyTask"/> and <see cref="ISetupTask"/>.
/// </summary>
public interface ITask
{
    /// <summary>
    /// Creates an estimate of the <see cref="WorldState"/> for after the <see cref="Task"/> has been performed.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Creates a new prediction, based on the previously predicted <see cref="WorldState"/> for the new <see cref="WorldState"/> after the <see cref="Task"/> is performed.</returns>
    WorldState ChangeWorldState(WorldState worldState);

    /// <summary>
    /// Checks the <see cref="WorldState"/> to see if the <see cref="Task"/>'s conditions are met.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns true if the conditions are met by <c>worldState</c>.</returns>
    bool ConditionsMet(WorldState worldState);

    /// <summary>
    /// Creates a list of <see cref="TaskAction"/>s to be performed by a <see cref="Actor"/> in order to complete the <see cref="Task"/>.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> performing the <see cref="Task"/>.</param>
    /// <returns>Enumerates over the <see cref="TaskAction"/>s to perform the <see cref="Task"/></returns>
    IEnumerable<TaskAction> GetActions(Actor actor);

    /// <summary>
    /// Gives the estimated time for the <see cref="Task"/> to be completed.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns the expected time it will take to perform the <see cref="Task"/> while in the given <see cref="WorldState"/>.</returns>
    float Time(WorldState worldState);

    /// <summary>
    /// Gives the utility score of the <see cref="Task"/>.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="Task"/> will occur.</param>
    /// <returns>Returns the expected utility for performing the <see cref="Task"/> while in the given <see cref="WorldState"/>.</returns>
    float Utility(WorldState worldState);
}
