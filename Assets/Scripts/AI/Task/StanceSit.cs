using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The <see cref="StanceSit"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> transition into <see cref="Stance.Sit"/>.
/// </summary>
public class StanceSit : Task, IRecoverableTask, IPlayerTask
{
    IOccupied seat;

    /// <summary>
    /// Initalizes a new instance of the <see cref="StanceSit"/> class.
    /// </summary>
    public StanceSit() : base(null, true, null, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StanceSit"/> class with a specified seat.
    /// </summary>
    /// <param name="seat">The <see cref="IOccupied"/> to be sat in.</param>
    public StanceSit(IOccupied seat) : base(null, true, null, null)
    {
        this.seat = seat;
    }

    /// <value>The list of all <see cref="IInteractable"/>s that can be sat on.</value>
    public static List<IInteractable> SittingObjects { get; } = new();

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
        seat = GetSeat(actor.Stats);
        if (seat == null)
            return Enumerable.Empty<TaskAction>();
        return GetActions(actor.Pawn);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskAction> GetActions(Pawn pawn)
    {
        yield return new TravelAction(seat, pawn);
        yield return new SitDownAction(seat, pawn);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {   
        if(action is SitDownAction)
            seat = GetSeat(actor.Stats);
        if(seat == null)
            yield break;
        yield return new TravelAction(seat, actor.Pawn);
        yield return new SitDownAction(seat, actor.Pawn);
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
    /// Finds the nearest seat to a <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="profile">The <see cref="ActorProfile"/> representing the <see cref="AdventurerPawn"/>.</param>
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
