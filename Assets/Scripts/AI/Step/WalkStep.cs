using UnityEngine;

/// <summary>
/// The <see cref="WalkStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to move from one <see cref="RoomNode"/> to another nearby <see cref="RoomNode"/>.
/// </summary>
public class WalkStep : TaskStep, IDirected
{
    int _animationOffset;
    Vector3Int _end;
    bool _isFinished = false;
    Vector3 _step;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkStep"/> class. Checks if the previous <see cref="TaskStep"/> of the <see cref="Pawn"/> 
    /// is also a <see cref="WalkStep"/> so that the animations work properly.</param>
    /// </summary>
    /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="Pawn"/>'s destination. 
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> to be performing.</param>
    public WalkStep(Vector3Int end, Pawn pawn, TaskStep step) : this(end, pawn)
    {
        if (step is WalkStep walk)
        {
            period = walk.period;
            frame = walk.frame;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkStep"/> class.
    /// </summary>
    /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="Pawn"/>'s destination.
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
    public WalkStep(Vector3Int end, Pawn pawn) : base(pawn)
    {
        _end = end;

        Vector3 gameVector = end - pawn.WorldPositionNonDiscrete;
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

    /// <inheritdoc/>
    public Direction Direction { get; }

    /// <inheritdoc/>
    protected override bool _isComplete
    {
        get
        {
            return Vector3.Dot(_end - _pawn.WorldPositionNonDiscrete, _step) < 0 || _isFinished;
        }
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        if (!_isComplete)
        {
            _pawn.WorldPositionNonDiscrete += _step * Time.deltaTime;
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
