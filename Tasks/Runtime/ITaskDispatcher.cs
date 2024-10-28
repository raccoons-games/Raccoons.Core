using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



public interface ITaskDispatcher
{
    Task Invoke(Func<Task> task)
    {
        return Invoke(async () =>
        {
            await task();
            return true;
        });
    }
    
    Task<TOut> Invoke<TOut>(Func<Task<TOut>> task);
    void RepeatOnException<TException>(int repeats);

    void RepeatOnException(int repeats)
    {
        RepeatOnException<Exception>(repeats);
    }

    event EventHandler<Exception> OnException;
}
