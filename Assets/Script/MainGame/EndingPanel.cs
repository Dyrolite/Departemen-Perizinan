using UnityEngine;

public class EndingPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject EndingMainmenu;
    public void aktifMainmenu()
    {
        EndingMainmenu.SetActive(true);
    }
}
