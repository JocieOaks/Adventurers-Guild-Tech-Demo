public interface IRiskyTask
{
    /// <summary>
    /// Gives the chance a task will be successful.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>The value, for 0 to 1, that a task will be successful if performed.</returns>
    public float ProbabilityOfSuccess(WorldState worldState);

    public float FailureUtility(WorldState worldState);
}
