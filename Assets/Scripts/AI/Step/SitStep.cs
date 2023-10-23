using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.Map.Sprite_Object.Furniture;
using UnityEngine;

namespace Assets.Scripts.AI.Step
{
    /// <summary>
    /// The <see cref="SitStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to sit down.
    /// </summary>
    public class SitStep : TaskStep, IDirected
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SitStep"/> task.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> that is sitting down.</param>
        /// <param name="seat">The seat on which the <see cref="Pawn"/> is sitting.</param>
        public SitStep(Pawn pawn, IOccupied seat) : base(pawn)
        {
            seat.Enter(pawn);

            if (seat is ChairSprite chair)
            {
                Direction = chair.Direction;
            }
            else if (seat is StoolSprite stool)
            {
                if (Map.Map.Instance[stool.WorldPosition + Utility.Utility.DirectionToVector(Direction.North) * 2].Occupant is BarSprite)
                {
                    Direction = Direction.North;
                }
                else if (Map.Map.Instance[stool.WorldPosition + Utility.Utility.DirectionToVector(Direction.South) * 2].Occupant is BarSprite)
                {
                    Direction = Direction.South;
                }
                else if (Map.Map.Instance[stool.WorldPosition + Utility.Utility.DirectionToVector(Direction.East) * 2].Occupant is BarSprite)
                {
                    Direction = Direction.East;
                }
                else if (Map.Map.Instance[stool.WorldPosition + Utility.Utility.DirectionToVector(Direction.West) * 2].Occupant is BarSprite)
                {
                    Direction = Direction.West;
                }
            }
            else
                Direction = Direction.North;

            pawn.Stance = Stance.Sit;
        }

        /// <inheritdoc/>
        public Direction Direction { get; }

        /// <inheritdoc/>
        protected override bool Complete => true;

        /// <inheritdoc/>
        public override void Perform()
        {
            Period += Time.deltaTime;
            if (Direction == Direction.West)
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(34); // 24 + _idleFrames[_frame]);
                    Frame++;
                    if (Frame == 22)
                    {
                        Period -= 2.75f;
                        Frame = 0;
                    }
                }
            }
            else if (Direction == Direction.South)
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(4); // 30 + _idleFrames[_frame]);
                    Frame++;
                    if (Frame == 22)
                    {
                        Period -= 2.75f;
                        Frame = 0;
                    }
                }
            }
            else if (Direction == Direction.East)
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(14);// 47);
                    Frame += 100;
                }
            }
            else
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(24);// 46);
                    Frame += 100;
                }
            }
        }
    }
}
