using UnityEngine;
using UnityEngine.UI;

public class ReplayLastRoundButton : MonoBehaviour
{
    [SerializeField] private Button replayButton;

    private void Awake()
    {
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);
    }

    private void Update()
    {
        // Grey the button out when there's nothing to replay yet
        // (e.g. very first try of the round, or right after a win/loss).
        if (replayButton != null && RoundPlacementManager.Instance != null)
            replayButton.interactable = RoundPlacementManager.Instance.HasSavedPlacements;
    }

    private void OnReplayClicked()
    {
        if (RoundPlacementManager.Instance == null)
            return;

        RoundPlacementManager.Instance.ReplaySavedPlacements();
    }
}