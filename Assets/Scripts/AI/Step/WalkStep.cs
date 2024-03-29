﻿using Assets.Scripts.Map;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.AI.Step
{
    /// <summary>
    /// The <see cref="WalkStep"/> class is a <see cref="TaskStep"/> for a <see cref="Pawn"/> to move from one <see cref="RoomNode"/> to another nearby <see cref="RoomNode"/>.
    /// </summary>
    public class WalkStep : TaskStep, IDirected
    {
        private readonly int _animationOffset;
        private readonly Vector3Int _end;
        private readonly bool _isFinished;
        private readonly Vector3 _step;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalkStep"/> class. Checks if the previous <see cref="TaskStep"/> of the <see cref="Pawn"/> 
        /// is also a <see cref="WalkStep"/> so that the animations work properly.
        /// </summary>
        /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="Pawn"/>'s destination.</param>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
        /// <param name="step">The previous <see cref="TaskStep"/> to be performing.</param>
        public WalkStep(Vector3Int end, Pawn pawn, TaskStep step) : this(end, pawn)
        {
            if (step is WalkStep walk)
            {
                Period = walk.Period;
                Frame = walk.Frame;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WalkStep"/> class.
        /// </summary>
        /// <param name="end">The <see cref="Map"/> coordinates of the <see cref="Pawn"/>'s destination.</param>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
        public WalkStep(Vector3Int end, Pawn pawn) : base(pawn)
        {
            if(pawn.Occupying != null)
            {
                pawn.Stance = Stance.Stand;
                pawn.Occupying.Exit(pawn, end);
            }

            _end = end;

            Vector2 gameVector = end - pawn.WorldPositionNonDiscrete;
            _step = gameVector.normalized;

            if (pawn.WorldPosition == end)
            {
                _isFinished = true;
                return;
            }

            int best = 0;
            float bestProduct = Vector2.Dot(gameVector, new Vector2(1, 1).normalized);

            for (int i = 1; i < 8; i++)
            {
                var value = i switch
                {
                    1 => Vector2.Dot(gameVector, new Vector2(1, 0).normalized),
                    2 => Vector2.Dot(gameVector, new Vector2(1, -1).normalized),
                    3 => Vector2.Dot(gameVector, new Vector2(0, -1).normalized),
                    4 => Vector2.Dot(gameVector, new Vector2(-1, -1).normalized),
                    5 => Vector2.Dot(gameVector, new Vector2(-1, 0).normalized),
                    6 => Vector2.Dot(gameVector, new Vector2(-1, 1).normalized),
                    _ => Vector2.Dot(gameVector, new Vector2(0, 1).normalized),
                };
                if (value > bestProduct)
                {
                    bestProduct = value;
                    best = i;
                }
            }

            switch (best)
            {
                case 0:
                    _animationOffset = 15;
                    Direction = Direction.NorthEast;
                    break;
                case 1:
                    _animationOffset = 10;
                    Direction = Direction.East;
                    break;
                case 2:
                    _animationOffset = 5;
                    Direction = Direction.SouthEast;
                    break;
                case 3:
                    _animationOffset = 0;
                    Direction = Direction.South;
                    break;
                case 4:
                    _animationOffset = 35;
                    Direction = Direction.SouthWest;
                    break;
                case 5:
                    _animationOffset = 30;
                    Direction = Direction.West;
                    break;
                case 6:
                    _animationOffset = 25;
                    Direction = Direction.NorthWest;
                    break;
                case 7:
                    _animationOffset = 20;
                    Direction = Direction.North;
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WalkStep"/> class by giving a <see cref="Scripts.Map.Direction"/> that will be traveled indefinitely.
        /// </summary>
        /// <param name="direction"><see cref="Scripts.Map.Direction"/> the <see cref="Pawn"/> will walk.</param>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WalkStep"/>.</param>
        /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
        public WalkStep(Direction direction, Pawn pawn, TaskStep step) : base(pawn)
        {
            Direction = direction;

            if (step is WalkStep walk)
            {
                Period = walk.Period;
                Frame = walk.Frame;
            }

            _animationOffset = direction switch
            {
                Direction.NorthEast => 15,
                Direction.East => 10,
                Direction.SouthEast => 5,
                Direction.South => 0,
                Direction.SouthWest => 35,
                Direction.West => 30,
                Direction.NorthWest => 25,
                Direction.North => 20,
                _ => 0
            };

            _step = Utility.Utility.DirectionToVectorNormalized(direction);
        }

        /// <inheritdoc/>
        public Direction Direction { get; }

        /// <inheritdoc/>
        protected override bool Complete
        {
            get
            {
                //If end is not defined, WalkStep continues infinitely and must be changed manually.
                if(_end == default)
                    return false;
                return Vector3.Dot(_end - Pawn.WorldPositionNonDiscrete, _step) < 0 || _isFinished;
            }
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            if (!Complete)
            {
                Pawn.WorldPositionNonDiscrete += Time.deltaTime * Pawn.Speed * _step;
                Period += Time.deltaTime * Pawn.Speed;

                if (Period >= Frame * STEP_TIME)
                {
                    Pawn.SetSprite(_animationOffset + Frame);
                    Frame++;
                    if (Frame >= 4)
                    {
                        Period -= 4 * STEP_TIME;
                        Frame = 0;
                    }
                }
            }
        }
    }
}
