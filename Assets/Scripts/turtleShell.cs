using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class turtleShell : MonoBehaviour
{
    private GameManager _gameManager;

    private Animator anim;
    public ParticleSystem hitEffect;
    public int HP;
    private bool isDead;

    public enemyState state;
    private NavMeshAgent agent;
    private Vector3 destination;

    private bool isWalk;
    private bool isAlert;
    public bool isAttack;
    private bool isPlayerVisible;


    void Start()
    {
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ChangeState(state);
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other) //Entrando na zona de colisão(visão) com o player
    {
        if (_gameManager.currentState != GameState.GamePlay) { return; }
        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = true;

            if (state == enemyState.Idle || state == enemyState.Patrol)
            {
                ChangeState(enemyState.Alert);
            }
            else if (state == enemyState.Follow)
            {
                StopCoroutine("Follow");
                ChangeState(enemyState.Follow);
            }
        }
    }

    private void OnTriggerExit(Collider other) //  Saindo da colisão(visão) com o player
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = false;
        }
    }
    void ChangeState(enemyState newState)
    {
        StopAllCoroutines();

        isAlert = false;
        switch (newState)
        {
            case enemyState.Idle: //PARADO
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                StartCoroutine("idle");
                break;

            case enemyState.Alert: // ALERTA
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                isAlert = true;
                StartCoroutine("Alert");
                break;

            case enemyState.Follow: //SEGUIR
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                StartCoroutine("Follow");
                break;

            case enemyState.Dead: // DERRROTADO
                destination = transform.position;
                agent.destination = destination;
                break;
        }
        state = newState;
    }
    void StateManager() // Controla o comportamento do Slime
    {
        if (_gameManager.currentState == GameState.Dead && (state == enemyState.Follow || state == enemyState.Fury || state == enemyState.Alert))
        {
            ChangeState(enemyState.Idle);
        }
        switch (state)
        {
            case enemyState.Alert:
                LookAt();
                break;
            case enemyState.Follow:
                LookAt();
                destination = _gameManager.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartCoroutine("ATTACK");
                }
                break;
            case enemyState.Fury:
                // LookAt();
                destination = _gameManager.player.position;
                agent.destination = destination;
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;
            case enemyState.Patrol:
                break;
        }
    }
    void Attack() // chama o ataque
    {
        if (isAttack == false && isPlayerVisible == true)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
        }
    }
    void LookAt()
    {
        Vector3 lookDirection = (_gameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gameManager.slimeLookSpeed * Time.deltaTime); // Slerp interpolação esférica. Faz um cáculo entre 2 posições para definir como irá rotacionar entre uma posição e outra
    }
    void GetHit(int amount)
    {
        if (isDead == true)
        {
            return; // Se isDead estiver ligado, ele não vai executar os comandos seguintes.
        }
        ChangeState(enemyState.Alert);
        if (HP > 0)
        {            
            anim.SetTrigger("GetHit");
            hitEffect.Emit(15);
        }
        else
        {
            ChangeState(enemyState.Dead);
            anim.SetTrigger("Dead");
            StartCoroutine("Dead");
        }
    }
}
