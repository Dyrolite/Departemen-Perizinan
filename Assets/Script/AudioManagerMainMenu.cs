using UnityEngine;
using UnityEngine.SceneManagement;
public class AudioManagerMainMenu : MonoBehaviour
{
     public GameObject pageVolume;




     void Start()
    {

        pageVolume.SetActive(false);
        if (AudioManager.instance != null) AudioManager.instance.PutarLaguMenu();
        pageVolume.SetActive(false);
    }
    
}
