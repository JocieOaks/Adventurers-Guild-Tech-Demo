using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// The <see cref="TravelAction"/> is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to navigate from one position to another.
/// </summary>
public class TravelAction : TaskAction
{
    bool _ready = false;
    readonly Queue<INode> _walkingPath = new();
    RoomNode nextNode;

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
        foreach(INode node in _walkingPath)
        {
            if (!node.Traversable)
            {
                GameManager.MapChanged -= OnMapEdited;
                return -1;
            }
        }

        if (!_ready || _walkingPath.Count > 0 || !_pawn.CurrentStep.IsComplete())
            return 0;
        else if (_pawn.WorldPosition == Destination)
        {
            GameManager.MapChanged -= OnMapEdited;
            return 1;
        }
        else if(Map.Instance[Destination].Occupant is IInteractable interactable)
        {
            foreach(RoomNode node in interactable.InteractionPoints)
            {
                if (_pawn.WorldPosition == node.WorldPosition)
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
        if(_pawn.Stance != Stance.Stand)
        {
            _pawn.Stance = Stance.Stand;
        }
        if(_pawn.CurrentStep.IsComplete())
            _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, false);
        _pawn.StartCoroutine(Pathfind());
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        if (_ready && _pawn.CurrentStep.IsComplete())
        {
            if (_pawn.CurrentNode != nextNode)
            {
                _pawn.CurrentStep = new WalkStep(nextNode.SurfacePosition, _pawn, _pawn.CurrentStep);
            }
            else
            {
                NextStep();
            }
        }
        else if (_pawn.CurrentStep is WalkStep && _pawn.CurrentStep.IsComplete())
            _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, false);
    }

    /// <summary>
    /// Pulls the next step in the path off the queue and sets the next <see cref="TaskStep"/>.
    /// </summary>
    void NextStep()
    {
        if (_walkingPath.Count > 0)
        {
            INode node = _walkingPath.Dequeue();
            if (node is RoomNode roomNode)
            {
                nextNode = roomNode;
                _pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, _pawn, _pawn.CurrentStep);
            }
            else if (node is ConnectingNode connection)
            {
                nextNode = connection.GetOppositeRoomNode(_pawn.CurrentNode);
                _pawn.CurrentStep = new TraverseStep(_pawn.CurrentNode, connection, _pawn, _pawn.CurrentStep);
            }
        }
    }

    /// <summary>
    /// Called whenever the <see cref="Map"/> has been updated.
    /// </summary>
    void OnMapEdited()
    {
        _ready = false;
        _walkingPath.Clear();
        _pawn.StartCoroutine(Pathfind());
    }

    /// <summary>
    /// Construct the path for the <see cref="AdventurerPawn"/> to follow.
    /// </summary>
    /// <returns>Returns <see cref="WaitUntil"/> objects for the <c>StartCoroutine</c> function until the <see cref="NavigateJob"/> has completed.</returns>
    IEnumerator Pathfind()
    {
        NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new(100, Allocator.Persistent);
        NavigateJob navigate = new(_pawn.WorldPosition, Destination, walkingPath);
        JobHandle navigateJobHandle = navigate.Schedule();
        yield return new WaitUntil(() => navigateJobHandle.IsCompleted);
        navigateJobHandle.Complete();

        for (int i = 0; i < walkingPath.Length; i++)
        {
            if (walkingPath[i] == default)
            {
                if(i != 0)
                    Destination = walkingPath[i - 1].position;
                break;
            }
            _walkingPath.Enqueue(walkingPath[i].isDoor ? Map.Instance.GetConnectionNode(walkingPath[i].position) : Map.Instance[walkingPath[i].position]);
        }

        walkingPath.Dispose();

        NextStep();

        _ready = true;
    }
}
