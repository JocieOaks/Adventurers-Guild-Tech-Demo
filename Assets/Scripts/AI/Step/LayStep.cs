using UnityEngine;

/// <summary>
/// The <see cref="SitStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to lay down.
/// </summary>
public class LayStep : TaskStep
{
    readonly BedSprite _bed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayStep"/> class.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> that is laying down.</param>
    /// <param name="bed">The <see cref="BedSprite"/> the <see cref="Pawn"/> is laying in.</param>
    public LayStep(Pawn pawn, BedSprite bed) : base(pawn)
    {
        _bed = bed;
        pawn.Stance = Stance.Lay;
        _bed.Enter(pawn);
    }

    /// <inheritdoc/>
    protected override bool _isComplete => true;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void Finish()
    {
        _bed.Exit(_pawn);
    }
}
