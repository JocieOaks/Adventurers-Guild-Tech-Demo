using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map.Sprite_Object.Furniture;
using UnityEngine;

namespace Assets.Scripts.AI.Step
{
    /// <summary>
    /// The <see cref="SitStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to lay down.
    /// </summary>
    public class LayStep : TaskStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayStep"/> class.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> that is laying down.</param>
        /// <param name="bed">The <see cref="BedSprite"/> the <see cref="Pawn"/> is laying in.</param>
        public LayStep(Pawn pawn, BedSprite bed) : base(pawn)
        {
            pawn.Stance = Stance.Lay;
            bed.Enter(pawn);
        }

        /// <inheritdoc/>
        protected override bool Complete => true;

        /// <inheritdoc/>
        public override void Perform()
        {
            Period += Time.deltaTime;

            if (Period >= Frame * BREATH_TIME)
            {
                Pawn.SetSprite(34);//24 + _idleFrames[_frame]);
                Frame++;
                if (Frame == 22)
                {
                    Period -= 2.75f;
                    Frame = 0;
                }
            }
        }
    }
}
