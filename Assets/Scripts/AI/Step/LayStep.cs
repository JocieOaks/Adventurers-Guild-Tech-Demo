using UnityEngine;

public class LayStep : TaskStep
{

    BedSprite _bed;
    public LayStep(Pawn pawn, BedSprite bed) : base(pawn)
    {
        _bed = bed;
        pawn.Stance = Stance.Lay;
        _bed.Enter(pawn);
    }

    protected override bool _isComplete => true;

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

    protected override void Finish()
    {
        _bed.Exit(_pawn);
    }
}
