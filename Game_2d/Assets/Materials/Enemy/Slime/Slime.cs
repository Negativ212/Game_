using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Entity
{
    public float speed;
    public int damage;
    public int smallSlimeHealth = 1;

    private Transform player;
    private Animator anim;

    public Transform attackPos;
    public LayerMask playerMask;
    public float radius;

    public float recharge;
    public float startRecharge;

    private bool isFacingRight = false;
    private bool isAttacking = false;
    private bool hasSplit = false;
    private bool recentlySplit = false;
    private bool isTakingDamage = false;
    private bool isDying = false;

    public Transform patrolPoint1; // Первая точка патруля
    public Transform patrolPoint2; // Вторая точка патруля
    public float detectionRange = 5.0f; // Радиус для обнаружения игрока
    public float chaseRange = 10.0f; // Максимальная дистанция для преследования
    public float verticalChaseRange = 2.5f; // Вертикальное расстояние для преследования

    private bool isPatrolling = true; // Флаг патрулирования
    private bool isChasing = false; // Флаг преследования
    private Transform currentPatrolPoint;

    public Transform groundCheck; // Точка проверки земли
    public LayerMask groundLayer; // Слой земли

    // Звуки
    public AudioClip patrolSound; // Звук патрулирования
    public AudioClip attackSound; // Звук удара
    public AudioSource audioSource; // Аудиоджерело

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        currentPatrolPoint = patrolPoint1;
    }

    void Update()
    {
        // Проверка состояния смерти
        if (isDying)
            return;

        // Проверка на смерть
        if (lives <= 0 && !recentlySplit)
        {
            Die();
        }

        // Проверяем, касается ли гоблин земли
        bool grounded = IsGrounded();

        // Проверка на расстояние до игрока
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float verticalDistanceToPlayer = Mathf.Abs(transform.position.y - player.position.y); // Вертикальное расстояние до игрока

            // Проверка, находится ли игрок в пределах радиусов
            if (distanceToPlayer <= detectionRange && verticalDistanceToPlayer <= verticalChaseRange && grounded)
            {
                isChasing = true; // Начинаем преследование
            }
            else if (distanceToPlayer > chaseRange || verticalDistanceToPlayer > verticalChaseRange)
            {
                isChasing = false; // Если игрок далеко, прекращаем преследование
            }

            // Если гоблин преследует игрока
            if (isChasing && grounded)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
    }

    private bool IsGrounded()
    {
        // Проверяем, соприкасается ли точка с землёй
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void Patrol()
    {
        anim.SetBool("GobHitTrigger", true); // Анимация для патрулирования
        transform.position = Vector2.MoveTowards(transform.position, currentPatrolPoint.position, speed * Time.deltaTime);

        // Проверка направления взгляда
        if ((currentPatrolPoint.position.x > transform.position.x && !isFacingRight) ||
            (currentPatrolPoint.position.x < transform.position.x && isFacingRight))
        {
            Flip();
        }

        // Переход к следующей точке сразу после достижения текущей
        if (Vector2.Distance(transform.position, currentPatrolPoint.position) < 0.1f)
        {
            currentPatrolPoint = currentPatrolPoint == patrolPoint1 ? patrolPoint2 : patrolPoint1;
        }

        // Воспроизведение звука патрулирования
        if (!audioSource.isPlaying)
        {
            audioSource.clip = patrolSound;
            audioSource.loop = true; // Зацикливаем звук
            audioSource.Play();
        }
    }

    private void ChasePlayer()
    {
        if (!isAttacking)
        {
            anim.SetBool("GobHitTrigger", true); // Преследование анимации
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            // Проверка направления взгляда
            if ((player.position.x > transform.position.x && !isFacingRight) ||
                (player.position.x < transform.position.x && isFacingRight))
            {
                Flip();
            }

            recharge += Time.deltaTime;
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        // Проверка состояния смерти перед атакой
        if (recharge >= startRecharge && other.CompareTag("player") && !isDying && lives > 0)
        {
            var playerComponent = other.GetComponent<Player>();
            if (playerComponent != null && !isAttacking)
            {
                isAttacking = true;
                anim.SetTrigger("SlimeHitTrigger");
                recharge = 0;
                anim.SetBool("SlimeWalk", false);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            EndAttack();
        }
    }

    public void ApplyDamageToPlayer()
    {
        // Проверка состояния смерти перед нанесением урона
        if (lives > 0)
        {
            Collider2D[] playersInRange = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
            foreach (var playerCollider in playersInRange)
            {
                Player playerComponent = playerCollider.GetComponent<Player>();
                if (playerComponent != null)
                {
                    playerComponent.TakeDamage(damage);
                }
            }
        }
    }

    public void EndAttack()
    {
        // Прерываем атаку, если слайм мертв
        if (isDying) return;

        isAttacking = false;
        anim.SetBool("SlimeHitTrigger", false);
        anim.SetBool("SlimeWalk", true);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    public override void GetDamage(int damage)
    {
        if (!recentlySplit && !isTakingDamage && !isDying)
        {
            isTakingDamage = true;
            base.GetDamage(damage); // Учитываем правильный урон

            if (lives <= 0)
            {
                Die();
            }
            StartCoroutine(ResetDamageCooldown());
        }
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        isTakingDamage = false;
    }

    public override void Die()
    {
        if (!isDying)
        {
            isDying = true; // Устанавливаем состояние смерти
            anim.SetTrigger("SlimeDead"); // Триггер анимации смерти
            anim.SetBool("SlimeWalk", false); // Отключаем движение

            // Убиваем любые текущие анимации атак
            anim.SetBool("SlimeHitTrigger", false); // Отменяем анимацию удара

            // Выполнение действий после анимации смерти
            StartCoroutine(HandleDeathAfterAnimation());
        }
    }

    private IEnumerator HandleDeathAfterAnimation()
    {
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("SlimeDead") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        if (!hasSplit)
        {
            Split();
            hasSplit = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Split()
    {
        Vector3 currentPosition = transform.position;
        Vector3 scale = transform.localScale * 0.5f;

        Vector3[] positions = { currentPosition + new Vector3(-4, 0, 0), currentPosition + new Vector3(4, 0, 0) };

        for (int i = 0; i < 2; i++)
        {
            GameObject newSlime = Instantiate(gameObject, positions[i], Quaternion.identity);
            newSlime.transform.localScale = scale;
            Slime slimeComponent = newSlime.GetComponent<Slime>();
            slimeComponent.lives = smallSlimeHealth;
            slimeComponent.hasSplit = true;
            slimeComponent.StartCoroutine(slimeComponent.ProtectionFromDamage());
        }

        Destroy(gameObject);
    }

    private IEnumerator ProtectionFromDamage()
    {
        recentlySplit = true;
        yield return new WaitForSeconds(0.5f);
        recentlySplit = false;
    }

    private void OnDrawGizmos()
    {
        if (attackPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPos.position, radius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f); // Визуализируем точку проверки земли
        }

        if (patrolPoint1 != null && patrolPoint2 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(patrolPoint1.position, patrolPoint2.position); // Визуализируем патрульные точки
        }
    }

    private void TakeDamage()
    {
        // Проверка, находится ли игрок в зоне атаки
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
        foreach (var playerCollider in playersInRange)
        {
            Player playerComponent = playerCollider.GetComponent<Player>();
            if (playerComponent != null)
            {
                // Воспроизведение звука атаки
                audioSource.clip = attackSound;
                audioSource.loop = false; // Звук удара не должен быть цикличным
                audioSource.Play();
                playerComponent.TakeDamage(damage); // Вызываем метод TakeDamage игрока, нанося ему урон
            }
        }
    }
}