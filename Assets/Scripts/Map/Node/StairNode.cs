using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Sprite_Object;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// The <see cref="StairNode"/> is a child of <see cref="RoomNode"/> that has a directed slant, an sits at two different z positions.
    /// </summary>
    public class StairNode : RoomNode, IDirected
    {
#pragma warning disable IDE0052
        // ReSharper disable once NotAccessedField.Local
        private readonly StairSprite _stairSprite;
#pragma warning restore IDE0052

        /// <summary>
        /// Initializes a new instance of the <see cref="StairNode"/> class that does not already have an associated <see cref="StairSprite"/>.
        /// </summary>
        /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="StairNode"/>.</param>
        /// <param name="direction">The <see cref="Scripts.Map.Direction"/> the <see cref="StairNode"/> is facing.</param>
        public StairNode(Vector3Int position, Direction direction) : base(Map.Instance[position])
        {
            Direction = direction;

            if (RoomPosition.z == Room.Height - 1)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new LandingConnector(this, direction);
            }

            _stairSprite = new StairSprite(direction, WorldPosition, RoomPosition.z, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StairNode"/> class that already has an associated <see cref="StairSprite"/>, and is replacing a pre-existing <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="sprite">The <see cref="StairSprite"/> associated with this <see cref="StairNode"/>.</param>
        /// <param name="node">The <see cref="RoomNode"/> this <see cref="StairNode"/> is taking the place of.</param>
        /// <param name="z">The vertical position, relative to <see cref="Room.Origin"/> that the <see cref="StairNode"/> will be placed.</param>
        /// <param name="direction">The <see cref="Scripts.Map.Direction"/> the <see cref="StairNode"/> is facing.</param>
        public StairNode(StairSprite sprite, RoomNode node, int z, Direction direction) : base(node)
        {
            Direction = direction;
            _stairSprite = sprite;
            SetZ(z - Room.Origin.z);
            if (RoomPosition.z == Room.Height - 1)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new LandingConnector(this, direction);
            }
        }

        /// <inheritdoc/>
        public Direction Direction { get; }

        /// <inheritdoc/>
        public override Vector3Int SurfacePosition => WorldPosition + Vector3Int.forward;

        public void Destroy()
        {
            SetZ(Room.Origin.z);
            RoomNode copy = new(this);
            if(copy.TryGetNodeAs(Direction, out LandingConnector landing))
                landing.Disconnect();
        }

        /// <inheritdoc/>
        public override T GetNodeAs<T>(Direction direction, bool traversable = true)
        {
            if (typeof(T) == typeof(RoomNode) && traversable && direction != ~Direction)
            {
                if (GetNode(direction) is T node)
                {
                    if (direction == Direction)
                    {
                        if (node.WorldPosition.z == WorldPosition.z + 1)
                        {
                            return node;
                        }
                    }
                    else if (node is StairNode && node.WorldPosition.z == WorldPosition.z)
                    {
                        return node;
                    }
                }
            }
            else
            {
                return base.GetNodeAs<T>(direction, traversable);
            }
            return default(T);
        }

        public Vector3 StairPosition(Vector3 pawnPosition)
        {
            float z = Direction switch
            {
                Direction.North => Mathf.Clamp(pawnPosition.y + 0.5f - WorldPosition.y, 0f, 1f) + WorldPosition.z,
                Direction.South => Mathf.Clamp(pawnPosition.y + 0.5f - WorldPosition.y, -1f, 0f) + WorldPosition.z,
                Direction.West => Mathf.Clamp(pawnPosition.x + 0.5f - WorldPosition.x, -1f, 0f) + WorldPosition.z,
                Direction.East => Mathf.Clamp(pawnPosition.x + 0.5f - WorldPosition.x, 0f, 1f) + WorldPosition.z,
                _ => WorldPosition.z
            };

            return new Vector3(pawnPosition.x, pawnPosition.y, z);
        }

        /// <inheritdoc/>
        public override float SpeedMultiplier => base.SpeedMultiplier * 0.75f;
    }
}
