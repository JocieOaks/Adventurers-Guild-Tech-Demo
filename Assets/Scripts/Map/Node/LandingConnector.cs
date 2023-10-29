using Assets.Scripts.AI.Step;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// Class <see cref="LandingConnector"/> is a <see cref="ConnectingNode"/> that connects <see cref="Room"/>s and <see cref="RoomNode"/>'s that are on two different levels.
    /// </summary>
    public class LandingConnector : ConnectingNode, IDirected
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingConnector"/> class.
        /// </summary>
        /// <param name="connection1">The <see cref="StairNode"/> that produced the <see cref="LandingConnector"/>.</param>
        /// <param name="direction">The <see cref="Scripts.Map.Direction"/> the <see cref="StairNode"/> is facing.</param>
        public LandingConnector(StairNode connection1, Direction direction) : base(connection1, Map.Instance[connection1.WorldPosition + Vector3Int.forward + Utility.Utility.DirectionToVector(direction)], Vector3Int.zero)
        {
            FirstNode.SetNode(direction, this);
            SecondNode.SetNode(~direction, this);
            Direction = direction;

            switch (direction)
            {
                case Direction.North:
                    WorldPosition = SecondNode.WorldPosition;
                    break;
                case Direction.South:
                    WorldPosition = FirstNode.WorldPosition;
                    break;
                case Direction.East:
                    WorldPosition = SecondNode.WorldPosition;
                    break;
                case Direction.West:
                    WorldPosition = FirstNode.WorldPosition;
                    break;
            }
        }

        /// <inheritdoc/>
        public Direction Direction { get; }

        /// <inheritdoc/>
        public override bool Obstructed => false;

        /// <inheritdoc/>
        public override Vector3Int Dimensions => Vector3Int.zero;

        /// <inheritdoc/>
        public override void Disconnect()
        {
            FirstNode.ResetConnection(Direction);
            SecondNode.ResetConnection(~Direction);
            FirstNode.Room.RemoveConnection(this);
            SecondNode.Room.RemoveConnection(this);
        }

        /// <inheritdoc/>
        public override RoomNode GetOppositeRoomNode(RoomNode entrance)
        {
            //Used to correct for the case in which entrance is selected based on coordinate position, instead of the specific RoomNode being entered from, given that a StairNode surface position
            //can be equal to the surface position of a RoomNode on the level above it.
            if (entrance == FirstNode || entrance.SurfacePosition == FirstNode.SurfacePosition)
                return SecondNode;
            else if (entrance == SecondNode || entrance.SurfacePosition == FirstNode.SurfacePosition)
                return FirstNode;
            else
            {
                throw new System.ArgumentException();
            }
        }

        /// <inheritdoc/>
        public override void RegisterRooms()
        {
            FirstNode.Room.AddConnection(this);
            SecondNode.Room.AddConnection(this);

            GameManager.MapChangingLate -= WhenMapChanging;
        }
    }
}