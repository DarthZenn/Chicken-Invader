using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenLegScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().CollectLeg(gameObject);
        }
    }
}
