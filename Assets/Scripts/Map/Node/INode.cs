using System.Collections.Generic;
using Assets.Scripts.AI.Actor;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// Interface for connected points on a <see cref="Map"/>.
    /// Can be either a coordinate location on a map such as <see cref="RoomNode"/> or a connecting point between two coordinates such as <see cref="ConnectingNode"/>.
    /// </summary>
    public interface INode : IWorldPosition
    {
        /// <value>Iterates over all <see cref="INode"/>'s directly adjacent to this <see cref="INode"/>.</value>
        public IEnumerable<INode> AdjacentNodes { get; }

        /// <value>Returns true if the <see cref="INode"/> is blocked and cannot be passed through.</value>
        public bool Obstructed {get;}

        /// <value> Returns true if the <see cref="INode"/> can be passed through by a navigating <see cref="AdventurerPawn"/>.</value>
        public bool Traversable => !Obstructed;

        /// <summary>
        /// Determines if the given <see cref="RoomNode"/> is connected to the <see cref="INode"/>.
        /// For <see cref="RoomNode"/> this is an equivalence check.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> being tested.</param>
        /// <returns>Returns true if the <paramref name="node"/> is within proximity of the <see cref="INode"/>.</returns>
        public bool AdjacentToRoomNode(RoomNode node);

        //Obstructed and Traversable are typically opposites, but have subtle differences, specifically for RoomNodes.
        //Because Pawns take up a three by three area, all 9 RoomNode tiles that they are occupying must not be Obstructed in order for the Pawn to stand there.
        //Therefore, it is possible for a RoomNode to not be Obstructed, but also not be Traversable.
    }
}
