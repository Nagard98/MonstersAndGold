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

    public void Reset()
    {
        perfectHits = 0;
        greatHits = 0;
        goodHits = 0;
        missHits = 0;
        longestStreak = 0;
        time = 0;
        score = 0;
    }
}
