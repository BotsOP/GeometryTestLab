using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowManager : MonoBehaviour
{
    [SerializeField]private Material[] growMaterials;
    [Range(0, 1)]
    [SerializeField] private float growth;

    [SerializeField] private bool useTime;

    [SerializeField] private float speedGrowth;
    private float speedGrowthMultiplier;

    private void Awake()
    {
        speedGrowthMultiplier = 1 / speedGrowth;
    }

    private void Update()
    {
        float t = Time.time * speedGrowthMultiplier % 2;
        if (t > 1)
        {
            t = 2 - t;
        }
        
        foreach (var mat in growMaterials)
        {
            if (useTime)
            {
                mat.SetFloat("_Grow", t);
            }
            else
            {
                mat.SetFloat("_Grow", growth);
            }
            
        }
    }
}
