using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    public ConsumableSlot[] slots; // assign 3 in inspector

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].currentItem == null)
                return i;
        }
        return -1;
    }

    public int GetEmptySlotCount()
    {
        int count = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].currentItem == null)
                count++;
        }

        return count;
    }

    public void AddConsumable(ConsumableItem item)
    {
        int index = GetFirstEmptySlot();

        if (index == -1)
        {
            Debug.Log("No empty consumable slots available.");
            return;
        }

        slots[index].SetItem(item);
    }
    void Start()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
    }
}