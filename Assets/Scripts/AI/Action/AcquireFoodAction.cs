using UnityEngine;

public class AcquireFoodAction : TaskAction
{
    public AcquireFoodAction(Actor actor, IInteractable interactable) : base(actor) 
    {
        _interactable = interactable;
    }

    float tick = 0;
    IInteractable _interactable;

    public override bool CanSpeak => true;

    public override bool CanListen => true;

    public override int Complete()
    {
        return tick > 2 ? 1 : 0;
    }

    public override void Initialize()
    {
        _actor.HasFood = true;
        _actor.Pawn.CurrentStep = new WaitStep(_actor.Pawn, Map.VectorToDir(_interactable.WorldPosition - _actor.Pawn.WorldPosition));
    }

    public override void Perform() 
    {
        tick += Time.deltaTime;
    }
}
