using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorkoutMonitor : MonoBehaviour
{
    public List<GoldVariable> goldTypes;
    public List<PotionVariable> potionTypes;
    private int conditionMetPotion;
    public List<ShieldVariable> shieldTypes;
    private int conditionMetShield;
    public List<EnemyVariable> enemyTypes;

    [SerializeField]
    private WorkoutPhasesVariable workoutPhases;
    private int currentPhase;
    private float phaseTimer;

    public FloatVariable beatsShownInAdvance;

    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private FloatVariable playerHealth, fullHealth;

    public bool isLastPROCGood;
    public FloatIntervalVariable optRange;
    public FloatVariable speed;
    public FloatVariable restHR;
    private float meanSpeed;
    private float optSpeed;
    public FloatVariable meanHR;
    public float currentHR;
    public float durationPROC;
    public float itemGroundOffset;

    public UnityEvent<POIVariable, SpawnSettings> SpawnPOI;
    public UnityEvent<float> PhaseStarted, PhaseEnded;
    private bool waitingPhaseUpdate;
    private float totalPhaseDuration;
    public UnityEvent EndWorkout;
    private bool hasWorkoutStarted;

    void Start()
    {
        hasWorkoutStarted = false;
        waitingPhaseUpdate = false;
        phaseTimer = 0;
        currentPhase = 0;
        currentHR = 0;
        meanSpeed = speed.Value;
        isLastPROCGood = true;
        conditionMetPotion = 0;
        conditionMetShield = 0;
        SetupTiers();
    }

    public void StartWorkout()
    {
        float beatDuration = 60f / (float)workoutPhases.Value[currentPhase].bpm;
        totalPhaseDuration = workoutPhases.Value[currentPhase].totalSongDuration + beatsShownInAdvance.Value * beatDuration;
        StartCoroutine(UpdateHR());
        PhaseStarted.Invoke(currentPhase);
        ContinuePhase();
        hasWorkoutStarted = true;
    }

    public void ContinuePhase()
    {
        if (phaseTimer < workoutPhases.Value[currentPhase].totalSongDuration)
        {
            //Debug.Log("Fase" + currentPhase + " iniziata");
            StartCoroutine(ContinuePhaseCoroutine());
        }
        else
        {
            //Debug.Log("Fase" + currentPhase + " completamente terminata");
            PhaseEnded.Invoke(currentPhase);
            StartCoroutine(NextPhase(workoutPhases.delayBetweenPhases));
        }
    }

    public IEnumerator ContinuePhaseCoroutine()
    {
        //print("InizioPROC");
        yield return new WaitForSeconds(durationPROC);
        //print("FinePROC");
        Debug.LogWarning("ROAV iniziato");
        StartROAV();
    }


    private IEnumerator NextPhase(float phaseDelay)
    {
        yield return new WaitForSeconds(phaseDelay);
        phaseTimer = 0f;
        currentPhase += 1;
        if (currentPhase < workoutPhases.Value.Length)
        {
            float beatDuration = 60f / (float)workoutPhases.Value[currentPhase].bpm;
            totalPhaseDuration = workoutPhases.Value[currentPhase].totalSongDuration + beatsShownInAdvance.Value * beatDuration;
            waitingPhaseUpdate = false;
            ContinuePhase();
            PhaseStarted.Invoke(currentPhase);
        }
        else EndWorkout.Invoke();
    }


    public float StartROAV()
    {
        
        meanSpeed = 0.9f * meanSpeed + 0.1f * speed.Value;
        float optHR = (optRange.lowerBound + optRange.higherBound) / 2;
        optSpeed = meanSpeed * (optHR - restHR.Value) / (meanHR.Value - restHR.Value);

        if (meanHR.Value < optRange.lowerBound)
        {
            return SpawnGold(1, optSpeed, itemGroundOffset);
        }
        else if (meanHR.Value > optRange.higherBound)
        {
            int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
            return SpawnEnemy(tier, optSpeed);
        }
        else
        {
            if (isLastPROCGood)
            {
                if (playerHealth.Value < fullHealth.Value)
                {
                    int tier = (Mathf.Clamp(conditionMetPotion, 0, 2) % 3) + 1;
                    return SpawnPotion(tier, optSpeed, itemGroundOffset);
                }
                else if (playerHealth.Value == fullHealth.Value)
                {
                    if (inventory.IsFull)
                    {
                        return SpawnGold(3, optSpeed, itemGroundOffset);
                    }
                    else
                    {
                        int tier = (Mathf.Clamp(conditionMetShield, 0, 2) % 3) + 1;
                        return SpawnShield(tier, optSpeed, itemGroundOffset);
                    }

                }
            }
            else
            {
                //TO-DO: definisci tier per questa situazione
                int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
                return SpawnGold(2, optSpeed, itemGroundOffset);
            }
            return 0;
        }
    }

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
        if (phaseTimer + ttl <= workoutPhases.Value[currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(potionTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl, itemGroundOffset));
            conditionMetPotion += 1;
        }
        return ttl;
    }

    private float SpawnShield(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (phaseTimer + ttl <= workoutPhases.Value[currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(shieldTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl, groundOffset));
            conditionMetShield += 1;
        }
        return ttl;
    }

    private float SpawnEnemy(int tier, float optSpeed, float itemGroundOffset = 0)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (phaseTimer + ttl <= workoutPhases.Value[currentPhase].totalSongDuration)
        {
            SpawnPOI.Invoke(enemyTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl, itemGroundOffset));
        }
        return ttl;
    }

    private float SpawnGold(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        float ttl = distance / optSpeed;
        if (phaseTimer + ttl <= workoutPhases.Value[currentPhase].totalSongDuration)
        {
            //Debug.LogWarning("Chiamato");
            SpawnPOI.Invoke(goldTypes.Find(x => x.Tier == tier), new SpawnSettings(distance, ttl, groundOffset));
        }
        return ttl;
    }

    private float ConvertValueToDistance(int tier)
    {
        return tier * 100f;
    }

    public IEnumerator UpdateHR()
    {
        while (true)
        {
            meanHR.Value = 0.9f * meanHR.Value + 0.1f * MathUtils.Remap(speed.Value, 1, 10, 70, 200);
            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 0 && hasWorkoutStarted)
        {
            currentHR = MathUtils.Remap(speed.Value, 1, 10, 70, 200);
            phaseTimer += Time.deltaTime;
            if (!waitingPhaseUpdate && phaseTimer >= totalPhaseDuration)
            {
                Debug.LogWarning(phaseTimer + " " + totalPhaseDuration);
                Debug.Log("Fase" + currentPhase + " completamente terminata");
                waitingPhaseUpdate = true;
                PhaseEnded.Invoke(currentPhase);
                StartCoroutine(NextPhase(workoutPhases.delayBetweenPhases));
            }
        }
        
    }


}

