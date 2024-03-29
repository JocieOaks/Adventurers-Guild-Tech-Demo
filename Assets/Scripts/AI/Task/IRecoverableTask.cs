﻿using System.Collections.Generic;
using Assets.Scripts.AI.Action;

namespace Assets.Scripts.AI.Task
{
    /// <summary>
    /// The <see cref="IRecoverableTask"/> interface is for <see cref="Task"/>s that can potentially recover if one of their <see cref="TaskAction"/>s fail.
    /// </summary>
    public interface IRecoverableTask : ITask
    {
        /// <summary>
        /// Tries to create a new list of <see cref="TaskAction"/>s to complete the <see cref="Task"/> even if a previous attempt failed.
        /// </summary>
        /// <param name="actor">The <see cref="Actor"/> performing the <see cref="Task"/>.</param>
        /// <param name="action">The <see cref="TaskAction"/> that had previously failed.</param>
        /// <returns>Enumerates over a new list of <see cref="TaskAction"/>s for the <c>actor</c> to perform. If the <see cref="Task"/> fails to recover the enumeration immediately halts.</returns>
        public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action);
    }
}
