using System.Collections.Generic;

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

    BedSprite bed;
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
        yield return new TravelAction(bed, actor);
        yield return new LayDownAction(bed, actor);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        if(action is LayDownAction)
            bed = GetBed(actor.Stats);
        if (bed == null)
            yield break;
        yield return new TravelAction(bed, actor);
        yield return new LayDownAction(bed, actor);
    }

    public override float Time(WorldState worldState)
    {
        return 3;
    }

    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    BedSprite GetBed(ActorProfile profile)
    {
        float closestDistance = float.PositiveInfinity;
        BedSprite best = null;
        foreach (BedSprite bed in LayingObjects)
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
