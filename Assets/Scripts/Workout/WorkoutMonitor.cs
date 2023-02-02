using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorkoutMonitor : MonoBehaviour
{
    public List<GoldVariable> goldTypes;
    public List<PotionVariable> potionTypes;
    private int _conditionMetPotion;
    public List<ShieldVariable> shieldTypes;
    private int _conditionMetShield;
    public List<EnemyVariable> enemyTypes;

    private GameStateVariable _gameState;

    [SerializeField]
    private WorkoutPhasesVariable _workoutPhases;
    private int _currentPhase;
    private float _phaseTimer;

    public FloatVariable beatsShownInAdvance;

    [SerializeField]
    private Inventory _inventory;

    [SerializeField]
    private FloatVariable _playerHealth, _fullHealth, _speed, _restHR, _optSpeed, _meanHR, _maxHR;

    [SerializeField]
    public FloatIntervalVariable optRange;

    //TODO lastPROCGood
    private bool _isLastPROCGood;
    private float _meanSpeed;
    public float _currentHR;
    public float durationPROC;
    public float itemGroundOffset;
    private float _lowerOptHR, _higherOptHR;

    public UnityEvent<POIVariable, SpawnSettings> SpawnPOI;
    public UnityEvent<float> PhaseStarted, PhaseEnded;
    public UnityEvent EndWorkout;

    private bool _waitingPhaseUpdate;
    private float _totalPhaseDuration;
    private bool _hasWorkoutStarted;
    private bool _startedPROC;

    private void Awake()
    {
        SetupTiers();
        _gameState = Resources.Load<GameStateVariable>("GameState");
        _lowerOptHR = (optRange.lowerBound / 100f) * _maxHR.Value;
        _higherOptHR = (optRange.higherBound / 100f) * _maxHR.Value;
    }

    void Start()
    {
        CleanUp();        
    }

    public void CleanUp()
    {
        StopAllCoroutines();
        _inventory.Clear();
        _startedPROC = false;
        _playerHealth.Value = _fullHealth.Value;
        _hasWorkoutStarted = false;
        _waitingPhaseUpdate = false;
        _phaseTimer = 0;
        _currentPhase = 0;
        _currentHR = 90;
        _meanHR.Value = 90;
        _meanSpeed = _speed.Value;
        _isLastPROCGood = true;
        _conditionMetPotion = 0;
        _conditionMetShield = 0;
    }

    public void StartWorkout()
    {
        float beatDuration = 60f / (float)_workoutPhases.Value[_currentPhase].bpm;
        _totalPhaseDuration = _workoutPhases.Value[_currentPhase].totalSongDuration + beatsShownInAdvance.Value * beatDuration;
        StartCoroutine(UpdateHR());
        StartCoroutine(CheckIsLastPROCGood());
        PhaseStarted.Invoke(_currentPhase);
        ContinuePhase();
        _hasWorkoutStarted = true;
    }

    public void ContinuePhase()
    {
        if (_phaseTimer < _workoutPhases.Value[_currentPhase].totalSongDuration)
        {
            StartCoroutine(ContinuePhaseCoroutine());
        }
        else
        {
            PhaseEnded.Invoke(_currentPhase);
            StartCoroutine(NextPhase(_workoutPhases.delayBetweenPhases));
        }
    }

    public IEnumerator ContinuePhaseCoroutine()
    {
        _startedPROC = true;
        yield return new WaitForSeconds(durationPROC);
        StartROAV();
    }


    private IEnumerator NextPhase(float phaseDelay)
    {
        yield return new WaitForSeconds(phaseDelay);
        _phaseTimer = 0f;
        _currentPhase += 1;
        if (_currentPhase < _workoutPhases.Value.Length)
        {
            float beatDuration = 60f / (float)_workoutPhases.Value[_currentPhase].bpm;
            _totalPhaseDuration = _workoutPhases.Value[_currentPhase].totalSongDuration + beatsShownInAdvance.Value * beatDuration;
            _waitingPhaseUpdate = false;
            ContinuePhase();
            PhaseStarted.Invoke(_currentPhase);
        }
        else EndWorkout.Invoke();
    }


    public float StartROAV()
    {
        
        _meanSpeed = 0.9f * _meanSpeed + 0.1f * _speed.Value;
        float optHR = (_lowerOptHR + _higherOptHR) / 2;
        _optSpeed.Value = _meanSpeed * (optHR - _restHR.Value) / (_meanHR.Value - _restHR.Value);


        if (_meanHR.Value < _lowerOptHR)
        {
            return SpawnGold(1, _optSpeed.Value, itemGroundOffset);
        }
        else if (_meanHR.Value > _higherOptHR)
        {
            int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(_meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
            return SpawnEnemy(tier, _optSpeed.Value);
        }
        else
        {
            if (_isLastPROCGood)
            {
                if (_playerHealth.Value < _fullHealth.Value)
                {
                    int tier = (Mathf.Clamp(_conditionMetPotion, 0, 2) % 3) + 1;
                    return SpawnPotion(tier, _optSpeed.Value, itemGroundOffset);
                }
                else if (_playerHealth.Value == _fullHealth.Value)
                {
                    if (_inventory.IsFull)
                    {
                        return SpawnGold(3, _optSpeed.Value, itemGroundOffset);
                    }
                    else
                    {
                        int tier = (Mathf.Clamp(_conditionMetShield, 0, 2) % 3) + 1;
                        return SpawnShield(tier, _optSpeed.Value, itemGroundOffset);
                    }
                }
            }
            else
            {
                //TO-DO: definisci tier per questa situazione
                int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(_meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
                return SpawnGold(2, _optSpeed.Value, itemGroundOffset);
            }
            return 0;
        }
    }

    //Sorts the various POI lists in tiers based on their value
    private void SetupTiers()
    {
        enemyTypes.Sort((poi1, poi2) => poi1.GetValue().CompareTo(poi2.GetValue()));
        for(int i = 0; i < enemyTypes.Count; i++)
        {
            enemyTypes[i].Tier = i + 1;
        }
        goldTypes.Sort((poi1, poi2) => poi1.GetValue().CompareTo(poi2.GetValue()));
        for (int i = 0; i < goldTypes.Count; i++)
        {
            goldTypes[i].Tier = i + 1;
        }
        shieldTypes.Sort((poi1, poi2) => poi1.GetValue().CompareTo(poi2.GetValue()));
        for (int i = 0; i < shieldTypes.Count; i++)
        {
            shieldTypes[i].Tier = i + 1;
        }
        potionTypes.Sort((poi1, poi2) => poi1.GetValue().CompareTo(poi2.GetValue()));
        for (int i = 0; i < potionTypes.Count; i++)
        {
            potionTypes[i].Tier = i + 1;
        }
    }

    private float SpawnPotion(int tier, float optSpeed, float itemGroundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (_phaseTimer + ttl <= _workoutPhases.Value[_currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(potionTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl+2f, itemGroundOffset));
            _conditionMetPotion += 1;
        }
        return ttl;
    }

    private float SpawnShield(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (_phaseTimer + ttl <= _workoutPhases.Value[_currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(shieldTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl+2f, groundOffset));
            _conditionMetShield += 1;
        }
        return ttl;
    }

    private float SpawnEnemy(int tier, float optSpeed, float itemGroundOffset = 0)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (_phaseTimer + ttl <= _workoutPhases.Value[_currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(enemyTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl-4f, itemGroundOffset));
        }
        return ttl;
    }

    private float SpawnGold(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (_phaseTimer + ttl <= _workoutPhases.Value[_currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(goldTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl+2f, groundOffset));
        }
        return ttl;
    }

    private float ConvertValueToDistance(int tier)
    {
        return (float)(Math.Log((float)tier * 50f) * 15f);
    }

    public IEnumerator UpdateHR()
    {
        while (true)
        {
            if (_startedPROC == true)
            {
                _meanHR.Value = 120f;
                _startedPROC = false;
            }
            _meanHR.Value = 0.8f * _meanHR.Value + 0.2f * MathUtils.Remap(_speed.Value, 1, 10, 70, 190);
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator CheckIsLastPROCGood()
    {
        int numGoodSamples = 0;
        while (true)
        {
            if (_meanHR.Value >= _lowerOptHR && _meanHR.Value <= _higherOptHR)
            {
                numGoodSamples = Mathf.Clamp(numGoodSamples + 1, -4, 4);
            }
            else
            {
                numGoodSamples = Mathf.Clamp(numGoodSamples - 1, -4, 4);
            }
            _isLastPROCGood = numGoodSamples > 0;
            yield return new WaitForSeconds(3f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameState.isPaused && _hasWorkoutStarted)
        {
            _currentHR = MathUtils.Remap(_speed.Value, 1, 10, 70, 200);
            _phaseTimer += Time.deltaTime;

            if (!_waitingPhaseUpdate && _phaseTimer >= _totalPhaseDuration)
            {
                _waitingPhaseUpdate = true;
                PhaseEnded.Invoke(_currentPhase);
                StartCoroutine(NextPhase(_workoutPhases.delayBetweenPhases));
            }
        }
        
    }


}

