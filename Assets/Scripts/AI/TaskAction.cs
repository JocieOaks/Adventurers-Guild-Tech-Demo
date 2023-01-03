using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public abstract class TaskAction
{
    public abstract bool CanSpeak { get; }

    public abstract bool CanListen { get; }

    protected Actor _actor;
    protected TaskAction(Actor actor)
    {
        _actor = actor;
    }

    public abstract void Initialize();

    public abstract void Perform();

    //1 - Completed
    //0 - Incomplete
    //-1 - Cannot be completed

    public abstract int Complete();
}

public struct Navigate : IJob
{

    Vector3Int end;
    Vector3Int start;
    NativeArray<(bool isDoor, Vector3Int)> walkingPath;

    public void Execute()
    {
        RoomNode endNode = Map.Instance[end];
        RoomNode startNode = Map.Instance[start];
        Stack<INode> nodes = new Stack<INode>();
        IEnumerator navigationIter;

        if (!endNode.Empty)
        {
            navigationIter = Map.Instance.NavigateBetweenRooms(startNode, endNode.Occupant);
        }
        else
        {
            if (!endNode.Traversible)
            {
                foreach (RoomNode node in new List<RoomNode> { endNode.GetNodeAs<RoomNode>(Direction.North), endNode.GetNodeAs<RoomNode>(Direction.South), endNode.GetNodeAs<RoomNode>(Direction.West), endNode.GetNodeAs<RoomNode>(Direction.East), endNode.NorthEast, endNode.NorthWest, endNode.SouthEast, endNode.SouthWest })
                {
                    if (node != null && node.Traversible)
                    {
                        endNode = node;
                        break;
                    }
                }
                if (!endNode.Traversible)
                    return;
            }

            navigationIter = Map.Instance.NavigateBetweenRooms(startNode, endNode);
        }

        navigationIter.MoveNext();
        if ((float)navigationIter.Current != float.PositiveInfinity)
        {
            while (navigationIter.MoveNext())
            {
                    nodes.Push(navigationIter.Current as INode);
            }
        }
        else
        {
            Debug.Log("Cannot Reach Location");
            return;
        }

        int pathLength = nodes.Count;

        for (int i = 0; i < pathLength; i++)
        {
            INode node = nodes.Pop();
            walkingPath[i] = (node is ConnectionNode, node.WorldPosition);
        }
    }

    public Navigate(Vector3Int startPosition, Vector3Int endPosition, NativeArray<(bool isDoor, Vector3Int)> walkingPath)
    {
        start = startPosition;
        end = endPosition;
        this.walkingPath = walkingPath;
    }
}

public class Traveling : TaskAction
{
    public Vector3Int Destination { get; private set; }
    //Queue<INode> WalkingPath = new Queue<INode> ();
    bool _ready = false;

    public Queue<INode> WalkingPath { get; } = new Queue<INode>();

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public Traveling(Vector3Int destination, Actor actor) : base(actor)
    {
        Destination = destination;
    }

    public Traveling(SpriteObject destination, Actor actor) : base(actor)
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

        if (!_ready || WalkingPath.Count > 0 || !_actor.Pawn.CurrentStep.IsComplete)
            return 0;
        else if (_actor.Stats.Position == Destination)
        {
            GameManager.MapChanged -= OnMapEdited;
            return 1;
        }
        else if(Map.Instance[Destination].Occupant is IInteractable interactable)
        {
            foreach(RoomNode node in interactable.GetInteractionPoints())
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
            _actor.Pawn.Stance = Stance.Stand;
        }
        if(_actor.Pawn.CurrentStep.IsComplete)
            _actor.Pawn.CurrentStep = new Wait(_actor.Pawn, _actor.Pawn.CurrentStep);
        _actor.Pawn.StartCoroutine(Pathfind());
    }

    void OnMapEdited()
    {
        _ready = false;
        WalkingPath.Clear();
        _actor.Pawn.StartCoroutine(Pathfind());
    }

    IEnumerator Pathfind()
    {
        NativeArray<(bool isDoor, Vector3Int position)> walkingPath = new NativeArray<(bool, Vector3Int)>(100, Allocator.Persistent);
        Navigate navigate = new Navigate(_actor.Stats.Position, Destination, walkingPath);
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
        Pawn pawn = _actor.Pawn;
        if(_ready && pawn.CurrentStep.IsComplete)
        {
            INode node = WalkingPath.Dequeue();
            if (node is RoomNode roomNode)
            {
                pawn.CurrentStep = new Walk(node.WorldPosition, pawn, pawn.CurrentStep);
            }
            else if(node is ConnectionNode connection)
            {
                pawn.CurrentStep = new Traverse(pawn.CurrentNode, connection, pawn, pawn.CurrentStep);
            }
        }
        else if(_actor.Pawn.CurrentStep is Walk && pawn.CurrentStep.IsComplete)
                _actor.Pawn.CurrentStep = new Wait(_actor.Pawn, _actor.Pawn.CurrentStep);
    }
}

public class LayingDown : TaskAction
{

    Bed _bed;

    float _period;
    const float WAITTIME = 0.5f;

    public LayingDown(Bed bed, Actor actor) : base(actor)
    {
        _bed = bed;
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (_bed.Occupied && _bed.Occupant != _actor)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.Pawn.CurrentStep = new Lay(_actor.Pawn, _bed);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}

public class SittingDown : TaskAction
{

    IOccupied _seat;
    public SittingDown(IOccupied seat, Actor actor) : base(actor)
    {
        _seat = seat;
    }

    float _period;
    const float WAITTIME = 0.5f;

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (_seat.Occupied && _seat.Occupant != _actor)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.Pawn.CurrentStep = new Sit(_actor.Pawn, _seat);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}

public class StandingUp : TaskAction
{
    float period = 0;
    public StandingUp(Actor actor) : base(actor)
    {
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return period > 0.5f ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.Pawn.Stance = Stance.Stand;
    }

    public override void Perform()
    {
        period += Time.deltaTime;
    }
}

public class Waiting : TaskAction
{
    float _period = 0;
    float _time;

    public Waiting(Actor actor, float time) : base(actor)
    {
        _time = time;
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return _period > _time ? 1 : 0;
    }

    public override void Initialize()
    {
        TaskStep step = _actor.Pawn.CurrentStep;
        if(step is not Wait && step is not Sit && step is not Lay)
            _actor.Pawn.CurrentStep = new Wait(_actor.Pawn, _actor.Pawn.CurrentStep);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}

public class Sleeping : TaskAction
{
    public Sleeping(Actor actor) : base(actor)
    {
    }

    public override bool CanSpeak => false;

    public override bool CanListen => false;

    public override int Complete()
    {
        if (_actor.Stats.Stance != Stance.Lay)
            return -1;

        return _actor.Stats.Sleep >= 10 ? 1 : 0;
    }

    public override void Initialize()
    {
    }

    public override void Perform()
    {
        _actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 5);
    }
}

public class AcquiringFood : TaskAction
{
    public AcquiringFood(Actor actor, IInteractable interactable) : base(actor) 
    {
        _interactable = interactable;
    }

    float tick = 0;
    IInteractable _interactable;

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return tick > 2 ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.HasFood = true;
        _actor.Pawn.CurrentStep = new Wait(_actor.Pawn, Map.VectorToDir(_interactable.WorldPosition - _actor.Pawn.CurrentPosition));
    }

    public override void Perform() 
    {
        tick += Time.deltaTime;
    }
}

public class Eating : TaskAction
{
    public Eating(Actor actor) : base(actor) {}

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (!_actor.Stats.HasFood)
            return -1;
        if(_actor.Stats.Hunger >= 10)
        {
            _actor.HasFood = false;

            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override void Initialize() {}

    public override void Perform()
    {
        _actor.ChangeNeeds(Needs.Hunger, Time.deltaTime / 1.5f);
    }
}

public class Approaching : TaskAction
{
    public override bool CanSpeak => true;

    public override bool CanListen => true;

    Conversation _conversation;

    bool _complete = false;

    public Approaching(Actor actor, Conversation conversation) : base(actor) 
    {
        _conversation = conversation;
    }

    public override int Complete()
    {
        if (_actor.Pawn.Social.Conversation == null)
            return -1;
        return _complete ? 1 : 0;
    }

    public override void Initialize()
    {
    }

    public override void Perform()
    {
        Pawn pawn = _actor.Pawn;

        if (pawn.CurrentStep.IsComplete)
        {
            (float value, Vector3Int position) best = (_conversation.PositionUtility(pawn.CurrentPosition), pawn.CurrentPosition);

            foreach((RoomNode node, float distance) node in pawn.CurrentNode.NextNodes)
            {
                float value = _conversation.PositionUtility(node.node.WorldPosition);
                if (value < best.value)
                    best = (value, node.node.WorldPosition);
            }
            

            if (best.position == pawn.CurrentPosition)
            {
                _complete = true;
                pawn.CurrentStep = new Wait(_actor.Pawn, Map.VectorToDir(_conversation.Nexus - best.position));
            }
            else
            {
                pawn.CurrentStep = new Walk(best.position, pawn);
            }
        }
    }
}

public class LeavingConversation : TaskAction
{
    public override bool CanSpeak => false;

    public override bool CanListen => true;

    public LeavingConversation(Actor actor) : base(actor){}

    float _period;
    const float DELAY = 0.5f;

    public override int Complete()
    {
        return _period > DELAY ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.Pawn.Social.EndConversation();
        _actor.Pawn.CurrentStep = new Wait(_actor.Pawn, _actor.Pawn.CurrentStep);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}

public class Questing : TaskAction
{
    public override bool CanSpeak => false;

    public override bool CanListen => false;

    public Questing(Actor actor) : base(actor){}

    public override int Complete()
    {
        return 1;
    }

    public override void Initialize()
    {
        _actor.Pawn.gameObject.SetActive(false);
    }

    public override void Perform()
    {
    }
}
