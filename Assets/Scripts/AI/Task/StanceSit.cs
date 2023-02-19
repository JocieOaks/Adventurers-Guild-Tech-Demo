using System.Collections.Generic;

/// <summary>
/// The <see cref="StanceSit"/> class is a <see cref="Task"/> for having a <see cref="Pawn"/> transition into <see cref="Stance.Sit"/>.
/// </summary>
public class StanceSit : Task, IRecovery
{
    IOccupied seat;

    /// <summary>
    /// Initalizes a new instance of the <see cref="StanceSit"/> class.
    /// </summary>
    public StanceSit() : base(null, true, null, null) { }

    /// <value>The list of all <see cref="IInteractable"/>s that can be sat on.</value>
    public static List<IInteractable> SittingObjects { get; } = new List<IInteractable>();

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool ConditionsMet(WorldState worldState)
    {
        return base.ConditionsMet(worldState) && InteractablesCondition(worldState, SittingObjects) && worldState.PreviousTask is not StanceStand && worldState.PreviousTask is not StanceLay; ;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    {
        //Debug.Log(actor.Stats.Name + " Sit");
        seat = GetSeat(actor.Stats);
        if (seat == null)
            yield break;
        yield return new TravelAction(seat, actor);
        yield return new SitDownAction(seat, actor);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {   
        if(action is SitDownAction)
            seat = GetSeat(actor.Stats);
        if(seat == null)
            yield break;
        yield return new TravelAction(seat, actor);
        yield return new SitDownAction(seat, actor);
    }

    /// <inheritdoc/>
    public override float Time(WorldState worldState)
    {
        return 3;
    }

    /// <inheritdoc/>
    public override float Utility(WorldState worldState)
    {
        return 0;
    }

    /// <summary>
    /// Finds the nearest seat to a <see cref="Pawn"/>.
    /// </summary>
    /// <param name="profile">The <see cref="ActorProfile"/> representing the <see cref="Pawn"/>.</param>
    /// <returns>Returns the nearest seat.</returns>
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
}
