using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PlayerPawn : Pawn
{
    const float DIRECTIONDELAYTIME = 0.025f;

    static PlayerPawn _instance;

    bool _aiControl;

    [SerializeField] Material _defaultMaterial;

    float _directionDelay = 0;

    IWorldPosition _nearestInteractable;

    [SerializeField] Material _outlineMaterial;

    float _speed = 2.5f;

    /// <value>Reference to the <see cref="PlayerPawn"/> singleton.</value>
    static public PlayerPawn Instance => _instance;

    /// <inheritdoc/>
    public override RoomNode CurrentNode
    {
        get => _currentNode;
        protected set
        {
            _currentNode = value;
            int level = GameManager.Instance.IsOnLevel(CurrentLevel);
            if (level < 0)
                GameManager.Instance.ChangeLevel(false);
            else if (level > 0)
                GameManager.Instance.ChangeLevel(true);
        }
    }

    /// <inheritdoc/>
    public override string Name => "Player";

    /// <inheritdoc/>
    public override float Speed => _speed * CurrentNode.SpeedMultiplier;

    /// <inheritdoc/>
    public override Vector3 WorldPositionNonDiscrete
    {
        get => base.WorldPositionNonDiscrete;
        set
        {

            Vector3Int vector = Utility.DirectionToVector(Direction);

            if (vector.x != 0 && !CurrentNode.GetNode(vector.x > 0 ? Direction.East : Direction.West).Traversable)
            {
                if ((value.x - WorldPosition.x) * vector.x > 0.25f)
                {
                    value = new Vector3(WorldPosition.x + 0.25f * vector.x, value.y, value.z);
                }
            }
            if (vector.y != 0 && !CurrentNode.GetNode(vector.y > 0 ? Direction.North : Direction.South).Traversable)
            {
                if ((value.y - WorldPosition.y) * vector.y > 0.25f)
                {
                    value = new Vector3(value.x, WorldPosition.y + 0.25f * vector.y, value.z);
                }
            }

            base.WorldPositionNonDiscrete = value;
        }
    }

    /// <value>Determines whether the <see cref="PlayerPawn"/> is performing an automatic <see cref="IPlayerTask"/> or the player is in control.</value>
    bool AIControl
    {
        get => _aiControl;
        set
        {
            if(_aiControl && !value && _nearestInteractable is IPlayerInteractable playerInteractable)
            {
                playerInteractable.EndPlayerInteraction();
            }

            _aiControl = value;
            
        }
    }

    /// <summary>
    /// Sets a <see cref="IPlayerTask"/> for the <see cref="PlayerPawn"/> to perform as an AI.
    /// </summary>
    /// <param name="task">The <see cref="IPlayerTask"/> for the <see cref="PlayerPawn"/> to perform.</param>
    public void SetTask(IPlayerTask task)
    {
        _taskActions.Clear();
        foreach (TaskAction action in task.GetActions(this))
            _taskActions.Enqueue(action);

        CurrentStep.ForceFinish();
        CurrentStep = new WaitStep(this, null, false);

        CurrentAction = _taskActions.Dequeue();
        CurrentAction.Initialize();

        AIControl = true;
    }

    /// <inheritdoc/>
    protected override void OnTaskFail()
    {
        OnTaskFinish();
    }

    /// <inheritdoc/>
    protected override void OnTaskFinish()
    {
        CurrentAction = null;
        _taskActions.Clear();
    }

    /// <inheritdoc/>
    protected override IEnumerator Startup()
    {
        yield return new WaitUntil(() => GameManager.GameReady);

        CurrentNode = Map.Instance[Utility.SceneCoordinatesToMapCoordinates(transform.position, 0)];

        WorldPositionNonDiscrete = WorldPosition;

        InitializeGameObject();

        Graphics.LevelChanged += OnLevelChange;
        Graphics.UpdatedGraphics += BuildSpriteMask;
        Graphics.LevelChangedLate += BuildSpriteMask;

        Race Race = (Race)Random.Range(0, 4);

        ActorAppearance appearance = new(Race);

        JobHandle jobHandle = Graphics.Instance.BuildSprites(appearance, out NativeArray<Color> pixels);

        yield return new WaitUntil(() => jobHandle.IsCompleted);
        jobHandle.Complete();
        _animationSprites = Graphics.Instance.SliceSprites(pixels.ToArray());
        pixels.Dispose();

        CurrentStep = new WaitStep(this, Direction.SouthEast, false);

        InitializeGameObject();

        _ready = true;
    }

    /// <inheritdoc/>
    private void Awake()
    {
        if (_instance != null)
            Destroy(this);
        else
            _instance = this;
    }

    /// <summary>
    /// Determined is a specified interactable is closer than <see cref="_nearestInteractable"/>.
    /// </summary>
    /// <param name="worldPosition">The interactable as an <see cref="IWorldPosition"/>.</param>
    /// <param name="nearestDistance">The distance between the <see cref="PlayerPawn"/> and <see cref="_nearestInteractable"/>.</param>
    void CompareInteractables(IWorldPosition worldPosition, ref float nearestDistance)
    {
        Vector3 relativePosition = worldPosition.WorldPosition - WorldPositionNonDiscrete;
        float distance = relativePosition.magnitude;


        if (distance < 3 && 
            distance < nearestDistance &&
            Vector3.Dot(relativePosition / distance, Utility.DirectionToVectorNormalized(Direction)) > Utility.RAD3_2) //Determines if the interactable is within a 60 degree FOV.
        {
            _nearestInteractable = worldPosition;
            nearestDistance = distance;
        }
    }

    /// <summary>
    /// Determines the nearest interactable <see cref="IWorldPosition"/> that the <see cref="PlayerPawn"/> is facing, if any.
    /// </summary>
    void FindNearbyInteractable()
    {
        IWorldPosition previousInteractable = _nearestInteractable;
        _nearestInteractable = null;
        float nearestDistance = float.PositiveInfinity;
        foreach(Collider2D collider in _overlappingColliders)
        {
            if(collider.TryGetComponent(out SpriteObject.SpriteCollider spriteCollider) && spriteCollider.SpriteObject is IPlayerInteractable interactable)
            {
                CompareInteractables(interactable, ref nearestDistance);
            }
            else if(collider.TryGetComponent(out AdventurerPawn pawn))
            {
                CompareInteractables(pawn, ref nearestDistance);
            }
        }

        if(previousInteractable != _nearestInteractable)
        {
            SetInteractableMaterial(previousInteractable, _defaultMaterial);
            if(_nearestInteractable != null)
            {
                SetInteractableMaterial(_nearestInteractable, _outlineMaterial);
            }
        }
    }

    /// <inheritdoc/>
    private void FixedUpdate()
    {
        if(!AIControl)
            FindNearbyInteractable();
    }

    /// <summary>
    /// Takes the movement input controls from the player, and sets the <see cref="Pawn.CurrentStep"/>.
    /// </summary>
    void GetMovement()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movement += new Vector3Int(1, 1);
            AIControl = false;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movement += new Vector3Int(-1, -1);
            AIControl = false;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movement += new Vector3Int(-1, 1);
            AIControl = false;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movement += new Vector3Int(1, -1);
            AIControl = false;
        }

        if (AIControl)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speed = 4f;
        }
        else
        {
            _speed = 2.5f;
        }

        Direction direction = movement switch
        {
            Vector3 v when v == new Vector3Int(1, 1) => Direction.NorthEast,
            Vector3 v when v == new Vector3Int(2, 0) => Direction.East,
            Vector3 v when v == new Vector3Int(1, -1) => Direction.SouthEast,
            Vector3 v when v == new Vector3Int(0, -2) => Direction.South,
            Vector3 v when v == new Vector3Int(-1, -1) => Direction.SouthWest,
            Vector3 v when v == new Vector3Int(-2, 0) => Direction.West,
            Vector3 v when v == new Vector3Int(-1, 1) => Direction.NorthWest,
            Vector3 v when v == new Vector3Int(0, 2) => Direction.North,
            _ => Direction.Undirected,
        };


        if (direction == Direction.Undirected)
        {


            if (CurrentStep is not WaitStep)
            {
                CurrentStep = new WaitStep(this, CurrentStep, false);
            }
        }
        else if (CurrentStep is not WalkStep || direction != Direction)
        {
            _directionDelay += Time.deltaTime;
            if (_directionDelay > DIRECTIONDELAYTIME)
            {
                _directionDelay = 0;

                CurrentStep = new WalkStep(direction, this, CurrentStep);
            }
        }
    }

    /// <summary>
    /// Sets the <see cref="Material"/> for an <see cref="IWorldPosition"/>'s <see cref="SpriteRenderer"/>. Used to outline the <see cref="IWorldPosition"/>.
    /// </summary>
    /// <param name="worldPosition">The <see cref="IWorldPosition"/> whose <see cref="Material"/> is being set.</param>
    /// <param name="material">The <see cref="Material"/> to use.</param>
    void SetInteractableMaterial(IWorldPosition worldPosition, Material material)
    {
        if(worldPosition is SpriteObject spriteObject)
        {
            spriteObject.SetMaterial(material);
        }
        else if(worldPosition is Pawn pawn)
        {
            pawn.SetMaterial(material);
        }
    }

    /// <inheritdoc/>
    void Update()
    {
        if (_ready && !GameManager.Instance.Paused && GameManager.Instance.GameMode == GameMode.Play)
        {
            GetMovement();

            if(AIControl)
            {
                ManageTask();
                CurrentAction?.Perform();
            }
            else
            {
                if(Input.GetKeyDown(KeyCode.E) && _nearestInteractable is IPlayerInteractable playerInteractable)
                {
                    playerInteractable.StartPlayerInteraction();
                }
            }

            CurrentStep.Perform();
        }
    }

}
