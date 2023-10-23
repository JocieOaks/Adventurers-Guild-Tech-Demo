using Assets.Scripts.AI;
using Assets.Scripts.AI.Actor;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The <see cref="HireAdventurerPopup"/> class is a UI element that gives the option for an <see cref="Actor"/> to be hired.
    /// </summary>
    public class HireAdventurerPopup : MonoBehaviour
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private TextMeshProUGUI _header;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        private Actor _adventurer;

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
        [UsedImplicitly]
        public void Cancel()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hires <see cref="Adventurer"/> and closes the <see cref="HireAdventurerPopup"/>.
        /// </summary>
        [UsedImplicitly]
        public void Hire()
        {
            GameManager.Instance.Hire(_adventurer);
            gameObject.SetActive(false);
            GUI.Instance.CloseHires();
        }

        /// <summary>
        /// Rejects <see cref="Adventurer"/> removing them from the list of potential hires and closes the <see cref="HireAdventurerPopup"/>.
        /// </summary>
        [UsedImplicitly]
        public void Reject()
        {
            GameManager.Instance.Reject(_adventurer);
            gameObject.SetActive(false);
        }
    }
}
