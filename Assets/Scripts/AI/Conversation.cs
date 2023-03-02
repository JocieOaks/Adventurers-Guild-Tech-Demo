using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// The <see cref="Conversation"/> class maintains data for an extended social interaction between two <see cref="Pawn"/>s.
/// Conversations will eventually be able to occur between more than two <see cref="Pawn"/>s.
/// </summary>
public class Conversation
{
    readonly List<Pawn> _pawns;

    /// <summary>
    /// Initializes a new instance of the <see cref="Conversation"/> class.
    /// </summary>
    /// <param name="a">The first of the two <see cref="Pawn"/>s that started the <see cref="Conversation"/>.</param>
    /// <param name="b">The second of the two <see cref="Pawn"/>s that started the <see cref="Conversation"/>.</param>
    public Conversation(Pawn a, Pawn b)
    {
        _pawns = new List<Pawn>() { a, b };
        GameManager.Ticked += OnTicked;
        GameManager.NonMonoUpdate += Update;
    }

    /// <value>The length of time since the <see cref="Conversation"/> was initialized in seconds. Does not count time while the game is paused.</value>
    public int Duration { get; private set; } = 0;

    /// <value>Gives the average position of all the <see cref="Pawn"/>s talking, which is approximated as the "center" of the <see cref="Conversation"/>.</value>
    public Vector3Int Nexus
    {
        get
        {
            Vector3Int position = Vector3Int.zero;
            foreach (Pawn pawn in _pawns)
            {
                position += pawn.WorldPosition;
            }
            return position / _pawns.Count;
        }
    }

    /// <value>Provides access to the list of <see cref="Pawn"/>s in the <see cref="Conversation"/>.</value>
    public IEnumerable Pawns
    {
        get
        {
            foreach (Pawn pawn in _pawns)
                yield return pawn;
        }
    }

    /// <summary>
    /// Checks if a position is close enough to the <see cref="Conversation"/> for a <see cref="Pawn"/> to go their without needing to leave the <see cref="Conversation"/>.
    /// </summary>
    /// <param name="position">The map position to evaluate/</param>
    /// <returns>Returns true if the position is acceptably close enough to the <see cref="Conversation"/>.</returns>
    public bool InRadius(Vector3Int position)
    {
        Room room = Map.Instance[position].Room;
        if (Map.Instance[position].Room != Map.Instance[Nexus].Room)
            return false;

        if (room is Layer)
        {
            if ((position - Nexus).sqrMagnitude > 36)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Removes a specified <see cref="Pawn"/> from the <see cref="Conversation"/>.
    /// If only one <see cref="Pawn"/> remains in the <see cref="Conversation"/>, the <see cref="Conversation"/> will end.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> to remove from the <see cref="Conversation"/>.</param>
    public void Leave(Pawn pawn)
    {
        _pawns.Remove(pawn);
        if (_pawns.Count <= 1)
        {
            _pawns.FirstOrDefault()?.OverrideTask(new LeaveConversationTask());
            GameManager.Ticked -= OnTicked;
        }
    }

    /// <summary>
    /// Gives a score for if a given position is too close or too far from the center of the <see cref="Conversation"/>.
    /// Used by the <see cref="Planner"/> to determine the Utility of moving from one position to the other.
    /// </summary>
    /// <param name="position">The map position to be evaluated.</param>
    /// <returns>The scored evaluation of the given position.</returns>
    public float PositionUtility(Vector3Int position)
    {
        float distance = Vector3.Distance(Nexus, position);

        return PositionUtility(distance);
    }

    /// <summary>
    /// Gives a score for if a given position is too close or too far from the center of the <see cref="Conversation"/>.
    /// Used by the <see cref="Planner"/> to determine the Utility of moving from one position to the other.
    /// </summary>
    /// <param name="distance">The distance from the center of the <see cref="Conversation"/>.</param>
    /// <returns>The scored evaluation of the given distance.</returns>
    public float PositionUtility(float distance)
    {
        return Mathf.Exp(-2 * (distance - 1)) + Mathf.Exp((distance - 1) / 4);
    }

    /// <summary>
    /// Called each frame. Sets Pawns to face one another.
    /// </summary>
    public void Update()
    {
        foreach (Pawn pawn in _pawns)
        {
            if (pawn.CurrentStep is WaitStep wait)
            {
                Direction direction = Utility.VectorToDir(Nexus - pawn.WorldPosition);
                wait.SetDirection(direction);
            }
        }
    }

    /// <summary>
    /// Called each GameManager tick to update the Duration.
    /// </summary>
    void OnTicked()
    {
        Duration++;
    }
}
