using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;

namespace Assets.Scripts.AI.Navigation.Goal
{
    /// <summary>
    /// The <see cref="DestinationGoal"/> class is an <see cref="IGoal"/> for traveling to any <see cref="IInteractable"/> that a <see cref="Pawn"/> can lay on.
    /// </summary>
    public class LayGoal : IGoal
    {
        private static readonly List<RoomNode> s_endpoints = new();
        private static readonly List<IInteractable> s_layingObjects = new();

        /// <value>The list of all <see cref="IInteractable"/>s that a <see cref="Pawn"/> can lay on.</value>
        public static IEnumerable<IInteractable> LayingObjects => s_layingObjects;

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints => s_endpoints;

        /// <summary>
        /// Adds a new food source to the list of objects that can be lain on.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being added to the list of objects that can be lain on.</param>
        public static void AddLayingObject(IInteractable source)
        {
            s_layingObjects.Add(source);
            if(Map.Map.Ready)
                s_endpoints.AddRange(source.InteractionPoints.Except(s_endpoints));
        }

        public static void OnMapReady()
        {
            foreach (IInteractable source in LayingObjects)
            {
                s_endpoints.AddRange(source.InteractionPoints.Except(s_endpoints));
            }
        }

        /// <summary>
        /// Removes an object from the list of objects that can be lain on.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being removed from the list of objects that can be lain on.</param>
        public static void RemoveLayingObject(IInteractable source)
        {
            s_layingObjects.Remove(source);
            s_endpoints.RemoveAll(endpoint =>
                !s_layingObjects.Any(interactable => interactable.InteractionPoints.Any(node => node == endpoint)));
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
