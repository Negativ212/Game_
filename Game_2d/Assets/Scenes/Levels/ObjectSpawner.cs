using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // Префаб для респавна
    public GameObject objectToRespawn;

    // Точка для респавна объекта
    public Transform respawnPoint;

    // Слой отслеживаемых объектов
    public LayerMask layerToWatch;

    // Счетчик количества объектов с нужным слоем в зоне
    private int objectsInZone = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверка, соответствует ли слой объекта отслеживаемому слою
        if (((1 << other.gameObject.layer) & layerToWatch) != 0)
        {
            // Увеличиваем счетчик объектов в зоне
            objectsInZone++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Проверка, соответствует ли слой объекта отслеживаемому слою
        if (((1 << other.gameObject.layer) & layerToWatch) != 0)
        {
            // Уменьшаем счетчик объектов в зоне
            objectsInZone--;

            // Если в зоне больше нет объектов с нужным слоем, вызываем респавн
            if (objectsInZone <= 0)
            {
                RespawnObject();
            }
        }
    }

    private void RespawnObject()
    {
        // Проверка на наличие префаба и точки респавна
        if (objectToRespawn != null && respawnPoint != null)
        {
            Instantiate(objectToRespawn, respawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Префаб или точка респавна не заданы.");
        }
    }
}
