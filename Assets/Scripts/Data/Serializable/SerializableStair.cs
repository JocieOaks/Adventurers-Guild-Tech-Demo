using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.Data.Serializable
{
    /// <summary>
    /// The <see cref="SerializableStair"/> class is a serializable version of the <see cref="StairNode"/> class used for data persistence.
    /// </summary>
    [System.Serializable]
    public struct SerializableStair
    {
        public Direction Direction;
        public Vector3Int Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableStair"/> class based on a <see cref="StairNode"/>.
        /// </summary>
        /// <param name="stair">The <see cref="StairNode"/> being serialized and saved.</param>
        public SerializableStair(StairNode stair)
        {
            Position = stair.WorldPosition;
            Direction = stair.Direction;
        }
    }
}
