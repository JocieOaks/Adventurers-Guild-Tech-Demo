using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// The <see cref="HireAdventurerPopup"/> class is a UI element that gives the option for an <see cref="Actor"/> to be hired.
/// </summary>
public class HireAdventurerPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _header;
    Actor _adventurer;

    /// <value>The <see cref="Actor"/> to be hired or rejected.</value>
    public Actor Adventurer {
        set
        {
            _adventurer = value;
            _header.text = $"Hire {_adventurer.Stats.Name}?";
        }
    }
    /// <summary>
    /// Closes the <see cref="HireAdventurerPopup"/> without hiring or rejecting <see cref="Adventurer"/>.
    /// </summary>
    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hires <see cref="Adventurer"/> and closes the <see cref="HireAdventurerPopup"/>.
    /// </summary>
    public void Hire()
    {
        GameManager.Instance.Hire(_adventurer);
        gameObject.SetActive(false);
        GUI.Instance.CloseHires();
    }

    /// <summary>
    /// Rejects <see cref="Adventurer"/> removing them from the list of potential hires and closes the <see cref="HireAdventurerPopup"/>.
    /// </summary>
    public void Reject()
    {
        GameManager.Instance.Reject(_adventurer);
        gameObject.SetActive(false);
    }
}
