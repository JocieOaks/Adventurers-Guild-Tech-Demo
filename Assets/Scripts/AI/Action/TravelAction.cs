using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class TravelAction : TaskAction
{
    public Vector3Int Destination { get; private set; }
    //Queue<INode> WalkingPath = new Queue<INode> ();
    bool _ready = false;

    public Queue<INode> WalkingPath { get; } = new Queue<INode>();

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public TravelAction(Vector3Int destination, Actor actor) : base(actor)
    {
        Destination = destination;
    }

    public TravelAction(SpriteObject destination, Actor actor) : base(actor)
    {
        Destination = destination.WorldPosition;
    }

    public override int Complete()
    {
        foreach(INode node in WalkingPath)
        {
            if (!node.Traversible)
            {
                GameManager.MapChanged -= OnMapEdited;
                return -1;
            }
        }

        if (!_ready || WalkingPath.Count > 0 || !_pawn.CurrentStep.IsComplete())
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

    void OnMapEdited()
    {
        _ready = false;
        WalkingPath.Clear();
        _pawn.StartCoroutine(Pathfind());
    }

    IEnumerator Pathfind()
    {
        NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new NativeArray<(bool, Vector3Int)>(100, Allocator.Persistent);
        NavigateJob navigate = new NavigateJob(_actor.Stats.Position, Destination, walkingPath);
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
            WalkingPath.Enqueue(walkingPath[i].isDoor ? Map.Instance.GetConnectionNode(walkingPath[i].position) : Map.Instance[walkingPath[i].position]);
        }

        walkingPath.Dispose();
        _ready = true;
    }

    public override void Perform()
    {
        Pawn pawn = _pawn;
        if(_ready && pawn.CurrentStep.IsComplete())
        {
            INode node = WalkingPath.Dequeue();
            if (node is RoomNode roomNode)
            {
                pawn.CurrentStep = new WalkStep(roomNode.SurfacePosition, pawn, pawn.CurrentStep);
            }
            else if(node is ConnectionNode connection)
            {
                pawn.CurrentStep = new TraverseStep(pawn.CurrentNode, connection, pawn, pawn.CurrentStep);
            }
        }
        else if(_pawn.CurrentStep is WalkStep && pawn.CurrentStep.IsComplete())
                _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, false);
    }
}
