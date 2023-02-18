using UnityEngine;

public class SitStep : TaskStep, IDirected
{
    IOccupied _seat;
    public Direction Direction { get; }
    public SitStep(Pawn pawn, IOccupied seat) : base(pawn)
    {
        _seat = seat;

        seat.Enter(pawn);

        if(seat is ChairSprite chair)
        {
            Direction = chair.Direction;
        }
        else if(seat is StoolSprite stool)
        {
            if (Map.Instance[stool.WorldPosition + Map.DirectionToVector(Direction.North) * 2].Occupant is BarSprite)
            {
                Direction = Direction.North;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirectionToVector(Direction.South) * 2].Occupant is BarSprite)
            {
                Direction = Direction.South;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirectionToVector(Direction.East) * 2].Occupant is BarSprite)
            {
                Direction = Direction.East;
            }
            else if (Map.Instance[stool.WorldPosition + Map.DirectionToVector(Direction.West) * 2].Occupant is BarSprite)
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
