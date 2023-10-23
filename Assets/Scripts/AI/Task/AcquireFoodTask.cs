using System.Collections.Generic;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="AcquireFoodTask"/> class is a <see cref="Task"/> for having a <see cref="AdventurerPawn"/> get food.
    /// </summary>
    public class AcquireFoodTask : Task, ISetupTask, IRecoverableTask
    {
        /// <value>The list of all <see cref="IInteractable"/>s from which a <see cref="AdventurerPawn"/> can get food.</value>
        public static List<IInteractable> FoodSources = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AcquireFoodTask"/> class.
        /// </summary>
        public AcquireFoodTask() : base(null, true, null, null) { }

        /// <inheritdoc/>
        public override WorldState ChangeWorldState(WorldState worldState)
        {
            IInteractable foodSource = GetFoodSource(worldState.PrimaryActor);
            if (foodSource == null) return worldState;

            worldState.PrimaryActor.HasFood = true;
            worldState.PrimaryActor.Position = foodSource.WorldPosition;
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
        public override IEnumerable<TaskAction> GetActions(Actor.Actor actor)
        {
            IInteractable foodSource = GetFoodSource(actor.Stats);
            if (foodSource == null)
                yield break;
            yield return new TravelAction(foodSource, actor.Pawn);
            yield return new AcquireFoodAction(actor, foodSource);
        }

        /// <inheritdoc/>
        public IEnumerable<TaskAction> Recover(Actor.Actor actor, TaskAction action)
        {
            IInteractable foodSource = GetFoodSource(actor.Stats);
            if (foodSource == null)
                yield break;
            yield return new TravelAction(foodSource, actor.Pawn);
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
        /// Finds the nearest source of food to a <see cref="AdventurerPawn"/>.
        /// </summary>
        /// <param name="profile">The <see cref="ActorProfile"/> representing the <see cref="AdventurerPawn"/>.</param>
        /// <returns>Returns the nearest food source.</returns>
        private static IInteractable GetFoodSource(ActorProfile profile)
        {
            float closestDistance = float.PositiveInfinity;
            IInteractable best = null;
            foreach (IInteractable foodSource in FoodSources)
            {
                float distance = Map.Map.Instance.ApproximateDistance(profile.Position, foodSource.WorldPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    best = foodSource;
                }
            
            }
            return best;
        }
    }
}
