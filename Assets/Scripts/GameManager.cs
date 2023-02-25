using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public enum enemyState
{
    Idle, Alert, Patrol, Fury, Follow, Dead
}
public enum GameState
{
    GamePlay, Dead
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState currentState;
    
    public int Gems { get; private set; }
    [Header("Info Player")]
    public Transform player;

    [Header("UI")]
    public Text txtGem;

    [Header("SlimeIA")]
    public float slimeIdleWaitTime;
    public Transform[] slimeWayPoints;
    public float slimeDistanceToAttack = 2.3f;
    public float slimeAlertTime = 2f;
    public float slimeAttackDelay = 1f;
    public float slimeLookSpeed = 3f;

    [Header("Rain Manager")]
    public PostProcessVolume postB;
    public ParticleSystem rainParticle;
    private ParticleSystem.EmissionModule rainModule;
    public int rainRateOvertime;
    public int rainIncrement;
    public float rainIncrementDelay;

    [Header("Drop Item")]
    public GameObject gemPreFab;
    public int dropChance = 25; // valor de 0 a 100

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        

        rainModule = rainParticle.emission;
        txtGem.text = Gems.ToString();
    }

    void Update()
    {
        
    }
    public void onOffRaind(bool isRain)
    {
        StopCoroutine("RainManager");
        StopCoroutine("PostBManager");
        StartCoroutine("RainManager", isRain);
        StartCoroutine("PostBManager", isRain);
    }
    IEnumerator RainManager(bool isRain)
    {
        switch (isRain)
        {
            case true: // Aumenta a chuva
                for (float r = rainModule.rateOverTime.constant; r <= rainRateOvertime; r += rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = rainRateOvertime;

                break;

            case false: // Diminui a chuva
                for (float r = rainModule.rateOverTime.constant; r > 0; r -= rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = rainRateOvertime;
                break;
        }
    }
    IEnumerator PostBManager(bool isRain) // controla escuridão
    {
        switch (isRain)
        {
            case true: // Escurece
                for (float w = postB.weight; w < 1; w += 1 * Time.deltaTime)
                {
                    postB.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 1;
                break;
            case false: //Clareia
                for (float w = postB.weight; w > 0; w -= 1 * Time.deltaTime)
                {
                    postB.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 0;
                break;
        }
    }
    public void ChangeGameState(GameState newState)
    {
        currentState = newState;
    }
    public void SetGem(int amout)
    {
        Gems += amout;
        txtGem.text = Gems.ToString();
    }
    public bool Drop(int p)
    {
        bool retorno;
        int Temp = Random.Range(0, 100);
        if(Temp <= p)
        {
            retorno = true;
        }
        else
        {
            retorno = false;
        }
        return retorno;
    }
}
