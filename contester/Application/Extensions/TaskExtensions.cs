﻿using System.Runtime.CompilerServices;

namespace contester.Application.Extensions;

public static class TaskEx
{
    public static async Task<(T0, T1)> WhenAll<T0, T1>(Task<T0> task0, Task<T1> task1)
    {
        await Task.WhenAll(task0, task1).ConfigureAwait(false);
        return (task0.Result, task1.Result);
    }

    public static async Task<(T0, T1, T2)> WhenAll<T0, T1, T2>(Task<T0> task0, Task<T1> task1, Task<T2> task2)
    {
        await Task.WhenAll(task0, task1, task2).ConfigureAwait(false);
        return (task0.Result, task1.Result, task2.Result);
    }

    public static async Task<(T0, T1, T2, T3)> WhenAll<T0, T1, T2, T3>(Task<T0> task0, Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
        await Task.WhenAll(task0, task1, task2, task3).ConfigureAwait(false);
        return (task0.Result, task1.Result, task2.Result, task3.Result);
    }

    public static async Task<(T0, T1, T2, T3, T4)> WhenAll<T0, T1, T2, T3, T4>(Task<T0> task0, Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {
        await Task.WhenAll(task0, task1, task2, task3, task4).ConfigureAwait(false);
        return (task0.Result, task1.Result, task2.Result, task3.Result, task4.Result);
    }

    public static async Task<(T0, T1, T2, T3, T4, T5)> WhenAll<T0, T1, T2, T3, T4, T5>(Task<T0> task0, Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {
        await Task.WhenAll(task0, task1, task2, task3, task4, task5).ConfigureAwait(false);
        return (task0.Result, task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
    }

    public static async Task<(T0, T1, T2, T3, T4, T5, T6)> WhenAll<T0, T1, T2, T3, T4, T5, T6>(Task<T0> task0, Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {
        await Task.WhenAll(task0, task1, task2, task3, task4, task5, task6).ConfigureAwait(false);
        return (task0.Result, task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
    }
}