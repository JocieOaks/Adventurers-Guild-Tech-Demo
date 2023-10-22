using System;
using System.Collections;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
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
        private RoomNode _nextNode;
        private ConnectingNode _nextConnectingNode;

        private IGoal _goal;

        private DLite DLite
        {
            get
            {
                if(Pawn is AdventurerPawn pawn)
                    return pawn.DLite;
                throw new AccessViolationException("Player Pawn cannot use Travel Action");
            }
        }
        private PathLink _root;

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
        public Vector3Int Destination { get; }

        /// <inheritdoc/>
        public override int Complete()
        {
            if (!_ready || _root != null|| !(Pawn.CurrentStep?.IsComplete() ?? false) || !(_goal is DestinationGoal goal && goal.Destination.WorldPosition == Destination))
                return 0;


            if (DLite.IsGoalReachable(Pawn.CurrentNode))
                return _goal.IsComplete(Pawn.CurrentNode);
            
            if (Pawn.WorldPosition == Destination)
            {
                GameManager.MapChanged -= OnMapEdited;
                return 1;
            }

            GameManager.MapChanged -= OnMapEdited;
            return -1;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            GameManager.MapChanged += OnMapEdited;
            
            if (Pawn.Stance != Stance.Stand)
            {
                Pawn.Stance = Stance.Stand;
            }
            if(Pawn.CurrentStep?.IsComplete() ?? true)
                Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);
            Pawn.StartCoroutine(PathFind());
            _nextNode = Pawn.CurrentNode;
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            if (_ready && (Pawn.CurrentStep?.IsComplete() ?? true))
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
            if ((_goal?.IsComplete(Pawn.CurrentNode) ?? 1) == 1 || Pawn.CurrentStep is TraverseStep)
            {
                if (_nextConnectingNode != null && Pawn.CurrentStep is not TraverseStep)
                {
                    _nextNode = _nextConnectingNode.GetOppositeRoomNode(Pawn.CurrentNode);
                    Pawn.CurrentStep = new TraverseStep(Pawn.CurrentNode, _nextConnectingNode, Pawn, Pawn.CurrentStep);
                    return;
                }

                Pawn.CurrentStep = null;
                if (_root != null)
                {
                    _goal = new DestinationGoal(_root.Node!.GetRoomNode(_nextNode.Room));
                    DLite?.SetGoal(_goal);
                    DLite?.EstablishPathing();
                    _nextConnectingNode = _root.Node;
                    _root = _root.Next;
                }
                else
                {
                    _goal = new DestinationGoal(Map.Map.Instance[Destination]);
                    DLite?.SetGoal(_goal);
                    DLite?.EstablishPathing();
                }
            }

            INode node = DLite?.GetNext(_nextNode);
            if (node is RoomNode roomNode)
            {
                _nextNode = roomNode;
                Pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, Pawn, Pawn.CurrentStep);
            }
            else
            {
                throw new ArgumentException("There is no path.");
            }
        }
        

        /// <summary>
        /// Called whenever the <see cref="Map"/> has been updated.
        /// </summary>
        private void OnMapEdited()
        {
            _ready = false;
            DLite.SetGoal(_goal);
        }

        /// <summary>
        /// Construct the path for the <see cref="AdventurerPawn"/> to follow.
        /// </summary>
        /// <returns>Returns <see cref="WaitUntil"/> objects for the <c>StartCoroutine</c> function until the <see cref="NavigateJob"/> has completed.</returns>
        private IEnumerator PathFind()
        {
            try
            {
                NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new(100, Allocator.Persistent);
                NavigateJob navigate = new(Pawn.WorldPosition, Destination, walkingPath);
                JobHandle navigateJobHandle = navigate.Schedule();
                yield return new WaitUntil(() => navigateJobHandle.IsCompleted);
                navigateJobHandle.Complete();
                PathLink current = null;
                for (int i = 0; i < walkingPath.Length; i++)
                {
                    if (walkingPath[i] == default)
                    {
                        break;
                    }

                    current = i == 0
                        ? _root = new PathLink(Map.Map.Instance.GetConnectionNode(walkingPath[i].position), Pawn)
                        : new PathLink(Map.Map.Instance.GetConnectionNode(walkingPath[i].position), current);
                }

                walkingPath.Dispose();

                NextStep();
            }
            finally
            {
                _ready = true;
            }
        }
    }
}
