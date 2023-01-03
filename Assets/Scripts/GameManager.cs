using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public enum GameMode
{
    Play,
    Build
}

public class GameManager : MonoBehaviour, IDataPersistence
{
    const float DOUBLECLICKTIME = 0.5f;
    const float TICK_TIME = 0.2f;
    const float ZOOMMAX = 40;
    const float ZOOMMIN = 5;
    static GameManager _instance;

    List<QuestData> _allQuests;

    List<(Actor adventurer, int timeAvailable)> _availableHires = new List<(Actor, int)>();

    List<QuestData> _availableQuests = new List<QuestData>();

    Camera _camera;

    float _doubleClick = 0;

    GameMode _gameMode;

    Graphics _graphics;

    int _lastAdventurerTick;

    int _lastQuestTick;

    int _levelMax = 6;

    int _levelMin = 0;

    MapAlignment _lineAlignment;
    Vector3Int _lineStart;

    Map _map;

    bool _placingArea = false;
    bool _placingLine = false;
    bool _placingWalls = false;

    WallMode _playWallMode;

    SpriteObject _prevObject = null;

    List<Quest> _runningQuests = new List<Quest>();

    float _time = 0;

    public static event System.Action Ticked;

    public static event System.Action MapChanging;

    public static event System.Action MapChanged;

    enum KeyMode
    {
        None,
        LeftClickDown,
        LeftClickUp,
        LeftClickHeld,
        DoubleLeftClick,
        MouseOverUI,
    }

    static public GameManager Instance => _instance;
    public List<Actor> Adventurers { get; } = new List<Actor>();
    public List<QuestData> AvailableQuests => _availableQuests;
    public GameMode GameMode => _gameMode;
    public List<string> Names { get; private set; }
    public bool NextTutorialStep { get; set; }
    public List<Actor> NPCs { get; } = new List<Actor>();
    public bool Paused { get; set; } = true;

    public List<QuestData> Quests
    {
        set
        {
            _allQuests = value;
        }
    }

    public List<Quest> RunningQuests => _runningQuests;

    public int Tick { get; private set; }

    public static bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public IEnumerable Actors()
    {
        foreach(Actor adventurer in Adventurers)
        {
            yield return adventurer;
        }

        foreach(Actor npc in NPCs)
        {
            yield return npc;
        }
    }
    public void ChangeLevel(bool up)
    {
        if (up)
        {
            Layer upLayer = Map.Instance.NextLayer(_levelMin, 1);
            if (upLayer != null)
            {
                _camera.transform.position += Vector3.up * (_levelMax - _levelMin);
                _levelMin = upLayer.Origin.z;
                _levelMax = upLayer.Origin.z + upLayer.Height;

            }
        }
        else
        {
            Layer downLayer = Map.Instance.NextLayer(_levelMin, -1);
            if (downLayer != null)
            {
                _camera.transform.position += Vector3.down * (_levelMax - _levelMin);
                _levelMin = downLayer.Origin.z;
                _levelMax = downLayer.Origin.z + downLayer.Height;
            }
        }
        Graphics.Instance.SetLevel();
    }

    public void Hire(Actor adventurer)
    {
        Adventurers.Add(adventurer);
        _availableHires.RemoveAll(x => x.adventurer == adventurer);
        GUI.Instance.BuildHires(_availableHires);
        adventurer.InitializePawn(Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, Vector3Int.one));
    }

    public int IsOnLevel(int z)
    {
        if (z >= _levelMax)
            return 1;
        else if (z < _levelMin)
            return -1;
        else
            return 0;
    }

    public void LoadData(GameData gameData)
    {
        Names = gameData.Names;
    }

    public void Reject(Actor adventurer)
    {
        _availableHires.RemoveAll(x => x.adventurer == adventurer);
        GUI.Instance.BuildHires(_availableHires);
    }

    public void SaveData(GameData gameData)
    {
    }

    public void StartQuest(int id, Actor adventurer)
    {
        id -= _runningQuests.Count;
        if(id < 0)
            return;

        QuestData data = _availableQuests[id];
        Quest quest = new Quest(data, adventurer);
        adventurer.IsOnQuest = true;
        _runningQuests.Add(quest);
        _availableQuests.Remove(data);
        data.CooldownUntil = Tick + data.Cooldown;
        GUI.Instance.BuildQuests(_availableQuests, _runningQuests);
        StartCoroutine(RunQuest(quest));
    }

    public IEnumerator Tutorial()
    {
        IEnumerator adventurerTutorial = TutorialUI.Instance.AdventurerTutorial(_availableHires);
        IEnumerator buildTutorial = TutorialUI.Instance.BuildTutorial();
        IEnumerator questTutorial = TutorialUI.Instance.QuestTutorial();
        adventurerTutorial.MoveNext();
        Paused = true;
        yield return new WaitUntil(() => NextTutorialStep);

        NextTutorialStep = false;
        Actor adventurer = new Actor();
        _availableHires.Add((adventurer, Tick + 500));
        GUI.Instance.BuildHires(_availableHires);
        adventurerTutorial.MoveNext();
        yield return new WaitUntil(() => NextTutorialStep);

        NextTutorialStep = false;
        Paused = false;
        adventurerTutorial.MoveNext();
        yield return new WaitUntil(() => NextTutorialStep);

        adventurerTutorial.MoveNext();
        yield return new WaitForSeconds(1);

        NextTutorialStep = false;
        Paused = true;
        buildTutorial.MoveNext();
        yield return new WaitUntil(() => NextTutorialStep);

        buildTutorial.MoveNext();
        yield return new WaitUntil(() => GameMode == GameMode.Build);

        NextTutorialStep = false;
        buildTutorial.MoveNext();
        yield return new WaitUntil(() => GameMode == GameMode.Play);

        NextTutorialStep = false;
        buildTutorial.MoveNext();

        yield return new WaitForSeconds(1);
        Paused = true;
        for (int i = 0; i < 6; i++)
        {
            NextTutorialStep = false;
            questTutorial.MoveNext();
            yield return new WaitUntil(() => NextTutorialStep);
        }
        TutorialUI.Instance.End();
        Paused = false;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _gameMode = GameMode.Play;
            _camera = Camera.main;

        }
        else
            Destroy(this);
    }

    void BuildingArea(KeyMode mode)
    {
        Vector3Int position = Map.SceneCoordinatesToMapCoordinates(_camera.ScreenToWorldPoint(Input.mousePosition), _levelMin);
        switch (mode)
        {
            case KeyMode.LeftClickDown:
                if (BuildFunctions.CheckPoint(position))
                {
                    _placingArea = true;
                    _graphics.HideHighlight();
                    _graphics.PlaceArea(position, position);
                }
                break;
            case KeyMode.LeftClickHeld:
                if (BuildFunctions.CheckPoint(position) && _placingArea)
                {
                    _graphics.PlaceArea(position);
                }
                break;
            case KeyMode.LeftClickUp:
            case KeyMode.MouseOverUI:
                if (_placingArea)
                {
                    _placingArea = false;

                    _graphics.Confirm();
                    _graphics.UpdateGraphics();
                }
                break;

            case KeyMode.None:
                if (BuildFunctions.CheckPoint(position))
                    BuildFunctions.HighlightPoint(Graphics.Instance._highlight, position);
                else
                    _graphics.HideHighlight();
                break;
        }
    }

    void BuildingDoor(KeyMode mode, WallSprite spriteObject)
    {
        switch (mode)
        {
            case KeyMode.None:
                if (spriteObject != _prevObject)
                {
                    _prevObject = spriteObject;
                    if (spriteObject != null)
                    {
                        WallSprite.PlaceDoorHighlight(spriteObject);
                    }
                    else
                    {
                        _graphics.HideHighlight();
                    }
                }
                break;
            case KeyMode.LeftClickDown:
                if (spriteObject != null)
                {
                    (Vector3Int position, MapAlignment alignment) = spriteObject.GetPosition;
                    if (WallSprite.CheckDoor(position, alignment))
                    {
                        _map.PlaceDoor(position, alignment);

                        WallSprite.CreateDoor(position, alignment);
                        _graphics.UpdateGraphics();
                    }
                }
                break;
        }
    }

    void BuildingLine(KeyMode mode)
    {
        (Vector3Int position, MapAlignment alignment) = Map.GetEdgeFromSceneCoordinates(_camera.ScreenToWorldPoint(Input.mousePosition), _levelMin);
        switch (mode)
        {
            case KeyMode.LeftClickDown:
                if (BuildFunctions.CheckLine(position, alignment))
                {
                    _placingLine = true;
                    _lineStart = position;
                    _lineAlignment = alignment;
                    _graphics.PlaceLine(position, alignment == MapAlignment.XEdge ? position.x : position.y, alignment);
                    _graphics.HideHighlight();
                }
                break;
            case KeyMode.LeftClickHeld:
                if (BuildFunctions.CheckLine(position, alignment) && _placingLine)
                {
                    _graphics.PlaceLine(_lineStart, _lineAlignment == MapAlignment.XEdge ? position.x : position.y, _lineAlignment);
                }
                break;
            case KeyMode.LeftClickUp:
            case KeyMode.MouseOverUI:
                if (_placingLine)
                {
                    _placingLine = false;

                    _graphics.Confirm();
                    _graphics.UpdateGraphics();
                }
                break;

            case KeyMode.None:
                if (BuildFunctions.CheckLine(position, alignment))
                    BuildFunctions.HighlightLine(Graphics.Instance._highlight, position, alignment);
                else
                    _graphics.HideHighlight();
                break;
        }
    }

    void BuildingPoint(KeyMode mode)
    {
        Vector3Int position = Map.SceneCoordinatesToMapCoordinates(_camera.ScreenToWorldPoint(Input.mousePosition), _levelMin);

        switch (mode)
        {
            case KeyMode.None:
                BuildFunctions.HighlightPoint(Graphics.Instance._highlight, position);
                break;
            case KeyMode.LeftClickDown:
                if (BuildFunctions.CheckPoint(position))
                    BuildFunctions.CreatePoint(position);
                break;
        }
    }

    void Demolish(KeyMode mode, SpriteObject spriteObject)
    {
        switch (mode)
        {

            case KeyMode.None:

                if (spriteObject != _prevObject)
                {
                    _prevObject = spriteObject;
                    if (spriteObject != null)
                    {
                        _graphics.HighlightDemolish(spriteObject);
                    }
                    else
                    {
                        _graphics.HideHighlight();
                    }
                }
                break;

            case KeyMode.LeftClickDown:

                if (spriteObject != null)
                {
                    _graphics.Demolish(spriteObject);
                }
                break;
        }
    }

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
        Vector2 ray = _camera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(ray);
        hits = Physics2D.RaycastAll(ray, Vector2.zero);
        SpriteObject nearest = null;
        foreach (RaycastHit2D hit in hits)
        {
            SpriteObject spriteObject = hit.collider.GetComponent<SpriteObject.MouseOver>().SpriteObject;
            if ((nearest?.SpriteRenderer.sortingOrder ?? -1000) < spriteObject.SpriteRenderer.sortingOrder)
                nearest = spriteObject;
        }

        return nearest;
    }

    T GetMouseOver<T>() where T : SpriteObject
    {
        RaycastHit2D[] hits;
        Vector2 ray = _camera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(ray);
        hits = Physics2D.RaycastAll(ray, Vector2.zero);
        SpriteObject nearest = null;
        foreach (RaycastHit2D hit in hits)
        {
            SpriteObject spriteObject = hit.collider.GetComponent<SpriteObject.MouseOver>().SpriteObject;
            if (spriteObject is T && (nearest?.SpriteRenderer.sortingOrder ?? -1000) < spriteObject.SpriteRenderer.sortingOrder)
                nearest = spriteObject;
        }

        return nearest as T;
    }

    IEnumerator RunQuest(Quest quest)
    {
        yield return new WaitUntil(() => !quest.Quester.Pawn.gameObject.activeSelf);
        yield return new WaitForSeconds(quest.Duration);
        GUI.Instance.DisplayQuestResults(quest);
        quest.Quester.IsOnQuest = false;
        _runningQuests.Remove(quest);
        GUI.Instance.BuildQuests(_availableQuests, _runningQuests);
    }
    private void Start()
    {
        _graphics = Graphics.Instance;
        _map = Map.Instance;

        StartCoroutine(Startup());
    }

    IEnumerator Startup()
    {
        yield return new WaitUntil(() => Map.Ready);

        MapChanging?.Invoke();

        for (int i = 0; i < 3; i++)
        {
            List<QuestData> potentialQuests = _allQuests.FindAll(x => x.CooldownUntil <= Tick).Except(_availableQuests).ToList();
            if (potentialQuests.Count > 0)
            {
                QuestData quest = potentialQuests[Random.Range(0, potentialQuests.Count)];
                quest.AvailableUntil = quest.AvailableDuration + Tick;
                _availableQuests.Add(quest);
            }
        }
        GUI.Instance.BuildQuests(_availableQuests, _runningQuests);

        for(int i = 0; i < 3; i++)
        {
            Actor adventurer = new Actor();
            _availableHires.Add((adventurer, Tick + 500));
            _lastAdventurerTick = Tick;
        }
        GUI.Instance.BuildHires(_availableHires);
    }
    void Tock()
    {
        Tick++;

        if (_availableQuests.Count + _runningQuests.Count < 3)
        {
            if (Mathf.Pow(2, (Tick - _lastQuestTick) / 10) / 1000 > Random.Range(0, 100))
            {
                List<QuestData> potentialQuests = _allQuests.FindAll(x => x.CooldownUntil <= Tick).Except(_availableQuests).ToList();
                if (potentialQuests.Count > 0)
                {
                    QuestData quest = potentialQuests[Random.Range(0, potentialQuests.Count)];
                    quest.AvailableUntil = quest.AvailableDuration + Tick;
                    _availableQuests.Add(quest);
                    GUI.Instance.BuildQuests(_availableQuests, _runningQuests);
                    _lastQuestTick = Tick;
                }
            }
        }

        if (_availableHires.Count < 3 && Adventurers.Count + _availableHires.Count < 6)
        {
            if (Mathf.Pow(2, (Tick - _lastAdventurerTick) / 10) / 1000 > Random.Range(0, 100))
            {
                Actor adventurer = new Actor();
                _availableHires.Add((adventurer, Tick + 500));
                GUI.Instance.BuildHires(_availableHires);
                _lastAdventurerTick = Tick;
            }
        }


        _availableHires.RemoveAll(x => x.timeAvailable < Tick);
        GUI.Instance.BuildHires(_availableHires);
        _availableQuests.RemoveAll(x =>
        {
            if (x.AvailableUntil < Tick)
            {
                x.CooldownUntil = Tick + x.Cooldown;
                return true;
            }
            return false;
        });

        Ticked?.Invoke();
    }
    private void Update()
    {
        if(TICK_TIME < _time)
        {
            _time -= TICK_TIME;
            Tock();
        }
        if(!Paused)
            _time += Time.deltaTime;

        if(_doubleClick > 0)
        {
            _doubleClick -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _graphics.CycleMode();
            if (_gameMode == GameMode.Build)
            {
                Graphics.Instance.EnableColliders(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_gameMode == GameMode.Play)
            {
                _gameMode = GameMode.Build;
                BuildFunctions.BuildMode = BuildMode.None;
                Paused = true;
                Graphics.Instance.EnableColliders(true);
                GUI.Instance.SwitchMode(true);
                _playWallMode = Graphics.Instance.Mode;
            }
            else
            {
                _gameMode = GameMode.Play;
                _graphics.HideHighlight();
                Paused = false;
                Graphics.Instance.ResetSprite();
                Graphics.Instance.EnableColliders(false);
                GUI.Instance.SwitchMode(false);
                Graphics.Instance.Mode = _playWallMode;
                MapChanging?.Invoke();
                MapChanged?.Invoke();
            }
        }

        if(Input.mouseScrollDelta.y != 0)
        {
            _camera.orthographicSize -= Input.mouseScrollDelta.y;

            if(_camera.orthographicSize < ZOOMMIN)
                _camera.orthographicSize = ZOOMMIN;
            else if(_camera.orthographicSize > ZOOMMAX)
                _camera.orthographicSize = ZOOMMAX;
        }

        float _cameraSpeed = _camera.orthographicSize / 200;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _camera.transform.Translate(Vector3.up * _cameraSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _camera.transform.Translate(Vector3.down * _cameraSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _camera.transform.Translate(Vector3.left * _cameraSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _camera.transform.Translate(Vector3.right * _cameraSpeed);
        }

        if(Input.GetKeyDown(KeyCode.E))
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

        if (_gameMode == GameMode.Build)
        {
            if (IsMouseOverUI() && !_placingWalls)
                _graphics.HideHighlight();
            else
            {
                KeyMode mode = GetKeyMode();

                switch(BuildFunctions.BuildMode)
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
