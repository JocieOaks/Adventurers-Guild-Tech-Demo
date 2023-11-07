using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Node;
using System;
using System.Linq;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Map.Sprite_Object;
using Room = Assets.Scripts.Map.Room;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="TravelAction"/> is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to navigate from one position to another.
    /// </summary>
    public class TravelAction : TaskAction
    {
        private RoomNode _nextNode;
        private INode _prevMapNode;
        private INode _nextMapNode;
        private IDestination _currentDestination;
        private readonly IDestination _primaryDestination;

        private NavigateRoom NavigateRoom
        {
            get
            {
                if (Pawn is AdventurerPawn pawn)
                    return pawn.NavigateRoom;
                throw new AccessViolationException("Player Pawn cannot use Travel Action");
            }
        }

        private NavigateMap NavigateMap
        {
            get
            {
                if (Pawn is AdventurerPawn pawn)
                    return pawn.NavigateMap;
                throw new AccessViolationException("Player Pawn cannot use Travel Action");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelAction"/> class.
        /// </summary>
        /// <param name="destination">The <see cref="IDestination"/> trying to be reached.</param>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="TravelAction"/>.</param>
        public TravelAction(IDestination destination, Pawn pawn) : base(pawn)
        {
            NavigateMap.SetGoal(destination);
            _primaryDestination = destination;
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <inheritdoc/>
        public override int Complete()
        {
            if (!(Pawn.CurrentStep is TraverseStep || NavigateRoom.IsGoalReachable()) || !NavigateMap.IsGoalReachable())
            {
                NavigateRoom.IsGoalReachable();
                NavigateMap.IsGoalReachable();
                return -1;
            }
            
            if (Pawn.CurrentStep?.IsComplete() ?? true)
                return 0;

            return _primaryDestination?.IsComplete(Pawn.CurrentNode) ?? false ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            GameManager.MapChanged += OnMapEdited;
            
            if (Pawn.Stance != Stance.Stand)
            {
                Pawn.Stance = Stance.Stand;
                if(Pawn.CurrentNode.Occupant is IInteractable)
                    Pawn.CurrentStep = new WalkStep((NavigateMap.GetNext() as RoomNode)!.SurfacePosition, Pawn, Pawn.CurrentStep);
            }
            if(Pawn.CurrentStep?.IsComplete() ?? true)
                Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);
            _nextNode = Pawn.CurrentNode;
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            if (Pawn.CurrentStep?.IsComplete() ?? true)
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
            INode current;
            if ((_currentDestination?.IsComplete(Pawn.CurrentNode) ?? true) || Pawn.CurrentStep is TraverseStep)
            {
                if (Pawn.CurrentStep is not TraverseStep)
                {
                    _prevMapNode = _nextMapNode;
                }

                current = NavigateMap.GetNext();
                INode next = current;

                if (current.AdjacentToRoomNode(Pawn.CurrentNode))
                {
                    NavigateMap.UpdateStart(current);
                    next = NavigateMap.GetNext();
                }

                if(SetDestination(next))
                    return;
            }

            current = NavigateMap.GetNext();
            if (current != _nextMapNode && (_currentDestination != _primaryDestination || !_primaryDestination.Endpoints.Contains(current)))
            {
                if(SetDestination(current))
                    return;
            }

            INode node = NavigateRoom?.GetNext();
            if (node is RoomNode roomNode)
            {
                _nextNode = roomNode;
                Pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, Pawn, Pawn.CurrentStep);
            }
        }

        private bool SetDestination(INode next)
        {
            if (_prevMapNode is ConnectingNode connection)
            {
                Room room = connection.GetCommonRoom(next) ??
                            throw new NullReferenceException(
                                "INodes do not share a common room. This should never be hit.");

                if (room != Pawn.CurrentNode.Room)
                {
                    if (!connection.AdjacentToRoomNode(Pawn.CurrentNode))
                    {
                        _currentDestination = new TargetDestination(connection);
                        NavigateRoom?.SetGoal(_currentDestination);
                    }

                    _nextNode = connection.GetOppositeRoomNode(Pawn.CurrentNode);
                    Pawn.CurrentStep = new TraverseStep(Pawn.CurrentNode, connection, Pawn, Pawn.CurrentStep);
                    return true;
                }
            }

            _nextMapNode = next;

            _currentDestination = _primaryDestination.Endpoints.Contains(next) ? _primaryDestination : new TargetDestination(next);
            NavigateRoom?.SetGoal(_currentDestination);
            NavigateMap.UpdateEdgeLength(next, NavigateRoom!.Score(Pawn.CurrentNode));
            return false;
        }
        

        /// <summary>
        /// Called whenever the <see cref="Map"/> has been updated.
        /// </summary>
        private void OnMapEdited()
        {
            NavigateRoom.SetGoal(_currentDestination);
        }
    }
}
