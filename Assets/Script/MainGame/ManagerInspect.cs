using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ManagerInspect : MonoBehaviour
{
    // ==========================================
    // 1. DATABASE
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

    public enum PilihLevel
    {
        level_1,
        level_2,
        level_3
    }

    [Header("Identitas Scene Ini")]
    [Tooltip("Pilih level untuk scene ini agar kesulitan menyesuaikan otomatis")]
    public PilihLevel level;

    [Header("Database Gambar Berkas")]
    public DataTemplateBerkas[] templateBerkas;

    [Header("Database Data Teks")]
    public string[] listOrangBenar = { "Dika", "Nafis", "Rajiv", "Dafa" };
    public string[] listNamaSalah = { "Ali", "Budi", "Citra", "Dewi" };
    string[] listAlamat =
    {
        "Banjarnegara", "Banyumas", "Batang", "Blora", "Boyolali",
        "Brebes", "Cilacap", "Demak", "Grobogan", "Jepara",
        "Karanganyar", "Kebumen", "Kendal", "Klaten", "Kudus",
        "Magelang", "Pati", "Pekalongan", "Pemalang", "Purbalingga",
        "Purworejo", "Rembang", "Salatiga", "Semarang", "Sragen",
        "Sukoharjo", "Surakarta", "Tegal", "Temanggung", "Wonogiri",
        "Wonosobo"
    };

    [Header("Database Data Pelengkap (Hanya Hiasan)")]
    List<string> listTTL = new List<string>
    {
        "Nganjuk, 14 Februari 1998", "Garut, 31 Desember 2001", "Bojonegoro, 09 September 1999",
        "Medan, 29 Februari 2000", "Makassar, 12 Desember 2012", "Bantul, 01 Januari 2005"
    };
    public List<string> listJenisKelamin;
    public List<string> listAgama;
    public List<string> listStatus;
    public List<string> listPekerjaan;

    private string currentTTL;
    private string currentJenisKelamin;
    private string currentAgama;
    private string currentStatus;
    private string currentPekerjaan;

    public bool isPenyuapan = false;

    [Header("Referensi Objek")]
    public GameObject karakterKlien;
    [Tooltip("Masukkan variasi gambar karakter klien di sini")]
    public List<Sprite> listSpriteKlien; 
    
    private GameObject klienAktif;
    public Button btnApprove;
    public Button btnReject;
    public GameObject Amplop;
    [SerializeField] private Animator Amplop1anim;
    public bool AmplopKebuka;
    public bool IsKamreaAtas = true;
    public bool IsGennerateBaru = false;
    public AmplopBuka scriptAmplopBawah;
    public GameObject PausePanel;
    public GameObject SummaryPanel;
    public GameObject sumContinue;
    public GameObject sumMainMenu;
    public TextMeshProUGUI App;
    public TextMeshProUGUI rejc;
    public TextMeshProUGUI skor;
    public TextMeshProUGUI sisautang;
    public GameObject PetunjukPanel;
    public TextMeshProUGUI Hutang;
    public GameObject EndingPanel;
    public TextMeshProUGUI GoodBadSec;
    public TextMeshProUGUI keterangan1;
    public TextMeshProUGUI keterangan2;

    [Header("Titik Transform")]
    public Transform titikSpawnKlien;
    public Transform titikTengahKlien;
    public Transform titikMeja;
    public Transform titikAmplopKedua;
    public Transform[] titikAkhirBerkas;

    [Header("Pengaturan")]
    public float kecepatanGerak = 5f;
    public int KlienPerLevel;

    private Animator AmplopAnim;
    private Collider2D AmplopCol;
    private GameObject objekUangSuap = null;
    private float scaleKlien;
    private List<GameObject> berkasDiMeja = new List<GameObject>();
    private bool klienValid;
    private bool sedangProsesAnimasi = false;
    private SpriteRenderer klienSpriteRenderer; // <-- Tambahan
    private Sprite spriteKlienTerpilih;
    private bool kunciGambar = false;
    int totalKlien;
    int approveTot;
    int RejectTot;
    public int SkorTot;
    private Animator klienanim;
    bool isPaused = false;

    [Header("Data Usaha Klien")]
    public string[] listNamaUsaha = { "Maju Jaya", "Sejahtera Bersama", "Mundur Terus", "Berkah Abadi" }; // Bisa abang tambah sendiri
    private string currentNamaUsaha;
    private string currentPrefixUsaha;

    void Start()
    {
        totalKlien = 0;
        approveTot = 0;
        RejectTot = 0;
        SkorTot = 0;

        PausePanel.SetActive(false);
        SummaryPanel.SetActive(false);
        PetunjukPanel.SetActive(false);
        EndingPanel.SetActive(false);

        AmplopAnim = Amplop.GetComponent<Animator>();
        AmplopCol = Amplop.GetComponent<Collider2D>();
        klienSpriteRenderer = karakterKlien.GetComponent<SpriteRenderer>();
        AmplopCol.enabled = false;

        SetTombolAktif(false);
        UpdateHutang();
        StartCoroutine(SiklusKlienMasuk());

        if (level == PilihLevel.level_1)
        {
            Time.timeScale = 0f;
            PetunjukPanel.SetActive(true);
        }
    }
    // FUNGSI JURUS PAMUNGKAS
    void LateUpdate()
    {
        // Hanya paksa gambar kalau status kunci sedang aktif (saat di meja)
        if (kunciGambar == true && klienSpriteRenderer != null && spriteKlienTerpilih != null)
        {
            klienSpriteRenderer.sprite = spriteKlienTerpilih;
        }
    }
    IEnumerator SiklusKlienMasuk()
    {
        // 1. Munculkan Klien dari Prefab
        klienAktif = Instantiate(karakterKlien, titikSpawnKlien.position, Quaternion.identity);
        klienanim = klienAktif.GetComponent<Animator>();
        klienSpriteRenderer = klienAktif.GetComponent<SpriteRenderer>();

        // 2. Acak Gambarnya Sejak Awal (LateUpdate akan langsung menjaganya)
        if (listSpriteKlien != null && listSpriteKlien.Count > 0)
        {
            spriteKlienTerpilih = listSpriteKlien[Random.Range(0, listSpriteKlien.Count)];
        }

        totalKlien++;
        IsGennerateBaru = true;
        sedangProsesAnimasi = true;
        SetTombolAktif(false);

        // Arahkan muka ke meja (positif)
        Vector3 skalaMasuk = klienAktif.transform.localScale;
        skalaMasuk.x = Mathf.Abs(skalaMasuk.x);
        klienAktif.transform.localScale = skalaMasuk;

        // 3. Mulai Jalan Masuk (Bentuk masih hitam sesuai animasimu)
        klienanim.SetBool("PopChar", false);
        klienanim.SetBool("walk", true);

        yield return StartCoroutine(GerakLerp(klienAktif, titikSpawnKlien.position, titikTengahKlien.position));
        kunciGambar = true;
        // 4. Sampai di Meja! 
        // walk = false mentrigger animasi memutih (memunculkan karakter)
        klienanim.SetBool("walk", false);
        sedangProsesAnimasi = false;
        SetTombolAktif(true);
        AmplopAnim.SetTrigger("taruh");
        AmplopCol.enabled = true;
    }

    public void PanggilBerkasDariAmplop()
    {
        StartCoroutine(KeluarkanBerkasAnimasi());
    }

    // =========================================================
    // FUNGSI GENERATE BERKAS (DISESUAIKAN BERDASARKAN LEVEL)
    // =========================================================
    IEnumerator KeluarkanBerkasAnimasi()
    {
        // 1. Tentukan Skenario Berdasarkan Level
        int tipeSkenario;
        if (level == PilihLevel.level_1)
        {
            // Level 1: Hanya ada Valid (0) dan Palsu (1). Tidak ada suap.
            tipeSkenario = Random.Range(0, 2);
        }
        else
        {
            // Level 2 & 3: Valid (0), Palsu (1), Suap (2)
            tipeSkenario = Random.Range(0, 3);
        }

        isPenyuapan = (tipeSkenario == 2);
        klienValid = (tipeSkenario == 0);

        // 2. Tentukan Daftar Dokumen Wajib Berdasarkan Level
        List<Files.TipeDokumen> dokumenWajibLevelIni = new List<Files.TipeDokumen>();
        dokumenWajibLevelIni.Add(Files.TipeDokumen.KTP);
        dokumenWajibLevelIni.Add(Files.TipeDokumen.NPWP);
        dokumenWajibLevelIni.Add(Files.TipeDokumen.SKU);


        // NPWP hanya muncul di Level 3

        // 3. Tentukan Jumlah Berkas yang Keluar (Logika Berkas Kurang)
        int jumlahBerkasWajib = dokumenWajibLevelIni.Count;
        if (tipeSkenario != 0)
        {
            // Jika klien bermasalah, ada kemungkinan dia kurang membawa 1 berkas
            if (Random.value > 0.5f)
            {
                jumlahBerkasWajib -= 1;
            }
        }

        // Jika kurang dari syarat wajib, klien otomatis invalid
        if (jumlahBerkasWajib < dokumenWajibLevelIni.Count) klienValid = false;

        int totalBerkasDimunculkan = isPenyuapan ? jumlahBerkasWajib + 1 : jumlahBerkasWajib;

        // 4. Persiapan Data Teks
        string namaAsliKlien = listOrangBenar[Random.Range(0, listOrangBenar.Length)];
        string alamatAsliKlien = listAlamat[Random.Range(0, listAlamat.Length)];

        if (listTTL.Count > 0) currentTTL = listTTL[Random.Range(0, listTTL.Count)];
        if (listJenisKelamin.Count > 0) currentJenisKelamin = listJenisKelamin[Random.Range(0, listJenisKelamin.Count)];
        if (listAgama.Count > 0) currentAgama = listAgama[Random.Range(0, listAgama.Count)];
        if (listStatus.Count > 0) currentStatus = listStatus[Random.Range(0, listStatus.Count)];
        if (listPekerjaan.Count > 0) currentPekerjaan = listPekerjaan[Random.Range(0, listPekerjaan.Count)];

        bool dataSudahDibuatSalah = false;
        objekUangSuap = null;

        // 5. Looping Memunculkan Berkas
        for (int i = 0; i < totalBerkasDimunculkan; i++)
        {
            Files.TipeDokumen tipeYangDibuat;
            string namaDiKertas = namaAsliKlien;
            string alamatDiKertas = alamatAsliKlien;

            string namaUsahaDiKertas = currentNamaUsaha;
            string prefixDiKertas = currentPrefixUsaha;

            GameObject prefabPilihan = null;
            Sprite spriteSalahPilihan = null;

            if (i < jumlahBerkasWajib)
            {
                tipeYangDibuat = dokumenWajibLevelIni[i];
                bool jadikanBerkasIniSalah = false;

                // Jika skenario salah dan dokumen lengkap, pastikan minimal ada 1 dokumen yang isinya salah
                if (tipeSkenario != 0 && jumlahBerkasWajib == dokumenWajibLevelIni.Count)
                {
                    if (i > 0 && !dataSudahDibuatSalah && Random.value > 0.4f) jadikanBerkasIniSalah = true;
                    if (i == jumlahBerkasWajib - 1 && !dataSudahDibuatSalah) jadikanBerkasIniSalah = true; // Paksa dokumen terakhir salah
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
                            // TTD Palsu akan dicari di bawah
                            spriteSalahPilihan = null;
                        }
                    }
                    else // Untuk KTP atau NPWP
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

            // Cari Prefab dari Database
            foreach (var db in templateBerkas)
            {
                if (db.tipeDokumen == tipeYangDibuat)
                {
                    prefabPilihan = db.prefabDokumen;

                    // Ambil TTD palsu khusus jika ini SKU dan diminta disalahkan gambarnya
                    if (dataSudahDibuatSalah && namaDiKertas == namaAsliKlien && tipeYangDibuat == Files.TipeDokumen.SKU)
                    {
                        spriteSalahPilihan = db.gambarSalahAlternatif;
                    }
                    break;
                }
            }

            if (prefabPilihan == null)
            {
                Debug.LogError($"Prefab untuk {tipeYangDibuat} belum diisi di Inspector!");
                continue;
            }

            // Instantiate
            GameObject berkasBaru = Instantiate(prefabPilihan, titikAmplopKedua.position, Quaternion.identity);
            Files data = berkasBaru.GetComponent<Files>();

            if (tipeYangDibuat == Files.TipeDokumen.AmplopUang)
            {
                objekUangSuap = berkasBaru;
            }

            data.SetupBerkasDinamis(tipeYangDibuat, spriteSalahPilihan, namaDiKertas, alamatDiKertas, currentTTL, currentJenisKelamin, currentAgama, currentStatus, currentPekerjaan, namaUsahaDiKertas, prefixDiKertas);
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
    // ==========================================
    // LOGIKA TOMBOL & UPDATE HUTANG
    // ==========================================
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
            if (scriptUang.isStored)
            {
                Debug.Log("TINDAKAN ILEGAL: Uang sudah dikembalikan ke amplop! Jika ingin menolak suap, pilih REJECT.");
                return;
            }
            if (Data.korup == false)
            {
                Data.korup = true;
            }
            SkorTot++;
            Data.hutang += 500000;
            UpdateHutang();
            Debug.Log("KONSEKUENSI: Kamu Menerima Suap! (Uang diambil, Klien di-Approve)");
        }
        else if (klienValid)
        {
            Data.hutang += 350000;
            UpdateHutang();
            SkorTot++;
            Debug.Log("TEPAT: Berkas lengkap dan asli di-Approve.");
        }
        else
        {
            SkorTot--;
            Debug.Log("PELANGGARAN: Ada berkas salah/palsu tapi kamu Approve (Tanpa ada suap)!");
        }
        approveTot++;
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
            if (!scriptUang.isStored)
            {
                Debug.Log("TINDAKAN ILEGAL: Kamu masih menahan uangnya di meja! Jika ingin terima suap, pilih APPROVE.");
                return;
            }
            if (Data.korup == false)
            {
                Data.korup = true;
            }
            Data.hutang += 250000;
            UpdateHutang();
            SkorTot++;
            Debug.Log("TEPAT: Kamu menolak suap dari klien berdokumen palsu. Uang dikembalikan!");
        }
        else if (!klienValid)
        {
            Data.hutang += 250000;
            UpdateHutang();
            SkorTot++;
            Debug.Log("TEPAT: Dokumen palsu berhasil di-Reject.");

        }
        else
        {
            Data.hutang += 100000;
            UpdateHutang();
            SkorTot--;
            Debug.Log("PELANGGARAN: Dokumen lengkap dan asli malah kamu Reject!");
        }
        RejectTot++;
        StartCoroutine(SiklusKlienKeluar());
    }

    private void UpdateHutang()
    {
        if (Hutang == null)
        {
            Debug.LogError("ERROR: Variabel 'Hutang' masih kosong di Inspector! Cek objek ini: " + gameObject.name);
            return;
        }

        if (Data.hutang < 0) Hutang.color = Color.red;
        else Hutang.color = Color.green;

        Hutang.text = "Rp" + Data.hutang.ToString("N0");
    }

    IEnumerator SiklusKlienKeluar()
    {
        AmplopAnim.SetTrigger("kembalikan");
        Amplop1anim.SetTrigger("TriggClose");
        AmplopCol.enabled = false;
        AmplopKebuka = false;
        sedangProsesAnimasi = true;
        SetTombolAktif(false);

        // 1. Trigger animasi Menghilang (putih ke hitam)
        klienanim.SetBool("PopChar", true);

        // Tunggu 0.5 detik agar animasinya selesai menghitam (sesuaikan kalau kurang lama)
        yield return new WaitForSeconds(0.5f);

        // 2. Balik arah karakter ke pintu keluar
        Vector3 skalaKeluar = klienAktif.transform.localScale;
        skalaKeluar.x = -Mathf.Abs(skalaKeluar.x);
        klienAktif.transform.localScale = skalaKeluar;
        
        kunciGambar = false;
        
        // 3. Mulai Jalan Keluar
        klienanim.SetBool("walk", true);

        if (scriptAmplopBawah != null)
        {
            scriptAmplopBawah.ResetAmplop();
        }

        foreach (GameObject berkas in berkasDiMeja)
        {
            Destroy(berkas);
        }
        berkasDiMeja.Clear();

        // 4. Bergerak kembali ke titik luar
        yield return StartCoroutine(GerakLerp(klienAktif, titikTengahKlien.position, titikSpawnKlien.position));

        // 5. HANCURKAN KLIEN! Bersihkan memori untuk klien berikutnya
        Destroy(klienAktif);
        spriteKlienTerpilih = null;

        yield return new WaitForSeconds(1f);
        if (totalKlien >= KlienPerLevel)
        {
            Time.timeScale = 0f;
            Summary();
        }
        else
        {
            StartCoroutine(SiklusKlienMasuk());
        }
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
        if (SemuaBerkasTersimpan())
        {
            if (Amplop1anim != null)
            {
                Amplop1anim.SetTrigger("TriggClose");
                AmplopKebuka = false;
            }
        }
    }

    private void Summary()
    {
        App.text = "APPROVE: " + approveTot;
        rejc.text = "REJECT: " + RejectTot;
        skor.text = "SKOR: " + SkorTot + "/" + totalKlien;
        sisautang.text = "SISA HUTANG: " + Data.hutang.ToString("N0");
        if (level == PilihLevel.level_3)
        {
            SummaryPanel.SetActive(true);
            sumContinue.SetActive(false);
            sumMainMenu.SetActive(false);

            StartCoroutine(EndingPop());
        }
        else
        {
            SummaryPanel.SetActive(true);
        }
    }

    IEnumerator EndingPop()
    {
        yield return new WaitForSecondsRealtime(1f);
        EndingPanel.SetActive(true);
        if (Data.hutang >= 0 && Data.korup == false)
        {
            GoodEnding();
            //tampilkan good ending
        }
        else if (Data.hutang < 0)
        {
            BadEnding();
            //tampilkan bad ending
        }
        else if (Data.hutang >= 0 && Data.korup == true)
        {
            VeryBadEnding();
            //tampilkan secret ending
        }

    }

    private void GoodEnding()
    {
        ColorUtility.TryParseHtmlString("#00FF09", out Color warnahijau);
        GoodBadSec.color = warnahijau;
        GoodBadSec.text = "good";
        keterangan1.text = "Anda berhasil melunasi Hutang" +
            ". Hidup anda sekarang Bahagia";
    }
    private void BadEnding()
    {
        ColorUtility.TryParseHtmlString("#FF0016", out Color warnaMerah);
        GoodBadSec.color = warnaMerah;
        GoodBadSec.text = "bad";
        keterangan1.text = "Anda Gagal Melunasi Hutang" +
            ". Hidup anda penuh kesengsaraan";
    }
    private void VeryBadEnding()
    {
        ColorUtility.TryParseHtmlString("#8E00FF", out Color warnaUngu);
        GoodBadSec.color = warnaUngu;
        GoodBadSec.text = "very Bad";
        keterangan1.text = "Anda dipecat dan dipenjara karena korup" +
            ". perbuatan tak jujur selalu berakhir buruk";
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }

    public void ContinuePtnjk()
    {
        PetunjukPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}