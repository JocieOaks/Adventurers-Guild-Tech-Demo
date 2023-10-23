using System;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Actor;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Designates what happens when the <see cref="AdventurerProfileUI"/> is selected.
    /// </summary>
    public enum AdventurerProfileMode
    {
        AdventurerSelect,
        Hire,
        HireTutorial
    }

    /// <summary>
    /// The <see cref="AdventurerProfileUI"/> class is a UI element that displays the details of a specified <see cref="Actor"/>.
    /// </summary>
    public class AdventurerProfileUI : MonoBehaviour
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private Image _charismaBar;
        [SerializeField] private TextMeshProUGUI _class;
        [SerializeField] private Image _dexterityBar;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _intelligenceBar;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _strengthBar;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        private Actor _adventurer;

        private AdventurerProfileMode _mode;

        /// <summary>
        /// Called when the <see cref="AdventurerProfileUI"/> is clicked on.
        /// </summary>
        [UsedImplicitly]
        public void Select()
        {
            switch (_mode)
            {
                case AdventurerProfileMode.AdventurerSelect:
                    GUI.Instance.AdventurerSelected(_adventurer);
                    break;
                case AdventurerProfileMode.Hire:
                    GUI.Instance.HireAdventurer(_adventurer);
                    break;
                case AdventurerProfileMode.HireTutorial:
                    TutorialUI.Instance.Hire(_adventurer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets the <see cref="AdventurerProfileUI"/> to display the stats of the specified <see cref="Actor"/>.
        /// </summary>
        /// <param name="adventurer">The <see cref="Actor"/> whose profile is being displayed.</param>
        /// <param name="mode">The context in which the <see cref="AdventurerProfileUI"/> is being displayed.</param>
        public void SetPanel(Actor adventurer, AdventurerProfileMode mode)
        {
            _adventurer = adventurer;
            _mode = mode;
            _name.text = adventurer.Stats.Name;
            _class.text = $"{adventurer.Race.ToString()} {adventurer.Class.ToString()} {adventurer.Level}";
            _icon.sprite = adventurer.ProfileSprite;
            int[] abilityScores = adventurer.Stats.Abilities;
            _strengthBar.rectTransform.sizeDelta = new Vector2(abilityScores[0] * 2.5f, 10);
            _dexterityBar.rectTransform.sizeDelta = new Vector2(abilityScores[1] * 2.5f, 10);
            _charismaBar.rectTransform.sizeDelta = new Vector2(abilityScores[2] * 2.5f, 10);
            _intelligenceBar.rectTransform.sizeDelta = new Vector2(abilityScores[3] * 2.5f, 10);
        }
    }
}