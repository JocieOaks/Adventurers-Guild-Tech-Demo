using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.AI.Actor
{
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
    /// The <see cref="Pawn"/> class is the base class for all characters in the game world, including the player and NPCs.
    /// </summary>
    public abstract class Pawn : MonoBehaviour, IWorldPosition
    {
        /// <value>A list containing all the <see cref="Collider2D"/>s that currently intersect with the <see cref="Pawn"/>'s hit box.</value>
        protected readonly List<Collider2D> OverlappingColliders = new();

        private static readonly Vector2 s_maskPivot = new(36, 18);

        [SerializeField][UsedImplicitly] private PolygonCollider2D _collider;

        private SpriteMask _mask;

        private Color32[] _maskArray;

        private Texture2D _maskTexture;

        private SortingGroup _sortingGroup;

        [SerializeField][UsedImplicitly] private SpriteRenderer _spriteRenderer;

        private Vector3 _worldPosition;

        /// <inheritdoc />
        public MapAlignment Alignment => MapAlignment.Center;

        /// <value> The current <see cref="TaskAction"/> the <see cref="AdventurerPawn"/> is performing.</value> 
        public TaskAction CurrentAction { get; protected set; }

        /// <value> The z coordinate of <see cref="AdventurerPawn"/>'s position.</value>
        public int CurrentLevel => CurrentNode.SurfacePosition.z;

        /// <value> The nearest <see cref="RoomNode"/> to the <see cref="AdventurerPawn"/>'s current position. When set, CurrentNode evaluates whether the <see cref="AdventurerPawn"/> should be visible on screen.</value>
        public virtual RoomNode CurrentNode { get; protected set; }

        /// <value> The current <see cref="TaskStep"/> the <see cref="Pawn"/> is performing.</value>
        public TaskStep CurrentStep { get; set; }

        /// <inheritdoc/>
        public Vector3Int Dimensions => new(3, 3, 5);

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

        /// <summary>
        /// The name of the <see cref="Pawn"/>.
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public Vector3Int NearestCornerPosition => WorldPosition - new Vector3Int(1, 1, 0);

        /// <inheritdoc/>
        public RoomNode Node => CurrentNode;

        /// <value>The <see cref="IOccupied"/> the <see cref="Pawn"/> is currently occupying. Null when the <see cref="Pawn"/> is not occupying anything.</value>
        public IOccupied Occupying { get; set; }

        /// <inheritdoc/>
        public Room Room => Node.Room;

        /// <value>The speed that the <see cref="AdventurerPawn"/> moves. Measured as the number of <see cref="RoomNode"/> tiles the <see cref="Pawn"/> can cross per second.</value>
        public virtual float Speed => 2.5f * CurrentNode?.SpeedMultiplier ?? 2.5f;

        /// <value>Assigns the animation <see cref="Sprite"/>s for the <see cref="AdventurerPawn"/> if they are not already assigned.</value>
        public Sprite[] Sprites
        {
            set
            {
                if (AnimationSprites == null || !AnimationSprites.Any())
                    AnimationSprites = value;
            }
        }

        /// <value> The current <c>Stance</c> the <see cref="Pawn"/> is in.</value>
        public Stance Stance { get; set; } = Stance.Stand;

        ///<value> The nearest <see cref="Vector3Int"/> position of the <see cref="Pawn"/> in the <see cref="Map"/>'s coordinate system, and the position of the nearest <see cref="RoomNode"/>.</value>
        public Vector3Int WorldPosition => CurrentNode.SurfacePosition;

        ///<value> Gives the Vector3 representation of the <see cref="Pawn"/>'s actual position in the <see cref="Map"/>'s coordinate system.</value>
        public virtual Vector3 WorldPositionNonDiscrete
        {
            get => _worldPosition;
            set
            {
                if (CurrentNode is StairNode stair)
                {
                    _worldPosition = stair.StairPosition(value);
                }
                else
                    _worldPosition = value;

                Vector3Int nearest = new(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y), Mathf.RoundToInt(value.z));
                if (nearest.x != WorldPosition.x || nearest.y != WorldPosition.y)
                {
                    Room prevRoom = Room;

                    CurrentNode = CurrentNode.GetRoomNode(Utility.Utility.VectorToDirection(_worldPosition - WorldPosition, true));
                    if (prevRoom != Room)
                    {
                        prevRoom.ExitRoom(this);
                        Room.EnterRoom(this);
                    }

                    _sortingGroup.sortingOrder = Utility.Utility.GetSortOrder(WorldPosition);
                    BuildSpriteMask();
                }
                transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(value);
            }
        }

        /// <value>Array containing all the <see cref="Sprite"/>s used by the <see cref="Pawn"/>.</value>
        [field: SerializeField] protected Sprite[] AnimationSprites { get; set; }

        /// <value>Set to true once the <see cref="Pawn"/> has finished initial setup.</value>
        protected bool Ready { get; set; } = false;
        /// <value>The number of times the AI has attempted to recover the current <see cref="Task"/>.
        /// After too many failed recoveries the current <see cref="Task"/> will be abandoned.</value>
        protected int Recovery { get; set; }

        /// <value>A queue containing all the <see cref="TaskAction"/>s to be performed to complete the current <see cref="Task"/>.</value>
        protected Queue<TaskAction> TaskActions { get; } = new();
        /// <summary>
        /// Forces the <see cref="Pawn"/> to a specified <see cref="RoomNode"/>, even if it is not adjacent to their previous position.
        /// </summary>
        /// <param name="roomNode">The <see cref="RoomNode"/> the <see cref="Pawn"/> is being moved to.</param>
        public void ForcePosition(RoomNode roomNode)
        {
            CurrentNode = roomNode;
            WorldPositionNonDiscrete = WorldPosition;
            BuildSpriteMask();
        }

        /// <summary>
        /// Forces the <see cref="Pawn"/> to a specified <see cref="Map"/> position, even if it is not adjacent to their previous position.
        /// </summary>
        /// <param name="position">The coordinates of the <see cref="RoomNode"/> the <see cref="Pawn"/> is being moved to.</param>
        public void ForcePosition(Vector3Int position)
        {
            ForcePosition(Map.Map.Instance[position]);
        }

        ///<inheritdoc/>
        public bool HasNavigatedTo(RoomNode node)
        {
            return Vector3Int.Distance(node.WorldPosition, WorldPosition) < 5;
        }

        /// <summary>
        /// Sets the <see cref="Material"/> for the <see cref="Pawn"/>'s <see cref="UnityEngine.SpriteRenderer"/>. Used to outline the <see cref="Pawn"/>.
        /// </summary>
        /// <param name="material">The <see cref="Material"/> to use.</param>
        public void SetMaterial(Material material)
        {
            _spriteRenderer.material = material;
        }

        /// <summary>
        /// Sets the current <see cref="Sprite"/>.
        /// </summary>
        /// <param name="spriteIndex">The index of the <see cref="Sprite"/> to set the <see cref="UnityEngine.SpriteRenderer"/> to. Must be less than 48.</param>
        public void SetSprite(int spriteIndex)
        {
            _spriteRenderer.sprite = AnimationSprites[spriteIndex];
        }

        /// <summary>
        /// Constructs a <see cref="SpriteMask"/> to layer over the <see cref="AdventurerPawn"/>'s <see cref="UnityEngine.SpriteRenderer"/> for whenever there is a <see cref="SpriteObject"/> in front of it.
        /// </summary>
        protected void BuildSpriteMask()
        {
            //The function is currently most efficient when there are only a few sprites overlaying the Pawn sprite.
            //When there is a large number of overlapping sprites covering the Pawn sprite, it may be more efficient to check pixels semi-individually, so that only the first sprite found is evaluated.

            if (_spriteRenderer.enabled)
            {
                int colliderCount = _collider.OverlapCollider(new ContactFilter2D().NoFilter(), OverlappingColliders);

                //Initialize the _maskArray
                for (int i = 0; i < _maskArray.Length; i++)
                {
                    _maskArray[i] = Color.clear;
                }

                for (int i = 0; i < colliderCount; i++)
                {
                    // ReSharper disable once LocalVariableHidesMember
                    if (OverlappingColliders[i].TryGetComponent(out SpriteObject.SpriteCollider collider))
                    {
                        SpriteObject spriteObject = collider.SpriteObject;

                        if (spriteObject.SpriteRenderer.enabled && Utility.Utility.IsInFrontOf(spriteObject, this))
                        { 
                            //Builds a flattened 2D array for the SpriteMask by checking the pixels of the SpriteObjects in front of the Pawn.
                            int transformOffset = 0;
                            foreach (bool[,] pixelArray in spriteObject.GetMaskPixels)
                            {
                                Vector2 pivot = spriteObject.SpriteRenderer.sprite.pivot;
                                Vector3 relScenePosition = Utility.Utility.MapCoordinatesToSceneCoordinates(WorldPosition) - (spriteObject.SpriteRenderer.transform.position + transformOffset * spriteObject.OffsetVector);

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

                _maskTexture.SetPixels32(_maskArray);
                _maskTexture.Apply();

                _mask.sprite = Sprite.Create(_maskTexture, new Rect(0, 0, _maskTexture.width, _maskTexture.height), new Vector2(s_maskPivot.x / _maskTexture.width, s_maskPivot.y / _maskTexture.height), 6);
                _mask.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(WorldPosition);
            }
        }

        /// <summary>
        /// Initializes _sortingGroup and the mask elements for the Pawn.
        /// </summary>
        protected void InitializeGameObject()
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

            _mask = new GameObject(Name + " Mask").AddComponent<SpriteMask>();
            _mask.transform.SetParent(_sortingGroup.transform);

            _maskArray = new Color32[_maskTexture.width * _maskTexture.height];
        }

        /// <summary>
        /// Evaluates the state of the current <see cref="Task"/> and <see cref="TaskAction"/>s and updates them if necessary.
        /// </summary>
        protected void ManageTask()
        {
            bool recover = false;
            foreach (TaskAction action in TaskActions)
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
                Recovery++;
                OnTaskFail();
            }
            else
            {
                Recovery = 0;
            }

            if (CurrentAction == null || CurrentAction.Complete() != 0)
            {
                if (TaskActions.Count == 0)
                {
                    OnTaskFinish();
                }

                if (TaskActions.Count > 0)
                {
                    CurrentAction = TaskActions.Dequeue();
                    CurrentAction.Initialize();
                }
                else
                {
                    CurrentAction = new WaitAction(2, this);
                    CurrentAction.Initialize();
                }
            }
        }

        /// <summary>
        /// Enables or disables the sprite from view if the <see cref="Pawn"/> is on a visible level.
        /// </summary>
        protected void OnLevelChange()
        {
            _spriteRenderer.enabled = GameManager.Instance.IsOnLevel(CurrentLevel) <= 0;
        }

        /// <summary>
        /// Called when the current <see cref="Task"/> has failed and must be recovered from or ended.
        /// </summary>
        protected abstract void OnTaskFail();

        /// <summary>
        /// Called when the current <see cref="Task"/> has completed.
        /// </summary>
        protected abstract void OnTaskFinish();

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected virtual void Start()
        {
            StartCoroutine(Startup());
        }

        /// <summary>
        /// Initializes the <see cref="Pawn"/> once the game is ready.
        /// </summary>
        /// <returns>Returns <see cref="WaitUntil"/> objects for the <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>, until the <see cref="GameManager"/> is ready.</returns>
        protected abstract IEnumerator Startup();
    }
}