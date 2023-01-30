using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent<T0> : ScriptableObject
{

    private List<GameEventListener<T0>> listeners = new List<GameEventListener<T0>>();

    public void Raise(T0 t0)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(t0);
        }
    }

    public void RegisterListener(GameEventListener<T0> listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener<T0> listener)
    {
        listeners.Remove(listener);
    }

}