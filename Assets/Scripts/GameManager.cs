using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Data;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.UI;
using JetBrains.Annotations;
using Unity.Jobs;
using UnityEngine;
using GUI = Assets.Scripts.UI.GUI;


namespace Assets.Scripts
{
    /// <summary>
    /// The current mode in which the game is being played.
    /// </summary>
    public enum GameMode
    {
        /// <summary>The game is active and the player can interact with <see cref="Pawn"/>s.</summary>
        Play,
        /// <summary>The game is active, but the player does not control the <see cref="PlayerPawn"/>.</summary>
        Overview,
        /// <summary>The game is paused, and the player can build <see cref="SpriteObject"/>s on the <see cref="Map"/>.</summary>
        Build
    }

    /// <summary>
    /// The <see cref="GameManager"/> class is a singleton that controls the basic flow of the game.
    /// </summary>
    public class GameManager : MonoBehaviour, IDataPersistence
    {
        private const float TICK_TIME = 0.2f;
        private const float ZOOM_MAX = 40;
        private const float ZOOM_MIN = 5;

        private readonly List<(Actor adventurer, int timeAvailable)> _availableHires = new();
        private readonly List<QuestData> _availableQuests = new();
        private readonly List<Quest> _runningQuests = new();

        private List<QuestData> _allQuests;
        private int _lastAdventurerTick;
        private int _lastQuestTick;
        [SerializeField][UsedImplicitly] private PlayerPawn _player;
        private WallDisplayMode _playWallMode;
        private float _time;

        /// <summary>
        /// Invoked when the <see cref="Map"/> has been modified.
        /// </summary>
        public static event System.Action MapChanged;

        /// <summary>
        /// Invoked when the <see cref="Map"/> is being modified.
        /// </summary>

        public static event System.Action MapChanging;

        /// <summary>
        /// Invoked when the <see cref="Map"/> is being modified, but after <see cref="MapChanging"/> is called.
        /// </summary>

        public static event System.Action MapChangingLate;

        /// <summary>
        /// Invoked each frame. Used for classes that don't inherit from <see cref="MonoBehaviour"/>.
        /// </summary>
        public static event System.Action NonMonoUpdate;

        /// <summary>
        /// Invoked every second while the game is not paused.
        /// </summary>
        public static event System.Action Ticked;

        /// <value>The primary game camera.</value>
        public static Camera Camera => Camera.main;

        /// <value>Determines whether the game has completed initial setup.</value>
        public static bool GameReady { get; private set; }

        /// <value>Reference to the <see cref="GameManager"/> singleton instance.</value>
        public static GameManager Instance { get; private set; }

        /// <value>The list of all hired adventurers.</value>
        public List<Actor> Adventurers { get; } = new();

        /// <value>The current <see cref="GameMode"/>.</value>
        public GameMode GameMode { get; private set; }

        /// <value>The maximum z value of the current level. Anything above this will not be rendered on screen.</value>
        public int LevelMax { get; private set; } = 6;

        /// <value>The minimum z value of the current level.</value>
        public int LevelMin { get; private set; }

        /// <value>The list of random names for randomly generated <see cref="Actor"/>s.</value>
        public List<string> Names { get; private set; }

        /// <value>Determines whether the tutorial should move on to the next step.</value>
        public bool NextTutorialStep { get; set; }

        /// <value>The list of predefined <see cref="Actor"/>s.</value>
        public List<Actor> NPCs { get; } = new();

        /// <value>The count of <see cref="SpriteObject"/>s that are waiting to begin initial setup at the start of the game.</value>
        public int ObjectsReady { get; set; }

        /// <value>Whether the game is currently paused.</value>
        public bool Paused { get; set; } = true;

        /// <value>The count of game time in seconds.</value>
        public int Tick { get; private set; }

        /// <summary>
        /// Modifies the current level. Level determines how objects are rendered on screen, based on being above, below, or at the level.
        /// </summary>
        /// <param name="up">The number of <see cref="Layer"/>s to shift the screen upward. Negative values shift it downward.</param>
        public void ChangeLevel(bool up)
        {
            if (up)
            {
                Layer upLayer = Map.Map.Instance[LevelMin, 1];
                if (upLayer != null)
                {
                    Camera.transform.position += Vector3.up * (LevelMax - LevelMin);
                    LevelMin = upLayer.Origin.z;
                    LevelMax = upLayer.Origin.z + upLayer.Height;

                }
            }
            else
            {
                Layer downLayer = Map.Map.Instance[LevelMin, -1];
                if (downLayer != null)
                {
                    Camera.transform.position += Vector3.down * (LevelMax - LevelMin);
                    LevelMin = downLayer.Origin.z;
                    LevelMax = downLayer.Origin.z + downLayer.Height;
                }
            }
            Graphics.Instance.SetLevel();
        }

        /// <summary>
        /// Changes the <see cref="Scripts.GameMode"/> between <see cref="GameMode.Build"/> and <see cref="GameMode.Play"/>.
        /// </summary>
        public void CycleGameMode(GameMode gameMode)
        {
            if (gameMode == GameMode.Build)
            {
                if (GameMode == GameMode.Play)
                {
                    GameMode = GameMode.Build;
                    BuildFunctions.BuildMode = BuildMode.None;
                    Paused = true;
                    GUI.Instance.SwitchMode(true);
                    _playWallMode = Graphics.Instance.Mode;
                }
                else
                {
                    GameMode = GameMode.Play;
                    Graphics.Instance.HideHighlight();
                    Paused = false;
                    Graphics.Instance.ResetSprite();
                    GUI.Instance.SwitchMode(false);
                    Graphics.Instance.Mode = _playWallMode;
                    MapChanging?.Invoke();
                    MapChangingLate?.Invoke();
                    MapChanged?.Invoke();

#if DEBUG
                    DebugHighlightRoomNodes();
#endif
                }
            }
            else
            {
                if (GameMode == GameMode.Play)
                    GameMode = GameMode.Overview;
                else if (GameMode == GameMode.Overview)
                {
                    GameMode = GameMode.Play;
#if DEBUG
                    {
                        GUI.Instance.SetDebugPanel(null);
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="Actor"/> to the list of hired adventurers in the guild.
        /// </summary>
        /// <param name="adventurer">The <see cref="Actor"/> being hired.</param>
        public void Hire(Actor adventurer)
        {
            Adventurers.Add(adventurer);
            _availableHires.RemoveAll(x => x.adventurer == adventurer);
            GUI.Instance.BuildHires(_availableHires);
            adventurer.InitializePawn(Vector3Int.one);
        }

        /// <summary>
        /// Determines if an object at the given position is above below or on the current level, determining how it should be rendered.
        /// </summary>
        /// <param name="z">The z coordinate of the object.</param>
        /// <returns>Returns 1 is <c>z</c> is above the level, -1 if <c>z</c> is below the level, and 0 if <c>z</c> is on the level.</returns>
        public int IsOnLevel(int z)
        {
            if (z >= LevelMax)
                return 1;
            else if (z < LevelMin)
                return -1;
            else
                return 0;
        }

        /// <inheritdoc/>
        public void LoadData(GameData gameData)
        {
            Names = gameData.Names;
            _allQuests = gameData.Quests;
        }

        /// <summary>
        /// Rejects an <see cref="Actor"/> removing them from the list of hireable adventurers.
        /// </summary>
        /// <param name="adventurer">The <see cref="Actor"/> being rejected.</param>
        public void Reject(Actor adventurer)
        {
            _availableHires.RemoveAll(x => x.adventurer == adventurer);
            GUI.Instance.BuildHires(_availableHires);
        }

        /// <inheritdoc/>
        public void SaveData(GameData gameData)
        {
        }

        /// <summary>
        /// Initiates a new <see cref="Quest"/>.
        /// </summary>
        /// <param name="id">The index of the <see cref="QuestData"/> in <see cref="_availableQuests"/>.</param>
        /// <param name="adventurer">The <see cref="Actor"/> going on the <see cref="Quest"/>.</param>
        public void StartQuest(int id, Actor adventurer)
        {
            id -= _runningQuests.Count;
            if (id < 0)
                return;

            QuestData data = _availableQuests[id];
            Quest quest = new(data, adventurer);
            adventurer.IsOnQuest = true;
            _runningQuests.Add(quest);
            _availableQuests.Remove(data);
            data.CooldownUntil = Tick + data.Cooldown;
            GUI.Instance.BuildQuests(_availableQuests, _runningQuests);
            StartCoroutine(RunQuest(quest));
        }

        /// <summary>
        /// Moves <see cref="Camera"/>.
        /// </summary>
        /// <param name="translation">The direction in which to move <see cref="Camera"/>.</param>
        public void TranslateCamera(Vector3 translation)
        {
            float cameraSpeed = Camera.orthographicSize / 200;

            Camera.transform.Translate(translation * cameraSpeed);
        }

        /// <summary>
        /// Controls the start of game tutorial.
        /// </summary>
        /// <returns>Yield returns <see cref="WaitUntil"/> objects until the condition has been met for the tutorial to continue.</returns>
        public IEnumerator Tutorial()
        {
            IEnumerator adventurerTutorial = TutorialUI.Instance.AdventurerTutorial(_availableHires);
            IEnumerator buildTutorial = TutorialUI.Instance.BuildTutorial();
            IEnumerator questTutorial = TutorialUI.Instance.QuestTutorial();
            adventurerTutorial.MoveNext();
            Paused = true;
            yield return new WaitUntil(() => NextTutorialStep);

            NextTutorialStep = false;
            Actor adventurer = new(out JobHandle jobHandle);
            _availableHires.Add((adventurer, Tick + 500));

            yield return new WaitUntil(() => jobHandle.IsCompleted);
            jobHandle.Complete();
            adventurer.MakeSpritesList();
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

            StartCoroutine(TutorialUI.Instance.End());
            Paused = false;
        }

        /// <summary>
        /// Zooms <see cref="Camera"/> in or out.
        /// </summary>
        /// <param name="zooming">The change to <see cref="Camera"/>'s size.</param>
        public void ZoomCamera(float zooming)
        {
            Camera.orthographicSize -= zooming;

            if (Camera.orthographicSize < ZOOM_MIN)
                Camera.orthographicSize = ZOOM_MIN;
            else if (Camera.orthographicSize > ZOOM_MAX)
                Camera.orthographicSize = ZOOM_MAX;
        }
        [UsedImplicitly]
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GameMode = GameMode.Play;
            }
            else
                Destroy(this);
        }

        /// <summary>
        /// Creates a new <see cref="Actor"/> and adds them to the list of available hires. Waits for the <see cref="Actor"/>'s sprite sheet to be created on a worker thread.
        /// </summary>
        /// <returns>Yield returns <see cref="WaitUntil"/> objects to wait for <see cref="Graphics.BuildSpriteJob"/> to complete.</returns>
        private IEnumerator CreateActor()
        {
            Actor adventurer = new(out JobHandle jobHandle);
            yield return new WaitUntil(() => jobHandle.IsCompleted);
            jobHandle.Complete();
            adventurer.MakeSpritesList();
            _availableHires.Add((adventurer, Tick + 500));
            GUI.Instance.BuildHires(_availableHires);
            _lastAdventurerTick = Tick;
        }

        private void DebugHighlightRoomNodes()
        {
            foreach (RoomNode node in Map.Map.Instance.AllNodes)
            {
                if (node != null && node != RoomNode.Undefined)
                {
                    if (!node.Traversable)
                    {
                        node.Floor.SpriteRenderer.color = Color.red;
                    }
                    else if (node.Reserved)
                    {
                        node.Floor.SpriteRenderer.color = Color.yellow;
                    }
                    else
                    {
                        node.Floor.SpriteRenderer.color = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// Runs the given <see cref="Quest"/>.
        /// </summary>
        /// <param name="quest">The <see cref="Quest"/> being run.</param>
        /// <returns>Yield returns <see cref="WaitUntil"/> objects, first to wait for the <see cref="Pawn"/> to leave the map, then wait the duration of the <see cref="Quest"/>.</returns>
        private IEnumerator RunQuest(Quest quest)
        {
            yield return new WaitUntil(() => !quest.Quester.Pawn.gameObject.activeSelf);
            yield return new WaitForSeconds(quest.Duration);
            GUI.Instance.DisplayQuestResults(quest);
            quest.Quester.IsOnQuest = false;
            _runningQuests.Remove(quest);
            GUI.Instance.BuildQuests(_availableQuests, _runningQuests);
        }

        [UsedImplicitly]
        private void Start()
        {
            StartCoroutine(Startup());
        }

        /// <summary>
        /// Runs the initial setup for the game.
        /// </summary>
        /// <returns>Yield returns <see cref="WaitUntil"/> objects until the <see cref="Map"/> and all <see cref="SpriteObject"/>s have completed setup.</returns>
        private IEnumerator Startup()
        {
            yield return new WaitUntil(() => Map.Map.Ready && ObjectsReady <= 0);

            SitDestination.OnMapReady();
            LayDestination.OnMapReady();
            FoodDestination.OnMapReady();

            MapChanging?.Invoke();
            MapChangingLate?.Invoke();
#if DEBUG
            DebugHighlightRoomNodes();
#endif

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
                StartCoroutine(CreateActor());
            }

            GameReady = true;
        }
        /// <summary>
        /// Called for every second that passes, updates <see cref="Tick"/> and performs periodic actions.
        /// </summary>
        private void Tock()
        {
            Tick++;

            if (_availableQuests.Count + _runningQuests.Count < 3)
            {
                if (Mathf.Pow(2, (Tick - _lastQuestTick) / 10f) / 1000 > Random.Range(0, 100))
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
                if (Mathf.Pow(2, (Tick - _lastAdventurerTick) / 10f) / 1000 > Random.Range(0, 100))
                {
                    StartCoroutine(CreateActor());
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

        [UsedImplicitly]
        private void Update()
        {
            if(TICK_TIME < _time)
            {
                _time -= TICK_TIME;
                Tock();
            }
            if(!Paused)
                _time += Time.deltaTime;

            if (GameMode == GameMode.Play)
            {
                Camera.transform.position = _player.transform.position + Vector3.back * 10;
            }

            NonMonoUpdate?.Invoke();
        }
    }
}