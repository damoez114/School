using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestarter : MonoBehaviour
{
    public void ResetScene()
    {
        Roe.fishCount = 0;
        EnemySpawner.enemyNum = 0;
        GameState.IsPlacing = false;
        RoundManager.initialtries = 3;
        FishStats.damageMult = 1.0f;
        RarityDamageBonusManager.ResetAll();

        // Prevent the Electric Eel's OnDisable/UnfreezeAll from touching
        // enemy objects mid-destroy during scene teardown
        ElectricEelController.PrepareForSceneReload();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}