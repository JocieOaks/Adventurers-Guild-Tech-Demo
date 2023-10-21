using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Step;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Map;
using UnityEngine;

namespace Assets.Scripts.AI
{
    /// <summary>
    /// The <see cref="Conversation"/> class maintains data for an extended social interaction between two <see cref="AdventurerPawn"/>s.
    /// Conversations will eventually be able to occur between more than two <see cref="AdventurerPawn"/>s.
    /// </summary>
    public class Conversation
    {
        private readonly List<AdventurerPawn> _pawns;

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversation"/> class.
        /// </summary>
        /// <param name="a">The first of the two <see cref="AdventurerPawn"/>s that started the <see cref="Conversation"/>.</param>
        /// <param name="b">The second of the two <see cref="AdventurerPawn"/>s that started the <see cref="Conversation"/>.</param>
        public Conversation(AdventurerPawn a, AdventurerPawn b)
        {
            _pawns = new List<AdventurerPawn>() { a, b };
            GameManager.Ticked += OnTicked;
            GameManager.NonMonoUpdate += Update;
        }

        /// <value>The length of time since the <see cref="Conversation"/> was initialized in seconds. Does not count time while the game is paused.</value>
        public int Duration { get; private set; }

        /// <value>Gives the average position of all the <see cref="AdventurerPawn"/>s talking, which is approximated as the "center" of the <see cref="Conversation"/>.</value>
        public Vector3Int Nexus
        {
            get
            {
                if (_pawns.Count == 0)
                    return Vector3Int.zero;

                Vector3Int position = Vector3Int.zero;
                foreach (AdventurerPawn pawn in _pawns)
                {
                    position += pawn.WorldPosition;
                }
                return position / _pawns.Count;
            }
        }

        /// <value>Provides access to the list of <see cref="AdventurerPawn"/>s in the <see cref="Conversation"/>.</value>
        public IEnumerable Pawns
        {
            get
            {
                foreach (AdventurerPawn pawn in _pawns)
                    yield return pawn;
            }
        }

        /// <summary>
        /// Checks if a position is close enough to the <see cref="Conversation"/> for a <see cref="AdventurerPawn"/> to go their without needing to leave the <see cref="Conversation"/>.
        /// </summary>
        /// <param name="position">The map position to evaluate/</param>
        /// <returns>Returns true if the position is acceptably close enough to the <see cref="Conversation"/>.</returns>
        public bool InRadius(Vector3Int position)
        {
            Room room = Map.Map.Instance[position].Room;
            if (Map.Map.Instance[position].Room != Map.Map.Instance[Nexus].Room)
                return false;

            if (room is Layer)
            {
                if ((position - Nexus).sqrMagnitude > 36)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Removes a specified <see cref="AdventurerPawn"/> from the <see cref="Conversation"/>.
        /// If only one <see cref="AdventurerPawn"/> remains in the <see cref="Conversation"/>, the <see cref="Conversation"/> will end.
        /// </summary>
        /// <param name="pawn">The <see cref="AdventurerPawn"/> to remove from the <see cref="Conversation"/>.</param>
        public void Leave(AdventurerPawn pawn)
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
            foreach (AdventurerPawn pawn in _pawns)
            {
                if (pawn.CurrentStep is WaitStep wait)
                {
                    Direction direction = Utility.Utility.VectorToDirection(Nexus - pawn.WorldPosition);
                    wait.SetDirection(direction);
                }
            }
        }

        /// <summary>
        /// Called each GameManager tick to update the Duration.
        /// </summary>
        private void OnTicked()
        {
            Duration++;
        }
    }
}
