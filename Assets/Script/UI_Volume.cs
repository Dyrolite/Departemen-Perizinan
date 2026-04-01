using UnityEngine;
using UnityEngine.UI;

public class UI_Volume : MonoBehaviour
{
    private Slider tuasSlider;

    void Start()
    {
        tuasSlider = GetComponent<Slider>();
        
        // Posisikan tuas sesuai memori HP/PC (default 50% / 0.5)
        if (tuasSlider != null)
        {
            tuasSlider.value = PlayerPrefs.GetFloat("VolumeBGM", 0.5f);
            
            // Perintahkan: Kalau tuas digeser, jalankan fungsi GantiSuara
            tuasSlider.onValueChanged.AddListener(GantiSuara);
        }
    }

    void GantiSuara(float angka)
    {
        // Panggil objek "Dewa" AudioManager tadi buat ganti volumenya
        if (AudioManager.instance != null)
        {
            AudioManager.instance.AturVolumeBGM(angka);
        }
    }
}