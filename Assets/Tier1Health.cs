using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private GameObject pauseManager;
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private bool dropsShells = true; // uncheck this on spore prefabs
    [SerializeField] private int shellDropAmount = 1;

    [Header("Status Effects")]
    [SerializeField] private GameObject vulnerableIcon; // small icon next to the health bar, disabled by default
    private const float vulnerableDamageMultiplier = 1.5f;
    private bool isVulnerable = false;

    private FishStats lastHitter;
    private FrogEnemy frog;
    private LilyEnemy lily;
    private SnailEnemy snail;
    private BoulderBoss boulderBoss;
    private ClamEnemy clam;
    private KelpEnemy kelp;
    private bool isDead = false;
    private SquidEnemy squid;
    private AnchorEnemy anchor;
    public bool IsDead => isDead;
    public static event Action<Vector3> OnEnemyDied;
    private HashSet<FishStats> fishThatAlreadyHit = new HashSet<FishStats>();
    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    private void Awake()
    {
        // cache these once instead of calling GetComponent on every single hit
        frog = GetComponent<FrogEnemy>();
        lily = GetComponent<LilyEnemy>();
        snail = GetComponent<SnailEnemy>();
        boulderBoss = GetComponent<BoulderBoss>();
        clam = GetComponent<ClamEnemy>();
        kelp = GetComponent<KelpEnemy>();
        squid = GetComponent<SquidEnemy>();
        anchor = GetComponent<AnchorEnemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameState.IsPlacing)
            return;

        if (isDead)
            return;

        if (frog != null && frog.IsInvulnerable)
            return;

        if (snail != null && snail.IsInvulnerable)
            return;
        if (clam != null && clam.IsInvulnerable) // add
            return;

        FishStats fish = collision.GetComponent<FishStats>();

        if (fish == null)
            return;

        if (fish.GetComponent<PufferfishController>() != null)
            return;

        if (fishThatAlreadyHit.Contains(fish))
            return;

        fishThatAlreadyHit.Add(fish);

        // Stingray handles its own two-hit combo + movement pause; hand off and skip normal flow
        StingrayFish stingray = fish.GetComponent<StingrayFish>();
        if (stingray != null)
        {
            stingray.OnHitEnemy(this);
            return;
        }

        // Snail starts hiding the moment it takes a real hit
        if (snail != null)
            snail.RegisterHit();

        TakeHitFrom(fish);
    }

    // Public so quirky fish (e.g. Stingray) can trigger damage outside the normal per-frame trigger flow
    public void TakeHitFrom(FishStats fish)
    {
        if (isDead || fish == null)
            return;

        lastHitter = fish;

        float damage = fish.damage;
        damage += fish.tripleHook;

        bool dropsShellBonus = lastHitter.isGoldfish;

        ApplyDamage(damage, spawnPopupAt: fish.transform.position, extraShellBonus: dropsShellBonus);

        // Koi grows every 5th hit landed, whether or not it's a killing blow
        KoiFish koi = fish.GetComponent<KoiFish>();
        if (koi != null)
        {
            koi.RegisterHit();
        }

        // Arowana leaves this enemy marked so all future hits deal +50% damage
        ArowanaFish arowana = fish.GetComponent<ArowanaFish>();
        if (arowana != null)
        {
            ApplyVulnerableStatus();
        }
    }

    // Public entry point for non-FishStats damage sources (e.g. Pufferfish radius, traps, hazards)
    public void ApplyDamage(float damage, Vector3 spawnPopupAt, bool extraShellBonus = false)
    {
        if (isDead)
            return;
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(hitSound);
        if (anchor != null)
        {
            damage = 1f;
        }

        if (isVulnerable)
            damage *= vulnerableDamageMultiplier;

        Vector3 spawnPos = spawnPopupAt + Vector3.up * 0.5f;
        GameObject popUp = Instantiate(popUpPrefab, spawnPos, Quaternion.identity);
        popUp.GetComponentInChildren<TMP_Text>().text = "-" + damage * FishStats.damageMult;

        healthManager.TakeDamage(damage);

        if (kelp != null)
        {
            kelp.UpdateSegments(healthManager.healthAmount);
        }

        if (squid != null)
        {
            squid.GoInvisible();
        }

        if (healthManager.healthAmount <= 0)
        {
            isDead = true;

            if (dropsShells)
            {
                if (extraShellBonus)
                {
                    ShellManager.Instance.AddShells(shellDropAmount);
                }

                ShellManager.Instance.AddShells(shellDropAmount);
            }

            OnEnemyDied?.Invoke(transform.position);

            EnemySpawner.enemyNum--;
            Debug.Log("Enemies Remaining " + EnemySpawner.enemyNum);

            if (lily != null)
            {
                lily.Die();
            }
            else if (kelp != null)
            {
                kelp.Die();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        if (boulderBoss != null && healthManager.healthAmount > 0)
        {
            boulderBoss.OnDamaged();
        }
    }

    private void ApplyVulnerableStatus()
    {
        if (isVulnerable)
            return; // already marked, no need to stack or reset

        isVulnerable = true;

        if (vulnerableIcon != null)
            vulnerableIcon.SetActive(true);
    }
}