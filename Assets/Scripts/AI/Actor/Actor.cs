using System;
using Assets.Scripts.AI.Planning;
using Assets.Scripts.Map.Sprite_Object.Furniture;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ReSharper disable UnusedMember.Global

namespace Assets.Scripts.AI.Actor
{
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
        private bool _isOnQuest;
        private float _needHunger;
        private float _needSleep;
        private float _needSocial;
        private NativeArray<Color> _spritePixels;
        private Sprite[] _spritesList;

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class, and randomizes it's character data.
        /// </summary>
        public Actor(out JobHandle job)
        {
            Race = (Race)Random.Range(0, 4);
            ActorAppearance appearance = new(Race);
            job = Graphics.Instance.BuildSprites(appearance, out _spritePixels);

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
            Appearance = default;

            Hunger = Random.Range(0f, 10f);
            Sleep = Random.Range(0f, 10f);

            GameManager.Instance.NPCs.Add(this);
        }

        /// <value>The characters appearance.</value>
        public ActorAppearance Appearance { get; }

        /// <value>A reference to the character's bed.</value>
        public BedSprite Bed { get; set; }

        /// <value>The character's charisma stat.</value>
        public int Charisma { get; private set; }

        /// <value>The character's class.</value>
        public Class Class { get; }

        /// <value>The character's dexterity stat.</value>
        public int Dexterity { get; private set; }

        /// <value>How much experience the character has accumulated.</value>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public float Experience { get; }

        /// <value>Whether the character currently possesses food (will eventually be replaced with an inventory system).</value>
        public bool HasFood { get; set; }

        /// <value>The character's level of hunger.</value>
        public float Hunger
        {
            get => _needHunger;
            private set => _needHunger = value > 0 ? value : 0;
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
        public AdventurerPawn Pawn { get; private set; }

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
        public ActorProfile Stats => new(this);

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
            Pawn.transform.position = Utility.Utility.MapCoordinatesToSceneCoordinates(position);
            Pawn.name = "Actor";
            Pawn.Actor = this;
        }

        /// <summary>
        /// Creates the list of <see cref="Sprite"/>s associated with this <see cref="Actor"/>. Called once <see cref="Graphics.BuildSpriteJob"/> has completed on a worker thread.
        /// </summary>
        public void MakeSpritesList()
        {
            _spritesList = Graphics.Instance.SliceSprites(_spritePixels.ToArray());
            _spritePixels.Dispose();
        }

        /// <summary>
        /// Called once per frame.
        /// Corresponds to the <see cref="MonoBehaviour"/> Update().
        /// </summary>
        public void Update()
        {
            _needHunger -= Time.deltaTime / 10;

            _needSleep -= Pawn.Stance switch
            {
                Stance.Stand => Time.deltaTime / 30,
                Stance.Sit => Time.deltaTime / 60,
                Stance.Lay => Time.deltaTime / 30,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (Pawn.IsInConversation)
                _needSocial += Time.deltaTime / 2;
            else
                _needSocial -= Time.deltaTime / 5;
        }

        /// <summary>
        /// Generates a stat line of random scores that should usually add up to 30 (on rare occasions it may be slightly higher) and then randomly assigns each score to an ability.
        /// </summary>
        private void RandomizeAbilityScores()
        {
            var total = 0;
            var scores = new int[4];

            scores[0] = Random.Range(1, 21);
            total += scores[0];
            scores[1] = Mathf.Max(Random.Range(1, 13) + (int)((30 - total) / 3f - 6.5), 1);
            total += scores[1];
            scores[2] = Mathf.Max(Random.Range(1, 8) + (int)((30 - total) / 2f - 4.5), 1);
            total += scores[2];
            scores[3] = Mathf.Max(30 - total, 1);

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
}