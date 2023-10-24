using System.Collections.Generic;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation.Goal
{
    /// <summary>
    /// The <see cref="IGoal"/> interface specifies a goal for navigation by a <see cref="Pawn"/>.
    /// </summary>
    public interface IGoal
    {
        /// <value>Iterates through the list of <see cref="RoomNode"/>s that when navigated to will complete the <see cref="IGoal"/>.</value>
        IEnumerable<RoomNode> Endpoints { get; }

        /// <summary>
        /// Estimates how close the given <see cref="RoomNode"/> is to one of the see <see cref="IGoal"/>'s <see cref="Endpoints"/>.
        /// </summary>
        /// <param name="start">The <see cref="RoomNode"/> whose proximity to the goal is being estimated.</param>
        /// <returns>Returns an estimate of the distance traveled by a <see cref="Pawn"/> to complete the <see cref="IGoal"/>.</returns>
        float Heuristic(RoomNode start);

        /// <summary>
        /// Determines if the given <see cref="RoomNode"/> upon being traversed to, would signify the completion of the <see cref="IGoal"/>.
        /// </summary>
        /// <param name="position">The <see cref="RoomNode"/> being evaluated.</param>
        /// <returns>Returns true if the <see cref="IGoal"/> is complete.</returns>
        bool IsComplete(RoomNode position);
    }
}
