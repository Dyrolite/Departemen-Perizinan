using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Bikin dia abadi & bisa dipanggil dari mana aja

    [Header("Mesin Pemutar")]
    public AudioSource bgmSource;

    [Header("Lagu BGM")]
    public AudioClip laguMainMenu;
    public AudioClip laguGameplay;

    void Awake()
    {
        // JURUS KEABADIAN: Biar nggak dobel pas ganti scene
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Jangan dihancurkan!
        }
        else
        {
            Destroy(gameObject); // Hancurkan kembarannya
            return;
        }
    }

    void Start()
    {
        // Otomatis atur settingan awal
        if (bgmSource != null)
        {
            bgmSource.loop = true; // Biar muter terus
            bgmSource.volume = PlayerPrefs.GetFloat("VolumeBGM", 0.5f); // Ambil memori volume
        }
        
        PutarLaguMenu(); // Langsung putar lagu menu pas game nyala
    }

    // Fungsi untuk ganti volume dari Slider nanti
    public void AturVolumeBGM(float volumeBaru)
    {
        if (bgmSource != null) bgmSource.volume = volumeBaru;
        PlayerPrefs.SetFloat("VolumeBGM", volumeBaru); // Simpan settingan
    }

    // Fungsi ganti lagu
    public void PutarLaguMenu()
    {
        if (bgmSource == null || bgmSource.clip == laguMainMenu) return;
        bgmSource.clip = laguMainMenu;
        bgmSource.Play();
    }

    public void PutarLaguGameplay()
    {
        if (bgmSource == null || bgmSource.clip == laguGameplay) return;
        bgmSource.clip = laguGameplay;
        bgmSource.Play();
    }
}