using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipScripts : MonoBehaviour
{
    [SerializeField] private float Speed;
    [SerializeField] private GameObject[] BulletList;
    [SerializeField] private int CurrentTierBullet;
    [SerializeField] private GameObject DieVFX;
    [SerializeField] private GameObject Shield;
    [SerializeField] private int ChickenLegScore;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisableShield());
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();

    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(x, y, 0);

        transform.position += direction.normalized * Time.deltaTime * Speed;

        Vector3 TopLeftPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, TopLeftPoint.x * -1, TopLeftPoint.x),
            Mathf.Clamp(transform.position.y, TopLeftPoint.y * -1, TopLeftPoint.y));

    }

    void Fire()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(BulletList[CurrentTierBullet], transform.position, Quaternion.identity);
        }
    }

    IEnumerator DisableShield()
    {
        yield return new WaitForSeconds(8);
        
        Shield.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Shield.activeSelf && (collision.CompareTag("Chicken") || collision.CompareTag("Egg")))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("ChickenLeg"))
        {
            Destroy(collision.gameObject);
            ScoreController.instance.GetScore(ChickenLegScore);
        }
    }

    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            var vfx = Instantiate(DieVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
            ShipController.Instance.SpawnShip();
        }
    }
}
