using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public abstract class TaskAction
{
    public abstract bool CanSpeak { get; }

    public abstract bool CanListen { get; }

    protected Actor _actor;
    protected TaskAction(Actor actor)
    {
        _actor = actor;
    }

    public abstract void Initialize();

    public abstract void Perform();

    //1 - Completed
    //0 - Incomplete
    //-1 - Cannot be completed

    public abstract int Complete();
}