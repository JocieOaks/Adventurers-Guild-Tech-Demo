/// <summary>
/// The <see cref="IRiskyTask"/> interface is for <see cref="Task"/>s the have a potential failure condition, such that utility score predictions have to account for both success and failure.
/// Currently <see cref="Planner.PlanNode"/> is not capable of calculating a splinter tree for the case in which an <see cref="IRiskyTask"/> is failed.
/// </summary>
public interface IRiskyTask : ITask
{
    /// <summary>
    /// Creates an estimate of the <see cref="WorldState"/> for after the <see cref="IRiskyTask"/> has been performed, in the case that it is unsuccessful.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="IRiskyTask"/> will occur.</param>
    /// <returns>Creates a new prediction, based on the previously predicted <see cref="WorldState"/> for the new <see cref="WorldState"/> after the <see cref="IRiskyTask"/> is failed.</returns>
    public WorldState FailureState(WorldState worldState);

    /// <summary>
    /// Gives the utility provided from the <see cref="IRiskyTask"/> in the case that it is unsuccessful.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="IRiskyTask"/> will occur.</param>
    /// <returns>Returns the utility score from failing an <see cref="IRiskyTask"/>.</returns>
    public float FailureUtility(WorldState worldState);

    /// <summary>
    /// Gives the chance the <see cref="IRiskyTask"/> will be successful.
    /// </summary>
    /// <param name="worldState">The predicted <see cref="WorldState"/> for when the <see cref="IRiskyTask"/> will occur.</param>
    /// <returns>The value, from 0 to 1, that the <see cref="IRiskyTask"/> will be successful if performed.</returns>
    public float ProbabilityOfSuccess(WorldState worldState);
}
