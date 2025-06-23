using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private int bulletdamage;

    GameManager gm;

    void Awake() => gm = FindObjectOfType<GameManager>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Chicken"))
        {
            gm.KillChicken(other.gameObject, gameObject);
        }
        else if (other.CompareTag("Boss"))
        {
            gm.DamageBoss(bulletdamage);
            gm.RecycleBullet(gameObject);
        }
    }
}
