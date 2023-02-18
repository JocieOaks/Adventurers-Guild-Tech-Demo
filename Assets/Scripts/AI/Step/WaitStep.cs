using UnityEngine;

public class WaitStep : TaskStep, IDirected
{ 
    public Direction Direction { get; private set; } = Direction.South;
    RoomNode roomNode;
    int animationIndex = 30;
    public WaitStep(Pawn pawn, TaskStep step, bool blocking) : this(pawn, step is IDirected directed ? directed.Direction : Direction.South, blocking) { }

    public WaitStep(Pawn pawn, Direction direction, bool blocking) : base(pawn)
    {
        roomNode = pawn.CurrentNode;
        if(blocking)
            roomNode.Occupant = pawn;
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

        try
        {
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
        catch (System.IndexOutOfRangeException e)
        {

            throw e;
        }
    }

    protected override void Finish()
    {
        if(roomNode.Occupant == _pawn)
            roomNode.Occupant = null;
    }
}
