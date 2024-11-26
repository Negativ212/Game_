using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBoss : Entity
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
    private Player playerPlayer;
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

    private bool isSpecialAttackActive = false; // Флаг для активации специальной атаки
    private bool isInvulnerable = false; // Флаг для неуязвимости
    private bool isSummoning = false; // Флаг, указывающий на состояние Summon

    private float specialAttackCooldown = 30f; // Время между активациями специальной атаки
    private float specialAttackDuration = 5f; // Длительность специальной атаки
    public GameObject miniSlimeTemplate; // Скрытый мини-слайм, настроенный заранее


    public GameObject projectilePrefab; // Префаб снаряда
    public float attackInterval = 9f; // Интервал между атаками
    public float attackDuration = 10f; // Длительность атаки
    public float projectileSpeed = 135f; // Скорость снаряда
    public int damageAttackShoot = 5; // Урон, наносимый игроку
    public LayerMask groundMask;  // Публичное поле для настройки маски земли в Unity Inspector

    private bool isAttackActive;
    // Счетчики для отслеживания пропусков атак
    private int[] attackMissCounts = new int[5]; // Счетчик для каждой атаки
    private int lastAttackType = -1; // Последняя выполненная атака
    private int consecutiveAttackCount = 0; // Количество повторов одной и той же атаки

    public GameObject objectToActivate;
    public GameObject Xyi; // Префаб снаряда
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Transform>();
        playerPlayer = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();
        anim = GetComponent<Animator>();
        currentPatrolPoint = patrolPoint1;

        PlayPatrolSounds();

        if (playerPlayer.CurrentState != Player.States.Dead)
            // Запускаем основную корутину для управления атаками
            StartCoroutine(AttackManagerRoutine());
    }

    void Update()
    {
        if (playerPlayer.CurrentState != Player.States.Dead)
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
                if (isChasing && !isSpecialAttackActive)
                {
                    ChasePlayer();
                }
                else if (!isSpecialAttackActive)
                {
                    Patrol();
                }
            }
        }
    }

    private IEnumerator AttackManagerRoutine()
    {
        while (true)
        {
            if (playerPlayer == null || playerPlayer.CurrentState == Player.States.Dead)
            {
                yield break; // Останавливаем корутину, если игрок мертв
            }

            // Ждем случайный интервал между атаками
            float randomInterval = Random.Range(7f, 10f);
            yield return new WaitForSeconds(randomInterval);

            // Выбираем тип атаки
            int attackType = SelectNextAttackType();

            // Выполняем выбранную атаку в зависимости от типа
            switch (attackType)
            {
                case 0:
                case 1:
                    yield return StartCoroutine(PerformRegularAttack());
                    break;
                case 2:
                    yield return StartCoroutine(PerformSpecialAttack());
                    break;
                case 3:
                case 4:
                    yield return StartCoroutine(PerformSuperAttack());
                    break;
            }

            // Увеличиваем счетчик пропуска для каждой атаки, кроме текущей
            for (int i = 0; i < attackMissCounts.Length; i++)
            {
                if (i != attackType) attackMissCounts[i]++;
                else attackMissCounts[i] = 0; // Сброс, если атака была выполнена
            }
        }
    }

    private int SelectNextAttackType()
    {
        int nextAttackType;

        // Проверка на выполнение атаки, пропущенной 4 раза подряд
        for (int i = 0; i < attackMissCounts.Length; i++)
        {
            if (attackMissCounts[i] >= 4)
            {
                nextAttackType = i;
                consecutiveAttackCount = 0;
                lastAttackType = nextAttackType;
                return nextAttackType;
            }
        }

        // Выбираем случайную атаку с учетом вероятности (2/5 для Shoot/Jump, 1/5 для SpecialAttack)
        float randomValue = Random.value;
        if (randomValue < 0.4f) // 40% вероятность
        {
            nextAttackType = (Random.value < 0.5f) ? 0 : 1; // Выбираем между Shoot и Jump
        }
        else if (randomValue < 0.8f) // Следующие 40% вероятность
        {
            nextAttackType = (Random.value < 0.5f) ? 3 : 4; // Выбираем между SuperAttack итерациями
        }
        else
        {
            nextAttackType = 2; // Остальные 20% вероятность для SpecialAttack
        }

        // Проверяем на двойное повторение той же атаки
        if (nextAttackType == lastAttackType)
        {
            consecutiveAttackCount++;
            if (consecutiveAttackCount >= 2)
            {
                nextAttackType = (nextAttackType + 1) % 5; // Переключаем на следующую атаку, чтобы избежать тройного повторения
                consecutiveAttackCount = 0;
            }
        }
        else
        {
            consecutiveAttackCount = 0; // Сброс счетчика, если атака изменилась
        }

        // Обновляем данные о последней атаке
        lastAttackType = nextAttackType;
        return nextAttackType;
    }

    private IEnumerator PerformSuperAttack()
    {
        Debug.Log("Начало PerformSuperAttack");
        objectToActivate.SetActive(true);
        isSpecialAttackActive = true;
        int attackRepeatCount = 3; // Количество повторов анимации супер-атаки

        for (int i = 0; i < attackRepeatCount; i++)
        {
            Debug.Log($"Прыжок {i + 1} начат");

            anim.SetTrigger("Jump");

            // Ждем начала анимации "Jump" или прерываем ожидание через 2 секунды
            float waitTime = 2f;
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Jump") && waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                yield return null;
            }

            if (waitTime <= 0)
            {
                Debug.Log("Не удалось запустить анимацию 'Jump'");
                break;
            }
            Debug.Log("Анимация 'Jump' началась");

            // Ждем до нормализованного времени 0.5 или прерываем ожидание через 1 секунду
            waitTime = 1f;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f && waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                yield return null;
            }

            if (waitTime <= 0)
            {
                Debug.Log("Не удалось дождаться времени 0.5 в анимации 'Jump'");
                break;
            }
            Debug.Log("Достигнуто время 0.5 в анимации 'Jump'");

            // Спавн префабов
            SpawnSuperAttackProjectiles();

            // Ждем до нормализованного времени 1.0 или прерываем ожидание через 2 секунды
            waitTime = 2f;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                yield return null;
            }

            if (waitTime <= 0)
            {
                Debug.Log("Не удалось дождаться завершения анимации 'Jump'");
                break;
            }
            Debug.Log("Анимация 'Jump' завершена");

            // Пауза перед следующим прыжком
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Атака завершена, FFFFFFF");
        anim.ResetTrigger("Jump");
        objectToActivate.SetActive(false);
        isSpecialAttackActive = false;
    }


    // Метод для спавна префабов в случайных позициях в пределах диапазона
    public void SpawnSuperAttackProjectiles()
    {
        // Определяем количество префабов, которые нужно заспавнить
        int numberOfProjectiles = 3;

        // Задаем диапазоны координат
        float minX = -1935f;
        float maxX = -1895f;
        float minY = 120f;
        float maxY = 130f;

        // Спавним префабы в случайных позициях
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            // Генерируем случайные координаты в заданных диапазонах
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            // Создаем префаб
            Instantiate(Xyi, spawnPosition, Quaternion.identity);
        }
    }



    private IEnumerator PerformRegularAttack()
    {
        objectToActivate.SetActive(true);
        isAttackActive = true;
        anim.SetTrigger("Shoot"); // Запускаем анимацию стрельбы

        yield return new WaitForSeconds(2);
        objectToActivate.SetActive(false);
        // Ждем полного завершения анимации (attackDuration - время на анимацию)
        yield return new WaitForSeconds(attackDuration - 2);

        anim.ResetTrigger("Shoot");
        isAttackActive = false;
    }

    // Эту функцию вызовет Animation Event из анимации
    public void SpawnProjectile()
    {
        // Проверяем, что игрок на сцене
        if (player == null) return;

        Vector3 playerPosition = player.position;
        Vector3 direction = (playerPosition - transform.position).normalized;

        // Позиция спавна снаряда (немного выше позиции босса)
        Vector3 spawnPosition = transform.position + direction * 1.5f;

        // Создаем снаряд
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Инициализируем снаряд с параметрами
        projectile.GetComponent<FallingProjectileGoblin>().Initialize(direction, projectileSpeed, damageAttackShoot, spawnPosition, groundMask);
    }

    private IEnumerator PerformSpecialAttack()
    {
        objectToActivate.SetActive(true);
        isSpecialAttackActive = true;
        anim.SetTrigger("Summon"); // Анимация призыва
        

        Vector2[] spawnPositions = new Vector2[]
        {
        new Vector2(-1944f, 100f),
        new Vector2(-1940f, 104f),
        new Vector2(-1906f, 100f),
        new Vector2(-1902f, 104f)
        };

        yield return new WaitForSeconds(5);
        objectToActivate.SetActive(false);

        foreach (Vector2 position in spawnPositions)
        {
            GameObject projectile = Instantiate(miniSlimeTemplate, position, Quaternion.identity);
            yield return new WaitForSeconds(1f);
        }
        
        anim.ResetTrigger("Summon");
        isSpecialAttackActive = false;
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
        if (recharge >= startRecharge && other.CompareTag("player") && !isDying && lives > 0 && playerPlayer.CurrentState != Player.States.Dead)
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
        anim.SetBool("GobWalk", false);
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

            StopAllSounds();               // Останавливаем все звуки при смерти
            StopAllCoroutines();           // Останавливаем все корутины атаки и призыва

            audioSource.Stop();            // Останавливаем текущий звук
            anim.SetTrigger("GobDead");     // Запускаем анимацию смерти

            Debug.Log("RRRRRRRRRRRRRRRRR");
            StartCoroutine(HandleDeathAfterAnimation());
        }
    }

    private IEnumerator HandleDeathAfterAnimation()
    {
        yield return new WaitForSeconds(1f);
        // Отключаем `AudioSource` и даем задержку в 5 секунд перед удалением
        audioSource.enabled = false;
        yield return new WaitForSeconds(5f);
        Debug.Log("QQQQQQQQQQQQQQQQQQQ");
        Destroy(gameObject); // Удаляем объект
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
