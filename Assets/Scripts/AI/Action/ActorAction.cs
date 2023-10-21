using Assets.Scripts.AI.Step;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="TaskAction"/> class is the base class for actions for an <see cref="AI.Actor"/> consisting of potentially multiple <see cref="TaskStep"/>s.
    /// </summary>
    /// <summary>
    /// The <see cref="ActorAction"/> class is an abstract class for <see cref="TaskAction"/>s that require a reference to an <see cref="AI.Actor"/>.
    /// This limits the <see cref="ActorAction"/> to only be used by <see cref="AdventurerPawn"/>s.
    /// </summary>
    public abstract class ActorAction : TaskAction
    {
        protected Actor Actor;
        protected new AdventurerPawn Pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorAction"/> class.
        /// </summary>
        /// <param name="actor">The <see cref="AI.Actor"/> that is performed the <see cref="ActorAction"/>.</param>
        protected ActorAction(Actor actor) : base(actor.Pawn)
        {
            Actor = actor;
            Pawn = actor.Pawn;
        }
    }
}