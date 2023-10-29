using System;
using System.Collections;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="TravelAction"/> is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to navigate from one position to another.
    /// </summary>
    public class TravelAction : TaskAction
    {
        private bool _ready;
        private PathLink _root;
        private RoomNode _nextNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelAction"/> class
        /// </summary>
        /// <param name="destination">The <see cref="Map"/> coordinates of <see cref="Actor"/>'s destination.</param>
        /// <param name="pawn">The <see cref="Pawn"/> that is traveling.</param>
        public TravelAction(Vector3Int destination, Pawn pawn) : base(pawn)
        {
            Destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelAction"/> class
        /// </summary>
        /// <param name="destination">The <see cref="IWorldPosition"/> the <see cref="Actor"/> is trying to reach..</param>
        /// <param name="pawn">The <see cref="Pawn"/> that is traveling.</param>
        public TravelAction(IWorldPosition destination, Pawn pawn) : base(pawn)
        {
            Destination = destination.WorldPosition;
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <value>The <see cref="Map"/> coordinates of the <see cref="Actor"/>'s destination.</value>
        public Vector3Int Destination { get; private set; }

        /// <inheritdoc/>
        public override int Complete()
        {
            PathLink pathLink = _root;
            while(pathLink != null)
            {
                INode node = pathLink.Node;
                if (!node.Traversable)
                {
                    GameManager.MapChanged -= OnMapEdited;
                    return -1;
                }
                pathLink = pathLink.Next;
            }

            if (!_ready || _root != null || !Pawn.CurrentStep.IsComplete())
                return 0;
            else if (Pawn.WorldPosition == Destination)
            {
                GameManager.MapChanged -= OnMapEdited;
                return 1;
            }
            else if(Map.Map.Instance[Destination].Occupant is IInteractable interactable)
            {
                foreach(RoomNode node in interactable.InteractionPoints)
                {
                    if (Pawn.WorldPosition == node.WorldPosition)
                    {
                        GameManager.MapChanged -= OnMapEdited;
                        return 1;
                    }
                }
            }
            GameManager.MapChanged -= OnMapEdited;
            return -1;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            GameManager.MapChanged += OnMapEdited;
            if(Pawn.Stance != Stance.Stand)
            {
                Pawn.Stance = Stance.Stand;
            }
            if(Pawn.CurrentStep.IsComplete())
                Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);
            Pawn.StartCoroutine(PathFind());
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            if (_ready && Pawn.CurrentStep.IsComplete())
            {
                if (Pawn.CurrentNode != _nextNode)
                {
                    Pawn.CurrentStep = new WalkStep(_nextNode.SurfacePosition, Pawn, Pawn.CurrentStep);
                }
                else
                {
                    NextStep();
                }
            }
            else if (Pawn.CurrentStep is WalkStep && Pawn.CurrentStep.IsComplete())
                Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);
        }

        /// <summary>
        /// Pulls the next step in the path off the queue and sets the next <see cref="TaskStep"/>.
        /// </summary>
        private void NextStep()
        {
            if (_root != null)
            {
                INode node = _root.Node;
                _root = _root.Next;
                if (node is RoomNode roomNode)
                {
                    _nextNode = roomNode;
                    Pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, Pawn, Pawn.CurrentStep);
                }
                else if (node is ConnectingNode connection)
                {
                    _nextNode = connection.GetOppositeRoomNode(Pawn.CurrentNode);
                    Pawn.CurrentStep = new TraverseStep(Pawn.CurrentNode, connection, Pawn, Pawn.CurrentStep);
                }
            }
        }

        /// <summary>
        /// Called whenever the <see cref="Map"/> has been updated.
        /// </summary>
        private void OnMapEdited(object sender, EventArgs eventArgs)
        {
            _ready = false;
            _root = null;
            Pawn.StartCoroutine(PathFind());
        }

        /// <summary>
        /// Construct the path for the <see cref="AdventurerPawn"/> to follow.
        /// </summary>
        /// <returns>Returns <see cref="WaitUntil"/> objects for the <c>StartCoroutine</c> function until the <see cref="NavigateJob"/> has completed.</returns>
        private IEnumerator PathFind()
        {
            NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new(100, Allocator.Persistent);
            NavigateJob navigate = new(Pawn.WorldPosition, Destination, walkingPath);
            JobHandle navigateJobHandle = navigate.Schedule();
            yield return new WaitUntil(() => navigateJobHandle.IsCompleted);
            navigateJobHandle.Complete();

            _root = new PathLink(Map.Map.Instance[walkingPath[0].position], Pawn);
            PathLink current = _root;

            for (int i = 1; i < walkingPath.Length; i++)
            {
                if (walkingPath[i] == default)
                {
                    if(i != 0)
                        Destination = walkingPath[i - 1].position;
                    break;
                }
                current = new PathLink(walkingPath[i].isDoor ? Map.Map.Instance.GetConnectionNode(walkingPath[i].position) : Map.Map.Instance[walkingPath[i].position], current);
            }

            walkingPath.Dispose();

            NextStep();

            _ready = true;
        }
    }
}
