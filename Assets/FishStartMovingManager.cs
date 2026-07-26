using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip startFishSound;

    [SerializeField] private RoundManager roundManager;

    public void StartFish()
    {
        // Already moving — ignore repeated presses until the round resolves
        // and MovementStarted gets reset back to false (which happens once
        // all fish are gone from the field).
        if (DraggableFish.MovementStarted)
            return;

        // Lock in whatever was placed this try as the layout to replay on
        // future tries this round. No-ops if nothing new was placed (e.g.
        // right after hitting Replay Last Round).
        if (RoundPlacementManager.Instance != null)
            RoundPlacementManager.Instance.SaveCurrentPlacements();

        SpriteMover.StartAllFish();
        GameState.IsPlacing = true;
        DraggableFish.SetMovementStarted(true);

        if (roundManager != null)
            roundManager.UseTry();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(startFishSound);
    }

}