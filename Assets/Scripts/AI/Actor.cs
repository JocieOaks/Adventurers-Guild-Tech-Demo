using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The character classes an <see cref="Actor"/> can have.
/// </summary>
public enum Class
{
    Mage,
    Fighter,
    Rogue,
    Bard,
    Monk
}

/// <summary>
/// The different needs of an <see cref="Actor"/>.
/// </summary>
public enum Needs
{
    /// <summary>The <see cref="Actor"/> needs food.</summary>
    Hunger,
    /// <summary>The <see cref="Actor"/> needs sleep.</summary>
    Sleep,
    /// <summary>The <see cref="Actor"/> needs to socialize with other <see cref="Pawn"/>s.</summary>
    Social
}

/// <summary>
/// The various races for an <see cref="Actor"/>.
/// </summary>
public enum Race
{
    Human,
    Elf,
    Orc,
    Tiefling,
    Firbolg
}

/// <summary>
/// The <see cref="Actor"/> class is the counterpart to the <see cref="Pawn"/> class that controls the characteristic data of an NPC, 
/// //including it's current status, and character information.
/// An <see cref="Actor"/> object can exist independently of a Pawn, and is usually constructed before it's corresponding Pawn is constructed.
/// </summary>
public class Actor
{
    //Arrays for random selection of an Actor's physical characteristics.
    static readonly List<int> s_humanSkinTones = new() {9, 10, 11, 12, 13, 14, 15, 16, 20};
    static readonly List<int> s_naturalHairColors = new() { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
    static readonly List<int> s_orcSkinTones = new() { 4, 5, 6, 7, 8, 16 };
    static readonly List<int> s_tieflingSkinTones = new() { 0, 1, 2, 3, 17, 18, 19, 20 };
    static readonly List<int> s_unnaturalHairColors = new() { 0, 1, 2 };

    bool _isOnQuest = false;
    float _needHunger;
    float _needSleep;
    float _needSocial;
    readonly Sprite[] _spritesList;

    /// <summary>
    /// Intializes a new instance of the <see cref="Actor"/> class, and randomizes it's character data.
    /// </summary>
    public Actor()
    {
        Race = (Race)Random.Range(0, 4);
        int skin;
        int hair;
        switch (Race)
        {
            case Race.Human:
                skin = s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                hair = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                _spritesList = Graphics.Instance.BuildSprites(skin, hair, 0, Random.Range(0, 2) == 0, Random.Range(0, 2) == 0, 2, false, Random.Range(0, 6), Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4, 4, Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
                break;
            case Race.Elf:
                skin = s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                hair = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                _spritesList = Graphics.Instance.BuildSprites(skin, hair, 0, Random.Range(0, 1f) < 0.7f, Random.Range(0, 1f) < 0.3f, 1, false, Random.Range(0, 6), Random.Range(0, 1f) < 0.5f ? Random.Range(0, 4) : 4, 4, Random.Range(0f, 1f) < 0.5f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
                break;
            case Race.Orc:
                skin = s_orcSkinTones[Random.Range(0, s_orcSkinTones.Count)];
                hair = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                _spritesList = Graphics.Instance.BuildSprites(skin, hair, 0, Random.Range(0, 1f) < 0.3f, Random.Range(0, 1f) < 0.7f, 1, true, Random.Range(0, 6), Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4, 4, Random.Range(0f, 1f) < 0.8f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
                break;
            case Race.Tiefling:
                skin = Random.Range(0, 1f) < 0.7 ? s_tieflingSkinTones[Random.Range(0, s_tieflingSkinTones.Count)] : s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                hair = Random.Range(0, 1f) < 0.4 ? s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)] : s_unnaturalHairColors[Random.Range(0, s_unnaturalHairColors.Count)];
                int ears = Random.Range(0, 1f) < 0.6f ? 1 : Random.Range(0, 1f) < 0.75 ? 0 : 2;
                _spritesList = Graphics.Instance.BuildSprites(skin, hair, Random.Range(0, 14), Random.Range(0, 2) == 0, Random.Range(0, 2) == 0, ears, Random.Range(0, 1f) < 0.1f, Random.Range(0, 6), Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4, Random.Range(0, 4), Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
                break;
        }

        Name = GameManager.Instance.Names[Random.Range(0, GameManager.Instance.Names.Count)];

        RandomizeAbilityScores();

        if (Strength > Dexterity && Strength > Charisma && Strength > Intelligence)
        {
            Class = Class.Fighter;
        }
        else if (Dexterity > Charisma && Dexterity > Intelligence)
        {
            Class = Class.Rogue;
        }
        else if (Charisma > Intelligence)
        {
            Class = Class.Bard;
        }
        else
        {
            Class = Class.Mage;
        }

        Hunger = Random.Range(0f, 10f);
        Sleep = Random.Range(0f, 10f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Actor"/> class based off of an <see cref="NPC"/> where the character stats have already been predetermined.
    /// </summary>
    /// <param name="npc"></param>
    public Actor(NPC npc)
    {
        Pawn = npc;
        Class = npc.CharacterClass;
        Race = npc.Race;
        Name = npc.CharacterName;
        Strength = npc.Strength;
        Dexterity = npc.Dexterity;
        Charisma = npc.Charisma;
        Intelligence = npc.Intelligence;

        Hunger = Random.Range(0f, 10f);
        Sleep = Random.Range(0f, 10f);

        GameManager.Instance.NPCs.Add(this);
    }

    /// <value>A reference to the character's bed.</value>
    public BedSprite Bed { get; set; }

    /// <value>The character's charisma stat.</value>
    public int Charisma { get; private set; }

    /// <value>The character's class.</value>
    public Class Class { get; }

    /// <value>The character's dexterity stat.</value>
    public int Dexterity { get; private set; }

    /// <value>How much experience the character has accumulated.</value>
    public float Experience { get; }

    /// <value>Whether the character currently possesses food (will eventually be replaced with an inventory system).</value>
    public bool HasFood { get; set; }

    /// <value>The character's level of hunger.</value>
    public float Hunger
    {
        get => _needHunger;
        private set
        {
            _needHunger = value > 0 ? value : 0;
        }

    }

    /// <value>The character's intelligence stat.</value>
    public int Intelligence { get; private set; }

    /// <value>Returns if the <see cref="Actor"/> is currently performing a <see cref="Quest"/>, and is thus unavailable.</value>
    public bool IsOnQuest
    {
        get => _isOnQuest;
        set
        {
            _isOnQuest = value;
            if (value)
            {
                Pawn.BeginQuest();
            }
            else
            {
                Pawn.gameObject.SetActive(true);
                Pawn.Social.Silenced = false;
            }
        }

    }

    /// <value>The character's level.</value>
    public int Level { get; } = 1;

    /// <value>The character's name.</value>
    public string Name { get; }

    /// <value>A reference to the <see cref="Pawn"/> that corresponds to the <see cref="Actor"/>. Can be null.</value>
    public Pawn Pawn { get; private set; }

    /// <value>The sprite used for the character when displayed in the UI.</value>
    public Sprite ProfileSprite => _spritesList[30];

    /// <value>The character's race.</value>
    public Race Race { get; }

    /// <value>The character's level of tiredness and energy.</value>
    public float Sleep
    {
        get => _needSleep;
        private set
        {
            _needSleep = value > 0 ? value : 0;
            _needSleep = _needSleep < 10 ? _needSleep : 10;
        }
    }

    /// <value>The character's level of socialization and loneliness.</value>
    public float Social
    {
        get => _needSocial;
        private set
        {
            _needSocial = value > 0 ? value : 0;
            _needSocial = _needSocial < 10 ? _needSocial : 10;
        }
    }

    /// <value>Creates an <see cref="ActorProfile"/> that holds a copy of the <see cref="Actor"/>'s character data.</value>
    public ActorProfile Stats
    {
        get
        {
            return new ActorProfile(this);
        }
    }

    /// <value>The character's strength stat.</value>
    public int Strength { get; private set; }

    /// <summary>
    /// Modifies the <see cref="Actor"/>'s need levels.
    /// Can only be adjusted additively, cannot be assigned to directly.
    /// </summary>
    /// <param name="need">The need that is being modified.</param>
    /// <param name="mod">The amount by which the need should be modified, through addition.</param>
    public void ChangeNeeds(Needs need, float mod)
    {
        switch (need)
        {
            case Needs.Hunger:
                Hunger += mod;
                break;
            case Needs.Sleep:
                Sleep += mod;
                break;
            case Needs.Social:
                Social += mod;
                break;
        }
    }

    /// <summary>
    /// Initializes a new <see cref="Pawn"/> game object to go with the <see cref="Actor"/>
    /// Will simply return without creating a new <see cref="Pawn"/> if the <see cref="Actor"/> already has a corresponding <see cref="Pawn"/>
    /// </summary>
    /// <param name="position">The position at which the Pawn will be placed, in scene coordinates.</param>
    public void InitializePawn(Vector3Int position)
    {
        if (Pawn != null)
            return;

        Pawn = Object.Instantiate(Graphics.Instance.PawnPrefab);
        Pawn.Sprites = _spritesList;
        Pawn.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
        Pawn.name = "Actor";
        Pawn.Actor = this;
    }

    /// <summary>
    /// Called once per frame.
    /// Corresponds to the Monobheaviour Update()
    /// </summary>
    public void Update()
    {
        _needHunger -= Time.deltaTime / 10;

        switch (Pawn.Stance)
        {
            case Stance.Stand:
                _needSleep -= Time.deltaTime / 30;
                break;
            case Stance.Sit:
                _needSleep -= Time.deltaTime / 60;
                break;
            case Stance.Lay:
                _needSleep -= Time.deltaTime / 30;
                break;
        }
        if (Pawn.IsInConversation)
            _needSocial += Time.deltaTime / 2;
        else
            _needSocial -= Time.deltaTime / 5;
    }

    /// <summary>
    /// Generates a statline of random scores that should usually add up to 30 (on rare occasions it may be slightly higher) and then randomly assigns each score to an ability.
    /// </summary>
    void RandomizeAbilityScores()
    {
        int total = 0;
        int[] scores = new int[4];

        scores[0] = Random.Range(1, 21);
        total += scores[0];
        scores[1] = Mathf.Max(Random.Range(1, 13) + (int)((30 - total) / 3f - 6.5), 1);
        total += scores[1];
        scores[2] = Mathf.Max(Random.Range(1, 8) + (int)((30 - total) / 2f - 4.5), 1);
        total += scores[2];
        scores[3] = Mathf.Max(30 - total, 1);

        System.Random rng = new();

        int n = scores.Length;
        while (n > 1)
        {
            int k = Random.Range(0, --n);
            (scores[k], scores[n]) = (scores[n], scores[k]);
        }

        Strength = scores[0];
        Dexterity = scores[1];
        Charisma = scores[2];
        Intelligence = scores[3];
    }
}

