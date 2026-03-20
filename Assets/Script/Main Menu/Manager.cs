using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    
    public void Play()
    {
        Data.hutang = -7500000;
        Data.korup = false;
        SceneManager.LoadScene("MainGame 1");
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
}
