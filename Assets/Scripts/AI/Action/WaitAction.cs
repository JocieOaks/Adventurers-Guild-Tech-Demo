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
        TaskStep step = _pawn.CurrentStep;
        if(_pawn.CurrentNode.Reserved)
        {
            foreach ((RoomNode node, float) node in _pawn.CurrentNode.NextNodes)
            {
                if (!node.node.Reserved)
                {
                    _pawn.CurrentStep = new WalkStep(node.node.WorldPosition, _pawn, step);
                    return;
                }
            }
            Debug.Log("No Unreserved Location");
        }
        else if(step is not WaitStep && step is not SitStep && step is not LayStep)
            _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, true);
    }

    public override void Perform()
    {
        if(_pawn.CurrentStep is WalkStep)
        {
            if(_pawn.CurrentStep.IsComplete())
            {
                _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, true);
            }
        }
        _period += Time.deltaTime;
    }
}
