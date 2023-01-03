using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupUI : MonoBehaviour
{
    public GameObject Welcome, Class, Party, Name;

    public void WelcomeClicked()
    {
        Welcome.SetActive(false);
        Class.SetActive(true);
    }

    public void ClassSelected()
    {
        Class.SetActive(false);
        Party.SetActive(true);
    }

    public void PartyChosen()
    {
        Party.SetActive(false);
        Name.SetActive(true);
    }

    public void Named()
    {
        SceneManager.LoadScene("Map");
        SceneManager.LoadScene("UI", LoadSceneMode.Additive);
    }
}
