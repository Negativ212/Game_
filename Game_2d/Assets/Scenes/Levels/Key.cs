using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private Door door; // Ссылка на объект двери

    // Радиус проверки для столкновения
    public float checkRadius = 0.5f;

    // Позиция проверки для столкновения
    public Transform checkPoint;

    // Слой, с которым должен взаимодействовать ключ
    public LayerMask playerLayer;

    void Start()
    {
        // Находим объект двери в сцене
        door = FindObjectOfType<Door>();
    }

    void Update()
    {
        // Проверка, находится ли игрок в зоне ключа
        if (IsPlayerNearby())
        {
            // Удаляем ключ из списка в двери
            door.RemoveKey(this);

            // Уничтожаем ключ
            Destroy(gameObject);
        }
    }

    private bool IsPlayerNearby()
    {
        // Используем OverlapCircle для проверки наличия игрока на заданном слое
        return Physics2D.OverlapCircle(checkPoint.position, checkRadius, playerLayer);
    }

    // Визуализация радиуса в редакторе
    private void OnDrawGizmos()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red; // Цвет сферы
            Gizmos.DrawWireSphere(checkPoint.position, checkRadius); // Отображение сферы
        }
    }
}
