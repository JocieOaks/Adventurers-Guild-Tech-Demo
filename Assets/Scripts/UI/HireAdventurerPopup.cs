using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HireAdventurerPopup : MonoBehaviour
{
    Actor _adventurer;
    public TextMeshProUGUI Header;
    public Actor Adventurer {
        set
        {
            _adventurer = value;
            Header.text = $"Hire {_adventurer.Stats.Name}?";
        }
    }
    public void Hire()
    {
        GameManager.Instance.Hire(_adventurer);
        gameObject.SetActive(false);
        GUI.Instance.CloseHires();
    }

    public void Reject()
    {
        GameManager.Instance.Reject(_adventurer);
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}
