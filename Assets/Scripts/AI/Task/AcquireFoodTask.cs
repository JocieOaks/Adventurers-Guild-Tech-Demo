using System.Collections.Generic;

/// <summary>
/// The <see cref="AcquireFoodTask"/> class is a <see cref="Task"/> for having a <see cref="Pawn"/> get food.
/// </summary>
public class AcquireFoodTask : Task, ISetupTask, IRecoverableTask
{
    /// <value>The list of all <see cref="IInteractable"/>s from which a <see cref="Pawn"/> can get food.</value>
    public static List<IInteractable> FoodSources = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AcquireFoodTask"/> class.
    /// </summary>
    public AcquireFoodTask() : base(null, true, null, null) { }

    /// <inheritdoc/>
    public override WorldState ChangeWorldState(WorldState worldState)
    {
        worldState.PrimaryActor.HasFood = true;
        worldState.PrimaryActor.Position = GetFoodSource(worldState.PrimaryActor).WorldPosition;
        return worldState;
    }

    /// <inheritdoc/>
    public override bool ConditionsMet(WorldState worldState)
    {
        return !worldState.PrimaryActor.HasFood && base.ConditionsMet(worldState) && InteractablesCondition(worldState, FoodSources);
    }

    /// <inheritdoc/>
    public Task ConstructPayoff(WorldState worldState)
    {
        return new EatTask();
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new TravelAction(foodSource.WorldPosition, actor);
        yield return new AcquireFoodAction(actor, foodSource);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        IInteractable foodSource = GetFoodSource(actor.Stats);
        if (foodSource == null)
            yield break;
        yield return new TravelAction(foodSource.WorldPosition, actor);
        yield return new AcquireFoodAction(actor, foodSource);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return 10;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    /// <summary>
    /// Finds the nearest source of food to a <see cref="Pawn"/>.
    /// </summary>
    /// <param name="profile">The <see cref="ActorProfile"/> representing the <see cref="Pawn"/>.</param>
    /// <returns>Returns the nearest food source.</returns>
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
