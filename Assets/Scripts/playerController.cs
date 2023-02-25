using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private GameManager _gameManager;
    private CharacterController controller;
    private Animator anim;

    [Header("Player Settings")]
    public int HP;
    public float movementSpeed;
    private Vector3 direction;
    private bool isWalk;

    [Header("Player Inputs")]
    private float horizontal;
    private float vertical;

    [Header("Attack Settings")]
    public ParticleSystem fxAttack;
    public Transform hitBox;
    private bool isAttack;
    [Range(0.2f, 1f)]
    public float hitRange = 0.5f;
    public Collider[] hitInfo;
    public int amountDamage;
    public LayerMask hitMask;       

    void Start()
    {
       // _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        _gameManager = GameManager.Instance;
        movementSpeed = 3f;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if(_gameManager.currentState != GameState.GamePlay) { return; }  //Se o currentstate não for Gameplay, ele não executa os comandos. Ou seja, se o player for derrotado, o jogador perde controle sobre ele.
        Inputs(); // movimentos e ataques

        MoveCharacter(); // direciona o personagem de acordo com o movimento

        UpDateAnimator(); //atualiza a condição(idle,walk, run, etc)
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TakeDamage")
        {
            GetHit(1);
        }
    }
    void Inputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (Input.GetButtonDown("Fire1") && isAttack == false)
        {
            Attack();
        }
    }
    void Attack()
    {
        anim.SetTrigger("Attack");
        isAttack = true;
        fxAttack.Emit(1);

        hitInfo = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);

        foreach(Collider c in hitInfo)
        {
            c.gameObject.SendMessage("GetHit", amountDamage, SendMessageOptions.DontRequireReceiver);
        }

    }
    void MoveCharacter()
    {
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > 0.1)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;  // rotacionando e convertendo radiano para graus através do Rad2Deg
            transform.rotation = Quaternion.Euler(0, targetAngle, 0); // Gira em torno de Y
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }

        controller.Move(direction * movementSpeed * Time.deltaTime);
    }

    void UpDateAnimator()
    {
        anim.SetBool("isWalk", isWalk);
    }

    void AttackIsDone()
    {
        isAttack = false;
    }
    void GetHit(int amount)
    {
        HP -= amount;
        if (HP > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            _gameManager.ChangeGameState(GameState.Dead);
            anim.SetTrigger("Die");
        }
    }
    private void OnDrawGizmosSelected()
    {
        if(hitBox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitBox.position, hitRange);
        }        
    }
}
