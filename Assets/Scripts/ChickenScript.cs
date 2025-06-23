using System.Collections;
using UnityEngine;

public class ChickenScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            FindObjectOfType<GameManager>().KillChicken(gameObject, other.gameObject);
        }
    }
}
