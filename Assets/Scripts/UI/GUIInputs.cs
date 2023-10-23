using System;
using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.Data;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.Map.Sprite_Object.Furniture;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The <see cref="GUIInputs"/> class is a singleton that exposes certain functions to be called whenever a UI button is clicked, then calls the corresponding functions.
    /// </summary>
    public class GUIInputs : MonoBehaviour
    {
        private const float DOUBLE_CLICK_TIME = 0.5f;
        private static GUI s_gui;
        private static GUIInputs s_instance;

        private float _doubleClick;

        private bool _placingArea;

        private bool _placingLine;

        /// <summary>
        /// Enum designating the current mouse inputs from the player.
        /// </summary>
        private enum KeyMode
        {
            None,
            LeftClickDown,
            LeftClickUp,
            LeftClickHeld,
            DoubleLeftClick,
            MouseOverUI,
        }
        /// <value>A quick reference to the <see cref="GUI"/> singleton.</value>
        private static GUI GUI
        {
            get
            {
                if (s_gui == null)
                    s_gui = GUI.Instance;
                return s_gui;
            }
        }

        public static bool IsMouseOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Called when the adventurer select panel is closed.
        /// </summary>
        [UsedImplicitly]
        public void AdventurerSelectCloseOnClick()
        {
            GUI.CloseAdventurerSelect();
        }

        /// <summary>
        /// Called when the bar object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void BarOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Line;
            BuildFunctions.CreateSpriteObject = BarSprite.CreateBar;
            BuildFunctions.CheckSpriteObject = BarSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = BarSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when the bed object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void BedOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Point;
            BuildFunctions.CreateSpriteObject = BedSprite.CreateBed;
            BuildFunctions.CheckSpriteObject = BedSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = BedSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when the chair object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void ChairOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Point;
            BuildFunctions.Direction = Direction.West;
            BuildFunctions.CreateSpriteObject = ChairSprite.CreateChair;
            BuildFunctions.CheckSpriteObject = ChairSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = ChairSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when demolish is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void DemolishOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Demolish;
            GUI.Instance.CloseObjects();
        }

        /// <summary>
        /// Called when door is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void DoorOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Door;
            GUI.Instance.CloseObjects();
        }

        /// <summary>
        /// Called when the down button is clicked.
        /// </summary>
        [UsedImplicitly]
        public void DownOnClick()
        {
            GameManager.Instance.ChangeLevel(false);
        }

        /// <summary>
        /// Called when floor is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void FloorOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Area;
            BuildFunctions.CreateSpriteObject = FloorSprite.CreateFloor;
            BuildFunctions.HighlightSpriteObject = FloorSprite.PlaceHighlight;
            BuildFunctions.CheckSpriteObject = FloorSprite.CheckObject;
            GUI.Instance.CloseObjects();
        }

        /// <summary>
        /// Called when the adventurers button is clicked.
        /// </summary>
        [UsedImplicitly]
        public void HiresOnClick()
        {
            GUI.OpenCloseHires();
        }

        /// <summary>
        /// Called when object is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void ObjectOnClick()
        {
            GUI.OpenCloseObjects();
        }

        /// <summary>
        /// Called when quests is selected from the play menu.
        /// </summary>
        [UsedImplicitly]
        public void QuestsOnClick()
        {
            GUI.OpenCloseQuests();
        }

        /// <summary>
        /// Called when the save button is clicked.
        /// </summary>
        [UsedImplicitly]
        public void SaveOnClick()
        {
            DataPersistenceManager.Instance.SaveGame();
        }

        /// <summary>
        /// Called when a quest is selected in the quest menu.
        /// </summary>
        /// <param name="id">The id of the chosen quest.</param>
        [UsedImplicitly]
        public void SelectQuestOnClick(int id)
        {
            GUI.SelectQuest(id);
        }

        /// <summary>
        /// Called when stair is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void StairOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Area;
            BuildFunctions.CreateSpriteObject = StairSprite.CreateStair;
            BuildFunctions.CheckSpriteObject = StairSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = StairSprite.PlaceHighlight;
            GUI.Instance.CloseObjects();
        }

        /// <summary>
        /// Called when start is clicked in the quest menu.
        /// </summary>
        [UsedImplicitly]
        public void StartQuestOnClick()
        {
            GUI.StartQuest();
        }

        /// <summary>
        /// Called when the stool object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void StoolOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Point;
            BuildFunctions.CreateSpriteObject = StoolSprite.CreateStool;
            BuildFunctions.CheckSpriteObject = StoolSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = StoolSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when the round table object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void TableRoundOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Point;
            BuildFunctions.CreateSpriteObject = TableRoundSprite.CreateTableRound;
            BuildFunctions.CheckSpriteObject = TableRoundSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = TableRoundSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when the square table object is selected from the objects build menu.
        /// </summary>
        [UsedImplicitly]
        public void TableSquareOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Point;
            BuildFunctions.CreateSpriteObject = TableSquareSprite.CreateTableSquare;
            BuildFunctions.CheckSpriteObject = TableSquareSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = TableSquareSprite.PlaceHighlight;
        }

        /// <summary>
        /// Called when the up button is clicked.
        /// </summary>
        [UsedImplicitly]
        public void UpOnClick()
        {
            GameManager.Instance.ChangeLevel(true);
        }

        /// <summary>
        /// Called when wall is selected from the build menu.
        /// </summary>
        [UsedImplicitly]
        public void WallOnClick()
        {
            BuildFunctions.BuildMode = BuildMode.Line;
            BuildFunctions.CreateSpriteObject = WallSprite.CreateWall;
            BuildFunctions.CheckSpriteObject = WallSprite.CheckObject;
            BuildFunctions.HighlightSpriteObject = WallSprite.PlaceHighlight;
            GUI.Instance.CloseObjects();
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (s_instance == null)
                s_instance = this;
            else
                Destroy(this);
        }

        /// <summary>
        /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Area"/>.
        /// </summary>
        /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
        private void BuildingArea(KeyMode mode)
        {
            Vector3Int position = Utility.Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);
            switch (mode)
            {
                case KeyMode.LeftClickDown:
                    if (BuildFunctions.CheckSpriteObject(position))
                    {
                        _placingArea = true;
                        Graphics.Instance.HideHighlight();
                        BuildFunctions.StartPlacingArea(position);
                    }
                    break;
                case KeyMode.LeftClickHeld:
                    if (BuildFunctions.CheckSpriteObject(position) && _placingArea)
                    {
                        BuildFunctions.PlaceArea(position);
                    }
                    break;
                case KeyMode.LeftClickUp:
                case KeyMode.MouseOverUI:
                    if (_placingArea)
                    {
                        _placingArea = false;

                        BuildFunctions.Confirm();
                    }
                    break;

                case KeyMode.None:
                    if (BuildFunctions.CheckSpriteObject(position))
                        BuildFunctions.HighlightSpriteObject(Graphics.Instance.Highlight, position);
                    else
                        Graphics.Instance.HideHighlight();
                    break;
                case KeyMode.DoubleLeftClick:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        /// <summary>
        /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Door"/>.
        /// </summary>
        /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
        /// <param name="spriteObject">The <see cref="WallSprite"/> that the mouse is currently over, if any.</param>
        private void BuildingDoor(KeyMode mode, WallSprite spriteObject)
        {
            switch (mode)
            {
                case KeyMode.None:
                    if (spriteObject != null)
                    {
                        WallSprite.PlaceDoorHighlight(spriteObject);
                    }
                    else
                    {
                        Graphics.Instance.HideHighlight();
                    }
                    break;
                case KeyMode.LeftClickDown:
                    if (spriteObject != null)
                    {
                        (Vector3Int position, MapAlignment alignment) = spriteObject.GetPosition;
                        if (WallSprite.CheckDoor(position, alignment))
                        {
                            Map.Map.Instance.PlaceDoor(position, alignment);

                            WallSprite.CreateDoor(position, alignment);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Line"/>.
        /// </summary>
        /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
        private void BuildingLine(KeyMode mode)
        {
            Vector3Int position = Utility.Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);
            switch (mode)
            {
                case KeyMode.LeftClickDown:
                    if (BuildFunctions.CheckSpriteObject(position))
                    {
                        _placingLine = true;
                        BuildFunctions.StartPlacingLine(position);
                        Graphics.Instance.HideHighlight();
                    }
                    break;
                case KeyMode.LeftClickHeld:
                    if (BuildFunctions.CheckSpriteObject(position) && _placingLine)
                    {
                        BuildFunctions.PlaceLine(position);
                    }
                    break;
                case KeyMode.LeftClickUp:
                case KeyMode.MouseOverUI:
                    if (_placingLine)
                    {
                        _placingLine = false;

                        BuildFunctions.Confirm();
                    }
                    break;

                case KeyMode.None:
                    if (BuildFunctions.CheckSpriteObject(position))
                        BuildFunctions.HighlightSpriteObject(Graphics.Instance.Highlight, position);
                    else
                        Graphics.Instance.HideHighlight();
                    break;
            }
        }

        /// <summary>
        /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Point"/>.
        /// </summary>
        /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
        private void BuildingPoint(KeyMode mode)
        {
            Vector3Int position = Utility.Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);

            switch (mode)
            {
                case KeyMode.None:
                    BuildFunctions.HighlightSpriteObject(Graphics.Instance.Highlight, position);
                    break;
                case KeyMode.LeftClickDown:
                    if (BuildFunctions.CheckSpriteObject(position))
                        BuildFunctions.CreateSpriteObject(position);
                    break;
            }
        }

        /// <summary>
        /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Demolish"/>.
        /// </summary>
        /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
        /// <param name="spriteObject">The <see cref="SpriteObject"/> that the mouse is currently over, if any.</param>
        private void Demolish(KeyMode mode, SpriteObject spriteObject)
        {
            switch (mode)
            {

                case KeyMode.None:
                    if (spriteObject != null)
                    {
                        Graphics.Instance.HighlightDemolish(spriteObject);
                    }
                    else
                    {
                        Graphics.Instance.HideHighlight();
                    }
                    break;

                case KeyMode.LeftClickDown:

                    if (spriteObject != null)
                    {
                        BuildFunctions.Demolish(spriteObject);
                    }
                    break;
            }
        }

        /// <summary>
        /// Determines what <see cref="KeyMode"/> corresponds to the current mouse inputs.
        /// </summary>
        /// <returns>Returns the <see cref="KeyMode"/> of the user input.</returns>
        private KeyMode GetKeyMode()
        {
            if (IsMouseOverUI())
            {
                return KeyMode.MouseOverUI;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (_doubleClick <= 0)
                {
                    _doubleClick = DOUBLE_CLICK_TIME;
                    return KeyMode.LeftClickDown;

                }
                else
                {
                    _doubleClick = 0;
                    return KeyMode.DoubleLeftClick;
                }
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                return KeyMode.LeftClickHeld;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                return KeyMode.LeftClickUp;
            }

            return KeyMode.None;

        }

        /// <summary>
        /// Gets the object that the mouse is currently hovering over.
        /// </summary>
        /// <typeparam name="T">The type of the object to find.</typeparam>
        /// <returns>Returns the forward most object of type <c>T</c> that the mouse is hovering over.</returns>
        private static T GetMouseOver<T>() where T : IWorldPosition
        {
            Vector2 ray = GameManager.Camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray, Vector2.zero);
            T nearest = default;
            foreach (RaycastHit2D hit in hits)
            {
                T next = default;
                if (hit.collider.TryGetComponent(out SpriteObject.SpriteCollider collider))
                {
                    SpriteObject spriteObject = collider.SpriteObject;
                    if (spriteObject is T tObject && spriteObject.SpriteRenderer.enabled)
                        next = tObject;
                }
                else if (hit.collider.TryGetComponent(out Pawn pawn))
                {
                    if (pawn is T tObject && GameManager.Instance.IsOnLevel(pawn.CurrentLevel) <= 0)
                        next = tObject;
                }
                if (!EqualityComparer<T>.Default.Equals(next, default) && (EqualityComparer<T>.Default.Equals(nearest, default) || Utility.Utility.IsInFrontOf(next, nearest)))
                    nearest = next;
            }

            return nearest;
        }

        [UsedImplicitly]
        private void Update()
        {
            if (_doubleClick > 0)
            {
                _doubleClick -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Graphics.Instance.CycleWallDisplayMode();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                GameManager.Instance.CycleGameMode(GameMode.Build);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                GameManager.Instance.CycleGameMode(GameMode.Overview);
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                GameManager.Instance.ZoomCamera(Input.mouseScrollDelta.y);
            }

            if (GameManager.Instance.GameMode != GameMode.Play)
            {
                Vector3 translation = Vector3.zero;

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    translation += Vector3.up;
                }
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    translation += Vector3.down;
                }
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    translation += Vector3.left;
                }
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    translation += Vector3.right;
                }

                GameManager.Instance.TranslateCamera(translation);
            }

            if (!_placingLine && !_placingArea)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    switch (BuildFunctions.Direction)
                    {
                        case Direction.North:
                            BuildFunctions.Direction = Direction.East;
                            break;
                        case Direction.East:
                            BuildFunctions.Direction = Direction.South;
                            break;
                        case Direction.South:
                            BuildFunctions.Direction = Direction.West;
                            break;
                        case Direction.West:
                            BuildFunctions.Direction = Direction.North;
                            break;
                    }
                if (Input.GetKeyDown(KeyCode.Q))
                    switch (BuildFunctions.Direction)
                    {
                        case Direction.North:
                            BuildFunctions.Direction = Direction.West;
                            break;
                        case Direction.West:
                            BuildFunctions.Direction = Direction.South;
                            break;
                        case Direction.South:
                            BuildFunctions.Direction = Direction.East;
                            break;
                        case Direction.East:
                            BuildFunctions.Direction = Direction.North;
                            break;
                    }
            }

            switch (GameManager.Instance.GameMode)
            {
                case GameMode.Overview when !IsMouseOverUI():
                {
#if DEBUG
                    var pawn = GetMouseOver<AdventurerPawn>();
                    GUI.Instance.SetDebugPanel(pawn);
#endif
                    break;
                }
                case GameMode.Build when IsMouseOverUI() && !_placingLine && !_placingArea:
                    Graphics.Instance.HideHighlight();
                    break;
                case GameMode.Build:
                {
                    KeyMode mode = GetKeyMode();

                    switch (BuildFunctions.BuildMode)
                    {
                        case BuildMode.Point:
                            BuildingPoint(mode);
                            break;
                        case BuildMode.Line:
                            BuildingLine(mode);
                            break;
                        case BuildMode.Door:
                            BuildingDoor(mode, GetMouseOver<WallSprite>());
                            break;
                        case BuildMode.Area:
                            BuildingArea(mode);
                            break;
                        case BuildMode.Demolish:
                            Demolish(mode, GetMouseOver<SpriteObject>());
                            break;
                    }

                    break;
                }
            }
        }
    }
}


