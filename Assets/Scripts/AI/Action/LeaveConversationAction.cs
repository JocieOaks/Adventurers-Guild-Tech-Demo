using UnityEngine;

public class LeaveConversationAction : TaskAction
{
    public override bool CanSpeak => false;

    public override bool CanListen => true;

    public LeaveConversationAction(Actor actor) : base(actor){}

    float _period;
    const float DELAY = 0.5f;

    public override int Complete()
    {
        return _period > DELAY ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.Pawn.Social.EndConversation();
        _actor.Pawn.CurrentStep = new WaitStep(_actor.Pawn, _actor.Pawn.CurrentStep);
    }

    public override void Perform()
    {
        _period += Time.deltaTime;
    }
}
