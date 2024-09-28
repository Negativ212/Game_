using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    public float speed;
    public int lives;
    public int damage;

    private Transform player;
    private Animator anim;

    public Transform attackPos;
    public LayerMask playerMask;
    public float radius;

    public float recharge;
    public float startRecharge;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Transform>();
        anim = GetComponent<Animator>();

        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        recharge += Time.deltaTime;

        anim.SetBool("GobWalk", true);

        if (lives <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerStay2D (Collider2D other)
    {
        if (recharge >= startRecharge)
        {
            if (other.CompareTag("player"))
            {
                anim.SetBool("GobHit", true);
                recharge = 0;
            }
        }
        else
        {

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (attackPos.position, radius);
    }

    public void OnAttack()
    {
        Collider2D[] playerCollider = Physics2D.OverlapCircleAll(attackPos.position, radius, playerMask);
        anim.SetBool("GobHit", false);
        for (int i = 0; i < playerCollider.Length; i++)
        {
            playerCollider[i].GetComponent<player>().TakeDamage(damage);
        }
    }

    public void TakeDamage (int damage)
    {
        lives -= damage;
    }
}
