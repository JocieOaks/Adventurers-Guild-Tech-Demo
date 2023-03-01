using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

/// <summary>
/// The current body position a <see cref="Pawn"/> is in.
/// </summary>
public enum Stance
{
    /// <summary>The <see cref="Pawn"/> is  standing or walking.</summary>
    Stand,
    /// <summary>The <see cref="Pawn"/> is sitting.</summary>
    Sit,
    /// <summary>The <see cref="Pawn"/> is laying down.</summary>
    Lay
}

/// <summary>
/// The <see cref="Pawn"/> class is the counterpart to the <see cref="global::Actor"/> class that controls the active functional aspect of an NPC, including the in game sprite representation and overseeing the AI behaviors.
/// </summary>
public class Pawn : MonoBehaviour, IWorldPosition
{
    static readonly Vector3Int s_alignmentVector = new(1, 1);
    static readonly Vector2 s_maskPivot = new(36, 18);
    
    Actor _actor;

    [SerializeField] Sprite[] _animationSprites;
    [SerializeField] PolygonCollider2D _collider;

    RoomNode _currentNode;
    Task _currentTask;

    [SerializeField] SpriteRenderer _emoji;

    SpriteMask _mask;
    
    Color32[] _maskArray;
    
    Texture2D _maskTexture;
    
    readonly List<Collider2D> _overlappingColliders = new();
    
    Planner _planner;
    
    bool _ready = false;
    
    int _recovery = 0;
    
    SortingGroup _sortingGroup;

    [SerializeField] SpriteRenderer _speechBubble;

    [SerializeField] SpriteRenderer _spriteRenderer;

    readonly Queue<TaskAction> _taskActions = new();

    Vector3 _worldPosition;

    /// <value>The <see cref="Pawn"/>'s corresponding <see cref="global::Actor"/>.</value>
    public Actor Actor { 
        get => _actor; 
        set
        {
            _actor ??= value;
        } 
    }

    /// <value> The current <see cref="TaskAction"/> the <see cref="Pawn"/> is performing.</value> 
    public TaskAction CurrentAction { get; private set; }

    /// <value> The z coordinate of <see cref="Pawn"/>'s position.</value>
    public int CurrentLevel => CurrentNode.SurfacePosition.z;

    /// <value> The nearest <see cref="RoomNode"/> to the <see cref="Pawn's"/> current position. When set, CurrentNode evaluates whether the <see cref="Pawn"/> should be visible on screen.</value>
    public RoomNode CurrentNode
    {
        get => _currentNode;
        private set
        {
            _currentNode = value;
            _spriteRenderer.enabled = GameManager.Instance.IsOnLevel(CurrentLevel) <= 0;
        }
    }

    /// <value> The <see cref="Room"/> the <see cref="Pawn"/> is currently located in.</value>
    public Room CurrentRoom => CurrentNode.Room;

    /// <value> The current <see cref="TaskStep"/> the <see cref="Pawn"/> is performing.</value>
    public TaskStep CurrentStep { get; set; }

    /// <value> The current <c>Direction</c> the <see cref="Pawn"/> is currently facing. May be undirected if the <see cref="TaskStep"/> the <see cref="Pawn"/> is performing does not face a cardinal direction, i.e. lying down.</value>
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

    /// <value>Returns true if the <see cref="Pawn"/> is currently engaged in a <see cref="Conversation"/> with another <see cref="Pawn"/>.</value>
    public bool IsInConversation => Social.Conversation != null;

    /// <value> Returns true if the <see cref="Pawn"/> is currently speaking.</value>
    public bool IsSpeaking => _speechBubble.gameObject.activeSelf;

    /// <value>The <see cref="SocialAI"/> that runs the <see cref="Pawn"/>'s social behaviours.</value>
    public SocialAI Social { get; private set; }

    /// <value>The speed that the <see cref="Pawn"/> moves. Measured as the number of <see cref="RoomNode"/> tiles the <see cref="Pawn"/> can cross per second.</value>
    public float Speed { get; } = 2.5f;

    /// <value>Assigns the animation <see cref="Sprite"/>s for the <see cref="Pawn"/> if they are not already assigned.</value>
    public Sprite[] Sprites
    {
        set
        {
            if (_animationSprites == null || _animationSprites.Count() == 0)
                _animationSprites = value;
        }
    }

    /// <value> The current <c>Stance</c> the <see cref="Pawn"/> is in.</value>
    public Stance Stance { get; set; } = Stance.Stand;

    ///<value> The nearest <see cref="Vector3Int"/> position of the <see cref="Pawn"/> in the <see cref="Map"/>'s coordinate system, and the position of the nearest <see cref="RoomNode"/>.</value>
    public Vector3Int WorldPosition => CurrentNode.SurfacePosition;

    ///<value> Gives the Vector3 representation of the <see cref="Pawn"/>'s actual position in the <see cref="Map"/>'s coordinate system.</value>
    public Vector3 WorldPositionNonDiscrete
    {
        get => _worldPosition;
        set
        {
            _worldPosition = value;
            Vector3Int nearest = new(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));
            if (nearest != WorldPosition)
            {
                CurrentNode = Map.Instance[nearest];
                _sortingGroup.sortingOrder = Graphics.GetSortOrder(WorldPosition);
                BuildSpriteMask();
            }
            transform.position = Map.MapCoordinatesToSceneCoordinates(value);
        }
    }

    /// <inheritdoc/>
    public Room Room => Node.Room;

    /// <inheritdoc/>
    public INode Node => CurrentNode;

    /// <summary>
    /// Sets the <see cref="Pawn"/> to begin going on a <see cref="Quest"/>
    /// </summary>
    public void BeginQuest()
    {
        Social.EndConversation();
        OverrideTask(new QuestTask());
        Social.Silenced = true;
    }

    ///<inheritdoc/>
    public bool HasNavigatedTo(RoomNode node)
    {
        return Vector3Int.Distance(node.WorldPosition, WorldPosition) < 5;
    }

    /// <summary>
    /// Force a new <see cref="Task"/> for the <see cref="Pawn"/> to take, without waiting for the previous <see cref="Task"/> and <see cref="TaskAction"/>s to complete.
    /// </summary>
    /// <param name="task">The new <see cref="Task"/> for the <see cref="Pawn"/> to perform.</param>
    public void OverrideTask(Task task)
    {
        _currentTask = task;
        _taskActions.Clear();
        foreach (TaskAction action in _currentTask.GetActions(Actor))
            _taskActions.Enqueue(action);

        CurrentStep = new WaitStep(this, null, false);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        _planner.OverrideTask(task);
    }
    /// <summary>
    /// Displays a speech bubble over the <see cref="Pawn"/>'s <see cref="Sprite"/>, to visually indicate that they are speaking with another <see cref="Pawn"/>.
    /// </summary>
    /// <param name="type">The type of speech that the <see cref="Pawn"/> is engaging in, which indicates the type of symbol that should be used.</param>
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
    /// Sets the current <see cref="Sprite"/>.
    /// </summary>
    /// <param name="spriteIndex">The index of the <see cref="Sprite"/> to set the <see cref="SpriteRenderer"/> to. Must be less than 48.</param>
    public void SetSprite(int spriteIndex)
    {
        _spriteRenderer.sprite = _animationSprites[spriteIndex];
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartCoroutine(Startup());
    }

    /// <summary>
    /// Constructs a <see cref="SpriteMask"/> to layer over the <see cref="Pawn"/>'s <see cref="SpriteRenderer"/> for whenever there is a <see cref="SpriteObject"/> in front of it.
    /// </summary>
    void BuildSpriteMask()
    {
        //The function is currently most efficient when there are only a few sprites overlaying the Pawn sprite.
        //When there is a large number of overlapping sprites covering the Pawn sprite, it may be more efficient to check pixels semi-individually, so that only the first sprite found is evaluated.

        if (_spriteRenderer.enabled)
        {
            int colliderCount = _collider.OverlapCollider(new ContactFilter2D().NoFilter(), _overlappingColliders);

            //Initialize the _maskArray
            for (int i = 0; i < _maskArray.Length; i++)
            {
                _maskArray[i] = Color.clear;
            }

            for (int i = 0; i < colliderCount; i++)
            {
                if (_overlappingColliders[i].TryGetComponent(out SpriteObject.SpriteCollider collider))
                {
                    SpriteObject spriteObject = collider.SpriteObject;
                    Vector3Int relPosition;

                    if (spriteObject.SpriteRenderer.enabled)
                    {

                        //Determines if the SpriteObject is in front of, or behind the Pawn.
                        //Walls have special rules because they exist on the line between RoomNodes.
                        //Stairs have special rules because the standard evaluation assumes that everything is on a flat plane.
                        if (spriteObject is WallSprite wall)
                        {
                            relPosition = WorldPosition - spriteObject.WorldPosition - (wall.Alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left);
                        }
                        else
                        {
                            relPosition = WorldPosition - spriteObject.WorldPosition;
                            if (spriteObject is StairSprite stair)
                            {
                                if (stair.Direction == Direction.North)
                                {
                                    relPosition -= Vector3Int.forward * relPosition.y;
                                }
                                else if (stair.Direction == Direction.East)
                                {
                                    relPosition -= Vector3Int.forward * relPosition.x;
                                }
                            }
                        }

                        //_alignmentVector is a static vector that points from the camera inward. (1,1,0)
                        //If the dot product of the alignment vector and the relative position of the Pawn to the SpriteObject is positive, it means that the Pawn is further into screen than the SpriteObject
                        if (relPosition.z - spriteObject.Dimensions.z < 0 && ( relPosition.z <= -5 || Vector3.Dot(relPosition, s_alignmentVector) > 0))
                        {

                            //Builds a flattened 2D array for the SpriteMask by checking the pixels of the SpriteObjects in front of the Pawn.
                            int transformOffset = 0;
                            foreach (bool[,] pixelArray in spriteObject.GetMaskPixels)
                            {
                                Vector2 pivot = spriteObject.SpriteRenderer.sprite.pivot;
                                Vector3 relScenePosition = Map.MapCoordinatesToSceneCoordinates(WorldPosition) - (spriteObject.SpriteRenderer.transform.position + transformOffset * spriteObject.OffsetVector);

                                int xOffset = (int)(pivot.x + relScenePosition.x * 6 - s_maskPivot.x);
                                int yOffset = (int)(pivot.y + relScenePosition.y * 6 - s_maskPivot.y);

                                for (int j = Mathf.Max(0, yOffset); j < Mathf.Min(pixelArray.GetLength(0), yOffset + _maskTexture.height); j++)
                                {
                                    for (int k = Mathf.Max(0, xOffset); k < Mathf.Min(pixelArray.GetLength(1), xOffset + _maskTexture.width); k++)
                                    {
                                        if (pixelArray[j, k])
                                        {
                                            _maskArray[(j - yOffset) * _maskTexture.width + k - xOffset] = Color.black;
                                        }
                                    }
                                }
                                transformOffset++;
                            }
                        }
                    }
                }
            }

            _maskTexture.SetPixels32(_maskArray);
            _maskTexture.Apply();

            _mask.sprite = Sprite.Create(_maskTexture, new Rect(0, 0, _maskTexture.width, _maskTexture.height), new Vector2(s_maskPivot.x / _maskTexture.width, s_maskPivot.y / _maskTexture.height), 6);
            _mask.transform.position = Map.MapCoordinatesToSceneCoordinates(WorldPosition);
        }
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
    /// Initializes _sortingGroup and the mask elements for the Pawn.
    /// </summary>
    void InitializeGameObject()
    {
        _sortingGroup = Instantiate(Graphics.Instance.SortingObject);
        _sortingGroup.transform.position = Vector3.zero;
        _sortingGroup.sortingLayerName = "Pawn";
        transform.SetParent(_sortingGroup.transform);

        _maskTexture = new Texture2D(72, 96, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _mask = new GameObject(Actor.Name + " Mask").AddComponent<SpriteMask>();
        _mask.transform.SetParent(_sortingGroup.transform);

        _maskArray = new Color32[_maskTexture.width * _maskTexture.height];
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

    /// <summary>
    /// Enables the or disables the sprite from view if the <see cref="Pawn"/> is on a visible level.
    /// </summary>
    void OnLevelChange()
    {
        _spriteRenderer.enabled = GameManager.Instance.IsOnLevel(CurrentLevel) <= 0;
    }

    /// <summary>
    /// Initializes the <see cref="Pawn"/> once the game is ready.
    /// </summary>
    /// <returns>Returns <see cref="WaitUntil"/> objects for the <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>, until the <see cref="GameManager"/> is ready.</returns>
    IEnumerator Startup()
    {
        yield return new WaitUntil(() => GameManager.GameReady);

        CurrentNode = Map.Instance[Map.SceneCoordinatesToMapCoordinates(transform.position, 0)];

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

