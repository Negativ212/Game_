using UnityEngine;

public class FallingProjectileGoblin : MonoBehaviour
{
    public float speed;
    private Vector3 direction;
    private int damage;
    private LineRenderer lineRenderer;
    private LayerMask groundMask;

    // Инициализация снаряда
    public void Initialize(Vector3 dir, float projectileSpeed, int damageValue, Vector3 spawnPosition, LayerMask groundMaskLayer)
    {
        direction = dir;
        speed = projectileSpeed;
        damage = damageValue;
        groundMask = groundMaskLayer;

        // Устанавливаем позицию спавна
        transform.position = spawnPosition;

        // Создаем LineRenderer и настраиваем его
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // Определяем конечную точку луча
        RaycastHit2D hit = Physics2D.Raycast(spawnPosition, direction, Mathf.Infinity, groundMask);
        Vector3 endPoint = hit.collider != null ? (Vector3)hit.point : spawnPosition + direction * 50f;

        // Устанавливаем начальную и конечную точки линии
        lineRenderer.SetPosition(0, spawnPosition);  // Начало линии — позиция босса при спавне
        lineRenderer.SetPosition(1, endPoint);       // Конец линии — точка пересечения с землёй
    }

    void Update()
    {
        // Движение снаряда по направлению
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collided with: " + other.gameObject.name);

        // Проверка на столкновение с игроком
        if (other.CompareTag("player"))
        {
            other.GetComponent<Player>().TakeDamage(damage); // Наносим урон игроку
            DestroyProjectile(); // Удаляем снаряд и линию
        }
        // Проверка на столкновение с землей по маске groundMask
        else if (other.CompareTag("Player"))
        {
            DestroyProjectile(); // Удаляем снаряд и линию при столкновении с землей
        }
    }


    // Метод для уничтожения снаряда и очистки линии
    private void DestroyProjectile()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject); // Удаление LineRenderer
        }
        Destroy(gameObject); // Удаление самого снаряда
    }
}
