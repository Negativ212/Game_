using System.Collections;
using UnityEngine;

public class FallingProjectile : MonoBehaviour
{
    public GameObject miniSlimeTemplate; // Настроенный и скрытый мини-слайм
    public LayerMask playerMask; // Маска игрока
    public LayerMask groundMask; // Маска земли
    public int damage = 10;
    public Animator anim;
    private bool hasHitGround = false; // Чтобы не запускать повторные срабатывания

    void Start()
    {
        //anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Падение снаряда
        transform.Translate(Vector3.down * 5f * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {

        // Проверка на столкновение с игроком
        if ((playerMask.value & 1 << other.gameObject.layer) != 0)
        {

            Player playerComponent = other.GetComponent<Player>();
            if (playerComponent != null)
            {

                playerComponent.TakeDamage(damage); // Наносим урон игроку
                Destroy(gameObject); // Удаляем снаряд после урона
            }
        }
        // Проверка на столкновение с землей
        else if (!hasHitGround && (groundMask.value & 1 << other.gameObject.layer) != 0)
        {
            hasHitGround = true;
            StartCoroutine(HandleHitGround());
        }
    }

    private IEnumerator HandleHitGround()
    {
        anim.SetTrigger("Impact"); // Запускаем анимацию при ударе о землю

        // Ожидание завершения анимации
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        if (miniSlimeTemplate != null)
        {
            // Клонируем и активируем мини-слайм
            GameObject miniSlimeClone = Instantiate(miniSlimeTemplate, transform.position, Quaternion.identity);
            miniSlimeClone.SetActive(true);
        }

        Destroy(gameObject); // Удаляем снаряд после спавна
    }
}
