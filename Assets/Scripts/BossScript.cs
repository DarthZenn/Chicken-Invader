using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    [SerializeField] private GameObject EggPrefab;
    [SerializeField] private int health = 100;
    [SerializeField] private GameObject VFX;

    public static BossScript instance;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEgg());
        StartCoroutine(MoveBossToRandomPoint());
    }

    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
            var vfx = Instantiate(VFX, transform.position, Quaternion.identity);
            Destroy(vfx, 1);
        }
    }

    IEnumerator SpawnEgg()
    {
        while (true)
        {
            Instantiate(EggPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0.0f, 1.0f));
        }
    }

    IEnumerator MoveBossToRandomPoint()
    {
        Vector3 point = GetRandomPoint();

        while (transform.position != point)
        {
            transform.position = Vector3.MoveTowards(transform.position, point, 0.1f);

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

        StartCoroutine(MoveBossToRandomPoint());
    }

    Vector3 GetRandomPoint()
    {
        Vector3 posRandom = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.5f, 1.0f)));
        posRandom.z = 0;
        return posRandom;
    }
}
