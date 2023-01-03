using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (DoTutorialPanel.activeSelf == false)
            End();
    }

    public GameObject HiresTutorial, HireTutorial2, AdventurersMessageBox, Build1, Build2, Build3, Quests1, Quests2, Quests3, Quests4, Quests5, EndMessage;
    public AdventurerProfileUI AdventurerProfilePrefab;
    bool _clickAnwyhere;

    public IEnumerator AdventurerTutorial(List<(Actor, int)> adventurers)
    {
        int i = 0;
        foreach((Actor adventurer, int a) in adventurers)
        {
            AdventurerProfileUI profile = Instantiate(AdventurerProfilePrefab, HiresTutorial.transform);
            profile.SetPanel(adventurer, Mode.HireTutorial);
            RectTransform rect = profile.GetComponent<RectTransform>();
            rect.pivot = Vector3.zero;
            rect.anchorMin = Vector3.zero;
            rect.anchorMax = Vector3.zero;
            rect.anchoredPosition = new Vector2(400 * i / 3f, 0);
            i++;
        }
        HiresTutorial.SetActive(true);
        yield return null;
        _clickAnwyhere = true;
        GUI.Instance.OpenCloseHires();
        HireTutorial2.SetActive(true);
        yield return null;
        TutorialUI.Instance.HireTutorial2.SetActive(false);
        GUI.Instance.CloseHires();
        _clickAnwyhere = true;
        AdventurersMessageBox.SetActive(true);
        yield return null;
        AdventurersMessageBox.SetActive(false);
        yield break;
    }

    public GameObject DoTutorialPanel;

    public void DoTutorial(bool yes)
    {
        if(yes)
        {
            StartCoroutine(GameManager.Instance.Tutorial());
            DoTutorialPanel.SetActive(false);
        }
        else
        {
            End();
        }

        
    }

    public void Hire(Actor adventurer)
    {
        GameManager.Instance.Hire(adventurer);
        HiresTutorial.SetActive(false);
        GameManager.Instance.NextTutorialStep = true;
    }

    public IEnumerator BuildTutorial()
    {
        Build1.SetActive(true);
        _clickAnwyhere = true;
        yield return null;
        Build1.SetActive(false);
        Build2.SetActive(true);
        yield return null;
        Build2.SetActive(false);
        Build3.SetActive(true);
        _clickAnwyhere = true;
        yield return null;
        Build3.SetActive(false);
        yield break;
    }

    public IEnumerator QuestTutorial()
    {
        Quests1.SetActive(true);
        _clickAnwyhere = true;
        yield return null;
        Quests1.SetActive(false);
        Quests2.SetActive(true);
        _clickAnwyhere = true;
        yield return null;
        Quests2.SetActive(false);
        Quests3.SetActive(true);
        GUI.Instance.OpenCloseQuests();
        _clickAnwyhere = true;
        yield return null;
        Quests3.SetActive(false);
        Quests4.SetActive(true);
        GUI.Instance.SelectQuest(0);
        _clickAnwyhere = true;
        yield return null;
        Quests4.SetActive(false);
        Quests5.SetActive(true);
        GUI.Instance.OpenCloseQuests();
        _clickAnwyhere = true;
        yield return null;
        Quests5.SetActive(false);
        EndMessage.SetActive(true);
        _clickAnwyhere = true;
        yield return null;
    }

    public void ClickAnywhere()
    {
        if(_clickAnwyhere)
        {
            GameManager.Instance.NextTutorialStep = true;
            _clickAnwyhere = false;
        }
    }

    public void End()
    {
        GameManager.Instance.Paused = false;
        gameObject.SetActive(false);
    }
}
