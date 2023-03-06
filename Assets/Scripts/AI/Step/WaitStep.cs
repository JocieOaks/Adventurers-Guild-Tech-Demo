using UnityEngine;

/// <summary>
/// The <see cref="WaitStep"/> class is a <see cref="TaskStep"/> for when a <see cref="Pawn"/> is waiting.
/// </summary>
public class WaitStep : TaskStep, IDirected
{
    int animationIndex = 30;
    readonly RoomNode _roomNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitStep"/> class. Checks the direction of the previous <see cref="TaskStep"/> if it is <see cref="IDirected"/> 
    /// to determine the <see cref="global::Direction"/> for the <see cref="Pawn"/> to face.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> that is waiting.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
    /// <param name="blocking">Determines if the <see cref="Pawn"/> blocks the <see cref="RoomNode"/> from being traversed by other <see cref="Pawn"/>s.</param>
    public WaitStep(Pawn pawn, TaskStep step, bool blocking) : this(pawn, step is IDirected directed ? directed.Direction : Direction.South, blocking) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitStep"/> class.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> that is waiting.</param>
    /// <param name="direction">The <see cref="global::Direction"/> the <see cref="Pawn"/> should be facing.</param>
    /// <param name="blocking">Determines if the <see cref="Pawn"/> blocks the <see cref="RoomNode"/> from being traversed by other <see cref="Pawn"/>s.</param>
    public WaitStep(Pawn pawn, Direction direction, bool blocking) : base(pawn)
    {
        _roomNode = pawn.CurrentNode;
        if (blocking)
            _roomNode.Occupant = pawn;
        SetDirection(direction);
    }

    /// <inheritdoc/>
    public Direction Direction { get; private set; } = Direction.South;

    /// <inheritdoc/>
    protected override bool _isComplete => true;

    /// <inheritdoc/>
    public override void Perform()
    {
        _period += Time.deltaTime;

        try
        {
            if (Direction == Direction.West || Direction == Direction.South)
            {
                if (_period >= _frame * BREATHTIME)
                {
                    _pawn.SetSprite(animationIndex + _idleFrames[_frame]);
                    _frame++;
                    if (_frame >= 22)
                    {
                        _period -= 2.75f;
                        _frame = 0;
                    }
                }
            }
            else
            {
                if (_period >= _frame * BREATHTIME)
                {
                    _pawn.SetSprite(animationIndex);
                    _frame += 100;
                }
            }
        }
        catch (System.IndexOutOfRangeException e)
        {

            throw e;
        }
    }

    /// <summary>
    /// Set the <see cref="global::Direction"/> for the <see cref="Pawn"/> to face while waiting.
    /// </summary>
    /// <param name="direction">The <see cref="global::Direction"/> for the <see cref="Pawn"/> to face.</param>
    public void SetDirection(Direction direction)
    {
        Direction = direction;

        animationIndex = direction switch
        {
            Direction.North => 44,
            Direction.NorthEast => 45,
            Direction.East => 45,
            Direction.SouthEast => 30,
            Direction.South => 30,
            Direction.SouthWest => 30,
            _ => 24
        };
        _period = Mathf.Clamp(_period, 0, 2.75f);
    }

    /// <inheritdoc/>
    protected override void Finish()
    {
        if(Equals(_roomNode.Occupant, _pawn))
            _roomNode.Occupant = null;
    }
}
