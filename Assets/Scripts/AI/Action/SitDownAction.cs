using UnityEngine;

/// <summary>
/// The <see cref="SitDownAction"/> class is a <see cref="TaskAction"/> for sitting down.
/// </summary>
public class SitDownAction : TaskAction
{

    const float WAITTIME = 0.5f;
    float _period;
    readonly IOccupied _seat;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitDownAction"/> class.
    /// </summary>
    /// <param name="seat">The seat the <see cref="Actor"/> is sitting in.</param>
    /// <param name="actor">The <see cref="Actor"/> that is sitting.</param>
    public SitDownAction(IOccupied seat, Actor actor) : base(actor)
    {
        _seat = seat;
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        if (_seat.Occupied && _seat.Occupant != _pawn)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        _pawn.CurrentStep = new SitStep(_pawn, _seat);
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
