using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;

namespace Assets.Scripts.AI.Navigation.Destination
{
    /// <summary>
    /// The <see cref="TargetDestination"/> class is an <see cref="IDestination"/> for traveling to any <see cref="IInteractable"/> from which a <see cref="Pawn"/> can acquire food.
    /// </summary>
    public class FoodDestination : IDestination
    {
        private static readonly List<RoomNode> s_endpoints = new();
        private static readonly List<IInteractable> s_foodSources = new();
        private static readonly List<Room> s_endRooms = new();

        /// <value>The list of all <see cref="IInteractable"/>s from which a <see cref="AdventurerPawn"/> can get food.</value>
        public static IEnumerable<IInteractable> FoodSources => s_foodSources;

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints => s_endpoints.Where(node => node.Traversable);

        /// <inheritdoc/>
        public IEnumerable<Room> EndRooms => s_endRooms;

        /// <summary>
        /// Adds a new food source to the list of potential food sources.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being added to the list of food sources.</param>
        public static void AddFoodSource(IInteractable source)
        {
            s_foodSources.Add(source);
            if (Map.Map.Ready)
            {
                s_endpoints.AddRange(source.InteractionPoints.Except(s_endpoints));
                if (!s_endRooms.Contains(source.Room))
                {
                    s_endRooms.Add(source.Room);
                }
            }
        }

        /// <summary>
        /// Sets up the <see cref="FoodDestination"/> endpoints once the map is ready.
        /// </summary>
        public static void OnMapReady()
        {
            foreach (IInteractable source in FoodSources)
            {
                s_endpoints.AddRange(source.InteractionPoints.Except(s_endpoints));
                if (!s_endRooms.Contains(source.Room))
                {
                    s_endRooms.Add(source.Room);
                }
            }
        }

        /// <summary>
        /// Removes a new food source to the list of potential food sources.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being removed from the list of food sources.</param>
        public static void RemoveFoodSource(IInteractable source)
        {
            s_foodSources.Remove(source);
            s_endpoints.RemoveAll(endpoint =>
                !s_foodSources.Any(interactable => interactable.InteractionPoints.Any(node => node == endpoint)));
            if (s_foodSources.All(interactable => interactable.Room != source.Room))
                s_endRooms.Remove(source.Room);
        }

        /// <inheritdoc/>
        public float Heuristic(RoomNode start)
        {
            float min = float.PositiveInfinity;
            foreach (RoomNode node in s_endpoints)
            {
                if (!node.Traversable || node.Room != start.Room) continue;
                float distance = Map.Map.EstimateDistance(start, node);
                if (distance < min) min = distance;
            }
            return min;
        }

        /// <inheritdoc/>
        public bool IsComplete(RoomNode position)
        {
            return s_endpoints.Contains(position);
        }
    }
}
