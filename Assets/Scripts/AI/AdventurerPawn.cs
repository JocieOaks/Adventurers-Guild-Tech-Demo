using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public enum Stance
{
    Stand,
    Sit,
    Lay
}

/// <summary>
/// The <see cref="AdventurerPawn"/> class is the counterpart to the <see cref="global::Actor"/> class that controls the active functional aspect of an NPC, including the in game sprite representation and overseeing the AI behaviors.
/// </summary>
public class AdventurerPawn : Pawn
{
    readonly Queue<TaskAction> _taskActions = new();
    Actor _actor;
    Task _currentTask;

    [SerializeField] SpriteRenderer _emoji;
    Planner _planner;
    int _recovery = 0;
    [SerializeField] SpriteRenderer _speechBubble;

    /// <value>The <see cref="AdventurerPawn"/>'s corresponding <see cref="global::Actor"/>.</value>
    public Actor Actor { 
        get => _actor; 
        set
        {
            _actor ??= value;
        } 
    }

    /// <value> The current <see cref="TaskAction"/> the <see cref="AdventurerPawn"/> is performing.</value> 
    public TaskAction CurrentAction { get; private set; }

    /// <value>Returns true if the <see cref="AdventurerPawn"/> is currently engaged in a <see cref="Conversation"/> with another <see cref="AdventurerPawn"/>.</value>
    public bool IsInConversation => Social.Conversation != null;

    /// <value> Returns true if the <see cref="AdventurerPawn"/> is currently speaking.</value>
    public bool IsSpeaking => _speechBubble.gameObject.activeSelf;

    /// <value>The <see cref="SocialAI"/> that runs the <see cref="AdventurerPawn"/>'s social behaviours.</value>
    public SocialAI Social { get; private set; }

    protected override string Name => Actor.Name;

    /// <summary>
    /// Sets the <see cref="AdventurerPawn"/> to begin going on a <see cref="Quest"/>
    /// </summary>
    public void BeginQuest()
    {
        Social.EndConversation();
        OverrideTask(new QuestTask());
        Social.Silenced = true;
    }

    /// <summary>
    /// Force a new <see cref="Task"/> for the <see cref="AdventurerPawn"/> to take, without waiting for the previous <see cref="Task"/> and <see cref="TaskAction"/>s to complete.
    /// </summary>
    /// <param name="task">The new <see cref="Task"/> for the <see cref="AdventurerPawn"/> to perform.</param>
    public void OverrideTask(Task task)
    {
        _currentTask = task;
        _taskActions.Clear();
        foreach (TaskAction action in _currentTask.GetActions(Actor))
            _taskActions.Enqueue(action);

        CurrentStep.ForceFinish();
        CurrentStep = new WaitStep(this, null, false);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        _planner.OverrideTask(task);
    }
    /// <summary>
    /// Displays a speech bubble over the <see cref="AdventurerPawn"/>'s <see cref="Sprite"/>, to visually indicate that they are speaking with another <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="type">The type of speech that the <see cref="AdventurerPawn"/> is engaging in, which indicates the type of symbol that should be used.</param>
    /// <returns>Returns <see cref="WaitForSeconds"/> objects for the <c>StartCoroutine</c> function.</returns>
    public IEnumerator Say(SpeechType type)
    {
        _speechBubble.gameObject.SetActive(true);
        Color tempColor = _speechBubble.color;
        tempColor.a = 1f;
        _speechBubble.color = tempColor;
        switch (type)
        {
            case SpeechType.Greet:
                _emoji.sprite = Graphics.Instance.Wave;
                break;
            case SpeechType.Comment:
                _emoji.sprite = Graphics.Instance.Commentary[Random.Range(0, 10)];
                break;
        }

        yield return new WaitForSeconds(2f);

        while (tempColor.a > 0f)
        {
            tempColor.a -= Time.deltaTime / 2;
            _speechBubble.color = tempColor;
            yield return new WaitForEndOfFrame();
        }
        _speechBubble.gameObject.SetActive(false);
    }

    /// <summary>
    /// Initializes the Planner and Social for the Pawn, and setsup the starting Task.
    /// </summary>
    void InitializeAI()
    {
        Social = new SocialAI(this);

        _currentTask = new WaitTask(0.5f);

        _planner = new Planner(Actor, _currentTask);
        Map.Instance.StartCoroutine(_planner.AStar());

        foreach (TaskAction action in _currentTask.GetActions(Actor))
            _taskActions.Enqueue(action);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        CurrentStep = new WaitStep(this, null, false);
    }

    /// <summary>
    /// Evaluates the state of the current <see cref="Task"/> and <see cref="TaskAction"/>s and updates them if necessary.
    /// </summary>
    void ManageTask()
    {
        bool recover = false;
        foreach (TaskAction action in _taskActions)
        {
            if (action.Complete() == -1)
            {
                recover = true;
                CurrentAction = action;
                break;
            }
        }

        if (recover || CurrentAction != null && CurrentAction.Complete() == -1)
        {
            _recovery++;
            _taskActions.Clear();
            if(!CurrentNode.Traversable)
            {
                foreach(RoomNode node in CurrentNode.AdjacentNodes)
                {
                    if(node.Traversable)
                    {
                        ForcePosition(node);
                        break;
                    }
                }
                if (!CurrentNode.Traversable)
                    ForcePosition(Vector3Int.one);
            }
            
            if (_currentTask is IRecoverableTask recovery && _recovery < 4)
            {
                foreach (TaskAction action in recovery.Recover(Actor, CurrentAction))
                    _taskActions.Enqueue(action);
            }
            else
            {
                _currentTask = new WaitTask(0.5f);
                foreach (TaskAction action in _currentTask.GetActions(Actor))
                    _taskActions.Enqueue(action);

                _planner.OverrideTask(_currentTask);
            }
            CurrentAction = null;
        }
        else
        {
            _recovery = 0;
        }

        if (CurrentAction == null || CurrentAction.Complete() != 0)
        {
            if (_taskActions.Count == 0)
            {
                _currentTask = _planner.GetTask();

                foreach (TaskAction action in _currentTask.GetActions(Actor))
                    _taskActions.Enqueue(action);
            }

            if (_taskActions.Count > 0)
            {
                CurrentAction = _taskActions.Dequeue();
                CurrentAction.Initialize();
            }
            else
            {
                CurrentAction = new WaitAction(Actor, 2);
                CurrentAction.Initialize();
            }
        }
    }


    /// <inheritdoc/>
    protected override IEnumerator Startup()
    {
        yield return new WaitUntil(() => GameManager.GameReady);

        CurrentNode = Map.GetNodeFromSceneCoordinates(transform.position, 0);

        WorldPositionNonDiscrete = WorldPosition;

        InitializeGameObject();

        InitializeAI();

        Map.Instance[0].EnterRoom(this);

        Graphics.LevelChanged += OnLevelChange;
        Graphics.UpdatedGraphics += BuildSpriteMask;
        Graphics.LevelChangedLate += BuildSpriteMask;

        _ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused && _ready)
        {
            ManageTask();

            Actor.Update();

            CurrentAction?.Perform();
            CurrentStep?.Perform();
        }
    }
}

