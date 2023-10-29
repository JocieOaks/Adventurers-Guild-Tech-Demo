using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The <see cref="StartupUI"/> class controls the UI elements for the starting portion of the game.
    /// </summary>
    public class StartupUI : MonoBehaviour
    {
        [SerializeField][UsedImplicitly] private GameObject _welcome, _class, _party, _name;

        /// <summary>
        /// Called when the welcome message is clicked.
        /// </summary>
        [UsedImplicitly]
        public void WelcomeClicked()
        {
            _welcome.SetActive(false);
            _class.SetActive(true);
        }

        /// <summary>
        /// Called once a class has been selected.
        /// </summary>
        [UsedImplicitly]
        public void ClassSelected()
        {
            _class.SetActive(false);
            _party.SetActive(true);
        }

        /// <summary>
        /// Called once a party member has been selected.
        /// </summary>
        [UsedImplicitly]
        public void PartyChosen()
        {
            _party.SetActive(false);
            _name.SetActive(true);
        }

        /// <summary>
        /// Called once the player's character has been named.
        /// </summary>
        [UsedImplicitly]
        public void Named()
        {
            SceneManager.LoadScene("Map");
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
    }
}
