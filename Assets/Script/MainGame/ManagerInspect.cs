using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManagerInspect : MonoBehaviour
{
    // ==========================================
    // 1. DATABASE BARU (MENGGANTIKAN YANG LAMA)
    // ==========================================
    [System.Serializable]
    public class DataTemplateBerkas
    {
        public Files.TipeDokumen tipeDokumen;
        [Tooltip("Gambar asli (Template kosongan / SKU ttd Walikota Asli)")]
        public Sprite gambarBenar; 
        [Tooltip("Gambar palsu (KHUSUS SKU ttd Walikota Palsu. KTP/NPWP biarkan kosong)")]
        public Sprite gambarSalah; 
    }

    [Header("Database Gambar Berkas")]
    public DataTemplateBerkas[] templateBerkas; 

    [Header("Database Data Teks")]
    public string[] listOrangBenar = { "Dika", "Nafis", "Rajiv", "Dafa" };
    public string[] listNamaSalah = { "Ali", "Budi", "Citra", "Dewi" };
    public string[] listAlamat = { "Jl. Merdeka No. 1", "Jl. Sudirman No. 2", "Jl. Diponegoro No. 3", "Jl. Gatot Subroto No. 4" };

    // ==========================================
    // 2. REFERENSI OBJEK & VARIABEL BAWAANMU
    // ==========================================
    public bool isPenyuapan = false;
    
    [Header("Referensi Objek")]
    public GameObject karakterKlien;
    public GameObject prefabBerkas;
    public Button btnApprove;
    public Button btnReject;
    public GameObject Amplop;
    [SerializeField] private Animator Amplop1anim;
    public bool AmplopKebuka;
    public AmplopBuka scriptAmplopBawah;
    public GameObject PausePanel;

    [Header("Titik Transform")]
    public Transform titikSpawnKlien; // Luar layar kanan
    public Transform titikTengahKlien; // Tengah layar
    public Transform titikMeja; // Posisi dasar dokumen di meja

    [Header("Pengaturan")]
    public float kecepatanGerak = 5f;

    [Header("Titik Spawn Baru")]
    public Transform titikAmplopKedua;

    private Animator AmplopAnim;
    private Collider2D AmplopCol;

    private List<GameObject> berkasDiMeja = new List<GameObject>();
    private bool klienValid; 
    private bool sedangProsesAnimasi = false;
    bool isPaused = false;

    [Tooltip("Masukkan 4 titik kosong untuk posisi akhir masing-masing berkas")]
    public Transform[] titikAkhirBerkas; 

    void Start()
    {
        PausePanel.SetActive(false);
        AmplopAnim = Amplop.GetComponent<Animator>();
        AmplopCol = Amplop.GetComponent<Collider2D>();
        AmplopCol.enabled = false;
        SetTombolAktif(false);
        StartCoroutine(SiklusKlienMasuk());
    }

    IEnumerator SiklusKlienMasuk()
    {
        sedangProsesAnimasi = true;
        SetTombolAktif(false);

        karakterKlien.transform.position = titikSpawnKlien.position;
        yield return StartCoroutine(GerakLerp(karakterKlien, titikSpawnKlien.position, titikTengahKlien.position));

        sedangProsesAnimasi = false;
        SetTombolAktif(true); 
        AmplopAnim.SetTrigger("taruh");
        AmplopCol.enabled = true;
    }

    public void PanggilBerkasDariAmplop()
    {
        StartCoroutine(KeluarkanBerkasAnimasi());
    }

    // ==========================================
    // 3. FUNGSI KELUARKAN BERKAS (SUDAH DI-UPGRADE)
    // ==========================================
    IEnumerator KeluarkanBerkasAnimasi()
    {
        int tipeSkenario = Random.Range(0, 3); // 0=Benar, 1=Salah, 2=Suap
        isPenyuapan = (tipeSkenario == 2);
        klienValid = (tipeSkenario == 0);

        int jumlahBerkasWajib = (tipeSkenario == 0) ? 3 : Random.Range(2, 4); 
        if (jumlahBerkasWajib < 3) klienValid = false; 
        
        int totalBerkasDimunculkan = isPenyuapan ? jumlahBerkasWajib + 1 : jumlahBerkasWajib;

        string namaAsliKlien = listOrangBenar[Random.Range(0, listOrangBenar.Length)];
        string alamatAsliKlien = listAlamat[Random.Range(0, listAlamat.Length)];
        bool dataSudahDibuatSalah = false;

        Files.TipeDokumen[] dokumenWajib = { Files.TipeDokumen.KTP, Files.TipeDokumen.NPWP, Files.TipeDokumen.SKU };

        for (int i = 0; i < totalBerkasDimunculkan; i++)
        {
            GameObject berkasBaru = Instantiate(prefabBerkas, titikAmplopKedua.position, Quaternion.identity);
            Files data = berkasBaru.GetComponent<Files>();

            Files.TipeDokumen tipeYangDibuat;
            string namaDiKertas = namaAsliKlien; 
            string alamatDiKertas = alamatAsliKlien; 
            Sprite templatePilihan = null;

            if (i < jumlahBerkasWajib) 
            {
                tipeYangDibuat = dokumenWajib[i];
                bool jadikanBerkasIniSalah = false;

                if (tipeSkenario != 0 && jumlahBerkasWajib == 3) 
                {
                    if (i > 0 && !dataSudahDibuatSalah && Random.value > 0.4f) jadikanBerkasIniSalah = true;
                    if (i == 2 && !dataSudahDibuatSalah) jadikanBerkasIniSalah = true; 
                }

                // Cari template BENAR dulu sebagai default
                foreach (var db in templateBerkas) {
                    if (db.tipeDokumen == tipeYangDibuat) { templatePilihan = db.gambarBenar; break; }
                }

                if (jadikanBerkasIniSalah)
                {
                    dataSudahDibuatSalah = true;
                    if (tipeYangDibuat == Files.TipeDokumen.SKU)
                    {
                        // KHUSUS SKU: Acak antara Nama yang salah ATAU Tanda Tangan yang salah
                        if (Random.value > 0.5f) {
                            namaDiKertas = listNamaSalah[Random.Range(0, listNamaSalah.Length)];
                        } else {
                            foreach (var db in templateBerkas) {
                                if (db.tipeDokumen == tipeYangDibuat) { templatePilihan = db.gambarSalah; break; }
                            }
                        }
                    }
                    else
                    {
                        // KHUSUS KTP & NPWP: Acak antara Nama salah ATAU Alamat salah
                        if (Random.value > 0.5f) namaDiKertas = listNamaSalah[Random.Range(0, listNamaSalah.Length)];
                        else alamatDiKertas = listAlamat[Random.Range(0, listAlamat.Length)];
                    }
                }
            }
            else 
            {
                // Bagian Amplop Uang Suap
                tipeYangDibuat = Files.TipeDokumen.AmplopUang;
                namaDiKertas = ""; alamatDiKertas = "";
                foreach (var db in templateBerkas) {
                    if (db.tipeDokumen == tipeYangDibuat) { templatePilihan = db.gambarBenar; break; }
                }
            }

            // Memanggil fungsi baru dari script Files
            data.SetupBerkasDinamis(tipeYangDibuat, templatePilihan, namaDiKertas, alamatDiKertas);
            berkasDiMeja.Add(berkasBaru);

            Vector3 posisiAkhir = titikAmplopKedua.position; 
            if (i < titikAkhirBerkas.Length && titikAkhirBerkas[i] != null) posisiAkhir = titikAkhirBerkas[i].position;

            StartCoroutine(GerakMulus(berkasBaru.transform, titikAmplopKedua.position, posisiAkhir));
            yield return new WaitForSeconds(0.2f);
        }

        SetTombolAktif(true);
    }

    // ==========================================
    // 4. KUMPULAN FUNGSI ASLI MILIKMU DI BAWAH INI
    // ==========================================
    IEnumerator GerakMulus(Transform objek, Vector3 mulai, Vector3 target)
    {
        float durasiAnimasi = 0.4f; 
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / durasiAnimasi;
            objek.position = Vector3.Lerp(mulai, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        objek.position = target;
    }

    public void PilihApprove()
    {
        if (sedangProsesAnimasi) return;
        if (!SemuaBerkasTersimpan())
        {
            Debug.Log("PERINGATAN: Rapikan dan masukkan semua dokumen ke amplop terlebih dahulu!");
            return; 
        }
        if (isPenyuapan)
        {
            Debug.Log("KONSEKUENSI: Kamu Menerima Suap! (Berkas salah tapi di-Approve karena ada uang)");
        }
        else if (klienValid)
        {
            Debug.Log("TEPAT: Berkas lengkap dan asli di-Approve.");
        }
        else
        {
            Debug.Log("PELANGGARAN: Ada berkas salah/palsu tapi kamu Approve (Tanpa ada suap)!");
        }
        
        StartCoroutine(SiklusKlienKeluar());
    }

    public void PilihReject()
    {
        if (sedangProsesAnimasi) return;
        if (!SemuaBerkasTersimpan())
        {
            Debug.Log("PERINGATAN: Rapikan dan masukkan semua dokumen ke amplop terlebih dahulu!");
            return; 
        }
        if (isPenyuapan)
        {
            Debug.Log("TEPAT: Kamu menolak suap dari klien berdokumen palsu. Integritas terjaga!");
        }
        else if (!klienValid)
        {
            Debug.Log("TEPAT: Dokumen palsu berhasil di-Reject.");
        }
        else
        {
            Debug.Log("PELANGGARAN: Dokumen lengkap dan asli malah kamu Reject!");
        }

        StartCoroutine(SiklusKlienKeluar());
    }

    IEnumerator SiklusKlienKeluar()
    {
        AmplopAnim.SetTrigger("kembalikan");
        Amplop1anim.SetTrigger("TriggClose");
        AmplopCol.enabled = false;
        AmplopKebuka = false;
        sedangProsesAnimasi = true;
        SetTombolAktif(false);

        if (scriptAmplopBawah != null) 
        {
            scriptAmplopBawah.ResetAmplop(); 
        }

        foreach (GameObject berkas in berkasDiMeja) {
            Destroy(berkas);
        }
        berkasDiMeja.Clear();

        yield return StartCoroutine(GerakLerp(karakterKlien, titikTengahKlien.position, titikSpawnKlien.position));

        yield return new WaitForSeconds(1f);
        StartCoroutine(SiklusKlienMasuk());
    }

    IEnumerator GerakLerp(GameObject objek, Vector3 start, Vector3 end)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * kecepatanGerak;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            objek.transform.position = Vector3.Lerp(start, end, smoothT);
            yield return null;
        }
        objek.transform.position = end;
    }

    void SetTombolAktif(bool aktif)
    {
        btnApprove.interactable = aktif;
        btnReject.interactable = aktif;
    }

    public bool SemuaBerkasTersimpan()
    {
        if (berkasDiMeja.Count == 0) return false;

        foreach (GameObject berkas in berkasDiMeja)
        {
            if (berkas != null)
            {
                DragFile scriptDrag = berkas.GetComponent<DragFile>();

                if (scriptDrag != null && !scriptDrag.isStored)
                {
                    Debug.Log($"Manager: Berkas '{berkas.name}' ternyata BELUM masuk nih!");
                    return false; 
                }
            }
        }
        return true;
    }

    public void CekAnimasiTutupAmplop()
    {
        Debug.Log("Manager: Oke, aku cek semua berkas di meja ya...");

        if (SemuaBerkasTersimpan())
        {
            Debug.Log("Manager: Mantap, SEMUA BERKAS SUDAH MASUK!");

            if (Amplop1anim != null)
            {
                Amplop1anim.SetTrigger("TriggClose");
                AmplopKebuka = false;
                Debug.Log("Manager: Animasi tutup (TriggClose) diputar!");
            }
            else
            {
                Debug.LogError("Manager: Eh tunggu, komponen Animator (Amplop1anim) belum kamu masukin di Inspector!");
            }
        }
        else
        {
            Debug.Log("Manager: Animasi batal diputar karena masih ada berkas di luar.");
        }
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0f; 

        isPaused = true;
        Debug.Log("Game Paused");
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game UnPaused");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}