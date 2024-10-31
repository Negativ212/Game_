using System.Collections;
using UnityEngine;

public class XyiPADAET : MonoBehaviour
{
    public LayerMask playerMask; // Маска игрока
    public int damage = 10;
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
        // Проверка на столкновение с землей по маске groundMask
        else if (other.CompareTag("Player"))
        {
            Destroy(gameObject); // Удаляем снаряд после урона
        }
    }
}
