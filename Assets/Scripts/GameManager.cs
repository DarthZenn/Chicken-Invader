using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Ship")]
    public Transform ship;
    public float bulletSpeed = 15f;
    public int shipMaxHealth = 20;
    public GameObject shipShield;

    [Header("Object Pools")]
    public ObjectPool bulletPool;
    public ObjectPool eggPool;
    public ObjectPool chickenPool;
    public ObjectPool legPool;
    public ObjectPool vfxPool;

    [Header("Score")]
    public int chickenScore = 100;
    public int legScore = 200;
    public int bossScore = 1000;

    [Header("Boss")]
    public GameObject boss;
    public int bossMaxHealth = 100;

    [Header("Audio")]
    public AudioClip shootClip;
    public AudioClip eggClip;
    public AudioClip shipDeathClip;
    public AudioClip chickenDeathClip;
    public AudioSource sfxSource;

    [Header("UI")]
    public GameObject pauseMenu;
    bool isPaused = false;
    public GameObject gameOverScreen;
    public TMPro.TextMeshProUGUI gameOverText;


    int shipCurrentHealth;
    int bossCurrentHealth;
    Coroutine bossEggRoutine, bossMoveRoutine;

    readonly Dictionary<GameObject, Coroutine> bulletRoutines = new();
    readonly Dictionary<GameObject, Coroutine> eggRoutines = new();
    readonly Dictionary<GameObject, Coroutine> legRoutines = new();

    readonly HashSet<GameObject> activeChickens = new();

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        shipCurrentHealth = shipMaxHealth;
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(DisableShield());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (!isPaused)
            HandleShipInput();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseMenu.SetActive(isPaused);
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void HandleShipInput()
    {
        Vector3 dir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        ship.position += dir.normalized * 5f * Time.deltaTime;

        Vector3 cam = Camera.main.ScreenToWorldPoint(new(Screen.width, Screen.height));
        ship.position = new(Mathf.Clamp(ship.position.x, -cam.x, cam.x),
                            Mathf.Clamp(ship.position.y, -cam.y, cam.y), 0);

        if (Input.GetKeyDown(KeyCode.Space)) Shoot();
    }

    void Shoot()
    {
        if (shootClip != null) sfxSource.PlayOneShot(shootClip);

        GameObject bullet = bulletPool.GetObject();
        bullet.transform.SetPositionAndRotation(ship.position, ship.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = bullet.transform.up * bulletSpeed;
        rb.angularVelocity = 0;

        bulletRoutines[bullet] = StartCoroutine(TrackBullet(bullet));
    }

    IEnumerator TrackBullet(GameObject b)
    {
        while (true)
        {
            Vector3 wp = b.transform.position;
            Vector3 min = Camera.main.ViewportToWorldPoint(new Vector3(-0.2f, -0.2f));
            Vector3 max = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 1.2f));

            if (wp.x < min.x || wp.x > max.x || wp.y < min.y || wp.y > max.y)
            {
                RecycleBullet(b);
                yield break;
            }
            yield return null;
        }
    }

    public void RecycleBullet(GameObject b)
    {
        if (bulletRoutines.TryGetValue(b, out var co))
        {
            StopCoroutine(co);
            bulletRoutines.Remove(b);
        }
        b.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        bulletPool.ReturnObject(b);
    }

    public void SpawnChicken(Vector3 pos)
    {
        GameObject c = chickenPool.GetObject();
        c.transform.position = pos;
        c.transform.SetParent(SpawnerScript.Instance.gridChicken);

        activeChickens.Add(c);
        StartCoroutine(ChickenEggRoutine(c));
    }

    IEnumerator ChickenEggRoutine(GameObject c)
    {
        while (c.activeInHierarchy)
        {
            yield return new WaitForSeconds(Random.Range(10.0f, 20.0f));
            SpawnEgg(c.transform.position);
        }
    }

    public void KillChicken(GameObject chicken, GameObject bullet)
    {
        if (!activeChickens.Remove(chicken)) return;

        if (chickenDeathClip != null)
            sfxSource.PlayOneShot(chickenDeathClip);

        ScoreController.instance.GetScore(chickenScore);
        RecycleBullet(bullet);
        SpawnLeg(chicken.transform.position);
        chickenPool.ReturnObject(chicken);

        if (activeChickens.Count == 0)
            ActivateBoss();
    }


    public void SpawnEgg(Vector3 pos)
    {
        GameObject e = eggPool.GetObject();
        e.transform.position = pos;

        if (eggClip != null) sfxSource.PlayOneShot(eggClip);

        eggRoutines[e] = StartCoroutine(CheckEggFall(e));
    }

    IEnumerator CheckEggFall(GameObject e)
    {
        Rigidbody2D rb = e.GetComponent<Rigidbody2D>();
        Animator an = e.GetComponent<Animator>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        while (true)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(e.transform.position);
            if (vp.y < .05f)
            {
                an.SetTrigger("break");
                rb.bodyType = RigidbodyType2D.Static;
                yield return new WaitForSeconds(1);
                RecycleEgg(e);
                yield break;
            }
            yield return null;
        }
    }

    void RecycleEgg(GameObject e)
    {
        if (eggRoutines.TryGetValue(e, out var co))
        {
            StopCoroutine(co);
            eggRoutines.Remove(e);
        }
        Animator a = e.GetComponent<Animator>();
        a.ResetTrigger("break");
        a.Rebind();
        a.Update(0);

        Rigidbody2D rb = e.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        eggPool.ReturnObject(e);
    }

    void SpawnLeg(Vector3 pos)
    {
        GameObject leg = legPool.GetObject();
        leg.transform.position = pos;
        legRoutines[leg] = StartCoroutine(TrackLeg(leg));
    }

    IEnumerator TrackLeg(GameObject l)
    {
        while (true)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(l.transform.position);
            if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f)
            {
                RecycleLeg(l);
                yield break;
            }
            yield return null;
        }
    }

    void RecycleLeg(GameObject l)
    {
        if (legRoutines.TryGetValue(l, out var co))
        {
            StopCoroutine(co);
            legRoutines.Remove(l);
        }
        legPool.ReturnObject(l);
    }

    public void CollectLeg(GameObject l)
    {
        ScoreController.instance.GetScore(legScore);
        RecycleLeg(l);
    }

    void ActivateBoss()
    {
        boss.SetActive(true);
        bossCurrentHealth = bossMaxHealth;
        bossEggRoutine = StartCoroutine(BossEggRoutine());
        bossMoveRoutine = StartCoroutine(BossMoveRoutine());
    }

    IEnumerator BossEggRoutine()
    {
        while (boss.activeSelf)
        {
            SpawnEgg(boss.transform.position);
            yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        }
    }

    IEnumerator BossMoveRoutine()
    {
        while (boss.activeSelf)
        {
            Vector3 target = RandomWorldPoint();
            while (Vector3.Distance(boss.transform.position, target) > .1f)
            {
                boss.transform.position = Vector3.MoveTowards(boss.transform.position, target, .1f);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(1);
        }
    }

    Vector3 RandomWorldPoint()
    {
        Vector3 v = Camera.main.ViewportToWorldPoint(new(Random.value, Random.Range(.6f, 1f)));
        v.z = 0;
        return v;
    }

    public void DamageBoss(int dmg)
    {
        bossCurrentHealth -= dmg;
        if (bossCurrentHealth <= 0) KillBoss();
    }

    void KillBoss()
    {
        if (bossEggRoutine != null) StopCoroutine(bossEggRoutine);
        if (bossMoveRoutine != null) StopCoroutine(bossMoveRoutine);
        boss.SetActive(false);

        if (chickenDeathClip != null)
            sfxSource.PlayOneShot(chickenDeathClip);

        ScoreController.instance.GetScore(bossScore);

        GameObject fx = vfxPool.GetObject();
        fx.transform.position = boss.transform.position;
        StartCoroutine(ReturnToPoolAfterTime(fx, vfxPool, 1));

        ShowGameOver("w");
    }

    IEnumerator DisableShield()
    {
        yield return new WaitForSeconds(8);
        shipShield.SetActive(false);
    }

    public void HitShip(GameObject attacker)
    {
        if (shipShield.activeSelf) return;

        shipCurrentHealth--;
        if (attacker.CompareTag("Egg")) RecycleEgg(attacker);
        else if (attacker.CompareTag("Chicken")) chickenPool.ReturnObject(attacker);

        if (shipCurrentHealth <= 0) Die();
    }

    void Die()
    {
        if (shipDeathClip != null)
            sfxSource.PlayOneShot(shipDeathClip);

        GameObject fx = vfxPool.GetObject();
        fx.transform.position = ship.position;
        StartCoroutine(ReturnToPoolAfterTime(fx, vfxPool, 1));
        ship.gameObject.SetActive(false);

        ShowGameOver("l");
    }

    IEnumerator ReturnToPoolAfterTime(GameObject obj, ObjectPool pool, float t)
    {
        yield return new WaitForSeconds(t);
        pool.ReturnObject(obj);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
    }

    void ShowGameOver(string result)
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (result == "w")
        {
            gameOverText.text = "<b>YOU WIN!</b>";
            gameOverText.color = Color.green;
        }
        else
        {
            gameOverText.text = "<b>YOU LOSE!</b>";
            gameOverText.color = Color.red;
        }

        gameOverScreen.SetActive(true);
    }



    public void Restart()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

}
