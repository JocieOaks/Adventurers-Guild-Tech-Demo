using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.Data;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object.Furniture;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Map.Sprite_Object
{
    /// <summary>
    /// The <see cref="SpriteObject"/> class is the base class for all environmental objects in the game and their corresponding <see cref="UnityEngine.SpriteRenderer"/>.
    /// <see cref="SpriteObject"/>s may encapsulate multiple <see cref="UnityEngine.SpriteRenderer"/>
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes.JsonSubtypes), "ObjectType")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(BedSprite), "Bed")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(ChairSprite), "Chair")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(StoolSprite), "Stool")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(TableRoundSprite), "TableRound")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(TableSquareSprite), "TableSquare")]
    [JsonSubtypes.JsonSubtypes.KnownSubType(typeof(BarSprite), "Bar")]
    public abstract class SpriteObject :  ISpriteObject
    {
        [JsonIgnore]
        protected SpriteRenderer[] SpriteRenderers;

        private readonly bool _blocking;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteObject"/> class.
        /// </summary>
        /// <param name="spriteCount">The number of individual sprites that make up the <see cref="SpriteObject"/>.</param>
        /// <param name="sprites">The initial <see cref="UnityEngine.Sprite"/> to set, depending on the direction. Has only one sprite if undirected.
        /// This is generally the forward most sprite for the <see cref="SpriteObject"/>.</param>
        /// <param name="direction">The direction the <see cref="SpriteObject"/> is facing.</param>
        /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="SpriteObject"/>. For <see cref="SpriteObject"/>s that extend over multiple <see cref="RoomNode"/>s, 
        /// this is the coordinate of the forward most <see cref="RoomNode"/> the object occupies.</param>
        /// <param name="name">The name of the <see cref="SpriteObject"/>.</param>
        /// <param name="dimensions">The 3D dimensions of the <see cref="SpriteObject"/> in <see cref="Map"/> coordinates.</param>
        /// <param name="blocking">If true, the <see cref="RoomNode"/>s the <see cref="SpriteObject"/> occupies are blocked and thus cannot be traversed by a <see cref="AdventurerPawn"/>.</param>
        protected SpriteObject(int spriteCount, Sprite[] sprites, Direction direction, Vector3Int position, string name, Vector3Int dimensions, bool blocking)
        {
            WorldPosition = position;
            Dimensions = direction is Direction.North or Direction.South or Direction.Undirected
                ? dimensions
                : new Vector3Int(dimensions.y, dimensions.x, dimensions.z);
            _blocking = blocking;

            SpriteRenderers = new SpriteRenderer[spriteCount];
            SpriteRenderers[0] = Object.Instantiate(Graphics.Instance.SpritePrefab).GetComponent<SpriteRenderer>();
            Transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
            for (int i = 1; i < spriteCount; i++)
            {
                SpriteRenderers[i] = Object.Instantiate(Graphics.Instance.SpritePrefab, Transform).GetComponent<SpriteRenderer>();
                SpriteRenderers[i].transform.localPosition = Vector3Int.zero;
                SpriteRenderers[i].name = name;
            }
            Sprite = direction switch
            {
                Direction.East => sprites[1],
                Direction.South => sprites[2],
                Direction.West => sprites[3],
                _ => sprites[0],
            };
            SpriteRenderer.name = name;
            SpriteRenderer.sortingOrder = Utility.Utility.GetSortOrder(position);

            SpriteRenderer.enabled = GameManager.Instance.IsOnLevel(WorldPosition.z) <= 0;

            Map.Instance.StartCoroutine(WaitForMap());

            Graphics.LevelChanged += WhenLevelChanged;
            GameManager.MapChangingLate += WhenMapChanging;

            if (this is not WallSprite && this is not StairSprite && this is not FloorSprite)
                DataPersistenceManager.Instance.NonMonoDataPersistenceObjects.Add(this);
        }

        /// <value>The 3D dimensions of any <see cref="SpriteObject"/> of this class in terms of <see cref="Map"/> coordinates.</value>
        [UsedImplicitly]
        public static Vector3Int ObjectDimensions => throw
            //ObjectDimensions should be hidden by any child class. This is only here in case a child class doesn't set it's dimensions so that it can throw an exception.
            new AccessViolationException("Should not be trying to access abstract class dimensions.");

        /// <inheritdoc/>
        [JsonIgnore]
        public virtual Vector3Int Dimensions { get; }

        /// <inheritdoc/>
        [JsonIgnore]
        public abstract IEnumerable<bool[,]> GetMaskPixels { get; }

        /// <inheritdoc/>
        public virtual Vector3Int NearestCornerPosition => WorldPosition;

        /// <inheritdoc/>
        [JsonIgnore]
        public INode Node { get; private set; }

        [JsonIgnore]
        public virtual Vector3 OffsetVector => Vector3.zero;

        /// <inheritdoc/>
        [JsonIgnore]
        public Room Room => Node.Room;

        /// <inheritdoc/>
        [JsonIgnore]
        public SpriteRenderer SpriteRenderer => SpriteRenderers[0];

        /// <inheritdoc/>
        [JsonProperty]
        public Vector3Int WorldPosition { get; }

        /// <value>The <see cref="PolygonCollider2D"/> corresponding to the <see cref="SpriteObject"/></value>
        [JsonIgnore]
        protected PolygonCollider2D Collider { get; private set; }

        /// <value>Gives the <see cref="UnityEngine.GameObject"/> for the forward most sprite of the <see cref="SpriteObject"/>.</value>
        [JsonIgnore]
        protected GameObject GameObject => SpriteRenderers[0].gameObject;

        /// <value>Used by <see cref="JsonConvert.DeserializeObject(string)"/> in order to determine what child of <see cref="SpriteObject"/> a Json object should be deserialized as.</value>
        [UsedImplicitly]
        protected virtual string ObjectType { get; }

        /// <value>Assign the primary sprite of the <see cref="SpriteObject"/>. Also, configures <see cref="Collider"/> when the sprite is changed.</value>
        [JsonIgnore]
        protected Sprite Sprite
        {
            get => SpriteRenderer.sprite;
            set
            {
                if (Collider != null)
                    Object.Destroy(Collider);

                SpriteRenderer.sprite = value;
                if (value != null)
                    AddCollider();
            }
        }
        /// <value>Gives the <see cref="UnityEngine.Transform"/> for the forward most sprite of the <see cref="SpriteObject"/>.
        /// All other sprites of the <see cref="SpriteObject"/> will be a child of <see cref="Transform"/>.</value>
        [JsonIgnore]
        protected Transform Transform => SpriteRenderers[0].transform;

        public virtual MapAlignment Alignment => MapAlignment.Center;

        /// <inheritdoc/>
        public virtual void Destroy()
        {
            Graphics.LevelChanged -= WhenLevelChanged;
            Graphics.ResettingSprite -= WhenResettingSprite;
            GameManager.MapChangingLate -= WhenMapChanging;

            if (_blocking)
            {
                for (int i = 0; i < Dimensions.x; i++)
                {
                    for (int j = 0; j < Dimensions.y; j++)
                    {
                        Map.Instance[WorldPosition + i * Vector3Int.right + j * Vector3Int.up].Occupant = null;
                    }
                }
            }

            if (SpriteRenderer != null)
                Object.Destroy(GameObject);
            DataPersistenceManager.Instance.NonMonoDataPersistenceObjects.Remove(this);
        }

        /// <summary>
        /// Determines if a <see cref="AdventurerPawn"/> positioned at the given <see cref="RoomNode"/> would be considered to have successfully navigated to the <see cref="SpriteObject"/>.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/> the calling <see cref="AdventurerPawn"/> occupies.</param>
        /// <returns>Returns true if the given node is a <see cref="IInteractable.InteractionPoints"/>. Always returns false if <see cref="SpriteObject"/> does not implement the <see cref="IInteractable"/> interface.</returns>
        public bool HasNavigatedTo(RoomNode node)
        {
            if (this is IInteractable interactable)
            {
                foreach (RoomNode roomNode in interactable.InteractionPoints)
                {
                    if (node == roomNode)
                        return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual void Highlight(Color color)
        {
            foreach (SpriteRenderer renderer in SpriteRenderers)
                renderer.color = color;

            Graphics.ResettingSprite += WhenResettingSprite;
        }

        /// <inheritdoc/>
        public void LoadData(GameData gameData)
        {

        }

        /// <inheritdoc/>
        public void SaveData(GameData gameData)
        {
            gameData.SpriteObjects.Add(this);
        }

        /// <summary>
        /// Sets the <see cref="Material"/> for the <see cref="SpriteObject"/>'s <see cref="UnityEngine.SpriteRenderer"/>. Used to outline the <see cref="SpriteObject"/>.
        /// </summary>
        /// <param name="material">The <see cref="Material"/> to use.</param>
        public void SetMaterial(Material material)
        {
            foreach (SpriteRenderer renderer in SpriteRenderers)
                renderer.material = material;
        }

        /// <inheritdoc/>
        public virtual float SpeedMultiplier(Vector3Int nodePosition)
        {
            Vector3Int vector = nodePosition - WorldPosition;

            if (
                vector.x >= 0 && vector.x < Dimensions.x &&
                vector.y >= 0 && vector.y < Dimensions.y &&
                vector.z >= 0 && vector.z < Dimensions.z
            )
                return 0;
            else return 1;
        }

        /// <summary>
        /// Constructs the array of bool for <see cref="GetMaskPixels"/> based on a list of <see cref="UnityEngine.Sprite"/>s.
        /// For each child of <see cref="SpriteObject"/> this should only be called once for each possible orientation, and then the bool array will be saved as a static object.
        /// </summary>
        /// <param name="spriteArray">An array of <see cref="UnityEngine.Sprite"/> from which <see cref="GetMaskPixels"/> should be built.</param>
        /// <param name="pixelArray">A reference to an array of bool that will be returned by <see cref="GetMaskPixels"/>.</param>
        protected static void BuildPixelArray(Sprite[] spriteArray, ref bool[,] pixelArray)
        {
            for (int i = 0; i < spriteArray.Length; i++)
            {
                BuildPixelArray(spriteArray[i], ref pixelArray, i == 0);
            }
        }

        /// <summary>
        /// Constructs the array of bool for <see cref="GetMaskPixels"/> from a <see cref="UnityEngine.Sprite"/>.
        /// For each child of <see cref="SpriteObject"/> this should only be called once for each possible orientation, and then the bool array will be saved as a static object.
        /// </summary>
        /// <param name="sprite">The <see cref="UnityEngine.Sprite"/> from which <see cref="GetMaskPixels"/> should be built.</param>
        /// <param name="pixelArray">A reference to an array of bool that will be returned by <see cref="GetMaskPixels"/>.</param>
        /// <param name="initialize">True if pixelArray needs to be initialized.</param>
        protected static void BuildPixelArray(Sprite sprite, ref bool[,] pixelArray, bool initialize = true)
        {
            Rect rect = sprite.rect;
            int xMin = (int)rect.xMin;
            int xMax = (int)rect.xMax;
            int yMin = (int)rect.yMin;
            int yMax = (int)rect.yMax;
            int width = sprite.texture.width;

            if (initialize)
            {
                pixelArray = new bool[(int)rect.height, (int)rect.width];
            }

            Color32[] array = sprite.texture.GetPixels32();
            for (int j = yMin; j < yMax; j++)
            {
                for (int k = xMin; k < xMax; k++)
                {
                    if (array[j * width + k].a > 0)
                    {
                        pixelArray[j - yMin, k - xMin] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="PolygonCollider2D"/> to <see cref="GameObject"/> and sets its path.
        /// </summary>
        protected void AddCollider()
        {
            Collider = GameObject.AddComponent<PolygonCollider2D>();

            SpriteCollider mouseOver = GameObject.AddComponent<SpriteCollider>();
            mouseOver.Set(this);

            if (Dimensions is { x: > 0, y: > 0, z: > 0 })
            {
                Collider.enabled = false;
                Collider.pathCount = 1;
                Collider.SetPath(0, new[]
                    {
                        new Vector2(0, -1.25f),
                        new Vector2(2 * Dimensions.x, Dimensions.x - 7f/6),
                        new Vector2(2 * Dimensions.x, Dimensions.x + 2 * Dimensions.z - 6.5f/6),
                        new Vector2(2 * (Dimensions.x - Dimensions.y), Dimensions.x + Dimensions.y + 2 * Dimensions.z - 5.5f/6),
                        new Vector2(-2 * Dimensions.y, Dimensions.y + 2 * Dimensions.z - 6.5f/6),
                        new Vector2(-2 * Dimensions.y, Dimensions.y - 7f/6)
                    }
                );

                Collider.enabled = true;
            }
        }

        /// <summary>
        /// Called when the game camera changes which level it is showing.
        /// </summary>
        protected virtual void WhenLevelChanged(object sender, EventArgs eventArgs)
        {
            int level = GameManager.Instance.IsOnLevel(WorldPosition.z);
            if (level > 0)
            {
                foreach (SpriteRenderer renderer in SpriteRenderers)
                    renderer.enabled = false;
            }
            else
                foreach (SpriteRenderer renderer in SpriteRenderers)
                    renderer.enabled = true;
        }

        /// <summary>
        /// Called whenever the <see cref="Map"/> changes.
        /// </summary>
        protected virtual void WhenMapChanging(object sender, EventArgs eventArgs)
        {

        }

        protected void WhenResettingSprite(object sender, EventArgs eventArgs)
        {
            ResetSprite();
        }

        /// <summary>
        /// Resets all the <see cref="UnityEngine.SpriteRenderer"/>s for the <see cref="SpriteObject"/>
        /// </summary>
        protected virtual void ResetSprite()
        {
            foreach (SpriteRenderer renderer in SpriteRenderers)
            {
                renderer.color = Color.white;
            }

            Graphics.ResettingSprite -= WhenResettingSprite;
        }
        /// <summary>
        /// Coroutine called by the <see cref="SpriteObject"/> constructor for processes that have to wait until after the <see cref="Map"/> has completed setup at game start.
        /// </summary>
        /// <returns>Returns <see cref="WaitUntil"/> objects for the <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/> to wait for when <see cref="Map"/> has completed its setup.</returns>
        private IEnumerator WaitForMap()
        {
            GameManager.Instance.ObjectsReady++;
            yield return new WaitUntil(() => Map.Ready);

            Node = Map.Instance[WorldPosition];
            if (_blocking)
            {
                for (int i = 0; i < Dimensions.x; i++)
                {
                    for (int j = 0; j < Dimensions.y; j++)
                    {
                        Map.Instance[WorldPosition + i * Vector3Int.right + j * Vector3Int.up].Occupant = this;
                    }
                }
            }

            GameManager.Instance.ObjectsReady--;
        }



        /// <summary>
        /// The <see cref="SpriteCollider"/> class is used as a component to attach to <see cref="GameObject"/> so that the <see cref="Sprite_Object.SpriteObject"/> can be accessed from the <see cref="GameObject"/>.
        /// </summary>
        public class SpriteCollider : MonoBehaviour
        {
            /// <value>The <see cref="Sprite_Object.SpriteObject"/> corresponding to the <see cref="UnityEngine.GameObject"/> this is a component of.</value>
            public SpriteObject SpriteObject { get; private set; }

            /// <summary>
            /// Set's <see cref="SpriteObject"/>.
            /// </summary>
            /// <param name="spriteObject">The <see cref="Sprite_Object.SpriteObject"/> corresponding to the <see cref="UnityEngine.GameObject"/> this is a component of.</param>
            public void Set(SpriteObject spriteObject)
            {
                SpriteObject = spriteObject;
            }
        }
    }
}
