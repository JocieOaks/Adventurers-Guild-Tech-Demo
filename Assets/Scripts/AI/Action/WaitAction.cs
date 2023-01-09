using UnityEngine;

public class WaitAction : TaskAction
{
    float _period = 0;
    float _time;

    public WaitAction(Actor actor, float time) : base(actor)
    {
        _time = time;
    }

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return _period > _time ? 1 : 0;
    }

    public override void Initialize()
    {
        TaskStep step = _actor.Pawn.CurrentStep;
        if(step is not WaitStep && step is not SitStep && step is not LayStep)
            _actor.Pawn.CurrentStep = new WaitStep(_actor.Pawn, _actor.Pawn.CurrentStep);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
