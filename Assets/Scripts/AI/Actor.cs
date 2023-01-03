using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Class
{
    Mage,
    Fighter,
    Rogue,
    Bard,
    Monk
}

public enum Race
{
    Human,
    Elf,
    Orc,
    Tiefling,
    Firbolg
}

public enum Needs
{
    Hunger,
    Sleep,
    Social
}

public class Actor
{
    static List<int> s_humanSkinTones = new List<int>() {9, 10, 11, 12, 13, 14, 15, 16, 20};
    static List<int> s_orcSkinTones = new List<int>() { 4, 5, 6, 7, 8, 16 };
    static List<int> s_tieflingSkinTones = new List<int>() { 0, 1, 2, 3, 17, 18, 19, 20 };
    static List<int> s_naturalHairColors = new List<int>() { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
    static List<int> s_unnaturalHairColors = new List<int>() { 0, 1, 2 };

    public Pawn Pawn { get; private set; }

    bool _isOnQuest = false;

    public bool IsOnQuest { 
        get => _isOnQuest;
        set
        {
            _isOnQuest = value;
            if(value)
            {
                Pawn.Quest();
            }
            else
            {
                Pawn.gameObject.SetActive(true);
                Pawn.Social.Silenced = false;
            }
        }
             
    }

    Sprite[] _spritesList;
    public Sprite ProfileSprite => _spritesList[30];
    public Class Class { get; }

    public Race Race { get; }
    public string Name { get; }
    public int Strength { get; private set; }
    public int Dexterity { get; private set; }
    public int Charisma { get; private set; }
    public int Intelligence { get; private set; }

    float _hunger, _sleep, _social;
    public float Hunger
    {
        get => _hunger;
        private set
        {
            _hunger = value > 0 ? value : 0;
        }

    }
    public float Sleep
    {
        get => _sleep;
        private set
        {
            _sleep = value > 0 ? value : 0;
            _sleep = _sleep < 10 ? _sleep : 10;
        }
    }
    public float Social
    {
        get => _social;
        private set
        {
            _social = value > 0 ? value : 0;
            _social = _social < 10 ? _social : 10;
        }
    }
    public bool HasFood { get; set; }

    public Bed Bed { get; set; }

    public float Experience { get; }
    public int Level { get; } = 1;

    public void ChangeNeeds(Needs need, float mod)
    {
        switch(need)
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

    public void InitializePawn(Vector3 position)
    {
        if (Pawn != null)
            return;

        Pawn = Object.Instantiate(Graphics.Instance.SpriteObject).AddComponent<Pawn>();
        Pawn.Sprites = _spritesList;
        Pawn.transform.position = position;
        Pawn.name = "Actor";
        Pawn.Actor = this;
    }

    public Actor()
    {
        Race = (Race)Random.Range(0, 4);
        int skin;
        int hair;
        switch(Race)
        {
            case Race.Human:
                skin = s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                hair = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                _spritesList = Graphics.Instance.BuildSprites(skin,hair,0,Random.Range(0,2) == 0, Random.Range(0,2) == 0, 2, false, Random.Range(0, 6), Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4, 4, Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
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
                _spritesList = Graphics.Instance.BuildSprites(skin, hair, Random.Range(0,14), Random.Range(0, 2) == 0, Random.Range(0, 2) == 0, ears, Random.Range(0,1f) < 0.1f, Random.Range(0, 6), Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4, Random.Range(0,4), Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0);
                break;
        }

        Name = GameManager.Instance.Names[Random.Range(0, GameManager.Instance.Names.Count)];

        int total = 0;
        int[] scores = new int[4];

        scores[0] = Random.Range(1, 21);
        total += scores[0];
        scores[1] = Mathf.Max(Random.Range(1, 13) + (int)((30 - total) / 3f - 6.5), 1);
        total += scores[1];
        scores[2] = Mathf.Max(Random.Range(1, 8) + (int)((30 - total) / 2f - 4.5), 1);
        total += scores[2];
        scores[3] = 30 - total;

        System.Random rng = new System.Random();

        int n = scores.Length;
        while (n > 1)
        {
            int k = Random.Range(0, --n);
            int temp = scores[n];
            scores[n] = scores[k];
            scores[k] = temp;
        }
        Strength = scores[0];
        Dexterity = scores[1];
        Charisma = scores[2];
        Intelligence = scores[3];

        if (Strength > Dexterity && Strength > Charisma && Strength > Intelligence)
        {
            Class = Class.Fighter;
        }
        else if(Dexterity > Charisma && Dexterity > Intelligence)
        {
            Class = Class.Rogue;
        }
        else if(Charisma > Intelligence)
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

    public Actor (Pawn pawn, Class characterClass, Race race, string name, int strength, int dexterity, int charisma, int intelligence)
    {
        Pawn = pawn;
        Class = characterClass;
        Race = race;
        Name = name;
        Strength = strength;
        Dexterity = dexterity;
        Charisma = charisma;
        Intelligence = intelligence;

        Hunger = 0;// Random.Range(0f, 10f);
        Sleep = 0;// Random.Range(0f, 10f);

        GameManager.Instance.NPCs.Add(this);
    }


    public ActorProfile Stats
    {
        get
        {
            return new ActorProfile(this);
        }
    }
}

