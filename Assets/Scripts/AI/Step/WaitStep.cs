using UnityEngine;

public class WaitStep : TaskStep, IDirected
{ 
    public Direction Direction { get; private set; } = Direction.South;
    RoomNode roomNode;
    int animationIndex = 30;
    public WaitStep(Pawn pawn, TaskStep step) : base(pawn)
    {
        roomNode = pawn.CurrentNode;
        roomNode.Standing = pawn;
        if(step is IDirected directed)
        {
            SetDirection(directed.Direction);
        }
    }

    public WaitStep(Pawn pawn, Direction direction) : base(pawn)
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
