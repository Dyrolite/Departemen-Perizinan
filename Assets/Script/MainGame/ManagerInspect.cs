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

        [Tooltip("Masukkan Prefab khusus untuk dokumen ini (KTP/NPWP/SKU/Amplop)")]
        public GameObject prefabDokumen;

        [Tooltip("Opsional: Jika butuh gambar alternatif saat salah (Misal stempel/TTD palsu SKU)")]
        public Sprite gambarSalahAlternatif;
    }

    [Header("Database Gambar Berkas")]
    public DataTemplateBerkas[] templateBerkas;

    [Header("Database Data Teks")]
    public string[] listOrangBenar = { "Dika", "Nafis", "Rajiv", "Dafa" };
    public string[] listNamaSalah = { "Ali", "Budi", "Citra", "Dewi" };
    public string[] listAlamat = { "Jl. Merdeka No. 1", "Jl. Sudirman No. 2", "Jl. Diponegoro No. 3", "Jl. Gatot Subroto No. 4" };

    public bool isPenyuapan = false;
    
    [Header("Referensi Objek")]
    public GameObject karakterKlien;
    public Button btnApprove;
    public Button btnReject;
    public GameObject Amplop;
    [SerializeField] private Animator Amplop1anim;
    public bool AmplopKebuka;
    public bool IsKamreaAtas = true;
    public bool IsGennerateBaru = false;
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
    private GameObject objekUangSuap = null;

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
        IsGennerateBaru = true;
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
        objekUangSuap = null;
        for (int i = 0; i < totalBerkasDimunculkan; i++)
        {
            Files.TipeDokumen tipeYangDibuat;
            string namaDiKertas = namaAsliKlien;
            string alamatDiKertas = alamatAsliKlien;

            // Variabel untuk menampung Prefab yang akan di-spawn
            GameObject prefabPilihan = null;
            Sprite spriteSalahPilihan = null; // Opsional untuk TTD palsu

            if (i < jumlahBerkasWajib)
            {
                tipeYangDibuat = dokumenWajib[i];
                bool jadikanBerkasIniSalah = false;

                if (tipeSkenario != 0 && jumlahBerkasWajib == 3)
                {
                    if (i > 0 && !dataSudahDibuatSalah && Random.value > 0.4f) jadikanBerkasIniSalah = true;
                    if (i == 2 && !dataSudahDibuatSalah) jadikanBerkasIniSalah = true;
                }

                if (jadikanBerkasIniSalah)
                {
                    dataSudahDibuatSalah = true;
                    if (tipeYangDibuat == Files.TipeDokumen.SKU)
                    {
                        if (Random.value > 0.5f)
                        {
                            namaDiKertas = listNamaSalah[Random.Range(0, listNamaSalah.Length)];
                        }
                        else
                        {
                            // Tandai bahwa dokumen ini butuh gambar TTD palsu
                            spriteSalahPilihan = templateBerkas[i].gambarSalahAlternatif;
                        }
                    }
                    else
                    {
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
            }

            // --- PERUBAHAN UTAMA: CARI PREFAB DARI DATABASE ---
            foreach (var db in templateBerkas)
            {
                if (db.tipeDokumen == tipeYangDibuat)
                {
                    prefabPilihan = db.prefabDokumen;

                    // Jika butuh gambar salah alternatif, ambil dari database
                    if (spriteSalahPilihan != null) spriteSalahPilihan = db.gambarSalahAlternatif;

                    break;
                }
            }

            // Keamanan: Jika prefab belum diisi di Inspector, lewati agar tidak error
            if (prefabPilihan == null)
            {
                Debug.LogError($"Prefab untuk {tipeYangDibuat} belum diisi di Inspector!");
                continue;
            }

            // --- INSTANTIATE PREFAB YANG SPESIFIK ---
            GameObject berkasBaru = Instantiate(prefabPilihan, titikAmplopKedua.position, Quaternion.identity);
            Files data = berkasBaru.GetComponent<Files>();
            if (tipeYangDibuat == Files.TipeDokumen.AmplopUang)
            {
                objekUangSuap = berkasBaru;
            }
            // Setup Data (Sekarang kita tidak perlu kirim template "Benar" karena Sprite aslinya sudah nempel di Prefab)
            data.SetupBerkasDinamis(tipeYangDibuat, spriteSalahPilihan, namaDiKertas, alamatDiKertas);

            berkasDiMeja.Add(berkasBaru);

            Vector3 posisiAkhir = titikAmplopKedua.position;
            if (i < titikAkhirBerkas.Length && titikAkhirBerkas[i] != null) posisiAkhir = titikAkhirBerkas[i].position;

            StartCoroutine(GerakMulus(berkasBaru.transform, titikAmplopKedua.position, posisiAkhir));
            yield return new WaitForSeconds(0.2f);
        }

        SetTombolAktif(true);
    }

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
        if (!IsKamreaAtas) return;
        if (!SemuaBerkasTersimpan())
        {
            Debug.Log("PERINGATAN: Rapikan dan masukkan semua dokumen ke amplop terlebih dahulu!");
            return; 
        }
        if (isPenyuapan && objekUangSuap != null)
        {
            DragFile scriptUang = objekUangSuap.GetComponent<DragFile>();

            // Jika kamu klik Approve TAPI uangnya sudah kamu masukkan ke amplop (dikembalikan)
            if (scriptUang.isStored)
            {
                Debug.Log("TINDAKAN ILEGAL: Uang sudah dikembalikan ke amplop! Jika ingin menolak suap, pilih REJECT.");
                return; // Gagalkan klik tombol
            }

            Debug.Log("KONSEKUENSI: Kamu Menerima Suap! (Uang diambil, Klien di-Approve)");
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
        if (!IsKamreaAtas) return;
        if (!SemuaBerkasTersimpan())
        {
            Debug.Log("PERINGATAN: Rapikan dan masukkan semua dokumen ke amplop terlebih dahulu!");
            return; 
        }
        if (isPenyuapan && objekUangSuap != null)
        {
            DragFile scriptUang = objekUangSuap.GetComponent<DragFile>();

            // Jika kamu klik Reject TAPI uangnya masih ada di meja (kamu tahan)
            if (!scriptUang.isStored)
            {
                Debug.Log("TINDAKAN ILEGAL: Kamu masih menahan uangnya di meja! Jika ingin terima suap, pilih APPROVE.");
                return; // Gagalkan klik tombol
            }

            Debug.Log("TEPAT: Kamu menolak suap dari klien berdokumen palsu. Uang dikembalikan!");
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
                // --- KUNCI MEKANIKNYA DI SINI ---
                // Jika berkas yang sedang dicek ini adalah UANG SUAP, abaikan saja!
                if (berkas == objekUangSuap) continue;

                DragFile scriptDrag = berkas.GetComponent<DragFile>();

                if (scriptDrag != null && !scriptDrag.isStored)
                {
                    Debug.Log($"Manager: Dokumen wajib '{berkas.name}' BELUM masuk!");
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