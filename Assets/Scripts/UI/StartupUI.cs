using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The <see cref="StartupUI"/> class controls the UI elements for the starting portion of the game.
/// </summary>
public class StartupUI : MonoBehaviour
{
     [SerializeField] GameObject _welcome, _class, _party, _name;

    /// <summary>
    /// Called when the welcome message is clicked.
    /// </summary>
    public void WelcomeClicked()
    {
        _welcome.SetActive(false);
        _class.SetActive(true);
    }

    /// <summary>
    /// Called once a class has been selected.
    /// </summary>
    public void ClassSelected()
    {
        _class.SetActive(false);
        _party.SetActive(true);
    }

    /// <summary>
    /// Called once a party member has been selected.
    /// </summary>
    public void PartyChosen()
    {
        _party.SetActive(false);
        _name.SetActive(true);
    }

    /// <summary>
    /// Called once the player's character has been named.
    /// </summary>
    public void Named()
    {
        SceneManager.LoadScene("Map");
        SceneManager.LoadScene("UI", LoadSceneMode.Additive);
    }
}
