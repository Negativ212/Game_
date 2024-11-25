using UnityEngine;

public class TheEndLVL1 : MonoBehaviour
{
    public LayerMask playerMask;
    public GameObject targetObject;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerMask.value & 1 << other.gameObject.layer) != 0)
        {
            Debug.Log("Коллизия с игроком!");

            // Увеличиваем текущий уровень
            int currentLevel = PlayerPrefs.GetInt("CurrentPlayerLevel", 1);
            currentLevel++;
            PlayerPrefs.SetInt("CurrentPlayerLevel", currentLevel);
            PlayerPrefs.Save();
            Debug.Log($"Новый уровень: {currentLevel}");

            // Активируем целевой объект
            targetObject.SetActive(true);

            // Ставим игру на паузу
            Time.timeScale = 0f;

            // Включаем курсор
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
