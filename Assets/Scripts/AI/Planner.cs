using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The <see cref="Planner"/> class is used to decide what <see cref="Task"/>s a <see cref="AdventurerPawn"/> should perform.
/// </summary>
public class Planner
{
    (PlanNode node, float utility) _best;
    readonly PriorityQueue<PlanNode, float> _priorityQueue = new(true);

    bool _reset = false;
    readonly Actor _actor;

    WorldState _startState;

    /// <summary>
    /// Initializes a new instance of the <see cref="Planner"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> for whom the <see cref="Planner"/> is determining their actions.</param>
    /// <param name="startTask">The first task the <see cref="Actor"/> is performing, to set the initial <see cref="WorldState"/>.</param>
    public Planner(Actor actor, Task startTask)
    {
        _startState = startTask.ChangeWorldState(new WorldState(actor));
        _startState.PreviousTask = startTask;
        _actor = actor;
    }

    /// <summary>
    /// Called every fixed update to build out the full possibility space for <see cref="Task"/>s the <see cref="Actor"/> could perform in order to find that <see cref="Task"/>s that provide the highest utility.
    /// Based on the principles behind the A* navigation algorithm.
    /// </summary>
    /// <returns>Returns <see cref="WaitForFixedUpdate"/> objects for the <c>StartCoroutine</c> function.</returns>
    public IEnumerator AStar()
    {
        while (true)
        {
            _reset = false;
            _best = (null, float.NegativeInfinity);
            _priorityQueue.Clear();
            foreach (Task task in GetTasks())
            {
                if (task.ConditionsMet(_startState))
                {
                    PlanNode planNode = new(task, _startState);

                    float priority = planNode.GetAverageUtility();

                    if (_best.utility < priority)
                    {
                        _best = (planNode, priority);
                    }

                    _priorityQueue.Push(planNode, priority);

                }
            }

            PlanNode current;
            while (!_reset)
            {
                yield return new WaitForFixedUpdate();
                if (_priorityQueue.Count > 0)
                {
                    current = _priorityQueue.Pop();
                    if (current.Depth < 4)
                    {
                        foreach (Task task in GetTasks())
                        {
                            WorldState worldState = current.WorldState;
                            if (task.ConditionsMet(worldState))
                            {

                                PlanNode planNode = new(current, task, worldState);

                                float priority = planNode.GetAverageUtility();

                                if (_best.utility < priority)
                                {
                                    _best = (planNode, priority);
                                }

                                _priorityQueue.Push(planNode, priority);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the next <see cref="Task"/> for the <see cref="Actor"/> to perform.
    /// </summary>
    /// <returns>Returns the <see cref="Task"/> for the <see cref="Actor"/> to perform.</returns>
    public Task GetTask()
    {
        if (_actor.IsOnQuest)
            return new QuestTask();

        Task task = _best.node.FirstTask;
        WorldState currentState = new(_actor);
        _startState = PlanNode.WorldStateDelta(currentState, task.Time(currentState), task);
        _startState.PreviousTask = task;
        _reset = true;
        return task;
    }

    /// <summary>
    /// Resets the <see cref="Planner"/> for when the <see cref="Actor"/> was forced to perform a particular <see cref="Task"/>.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> the <see cref="Actor"/> was forced to perform.</param>
    public void OverrideTask(Task task)
    {
        WorldState currentState = new(_actor);
        _startState = PlanNode.WorldStateDelta(currentState, task.Time(currentState), task);
        _startState.PreviousTask = task;
        _reset = true;
    }

    /// <summary>
    /// Gets the list of all <see cref="Task"/>s the <see cref="Actor"/> can perform.
    /// </summary>
    /// <returns>Iterates over all the <see cref="Task"/>s the <see cref="Actor"/> perform.</returns>
    IEnumerable<ITask> GetTasks()
    {
        yield return new SleepTask();
        yield return new EatTask();
        yield return new WanderTask();
        yield return new StanceLay();
        yield return new StanceSit();
        yield return new StanceStand();
        yield return new AcquireFoodTask();
        yield return new WaitTask(5);
        yield return new LeaveConversationTask();
    }

    /// <summary>
    /// The <see cref="PlanNode"/> class is a node for building out chains of <see cref="Task"/>s to determine the optimal <see cref="Task"/>s to perform.
    /// </summary>
    class PlanNode
    {
        public Task _task;
        readonly List<GetPayoffDelegate> _payoff;
        readonly float _time;
        readonly float _utility;
        WorldState _worldState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanNode"/> class.
        /// </summary>
        /// <param name="previous">The previous <see cref="Task"/> in the chain of <see cref="Task"/>s.</param>
        /// <param name="task">The <see cref="Task"/> to be performed.</param>
        /// <param name="startState">The <see cref="global::WorldState"/> before the <see cref="Task"/> is performed.</param>
        public PlanNode(PlanNode previous, Task task, WorldState startState) : this(task, startState)
        {
            Depth = previous.Depth + 1;
            Root = previous.Root;

            if (FirstTask is INestingTask planTask)
            {
                _task = planTask.CreateNestedTask(task);
                Root = this;
            }

            _utility += previous._utility;
            _time += previous._time;

            if (task is ISetupTask)
            {
                if (previous._payoff != null)
                    _payoff.AddRange(previous._payoff);
            }
            else
            {
                _payoff = previous._payoff;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanNode"/> class, starting a new chain of <see cref="Task"/>s.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to be performed.</param>
        /// <param name="startState">The <see cref="global::WorldState"/> before the <see cref="Task"/> is performed.</param>
        public PlanNode(Task task, WorldState startState)
        {
            Depth = 1;
            Root = this;

            _task = task;
            _utility = task.Utility(startState);
            _time = task.Time(startState);

            if (task is IRiskyTask riskyTask)
            {
                float probability = riskyTask.ProbabilityOfSuccess(startState);
                float failUtility = riskyTask.FailureUtility(startState);
                _utility = _utility * probability + failUtility * (1 - probability);
            }

            if (task is ISetupTask setup)
            {
                _payoff = new List<GetPayoffDelegate>() { setup.Payoff };
            }

            _worldState = WorldStateDelta(startState, _time, task);

            _utility += UtilityDelta(startState, _worldState);
        }

        /// <value>The number of <see cref="Task"/>'s in the chain of <see cref="Task"/>s.</value>
        public int Depth { get; }

        /// <value>The first <see cref="Task"/> in the chain of <see cref="Task"/>s.</value>
        public Task FirstTask
        {
            get
            {
                return Root._task;
            }
        }

        /// <value>The first <see cref="PlanNode"/> in the chain.</value>
        public PlanNode Root { get; }

        /// <value>The predicted <see cref="global::WorldState"/> after the <see cref="Task"/> is performed.</value>
        public WorldState WorldState => _worldState;

        /// <summary>
        /// Creates a new <see cref="global::WorldState"/> based on a given <see cref="Task"/> and the passage of time.
        /// </summary>
        /// <param name="startState">The <see cref="global::WorldState"/> before the <see cref="Task"/> is performed.</param>
        /// <param name="time">The amount of time for the <see cref="Task"/> to be performed.</param>
        /// <param name="task">The <see cref="Task"/> modifying the <see cref="global::WorldState"/>.</param>
        /// <returns>Returns the predicted <see cref="global::WorldState"/>.</returns>
        public static WorldState WorldStateDelta(WorldState startState, float time, Task task)
        {
            startState.PrimaryActor.Hunger -= time / 10;
            switch (startState.PrimaryActor.Stance)
            {
                case Stance.Stand:
                    startState.PrimaryActor.Sleep -= Time.deltaTime / 30;
                    break;
                case Stance.Sit:
                    startState.PrimaryActor.Sleep += Time.deltaTime / 60;
                    break;
                case Stance.Lay:
                    startState.PrimaryActor.Sleep += Time.deltaTime / 30;
                    break;
            }

            if (startState.Conversation == null)
                startState.PrimaryActor.Social -= time / 5;
            else
                startState.PrimaryActor.Social += time / startState.Conversation.PositionUtility(startState.ConversationDistance);

            startState.PreviousTask = task;
            return task.ChangeWorldState(startState);
        }

        /// <summary>
        /// Evaluates the average utility relative to time for the full chain of <see cref="Task"/>s up to this <see cref="PlanNode"/>.
        /// </summary>
        /// <returns>Returns the average utility of the full chain of <see cref="PlanNode"/>s that ends with this <see cref="PlanNode"/>.</returns>
        public float GetAverageUtility()
        {
            float utility = _utility;
            float time = _time;

            WorldState prevState = _worldState;

            _payoff?.RemoveAll(x => PayoffUtility(x));
            

            return utility / time;

            bool PayoffUtility(GetPayoffDelegate payoff)
            {
                Task payoffTask = payoff(prevState);
                if (payoffTask.ConditionsMet(prevState))
                {
                    float payoffUtility = payoffTask.Utility(prevState);
                    float payoffTime = payoffTask.Time(prevState);

                    WorldState nextState = WorldStateDelta(prevState, payoffTime, payoffTask);
                    payoffUtility += UtilityDelta(prevState, nextState);

                    if (payoffUtility / payoffTime > utility / time)
                    {
                        utility += payoffUtility;
                        time += payoffTime;
                        prevState = nextState;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Calculates the baseline change in utility between two <see cref="global::WorldState"/>s, based on the change in the <see cref="Actor"/>'s needs.
        /// </summary>
        /// <param name="start">The initial <see cref="global::WorldState"/>.</param>
        /// <param name="end">The ending <see cref="global::WorldState"/>.</param>
        /// <returns></returns>
        static float UtilityDelta(WorldState start, WorldState end)
        {
            float utility = 0;

            float sleepStart = start.PrimaryActor.Sleep;
            float sleepEnd = end.PrimaryActor.Sleep;
            utility -= Mathf.Pow(10, 2 - sleepEnd) - Mathf.Pow(10, 2 - sleepStart);

            float hungerStart = start.PrimaryActor.Hunger;
            float hungerEnd = end.PrimaryActor.Hunger;
            utility -= Mathf.Pow(10, 2 - hungerEnd) - Mathf.Pow(10, 2 - hungerStart);

            float socialStart = start.PrimaryActor.Social;
            float socialEnd = end.PrimaryActor.Social;
            utility -= Mathf.Pow(10, 2 - socialEnd) - Mathf.Pow(10, 2 - socialStart);

            return utility;
        }
    }
}

