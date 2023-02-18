using System.Collections.Generic;

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
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new TravelAction(foodSource.WorldPosition, actor);
        yield return new AcquireFoodAction(actor, foodSource);
    }

    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new TravelAction(foodSource.WorldPosition, actor);
        yield return new AcquireFoodAction(actor, foodSource);
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
