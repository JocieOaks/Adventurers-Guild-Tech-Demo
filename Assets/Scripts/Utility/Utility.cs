using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="Utility"/> class is a static class that contains a variety of useful functions.
/// </summary>
public static class Utility
{

    public static readonly float RAD2_2 = Mathf.Sqrt(2) / 2;
    public static readonly float RAD6_4 = Mathf.Sqrt(5) / 4;

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
    /// Converts a <see cref="Direction"/> to the <see cref="MapAlignment"/> that is perpendicular to it.
    /// </summary>
    /// <param name="direction">The <see cref="Direction"/> being queried.</param>
    /// <returns>Returns the <see cref="MapAlignment"/> perpendicular to <c>direction</c>./returns>
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
            Direction.NorthEast => new Vector3(RAD2_2, RAD2_2),
            Direction.SouthEast => new Vector3(RAD2_2, -RAD2_2),
            Direction.NorthWest => new Vector3(-RAD2_2, RAD2_2),
            Direction.SouthWest => new Vector3(-RAD2_2, -RAD2_2),
            _ => default,
        };
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
    /// <returns>Returns the <see cref="Direction"/> that <c>vector</c> points in.</returns>
    public static Direction VectorToDirection(Vector3 vector, bool cardinal = false)
    {
        Vector2 gameVector = new(vector.x, vector.y);

        if (gameVector == Vector2.zero)
            return Direction.Undirected;

        int best = 7;
        float best_product = Vector2.Dot(gameVector, new Vector2(0, 1));

        for (int i = 0; i < 7; i++)
        {
            if (cardinal && i % 2 == 0)
                continue;
            var value = i switch
            {
                1 => Vector2.Dot(gameVector, new Vector2(1, 0)),
                2 => Vector2.Dot(gameVector, new Vector2(RAD2_2, -RAD2_2)),
                3 => Vector2.Dot(gameVector, new Vector2(0, -1)),
                4 => Vector2.Dot(gameVector, new Vector2(-RAD2_2, -RAD2_2)),
                5 => Vector2.Dot(gameVector, new Vector2(-1, 0)),
                6 => Vector2.Dot(gameVector, new Vector2(-RAD2_2, RAD2_2)),
                _ => Vector2.Dot(gameVector, new Vector2(RAD2_2, RAD2_2)),
            };
            if (value > best_product)
            {
                best_product = value;
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
