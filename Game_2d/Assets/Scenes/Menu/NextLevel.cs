using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public void LoadNextScene()
    {
        // Возвращаем время к нормальному состоянию
        Time.timeScale = 1f;

        // Получаем индекс текущей сцены и прибавляем 1, чтобы перейти к следующей
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Проверяем, существует ли следующая сцена
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Загружаем следующую сцену
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Следующая сцена отсутствует. Возможно, это последняя сцена в списке.");
        }
    }
}
