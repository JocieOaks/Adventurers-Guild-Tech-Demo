﻿using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Action;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.Map.Sprite_Object.Furniture;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="StanceLay"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> transition into <see cref="Stance.Lay"/>.
    /// </summary>
    public class StanceLay : Task, IRecoverableTask, IPlayerTask
    {
        private BedSprite _bed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StanceLay"/> class.
        /// </summary>
        public StanceLay() : base(null, true, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StanceSit"/> class with a specified <see cref="BedSprite"/>.
        /// </summary>
        /// <param name="bed">The <see cref="BedSprite"/> to lay in.</param>
        public StanceLay(BedSprite bed) : base(null, true, null, null)
        {
            this._bed = bed;
        }

        /// <value>The list of all <see cref="IInteractable"/>s that can be laid on by a <see cref="AdventurerPawn"/>.</value>
        public static List<IInteractable> LayingObjects { get; } = new();

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            _bed = GetBed(worldState.PrimaryActor);
            if (_bed != null)
            {
                worldState.PrimaryActor.Position = _bed.WorldPosition;
                worldState.PrimaryActor.Stance = Stance.Lay;
            }
            return worldState;
        }

        /// <inheritdoc/>
        public override bool ConditionsMet(WorldState worldState)
        {
            return base.ConditionsMet(worldState) && InteractablesCondition(worldState, LayingObjects) && worldState.PreviousTask is not StanceSit && worldState.PreviousTask is not StanceStand;
        }

        /// <inheritdoc/>
        public override IEnumerable<TaskAction> GetActions(Actor actor)
        { 
            _bed = GetBed(actor.Stats);
            if (_bed == null)
                return Enumerable.Empty<TaskAction>();
            return GetActions(actor.Pawn);
        }

        /// <inheritdoc/>
        public IEnumerable<TaskAction> GetActions(Pawn pawn)
        {
            yield return new TravelAction(_bed, pawn);
            yield return new LayDownAction(_bed, pawn);
        }

        /// <inheritdoc/>
        public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action)
        {
            if(action is LayDownAction)
                _bed = GetBed(actor.Stats);
            if (_bed == null)
                yield break;
            yield return new TravelAction(_bed, actor.Pawn);
            yield return new LayDownAction(_bed, actor.Pawn);
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
        private BedSprite GetBed(ActorProfile profile)
        {
            float closestDistance = float.PositiveInfinity;
            BedSprite best = null;
            foreach (var interactable in LayingObjects)
            {
                var bed = (BedSprite)interactable;
                if (!bed.Occupied)
                {
                    float distance = Map.Map.Instance.ApproximateDistance(profile.Position, bed.WorldPosition);
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
}
