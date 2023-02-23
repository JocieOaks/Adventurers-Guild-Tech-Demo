using System.Collections.Generic;
/// <summary>
/// Interface <see cref="INestingTask"/> is a type of <see cref="ITask"/> that is very brief, normally a quick status change, or is only used by the <see cref="Planner"/> and does not affect the <see cref="Pawn"/>'s behaviour. 
/// Therefore it works better when chained with a followup <see cref="Task"/>.
/// </summary>
public interface INestingTask : ITask
{
    /// <summary>
    /// Creates a new <see cref="NestedTask"/>.
    /// </summary>
    /// <param name="nestedTask">The <see cref="Task"/> to be nested inside of the <see cref="INestingTask"/> to create a <see cref="NestedTask"/>.</param>
    /// <returns>Returns the new <see cref="NestedTask"/>.</returns>
    public Task CreateNestedTask(Task nestedTask)
    {
        return new NestedTask(this, nestedTask);
    }

    /// <summary>
    /// Class <see cref="NestedTask"/> is a <see cref="Task"/> that is the combination of an <see cref="INestingTask"/> and another <see cref="Task"/>.
    /// </summary>
    class NestedTask : Task
    {
        INestingTask _initialTask;
        Task _followupTask;

        /// <summary>
        /// Initialize a new instance of <see cref="NestedTask"/>. The conditions for the <see cref="Task"/> constructor are empty, as it is presumed that the conditions for <c>initialTask</c>
        /// and <c>followupTask</c> have already been evaluated, and will not be checked further.
        /// </summary>
        /// <param name="initialTask">The <see cref="INestingTask"/> at the root of the <see cref="Planner.PlanNode"/>.</param>
        /// <param name="followupTask">The <see cref="Task"/> being nested inside <c>planTask.</c></param>
        public NestedTask(INestingTask initialTask, Task followupTask) : base(null, null, null, null) 
        {
            _initialTask = initialTask;
            _followupTask = followupTask;
        }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            return _followupTask.ChangeWorldState(_initialTask.ChangeWorldState(worldState));
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        {
            foreach(TaskAction action in _initialTask.GetActions(actor))
            {
                yield return action;
            }
            foreach (TaskAction action in _followupTask.GetActions(actor))
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
