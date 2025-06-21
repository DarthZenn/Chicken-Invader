using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckEggPosition());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CheckEggPosition()
    {
        while (true)
        {
            Vector3 viewPort = Camera.main.WorldToViewportPoint(transform.position);

            if (viewPort.y < 0.05)
            {
                _animator.SetTrigger("break");
                _rb.bodyType = RigidbodyType2D.Static;
                Destroy(gameObject, 1);
                break;
            }

            yield return null;
        }
    }
}
