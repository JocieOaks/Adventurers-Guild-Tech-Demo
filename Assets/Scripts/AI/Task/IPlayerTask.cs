using System.Collections.Generic;
using Assets.Scripts.AI.Action;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="IPlayerTask"/> interface is for <see cref="ITask"/>'s that can be performed by the <see cref="PlayerPawn"/> and thus only need a reference to
    /// the <see cref="Pawn"/> performing the <see cref="IPlayerTask"/> and not an <see cref="Actor"/>.
    /// </summary>
    public interface IPlayerTask
    {
        /// <summary>
        /// Creates a list of <see cref="TaskAction"/>s to be performed by a <see cref="Pawn"/> in order to complete the <see cref="Task"/>.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> performing the <see cref="Task"/>.</param>
        /// <returns>Enumerates over the <see cref="TaskAction"/>s to perform the <see cref="Task"/></returns>
        IEnumerable<TaskAction> GetActions(Pawn pawn);
    }
}