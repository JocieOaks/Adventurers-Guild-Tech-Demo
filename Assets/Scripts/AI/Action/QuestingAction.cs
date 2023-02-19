/// <summary>
/// The <see cref="QuestingAction"/> class is a <see cref="TaskAction"/> for when a <see cref="Pawn"/> is on a <see cref="Quest"/>.
/// </summary>
public class QuestingAction : TaskAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuestingAction"/> class.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> going on the <see cref="Quest"/>.</param>
    public QuestingAction(Actor actor) : base(actor) { }

    /// <inheritdoc/>
    public override bool CanListen => false;

    /// <inheritdoc/>
    public override bool CanSpeak => false;

    /// <inheritdoc/>
    public override int Complete()
    {
        return 1;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        _pawn.gameObject.SetActive(false);
    }

    /// <inheritdoc/>
    public override void Perform()
    {
    }
}
