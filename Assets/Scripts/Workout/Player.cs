using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public FloatVariable playerHP;
    [SerializeField]
    private FloatVariable _FullHealth;
    public FloatVariable playerGP;
    public FloatVariable meanHR;
    public FloatVariable maxHR;
    public FloatVariable playerSpeed;
    public Inventory shieldInventory;

    void Awake()
    {
        SetInitialValues();
    }

    public void SetInitialValues()
    {
        playerHP.Value = _FullHealth.Value;
        playerGP.Value = 0f;
        playerSpeed.Value = 2f;
        shieldInventory.Clear();
    }

}
