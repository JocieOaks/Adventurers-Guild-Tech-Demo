﻿using UnityEngine;

public class LayDownAction : TaskAction
{

    BedSprite _bed;

    float _period;
    const float WAITTIME = 0.5f;

    public LayDownAction(BedSprite bed, Actor actor) : base(actor)
    {
        _bed = bed;
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (_bed.Occupied && _bed.Occupant != _pawn)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    public override void Initialize()
    {
        _pawn.CurrentStep = new LayStep(_pawn, _bed);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
