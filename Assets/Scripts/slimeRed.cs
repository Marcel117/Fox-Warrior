using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class slimeRed : MonoBehaviour
{
    private GameManager _gameManager;

    private Animator anim;
    public ParticleSystem hitEffect;
    public int HP;
    private bool isDead;

    public enemyState state;

    //IA
    private bool isWalk;
    private bool isAlert;
    public bool isAttack;
    private bool isPlayerVisible;
    private NavMeshAgent agent;
    private int idWayPoint;
    private Vector3 destination;

    private int isWalkAnim;    
    void Start()
    {
        isWalkAnim = Animator.StringToHash("isWalk");

        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ChangeState(state);        
    }
    void Update()
    {
        StateManager();

        if(agent.desiredVelocity.magnitude >= 0.1f)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
        anim.SetBool(isWalkAnim, isWalk);
        anim.SetBool("isAlert", isAlert);
    }    

    private void OnTriggerEnter(Collider other) //Entrando na zona de colisão(visão) com o player
    {
        if(_gameManager.currentState != GameState.GamePlay) { return;}
        if(other.gameObject.tag == "Player")
        {
            isPlayerVisible = true;

            if(state == enemyState.Idle || state == enemyState.Patrol)
            {
                ChangeState(enemyState.Alert);
            }
            else if(state == enemyState.Follow)
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
    void GetHit(int amount)
    {
        if (isDead == true)
        {
            return; // Se isDead estiver ligado, ele não vai executar os comandos seguintes.
        }        
        HP -= amount;
        if(HP > 0)
        {
            ChangeState(enemyState.Fury);
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
    void StateManager() // Controla o comportamento do Slime
    {
        if(_gameManager.currentState == GameState.Dead && (state == enemyState.Follow || state == enemyState.Fury || state == enemyState.Alert))
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

                if(agent.remainingDistance <= agent.stoppingDistance)
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

            case enemyState.Patrol: //PATRULHA
                agent.stoppingDistance = 0;
                idWayPoint = Random.Range(0, _gameManager.slimeWayPoints.Length);
                destination = _gameManager.slimeWayPoints[idWayPoint].position;
                agent.destination = destination;
                StartCoroutine("Patrol");
                
                break;

            case enemyState.Follow: //SEGUIR
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                StartCoroutine("Follow");
                break;

            case enemyState.Fury: //FÚRIA
                destination = transform.position;
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                agent.destination = destination;
                break;
            case enemyState.Dead: // DERRROTADO
                destination = transform.position;
                agent.destination = destination;
                break;
        }
        state = newState;
    }
    IEnumerator Dead()
    {
        isDead = true;
        yield return new WaitForSeconds(2f);
        if (_gameManager.Drop(_gameManager.dropChance))
        {
            Instantiate(_gameManager.gemPreFab, new Vector3(transform.position.x, 0.95f, transform.position.z), _gameManager.gemPreFab.transform.rotation);
        }
        Destroy(this.gameObject);
    }
    IEnumerator idle()
    {
        yield return new WaitForSeconds(_gameManager.slimeIdleWaitTime);
        StayStill(50); // 50% de chance de ficar em Idle
    }
    IEnumerator Patrol()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0);
        StayStill(30); // 70% de chance de ficar em Patrol e 30% de chance de ficar em Idel
    }
    IEnumerator Alert()
    {
        yield return new WaitForSeconds(_gameManager.slimeAlertTime);
        if(isPlayerVisible == true)
        {
            ChangeState(enemyState.Follow);
        }
        else
        {
            StayStill(10); // 90% de chance de após ficar em alerta, entrar em patrulha
        }
    }
    IEnumerator Follow()
    {
        yield return new WaitUntil(() => isPlayerVisible);

        yield return new WaitForSeconds(_gameManager.slimeAlertTime);

        //StayStill(50);      
    }
    IEnumerator ATTACK() //Evita que o inimigo ataque sem parar
    {
        yield return new WaitForSeconds(_gameManager.slimeAttackDelay);
        isAttack = false;
        Attack();
        yield return new WaitForEndOfFrame();
        StopCoroutine("ATTACK");
    }
    int Rand()
    {
        int rand = Random.Range(0, 100); // vai de 0 a 99(o da direita não entra)
        return rand;
    } 

    void StayStill(int yes) // Yes seria a porcentagem de chance dele ficar em Idle
    {
        if(Rand() <= yes)
        {
            ChangeState(enemyState.Idle);
        }
        else // caso Não
        {
            ChangeState(enemyState.Patrol);
        }
    }
    void Attack() // chama o ataque
    {
        if(isAttack == false && isPlayerVisible == true)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
        }        
    }
    void AttackIsDone() // Evita que o inimigo ataque sem parar
    {
        StopCoroutine("ATTACK");
    }
    void LookAt()
    {
        Vector3 lookDirection = (_gameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gameManager.slimeLookSpeed * Time.deltaTime); // Slerp interpolação esférica. Faz um cáculo entre 2 posições para definir como irá rotacionar entre uma posição e outra
    }
}
