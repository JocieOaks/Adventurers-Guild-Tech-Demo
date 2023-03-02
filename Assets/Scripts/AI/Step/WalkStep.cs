using UnityEngine;

/// <summary>
/// The <see cref="WalkStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to move from one <see cref="RoomNode"/> to another nearby <see cref="RoomNode"/>.
/// </summary>
public class WalkStep : TaskStep, IDirected
{
    readonly int _animationOffset;
    Vector3Int _end;
    readonly bool _isFinished = false;
    Vector3 _step;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkStep"/> class. Checks if the previous <see cref="TaskStep"/> of the <see cref="Pawn"/> 
    /// is also a <see cref="WalkStep"/> so that the animations work properly.</param>
    /// </summary>
    /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="Pawn"/>'s destination. 
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> to be performing.</param>
    public WalkStep(Vector3Int end, AdventurerPawn pawn, TaskStep step) : this(end, pawn)
    {
        if (step is WalkStep walk)
        {
            _period = walk._period;
            _frame = walk._frame;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkStep"/> class.
    /// </summary>
    /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="AdventurerPawn"/>'s destination.
    /// <param name="pawn">The <see cref="AdventurerPawn"/> performing the <see cref="WalkStep"/>.</param>
    public WalkStep(Vector3Int end, AdventurerPawn pawn) : base(pawn)
    {
        if(pawn.Occupying != null)
        {
            pawn.Stance = Stance.Stand;
            pawn.Occupying.Exit(pawn, end);
        }

        _end = end;

        Vector2 gameVector = end - pawn.WorldPositionNonDiscrete;
        _step = gameVector.normalized;

        if (pawn.WorldPosition == end)
        {
            _isFinished = true;
            return;
        }

        int best = 0;
        float best_product = Vector2.Dot(gameVector, new Vector2(1, 1).normalized);

        for (int i = 1; i < 8; i++)
        {
            var value = i switch
            {
                1 => Vector2.Dot(gameVector, new Vector2(1, 0).normalized),
                2 => Vector2.Dot(gameVector, new Vector2(1, -1).normalized),
                3 => Vector2.Dot(gameVector, new Vector2(0, -1).normalized),
                4 => Vector2.Dot(gameVector, new Vector2(-1, -1).normalized),
                5 => Vector2.Dot(gameVector, new Vector2(-1, 0).normalized),
                6 => Vector2.Dot(gameVector, new Vector2(-1, 1).normalized),
                _ => Vector2.Dot(gameVector, new Vector2(0, 1).normalized),
            };
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkStep"/> class by giving a <see cref="global::Direction"/> that will be traveled indefinitely.
    /// </summary>
    /// <param name="direction"><see cref="global::Direction"/> the <see cref="Pawn"/> will walk.</param>
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
    public WalkStep(Direction direction, Pawn pawn, TaskStep step) : base(pawn)
    {
        Direction = direction;

        if (step is WalkStep walk)
        {
            _period = walk._period;
            _frame = walk._frame;
        }

        _animationOffset = direction switch
        {
            Direction.NorthEast => 0,
            Direction.East => 36,
            Direction.SouthEast => 4,
            Direction.South => 16,
            Direction.SouthWest => 12,
            Direction.West => 20,
            Direction.NorthWest => 8,
            Direction.North => 40,
            _ => 0
        };

        _step = Map.DirectionToVectorNormalized(direction);
    }

    /// <inheritdoc/>
    public Direction Direction { get; }

    /// <inheritdoc/>
    protected override bool _isComplete
    {
        get
        {
            //If end is not defined, WalkStep continues infinitely and must be changed manually.
            if(_end == default)
                return false;
            return Vector3.Dot(_end - _pawn.WorldPositionNonDiscrete, _step) < 0 || _isFinished;
        }
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        if (!_isComplete)
        {
            _pawn.WorldPositionNonDiscrete += Time.deltaTime * _pawn.Speed * _step;
            _period += Time.deltaTime * _pawn.Speed;

            if (_period >= _frame * STEPTIME)
            {
                _pawn.SetSprite(_animationOffset + _frame);
                _frame++;
                if (_frame >= 4)
                {
                    _period -= 2.5f;
                    _frame = 0;
                }
            }
        }
    }
}
