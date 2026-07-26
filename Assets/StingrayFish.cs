using System.Collections;
using UnityEngine;

public class StingrayFish : MonoBehaviour
{
    [SerializeField] private float pauseBetweenHits = 0.4f;

    private SpriteMover spriteMover;
    private FishStats fishStats;
    private bool isBusy = false; // true mid-combo, so a second overlapping enemy can't interrupt it

    private void Awake()
    {
        spriteMover = GetComponent<SpriteMover>();
        fishStats = GetComponent<FishStats>();
    }

    // Called by Health.cs the moment this fish's collider first touches an enemy
    public void OnHitEnemy(Health enemyHealth)
    {
        if (isBusy)
            return;

        StartCoroutine(HitSequence(enemyHealth));
    }

    private IEnumerator HitSequence(Health enemyHealth)
    {
        isBusy = true;

        if (spriteMover != null)
            spriteMover.PauseMovement(pauseBetweenHits + 0.05f); // small buffer past the wait below

        if (enemyHealth != null && !enemyHealth.IsDead)
            enemyHealth.TakeHitFrom(fishStats);

        yield return new WaitForSeconds(pauseBetweenHits);

        // re-check in case the first hit (or something else) killed it during the pause
        if (enemyHealth != null && !enemyHealth.IsDead)
            enemyHealth.TakeHitFrom(fishStats);

        isBusy = false;
    }
}