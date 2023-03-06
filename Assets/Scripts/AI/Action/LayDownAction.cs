using UnityEngine;

/// <summary>
/// The <see cref="LayDownAction"/> class is a <see cref="TaskAction"/> for a <see cref="AdventurerPawn"/> to lay down.
/// </summary>
public class LayDownAction : TaskAction
{

    const float WAITTIME = 0.5f;
    readonly BedSprite _bed;
    float _period;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayDownAction"/>.
    /// </summary>
    /// <param name="bed">The <see cref="BedSprite"/> the <see cref="Actor"/> is laying in.</param>
    /// <param name="pawn">The <see cref="Pawn"/> laying down.</param>
    public LayDownAction(BedSprite bed, Pawn pawn) : base(pawn)
    {
        _bed = bed;
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        if (_bed.Occupied && _bed.Occupant != _pawn)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        _pawn.CurrentStep = new LayStep(_pawn, _bed);
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
