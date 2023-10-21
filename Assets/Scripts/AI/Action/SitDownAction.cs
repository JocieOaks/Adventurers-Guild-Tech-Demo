using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Sprite_Object;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="SitDownAction"/> class is a <see cref="TaskAction"/> for sitting down.
    /// </summary>
    public class SitDownAction : TaskAction
    {
        private const float WAIT_TIME = 0.5f;
        private float _period;
        private readonly IOccupied _seat;

        /// <summary>
        /// Initializes a new instance of the <see cref="SitDownAction"/> class.
        /// </summary>
        /// <param name="seat">The seat the <see cref="Actor"/> is sitting in.</param>
        /// <param name="pawn">The <see cref="Pawn"/> that is sitting.</param>
        public SitDownAction(IOccupied seat, Pawn pawn) : base(pawn)
        {
            _seat = seat;
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <inheritdoc/>
        public override int Complete()
        {
            if (_seat.Occupied && _seat.Occupant != Pawn)
                return -1;
            return _period > WAIT_TIME ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Pawn.CurrentStep = new SitStep(Pawn, _seat);
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            _period += Time.deltaTime;
        }
    }
}
