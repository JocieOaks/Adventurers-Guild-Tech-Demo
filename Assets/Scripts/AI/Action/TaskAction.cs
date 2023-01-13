public abstract class TaskAction
{
    public abstract bool CanSpeak { get; }

    public abstract bool CanListen { get; }

    protected Actor _actor;

    protected Pawn _pawn;
    protected TaskAction(Actor actor)
    {
        _actor = actor;
        _pawn = actor.Pawn;
    }

    public abstract void Initialize();

    public abstract void Perform();

    //1 - Completed
    //0 - Incomplete
    //-1 - Cannot be completed

    public abstract int Complete();
}