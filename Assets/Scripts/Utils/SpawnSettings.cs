using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpawnSettings
{
    public float distance;
    public float TTL;
    public float groundOffset;

    public SpawnSettings(float distFromPlayer, float timeToLive, float groundOffset=0)
    {
        distance = distFromPlayer;
        TTL = timeToLive;
        this.groundOffset = groundOffset;
    }
}
