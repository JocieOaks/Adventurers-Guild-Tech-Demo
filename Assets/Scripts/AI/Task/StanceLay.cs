using System.Collections.Generic;

/// <summary>
/// The <see cref="StanceLay"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> transition into <see cref="Stance.Lay"/>.
/// </summary>
public class StanceLay : Task, IRecoverableTask
{
    BedSprite bed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StanceLay"/> class.
    /// </summary>
    public StanceLay() : base(null, true, null, null) { }

    /// <value>The list of all <see cref="IInteractable"/>s that can be laid on by a <see cref="AdventurerPawn"/>.</value>
    public static List<IInteractable> LayingObjects { get; } = new List<IInteractable>();

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool ConditionsMet(WorldState worldState)
    {
        return false;// base.ConditionsMet(worldState) && InteractablesCondition(worldState, LayingObjects) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceStand;
    }

    /// <inheritdoc/>
    public override IEnumerable<TaskAction> GetActions(Actor actor)
    { 
        bed = GetBed(actor.Stats);
        if (bed == null)
            yield break;
        yield return new TravelAction(bed, actor);
        yield return new LayDownAction(bed, actor);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
    {
        if(action is LayDownAction)
            bed = GetBed(actor.Stats);
        if (bed == null)
            yield break;
        yield return new TravelAction(bed, actor);
        yield return new LayDownAction(bed, actor);
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
    /// Finds the nearest bed to a <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="profile">The <see cref="ActorProfile"/> representing the <see cref="AdventurerPawn"/>.</param>
    /// <returns>Returns the nearest bed.</returns>
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
