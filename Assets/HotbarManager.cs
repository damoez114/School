using System.Collections.Generic;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance;

    [SerializeField] private HotbarItemEntry[] allItems;
    [SerializeField] private HotbarSlot[] slotUI;

    [System.Serializable]
    public class HotbarItemEntry
    {
        public string itemID;
        public GameObject prefab;
        public Sprite icon;
        public int cost;
        public RoeItem sourceItem;
        public bool limitOneOnScreen;
    }

    [System.Serializable]
    public class HotbarSaveData
    {
        public string[] itemIDs;
        public float[] growthMultipliers;
        public float[] tripleHookBonuses;
    }

    private int selectedIndex = 0;

    // =========================
    // TRACKER
    // =========================
    public int initialRoeCount;
    public int roeCountInHotbar = 0;
    public static bool roeUsedThisAttempt = false;
    private void Awake()
    {
        Instance = this;
        Debug.Log("HOTBARMANAGER INSTANCE: " + gameObject.name);
    }

    private void Start()
    {
        InitializeFromScene();
        UpdateRoeCount();
        initialRoeCount = roeCountInHotbar;
    }

    // =========================
    // INIT
    // =========================
    private void InitializeFromScene()
    {
        for (int i = 0; i < slotUI.Length; i++)
        {
            HotbarSlot slot = slotUI[i];

            if (slot == null)
                continue;

            Roe roe = slot.GetComponentInChildren<Roe>();

            if (roe != null)
            {
                slot.SetRoe(roe);
            }
        }

        UpdateRoeCount();
    }

    // =========================
    // SLOT SELECTION
    // =========================
    public void SelectSlot(int index)
    {
        if (slotUI == null || slotUI.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, slotUI.Length - 1);
    }

    public HotbarItemEntry GetSelectedRoe()
    {
        if (slotUI == null || selectedIndex >= slotUI.Length || slotUI[selectedIndex] == null)
            return null;

        return FindItemByID(slotUI[selectedIndex].itemID);
    }

    // =========================
    // SET SLOT
    // =========================
    public void SetSlot(int index, HotbarItemEntry roeItem)
    {
        if (slotUI == null || index < 0 || index >= slotUI.Length || slotUI[index] == null)
            return;

        if (roeItem == null || roeItem.prefab == null)
        {
            slotUI[index].Clear();
            UpdateRoeCount();
            return;
        }

        GameObject obj = Instantiate(roeItem.prefab, slotUI[index].transform);
        obj.transform.localPosition = Vector3.zero;

        Roe roe = obj.GetComponent<Roe>();

        if (roe != null)
        {
            roe.itemID = roeItem.itemID;
            roe.SourceItem = roeItem.sourceItem; // pulls the actual RoeItem asset, if linked
        }

        slotUI[index].SetRoe(roe);

        UpdateRoeCount();
    }

    // =========================
    // SAVE
    // =========================
    public void SaveHotbar()
    {
        if (slotUI == null)
            return;

        HotbarSaveData data = new HotbarSaveData();
        data.itemIDs = new string[slotUI.Length];
        data.growthMultipliers = new float[slotUI.Length];
        data.tripleHookBonuses = new float[slotUI.Length];

        Debug.Log("Hotbar saved");

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
            {
                data.itemIDs[i] = "";
                data.growthMultipliers[i] = 1f;
                data.tripleHookBonuses[i] = 0f;
                continue;
            }

            Roe roe = slotUI[i].GetComponentInChildren<Roe>();
            data.itemIDs[i] = roe != null ? roe.itemID : "";
            data.growthMultipliers[i] = roe != null ? roe.GrowthMultiplier : 1f;
            data.tripleHookBonuses[i] = roe != null ? roe.DamageBonus : 0f;
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("HOTBAR_SAVE", json);
        PlayerPrefs.Save();
        UpdateRoeCount();
    }

    // =========================
    // LOAD
    // =========================
    public void LoadHotbar(bool restoreUniqueItems = true)
    {
        if (slotUI == null) return;
        if (!PlayerPrefs.HasKey("HOTBAR_SAVE")) return;

        string json = PlayerPrefs.GetString("HOTBAR_SAVE");
        HotbarSaveData data = JsonUtility.FromJson<HotbarSaveData>(json);
        if (data == null || data.itemIDs == null) return;

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null) continue;
            slotUI[i].Clear();
            if (i >= data.itemIDs.Length) continue;

            string id = data.itemIDs[i];
            if (string.IsNullOrEmpty(id)) continue;

            HotbarItemEntry item = FindItemByID(id);
            if (item == null || item.prefab == null)
            {
                Debug.LogWarning("Missing item: " + id);
                continue;
            }

            if (item.limitOneOnScreen)
            {
                if (!restoreUniqueItems) continue;

                bool alreadyActive =
                    (item.prefab.GetComponent<PufferfishController>() != null && PufferfishController.IsActiveInScene) ||
                    (item.prefab.GetComponent<HalibutController>() != null && HalibutController.IsActiveInScene) ||
                    (item.prefab.GetComponent<ElectricEelController>() != null && ElectricEelController.IsActiveInScene);

                if (alreadyActive) continue;
            }

            GameObject obj = Instantiate(item.prefab, slotUI[i].transform);
            obj.transform.localPosition = Vector3.zero;

            Roe roe = obj.GetComponent<Roe>();
            if (roe != null)
            {
                roe.itemID = item.itemID;
                roe.SourceItem = item.sourceItem;

                if (data.growthMultipliers != null && i < data.growthMultipliers.Length)
                    roe.SetGrowthMultiplier(data.growthMultipliers[i]);
                if (data.tripleHookBonuses != null && i < data.tripleHookBonuses.Length)
                    roe.SetDamageBonus(data.tripleHookBonuses[i]);

                // Freshly (re)created cards always start deployable
                roe.SetPlaced(false);
            }

            slotUI[i].SetRoe(roe);
        }

        UpdateRoeCount();
    }

    // =========================
    // CLEAR
    // =========================
    public void ClearHotbar()
    {
        if (slotUI == null)
            return;

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
                continue;

            slotUI[i].Clear();

            Roe roe = slotUI[i].GetComponentInChildren<Roe>();
            if (roe != null)
            {
                Destroy(roe.gameObject);
            }
        }

    }

    // =========================
    // TRACKER LOGIC
    // =========================
    public void UpdateRoeCount()
    {
        int count = 0;

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
                continue;

            Roe roe = slotUI[i].GetComponentInChildren<Roe>();

            // Only count roe that are still deployable this attempt — a
            // placed (greyed-out) roe is still physically in the hotbar,
            // but it's already been used.
            if (roe != null && !roe.IsPlaced)
                count++;
        }

        roeCountInHotbar = count;
    }

    // Un-greys every roe currently sitting in the hotbar, making them all
    // deployable again. Call this whenever a fresh attempt/round should
    // start (e.g. once the shop is closed).
    public void ResetAllPlacedStates()
    {
        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
                continue;

            Roe roe = slotUI[i].GetComponentInChildren<Roe>();
            if (roe != null)
                roe.SetPlaced(false);
        }

        UpdateRoeCount();
    }

    // =========================
    // FIND ITEM
    // =========================
    private HotbarItemEntry FindItemByID(string id)
    {
        if (allItems == null)
            return null;

        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].itemID == id)
                return allItems[i];
        }

        return null;
    }
    public void resetCount()
    {
        roeCountInHotbar = initialRoeCount;
    }
    public void minusCount()
    {
        roeCountInHotbar--;
    }
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
                continue;

            if (slotUI[i].GetComponentInChildren<Roe>() == null)
                return i;
        }

        return -1; // no space
    }

    // sourceItem is optional — pass the RoeItem asset that's spawning this prefab
    // (e.g. from RoeItem.Purchase, which already has "this" available) so the
    // resulting Roe card knows which asset to pull tooltip data from.
    public void SpawnFromPrefab(int index, GameObject prefab, RoeItem sourceItem = null)
    {
        if (slotUI[index] == null || prefab == null)
        {
            return;
        }
        GameObject obj = Instantiate(prefab, slotUI[index].transform);
        obj.transform.localPosition = Vector3.zero;

        Roe roe = obj.GetComponent<Roe>();
        HotbarItemEntry matchedItem = FindItemByPrefab(prefab);

        if (roe != null)
        {
            roe.itemID = matchedItem != null ? matchedItem.itemID : "";

            // Prefer the explicitly passed sourceItem (from the shop purchase),
            // fall back to whatever's linked on the matched HotbarItemEntry
            roe.SourceItem = sourceItem != null ? sourceItem : (matchedItem != null ? matchedItem.sourceItem : null);
        }

        slotUI[index].SetRoe(roe);

        UpdateRoeCount();
    }
    private HotbarItemEntry FindItemByPrefab(GameObject prefab)
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].prefab == prefab)
                return allItems[i];
        }
        return null;
    }
    public void OnCardDrop(Roe card, HotbarSlot slot)
    {
        if (slot == null || card == null)
            return;

        card.transform.position = slot.transform.position;

        slot.SetRoe(card);
        UpdateRoeCount();
    }
    public int GetCostByID(string id)
    {
        HotbarItemEntry item = FindItemByID(id);
        return item != null ? item.cost : 0;
    }
    // Picks a random entry from allItems that's actually a Roe (has a Roe component on its prefab)
    public HotbarItemEntry GetRandomRoeEntryByRarity(Rarity rarity)
    {
        List<HotbarItemEntry> matching = new List<HotbarItemEntry>();

        for (int i = 0; i < allItems.Length; i++)
        {
            HotbarItemEntry entry = allItems[i];
            if (entry == null || entry.prefab == null) continue;
            if (entry.prefab.GetComponent<Roe>() == null) continue;
            if (entry.sourceItem == null) continue; // needs a linked RoeItem to know its rarity
            if (entry.sourceItem.ItemRarity != rarity) continue;

            matching.Add(entry);
        }

        if (matching.Count == 0) return null;
        return matching[Random.Range(0, matching.Count)];
    }
    public HotbarItemEntry GetRandomRoeEntry()
    {
        List<HotbarItemEntry> roeEntries = new List<HotbarItemEntry>();

        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].prefab != null && allItems[i].prefab.GetComponent<Roe>() != null)
            {
                roeEntries.Add(allItems[i]);
            }
        }

        if (roeEntries.Count == 0)
            return null;

        return roeEntries[Random.Range(0, roeEntries.Count)];
    }
}