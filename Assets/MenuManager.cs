using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject escScreen;
    public GameObject optionMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check whether it's active / inactive 
            bool isActive = escScreen.activeSelf;

            escScreen.SetActive(!isActive);
            optionMenu.SetActive(false);
        }
    }
}
