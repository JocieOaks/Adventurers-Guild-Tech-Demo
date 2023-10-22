using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// Class <see cref="BlockingNode"/> is an <see cref="IDividerNode"/> that separates two <see cref="RoomNode"/>s.
    /// </summary>
    public abstract class BlockingNode : IDividerNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingNode"/> class.
        /// </summary>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="BlockingNode"/>.</param>
        /// <param name="alignment"></param>
        protected BlockingNode(Vector3Int worldPosition, MapAlignment alignment)
        {
            WorldPosition = worldPosition;
            Alignment = alignment;

            FirstNode = Map.Instance[worldPosition];
            SecondNode = alignment == MapAlignment.XEdge
                ? Map.Instance[worldPosition + Vector3Int.down]
                : Map.Instance[worldPosition + Vector3Int.left];
        }

        /// <inheritdoc/>
        public IEnumerable<INode> AdjacentNodes
        {
            get
            {
                yield return FirstNode;
                yield return SecondNode;
            }
        }

        /// <inheritdoc/>
        public MapAlignment Alignment { get; }

        /// <inheritdoc/>
        public abstract Vector3Int Dimensions { get; }

        /// <inheritdoc/>
        public RoomNode FirstNode { get; }

        /// <inheritdoc/>
        public Vector3Int NearestCornerPosition => WorldPosition;

        /// <inheritdoc/>
        public RoomNode Node => FirstNode;

        /// <inheritdoc/>
        public bool Obstructed => true;

        /// <inheritdoc/>
        public Room Room => null;

        /// <inheritdoc/>
        public RoomNode SecondNode { get; }

        /// <inheritdoc/>
        public Vector3Int WorldPosition { get; }

        /// <inheritdoc/>
        public bool HasNavigatedTo(RoomNode node)
        {
            return Map.Instance[WorldPosition] == node || Map.Instance[WorldPosition + (Alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left)] == node;
        }
    }
}