using System;
using System.Collections;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation;
using Assets.Scripts.AI.Navigation.Goal;
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
        private RoomNode _nextNode;
        private ConnectingNode _nextConnectingNode;

        private IGoal _currentGoal;
        private readonly IGoal _primaryGoal;

        private DLite DLite
        {
            get
            {
                if (Pawn is AdventurerPawn pawn)
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
        public TravelAction(RoomNode destination, Pawn pawn) : base(pawn)
        {
            Destination = destination;
            _primaryGoal = new DestinationGoal(destination);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelAction"/> class
        /// </summary>
        /// <param name="destination">The <see cref="IWorldPosition"/> the <see cref="Actor"/> is trying to reach..</param>
        /// <param name="pawn">The <see cref="Pawn"/> that is traveling.</param>
        public TravelAction(IInteractable destination, Pawn pawn) : base(pawn)
        {
            Destination = destination.Node;
            _primaryGoal = new InteractableGoal(destination);
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <value>The <see cref="Map"/> coordinates of the <see cref="Actor"/>'s destination.</value>
        public RoomNode Destination { get; }

        /// <inheritdoc/>
        public override int Complete()
        {
            if(!_ready)
                return 0;

            int complete = _currentGoal?.IsComplete(Pawn.CurrentNode) ?? -1;
            if (complete == -1)
                return -1;
            if (!(Pawn.CurrentStep?.IsComplete() ?? false) || _currentGoal != _primaryGoal)
                return 0;


            if (DLite.IsGoalReachable(Pawn.CurrentNode))
                return complete;

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
            if ((_currentGoal?.IsComplete(Pawn.CurrentNode) ?? 1) == 1 || Pawn.CurrentStep is TraverseStep)
            {
                if (_nextConnectingNode != null && Pawn.CurrentStep is not TraverseStep)
                {
                    _nextNode = _nextConnectingNode.GetOppositeRoomNode(Pawn.CurrentNode);
                    Pawn.CurrentStep = new TraverseStep(Pawn.CurrentNode, _nextConnectingNode, Pawn, Pawn.CurrentStep);
                    return;
                }
                if (_root != null)
                {
                    _currentGoal = new DestinationGoal(_root.Node!.GetRoomNode(_nextNode.Room));
                    DLite?.SetGoal(_currentGoal);
                    _nextConnectingNode = _root.Node;
                    _root = _root.Next;
                }
                else
                {
                    _currentGoal = _primaryGoal;
                    DLite?.SetGoal(_currentGoal);
                }
            }

            INode node = DLite?.GetNext(_nextNode);
            if (node is RoomNode roomNode)
            {
                _nextNode = roomNode;
                Pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, Pawn, Pawn.CurrentStep);
            }
        }
        

        /// <summary>
        /// Called whenever the <see cref="Map"/> has been updated.
        /// </summary>
        private void OnMapEdited()
        {
            _ready = false;
            DLite.SetGoal(_currentGoal);
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
                NavigateJob navigate = new(Pawn.WorldPosition, Destination.WorldPosition, walkingPath);
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
