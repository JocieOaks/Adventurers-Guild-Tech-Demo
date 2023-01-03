using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Task
{
    protected abstract bool? _sitting { get; }
    protected abstract bool? _standing { get; }
    protected abstract bool? _laying { get; }
    protected abstract bool? _conversing { get; }


    /// <summary>
    /// Checks the worldState to see if the Tasks conditions (if it has any) are met.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>Returns true if conditions are met by the worldState.</returns>
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

    protected static bool InteractablesCondition(WorldState worldState, List<IInteractable> interactables)
    {

        return interactables.Any(x => x.GetInteractionPoints().Any(y => y.Traversible)) && (worldState.Conversation == null || interactables.Any(x => worldState.Conversation.InRadius(x.WorldPosition)));
    }

    /// <summary>
    /// Gives the Utility value of a task.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>Returns the utility of success and failure. If there is no failure state, both values will be the same.</returns>
    public abstract float Utility(WorldState worldState);

    /// <summary>
    /// Gives the estimated time for the Task to be completed.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>The estimated time for the Task to be completed.</returns>
    public abstract float Time(WorldState worldState);

    /// <summary>
    /// Creates an estimate of the worldState for after the Task has been performed.
    /// </summary>
    /// <param name="worldState">The current presumed world state for when the task will occur.</param>
    /// <returns>A new world state based on the expected results of performing the task.</returns>
    public abstract WorldState ChangeWorldState(WorldState worldState);

    public abstract IEnumerable<TaskAction> GetActions(Actor actor);
}

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

public delegate Task GetPayoffDelegate(WorldState worldState);

public interface ISetup
{
    public GetPayoffDelegate Payoff => ConstructPayoff;

    public Task ConstructPayoff(WorldState worldState);
}

public interface IRecovery
{
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action);
}

public class RestTask : Task
{
    Task sleep;

    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public RestTask(WorldState worldState)
    {
        sleep = new SleepTask();
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        return sleep.ChangeWorldState(worldState);
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return sleep.ConditionsMet(worldState);
    }

    public override float Time(WorldState worldState)
    {
        return sleep.Time(worldState);
    }

    public override float Utility(WorldState worldState)
    {
        return sleep.Utility(worldState);
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        foreach (TaskAction action in sleep.GetActions(actor))
            yield return action;
    }
}

class SleepTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => false;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Sleep = 10f;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Sleep " + actor.Stats.Sleep);
        yield return new Sleeping(actor);
    }
    public override float Time(WorldState worldState)
    {
        return Mathf.Max(20, (10 - worldState.PrimaryActor.Sleep) * 5);
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Sit)
            return -5 * Time(worldState);
        return 0;
    }
}

class GoToTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;

    Vector3Int _destination;
    SpriteObject _targetObject;
    public GoToTask(Vector3Int destination)
    {
        _destination = destination;
    }

    public GoToTask(SpriteObject target)
    {
        _destination = target.WorldPosition;
        _targetObject = target;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = _destination;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return (new Traveling(_destination, actor));
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        yield break;
    }

    public override float Time(WorldState worldState)
    {
        return Map.Instance.ApproximateDistance(worldState.PrimaryActor.Position, _destination) / worldState.PrimaryActor.Speed;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}

class EatTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Hunger = 10;
        worldState.PrimaryActor.HasFood = false;
        return worldState;
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return worldState.PrimaryActor.HasFood;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Eat " + actor.Stats.Hunger);
        yield return new Eating(actor);
    }

    public override float Time(WorldState worldState)
    {
        return Mathf.Max(5, (10 - worldState.PrimaryActor.Hunger) * 1.5f);
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Stand)
            return -5 * Time(worldState);
        else if (worldState.PrimaryActor.Stance == Stance.Lay)
            return -10 * Time(worldState);
        return 0;
    }
}

public class WanderTask : Task
{
    RoomNode node;

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => false;
    public WanderTask()
    {
        do
        {
            node = Map.Instance[Random.Range(0, Map.Instance.MapWidth), Random.Range(0, Map.Instance.MapLength), 0, Random.Range(0, 2)];
        } while (!node.Traversible);
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = node.WorldPosition;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new Traveling(node.WorldPosition, actor);
    }

    public override float Time(WorldState worldState)
    {
        return 10;
    }

    public override float Utility(WorldState worldState)
    {
        return 10;
    }
}


public class StanceLay : Task, /*ISetup,*/ IRecovery
{
    public static List<IInteractable> LayingObjects { get; } = new List<IInteractable>();

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public Task ConstructPayoff(WorldState worldState)
    {
        return new RestTask(worldState);
    }

    Bed bed;
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        bed = GetBed(worldState.PrimaryActor);
        if (bed != null)
        {
            worldState.PrimaryActor.Position = bed.WorldPosition;
            worldState.PrimaryActor.Stance = Stance.Lay;
        }
        return worldState;
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return base.ConditionsMet(worldState) && InteractablesCondition(worldState, LayingObjects) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceStand; ;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Lay");
        bed = GetBed(actor.Stats);
        if (bed == null)
            yield break;
        yield return new Traveling(bed, actor);
        yield return new LayingDown(bed, actor);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        if(action is LayingDown)
            bed = GetBed(actor.Stats);
        if (bed == null)
            yield break;
        yield return new Traveling(bed, actor);
        yield return new LayingDown(bed, actor);
    }

    public override float Time(WorldState worldState)
    {
        return 3;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    Bed GetBed(ActorProfile profile)
    {
        float closestDistance = float.PositiveInfinity;
        Bed best = null;
        foreach (Bed bed in LayingObjects)
        {
            if (!bed.Occupied)
            {
                float distance = Map.Instance.ApproximateDistance(profile.Position, bed.WorldPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    best = bed;
                }
            }
        }
        return best;
    }
}

public class StanceSit : Task, /*ISetup,*/  IRecovery
{
    public static List<IInteractable> SittingObjects { get; } = new List<IInteractable>();

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;


    IOccupied seat;
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        seat = GetSeat(worldState.PrimaryActor);
        if (seat != null)
        {
            worldState.PrimaryActor.Position = seat.WorldPosition;
            worldState.PrimaryActor.Stance = Stance.Sit;
        }
        return worldState;
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return base.ConditionsMet(worldState) && InteractablesCondition(worldState, SittingObjects) && worldState.PreviousTask is not StanceStand && worldState.PreviousTask is not StanceLay; ;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Sit");
        seat = GetSeat(actor.Stats);
        if (seat == null)
            yield break;
        yield return new Traveling(seat.WorldPosition, actor);
        yield return new SittingDown(seat, actor);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {   
        if(action is SittingDown)
            seat = GetSeat(actor.Stats);
        if(seat == null)
            yield break;
        yield return new Traveling(seat.WorldPosition, actor);
        yield return new SittingDown(seat, actor);
    }

    public override float Time(WorldState worldState)
    {
        return 3;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    IOccupied GetSeat(ActorProfile profile)
    {
        float closestDistance = float.PositiveInfinity;
        IOccupied best = null;
        foreach (IOccupied chair in SittingObjects)
        {
            if (!chair.Occupied)
            {
                float distance = Map.Instance.ApproximateDistance(profile.Position, chair.WorldPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    best = chair;
                }
            }
        }
        return best;
    }

    public Task ConstructPayoff(WorldState worldState)
    {
        return new RestTask(worldState);
    }
}

public class StanceStand : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => false;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override bool ConditionsMet(WorldState worldState)
    {
        return base.ConditionsMet(worldState) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceLay;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Stance = Stance.Stand;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Stand");
        yield return new StandingUp(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 1;
    }

    public override float Utility(WorldState worldState)
    {
        if (worldState.PrimaryActor.Stance == Stance.Lay)
            return -1 * (10 -worldState.PrimaryActor.Sleep);
        else
            return -0.5f * (10 - worldState.PrimaryActor.Sleep);
    }
}

public class WaitTask : Task
{
    float _time;

    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public WaitTask(float time)
    {
        _time = time;
    }

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new Waiting(actor, _time);
    }

    public override float Time(WorldState worldState)
    {
        return _time;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}

public class AcquireFoodTask : Task, ISetup, IRecovery
{

    public static List<IInteractable> FoodSources = new List<IInteractable>();

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.HasFood = true;
        worldState.PrimaryActor.Position = GetFoodSource(worldState.PrimaryActor).WorldPosition;
        return worldState;
    }

    public override bool ConditionsMet(WorldState worldState)
    {
        return !worldState.PrimaryActor.HasFood && base.ConditionsMet(worldState) && InteractablesCondition(worldState, FoodSources);
    }

    public Task ConstructPayoff(WorldState worldState)
    {
        return new EatTask();
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Food");
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new Traveling(foodSource.WorldPosition, actor);
        yield return new AcquiringFood(actor, foodSource);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new Traveling(foodSource.WorldPosition, actor);
        yield return new AcquiringFood(actor, foodSource);
    }

    public override float Time(WorldState worldState)
    {
        return 10;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    IInteractable GetFoodSource(ActorProfile profile)
    {
        float closestDistance = float.PositiveInfinity;
        IInteractable best = null;
        foreach (IInteractable foodSource in FoodSources)
        {
            float distance = Map.Instance.ApproximateDistance(profile.Position, foodSource.WorldPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                best = foodSource;
            }
            
        }
        return best;
    }
}

public class ApproachTask : Task
{

    protected override bool? _sitting => null;

    protected override bool? _standing => true;

    protected override bool? _laying => null;

    protected override bool? _conversing => true;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.ConversationDistance = 2;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        Debug.Log(actor.Stats.Name + " Approach");
        yield return new Approaching(actor, actor.Pawn.Social.Conversation);
    }

    public override float Time(WorldState worldState)
    {
        return Mathf.Abs(worldState.ConversationDistance - 2) * 2 / worldState.PrimaryActor.Speed;
    }

    public override float Utility(WorldState worldState)
    {
        return -5;
    }
}

public class LeaveConversationTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => true;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.Conversation = null;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        Debug.Log(actor.Stats.Name + " Leaving Conversation");
        yield return new LeavingConversation(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 0.5f;
    }

    public override float Utility(WorldState worldState)
    {
        return -Mathf.Exp( 100 - worldState.Conversation.Duration) - 10;
    }
}

public class QuestTask : Task
{
    protected override bool? _sitting => null;

    protected override bool? _standing => null;

    protected override bool? _laying => null;

    protected override bool? _conversing => null;

    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.Position = Vector3Int.one;
        return worldState;
    }

    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        yield return new Traveling(Vector3Int.one, actor);
        yield return new Questing(actor);
    }

    public override float Time(WorldState worldState)
    {
        return 0;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }
}