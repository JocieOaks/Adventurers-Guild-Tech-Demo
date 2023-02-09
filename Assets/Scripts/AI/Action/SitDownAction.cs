using UnityEngine;

public class SitDownAction : TaskAction
{

    IOccupied _seat;
    public SitDownAction(IOccupied seat, Actor actor) : base(actor)
    {
        _seat = seat;
    }

    float _period;
    const float WAITTIME = 0.5f;

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (_seat.Occupied && _seat.Occupant != _pawn)
            return -1;
        return _period > WAITTIME ? 1 : 0;
    }

    public override void Initialize()
    {
        _pawn.CurrentStep = new SitStep(_pawn, _seat);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
