using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheEndLVL1 : MonoBehaviour
{
    public LayerMask playerMask;  // Открытая переменная слоя для игрока
    public GameObject targetObject;  // Объект, который нужно активировать

    public void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerMask.value & 1 << other.gameObject.layer) != 0)
        {
            Debug.Log("Коллизия с игроком!");

            // Активируем целевой объект (например, меню перехода)
            targetObject.SetActive(true);

            // Ставим игру на паузу
            Time.timeScale = 0f;

            // Включаем курсор для взаимодействия с меню
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

        }
    }
}
