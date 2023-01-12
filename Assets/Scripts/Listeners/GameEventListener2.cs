using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener<T0,T1> : MonoBehaviour
{
    public GameEvent<T0,T1> Event;
    public UnityEvent<T0,T1> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(T0 t0, T1 t1)
    {
        Response.Invoke(t0,t1);
    }
}
