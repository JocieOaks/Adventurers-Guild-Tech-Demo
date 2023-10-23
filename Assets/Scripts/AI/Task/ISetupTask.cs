using Assets.Scripts.AI.Planning;

namespace Assets.Scripts.AI.Task
{
    public delegate Task GetPayoffDelegate(WorldState worldState);

    /// <summary>
    /// The <see cref="ISetupTask"/> interface is for <see cref="Task"/>s that enable another <see cref="Task"/> to be performed. This ensures that the <see cref="Payoff"/> <see cref="Task"/> is considered 
    /// when calculating the utility of performing this <see cref="Task"/>.
    /// </summary>
    public interface ISetupTask : ITask
    {
        /// <value>Gets a delegate of the <see cref="ConstructPayoff(WorldState)"/> method for use by <see cref="Planner.PlanNode"/>.</value>
        public GetPayoffDelegate Payoff => ConstructPayoff;

        /// <summary>
        /// Creates the payoff <see cref="Task"/> to be used by <see cref="Planner.PlanNode"/> when calculating the utility of a string of <see cref="Task"/>s.
        /// </summary>
        /// <param name="worldState">The predicted <see cref="WorldState"/> for when the payoff <see cref="Task"/> will occur.</param>
        /// <returns>Returns a new <see cref="Task"/> to be used to calculate the utility of the <see cref="ISetupTask"/>.</returns>
        public Task ConstructPayoff(WorldState worldState);
    }
}