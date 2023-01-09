using UnityEngine;

public class EatAction : TaskAction
{
    public EatAction(Actor actor) : base(actor) {}

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        if (!_actor.Stats.HasFood)
            return -1;
        if(_actor.Stats.Hunger >= 10)
        {
            _actor.HasFood = false;

            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override void Initialize() {}

    public override void Perform()
    {
        _actor.ChangeNeeds(Needs.Hunger, Time.deltaTime / 1.5f);
    }
}
