using System.Collections;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Step;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.AI.Actor
{
    /// <summary>
    /// The <see cref="PlayerPawn"/> class is a <see cref="Pawn"/> that is controlled by the player rather than AI.
    /// </summary>
    public class PlayerPawn : Pawn
    {
        private const float DIRECTION_DELAY_TIME = 0.025f;

        private static PlayerPawn s_instance;

        private bool _aiControl;

        [SerializeField][UsedImplicitly] private Material _defaultMaterial;


        private float _directionDelay;

        private IWorldPosition _nearestInteractable;

        [SerializeField][UsedImplicitly] private Material _outlineMaterial;

        private float _speed = 2.5f;

        /// <value>Reference to the <see cref="PlayerPawn"/> singleton.</value>
        public static PlayerPawn Instance => s_instance;

        /// <inheritdoc/>
        public override RoomNode CurrentNode
        {
            get => base.CurrentNode;
            protected set
            {
                base.CurrentNode = value;
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

                Vector3Int vector = Utility.Utility.DirectionToVector(Direction);

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
        private bool AIControl
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
            TaskActions.Clear();
            foreach (TaskAction action in task.GetActions(this))
                TaskActions.Enqueue(action);

            CurrentStep.ForceFinish();
            CurrentStep = new WaitStep(this, null, false);

            CurrentAction = TaskActions.Dequeue();
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
            TaskActions.Clear();
        }

        /// <inheritdoc/>
        protected override IEnumerator Startup()
        {
            yield return new WaitUntil(() => GameManager.GameReady);

            CurrentNode = Map.Map.Instance[Utility.Utility.SceneCoordinatesToMapCoordinates(transform.position, 0)];

            WorldPositionNonDiscrete = WorldPosition;

            InitializeGameObject();

            Graphics.LevelChanged += OnLevelChange;
            Graphics.UpdatedGraphics += BuildSpriteMask;
            Graphics.LevelChangedLate += BuildSpriteMask;

            var race = (Race)Random.Range(0, 4);

            ActorAppearance appearance = new(race);

            JobHandle jobHandle = Graphics.Instance.BuildSprites(appearance, out NativeArray<Color> pixels);

            yield return new WaitUntil(() => jobHandle.IsCompleted);
            jobHandle.Complete();
            AnimationSprites = Graphics.Instance.SliceSprites(pixels.ToArray());
            pixels.Dispose();

            CurrentStep = new WaitStep(this, Direction.SouthEast, false);

            InitializeGameObject();

            Ready = true;
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (s_instance != null)
                Destroy(this);
            else
                s_instance = this;
        }

        /// <summary>
        /// Determined is a specified interactable is closer than <see cref="_nearestInteractable"/>.
        /// </summary>
        /// <param name="worldPosition">The interactable as an <see cref="IWorldPosition"/>.</param>
        /// <param name="nearestDistance">The distance between the <see cref="PlayerPawn"/> and <see cref="_nearestInteractable"/>.</param>
        private void CompareInteractables(IWorldPosition worldPosition, ref float nearestDistance)
        {
            Vector3 relativePosition = worldPosition.WorldPosition - WorldPositionNonDiscrete;
            float distance = relativePosition.magnitude;


            if (distance < 3 && 
                distance < nearestDistance &&
                Vector3.Dot(relativePosition / distance, Utility.Utility.DirectionToVectorNormalized(Direction)) > Utility.Utility.RAD3_2) //Determines if the interactable is within a 60 degree FOV.
            {
                _nearestInteractable = worldPosition;
                nearestDistance = distance;
            }
        }

        /// <summary>
        /// Determines the nearest interactable <see cref="IWorldPosition"/> that the <see cref="PlayerPawn"/> is facing, if any.
        /// </summary>
        private void FindNearbyInteractable()
        {
            IWorldPosition previousInteractable = _nearestInteractable;
            _nearestInteractable = null;
            float nearestDistance = float.PositiveInfinity;
            // ReSharper disable once LocalVariableHidesMember
            foreach(Collider2D collider in OverlappingColliders)
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

        [UsedImplicitly]
        private void FixedUpdate()
        {
            if(!AIControl)
                FindNearbyInteractable();
        }

        /// <summary>
        /// Takes the movement input controls from the player, and sets the <see cref="Pawn.CurrentStep"/>.
        /// </summary>
        private void GetMovement()
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

            _speed = Input.GetKey(KeyCode.LeftShift) ? 4f : 2.5f;

            Direction direction = movement switch
            {
                _ when movement == new Vector3Int(1, 1) => Direction.NorthEast,
                _ when movement == new Vector3Int(2, 0) => Direction.East,
                _ when movement == new Vector3Int(1, -1) => Direction.SouthEast,
                _ when movement == new Vector3Int(0, -2) => Direction.South,
                _ when movement == new Vector3Int(-1, -1) => Direction.SouthWest,
                _ when movement == new Vector3Int(-2, 0) => Direction.West,
                _ when movement == new Vector3Int(-1, 1) => Direction.NorthWest,
                _ when movement == new Vector3Int(0, 2) => Direction.North,
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
                if (_directionDelay > DIRECTION_DELAY_TIME)
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
        private void SetInteractableMaterial(IWorldPosition worldPosition, Material material)
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

        [UsedImplicitly]
        private void Update()
        {
            if (Ready && !GameManager.Instance.Paused && GameManager.Instance.GameMode == GameMode.Play)
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
}
