using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.AI.Step
{
    /// <summary>
    /// The <see cref="WaitStep"/> class is a <see cref="TaskStep"/> for when a <see cref="Pawn"/> is waiting.
    /// </summary>
    public class WaitStep : TaskStep, IDirected
    {
        private int _animationIndex = 30;
        private readonly RoomNode _roomNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitStep"/> class. Checks the direction of the previous <see cref="TaskStep"/> if it is <see cref="IDirected"/> 
        /// to determine the <see cref="Scripts.Map.Direction"/> for the <see cref="Pawn"/> to face.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> that is waiting.</param>
        /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
        /// <param name="blocking">Determines if the <see cref="Pawn"/> blocks the <see cref="RoomNode"/> from being traversed by other <see cref="Pawn"/>s.</param>
        public WaitStep(Pawn pawn, TaskStep step, bool blocking) : this(pawn, step is IDirected directed ? directed.Direction : Direction.South, blocking) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitStep"/> class.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> that is waiting.</param>
        /// <param name="direction">The <see cref="Scripts.Map.Direction"/> the <see cref="Pawn"/> should be facing.</param>
        /// <param name="blocking">Determines if the <see cref="Pawn"/> blocks the <see cref="RoomNode"/> from being traversed by other <see cref="Pawn"/>s.</param>
        public WaitStep(Pawn pawn, Direction direction, bool blocking) : base(pawn)
        {
            _roomNode = pawn.CurrentNode;
            if (blocking)
                _roomNode.Occupant = pawn;
            SetDirection(direction);
        }

        /// <inheritdoc/>
        public Direction Direction { get; private set; } = Direction.South;

        /// <inheritdoc/>
        protected override bool Complete => true;

        /// <inheritdoc/>
        public override void Perform()
        {
            Period += Time.deltaTime;

            if (Direction == Direction.West || Direction == Direction.South)
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(_animationIndex);// + _idleFrames[_frame]);
                    Frame++;
                    if (Frame >= 22)
                    {
                        Period -= 2.75f;
                        Frame = 0;
                    }
                }
            }
            else
            {
                if (Period >= Frame * BREATH_TIME)
                {
                    Pawn.SetSprite(_animationIndex);
                    Frame += 100;
                }
            }
        }

        /// <summary>
        /// Set the <see cref="Scripts.Map.Direction"/> for the <see cref="Pawn"/> to face while waiting.
        /// </summary>
        /// <param name="direction">The <see cref="Scripts.Map.Direction"/> for the <see cref="Pawn"/> to face.</param>
        public void SetDirection(Direction direction)
        {
            Direction = direction;

            _animationIndex = direction switch
            {
                Direction.North => 24,
                Direction.NorthEast => 19,
                Direction.East => 14,
                Direction.SouthEast => 9,
                Direction.South => 4,
                Direction.SouthWest => 39,
                Direction.West => 34,
                _ => 29
            };
            Period = Mathf.Clamp(Period, 0, 2.75f);
        }

        /// <inheritdoc/>
        protected override void Finish()
        {
            if(Equals(_roomNode.Occupant, Pawn))
                _roomNode.Occupant = null;
        }
    }
}
