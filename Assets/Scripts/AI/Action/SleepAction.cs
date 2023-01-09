using UnityEngine;

public class SleepAction : TaskAction
{
    public SleepAction(Actor actor) : base(actor)
    {
    }

    public override bool CanSpeak => false;

    public override bool CanListen => false;

    public override int Complete()
    {
        if (_actor.Stats.Stance != Stance.Lay)
            return -1;

        return _actor.Stats.Sleep >= 10 ? 1 : 0;
    }

    public override void Initialize()
    {
    }

    public override void Perform()
    {
        _actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 5);
    }
}
