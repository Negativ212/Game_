using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform2D : MonoBehaviour
{
    // Список точек для перемещения платформы
    public List<Transform> points;

    // Скорость платформы
    public float speed = 2f;

    // Индекс текущей целевой точки
    private int currentPointIndex = 0;

    void Update()
    {
        if (points.Count == 0) return;

        // Получаем текущую и следующую точки
        Transform targetPoint = points[currentPointIndex];

        // Перемещаем платформу к текущей целевой точке
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Если платформа достигла текущей целевой точки
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Переходим к следующей точке, циклично
            currentPointIndex = (currentPointIndex + 1) % points.Count;
        }
    }

    // Рисуем траекторию в редакторе
    private void OnDrawGizmos()
    {
        if (points == null || points.Count == 0) return;

        Gizmos.color = Color.cyan;

        // Рисуем линии между точками, чтобы видеть траекторию
        for (int i = 0; i < points.Count; i++)
        {
            Transform currentPoint = points[i];
            Transform nextPoint = points[(i + 1) % points.Count];

            if (currentPoint != null && nextPoint != null)
            {
                Gizmos.DrawLine(currentPoint.position, nextPoint.position);
            }
        }
    }
}
