using UnityEngine;

/// <summary>
/// The <see cref="WaitAction"/> class is a <see cref="TaskAction"/> for when a <see cref="AdventurerPawn"/> is waiting.
/// </summary>
public class WaitAction : TaskAction
{
    float _period = 0;
    readonly float _time;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitAction"/> class.
    /// </summary>
    /// <param name="time">The length of time in seconds for the <see cref="Actor"/> to be waiting.</param>
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WaitAction"/>.</param>
    public WaitAction(float time, Pawn pawn) : base(pawn)
    {
        _time = time;
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        return _period > _time ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        TaskStep step = _pawn.CurrentStep;
        if (step is not SitStep && step is not LayStep)
        {
            if (_pawn.CurrentNode.Reserved)
            {
                if (_pawn is AdventurerPawn)
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
                else if(step is not WaitStep)
                _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, false);

            }
            else if (step is not WaitStep)
                _pawn.CurrentStep = new WaitStep(_pawn, _pawn.CurrentStep, true);
        }
    }

    /// <inheritdoc/>
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
