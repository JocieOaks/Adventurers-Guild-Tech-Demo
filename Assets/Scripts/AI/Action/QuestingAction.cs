public class QuestingAction : TaskAction
{
    public override bool CanSpeak => false;

    public override bool CanListen => false;

    public QuestingAction(Actor actor) : base(actor){}

    public override int Complete()
    {
        return 1;
    }

    public override void Initialize()
    {
        _pawn.gameObject.SetActive(false);
    }

    public override void Perform()
    {
    }
}
