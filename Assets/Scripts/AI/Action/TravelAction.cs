using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// The <see cref="TravelAction"/> is a <see cref="TaskAction"/> for a <see cref="Pawn"/> to navigate from one position to another.
/// </summary>
public class TravelAction : TaskAction
{
    bool _ready = false;
    readonly Queue<INode> _walkingPath = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TravelAction"/> class
    /// </summary>
    /// <param name="destination">The <see cref="Map"/> coordinates of <see cref="Actor"/>'s destination.</param>
    /// <param name="actor">The <see cref="Actor"/> that is traveling.</param>
    public TravelAction(Vector3Int destination, Actor actor) : base(actor)
    {
        Destination = destination;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TravelAction"/> class
    /// </summary>
    /// <param name="destination">The <see cref="IWorldPosition"/> the <see cref="Actor"/> is trying to reach..</param>
    /// <param name="actor">The <see cref="Actor"/> that is traveling.</param>
    public TravelAction(IWorldPosition destination, Actor actor) : base(actor)
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
        else if (_actor.Stats.Position == Destination)
        {
            GameManager.MapChanged -= OnMapEdited;
            return 1;
        }
        else if(Map.Instance[Destination].Occupant is IInteractable interactable)
        {
            foreach(RoomNode node in interactable.InteractionPoints)
            {
                if (_actor.Stats.Position == node.WorldPosition)
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
        if(_actor.Stats.Stance != Stance.Stand)
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
        Pawn pawn = _pawn;
        if (_ready && pawn.CurrentStep.IsComplete())
        {
            INode node = _walkingPath.Dequeue();
            if (node is RoomNode roomNode)
            {
                pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, pawn, pawn.CurrentStep);
            }
            else if (node is ConnectingNode connection)
            {
                pawn.CurrentStep = new TraverseStep(pawn.CurrentNode, connection, pawn, pawn.CurrentStep);
            }
        }
        else if (_pawn.CurrentStep is WalkStep && pawn.CurrentStep.IsComplete())
            _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, false);
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
    /// Construct the path for the <see cref="Pawn"/> to follow.
    /// </summary>
    /// <returns>Returns <see cref="WaitUntil"/> objects for the <c>StartCoroutine</c> function until the <see cref="NavigateJob"/> has completed.</returns>
    IEnumerator Pathfind()
    {
        NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new(100, Allocator.Persistent);
        NavigateJob navigate = new(_actor.Stats.Position, Destination, walkingPath);
        JobHandle navigateJobHandle = navigate.Schedule();
        yield return new WaitUntil(() => navigateJobHandle.IsCompleted);
        navigateJobHandle.Complete();

        for (int i = 1; i < walkingPath.Length; i++)
        {
            if (walkingPath[i] == default)
            {
                Destination = walkingPath[i - 1].position;
                break;
            }
            _walkingPath.Enqueue(walkingPath[i].isDoor ? Map.Instance.GetConnectionNode(walkingPath[i].position) : Map.Instance[walkingPath[i].position]);
        }

        walkingPath.Dispose();
        _ready = true;
    }
}
