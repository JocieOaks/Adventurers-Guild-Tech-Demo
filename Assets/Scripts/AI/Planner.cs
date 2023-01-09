using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Profiling;

public class Planner
{
    (PlanNode node, float utility) best;

    PriorityQueue<PlanNode, float> priorityQueue = new PriorityQueue<PlanNode, float>(true);

    bool reset = false;

    WorldState startState;

    public Planner(Actor actor, Task startTask)
    {
        startState = startTask.ChangeWorldState(new WorldState(actor));
        startState.PreviousTask = startTask;
    }

    public IEnumerator AStar()
    {
        while (true)
        {
            reset = false;
            best = (null, float.NegativeInfinity);
            priorityQueue.Clear();
            foreach (Task task in GetTasks())
            {
                if (task.ConditionsMet(startState))
                {
                    PlanNode planNode = new PlanNode(task, startState);

                    float priority = planNode.GetAverageUtility();

                    if (best.utility < priority)
                    {
                        best = (planNode, priority);
                    }

                    priorityQueue.Push(planNode, priority);

                }
            }

            PlanNode current;
            while (!reset)
            {
                yield return new WaitForFixedUpdate();
                if (priorityQueue.Count > 0)
                {
                    current = priorityQueue.PopMax();
                    if (current.Depth < 4)
                    {
                        foreach (Task task in GetTasks())
                        {
                            WorldState worldState = current.WorldState;
                            if (task.ConditionsMet(worldState))
                            {

                                PlanNode planNode = new PlanNode(current, task, worldState);

                                float priority = planNode.GetAverageUtility();

                                if (best.utility < priority)
                                {
                                    best = (planNode, priority);
                                }

                                priorityQueue.Push(planNode, priority);
                            }
                        }
                    }
                }
            }
        }
    }

    public Task GetTask(Actor actor)
    {
        if (actor.IsOnQuest)
            return new QuestTask();

        Task task = best.node.FirstTask;
        WorldState currentState = new WorldState(actor);
        startState = PlanNode.WorldStateDelta(currentState, task.Time(currentState), task);
        startState.PreviousTask = task;
        reset = true;
        return task;
    }

    public void OverrideTask(Actor actor, Task task)
    {
        WorldState currentState = new WorldState(actor);
        startState = PlanNode.WorldStateDelta(currentState, task.Time(currentState), task);
        startState.PreviousTask = task;
        reset = true;
    }

    IEnumerable<Task> GetTasks()
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

    public class PlanNode
    {
        public Task _task;
        List<GetPayoffDelegate> _payoff;
        float _time;
        float _utility;
        WorldState _worldState;
        public PlanNode(PlanNode previous, Task task, WorldState startState) : this(task, startState)
        {
            Depth = previous.Depth + 1;
            Root = previous.Root;

            _utility += previous._utility;
            _time += previous._time;

            if (task is ISetup)
            {
                if (previous._payoff != null)
                    _payoff.AddRange(previous._payoff);
            }
            else
            {
                _payoff = previous._payoff;
            }
        }

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

            if (task is ISetup setup)
            {
                _payoff = new List<GetPayoffDelegate>() { setup.Payoff };
            }

            _worldState = WorldStateDelta(startState, _time, task);

            _utility += UtilityDelta(startState, _worldState);
        }

        public int Depth { get; }
        public Task FirstTask
        {
            get
            {
                return Root._task;
            }
        }

        public PlanNode Root { get; }
        public WorldState WorldState => _worldState;

        //Changes the world state based on the task.
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

        public float GetAverageUtility()
        {
            float utility = _utility;
            float time = _time;

            WorldState prevState = _worldState;

            if (_payoff != null)
            {
                _payoff.RemoveAll(x => PayoffUtility(x));
            }

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

        //Finds the amount of utility based on the changes between two world states based on Actor's needs.
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

