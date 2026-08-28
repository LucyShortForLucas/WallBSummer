#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class DependencyContainer
{
    // Stores all game systems by Type
    private Dictionary<Type, object> dependencies = new Dictionary<Type, object>();

    // Put system into box
    public void Register<T>(T dependency) where T : notnull
    {
        dependencies[typeof(T)] = dependency;
    }

    public void SafeRegister<T>(T? dependency) where T : class
    {
        if (dependency is not null) Register(dependency);
    }

    // Take system out of box
    public T? Get<T>() 
    {
        if (dependencies.TryGetValue(typeof(T), out object dependency))
        {
            return (T)dependency;
        }

        Debug.LogError($"[DependencyContainer] Could not find dependency of type {typeof(T)}!");
        return default;
    }
}