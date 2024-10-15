using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Entity
{
    public float speed;
    public int damage;

    public Transform patrolPoint1; // Первая точка патруля
    public Transform patrolPoint2; // Вторая точка патруля
    public float detectionRange = 5.0f; // Радиус для обнаружения игрока
    public float chaseRange = 10.0f; // Максимальная дистанция для преследования
    public float verticalChaseRange = 2.5f; // Вертикальное расстояние для преследования
    public Transform groundCheck; // Точка проверки земли
    public LayerMask groundLayer; // Слой земли

    private Transform currentPatrolPoint;

    private Animator anim;
    private Transform player;

    public Transform attackPos;
    public LayerMask playerMask;
    public float radius;

    public float recharge;
    public float startRecharge;

    private bool isFacingRight = false;
    private bool isAttacking = false;
    private bool isDying = false; // Новый флаг для состояния смерти
    private bool isPatrolling = true; // Флаг патрулирования
    private bool isChasing = false; // Флаг преследования

    public AudioSource audioSource;
    public AudioClip[] patrolSounds; // Звуки для патруля
    public AudioClip chaseSound;     // Звук для преследования
    public AudioClip attackSound;    // Звук для атаки
    public AudioClip deathSound;     // Звук для смерти

    private Coroutine patrolSoundCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        currentPatrolPoint = patrolPoint1;

        PlayPatrolSounds();
    }

    void Update()
    {
        if (isDying)
            return;

        if (lives <= 0)
        {
            Die();
            return;
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
                StartChase();
            }
            else if (distanceToPlayer > chaseRange || verticalDistanceToPlayer > verticalChaseRange)
            {
                StopChase();
            }

            // Если гоблин преследует игрока
            if (isChasing)
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
    }

    private void StartChase()
    {
        if (!isChasing)
        {
            StopAllSounds(); // Останавливаем все текущие звуки
            isChasing = true; // Начинаем преследование
            PlayChaseSound(); // Запускаем звук преследования
        }
    }

    private void StopChase()
    {
        if (isChasing)
        {
            isChasing = false; // Прекращаем преследование
            StopChaseSound(); // Останавливаем звук преследования
            PlayPatrolSounds(); // Возвращаем звуки патруля
        }
    }

    private void PlayPatrolSounds()
    {
        if (patrolSoundCoroutine != null)
            StopCoroutine(patrolSoundCoroutine);

        patrolSoundCoroutine = StartCoroutine(PlayRandomPatrolSound());
    }

    private IEnumerator PlayRandomPatrolSound()
    {
        while (isPatrolling)
        {
            // Используйте UnityEngine.Random вместо просто Random
            int randomIndex = UnityEngine.Random.Range(0, patrolSounds.Length);

            audioSource.clip = patrolSounds[randomIndex];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
    }

    private void StopAllSounds()
    {
        // Останавливаем все звуки
        audioSource.Stop();
        if (patrolSoundCoroutine != null)
        {
            StopCoroutine(patrolSoundCoroutine);
        }
    }

    private void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void PlayChaseSound()
    {
        audioSource.clip = chaseSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopChaseSound()
    {
        audioSource.loop = false;
        audioSource.Stop();
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
        if (recharge >= startRecharge && other.CompareTag("player") && !isDying && lives > 0)
        {
            var playerComponent = other.GetComponent<Player>();
            if (playerComponent != null && !isAttacking)
            {
                isAttacking = true;
                anim.SetTrigger("GobWalk");
                StopAllSounds(); // Останавливаем все звуки перед атакой
                PlayAttackSound(); // Звук атаки
                recharge = 0;
                anim.SetBool("GobHitTrigger", false);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            EndAttack();
            StopChase(); // Останавливаем преследование, когда игрок покидает триггер
        }
    }

    public void ApplyDamageToPlayer()
    {
        if (lives > 0)
        {
            Collider2D[] playersInRange = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
            foreach (var playerCollider in playersInRange)
            {
                Player playerComponent = playerCollider.GetComponent<Player>();
                if (playerComponent != null)
                {
                    PlayAttackSound(); // Звук при атаке
                    playerComponent.TakeDamage(damage);
                }
            }
        }
    }

    public void EndAttack()
    {
        if (isDying) return;

        isAttacking = false;
        anim.SetBool("GobHitTrigger", true);
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
        base.GetDamage(damage);
        if (lives <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        if (!isDying)
        {
            isDying = true;
            StopAllSounds(); // Останавливаем все звуки при смерти
            audioSource.clip = deathSound; // Устанавливаем звук смерти
            audioSource.loop = false; // Смерть не должна быть цикличной
            audioSource.Play(); // Играем звук смерти
            anim.SetTrigger("GobDead");
            anim.SetBool("GobHitTrigger", false);
            StartCoroutine(HandleDeathAfterAnimation());
        }
    }

    private IEnumerator HandleDeathAfterAnimation()
    {
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("GobDead") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        Destroy(gameObject);
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
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
        foreach (var playerCollider in playersInRange)
        {
            Player playerComponent = playerCollider.GetComponent<Player>();
            if (playerComponent != null)
            {
                PlayAttackSound(); // Звук при атаке
                playerComponent.TakeDamage(damage);
            }
        }
    }
}
