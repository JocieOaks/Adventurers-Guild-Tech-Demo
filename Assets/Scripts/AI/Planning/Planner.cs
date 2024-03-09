using System;
using System.Collections.Generic;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.AI.Planning
{
    /// <summary>
    /// The <see cref="Planner"/> class is used to decide what <see cref="Task"/>s a <see cref="AdventurerPawn"/> should perform.
    /// </summary>
    public class Planner
    {
        private (PlanNode node, float utility) _best;

        private readonly PriorityQueue<PlanNode, float> _priorityQueue =
            new(PriorityQueue<PlanNode, float>.MaxComparer.Instance);

        private bool _reset;
        private readonly Actor.Actor _actor;

        private WorldState _startState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Planner"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> for whom the <see cref="Planner"/> is determining their actions.</param>
        /// <param name="startTask">The first task the <see cref="Actor"/> is performing, to set the initial <see cref="WorldState"/>.</param>
        public Planner(Actor.Actor actor, Task.Task startTask)
        {
            _startState = startTask.ChangeWorldState(new WorldState(actor));
            _startState.PreviousTask = startTask;
            _actor = actor;
            _reset = true;
        }

        /// <summary>
        /// Called every fixed update to build out the full possibility space for <see cref="Task"/>s the <see cref="Actor"/> could perform in order to find that <see cref="Task"/>s that provide the highest utility.
        /// Based on the principles behind the A* navigation algorithm.
        /// </summary>
        /// <returns>Returns <see cref="WaitForFixedUpdate"/> objects for the <c>StartCoroutine</c> function.</returns>
        public void AStar()
        {
            if (_reset)
            {
                _reset = false;
                _best = (null, float.NegativeInfinity);
                _priorityQueue.Clear();
                foreach (Task.Task task in GetTasks())
                {
                    if (!task.ConditionsMet(_startState)) continue;

                    PlanNode planNode = new(task, _startState);

                    float priority = planNode.GetAverageUtility();

                    if (_best.utility < priority)
                    {
                        _best = (planNode, priority);
                    }

                    _priorityQueue.Push(planNode, priority);
                }
            }
            else
            {
                if (_priorityQueue.Empty) return;
                PlanNode current = _priorityQueue.Pop();

                if (current.Depth >= 4) return;
                foreach (Task.Task task in GetTasks())
                {
                    if (!task.ConditionsMet(current.WorldState)) continue;

                    PlanNode planNode = new(current, task, current.WorldState);

                    float priority = planNode.GetAverageUtility();

                    if (_best.utility < priority)
                    {
                        _best = (planNode, priority);
                    }

                    _priorityQueue.Push(planNode, priority);
                }
            }
        }

        /// <summary>
        /// Gets the next <see cref="Task"/> for the <see cref="Actor"/> to perform.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/> for the <see cref="Actor"/> to perform.</returns>
        public Task.Task GetTask()
        {
            if (_actor.IsOnQuest)
                return new QuestTask();

            Task.Task task = _best.node.FirstTask;
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
        public void OverrideTask(Task.Task task)
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
        private static IEnumerable<Task.Task> GetTasks()
        {
            //yield return new SleepTask();
            //yield return new EatTask();
            yield return new WanderTask();
            //yield return new StanceLay();
            //yield return new StanceSit();
            //yield return new StanceStand();
            //yield return new AcquireFoodTask();
            yield return new WaitTask(5);
            //yield return new LeaveConversationTask();
        }

        /// <summary>
        /// The <see cref="PlanNode"/> class is a node for building out chains of <see cref="Task"/>s to determine the optimal <see cref="Task"/>s to perform.
        /// </summary>
        private class PlanNode
        {
            private readonly Task.Task _task;
            private readonly List<GetPayoffDelegate> _payoff;
            private readonly float _time;
            private readonly float _utility;

            /// <summary>
            /// Initializes a new instance of the <see cref="PlanNode"/> class.
            /// </summary>
            /// <param name="previous">The previous <see cref="Task"/> in the chain of <see cref="Task"/>s.</param>
            /// <param name="task">The <see cref="Task"/> to be performed.</param>
            /// <param name="startState">The <see cref="Planning.WorldState"/> before the <see cref="Task"/> is performed.</param>
            public PlanNode(PlanNode previous, Task.Task task, WorldState startState) : this(task, startState)
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
            /// <param name="startState">The <see cref="Planning.WorldState"/> before the <see cref="Task"/> is performed.</param>
            public PlanNode(Task.Task task, WorldState startState)
            {
                Depth = 1;
                Root = this;

                _task = task;
                _utility = task.Utility(startState);
                _time = task.Time(startState);

                switch (task)
                {
                    // IRiskyTask not yet implemented
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    case IRiskyTask riskyTask:
                    {
                        float probability = riskyTask.ProbabilityOfSuccess(startState);
                        float failUtility = riskyTask.FailureUtility(startState);
                        _utility = _utility * probability + failUtility * (1 - probability);
                        break;
                    }
                    case ISetupTask setup:
                        _payoff = new List<GetPayoffDelegate>() { setup.Payoff };
                        break;
                }

                WorldState = WorldStateDelta(startState, _time, task);

                _utility += UtilityDelta(startState, WorldState);
            }

            /// <value>The number of <see cref="Task"/>'s in the chain of <see cref="Task"/>s.</value>
            public int Depth { get; }

            /// <value>The first <see cref="Task"/> in the chain of <see cref="Task"/>s.</value>
            public Task.Task FirstTask => Root._task;

            /// <value>The first <see cref="PlanNode"/> in the chain.</value>
            private PlanNode Root { get; }

            /// <value>The predicted <see cref="Planning.WorldState"/> after the <see cref="Task"/> is performed.</value>
            public WorldState WorldState { get; }

            /// <summary>
            /// Creates a new <see cref="Planning.WorldState"/> based on a given <see cref="Task"/> and the passage of time.
            /// </summary>
            /// <param name="startState">The <see cref="Planning.WorldState"/> before the <see cref="Task"/> is performed.</param>
            /// <param name="time">The amount of time for the <see cref="Task"/> to be performed.</param>
            /// <param name="task">The <see cref="Task"/> modifying the <see cref="Planning.WorldState"/>.</param>
            /// <returns>Returns the predicted <see cref="Planning.WorldState"/>.</returns>
            public static WorldState WorldStateDelta(WorldState startState, float time, Task.Task task)
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (startState.Conversation == null)
                    startState.PrimaryActor.Social -= time / 5;
                else
                    startState.PrimaryActor.Social +=
                        time / startState.Conversation.PositionUtility(startState.ConversationDistance);

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

                WorldState prevState = WorldState;

                _payoff?.RemoveAll(PayoffUtility);


                return utility / time;

                bool PayoffUtility(GetPayoffDelegate payoff)
                {
                    Task.Task payoffTask = payoff(prevState);
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
            /// Calculates the baseline change in utility between two <see cref="Planning.WorldState"/>s, based on the change in the <see cref="Actor"/>'s needs.
            /// </summary>
            /// <param name="start">The initial <see cref="Planning.WorldState"/>.</param>
            /// <param name="end">The ending <see cref="Planning.WorldState"/>.</param>
            /// <returns></returns>
            private static float UtilityDelta(WorldState start, WorldState end)
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
}

