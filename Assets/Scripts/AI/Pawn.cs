using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stance
{
    Stand,
    Sit,
    Lay
}

public class Pawn : MonoBehaviour
{
    public float Speed { get; } = 2.5f;
    public SpriteRenderer _spriteRenderer;

    SpriteRenderer _speechBubble;
    SpriteRenderer _emoji;

    SpriteRenderer _frontSprite;
    SpriteMask _frontMask;
    SpriteMask _backMask;

    RoomNode _currentNode;
    public RoomNode CurrentNode
    {
        get => _currentNode;
        private set
        {
            _currentNode = value;
            _spriteRenderer.enabled = GameManager.Instance.IsOnLevel(CurrentLevel) <= 0;
        }
    }

    public int CurrentLevel => CurrentNode.WorldPosition.z;

    public Vector3Int CurrentPosition => CurrentNode.WorldPosition;

    public Room CurrentRoom => CurrentNode.Room;

    public Direction Direction
    {
        get
        {
            if (CurrentStep is IDirected directed)
            {
                return directed.Direction;
            }
            else
                return Direction.Undirected;
        }
    }


    TaskStep _currentStep;
    public TaskStep CurrentStep 
    { 
        get => _currentStep; 
        
        set 
        {
            _currentStep = value;
        } 
    }

    public Stance Stance { get; set; } = Stance.Stand;

    public Actor Actor { get; set; }

    public Social Social { get; private set; }

    Queue<TaskAction> _taskActions = new Queue<TaskAction>();

    Task _currentTask;
    public TaskAction CurrentAction { get; private set; }

    Planner _planner;

    public Sprite[] Sprites
    {
        set
        {
            if (_animationSprites == null)
                _animationSprites = value;
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartCoroutine(Startup());
    }

    bool _ready = false;

    IEnumerator Startup()
    {
        yield return new WaitUntil(() => Map.Ready);
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _speechBubble = Instantiate(Graphics.Instance.SpeechBubble, transform);

        _emoji = _speechBubble.transform.GetComponentsInChildren<SpriteRenderer>()[1];

        _speechBubble.gameObject.SetActive(false);

        _backMask = Instantiate(Graphics.Instance.PawnMask);
        _backMask.transform.position = transform.position;
        transform.SetParent(_backMask.transform);
        _frontMask = Instantiate(Graphics.Instance.PawnMask, _backMask.transform);
        _frontMask.transform.localPosition = Vector3.zero;
        _frontSprite = new GameObject().AddComponent<SpriteRenderer>();
        _frontSprite.transform.SetParent(transform);
        _frontSprite.transform.localPosition = Vector3.zero;
        _frontSprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        CurrentNode = Map.GetNodeFromSceneCoordinates(transform.position, 0);

        WorldPosition = CurrentPosition;

        Graphics.LevelChanging += SetLevel;

        Social = new Social(this);

        _currentTask = new WaitTask(0.5f);
        _planner = new Planner(Actor, _currentTask);
        Map.Instance.StartCoroutine(_planner.AStar());
        foreach (TaskAction action in _currentTask.GetActions(Actor))
            _taskActions.Enqueue(action);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        _currentStep = new Wait(this, null);

        Map.Instance[0].EnterRoom(this);

        _ready = true;

        //GameManager.recalculatePath += Recalculate;
    }

    void SetLevel()
    {
        _spriteRenderer.enabled = GameManager.Instance.IsOnLevel(CurrentLevel) <= 0;
    }

    public void OverrideTask(Task task)
    {
        _currentTask = task;
        _taskActions.Clear();
        foreach (TaskAction action in _currentTask.GetActions(Actor))
            _taskActions.Enqueue(action);

        _currentStep = new Wait(this, null);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        _planner.OverrideTask(Actor, task);
    }

    int _recovery = 0;

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused && _ready)
        {
            bool recover = false;
            foreach(TaskAction action in _taskActions)
            {
                if(action.Complete() == -1)
                {
                    recover = true;
                    CurrentAction = action;
                    break;
                }
            }

            if(recover || CurrentAction != null && CurrentAction.Complete() == -1)
            {
                _recovery++;
                _taskActions.Clear();

                if (_currentTask is IRecovery recovery && _recovery < 4)
                {
                    foreach (TaskAction action in recovery.Recover(Actor, CurrentAction))
                        _taskActions.Enqueue(action);
                }
                else
                {
                    _currentTask = new WaitTask(0.5f);
                    foreach (TaskAction action in _currentTask.GetActions(Actor))
                        _taskActions.Enqueue(action);

                    _planner.OverrideTask(Actor, _currentTask);
                }
                CurrentAction = null;
            }
            else
            {
                _recovery = 0;
            }

            if(CurrentAction == null || CurrentAction.Complete() != 0)
            {
                if(_taskActions.Count == 0)
                {
                    _currentTask = _planner.GetTask(Actor);

                    foreach (TaskAction action in _currentTask.GetActions(Actor))
                        _taskActions.Enqueue(action);
                }

                if (_taskActions.Count > 0)
                {
                    CurrentAction = _taskActions.Dequeue();
                    CurrentAction.Initialize();
                }
            }

            Actor.ChangeNeeds(Needs.Hunger, -Time.deltaTime / 10);

            switch(Stance)
            {
                case Stance.Stand:
                    Actor.ChangeNeeds(Needs.Sleep, -Time.deltaTime / 30);
                    break;
                case Stance.Sit:
                    Actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 60);
                    break;
                case Stance.Lay:
                    Actor.ChangeNeeds(Needs.Sleep, Time.deltaTime / 30);
                    break;
            }
            if(Social.Conversation == null)
                Actor.ChangeNeeds(Needs.Social, -Time.deltaTime / 5);
            else
                Actor.ChangeNeeds(Needs.Social, Time.deltaTime / 2);

            CurrentAction?.Perform();
            CurrentStep?.Perform();
        }
    }

    public void SetSprite(int spriteIndex)
    {
        _spriteRenderer.sprite = _animationSprites[spriteIndex];
        _frontSprite.sprite = _spriteRenderer.sprite;
    }

    Vector3 _worldPosition;

    public Vector3 WorldPosition
    {
        get => _worldPosition;
        set
        {
            _worldPosition = value;
            Vector3Int nearest = new Vector3Int(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));
            if ( nearest != CurrentPosition)
            {
                CurrentNode = Map.Instance[nearest];
                int sortingOrder =  Graphics.GetSortOrder(CurrentPosition.x - 1, CurrentPosition.y);
                _spriteRenderer.sortingOrder = sortingOrder;
                _frontSprite.sortingOrder = sortingOrder + 2;
                _backMask.frontSortingOrder = sortingOrder;
                _backMask.backSortingOrder = sortingOrder - 1;
                _frontMask.frontSortingOrder = sortingOrder + 2;
                _frontMask.backSortingOrder = sortingOrder + 1;

                _backMask.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, CurrentPosition);

            }
            transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, value);
        }
    }

    public void Quest()
    {
        Social.EndConversation();
        OverrideTask(new QuestTask());
        Social.Silenced = true;
    }

    public bool IsSpeaking => _speechBubble.gameObject.activeSelf;

    public IEnumerator Say(SpeechType type)
    {
        _speechBubble.gameObject.SetActive(true);
        Color tempColor = _speechBubble.color;
        tempColor.a = 1f;
        _speechBubble.color = tempColor;
        switch(type)
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

    [SerializeField] Sprite[] _animationSprites;
}

