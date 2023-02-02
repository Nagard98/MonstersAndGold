using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ScorePanel : MonoBehaviour
{
    public RectTransform personalBestTransform;
    public TextMeshProUGUI[] hits;
    public TextMeshProUGUI timeText, scoreText;
    public ResultsVariable results, personalBest;
    private int minutes, minutesTween, seconds, secondsTween, perfectHits,greatHits, goodHits, missHits, score;

    // Start is called before the first frame update

    private void SavePB()
    {
        personalBest = results;
    }

    private void UpdateValues()
    {
        hits[0].text = string.Format("{0}", perfectHits);
        hits[1].text = string.Format("{0}", greatHits);
        hits[2].text = string.Format("{0}", goodHits);
        hits[3].text = string.Format("{0}", missHits);

        timeText.text = string.Format("{0:00}:{1:00}", minutesTween, secondsTween);
        scoreText.text = string.Format("{0}", score);
    }

    public void Init()
    {
        minutesTween = 0;
        secondsTween = 0;
        perfectHits = 0;
        greatHits = 0;
        goodHits = 0;
        missHits = 0;
        score = 0;
        minutes = Mathf.FloorToInt(results.time / 60);
        seconds = Mathf.FloorToInt(results.time % 60);
    }

    private void IsPersonalBest()
    {
        if (results.score > personalBest.score)
        {
            personalBestTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce).SetDelay(1f);
            SavePB();
        }
    }

    //When enabled animates all the scores appearing
    private void OnEnable()
    {
        Init();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(()=>perfectHits, (x)=>perfectHits=x, results.perfectHits, 1f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => greatHits, (x) => greatHits = x, results.greatHits, 1f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => goodHits, (x) => goodHits = x, results.goodHits, 1f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => missHits, (x) => missHits = x, results.missHits, 1f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => secondsTween, (x) => secondsTween = x, seconds, 0.5f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => minutesTween, (x) => minutesTween = x, minutes, 0.2f).SetEase(Ease.Linear));
        sequence.Append(DOTween.To(() => score, (x) => score = x, results.CalculateScore(), 1f).SetEase(Ease.Linear));
        sequence.SetDelay(1f);
        sequence.onUpdate = UpdateValues;
        sequence.onComplete = IsPersonalBest;
    }

}
