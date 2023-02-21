using UnityEngine;

/// <summary>
/// The <see cref="SleepAction"/> class is a <see cref="TaskAction"/> for when a <see cref="AdventurerPawn"/> is sleeping.
/// </summary>
public class SleepAction : TaskAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SleepAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> that is sleeping.</param>
    public SleepAction(Actor actor) : base(actor)
    {
    }

    /// <inheritdoc/>
    public override bool CanSpeak => false;

    /// <inheritdoc/>
    public override bool CanListen => false;

    /// <inheritdoc/>
    public override int Complete()
    {
        if (_actor.Stats.Stance != Stance.Lay)
            return -1;

        return _actor.Stats.Sleep >= 10 ? 1 : 0;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    /// <inheritdoc/>
    public override void Perform()
    {
        _actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 5);
    }
}
