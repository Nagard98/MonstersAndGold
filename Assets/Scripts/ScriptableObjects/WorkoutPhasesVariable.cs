using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorkoutPhasesVariable : ScriptableObject
{
    public WorkoutPhase[] Value;
    public float delayBetweenPhases;
}

[Serializable]
public struct WorkoutPhase
{
    public float totalSongDuration;
    public AudioClip phaseSong;
    public int bpm;

    public WorkoutPhase(float totalDuration, AudioClip phaseSong, int bpm)
    {
        this.totalSongDuration = totalDuration;
        this.phaseSong = phaseSong;
        this.bpm = bpm;
    }
}
