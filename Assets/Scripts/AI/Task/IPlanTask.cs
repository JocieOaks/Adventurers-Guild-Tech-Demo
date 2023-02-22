using System.Collections.Generic;
/// <summary>
/// Interface <see cref="IPlanTask"/> is a type of <see cref="ITask"/> that does not actually affect <see cref="Pawn"/> behaviour, but is only used by the <see cref="Planner"/>.
/// </summary>
public interface IPlanTask : ITask
{
    /// <summary>
    /// Creates a new <see cref="NestingTask"/>.
    /// </summary>
    /// <param name="nestedTask">The <see cref="Task"/> to be nested inside of the <see cref="IPlanTask"/> to create a <see cref="NestingTask"/>.</param>
    /// <returns>Returns the new <see cref="NestingTask"/>.</returns>
    public Task CreateNestingTask(Task nestedTask)
    {
        return new NestingTask(this, nestedTask);
    }

    /// <summary>
    /// Class <see cref="NestingTask"/> is a <see cref="Task"/> that is the combination of an <see cref="IPlanTask"/> and another <see cref="Task"/>.
    /// It combines the <see cref="Task.ChangeWorldState(WorldState)"/> of both <see cref="Task"/>s but only returns <see cref="Task.GetActions(Actor)"/> from the nested <see cref="Task"/> because
    /// the <see cref="IPlanTask"/> has no <see cref="TaskAction"/>s of it's own. This is used for when the root of the highest utility <see cref="Planner.PlanNode"/> is a <see cref="IPlanTask"/>, taking its place.
    /// </summary>
    class NestingTask : Task
    {
        IPlanTask _planTask;
        Task _nestedTask;

        /// <summary>
        /// Initialize a new instance of <see cref="NestingTask"/>. The conditions for the <see cref="Task"/> constructor are empty, as it is presumed that the conditions for <c>planTask</c>
        /// and <c>nestedTask</c> have already been evaluated, and will not be checked further.
        /// </summary>
        /// <param name="planTask">The <see cref="IPlanTask"/> at the root of the <see cref="Planner.PlanNode"/>.</param>
        /// <param name="nestedTask">The <see cref="Task"/> being nested inside <c>planTask.</c></param>
        public NestingTask(IPlanTask planTask, Task nestedTask) : base(null, null, null, null) 
        {
            _planTask = planTask;
            _nestedTask = nestedTask;
        }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            return _nestedTask.ChangeWorldState(_planTask.ChangeWorldState(worldState));
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        {
            foreach (TaskAction action in _nestedTask.GetActions(actor))
            {
                yield return action;
            }
        }

        /// <inheritdoc/>
        public override float Time(WorldState worldState)
        {
            return 0;
        }

        /// <inheritdoc/>
        public override float Utility(WorldState worldState)
        {
            return 0;
        }
    }
}
