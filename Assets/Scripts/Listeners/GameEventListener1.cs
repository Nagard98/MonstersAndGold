using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener<T0> : MonoBehaviour
{
    public GameEvent<T0> Event;
    public UnityEvent<T0> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(T0 t0)
    {
        Response.Invoke(t0);
    }
}
