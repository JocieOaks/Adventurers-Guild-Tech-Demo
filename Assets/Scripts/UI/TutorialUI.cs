using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AI.Actor;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The <see cref="TutorialUI"/> class is a singleton controls the UI elements for the game's tutorial.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        private bool _clickAnywhere;
        [SerializeField][UsedImplicitly] private AdventurerProfileUI _adventurerProfilePrefab;

        [SerializeField][UsedImplicitly] private GameObject _doTutorialPanel;
        [SerializeField][UsedImplicitly] private GameObject _hiresTutorial, _hireTutorial2, _adventurersMessageBox, _build1, _build2, _build3, _quests1, _quests2, _quests3, _quests4, _quests5, _endMessage;

        /// <value>Gives the singleton instance of <see cref="TutorialUI"/>.</value>
        public static TutorialUI Instance { get; private set; }

        /// <summary>
        /// Controls the tutorial going over hiring adventurer's.
        /// </summary>
        /// <param name="adventurers">The list of <see cref="Actor"/>'s that can potentially be hired as adventurers.</param>
        /// <returns>Returns to give control back to the calling function.</returns>
        public IEnumerator AdventurerTutorial(List<(Actor, int)> adventurers)
        {
            int i = 0;
            foreach ((Actor adventurer, int _) in adventurers)
            {
                AdventurerProfileUI profile = Instantiate(_adventurerProfilePrefab, _hiresTutorial.transform);
                profile.SetPanel(adventurer, AdventurerProfileMode.HireTutorial);
                RectTransform rect = profile.GetComponent<RectTransform>();
                rect.pivot = Vector3.zero;
                rect.anchorMin = Vector3.zero;
                rect.anchorMax = Vector3.zero;
                rect.anchoredPosition = new Vector2(400 * i / 3f, 0);
                i++;
            }
            _hiresTutorial.SetActive(true);
            yield return null;
            _clickAnywhere = true;
            GUI.Instance.OpenCloseHires();
            _hireTutorial2.SetActive(true);
            yield return null;
            TutorialUI.Instance._hireTutorial2.SetActive(false);
            GUI.Instance.CloseHires();
            _clickAnywhere = true;
            _adventurersMessageBox.SetActive(true);
            yield return null;
            _adventurersMessageBox.SetActive(false);
        }

        /// <summary>
        /// Controls the tutorial over how to build.
        /// </summary>
        /// <returns>Returns to give control back to the calling function.</returns>
        public IEnumerator BuildTutorial()
        {
            _build1.SetActive(true);
            _clickAnywhere = true;
            yield return null;
            _build1.SetActive(false);
            _build2.SetActive(true);
            yield return null;
            _build2.SetActive(false);
            _build3.SetActive(true);
            _clickAnywhere = true;
            yield return null;
            _build3.SetActive(false);
        }

        /// <summary>
        /// When clicked, indicates to the <see cref="GameManager"/> that the tutorial can move on to the next step.
        /// </summary>
        [UsedImplicitly]
        public void ClickAnywhere()
        {
            if (_clickAnywhere)
            {
                GameManager.Instance.NextTutorialStep = true;
                _clickAnywhere = false;
            }
        }

        /// <summary>
        /// Determines whether the tutorial is done.
        /// </summary>
        /// <param name="yes">If true, the tutorial will begin.</param>
        [UsedImplicitly]
        public void DoTutorial(bool yes)
        {
            if (yes)
            {
                StartCoroutine(GameManager.Instance.Tutorial());
                _doTutorialPanel.SetActive(false);
            }
            else
            {
                StartCoroutine(End());
            }
        }

        /// <summary>
        /// Ends the tutorial, making the game playable.
        /// </summary>
        public IEnumerator End()
        {
            yield return new WaitUntil(() => GameManager.GameReady);
            GameManager.Instance.Paused = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Moves the tutorial on to the next step once an adventurer has been hired.
        /// </summary>
        /// <param name="adventurer">The adventurer being hired.</param>
        public void Hire(Actor adventurer)
        {
            GameManager.Instance.Hire(adventurer);
            _hiresTutorial.SetActive(false);
            GameManager.Instance.NextTutorialStep = true;
        }

        /// <summary>
        /// Controls the tutorial explaining <see cref="Quest"/>s.
        /// </summary>
        /// <returns>Returns to give control back to the calling function.</returns>
        public IEnumerator QuestTutorial()
        {
            _quests1.SetActive(true);
            _clickAnywhere = true;
            yield return null;
            _quests1.SetActive(false);
            _quests2.SetActive(true);
            _clickAnywhere = true;
            yield return null;
            _quests2.SetActive(false);
            _quests3.SetActive(true);
            GUI.Instance.OpenCloseQuests();
            _clickAnywhere = true;
            yield return null;
            _quests3.SetActive(false);
            _quests4.SetActive(true);
            GUI.Instance.SelectQuest(0);
            _clickAnywhere = true;
            yield return null;
            _quests4.SetActive(false);
            _quests5.SetActive(true);
            GUI.Instance.OpenCloseQuests();
            _clickAnywhere = true;
            yield return null;
            _quests5.SetActive(false);
            _endMessage.SetActive(true);
            _clickAnywhere = true;
            yield return null;
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            if (_doTutorialPanel.activeSelf == false)
                StartCoroutine(End());
        }
    }
}
