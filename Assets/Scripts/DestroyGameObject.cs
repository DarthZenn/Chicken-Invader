using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    [SerializeField] private float DestroyDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy();
    }

    void Destroy()
    {
        Vector3 CenterScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2), 0);
        Debug.Log(Vector3.Distance(CenterScreen, transform.position));
        Debug.Log(transform.position);

        if (Vector3.Distance(CenterScreen, transform.position) > DestroyDistance)
        {
            Destroy(this.gameObject);
        }
    }
}
