using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Conversation
{

    List<Pawn> _pawns;

    public int Duration { get; private set; } = 0;

    void OnTicked()
    {
        Duration++;
    }

    public Vector3Int Nexus { 
        get
        {
            Vector3Int position = Vector3Int.zero;
            foreach(Pawn pawn in _pawns)
            {
                position += pawn.CurrentPosition;
            }
            return position / _pawns.Count;
        }
    }

    public IEnumerable Pawns
    {
        get
        {
            foreach (Pawn pawn in _pawns)
                yield return pawn;
        }
    }

    public Conversation(Pawn a, Pawn b)
    {
        _pawns = new List<Pawn>() { a, b };
        GameManager.Ticked += OnTicked;
    }

    public void Leave(Pawn pawn)
    {
        _pawns.Remove(pawn);
        if (_pawns.Count <= 1)
        {
            _pawns.FirstOrDefault()?.OverrideTask(new LeaveConversationTask());
            GameManager.Ticked -= OnTicked;
        }
    }

    public void Update()
    {
        foreach(Pawn pawn in _pawns)
        {
            if(pawn.CurrentStep is Wait wait)
            {
                Direction direction = Map.VectorToDir(Nexus - pawn.CurrentPosition);
                wait.SetDirection(direction);
            }
        }
    }

    public float PositionUtility(Vector3Int position)
    {
        float distance = Vector3.Distance(Nexus, position);

        return PositionUtility(distance);
    }

    public float PositionUtility(float distance)
    {
        return Mathf.Exp(-2 * (distance - 1)) + Mathf.Exp((distance - 1) / 4);
    }

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
}
