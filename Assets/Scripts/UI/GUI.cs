using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The <see cref="GUI"/> class is a singleton that controls the visual UI elements of the game.
/// </summary>
public class GUI : MonoBehaviour
{

    readonly List<AdventurerProfileUI> _availableHires = new();
    readonly List<AdventurerProfileUI> _questAdventurers = new();
    [SerializeField] AdventurerProfileUI _adventurerProfilePrefab;
    [SerializeField] GameObject _adventurerSelect;
    [SerializeField] GameObject _baseBar;
    [SerializeField] GameObject _buildBar;
    [SerializeField] GameObject _debugPanel;
    [SerializeField] TextMeshProUGUI _debugPanelText;
    [SerializeField] GameObject _demolishPanel;
    [SerializeField] HireAdventurerPopup _hirePopup;
    [SerializeField] GameObject _hiresPanel;
    [SerializeField] GameObject _objectPanel;
    Actor _questAdventurer;
    [SerializeField] QuestCompletePopup _questCompletePopup;
    int _questId;
    [SerializeField] GameObject _questPanel;
    [SerializeField] Button _startQuestButton;
    /// <value>Gives reference to the <see cref="GUI"/> singleton.</value>
    public static GUI Instance { get; private set; }

    /// <summary>
    /// Selects an adventurer to go on a <see cref="Quest"/>.
    /// </summary>
    /// <param name="adventurer">The adventurer selected.</param>
    public void AdventurerSelected(Actor adventurer)
    {
        if (_questAdventurer != adventurer)
        {
            _questAdventurer = adventurer;
            _startQuestButton.interactable = true;
        }
        else
        {
            _questAdventurer = null;
            _startQuestButton.interactable = false;
        }
    }

    /// <summary>
    /// Builds the hires panel with the available adventurers.
    /// </summary>
    /// <param name="adventurers">The list of adventurers that can be hired.</param>
    public void BuildHires(List<(Actor, int)> adventurers)
    {
        foreach (AdventurerProfileUI adventurer in _availableHires)
        {
            Destroy(adventurer.gameObject);
        }
        _availableHires.Clear();
        int i = 0;
        foreach ((Actor adventurer, int availableUntil) in adventurers)
        {
            AdventurerProfileUI hirePanel = Instantiate(_adventurerProfilePrefab, _hiresPanel.transform);
            _availableHires.Add(hirePanel);
            hirePanel.SetPanel(adventurer, AdventurerProfileMode.Hire);
            hirePanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(400 * i / 3f, 0);
            i++;
        }
    }

    /// <summary>
    /// Creates the panel of quests.
    /// </summary>
    /// <param name="available">The list of available <see cref="Quest"/>s.</param>
    /// <param name="running">The list of <see cref="Quest"/>s currently running.</param>
    public void BuildQuests(List<QuestData> available, List<Quest> running)
    {
        int i = 0;

        foreach (Quest quest in running)
        {
            Transform questSlot = _questPanel.transform.Find("Quest " + i);
            questSlot.gameObject.SetActive(true);

            questSlot.GetComponent<Image>().color = Color.blue;

            questSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = quest.Name;
            questSlot.Find("Level").GetComponent<TextMeshProUGUI>().text = quest.Level;
            questSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.Description;
            i++;
        }

        foreach (QuestData quest in available)
        {
            Transform questSlot = _questPanel.transform.Find("Quest " + i);
            questSlot.GetComponent<Image>().color = new Color(1, 1, 1, 1 / 3f);
            questSlot.gameObject.SetActive(true);
            questSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = quest.Name;
            questSlot.Find("Level").GetComponent<TextMeshProUGUI>().text = quest.Level;
            questSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.Description;
            i++;
        }

        for (int j = i; j < 3; j++)
        {
            Transform questSlot = _questPanel.transform.Find("Quest " + j);
            questSlot.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Closes the adventurer select panel.
    /// </summary>
    public void CloseAdventurerSelect()
    {
        _adventurerSelect.SetActive(false);
        foreach (AdventurerProfileUI adventurer in _questAdventurers)
        {
            Destroy(adventurer.gameObject);
        }
        _questAdventurers.Clear();
        _startQuestButton.interactable = false;
    }

    /// <summary>
    /// Closes the hires panel.
    /// </summary>
    public void CloseHires()
    {
        foreach (AdventurerProfileUI adventurer in _availableHires)
        {
            Destroy(adventurer.gameObject);
        }
        _availableHires.Clear();
        _hiresPanel.SetActive(false);
    }

    /// <summary>
    /// Closes the objects panel.
    /// </summary>
    public void CloseObjects()
    {
        _objectPanel.SetActive(false);
    }

    /// <summary>
    /// Displays the results of a completed <see cref="Quest"/>.
    /// </summary>
    /// <param name="quest">The <see cref="Quest"/> completed.</param>
    public void DisplayQuestResults(Quest quest)
    {
        _questCompletePopup.QuestComplete(quest);
    }

    /// <summary>
    /// Opens the <see cref="HireAdventurerPopup"/>.
    /// </summary>
    /// <param name="adventurer">The adventurer to be hired or rejected.</param>
    public void HireAdventurer(Actor adventurer)
    {
        _hirePopup.Adventurer = adventurer;
        _hirePopup.gameObject.SetActive(true);
    }

    /// <summary>
    /// Toggles the hires panel.
    /// </summary>
    public void OpenCloseHires()
    {
        if (_hiresPanel.activeSelf)
        {
            foreach (AdventurerProfileUI adventurer in _availableHires)
            {
                Destroy(adventurer.gameObject);
            }
            _availableHires.Clear();
        }
        _hiresPanel.SetActive(!_hiresPanel.activeSelf);
    }

    /// <summary>
    /// Toggles the objects panel.
    /// </summary>
    public void OpenCloseObjects()
    {
        _objectPanel.SetActive(!_objectPanel.activeSelf);
    }

    /// <summary>
    /// Toggles the quests panel.
    /// </summary>
    public void OpenCloseQuests()
    {
        if (_questPanel.activeSelf)
            CloseAdventurerSelect();
        _questPanel.SetActive(!_questPanel.activeSelf);
    }

    /// <summary>
    /// Select a specific <see cref="Quest"/> so that an adventurer can be sent on it.
    /// </summary>
    /// <param name="id">The id of the <see cref="Quest"/>.</param>
    public void SelectQuest(int id)
    {
        _questId = id;
        int i = 0;
        foreach (Actor adventurer in GameManager.Instance.Adventurers)
        {
            if (adventurer.IsOnQuest)
                continue;

            AdventurerProfileUI questAdventurer = Instantiate(_adventurerProfilePrefab, _adventurerSelect.transform);
            _questAdventurers.Add(questAdventurer);
            questAdventurer.SetPanel(adventurer, AdventurerProfileMode.AdventurerSelect);
            questAdventurer.GetComponent<RectTransform>().anchoredPosition = new Vector3(400 * (i % 3) / 3f, -30 - 100 * (i / 3));
            i++;
        }
        _startQuestButton.interactable = false;
        _adventurerSelect.SetActive(true);
    }

    public void SetDebugPanel(AdventurerPawn pawn)
    {
        if (pawn != null)
        {
            _debugPanel.gameObject.SetActive(true);
            _debugPanel.transform.position = Input.mousePosition;
            _debugPanelText.text = pawn.CurrentTask.GetType().Name + "\n" + pawn.CurrentAction.GetType().Name + "\n" + pawn.CurrentStep.GetType().Name;
        }
        else
            _debugPanel.gameObject.SetActive(false);
    }
    /// <summary>
    /// Initiates a <see cref="Quest"/> after the questing adventurer has been selected.
    /// </summary>
    public void StartQuest()
    {
        CloseAdventurerSelect();
        _questPanel.SetActive(false);
        GameManager.Instance.StartQuest(_questId, _questAdventurer);
    }

    /// <summary>
    /// Swithces the panel at the bottom of the screen when switching <see cref="GameMode"/>.
    /// </summary>
    /// <param name="build">True if the mode is being switched to build mode.</param>
    public void SwitchMode(bool build)
    {
        _baseBar.SetActive(!build);
        _buildBar.SetActive(build);
        _demolishPanel.SetActive(build);
        CloseObjects();
    }

    /// <inheritdoc/>
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
