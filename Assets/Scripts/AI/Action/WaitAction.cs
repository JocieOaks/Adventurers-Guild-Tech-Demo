using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Node;
using UnityEngine;

namespace Assets.Scripts.AI.Action
{
    /// <summary>
    /// The <see cref="WaitAction"/> class is a <see cref="TaskAction"/> for when a <see cref="AdventurerPawn"/> is waiting.
    /// </summary>
    public class WaitAction : TaskAction
    {
        private float _period;
        private readonly float _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitAction"/> class.
        /// </summary>
        /// <param name="time">The length of time in seconds for the <see cref="Actor"/> to be waiting.</param>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="WaitAction"/>.</param>
        public WaitAction(float time, Pawn pawn) : base(pawn)
        {
            _time = time;
        }

        /// <inheritdoc/>
        public override bool CanListen => true;

        /// <inheritdoc/>
        public override bool CanSpeak => true;

        /// <inheritdoc/>
        public override int Complete()
        {
            return _period > _time ? 1 : 0;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            TaskStep step = Pawn.CurrentStep;
            if (step is not SitStep && step is not LayStep)
            {
                if (Pawn.CurrentNode.Reserved)
                {
                    if (Pawn is AdventurerPawn)
                    {
                        foreach ((RoomNode node, float) node in Pawn.CurrentNode.NextNodes)
                        {
                            if (!node.node.Reserved)
                            {
                                Pawn.CurrentStep = new WalkStep(node.node.WorldPosition, Pawn, step);
                                return;
                            }
                        }
                        Debug.Log("No Unreserved Location");
                    }
                    else if(step is not WaitStep)
                        Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, false);

                }
                else if (step is not WaitStep)
                    Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, true);
            }
        }

        /// <inheritdoc/>
        public override void Perform()
        {
            if(Pawn.CurrentStep is WalkStep)
            {
                if(Pawn.CurrentStep.IsComplete())
                {
                    Pawn.CurrentStep = new WaitStep(Pawn, Pawn.CurrentStep, true);
                }
            }
            _period += Time.deltaTime;
        }
    }
}
