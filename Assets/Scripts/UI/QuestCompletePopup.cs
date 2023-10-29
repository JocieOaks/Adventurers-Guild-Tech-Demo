using Assets.Scripts.AI.Actor;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The <see cref="QuestCompletePopup"/> is a UI element that appears whenever a <see cref="Actor"/> has completed a <see cref="Quest"/>.
    /// </summary>
    public class QuestCompletePopup : MonoBehaviour
    {
        [SerializeField][UsedImplicitly] private Image _icon;
        [SerializeField][UsedImplicitly] private TextMeshProUGUI _results;
        [SerializeField][UsedImplicitly] private TextMeshProUGUI _gold;
        [SerializeField][UsedImplicitly] private TextMeshProUGUI _prestige;

        /// <summary>
        /// Sets the <see cref="QuestCompletePopup"/> to display the results of the given <see cref="Quest"/>.
        /// </summary>
        /// <param name="quest">The <see cref="Quest"/> that has completed.</param>
        public void QuestComplete(Quest quest)
        {
            _icon.sprite = quest.Quester.ProfileSprite;
            (string text, int gold, int prestige) = quest.Results();
            _results.text = string.Format(text, quest.Quester.Stats.Name, quest.Quester.Stats.Class.ToString());
            _gold.text = $"Gold Earned: {gold}";
            _prestige.text = $"Prestige Earned: {prestige}";
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when the <see cref="QuestCompletePopup"/> is clicked on, closing it.
        /// </summary>
        [UsedImplicitly]
        public void OnClick()
        {
            gameObject.SetActive(false);
        }
    }
}
