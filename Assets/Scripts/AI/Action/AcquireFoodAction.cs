using UnityEngine;

/// <summary>
/// The <see cref="AcquireFoodAction"/> class is a <see cref="TaskAction"/> for a <see cref="Pawn"/> to obtain food. Will be replaced when the inventory system is more fleshed out.
/// </summary>
public class AcquireFoodAction : TaskAction
{
    IInteractable _interactable;

    float tick = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcquireFoodAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> that is getting food.</param>
    /// <param name="interactable">The <see cref="IInteractable"/> from which <c>actor</c> is getting food.</param>
    public AcquireFoodAction(Actor actor, IInteractable interactable) : base(actor)
    {
        _interactable = interactable;
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        return tick > 2 ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        _actor.HasFood = true;
        _actor.Pawn.CurrentStep = new WaitStep(_actor.Pawn, Map.VectorToDir(_interactable.WorldPosition - _actor.Pawn.WorldPosition), true);
    }

    /// <inheritdoc/>
    public override void Perform() 
    {
        tick += Time.deltaTime;
    }
}
