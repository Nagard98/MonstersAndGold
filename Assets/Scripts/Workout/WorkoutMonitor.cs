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
    private FloatVariable _playerHealth, _playerGold, _fullHealth, _speed, _maxSpeed, _restHR, _optSpeed, _meanHR, _maxHR;

    [SerializeField]
    public FloatIntervalVariable optRange;

    //TODO lastPROCGood
    private bool _isLastPROCGood;
    private float _meanPROCSpeed;
    private float _smoothSpeed;
    public float _currentHR;
    private float _lastRegistredPROCMeanHR;
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

    private bool _potionCooldownEnded;
    private bool _shieldCooldownEnded;

    private void Awake()
    {
        SetupTiers();
        _gameState = Resources.Load<GameStateVariable>("GameState");
        _lowerOptHR = (optRange.lowerBound) * _maxHR.Value;
        _higherOptHR = (optRange.higherBound) * _maxHR.Value;
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
        _playerGold.Value = 0.0f;
        _hasWorkoutStarted = false;
        _waitingPhaseUpdate = false;
        _phaseTimer = 0;
        _currentPhase = 0;
        //_currentHR = 90;
        _meanHR.Value = _restHR.Value;
        _lastRegistredPROCMeanHR = _meanHR.Value;
        _meanPROCSpeed = 0.0f;
        _smoothSpeed = 0.0f;//_speed.Value;
        _isLastPROCGood = true;
        _conditionMetPotion = 0;
        _conditionMetShield = 0;

        _potionCooldownEnded = true;
        _shieldCooldownEnded = true;
    }

    public IEnumerator PotionCooldownCoroutine()
    {
        _potionCooldownEnded = false;
        yield return new WaitForSeconds(60.0f);
        _potionCooldownEnded = true;
    }

    public IEnumerator ShieldCooldownCoroutine()
    {
        _shieldCooldownEnded = false;
        yield return new WaitForSeconds(60.0f);
        _shieldCooldownEnded = true;
    }

    public void StartWorkout()
    {
        //float beatDuration = 60f / (float)_workoutPhases.Value[_currentPhase].bpm;
        _totalPhaseDuration = _workoutPhases.Value[_currentPhase].totalSongDuration/* + beatsShownInAdvance.Value * beatDuration*/;
        StartCoroutine(UpdateHR());
        //StartCoroutine(CheckIsLastPROCGood());
        PhaseStarted.Invoke(_currentPhase);
        ContinuePhase();
        _hasWorkoutStarted = true;
    }

    public void ContinuePhase()
    {
        Debug.Log("Continuing Phase...");
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
        yield return StartCoroutine(CheckIsLastPROCGood());//new WaitForSeconds(durationPROC);
        StartROAV();
    }


    private IEnumerator NextPhase(float phaseDelay)
    {
        yield return new WaitForSeconds(phaseDelay);
        _phaseTimer = 0f;
        _currentPhase += 1;
        if (_currentPhase < _workoutPhases.Value.Length)
        {
            //float beatDuration = 60f / (float)_workoutPhases.Value[_currentPhase].bpm;
            _totalPhaseDuration = _workoutPhases.Value[_currentPhase].totalSongDuration /*+ beatsShownInAdvance.Value * beatDuration*/;
            _waitingPhaseUpdate = false;
            ContinuePhase();
            PhaseStarted.Invoke(_currentPhase);
        }
        else EndWorkout.Invoke();
    }


    public float StartROAV()
    {
        Debug.Log("Started ROAV..");
        float optHR = (_lowerOptHR + _higherOptHR) / 2;
        _optSpeed.Value = _smoothSpeed * (optHR - _restHR.Value) / Math.Max((_lastRegistredPROCMeanHR - _restHR.Value), 1.0f);

        if(_optSpeed.Value > 0.0f)
        {
            if (_isLastPROCGood)
            {
                if ((_playerHealth.Value < _fullHealth.Value) && _potionCooldownEnded)
                {
                    StartCoroutine(PotionCooldownCoroutine());
                    int tier = (Mathf.Clamp(_conditionMetPotion, 0, 2) % 3) + 1;
                    return SpawnPotion(tier, _optSpeed.Value, itemGroundOffset);
                }
                else// (_playerHealth.Value == _fullHealth.Value)
                {
                    if ((!_inventory.IsFull) && _shieldCooldownEnded)
                    {
                        StartCoroutine(ShieldCooldownCoroutine());
                        int tier = (Mathf.Clamp(_conditionMetShield, 0, 2) % 3) + 1;
                        return SpawnShield(tier, _optSpeed.Value, itemGroundOffset);
                    }
                    else
                    {
                        float diffHR = Math.Abs(optHR - _lastRegistredPROCMeanHR);
                        float diffRangeHR = _higherOptHR - optHR;
                        int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(diffHR, 0.0f, diffRangeHR, 1, 3), 1, 3));
                        return SpawnGold(tier, _optSpeed.Value, itemGroundOffset);
                    }
                }
            }
            else
            {
                if (_lastRegistredPROCMeanHR <= _lowerOptHR)
                {
                    return SpawnGold(1, _optSpeed.Value, itemGroundOffset);
                }
                else if (_lastRegistredPROCMeanHR >= _higherOptHR)
                {
                    int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(_lastRegistredPROCMeanHR, _higherOptHR, _maxHR.Value, 1, 3), 1, 3));
                    return SpawnEnemy(tier, _optSpeed.Value);
                }

                Debug.LogError("This ROAV code should be unreachable");
                return SpawnGold(1, _optSpeed.Value, itemGroundOffset);
            }
        }
        else
        {
            ContinuePhase();
            return 0;
        }
        

        //if (_meanHR.Value < _lowerOptHR)
        //{
        //    return SpawnGold(1, _optSpeed.Value, itemGroundOffset);
        //}
        //else if (_meanHR.Value > _higherOptHR)
        //{
        //    int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(_meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
        //    return SpawnEnemy(tier, _optSpeed.Value);
        //}
        //else
        //{
        //    if (_isLastPROCGood)
        //    {
        //        if (_playerHealth.Value < _fullHealth.Value)
        //        {
        //            int tier = (Mathf.Clamp(_conditionMetPotion, 0, 2) % 3) + 1;
        //            return SpawnPotion(tier, _optSpeed.Value, itemGroundOffset);
        //        }
        //        else if (_playerHealth.Value == _fullHealth.Value)
        //        {
        //            if (_inventory.IsFull)
        //            {
        //                return SpawnGold(3, _optSpeed.Value, itemGroundOffset);
        //            }
        //            else
        //            {
        //                int tier = (Mathf.Clamp(_conditionMetShield, 0, 2) % 3) + 1;
        //                return SpawnShield(tier, _optSpeed.Value, itemGroundOffset);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //TO-DO: definisci tier per questa situazione
        //        int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(_meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
        //        return SpawnGold(2, _optSpeed.Value, itemGroundOffset);
        //    }
        //    //return 0;
        //}

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
        Debug.Log("Spawn Potion at: " + distance + "m. OptSpeed: " + optSpeed + "m/s  ttl: " + ttl + "s");
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
        Debug.Log("Spawn Shield at: " + distance + "m. OptSpeed: " + optSpeed + "m/s  ttl: " + ttl + "s");
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
        Debug.Log("Spawn Enemy at: " + distance + "m. OptSpeed: " + optSpeed + "m/s  ttl: " + ttl + "s");
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
        Debug.Log("Spawn Gold at: " + distance + "m. OptSpeed: " + optSpeed + "m/s  ttl: " + ttl + "s");
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
                //_meanHR.Value = 120f;
                //_startedPROC = false;
            }
            _meanHR.Value = _restHR.Value + ((_smoothSpeed / _maxSpeed.Value) * (_maxHR.Value - _restHR.Value)); //0.8f * _meanHR.Value + 0.2f * MathUtils.Remap(_speed.Value, 1, 10, 70, 190);
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator CheckIsLastPROCGood()
    {
        //int numGoodSamples = 0;
        //while (true)
        //{
        //    if (_meanHR.Value >= _lowerOptHR && _meanHR.Value <= _higherOptHR)
        //    {
        //        numGoodSamples = Mathf.Clamp(numGoodSamples + 1, -4, 4);
        //    }
        //    else
        //    {
        //        numGoodSamples = Mathf.Clamp(numGoodSamples - 1, -4, 4);
        //    }
        //    _isLastPROCGood = numGoodSamples > 0;
        //    yield return new WaitForSeconds(3f);
        //}
        int numGoodSamples = 0;
        float elapsedTime = 0.0f;
        float PROC_STEP_CHECK_LENGTH = 3.0f;
        _lastRegistredPROCMeanHR = 0.0f;
        _meanPROCSpeed = 0.0f;
        Debug.Log("Started PROC...");

        while (elapsedTime < durationPROC)
        {
            _lastRegistredPROCMeanHR += (_meanHR.Value * ((1 / durationPROC) * PROC_STEP_CHECK_LENGTH));
            _meanPROCSpeed += (_smoothSpeed * ((1 / durationPROC) * PROC_STEP_CHECK_LENGTH));
            Debug.Log("Sampling PROC... Mean PROC Speed: "+_meanPROCSpeed);

            if (_meanHR.Value >= _lowerOptHR && _meanHR.Value <= _higherOptHR)
            {
                numGoodSamples = Mathf.Clamp(numGoodSamples + 1, -4, 4);
            }
            else
            {
                numGoodSamples = Mathf.Clamp(numGoodSamples - 1, -4, 4);
            }
            elapsedTime += PROC_STEP_CHECK_LENGTH;

            yield return new WaitForSeconds(PROC_STEP_CHECK_LENGTH);
        }

        Debug.Log("Finished PROC...");
        _isLastPROCGood = numGoodSamples > 0;
        _startedPROC = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameState.isPaused && _hasWorkoutStarted)
        {
            _currentHR = _meanHR.Value; //MathUtils.Remap(_speed.Value, 1, 10, 70, 200);
            _smoothSpeed = 0.9f * _smoothSpeed + 0.1f * _speed.Value;
            _phaseTimer += Time.deltaTime;
            //Debug.Log(_smoothSpeed);

            if (!_waitingPhaseUpdate && _phaseTimer >= _totalPhaseDuration)
            {
                _waitingPhaseUpdate = true;
                PhaseEnded.Invoke(_currentPhase);
                StartCoroutine(NextPhase(_workoutPhases.delayBetweenPhases));
            }
        }
        
    }


}

