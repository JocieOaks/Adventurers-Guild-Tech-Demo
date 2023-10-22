using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Action;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="StanceSit"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> transition into <see cref="Stance.Sit"/>.
    /// </summary>
    public class StanceSit : Task, IRecoverableTask, IPlayerTask
    {
        private IOccupied _seat;

        /// <summary>
        /// Initializes a new instance of the <see cref="StanceSit"/> class.
        /// </summary>
        public StanceSit() : base(null, true, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StanceSit"/> class with a specified seat.
        /// </summary>
        /// <param name="seat">The <see cref="IOccupied"/> to be sat in.</param>
        public StanceSit(IOccupied seat) : base(null, true, null, null)
        {
            this._seat = seat;
        }

        /// <value>The list of all <see cref="IInteractable"/>s that can be sat on.</value>
        public static List<IInteractable> SittingObjects { get; } = new();

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            _seat = GetSeat(worldState.PrimaryActor);
            if (_seat != null)
            {
                worldState.PrimaryActor.Position = _seat.WorldPosition;
                worldState.PrimaryActor.Stance = Stance.Sit;
            }
            return worldState;
        }

        /// <inheritdoc/>
        public override bool ConditionsMet(WorldState worldState)
        {
            return base.ConditionsMet(worldState) && InteractablesCondition(worldState, SittingObjects) && worldState.PreviousTask is not StanceStand && worldState.PreviousTask is not StanceLay;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        {
            _seat = GetSeat(actor.Stats);
            return _seat == null ? Enumerable.Empty<TaskAction>() : GetActions(actor.Pawn);
        }

        /// <inheritdoc/>
        public IEnumerable<TaskAction> GetActions(Pawn pawn)
        {
            yield return new TravelAction(_seat, pawn);
            yield return new SitDownAction(_seat, pawn);
        }

        /// <inheritdoc/>
        public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
        {   
            if(action is SitDownAction)
                _seat = GetSeat(actor.Stats);
            if(_seat == null)
                yield break;
            yield return new TravelAction(_seat, actor.Pawn);
            yield return new SitDownAction(_seat, actor.Pawn);
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
        private static IOccupied GetSeat(ActorProfile profile)
        {
            float closestDistance = float.PositiveInfinity;
            IOccupied best = null;
            foreach (IInteractable interactable in SittingObjects)
            {
                var chair = (IOccupied)interactable;
                if (!chair.Occupied)
                {
                    float distance = Map.Map.Instance.ApproximateDistance(profile.Position, chair.WorldPosition);
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
}
