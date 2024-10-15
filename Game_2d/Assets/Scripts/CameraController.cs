using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    private Vector3 pos;

    public float positionZ = -10f;
    public float positionY = 3f;
    public float speed = 1;

    private void Awake()
    {
        if (!player)
        {
            player = FindObjectOfType<Player>().transform;
        }
    }
    private void Update()
    {
        // Проверка на существование игрока
        if (player != null)
        {
            pos = player.position;
            pos.z = positionZ;
            pos.y += positionY;

            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime + speed);
        }
        else
        {
            // Вы можете добавить логику, если игрока нет
            Debug.LogWarning("Player transform is missing or has been destroyed.");
        }
    }

}