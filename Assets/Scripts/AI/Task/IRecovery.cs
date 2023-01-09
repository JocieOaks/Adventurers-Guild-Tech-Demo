using System.Collections.Generic;

public interface IRecovery
{
    public IEnumerable<TaskAction> Recover(Actor actor, TaskAction action);
}
