using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.Utility;

namespace Assets.Scripts.AI.Navigation
{
    /// <summary>
    /// The <see cref="NavigateMap"/> is a <see cref="DLite{T}"/> for finding the best path through a constantly changing map.
    /// </summary>
    public class NavigateMap : DLite<INode>
    {
        private readonly Dictionary<INode, (float gScore, float rhs, IReference element)> _valueDictionary = new();
        private readonly Dictionary<(INode, INode), float> _edgeLength = new();
        private readonly Pawn _pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateRoom"/> algorithm.
        /// </summary>
        /// <param name="pawn">The <see cref="_pawn"/> who's path is being navigated.</param>
        public NavigateMap(Pawn pawn)
        {
            _pawn = pawn;
        }

        /// <inheritdoc/>
        protected override INode Start { get; set; }

        /// <inheritdoc/>
        public override void UpdateStart(INode node)
        {
            if (node != null)
            {
                if (_edgeLength.TryGetValue((Start, node), out float distance) ||
                    _edgeLength.TryGetValue((node, Start), out distance))
                {
                    PriorityAdjustment += distance;
                }
                else
                {
                    PriorityAdjustment += Map.Map.EstimateDistance(Start, node);
                }

                Start = node;
            }

            EstablishPathing();
        }

        /// <inheritdoc />
        protected override IEnumerable<INode> Endpoints()
        {
            return Destination.Endpoints;
        }

        /// <inheritdoc />
        public override bool IsGoalReachable()
        {
            return Destination.EndRooms.Contains(_pawn.Room) || base.IsGoalReachable();
        }

        /// <inheritdoc />
        protected override void InitializeGraph()
        {
            Start = _pawn.CurrentNode;
            _valueDictionary.Clear();
            _edgeLength.Clear();
        }

        public void UpdateEdgeLength(INode node, float length)
        {
            UpdateEdgeLength(Start, node, length);
        }

        /// <summary>
        /// Sets the path length between two nodes.
        /// Used for when the actual path length is different from the expected path length.
        /// </summary>
        /// <param name="first">The first <see cref="INode"/>.</param>
        /// <param name="second">The second <see cref="INode"/>.</param>
        /// <param name="length">The distance from <paramref name="first"/> to <paramref name="second"/>.</param>
        public void UpdateEdgeLength(INode first, INode second, float length)
        {
            _edgeLength[(first, second)] = length;
            UpdateNode(first);
            UpdateNode(second);
            EstablishPathing();
        }

        /// <inheritdoc />
        protected override (float gScore, float rhs, IReference reference) NodeValues(INode node)
        {
            if (_valueDictionary.TryGetValue(node, out (float gScore, float rhs, IReference element) values))
            {
                return values;
            }

            return (float.PositiveInfinity, float.PositiveInfinity, null);
        }

        /// <inheritdoc />
        protected override void SetElement(INode node, IReference value)
        {

            (float gScore, float rhs, IReference _) = NodeValues(node);
            _valueDictionary[node] = (gScore, rhs, value);
        }

        /// <inheritdoc />
        protected override void SetGScore(INode node, float value)
        {
            (float _, float rhs, IReference element) = NodeValues(node);
            _valueDictionary[node] = (value, rhs, element);
        }

        /// <inheritdoc />
        protected override void SetRHS(INode node, float value)
        {
            (float gScore, float _, IReference element) = NodeValues(node);
            _valueDictionary[node] = (gScore, value, element);
        }

        /// <inheritdoc />
        protected override IEnumerable<(INode, float)> Successors(INode node)
        {
            switch (node)
            {
                case ConnectingNode connection:
                {
                    foreach (ConnectingNode successor in connection.FirstNode.Room.Connections)
                    {
                        if (successor == connection) continue;

                        if (_edgeLength.TryGetValue((successor, connection), out float distance) ||
                            _edgeLength.TryGetValue((connection, successor), out distance))
                        {
                            yield return (successor, distance);
                        }
                        else
                            yield return (successor, Map.Map.EstimateDistance(connection.FirstNode, successor));
                    }


                    if (!connection.IsWithinSingleRoom)
                    {
                        foreach (ConnectingNode successor in connection.SecondNode.Room.Connections)
                        {
                            if (successor == connection) continue;

                            if (_edgeLength.TryGetValue((successor, connection), out float distance) ||
                                _edgeLength.TryGetValue((connection, successor), out distance))
                            {
                                yield return (successor, distance);
                            }
                            else
                                yield return (successor, Map.Map.EstimateDistance(connection.SecondNode, successor));
                        }
                    }

                    foreach (INode endpoint in Endpoints())
                    {
                        Room commonRoom = connection.GetCommonRoom(endpoint);
                        if (commonRoom != null)
                        {
                            if (endpoint is ConnectingNode endConnection)
                            {
                                yield return (endConnection,
                                    Map.Map.EstimateDistance(endConnection.GetRoomNode(commonRoom), connection));
                            }
                            else if (endpoint is RoomNode endNode)
                            {
                                yield return (endNode, Map.Map.EstimateDistance(endNode, connection));
                            }
                        }
                    }

                    if (Start is RoomNode startNode)
                    {
                        if (startNode.Room.Connections.Contains(node))
                        {
                            yield return (startNode, Map.Map.EstimateDistance(startNode, connection));
                            if (startNode.Occupant is IInteractable interactable)
                            {
                                foreach (RoomNode interaction in interactable.InteractionPoints)
                                {
                                    if (interaction.Room == startNode.Room)
                                    {
                                        yield return (interaction, Map.Map.EstimateDistance(interaction, connection));
                                    }
                                }
                            }
                        }
                    }

                    break;
                }
                case RoomNode roomNode:
                {
                    if (roomNode.Occupant is IInteractable interactable)
                    {
                        foreach (RoomNode interaction in interactable.InteractionPoints)
                        {
                            if (interaction.Room == roomNode.Room)
                            {
                                yield return (interaction, 0);
                            }
                        }
                    }
                    else
                    {
                        Room room = roomNode.Room;
                        foreach (ConnectingNode successor in room.Connections)
                            if (_edgeLength.TryGetValue((successor, roomNode), out float distance) ||
                                _edgeLength.TryGetValue((roomNode, successor), out distance))
                            {
                                yield return (successor, distance);
                            }
                            else
                                yield return (successor, Map.Map.EstimateDistance(roomNode, successor));

                        if (roomNode != Start && Start is RoomNode startNode && roomNode.Room == Start.Room)
                        {
                            yield return (Start, Map.Map.EstimateDistance(Start, roomNode));
                            if (startNode.Occupant is IInteractable interactable2)
                            {
                                foreach (RoomNode interaction in interactable2.InteractionPoints)
                                {
                                    if (interaction.Room == startNode.Room)
                                    {
                                        yield return (interaction, Map.Map.EstimateDistance(roomNode, interaction));
                                    }
                                }
                            }
                        }

                        foreach (INode endpoint in Endpoints())
                        {
                            if (endpoint != roomNode && endpoint is RoomNode endNode && endNode.Room == roomNode.Room)
                            {
                                yield return (endpoint, Map.Map.EstimateDistance(roomNode, endpoint));
                            }
                        }
                    }


                    break;
                }
                default:
                    throw new ArgumentException("Invalid node type.");
            }
        }

        /// <inheritdoc />
        protected override void WhenDestinationMoved(object sender, MovingEventArgs eventArgs)
        {
            InitializeEndpoints();

            foreach (RoomNode oldEndpoint in eventArgs.PreviousEndpoints)
            {
                _valueDictionary.Remove(oldEndpoint);
            }

            EstablishPathing();
        }
    }
}
