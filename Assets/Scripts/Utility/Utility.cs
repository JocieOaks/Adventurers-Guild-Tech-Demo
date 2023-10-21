using Assets.Scripts.Map;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// The <see cref="Utility"/> class is a static class that contains a variety of useful functions.
    /// </summary>
    public static class Utility
    {
        public const float TOLERANCE = 1e-6f;
        public static readonly float Rad2Over2 = Mathf.Sqrt(2) / 2;
        public static readonly float Rad3Over2 = Mathf.Sqrt(3) / 2;
        public static readonly float Rad5Over4 = Mathf.Sqrt(5) / 4;

        /// <summary>
        /// Converts a <see cref="Direction"/> to the <see cref="MapAlignment"/> that is perpendicular to it.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> being queried.</param>
        /// <returns>Returns the <see cref="MapAlignment"/> perpendicular to <c>direction</c>.</returns>
        public static MapAlignment DirectionToEdgeAlignment(Direction direction)
        {
            return (direction == Direction.North || direction == Direction.South) ? MapAlignment.XEdge : MapAlignment.YEdge;
        }

        /// <summary>
        /// Get's the vector that is in the direction of a <see cref="Direction"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> of the vector.</param>
        /// <returns>Returns a <see cref="Vector3Int"/> that points the same way as <c>direction</c>.</returns>
        public static Vector3Int DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.North => Vector3Int.up,
                Direction.South => Vector3Int.down,
                Direction.East => Vector3Int.right,
                Direction.West => Vector3Int.left,
                Direction.NorthEast => new Vector3Int(1, 1),
                Direction.SouthEast => new Vector3Int(1, -1),
                Direction.NorthWest => new Vector3Int(-1, 1),
                Direction.SouthWest => new Vector3Int(-1, -1),
                _ => default,
            };
        }

        /// <summary>
        /// Get's the vector that is in the direction of a <see cref="Direction"/>, with a magnitude of 1.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> of the vector.</param>
        /// <returns>Returns a normalized <see cref="Vector3Int"/> that points the same way as <c>direction</c>.</returns>
        public static Vector3 DirectionToVectorNormalized(Direction direction)
        {
            return direction switch
            {
                Direction.North => Vector3.up,
                Direction.South => Vector3.down,
                Direction.East => Vector3.right,
                Direction.West => Vector3.left,
                Direction.NorthEast => new Vector3(Rad2Over2, Rad2Over2),
                Direction.SouthEast => new Vector3(Rad2Over2, -Rad2Over2),
                Direction.NorthWest => new Vector3(-Rad2Over2, Rad2Over2),
                Direction.SouthWest => new Vector3(-Rad2Over2, -Rad2Over2),
                _ => default,
            };
        }

        /// <summary>
        /// Calculates the <see cref="SpriteRenderer"/> sorting order of an object at this position.
        /// </summary>
        /// <param name="position">The position of the object.</param>
        /// <returns>Returns the sort order at the given position.</returns>
        public static int GetSortOrder(Vector3Int position)
        {
            return 1 - 2 * position.x - 2 * position.y + 2 * position.z;
        }

        /// <summary>
        /// Determines if an <see cref="IWorldPosition"/> is closer to the camera than another <see cref="IWorldPosition"/>.
        /// </summary>
        /// <param name="first">The first <see cref="IWorldPosition"/> being compared.</param>
        /// <param name="second">The second <see cref="IWorldPosition"/> being compared.</param>
        /// <returns>Returns true if <c>first</c> is closer than <c>second</c>.</returns>
        public static bool IsInFrontOf(IWorldPosition first, IWorldPosition second)
        {
            Vector3 relPosition = second.NearestCornerPosition - first.NearestCornerPosition;

            if(first.Alignment == MapAlignment.XEdge)
            {
                relPosition += new Vector3(0, 0.5f);
            }
            else if (first.Alignment == MapAlignment.YEdge)
            {
                relPosition += new Vector3(0.5f, 0);
            }

            if (second.Alignment == MapAlignment.XEdge)
            {
                relPosition -= new Vector3(0, 0.5f);
            }
            else if (second.Alignment == MapAlignment.YEdge)
            {
                relPosition -= new Vector3(0.5f, 0);
            }

            bool xIntersection = (relPosition.x > 0 && first.Dimensions.x > relPosition.x) || (second.Dimensions.x > -relPosition.x);
            bool yIntersection = (relPosition.y > 0 && first.Dimensions.y > relPosition.y) || (second.Dimensions.y > -relPosition.y);
            //_alignmentVector is a static vector that points from the camera inward. (1,1,0)
            //If the dot product of the alignment vector and the relative position of the second to the first is positive, it means that the second is further into screen than the first
            return relPosition.z - first.Dimensions.z < 0 && (-relPosition.z >= second.Dimensions.z || (xIntersection && relPosition.y >= 0 || yIntersection && relPosition.x >= 0));
        }

        /// <summary>
        /// Gives the scene coordinates corresponding to a given <see cref="Map"/> coordinate.
        /// </summary>
        /// <param name="position">The position on the <see cref="Map"/> being queried.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the object, defaulting to <see cref="MapAlignment.Center"/>.</param>
        /// <returns>Returns the coordinates for a <see cref="Transform"/> at the given <see cref="Map"/> position.</returns>
        public static Vector3 MapCoordinatesToSceneCoordinates(Vector3 position, MapAlignment alignment = MapAlignment.Center)
        {
            float mapX = position.x;
            float mapY = position.y;
            float mapZ = position.z;

            float x = 150 + 2 * mapX - 2 * mapY;
            float y = 2 + mapX + mapY;
            switch (alignment)
            {
                case MapAlignment.XEdge:

                    x++;
                    break;
                case MapAlignment.YEdge:
                    x--;
                    break;
                case MapAlignment.Corner:
                    y--;
                    break;
            }
            return new Vector3(x, y + 2 * mapZ);
        }

        /// <summary>
        /// Gives the corresponding <see cref="Map"/> coordinates of a <see cref="Transform"/> coordinate. Requires a z-coordinate to determine exactly where a point is.
        /// </summary>
        /// <param name="position">The scene position being queried.</param>
        /// <param name="level">The z coordinate to set the position at.</param>
        /// <returns>Returns the <see cref="Map"/> coordinates of the position.</returns>
        public static Vector3Int SceneCoordinatesToMapCoordinates(Vector3 position, int level)
        {
            float x = (position.x - 154 + 2 * (position.y - 2 * level)) / 4f;
            float y = position.y - 2 - x - 2 * level;

            return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), level);
        }

        /// <summary>
        /// Determines the <see cref="Direction"/> that is most closely aligning to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3"/> being evaluated.</param>
        /// <param name="cardinal">Determines whether to only return a cardinal direction.</param>
        /// <returns>Returns the <see cref="Direction"/> that <c>vector</c> points in.</returns>
        public static Direction VectorToDirection(Vector3 vector, bool cardinal = false)
        {
            Vector2 gameVector = new(vector.x, vector.y);

            if (gameVector == Vector2.zero)
                return Direction.Undirected;

            int best = 7;
            float bestProduct = Vector2.Dot(gameVector, new Vector2(0, 1));

            for (int i = 0; i < 7; i++)
            {
                if (cardinal && i % 2 == 0)
                    continue;
                var value = i switch
                {
                    1 => Vector2.Dot(gameVector, new Vector2(1, 0)),
                    2 => Vector2.Dot(gameVector, new Vector2(Rad2Over2, -Rad2Over2)),
                    3 => Vector2.Dot(gameVector, new Vector2(0, -1)),
                    4 => Vector2.Dot(gameVector, new Vector2(-Rad2Over2, -Rad2Over2)),
                    5 => Vector2.Dot(gameVector, new Vector2(-1, 0)),
                    6 => Vector2.Dot(gameVector, new Vector2(-Rad2Over2, Rad2Over2)),
                    _ => Vector2.Dot(gameVector, new Vector2(Rad2Over2, Rad2Over2)),
                };
                if (value > bestProduct)
                {
                    bestProduct = value;
                    best = i;
                }
            }

            return best switch
            {
                0 => Direction.NorthEast,
                1 => Direction.East,
                2 => Direction.SouthEast,
                3 => Direction.South,
                4 => Direction.SouthWest,
                5 => Direction.West,
                6 => Direction.NorthWest,
                7 => Direction.North,
                _ => Direction.Undirected,
            };
        }
    }
}
