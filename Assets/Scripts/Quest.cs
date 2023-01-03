using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum QuestLevel
{
    Menial,
    Basic,
    Advanced
}

[System.Serializable]
public class QuestData
{
    static string[] s_levelNames = new string[] { "Menial", "Basic", "Advanced" };

    [JsonProperty] public string Name { get; protected set; }
    [JsonProperty] public string Description { get; protected set; }
    [JsonProperty] protected QuestLevel _level;
    [JsonIgnore] public string Level => s_levelNames[(int)_level];
    [JsonProperty] public int Duration { get; protected set; }
    [JsonProperty] public float[] AbilityScaling { get; protected set; }
    [JsonProperty] protected string[] _results { get; set; }
    [JsonProperty] public int Experience { get; protected set; }
    [JsonProperty] public int Gold { get; protected set; }
    [JsonProperty] public int Prestige { get; protected set; }

    [JsonIgnore] public int Cooldown { get; } = 500;
    [JsonIgnore] public int CooldownUntil { get; set; } = 0;

    [JsonIgnore] public int AvailableDuration { get; } = 500;
    [JsonIgnore] public int AvailableUntil { get; set; } = 0;


    protected QuestData(QuestData data)
    {
        Name = data.Name;
        Description = data.Description;
        _level = data._level;
        Duration = data.Duration;
        AbilityScaling = data.AbilityScaling;
        _results = data._results;
        Experience = data.Experience;
        Gold = data.Gold;
        Prestige = data.Prestige;
    }

    public QuestData()
    {
        Name = default;
        Description = default;
        _level = default;
        Duration = default;
        AbilityScaling = new float[4];
        _results = new string[5];
        Experience = default;
        Gold = default;
        Prestige = default;
    }
}

public class Quest : QuestData
{
    public Actor Quester { get;}

    public Quest(QuestData data, Actor adventurer) : base(data)
    {
        Quester = adventurer;
    }

    public (string, int, int) Results()
    {
        float skillValue = 0;
        for (int i = 0; i < 4; i++)
        {
            skillValue += AbilityScaling[i] * Quester.Stats.Abilities[i];
        }

        skillValue += Quester.Stats.Level;

        int diceroll = Random.Range(1, 101);

        switch(diceroll + skillValue)
        {
            case < 20:
                return (_results[0], 0, - Prestige);
            case < 40:
                return (_results[1], Gold / 2, - Prestige / 2);
            case < 60:
                return (_results[2], Gold, Prestige);
            case < 80:
                return (_results[3], Gold, Prestige * 3 / 2);
            default:
                return (_results[4], Gold * 3 / 2, Prestige * 2);
        }
    }
}
