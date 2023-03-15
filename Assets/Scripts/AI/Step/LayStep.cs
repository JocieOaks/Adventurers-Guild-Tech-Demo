﻿using UnityEngine;

/// <summary>
/// The <see cref="SitStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to lay down.
/// </summary>
public class LayStep : TaskStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayStep"/> class.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> that is laying down.</param>
    /// <param name="bed">The <see cref="BedSprite"/> the <see cref="Pawn"/> is laying in.</param>
    public LayStep(Pawn pawn, BedSprite bed) : base(pawn)
    {
        pawn.Stance = Stance.Lay;
        bed.Enter(pawn);
    }

    /// <inheritdoc/>
    protected override bool _isComplete => true;

    /// <inheritdoc/>
    public override void Perform()
    {
        _period += Time.deltaTime;

        if (_period >= _frame * BREATHTIME)
        {
            _pawn.SetSprite(34);//24 + _idleFrames[_frame]);
            _frame++;
            if (_frame == 22)
            {
                _period -= 2.75f;
                _frame = 0;
            }
        }
    }
}
