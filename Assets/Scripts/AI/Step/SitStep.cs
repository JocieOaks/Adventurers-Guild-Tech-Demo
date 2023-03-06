using UnityEngine;

/// <summary>
/// The <see cref="SitStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to sit down.
/// </summary>
public class SitStep : TaskStep, IDirected
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SitStep"/> task.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> that is sitting down.</param>
    /// <param name="seat">The seat on which the <see cref="Pawn"/> is sitting.</param>
    public SitStep(Pawn pawn, IOccupied seat) : base(pawn)
    {
        seat.Enter(pawn);

        if (seat is ChairSprite chair)
        {
            Direction = chair.Direction;
        }
        else if (seat is StoolSprite stool)
        {
            if (Map.Instance[stool.WorldPosition + Utility.DirectionToVector(Direction.North) * 2].Occupant is BarSprite)
            {
                Direction = Direction.North;
            }
            else if (Map.Instance[stool.WorldPosition + Utility.DirectionToVector(Direction.South) * 2].Occupant is BarSprite)
            {
                Direction = Direction.South;
            }
            else if (Map.Instance[stool.WorldPosition + Utility.DirectionToVector(Direction.East) * 2].Occupant is BarSprite)
            {
                Direction = Direction.East;
            }
            else if (Map.Instance[stool.WorldPosition + Utility.DirectionToVector(Direction.West) * 2].Occupant is BarSprite)
            {
                Direction = Direction.West;
            }
        }
        else
            Direction = Direction.North;

        pawn.Stance = Stance.Sit;
    }

    /// <inheritdoc/>
    public Direction Direction { get; }

    /// <inheritdoc/>
    protected override bool _isComplete => true;

    /// <inheritdoc/>
    public override void Perform()
    {
        _period += Time.deltaTime;
        if (Direction == Direction.West)
        {
            if (_period >= _frame * BREATHTIME)
            {
                _pawn.SetSprite(24 + _idleFrames[_frame]);
                _frame++;
                if (_frame == 22)
                {
                    _period -= 2.75f;
                    _frame = 0;
                }
            }
        }
        else if (Direction == Direction.South)
        {
            if (_period >= _frame * BREATHTIME)
            {
                _pawn.SetSprite(30 + _idleFrames[_frame]);
                _frame++;
                if (_frame == 22)
                {
                    _period -= 2.75f;
                    _frame = 0;
                }
            }
        }
        else if (Direction == Direction.East)
        {
            if (_period >= _frame * BREATHTIME)
            {
                _pawn.SetSprite(47);
                _frame += 100;
            }
        }
        else
        {
            if (_period >= _frame * BREATHTIME)
            {
                _pawn.SetSprite(46);
                _frame += 100;
            }
        }
    }
}
