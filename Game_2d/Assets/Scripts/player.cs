using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] int lives = 5;

    public bool isAttacking = false;
    public bool isRecharger = true;
    private bool isGrounded = false;

    public Transform attackPos;
    public float attackRange;
    public LayerMask enemy;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    public static player Instance { get; set; }

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }
    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Instance = this;
        isRecharger = true;
    }

    private void Run()
    {
        if (isGrounded) State = States.run;
        Vector3 dir = transform.right * Input.GetAxis("Horizontal");
        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);
        sprite.flipX = dir.x < 0.0f;
    }
    private void Update()
    {
        if (isGrounded && !isAttacking) State = States.idle;
        if (!isAttacking && Input.GetButton("Horizontal"))
            Run();
        if (!isAttacking && isGrounded && Input.GetButtonDown("Jump"))
            Jump();
        if (Input.GetButtonDown("Fire1"))
            Attack();
        CheckGround();
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

    private IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(0.5f);
        isRecharger = true;
    }
    
    private void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        isGrounded = collider.Length > 1;
        if (!isGrounded) State = States.jump;
    }

    public void GetDamage()
    {
        lives -= 1;
        Debug.Log(lives);
    }

    private void Attack()
    {
        if (isGrounded && isRecharger)
        {
            State = States.attack;
            isAttacking = true;
            isRecharger = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCoolDown());
        }
    }

    private void OnAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].GetComponent<Entity>().GetDamage();
        }
    }

    public void TakeDamage(int damage)
    {
        lives -= damage;
    }

    public enum States
    {
        idle,
        run,
        jump,
        attack
    }
}



