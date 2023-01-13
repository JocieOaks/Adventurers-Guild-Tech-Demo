using UnityEngine;

public class StandUpAction : TaskAction
{
    float period = 0;
    public StandUpAction(Actor actor) : base(actor)
    {
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return period > 0.5f ? 1 : 0;
    }

    public override void Initialize()
    {
        _pawn.Stance = Stance.Stand;
    }

    public override void Perform()
    {
        period += Time.deltaTime;
    }
}
