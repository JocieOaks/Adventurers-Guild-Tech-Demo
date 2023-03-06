/// <summary>
/// The <see cref="TaskAction"/> class is the base class for actions for an <see cref="Actor"/> consisting of potentially multipe <see cref="TaskStep"/>s.
/// </summary>
/// <summary>
/// The <see cref="ActorAction"/> class is an abstract class for <see cref="TaskAction"/>s that require a reference to an <see cref="Actor"/>.
/// This limits the <see cref="ActorAction"/> to only be used by <see cref="AdventurerPawn"/>s.
/// </summary>
public abstract class ActorAction : TaskAction
{
    protected Actor _actor;
    protected new AdventurerPawn _pawn;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> that is performind the <see cref="ActorAction"/>.</param>
    public ActorAction(Actor actor) : base(actor.Pawn)
    {
        _actor = actor;
        _pawn = actor.Pawn;
    }
}