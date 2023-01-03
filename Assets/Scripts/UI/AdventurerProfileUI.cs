using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Mode
{
    AdventurerSelect,
    Hire,
    HireTutorial
}

public class AdventurerProfileUI : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Class;
    public Image Icon;
    public Image StrengthBar;
    public Image DexterityBar;
    public Image CharismaBar;
    public Image IntelligenceBar;

    Actor _adventurer;

    Mode _mode;

    public void SetPanel(Actor adventurer, Mode mode)
    {
        _adventurer = adventurer;
        _mode = mode;
        Name.text = adventurer.Stats.Name;
        Class.text = $"{adventurer.Race.ToString()} {adventurer.Class.ToString()} {adventurer.Level}";
        Icon.sprite = adventurer.ProfileSprite;
        int[] abilityScores = adventurer.Stats.Abilities;
        StrengthBar.rectTransform.sizeDelta = new Vector2(abilityScores[0] * 2.5f, 10);
        DexterityBar.rectTransform.sizeDelta = new Vector2(abilityScores[1] * 2.5f, 10);
        CharismaBar.rectTransform.sizeDelta = new Vector2(abilityScores[2] * 2.5f, 10);
        IntelligenceBar.rectTransform.sizeDelta = new Vector2(abilityScores[3] * 2.5f, 10);
    }

    public void Select()
    {
        switch (_mode)
        {
            case Mode.AdventurerSelect:
                GUI.Instance.AdventurerSelected(_adventurer);
                break;
            case Mode.Hire:
                GUI.Instance.HireAdventurer(_adventurer);
                break;
            case Mode.HireTutorial:
                TutorialUI.Instance.Hire(_adventurer);
                break;
        }
    }
}
