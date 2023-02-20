using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GUI : MonoBehaviour
{

    public static GUI Instance;
    public AdventurerProfileUI AdventurerProfilePrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public GameObject QuestPanel;

    public GameObject AdventurerSelect;

    public GameObject HiresPanel;

    public GameObject ObjectPanel;

    public GameObject BuildBar;

    public GameObject BaseBar;

    public GameObject DemolishPanel;

    public Button StartQuestButton;

    public HireAdventurerPopup HirePopup;

    public void SwitchMode(bool build)
    {
        BaseBar.SetActive(!build);
        BuildBar.SetActive(build);
        DemolishPanel.SetActive(build);
        CloseObjects();
    }

    public void OpenCloseQuests()
    {
        if (QuestPanel.activeSelf)
            CloseAdventurerSelect();
        QuestPanel.SetActive(!QuestPanel.activeSelf);
    }

    public void OpenCloseHires()
    {
        if (HiresPanel.activeSelf)
        {
            foreach (AdventurerProfileUI adventurer in _availableHires)
            {
                Destroy(adventurer.gameObject);
            }
            _availableHires.Clear();
        }
        HiresPanel.SetActive(!HiresPanel.activeSelf);
    }

    public void OpenCloseObjects()
    {
        ObjectPanel.SetActive(!ObjectPanel.activeSelf);
    }

    public void CloseObjects()
    {
        ObjectPanel.SetActive(false);
    }

    public void CloseHires()
    {
        foreach (AdventurerProfileUI adventurer in _availableHires)
        {
            Destroy(adventurer.gameObject);
        }
        _availableHires.Clear();
        HiresPanel.SetActive(false);
    }

    readonly List<AdventurerProfileUI> _questAdventurers = new();

    public void SelectQuest(int id)
    {
        _questId = id;
        int i = 0;
        foreach (Actor adventurer in GameManager.Instance.Adventurers)
        {
            if (adventurer.IsOnQuest)
                continue;

            AdventurerProfileUI questAdventurer = Instantiate(AdventurerProfilePrefab, AdventurerSelect.transform);
            _questAdventurers.Add(questAdventurer);
            questAdventurer.SetPanel(adventurer, Mode.AdventurerSelect);
            questAdventurer.GetComponent<RectTransform>().anchoredPosition = new Vector3(400 * (i % 3) / 3f, -30 -100 * (i / 3));
            i++;
        }
        StartQuestButton.interactable = false;
        AdventurerSelect.SetActive(true);
    }

    Actor _questAdventurer;
    int _questId;

    public void AdventurerSelected(Actor adventurer)
    {
        if (_questAdventurer != adventurer)
        {
            _questAdventurer = adventurer;
            StartQuestButton.interactable = true;
        }
        else
        {
            _questAdventurer = null;
            StartQuestButton.interactable = false;
        }
    }

    public void CloseAdventurerSelect()
    {
        AdventurerSelect.SetActive(false);
        foreach(AdventurerProfileUI adventurer in _questAdventurers)
        {
            Destroy(adventurer.gameObject);
        }
        _questAdventurers.Clear();
        StartQuestButton.interactable = false;
    }

    public void StartQuest()
    {
        CloseAdventurerSelect();
        QuestPanel.SetActive(false);
        GameManager.Instance.StartQuest(_questId, _questAdventurer);
    }

    public QuestCompletePopup popup;

    public void DisplayQuestResults(Quest quest)
    {
        popup.QuestComplete(quest);
    }

    readonly List<AdventurerProfileUI> _availableHires = new();

    public void HireAdventurer(Actor adventurer)
    {
        HirePopup.Adventurer = adventurer;
        HirePopup.gameObject.SetActive(true);
    }

    public void BuildHires(List<(Actor,int)> adventurers)
    {
        foreach (AdventurerProfileUI adventurer in _availableHires)
        {
            Destroy(adventurer.gameObject);
        }
        _availableHires.Clear();
        int i = 0;
        foreach ((Actor adventurer, int availableUntil) in adventurers)
        {
            AdventurerProfileUI hirePanel = Instantiate(AdventurerProfilePrefab, HiresPanel.transform);
            _availableHires.Add(hirePanel);
            hirePanel.SetPanel(adventurer, Mode.Hire);
            hirePanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(400 * i / 3f, 0);
            i++;
        }
    }

    public void BuildQuests(List<QuestData> available, List<Quest> running)
    {
        int i = 0;

        foreach(Quest quest in running)
        {
            Transform questSlot = QuestPanel.transform.Find("Quest " + i);
            questSlot.gameObject.SetActive(true);

            questSlot.GetComponent<Image>().color = Color.blue;

            questSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = quest.Name;
            questSlot.Find("Level").GetComponent<TextMeshProUGUI>().text = quest.Level;
            questSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.Description;
            i++;
        }

        foreach (QuestData quest in available)
        {
            Transform questSlot = QuestPanel.transform.Find("Quest " + i);
            questSlot.GetComponent<Image>().color = new Color(1,1,1,1/3f);
            questSlot.gameObject.SetActive(true);
            questSlot.Find("Name").GetComponent<TextMeshProUGUI>().text = quest.Name;
            questSlot.Find("Level").GetComponent<TextMeshProUGUI>().text = quest.Level;
            questSlot.Find("Description").GetComponent<TextMeshProUGUI>().text = quest.Description;
            i++;
        }

        for(int j = i; j < 3; j++)
        {
            Transform questSlot = QuestPanel.transform.Find("Quest " + j);
            questSlot.gameObject.SetActive(false);
        }
    }
}
