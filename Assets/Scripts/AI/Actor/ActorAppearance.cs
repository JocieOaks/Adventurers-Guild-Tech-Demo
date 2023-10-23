using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Actor
{
    /// <summary>
    /// The <see cref="ActorAppearance"/> struct contains the appearance data for an <see cref="Actor"/> defining how their sprite should appear.
    /// </summary>
    public readonly struct ActorAppearance
    {
        //Arrays for random selection of an Actor's physical characteristics.
        private static readonly List<int> s_humanSkinTones = new() { 9, 10, 11, 12, 13, 14, 15, 16, 20 };
        private static readonly List<int> s_naturalHairColors = new() { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        private static readonly List<int> s_orcSkinTones = new() { 4, 5, 6, 7, 8, 16 };
        private static readonly List<int> s_tieflingSkinTones = new() { 0, 1, 2, 3, 17, 18, 19, 20 };
        private static readonly List<int> s_unnaturalHairColors = new() { 0, 1, 2 };

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorAppearance"/> struct, randomly initializing the appearance properties based on the predetermined <see cref="Race"/>.
        /// </summary>
        /// <param name="race"></param>
        public ActorAppearance(Race race)
        {
            switch (race)
            {
                case Race.Human:
                    SkinColor = s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                    HairColor = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                    HornsColor = 0;
                    Narrow = Random.Range(0, 2) == 0;
                    Thick = Random.Range(0, 2) == 0;
                    Ears = 2;
                    Tusks = false;
                    HairType = Random.Range(0, 6);
                    BeardType = Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4;
                    Horns = 4;
                    BodyHair = Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0;
                    break;
                case Race.Elf:
                    SkinColor = s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                    HairColor = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                    HornsColor = 0;
                    Narrow = Random.Range(0, 1f) < 0.7f;
                    Thick = Random.Range(0, 1f) < 0.3f;
                    Ears = 1;
                    Tusks = false;
                    HairType = Random.Range(0, 6);
                    BeardType = Random.Range(0, 1f) < 0.5f ? Random.Range(0, 4) : 4;
                    Horns = 4;
                    BodyHair = Random.Range(0f, 1f) < 0.5f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0;
                    break;
                case Race.Orc:
                    SkinColor = s_orcSkinTones[Random.Range(0, s_orcSkinTones.Count)];
                    HairColor = s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)];
                    HornsColor = 0;
                    Narrow = Random.Range(0, 1f) < 0.3f;
                    Thick = Random.Range(0, 1f) < 0.7f;
                    Ears = 0;
                    Tusks = true;
                    HairType = Random.Range(0, 6);
                    BeardType = Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4;
                    Horns = 4;
                    BodyHair = Random.Range(0f, 1f) < 0.8f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0;
                    break;
                case Race.Tiefling:
                case Race.Firbolg:
                default:
                    SkinColor = Random.Range(0, 1f) < 0.7 ? s_tieflingSkinTones[Random.Range(0, s_tieflingSkinTones.Count)] : s_humanSkinTones[Random.Range(0, s_humanSkinTones.Count)];
                    HairColor = Random.Range(0, 1f) < 0.4 ? s_naturalHairColors[Random.Range(0, s_naturalHairColors.Count)] : s_unnaturalHairColors[Random.Range(0, s_unnaturalHairColors.Count)];
                    HornsColor = Random.Range(0, 14);
                    Narrow = Random.Range(0, 2) == 0;
                    Thick = Random.Range(0, 2) == 0;
                    Ears = Random.Range(0, 1f) < 0.6f ? 1 : Random.Range(0, 1f) < 0.75 ? 0 : 2;
                    Tusks = Random.Range(0, 1f) < 0.1f;
                    HairType = Random.Range(0, 6);
                    BeardType = Random.Range(0, 1f) < 0.7f ? Random.Range(0, 4) : 4;
                    Horns = Random.Range(0, 4);
                    BodyHair = Random.Range(0f, 1f) < 0.75f ? Random.Range(0, 1f) < 0.6f ? 1 : 2 : 0;
                    break;
            }
        }

        /// <value>The index of the <see cref="Actor"/>'s beard sprite.</value>
        public int BeardType { get; }
    
        /// <value>
        /// Determines whether the <see cref="Actor"/> has body hair, and how much. 
        /// 0 - No body hair.
        /// 1 - Full body hair.
        /// 2 - Only chest hair.
        /// </value>
        public int BodyHair { get; }

        /// <value>
        /// Determines the length of the <see cref="Actor"/>'s ears.
        /// 0 - Short pointed
        /// 1 - Long point
        /// 2 - Rounded
        /// </value>
        public int Ears { get; }

        /// <value>Determines the <see cref="Actor"/>'s hair color.</value>
        public int HairColor { get; }

        /// <value>The index of the <see cref="Actor"/>'s hair sprite.</value>
        public int HairType { get; }

        /// <value>The index of the <see cref="Actor"/>'s horn sprite. 4 for no horns.</value>
        public int Horns { get; }

        /// <value>Determines the <see cref="Actor"/>'s horn color (if they have horns).</value>
        public int HornsColor { get; }

        /// <value>Determines if the <see cref="Actor"/> has a narrower face sprite.</value>
        public bool Narrow { get; }

        /// <value>Determines if the <see cref="Actor"/> has orc tusks.</value>
        public bool Tusks { get; }

        /// <value>Determines the <see cref="Actor"/>'s skin color.</value>
        public int SkinColor { get; }

        /// <value>Determines if the <see cref="Actor"/> has the thicker body sprite, or the more muscular body sprite.</value>
        public bool Thick { get; }
    }
}
