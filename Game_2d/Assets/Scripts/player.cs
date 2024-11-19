using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity // Наследуем от Entity
{
    public GameObject player;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpForce = 15f;
    public float boostJump = 5f; // Сила буста прыжка
    public bool isAttacking = false;
    public bool isRecharger = true;
    private bool isGrounded = false;

    public Transform attackPos;
    public float attackRange;
    public LayerMask enemy;
    public LayerMask enemy2; // Слой для слизня
    private bool isAttackFinished = true; // По умолчанию true, чтобы можно было начать бежать
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    public static Player Instance { get; set; }

    // Переменные для проверки заземления
    [Header("Ground Check")]
    public Transform groundedPoint; // Точка для проверки заземления
    public LayerMask groundLayer; // Слой земли для проверки
    public float checkRadius = 0.2f; // Радиус проверки касания земли

    // Переменные для звуков
    [Header("Sound Effects")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip attackSound;
    public AudioClip runSound;
    public AudioClip jumpFromSlimeSound;
    private AudioSource audioSource;

    public GameObject deathScreen;

    public States CurrentState => State; // Свойство для получения значения State

    public enum States
    {
        idle,
        run,
        jump,
        attack,
        Dead
    }

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
        audioSource = GetComponent<AudioSource>(); // Получаем компонент AudioSource
        Instance = this;
        isRecharger = true;
        lives = maxLives;
    }

    private void Update()
    {
        if (State != States.Dead)
        {
// Устанавливаем состояние игрока в idle, если он на земле и не атакует
            if (isGrounded && !isAttacking && isAttackFinished)
            State = States.idle;

        // Обработка ввода для атаки
        if (Input.GetButtonDown("Fire1") && !isAttacking)
            Attack();

        // Обработка ввода для движения
        if (Input.GetButton("Horizontal"))
            Run();

        // Проверка нажатия клавиши прыжка
        if (Input.GetButtonDown("Jump"))
            Jump();

        // Проверка состояния заземления
        CheckGround();
        }
            
    }

    private void Run()
    {
        if (isAttacking) State = States.attack;
        if (isGrounded && !isAttacking) State = States.run;

        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 dir = new Vector3(horizontalInput, 0, 0);
        transform.position += dir * speed * Time.deltaTime;

        // Поворот игрока в зависимости от направления
        if (horizontalInput > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            PlayRunSound(); // Воспроизводим звук бега
        }
        else if (horizontalInput < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            PlayRunSound(); // Воспроизводим звук бега
        }
    }

    private void Jump()
    {
        // Проверяем, может ли игрок прыгнуть (находится ли он на земле или на слизне)
        if (isGrounded || IsOnEnemyLayer())
        {
            float currentJumpForce = jumpForce; // Устанавливаем текущую силу прыжка

            // Проверяем, стоим ли мы на слизне (слой enemy2)
            if (IsOnEnemyLayer())
            {
                currentJumpForce += boostJump; // Добавляем буст к силе прыжка
                PlayJumpFromSlimeSound(); // Воспроизводим звук прыжка от слизня
            }

            rb.AddForce(transform.up * currentJumpForce, ForceMode2D.Impulse);
            PlayJumpSound(); // Воспроизводим звук прыжка
            isGrounded = false; // Убираем флаг заземления после прыжка
        }
    }

    private void CheckGround()
    {
        // Проверка заземления с использованием точки groundedPoint
        // Проверяем, касается ли игрок земли
        isGrounded = Physics2D.OverlapCircle(groundedPoint.position, checkRadius, groundLayer);

        // Также проверяем, стоит ли игрок на слизне
        if (IsOnEnemyLayer())
        {
            isGrounded = true; // Устанавливаем isGrounded в true, если игрок на слизне
        }

        if (!isGrounded && !isAttacking) // Если игрок не касается земли и не атакует
            State = States.jump;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundedPoint.position, checkRadius); // Рисуем Gizmos для точки заземления
    }

    private void Attack()
    {
        if (isRecharger)
        {
            State = States.attack;
            isAttacking = true;
            isRecharger = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCoolDown());

            OnAttack();
            PlayAttackSound(); // Воспроизводим звук атаки
        }
    }

    private void OnAttack()
    {
        // Получаем коллайдеры всех объектов в радиусе атаки для первой маски
        Collider2D[] collidersLayer1 = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy);

        // Получаем коллайдеры всех объектов в радиусе атаки для второй маски
        Collider2D[] collidersLayer2 = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy2);

        // Обрабатываем коллайдеры из первой маски
        ProcessColliders(collidersLayer1);

        // Обрабатываем коллайдеры из второй маски
        ProcessColliders(collidersLayer2);
    }

    private void ProcessColliders(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            var entity = colliders[i].GetComponent<Entity>();
            if (entity != null)
            {
                // Убедимся, что урон не наносится дважды
                if (entity.IsAttackable()) // Если у вашего существа есть этот метод
                {
                    entity.GetDamage(1);
                    Debug.Log("Гоблин получил урон!");
                }
            }
        }
    }

    // Воспроизведение звука прыжка
    private void PlayJumpSound()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Воспроизведение звука приземления
    private void PlayLandSound()
    {
        if (landSound != null && isGrounded)
        {
            audioSource.PlayOneShot(landSound);
        }
    }

    // Воспроизведение звука атаки
    private void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    // Воспроизведение звука бега
    private void PlayRunSound()
    {
        if (runSound != null && isGrounded) // Убедимся, что игрок на земле
        {
            audioSource.clip = runSound; // Устанавливаем звук бега
            if (!audioSource.isPlaying)
            {
                audioSource.Play(); // Воспроизводим звук
            }
        }
    }

    // Воспроизведение звука прыжка от слизня
    private void PlayJumpFromSlimeSound()
    {
        if (jumpFromSlimeSound != null)
        {
            audioSource.PlayOneShot(jumpFromSlimeSound);
        }
    }

    public override void GetDamage(int damage)
    {
        base.GetDamage(damage);
    }

    public override void Die()
    {
        State = States.Dead;
        Debug.Log("Dead");

        if (!deathScreen.activeSelf)
        {
            deathScreen.SetActive(true);
        }
        //base.Die();
    }


    // Проверка завершения атаки
    public bool IsAttackFinished()
    {
        return isAttackFinished;
    }

    private IEnumerator AttackAnimation()
    {
        isAttackFinished = false;
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        isAttackFinished = true;
    }

    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(0.5f);
        isRecharger = true;
    }

    // Метод для вызова из других скриптов
    public void TakeDamage(int damage)
    {
        GetDamage(damage);
    }

    private bool IsOnEnemyLayer()
    {
        // Проверка, стоит ли игрок на слое enemy2 (слизне)
        return Physics2D.OverlapCircle(groundedPoint.position, checkRadius, enemy2);
    }


}
