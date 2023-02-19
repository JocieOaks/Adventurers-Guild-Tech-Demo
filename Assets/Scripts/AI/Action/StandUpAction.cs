using UnityEngine;

/// <summary>
/// The <see cref="StandUpAction"/> class is a <see cref="TaskAction"/> for standing up.
/// </summary>
public class StandUpAction : TaskAction
{
    float period = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandUpAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> that is standing up.</param>
    public StandUpAction(Actor actor) : base(actor)
    {
    }

    /// <inheritdoc/>
    public override bool CanListen => true;

    /// <inheritdoc/>
    public override bool CanSpeak => true;

    /// <inheritdoc/>
    public override int Complete()
    {
        return period > 0.5f ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        _pawn.Stance = Stance.Stand;
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        period += Time.deltaTime;
    }
}
