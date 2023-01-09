public abstract class TaskStep
{

    protected Pawn _pawn;

    public bool IsComplete
    {
        get
        {
            if(_isComplete)
            {
                Finish();
                return true;
            }
            return false;
        }
    }
    protected abstract bool _isComplete { get; }

    protected static int[] _idleFrames = new int[] { 1, 2, 3, 4, 5, 5, 5, 5, 5, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    protected float period = 0f;
    protected int frame = 0;
    protected const float STEPTIME = 0.25f;
    protected const float BREATHTIME = 0.125f;

    protected TaskStep(Pawn pawn)
    {
        _pawn = pawn;
    }

    public abstract void Perform();

    protected virtual void Finish() { }
}