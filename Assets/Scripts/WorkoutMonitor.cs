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
    private Inventory inventory;

    [SerializeField]
    private FloatVariable playerHealth;
    [SerializeField]
    private FloatVariable fullHealth;

    public bool isLastPROCGood;
    public FloatIntervalVariable optRange;
    public FloatVariable speed;
    public FloatVariable restHR;
    private float meanSpeed;
    private float optSpeed;
    public FloatVariable meanHR;
    public float currentHR;

    public UnityEvent<POIVariable, SpawnSettings> SpawnPOI;
    public float itemGroundOffset;

    void Start()
    {
        currentHR = 0;
        meanSpeed = speed.Value;
        isLastPROCGood = true;
        conditionMetPotion = 0;
        conditionMetShield = 0;
        StartCoroutine(UpdateHR());
        StartCoroutine(StartPROC());
    }

    public void ResetPhase()
    {
        StartCoroutine(StartPROC());
    }

    public IEnumerator StartPROC()
    {
        yield return new WaitForSeconds(10);
        StartROAV();
    }

    public void StartROAV()
    {
        meanSpeed = 0.9f * meanSpeed + 0.1f * speed.Value;
        float optHR = (optRange.lowerBound + optRange.higherBound) / 2;
        optSpeed = meanSpeed * (optHR - restHR.Value) / (meanHR.Value - restHR.Value);

        if (meanHR.Value < optRange.lowerBound)
        {
            SpawnGold(1, optSpeed, itemGroundOffset);
        }
        else if (meanHR.Value > optRange.higherBound)
        {
            int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
            SpawnEnemy(tier, optSpeed);
        }
        else
        {
            if (isLastPROCGood)
            {
                if (playerHealth.Value < fullHealth.Value)
                {
                    int tier = (Mathf.Clamp(conditionMetPotion, 0, 2) % 3) + 1;
                    SpawnPotion(tier, optSpeed, itemGroundOffset);
                }
                else if (playerHealth.Value == fullHealth.Value)
                {
                    if (inventory.IsFull)
                    {
                        SpawnGold(3, optSpeed, itemGroundOffset);
                    }
                    else
                    {
                        int tier = (Mathf.Clamp(conditionMetShield, 0, 2) % 3) + 1;
                        SpawnShield(tier, optSpeed, itemGroundOffset);
                    }

                }
            }
            else
            {
                //TO-DO: definisci tier per questa situazione
                int tier = (int)Math.Round(Mathf.Clamp(MathUtils.Remap(meanHR.Value, optRange.higherBound, optRange.higherBound + 50, 1, 3), 1, 3));
                SpawnGold(2, optSpeed, itemGroundOffset);
            }

        }
    }

    private void SpawnPotion(int tier, float optSpeed, float itemGroundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        SpawnPOI.Invoke(potionTypes.Find(x => x.tier == tier), new SpawnSettings(distance, distance / optSpeed, itemGroundOffset));
        conditionMetPotion += 1;
    }

    private void SpawnShield(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        SpawnPOI.Invoke(shieldTypes.Find(x => x.tier == tier), new SpawnSettings(distance, distance / optSpeed, groundOffset));
        conditionMetShield += 1;
    }

    private void SpawnEnemy(int tier, float optSpeed, float itemGroundOffset = 0)
    {
        float distance = ConvertValueToDistance(tier);
        SpawnPOI.Invoke(enemyTypes.Find(x => x.tier == tier), new SpawnSettings(distance, distance / optSpeed, itemGroundOffset));
    }

    private void SpawnGold(int tier, float optSpeed, float groundOffset)
    {
        float distance = ConvertValueToDistance(tier);
        SpawnPOI.Invoke(goldTypes.Find(x => x.tier == tier), new SpawnSettings(distance, distance / optSpeed, groundOffset));
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
        currentHR = MathUtils.Remap(speed.Value, 1, 10, 70, 200);
    }


}
