using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskStep
{

    protected Pawn _pawn;

    public bool IsComplete
    {
        get
        {
            if(_isComplete)
            {
                Finish();
                return true;
            }
            return false;
        }
    }
    protected abstract bool _isComplete { get; }

    protected static int[] _idleFrames = new int[] { 1, 2, 3, 4, 5, 5, 5, 5, 5, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    protected float period = 0f;
    protected int frame = 0;
    protected const float STEPTIME = 0.25f;
    protected const float BREATHTIME = 0.125f;

    protected TaskStep(Pawn pawn)
    {
        _pawn = pawn;
    }

    public abstract void Perform();

    protected virtual void Finish() { }
}

public interface IDirected
{
    Direction Direction { get; }
}

public class Walk : TaskStep, IDirected
{
    int _animationOffset;
    Vector3Int _end;
    Vector3 _step;
    bool _isFinished = false;
    public Direction Direction { get; }

    public Walk(Vector3Int end, Pawn pawn, TaskStep step) : this(end, pawn)
    {
        if (step is Walk walk)
        {
            period = walk.period;
            frame = walk.frame;
        }
    }

    public Walk(Vector3Int end, Pawn pawn) : base(pawn)
    {
        _end = end;

        Vector3 gameVector = end - pawn.WorldPosition;
        _step = gameVector.normalized * pawn.Speed;


        _isFinished = _step.sqrMagnitude < 0.01;

        int best = 0;
        float best_product = Vector2.Dot(gameVector, new Vector2(1, 1).normalized);

        for (int i = 1; i < 8; i++)
        {
            float value;

            switch (i)
            {
                case 1:
                    value = Vector2.Dot(gameVector, new Vector2(1, 0).normalized);
                    break;
                case 2:
                    value = Vector2.Dot(gameVector, new Vector2(1, -1).normalized);
                    break;
                case 3:
                    value = Vector2.Dot(gameVector, new Vector2(0, -1).normalized);
                    break;
                case 4:
                    value = Vector2.Dot(gameVector, new Vector2(-1, -1).normalized);
                    break;
                case 5:
                    value = Vector2.Dot(gameVector, new Vector2(-1, 0).normalized);
                    break;
                case 6:
                    value = Vector2.Dot(gameVector, new Vector2(-1, 1).normalized);
                    break;
                default:
                    value = Vector2.Dot(gameVector, new Vector2(0, 1).normalized);
                    break;
            }

            if (value > best_product)
            {
                best_product = value;
                best = i;
            }
        }

        switch (best)
        {
            case 0:
                _animationOffset = 0;
                Direction = Direction.NorthEast;
                break;
            case 1:
                _animationOffset = 36;
                Direction = Direction.East;
                break;
            case 2:
                _animationOffset = 4;
                Direction = Direction.SouthEast;
                break;
            case 3:
                _animationOffset = 16;
                Direction = Direction.South;
                break;
            case 4:
                _animationOffset = 12;
                Direction = Direction.SouthWest;
                break;
            case 5:
                _animationOffset = 20;
                Direction = Direction.West;
                break;
            case 6:
                _animationOffset = 8;
                Direction = Direction.NorthWest;
                break;
            case 7:
                _animationOffset = 40;
                Direction = Direction.North;
                break;
        }
    }

    protected override bool _isComplete
    {
        get
        {
            return Vector3.Dot(_end - _pawn.WorldPosition, _step) < 0 || _isFinished;
        }
    }

    public override void Perform()
    {
        if (!_isComplete)
        {
            _pawn.WorldPosition += _step * Time.deltaTime;
            period += Time.deltaTime;

            if (period >= frame * STEPTIME)
            {
                _pawn.SetSprite(_animationOffset + frame);
                frame++;
                if (frame == 4)
                {
                    period -= 1f;
                    frame = 0;
                }
            }
        }
    }
}

public class Lay : TaskStep
{

    Bed _bed;
    public Lay(Pawn pawn, Bed bed) : base(pawn)
    {
        _bed = bed;
        pawn.Stance = Stance.Lay;
        _bed.Enter(pawn);
    }

    protected override bool _isComplete => true;

    public override void Perform()
    {
        period += Time.deltaTime;

        if (period >= frame * BREATHTIME)
        {
            _pawn.SetSprite(24 + +_idleFrames[frame]);
            frame++;
            if (frame == 22)
            {
                period -= 2.75f;
                frame = 0;
            }
        }
    }

    protected override void Finish()
    {
        _bed.Exit(_pawn);
    }
}

public class Sit : TaskStep, IDirected
{
    IOccupied _seat;
    public Direction Direction { get; }
    public Sit(Pawn pawn, IOccupied seat) : base(pawn)
    {
        _seat = seat;

        seat.Enter(pawn);

        if(seat is Chair chair)
        {
            Direction = chair.Direction;
        }
        else if(seat is Stool stool)
        {
            if (Map.Instance[stool.WorldPosition + Map.DirToVector(Direction.North) * 2].Occupant is Bar)
            {
                Direction = Direction.North;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirToVector(Direction.South) * 2].Occupant is Bar)
            {
                Direction = Direction.South;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirToVector(Direction.East) * 2].Occupant is Bar)
            {
                Direction = Direction.East;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirToVector(Direction.West) * 2].Occupant is Bar)
            {
                Direction = Direction.West;
            }
        }
        else
            Direction = Direction.North;

        pawn.Stance = Stance.Sit;
    }

    protected override bool _isComplete => true;

    public override void Perform()
    {
        period += Time.deltaTime;
        if (Direction == Direction.West)
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(24 + _idleFrames[frame]);
                frame++;
                if (frame == 22)
                {
                    period -= 2.75f;
                    frame = 0;
                }
            }
        }
        else if (Direction == Direction.South)
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(30 + _idleFrames[frame]);
                frame++;
                if (frame == 22)
                {
                    period -= 2.75f;
                    frame = 0;
                }
            }
        }
        else if (Direction == Direction.East)
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(47);
                frame += 100;
            }
        }
        else
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(46);
                frame += 100;
            }
        }
    }

    protected override void Finish()
    {
        _seat.Exit(_pawn);
    }
}

public class Wait : TaskStep, IDirected
{ 
    public Direction Direction { get; private set; } = Direction.South;
    RoomNode roomNode;
    int animationIndex = 30;
    public Wait(Pawn pawn, TaskStep step) : base(pawn)
    {
        roomNode = pawn.CurrentNode;
        roomNode.Standing = pawn;
        if(step is IDirected directed)
        {
            SetDirection(directed.Direction);
        }
    }

    public Wait(Pawn pawn, Direction direction) : base(pawn)
    {
        roomNode = pawn.CurrentNode;
        roomNode.Standing = pawn;
        SetDirection(direction);
    }

    public void SetDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                Direction = Direction.North;
                animationIndex = 44;
                break;
            case Direction.NorthEast:
            case Direction.East:
                Direction = Direction.East;
                animationIndex = 45;
                break;
            case Direction.SouthEast:
            case Direction.South:
            case Direction.SouthWest:
                Direction = Direction.South;
                animationIndex = 30;
                break;
            default:
                Direction = Direction.West;
                animationIndex = 24;
                break;
        }
    }

    protected override bool _isComplete => true;

    public override void Perform()
    {
        period += Time.deltaTime;

        if (Direction == Direction.West || Direction == Direction.South)
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(animationIndex + _idleFrames[frame]);
                frame++;
                if (frame == 22)
                {
                    period -= 2.75f;
                    frame = 0;
                }
            }
        }
        else
        {
            if (period >= frame * BREATHTIME)
            {
                _pawn.SetSprite(animationIndex);
                frame += 100;
            }
        }
    }

    protected override void Finish()
    {
        roomNode.Standing = null;
    }
}

public class Traverse : Walk
{
    ConnectionNode _connection;
    Room _oldRoom;
    Room _newRoom;

    public Traverse(RoomNode start, ConnectionNode connection, Pawn pawn, TaskStep step) : base(connection.GetRoomNode(start).WorldPosition, pawn, step)
    {
        _connection = connection;
        _oldRoom = start.Room;
        _newRoom = connection.GetConnectedRoom(_oldRoom);

    }

    protected override void Finish()
    {
        _oldRoom.ExitRoom(_pawn);
        _newRoom.EnterRoom(_pawn);
    }
}
