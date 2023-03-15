/// <summary>
/// The <see cref="TaskStep"/> class is the base class for small single steps for a <see cref="Pawn"/> to take.
/// <see cref="TaskStep"/> is also responsible for controlling <see cref="Pawn"/> animations.
/// </summary>
public abstract class TaskStep
{

    protected const float BREATHTIME = 0.125f;
    protected const float STEPTIME = 0.5f;
    protected static int[] _idleFrames = new int[] { 1, 2, 3, 4, 5, 5, 5, 5, 5, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
    protected Pawn _pawn;
    protected int _frame = 0;
    protected float _period = 0f;
    bool _finished = false;

    /// <summary>
    /// Initializes a new instance of <see cref="TaskStep"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="TaskStep"/>.</param>
    protected TaskStep(Pawn pawn)
    {
        _pawn = pawn;
    }

    /// <value>Evaluates whether the <see cref="TaskStep"/> has finished.</value>
    protected abstract bool _isComplete { get; }

    /// <summary>
    /// Evaluates whether the <see cref="TaskStep"/> has finished. Calls <see cref="Finish"/> the first time it returns true.
    /// </summary>
    /// <returns>True if the <see cref="TaskStep"/> has finished.</returns>
    public bool IsComplete()
    {
        if(_isComplete)
        {
            if (!_finished)
            {
                Finish();
                _finished= true;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Called every Unity update.
    /// </summary>
    public abstract void Perform();

    /// <summary>
    /// Forces the <see cref="TaskStep"/> to run <see cref="Finish"/>, even if the <see cref="IsComplete"/> is not true.
    /// </summary>
    public void ForceFinish()
    {
        if(!_finished)
        {
            Finish();
            _finished = true;
        }
    }

    /// <summary>
    /// Called once the <see cref="TaskStep"/> has completed.
    /// </summary>
    protected virtual void Finish() { }
}