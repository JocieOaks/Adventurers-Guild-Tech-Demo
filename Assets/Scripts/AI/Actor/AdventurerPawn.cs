using System.Collections;
using Assets.Scripts.AI.Action;
using Assets.Scripts.AI.Navigation;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.AI.Social;
using Assets.Scripts.AI.Step;
using Assets.Scripts.AI.Task;
using Assets.Scripts.Map.Node;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.AI.Actor
{
    /// <summary>
    /// The <see cref="AdventurerPawn"/> class is the counterpart to the <see cref="AI.Actor.Actor"/> class that controls the active functional aspect of an NPC, including the in game sprite representation and overseeing the AI behaviors.
    /// </summary>
    public class AdventurerPawn : Pawn
    {
        private Actor _actor;
        private Planner _planner;

        [SerializeField][UsedImplicitly] private SpriteRenderer _emoji;
        [SerializeField][UsedImplicitly] private SpriteRenderer _speechBubble;

        /// <value>The <see cref="AdventurerPawn"/>'s corresponding <see cref="AI.Actor.Actor"/>.</value>
        public Actor Actor
        {
            get => _actor;
            set => _actor ??= value;
        }

        /// <value>The <see cref="DLite{T}"/> navigator the <see cref="AdventurerPawn"/> uses to navigate through a room.</value>
        public NavigateRoom NavigateRoom { get; private set; }

        /// <value>The <see cref="DLite{T}"/> navigator the <see cref="AdventurerPawn"/> uses to navigate through the map.</value>
        public NavigateMap NavigateMap { get; private set; }

        /// <value>The <see cref="Task"/> the <see cref="AdventurerPawn"/> is currently performing.</value>
        public Task.Task CurrentTask { get; private set; }

        /// <value>Returns true if the <see cref="AdventurerPawn"/> is currently engaged in a <see cref="Conversation"/> with another <see cref="AdventurerPawn"/>.</value>
        public bool IsInConversation => Social.Conversation != null;

        /// <value> Returns true if the <see cref="AdventurerPawn"/> is currently speaking.</value>
        public bool IsSpeaking => _speechBubble.gameObject.activeSelf;

        /// <inheritdoc/>
        public override RoomNode CurrentNode
        {
            get => base.CurrentNode;
            protected set
            {
                base.CurrentNode = value;
                OnLevelChange();
                NavigateRoom?.UpdateStart(value);
            }
        }

        /// <inheritdoc/>
        public override string Name => Actor.Name;

        /// <value>The <see cref="SocialAI"/> that runs the <see cref="AdventurerPawn"/>'s social behaviors.</value>
        public SocialAI Social { get; private set; }

        /// <summary>
        /// Sets the <see cref="AdventurerPawn"/> to begin going on a <see cref="Quest"/>
        /// </summary>
        public void BeginQuest()
        {
            Social.EndConversation();
            OverrideTask(new QuestTask());
            Social.Silenced = true;
        }

        /// <summary>
        /// Force a new <see cref="Task"/> for the <see cref="AdventurerPawn"/> to take, without waiting for the previous <see cref="Task"/> and <see cref="TaskAction"/>s to complete.
        /// </summary>
        /// <param name="task">The new <see cref="Task"/> for the <see cref="AdventurerPawn"/> to perform.</param>
        public void OverrideTask(Task.Task task)
        {
            CurrentTask = task;
            TaskActions.Clear();
            foreach (TaskAction action in CurrentTask.GetActions(Actor))
                TaskActions.Enqueue(action);

            CurrentStep.ForceFinish();
            CurrentStep = new WaitStep(this, null, false);

            CurrentAction = TaskActions.Dequeue();
            CurrentAction.Initialize();

            _planner.OverrideTask(task);
        }

        /// <summary>
        /// Displays a speech bubble over the <see cref="AdventurerPawn"/>'s <see cref="Sprite"/>, to visually indicate that they are speaking with another <see cref="AdventurerPawn"/>.
        /// </summary>
        /// <param name="type">The type of speech that the <see cref="AdventurerPawn"/> is engaging in, which indicates the type of symbol that should be used.</param>
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

        /// <inheritdoc/>
        protected override void OnTaskFail()
        {
            TaskActions.Clear();
            if (!CurrentNode.Traversable)
            {
                foreach ((RoomNode node, float _) in CurrentNode.NextNodes)
                {
                    if (node.Traversable)
                    {
                        ForcePosition(node);
                        break;
                    }
                }
                if (!CurrentNode.Traversable)
                    ForcePosition(Vector3Int.one);
            }

            if (CurrentTask is IRecoverableTask recovery && Recovery < 4)
            {
                foreach (TaskAction action in recovery.Recover(Actor, CurrentAction))
                    TaskActions.Enqueue(action);
            }
            else
            {
                CurrentTask = new WaitTask(0.5f);
                foreach (TaskAction action in CurrentTask.GetActions(Actor))
                    TaskActions.Enqueue(action);

                _planner.OverrideTask(CurrentTask);
            }
            CurrentAction = null;
        }

        /// <inheritdoc/>
        protected override void OnTaskFinish()
        {
            CurrentTask = _planner.GetTask();

            foreach (TaskAction action in CurrentTask.GetActions(Actor))
                TaskActions.Enqueue(action);
        }

        /// <inheritdoc/>
        protected override IEnumerator Startup()
        {
            yield return new WaitUntil(() => GameManager.GameReady);

            CurrentNode = Map.Map.Instance[Utility.Utility.SceneCoordinatesToMapCoordinates(transform.position, 0)];

            WorldPositionNonDiscrete = WorldPosition;

            NavigateRoom = new NavigateRoom(this);
            NavigateMap = new NavigateMap(this);

            InitializeGameObject();

            InitializeAI();

            Map.Map.Instance[0].EnterRoom(this);

            Graphics.LevelChanged += OnLevelChange;
            Graphics.UpdatedGraphics += BuildSpriteMask;
            Graphics.LevelChangedLate += BuildSpriteMask;

            Ready = true;
        }

        /// <summary>
        /// Initializes the Planner and Social for the Pawn, and setup the starting Task.
        /// </summary>
        private void InitializeAI()
        {
            Social = new SocialAI(this);

            CurrentTask = new WaitTask(0.5f);

            _planner = new Planner(Actor, CurrentTask);

            foreach (TaskAction action in CurrentTask.GetActions(Actor))
                TaskActions.Enqueue(action);

            CurrentAction = TaskActions.Dequeue();
            CurrentAction.Initialize();

            CurrentStep = new WaitStep(this, null, false);
        }

        // Update is called once per frame
        [UsedImplicitly]
        private void Update()
        {
            if (GameManager.Instance.Paused || !Ready) return;

            _planner.AStar();
            ManageTask();

            Actor.Update();

            try
            {
                CurrentAction?.Perform();
                CurrentStep?.Perform();
            }
            catch
            {
                OnTaskFail();
                throw;
            }
        }
    }
}

