using Assets.Scripts.AI.Actor;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Designates the level of the <see cref="Quest"/> and it's difficulty and reward.
    /// </summary>
    public enum QuestLevel
    {
        Menial,
        Basic,
        Advanced
    }

    /// <summary>
    /// The <see cref="Quest"/> class is for quests that <see cref="Actor"/> adventurer's can go on with varied outcomes.
    /// </summary>
    public class Quest : QuestData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Quest"/> class.
        /// </summary>
        /// <param name="data">The <see cref="QuestData"/> corresponding to the selected new <see cref="Quest"/>.</param>
        /// <param name="adventurer">The <see cref="Actor"/> going on the <see cref="Quest"/>.</param>
        public Quest(QuestData data, Actor adventurer) : base(data)
        {
            Quester = adventurer;
        }

        /// <value>The <see cref="Actor"/> going on the <see cref="Quest"/>.</value>
        public Actor Quester { get; }

        /// <summary>
        /// Called when a <see cref="Quest"/> has finished, to give the results of the <see cref="Quest"/>.
        /// </summary>
        /// <returns>Returns a tuple, containing the text of the quest results, and the gold and prestige earned.</returns>
        public (string, int, int) Results()
        {
            float skillValue = 0;
            for (int i = 0; i < 4; i++)
            {
                skillValue += AbilityScaling[i] * Quester.Stats.Abilities[i];
            }

            skillValue += Quester.Stats.Level;

            int diceRoll = Random.Range(1, 101);

            return (diceRoll + skillValue) switch
            {
                < 20 => (ResultsText[0], 0, -Prestige),
                < 40 => (ResultsText[1], Gold / 2, -Prestige / 2),
                < 60 => (ResultsText[2], Gold, Prestige),
                < 80 => (ResultsText[3], Gold, Prestige * 3 / 2),
                _ => (ResultsText[4], Gold * 3 / 2, Prestige * 2),
            };
        }
    }

    /// <summary>
    /// The <see cref="QuestData"/> class contains the serializable data for creating new <see cref="Quest"/>s.
    /// </summary>
    [System.Serializable]
    public class QuestData
    {
        [JsonProperty] protected QuestLevel LevelValue;
        private static readonly string[] s_levelNames = new string[] { "Menial", "Basic", "Advanced" };

        /// <summary>
        /// The constructor for <see cref="QuestData"/> used to serialize quest data from a json file.
        /// </summary>
        public QuestData()
        {
            Name = default;
            Description = default;
            LevelValue = default;
            Duration = default;
            AbilityScaling = new float[4];
            ResultsText = new string[5];
            Experience = default;
            Gold = default;
            Prestige = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestData"/> class from another <see cref="QuestData"/> instance.
        /// </summary>
        /// <param name="data">The <see cref="QuestData"/> being copied.</param>
        protected QuestData(QuestData data)
        {
            Name = data.Name;
            Description = data.Description;
            LevelValue = data.LevelValue;
            Duration = data.Duration;
            AbilityScaling = data.AbilityScaling;
            ResultsText = data.ResultsText;
            Experience = data.Experience;
            Gold = data.Gold;
            Prestige = data.Prestige;
        }

        /// <value>Determines how the success chance of the <see cref="Quest"/> scales with ability scores.</value>
        [JsonProperty] public float[] AbilityScaling { get; protected set; }

        /// <value>Sets the length of time that is a <see cref="Quest"/> is available for the player.</value>
        [JsonIgnore] public int AvailableDuration { get; } = 500;

        /// <value>Determines at what <see cref="GameManager.Tock"/> value the <see cref="Quest"/> will no longer be available.</value>
        [JsonIgnore] public int AvailableUntil { get; set; } = 0;

        /// <value>Sets how long after a <see cref="Quest"/> has been available before it becomes available again.</value>
        [JsonIgnore] public int Cooldown { get; } = 500;

        /// <value>Determines at what <see cref="GameManager.Tock"/> value the <see cref="Quest"/> will no longer be on cooldown.</value> 
        [JsonIgnore] public int CooldownUntil { get; set; } = 0;

        /// <value>The description of the <see cref="Quest"/>.</value>
        [JsonProperty] public string Description { get; protected set; }

        /// <value>The length of time an adventurer will be gone on the <see cref="Quest"/>.</value>
        [JsonProperty] public int Duration { get; protected set; }

        /// <value>The amount of experience a questing adventurer will earn.</value>
        [JsonProperty] public int Experience { get; protected set; }

        /// <value>The amount of gold earned from going on the <see cref="Quest"/>.</value>
        [JsonProperty] public int Gold { get; protected set; }

        /// <value>Gives the string name of the <see cref="Quest"/>'s level.</value>
        [JsonIgnore] public string Level => s_levelNames[(int)LevelValue];

        /// <value>The name of the <see cref="Quest"/>.</value>
        [JsonProperty] public string Name { get; protected set; }

        /// <value>The amount of prestige earned from going on the <see cref="Quest"/>.</value>
        [JsonProperty] public int Prestige { get; protected set; }

        /// <value>An array of different possible results from going on the <see cref="Quest"/>. The results shown depend on the degree of success.</value>
        [JsonProperty] protected string[] ResultsText { get; set; }
    }
}