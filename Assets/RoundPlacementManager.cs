using System.Collections.Generic;
using UnityEngine;

public class RoundPlacementManager : MonoBehaviour
{
    public static RoundPlacementManager Instance { get; private set; }

    public class PlacementRecord
    {
        public string itemID;
        public Vector3 position;
        public Quaternion rotation;
        public float growthMultiplier;
        public float damageBonus;

        // Live reference to the fish spawned for this placement. Used only
        // to read its final rotation/position right before saving — since
        // players can still rotate a fish (FishRotateHandle) after dropping
        // it and before hitting Play. Not persisted anywhere, runtime-only.
        public Transform spawnedFish;
    }

    // Fish placed since the current try started.
    private List<PlacementRecord> currentTryPlacements = new List<PlacementRecord>();

    // The layout that "Replay Last Round" will recreate.
    private List<PlacementRecord> savedRoundPlacements = new List<PlacementRecord>();

    public bool HasSavedPlacements => savedRoundPlacements.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        RoundManager.OnTryEnded += HandleTryEnded;
    }

    private void OnDisable()
    {
        RoundManager.OnTryEnded -= HandleTryEnded;
    }

    // Called from Roe.cs every time a fish is manually dropped onto the field.
    // spawnedFish is the fish's own transform, so its rotation can still be
    // read later even after the player rotates it post-drop.
    public void RecordPlacement(string itemID, Vector3 position, float growthMultiplier, float damageBonus, Transform spawnedFish)
    {
        currentTryPlacements.Add(new PlacementRecord
        {
            itemID = itemID,
            position = position,
            rotation = Quaternion.identity,
            growthMultiplier = growthMultiplier,
            damageBonus = damageBonus,
            spawnedFish = spawnedFish
        });
    }

    // Called from GameManager.StartFish(). Only overwrites the saved layout
    // if something was actually placed this try — otherwise pressing Play
    // right after a Replay (which doesn't call RecordPlacement) would wipe
    // the save with an empty list.
    public void SaveCurrentPlacements()
    {
        if (currentTryPlacements.Count == 0)
            return;

        // Read final position/rotation now — this is the last moment before
        // Play locks movement in, so it reflects any post-drop rotation.
        foreach (PlacementRecord record in currentTryPlacements)
        {
            if (record.spawnedFish != null)
            {
                record.position = record.spawnedFish.position;
                record.rotation = record.spawnedFish.rotation;
            }

        }

        savedRoundPlacements = new List<PlacementRecord>(currentTryPlacements);
    }

    // Finds a matching un-placed Roe card in the hotbar for each saved
    // record and deploys it at the saved position.
    public bool ReplaySavedPlacements()
    {
        if (savedRoundPlacements.Count == 0)
            return false;

        HotbarSlot[] slots = FindObjectsOfType<HotbarSlot>();

        foreach (PlacementRecord record in savedRoundPlacements)
        {
            Roe match = null;

            foreach (HotbarSlot slot in slots)
            {
                if (slot.currentRoe != null &&
                    !slot.currentRoe.IsPlaced &&
                    slot.currentRoe.itemID == record.itemID)
                {
                    match = slot.currentRoe;
                    break;
                }
            }

            if (match != null)
                match.TryAutoDeploy(record.position, record.rotation);
        }

        return true;
    }

    private void HandleTryEnded()
    {
        currentTryPlacements.Clear();
    }

    // Called on round win or loss to fully wipe the memory.
    public void ClearRoundMemory()
    {
        savedRoundPlacements.Clear();
        currentTryPlacements.Clear();
    }
}