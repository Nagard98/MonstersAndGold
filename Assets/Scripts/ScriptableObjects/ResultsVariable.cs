using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ResultsVariable : ScriptableObject
{
    public int perfectHits;
    public int greatHits;
    public int goodHits;
    public int missHits;

    public int longestStreak;

    public float time;
    public int score;

    public int CalculateScore()
    {
        score = perfectHits * 100 + greatHits * 50 + goodHits * 10 - missHits * 25;
        return score;
    }
}
