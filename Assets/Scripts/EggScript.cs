using System.Collections;
using UnityEngine;

public class EggScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().HitShip(gameObject);
        }
    }
}
