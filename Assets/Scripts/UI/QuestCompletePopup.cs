using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCompletePopup : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Results;
    public TextMeshProUGUI Gold;
    public TextMeshProUGUI Prestige;

    public void QuestComplete(Quest quest)
    {
        Icon.sprite = quest.Quester.ProfileSprite;
        (string text, int gold, int prestige) = quest.Results();
        Results.text = string.Format(text, quest.Quester.Stats.Name, quest.Quester.Stats.Class.ToString());
        Gold.text = $"Gold Earned: {gold}";
        Prestige.text = $"Prestige Earned: {prestige}";
        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        gameObject.SetActive(false);
    }
}
