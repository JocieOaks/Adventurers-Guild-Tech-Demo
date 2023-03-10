using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The <see cref="GUIInputs"/> class is a singleton that exposes certain functions to be called whenever a UI button is clicked, then calls the corresponding functions.
/// </summary>
public class GUIInputs : MonoBehaviour
{
    const float DOUBLECLICKTIME = 0.5f;
    static GUI _gui;
    static GUIInputs _instance;

    float _doubleClick = 0;

    bool _placingArea;

    bool _placingLine;

    /// <summary>
    /// Enum designating the current mouse inputs from the player.
    /// </summary>
    enum KeyMode
    {
        None,
        LeftClickDown,
        LeftClickUp,
        LeftClickHeld,
        DoubleLeftClick,
        MouseOverUI,
    }
    /// <value>A quick reference to the <see cref="GUI"/> singleton.</value>
    static GUI GUI
    {
        get
        {
            if (_gui == null)
                _gui = GUI.Instance;
            return _gui;
        }
    }

    public static bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Called when the adventurer select panel is closed.
    /// </summary>
    public void AdventurerSelectCloseOnClick()
    {
        GUI.CloseAdventurerSelect();
    }

    /// <summary>
    /// Called when the bar object is selected from the objects build menu.
    /// </summary>
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
    public void DemolishOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Demolish;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when door is selected from the build menu.
    /// </summary>
    public void DoorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Door;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when the down button is clicked.
    /// </summary>
    public void DownOnClick()
    {
        GameManager.Instance.ChangeLevel(false);
    }

    /// <summary>
    /// Called when floor is selected from the build menu.
    /// </summary>
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
    public void HiresOnClick()
    {
        GUI.OpenCloseHires();
    }

    /// <summary>
    /// Called when object is selected from the build menu.
    /// </summary>
    public void ObjectOnClick()
    {
        GUI.OpenCloseObjects();
    }

    /// <summary>
    /// Called when quests is selected from the play menu.
    /// </summary>
    public void QuestsOnClick()
    {
        GUI.OpenCloseQuests();
    }

    /// <summary>
    /// Called when the save button is clicked.
    /// </summary>
    public void SaveOnClick()
    {
        DataPersistenceManager.Instance.SaveGame();
    }

    /// <summary>
    /// Called when a quest is selected in the quest menu.
    /// </summary>
    /// <param name="id">The id of the chosen quest.</param>
    public void SelectQuestOnClick(int id)
    {
        GUI.SelectQuest(id);
    }

    /// <summary>
    /// Called when stair is selected from the build menu.
    /// </summary>
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
    public void StartQuestOnClick()
    {
        GUI.StartQuest();
    }

    /// <summary>
    /// Called when the stool object is selected from the objects build menu.
    /// </summary>
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
    public void UpOnClick()
    {
        GameManager.Instance.ChangeLevel(true);
    }

    /// <summary>
    /// Called when wall is selected from the build menu.
    /// </summary>
    public void WallOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreateSpriteObject = WallSprite.CreateWall;
        BuildFunctions.CheckSpriteObject = WallSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = WallSprite.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    /// <inheritdoc/>
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Area"/>.
    /// </summary>
    /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
    void BuildingArea(KeyMode mode)
    {
        Vector3Int position = Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);
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
                    BuildFunctions.HighlightSpriteObject(Graphics.Instance._highlight, position);
                else
                    Graphics.Instance.HideHighlight();
                break;
        }
    }

    /// <summary>
    /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Door"/>.
    /// </summary>
    /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
    /// <param name="spriteObject">The <see cref="WallSprite"/> that the mouse is currently over, if any.</param>
    void BuildingDoor(KeyMode mode, WallSprite spriteObject)
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
                        Map.Instance.PlaceDoor(position, alignment);

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
    void BuildingLine(KeyMode mode)
    {
        Vector3Int position = Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);
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
                    BuildFunctions.HighlightSpriteObject(Graphics.Instance._highlight, position);
                else
                    Graphics.Instance.HideHighlight();
                break;
        }
    }

    /// <summary>
    /// Determines how to use the <see cref="KeyMode"/> inputs when in <see cref="BuildMode.Point"/>.
    /// </summary>
    /// <param name="mode">The <see cref="KeyMode"/> of the current input.</param>
    void BuildingPoint(KeyMode mode)
    {
        Vector3Int position = Utility.SceneCoordinatesToMapCoordinates(GameManager.Camera.ScreenToWorldPoint(Input.mousePosition), GameManager.Instance.LevelMin);

        switch (mode)
        {
            case KeyMode.None:
                BuildFunctions.HighlightSpriteObject(Graphics.Instance._highlight, position);
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
    void Demolish(KeyMode mode, SpriteObject spriteObject)
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
    KeyMode GetKeyMode()
    {
        if (IsMouseOverUI())
        {
            return KeyMode.MouseOverUI;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (_doubleClick <= 0)
            {
                _doubleClick = DOUBLECLICKTIME;
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

    SpriteObject GetMouseOver()
    {
        RaycastHit2D[] hits;
        Vector2 ray = GameManager.Camera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(ray);
        hits = Physics2D.RaycastAll(ray, Vector2.zero);
        SpriteObject nearest = null;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.TryGetComponent(out SpriteObject.SpriteCollider collider))
            {
                SpriteObject spriteObject = collider.SpriteObject;
                if (spriteObject.SpriteRenderer.enabled && (nearest?.SpriteRenderer.sortingOrder ?? -1000) < spriteObject.SpriteRenderer.sortingOrder)
                    nearest = spriteObject;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Gets the object that the mouse is currently hovering over.
    /// </summary>
    /// <typeparam name="T">The type of the object to find.</typeparam>
    /// <returns>Returns the forward most object of type <c>T</c> that the mouse is hovering over.</returns>
    T GetMouseOver<T>() where T : SpriteObject
    {
        RaycastHit2D[] hits;
        Vector2 ray = GameManager.Camera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(ray);
        hits = Physics2D.RaycastAll(ray, Vector2.zero);
        SpriteObject nearest = null;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.TryGetComponent(out SpriteObject.SpriteCollider collider))
            {
                SpriteObject spriteObject = collider.SpriteObject;
                if (spriteObject is T && spriteObject.SpriteRenderer.enabled && (nearest?.SpriteRenderer.sortingOrder ?? -1000) < spriteObject.SpriteRenderer.sortingOrder)
                    nearest = spriteObject;
            }
        }

        return nearest as T;
    }

    /// <inheritdoc/>
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
            GameManager.Instance.CycleGameMode();
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            GameManager.Instance.ZoomCamera(Input.mouseScrollDelta.y);
        }

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

        if (GameManager.Instance.GameMode == GameMode.Build)
        {
            if (IsMouseOverUI() && !_placingLine && !_placingArea)
                Graphics.Instance.HideHighlight();
            else
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
                        Demolish(mode, GetMouseOver());
                        break;
                }
            }
        }
    }
}


