﻿using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;

namespace Assets.Scripts.AI.Navigation.Goal
{
    /// <summary>
    /// The <see cref="DestinationGoal"/> class is an <see cref="IGoal"/> for traveling to any <see cref="IInteractable"/> that a <see cref="Pawn"/> can sit on.
    /// </summary>
    public class SitGoal : IGoal
    {
        private static readonly List<RoomNode> s_endpoints = new();
        private static readonly List<IInteractable> s_sittingObjects = new();

        /// <value>The list of all <see cref="IInteractable"/>s from which a <see cref="Pawn"/> can sit on.</value>
        public static IEnumerable<IInteractable> SittingObjects => s_sittingObjects;

        /// <inheritdoc/>
        public IEnumerable<RoomNode> Endpoints => s_endpoints;

        /// <summary>
        /// Adds a new food source to the list of objects that can be sat on.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being added to the list of objects that can be sat on.</param>
        public static void AddSittingObject(IInteractable source)
        {
            s_sittingObjects.Add(source);
            s_endpoints.AddRange(source.InteractionPoints.Except(s_endpoints));
        }

        /// <summary>
        /// Removes an object from the list of objects that can be sat on.
        /// </summary>
        /// <param name="source">The <see cref="IInteractable"/> being removed from the list of objects that can be sat on.</param>
        public static void RemoveSittingObject(IInteractable source)
        {
            s_sittingObjects.Remove(source);
            s_endpoints.RemoveAll(endpoint =>
                !s_sittingObjects.Any(interactable => interactable.InteractionPoints.Any(node => node == endpoint)));
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
