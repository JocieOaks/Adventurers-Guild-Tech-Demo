using System.Collections.Generic;

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
        yield return new TravelAction(seat.WorldPosition, actor);
        yield return new SitDownAction(seat, actor);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {   
        if(action is SitDownAction)
            seat = GetSeat(actor.Stats);
        if(seat == null)
            yield break;
        yield return new TravelAction(seat.WorldPosition, actor);
        yield return new SitDownAction(seat, actor);
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
