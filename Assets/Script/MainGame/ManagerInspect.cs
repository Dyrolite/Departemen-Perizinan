using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerInspect : MonoBehaviour
{

    [System.Serializable]
    public class DataGambarBerkas
    {
        public Files.TipeDokumen tipeDokumen;
        public Sprite spriteBenar;
        public Sprite spriteSalah1;
        public Sprite spriteSalah2;
    }

    [Header("Database Gambar Berkas")]
    public DataGambarBerkas[] databaseGambar; // Tempat kamu masukin gambar KTP, NPWP, dll di Inspector

    // Variabel Logika Spesifik
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

    // List untuk menampung dokumen yang ada di meja saat ini
    private List<GameObject> berkasDiMeja = new List<GameObject>();
    private bool klienValid; // Status apakah klien ini layak di-approve
    private bool sedangProsesAnimasi = false;

    
    
    [Tooltip("Masukkan 4 titik kosong untuk posisi akhir masing-masing berkas")]
    public Transform[] titikAkhirBerkas; // Menggantikan public Transform titikMeja;

    void Start()
    {
        // Setup tombol
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

        // 1. Pindahkan klien ke titik spawn (kanan)
        karakterKlien.transform.position = titikSpawnKlien.position;

        // 2. Gerakkan klien ke tengah
        yield return StartCoroutine(GerakLerp(karakterKlien, titikSpawnKlien.position, titikTengahKlien.position));

        // 3. Generate dan tampilkan 4 berkas (Dikomendasikan karena dipanggil dari Amplop)
        // GenerateBerkasDiMeja();

        sedangProsesAnimasi = false;
        SetTombolAktif(true); // Tombol langsung aktif (Aman dan anti macet)
        AmplopAnim.SetTrigger("taruh");
        AmplopCol.enabled = true;
    }


    public void PanggilBerkasDariAmplop()
    {
        StartCoroutine(KeluarkanBerkasAnimasi());
    }

    // IEnumerator KeluarkanBerkasAnimasi()
    // {
    //     klienValid = true; 

    //     Files.TipeDokumen[] dokumenWajib = { 
    //         Files.TipeDokumen.SuratPengantar, 
    //         Files.TipeDokumen.KTP, 
    //         Files.TipeDokumen.IzinUsaha, 
    //         Files.TipeDokumen.LaporanKeuangan 
    //     };

    //     int jumlahAkanDibuat = Random.value > 0.9f ? 3 : 4; 
    //     if (jumlahAkanDibuat < 4) klienValid = false;

    //     for (int i = 0; i < jumlahAkanDibuat; i++)
    //     {
    //         // 1. Munculkan berkas di posisi amplop (titik awal)
    //         GameObject berkasBaru = Instantiate(prefabBerkas, titikAmplopKedua.position, Quaternion.identity);
    //         Files data = berkasBaru.GetComponent<Files>();

    //         bool isAsli = Random.value > 0.2f;
    //         if (!isAsli) klienValid = false;

    //         data.SetupBerkas(dokumenWajib[i], isAsli);
    //         berkasDiMeja.Add(berkasBaru);

    //         // 2. Ambil posisi akhir dari Array yang kamu atur di Inspector
    //         Vector3 posisiAkhir = titikAmplopKedua.position; // Default fallback
    //         if (i < titikAkhirBerkas.Length && titikAkhirBerkas[i] != null)
    //         {
    //             posisiAkhir = titikAkhirBerkas[i].position;
    //         }

    //         // 3. Jalankan animasi pergerakan mulus (Lerp)
    //         StartCoroutine(GerakMulus(berkasBaru.transform, titikAmplopKedua.position, posisiAkhir));

    //         // 4. Jeda 0.2 detik sebelum berkas selanjutnya keluar
    //         yield return new WaitForSeconds(0.2f);
    //     }

    //     Debug.Log($"Klien Baru. Status Seharusnya: {(klienValid ? "APPROVE" : "REJECT")}");
    // }


    IEnumerator KeluarkanBerkasAnimasi()
    {
        // Tentukan skenario klien yang datang (0 = Normal, 1 = Salah, 2 = Suap)
        int tipeSkenario = Random.Range(0, 3);
        
        isPenyuapan = (tipeSkenario == 2);
        int jumlahAkanDibuat = isPenyuapan ? 4 : 3;
        
        klienValid = (tipeSkenario == 0); // Klien hanya murni valid jika skenario 0
        bool adaYangSalah = false;

        Files.TipeDokumen[] dokumenWajib = { 
            Files.TipeDokumen.KTP, 
            Files.TipeDokumen.NPWP, 
            Files.TipeDokumen.SKU 
        };

        for (int i = 0; i < jumlahAkanDibuat; i++)
        {
            GameObject berkasBaru = Instantiate(prefabBerkas, titikAmplopKedua.position, Quaternion.identity);
            Files data = berkasBaru.GetComponent<Files>();

            Files.TipeDokumen tipeYangDibuat;
            Files.StatusDokumen statusYangDibuat;
            Sprite spritePilihan = null;

            if (i < 3) 
            {
                // Bagian 3 Berkas Wajib
                tipeYangDibuat = dokumenWajib[i];

                if (tipeSkenario == 0) 
                {
                    // Skenario Normal: Semua pasti BENAR
                    statusYangDibuat = Files.StatusDokumen.Benar;
                } 
                else 
                {
                    // Skenario Salah / Suap: Acak salah satu menjadi palsu
                    statusYangDibuat = Random.value > 0.5f ? Files.StatusDokumen.Benar : (Random.value > 0.5f ? Files.StatusDokumen.Salah1 : Files.StatusDokumen.Salah2);
                    
                    if (statusYangDibuat != Files.StatusDokumen.Benar) adaYangSalah = true;

                    // Paksa berkas terakhir (SKU) jadi salah jika 2 berkas sebelumnya kebetulan benar terus
                    if (i == 2 && !adaYangSalah) statusYangDibuat = Files.StatusDokumen.Salah1;
                }
            }
            else 
            {
                // Bagian Berkas Ke-4 (Amplop Uang)
                tipeYangDibuat = Files.TipeDokumen.AmplopUang;
                statusYangDibuat = Files.StatusDokumen.Benar;
            }

            // --- AMBIL GAMBAR DARI DATABASE ---
            foreach (var db in databaseGambar)
            {
                if (db.tipeDokumen == tipeYangDibuat)
                {
                    if (statusYangDibuat == Files.StatusDokumen.Benar) spritePilihan = db.spriteBenar;
                    else if (statusYangDibuat == Files.StatusDokumen.Salah1) spritePilihan = db.spriteSalah1;
                    else if (statusYangDibuat == Files.StatusDokumen.Salah2) spritePilihan = db.spriteSalah2;
                    break;
                }
            }

            data.SetupBerkas(tipeYangDibuat, statusYangDibuat, spritePilihan);
            berkasDiMeja.Add(berkasBaru);

            // Pergerakan animasi ke meja
            Vector3 posisiAkhir = titikAmplopKedua.position; 
            if (i < titikAkhirBerkas.Length && titikAkhirBerkas[i] != null)
            {
                posisiAkhir = titikAkhirBerkas[i].position;
            }

            StartCoroutine(GerakMulus(berkasBaru.transform, titikAmplopKedua.position, posisiAkhir));
            yield return new WaitForSeconds(0.2f);
        }

        SetTombolAktif(true);
    }

    // Fungsi untuk menggerakkan objek dari A ke B secara mulus
    IEnumerator GerakMulus(Transform objek, Vector3 mulai, Vector3 target)
    {
        float durasiAnimasi = 0.4f; // Kecepatan meluncur, ubah jika kurang cepat
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime / durasiAnimasi;
            // Mathf.SmoothStep membuat gerakan melambat di akhir (tidak kaku)
            objek.position = Vector3.Lerp(mulai, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        objek.position = target;
    }

    // public void PilihApprove()
    // {
    //     if (sedangProsesAnimasi) return;
        
    //     if (klienValid) Debug.Log("KERJA BAGUS: Klien valid di-approve!");
    //     else Debug.Log("PELANGGARAN: Dokumen palsu/kurang tapi di-approve!");
        
    //     StartCoroutine(SiklusKlienKeluar());
    // }

    // public void PilihReject()
    // {
    //     if (sedangProsesAnimasi) return;

    //     if (!klienValid) Debug.Log("KERJA BAGUS: Klien bermasalah di-reject!");
    //     else Debug.Log("PELANGGARAN: Dokumen lengkap dan asli tapi di-reject!");

    //     StartCoroutine(SiklusKlienKeluar());
    // }
public void PilihApprove()
    {
        if (sedangProsesAnimasi) return;

        if (isPenyuapan)
        {
            // KASUS 1: Terima Suap
            Debug.Log("KONSEKUENSI: Kamu Menerima Suap! (Berkas salah tapi di-Approve karena ada uang)");
            // TODO: Panggil fungsi kurangi poin/munculkan surat peringatan di sini
        }
        else if (klienValid)
        {
            // KASUS 2: Normal & Benar
            Debug.Log("TEPAT: Berkas lengkap dan asli di-Approve.");
        }
        else
        {
            // KASUS 3: Ceroboh
            Debug.Log("PELANGGARAN: Ada berkas salah/palsu tapi kamu Approve (Tanpa ada suap)!");
        }
        
        StartCoroutine(SiklusKlienKeluar());
    }

    public void PilihReject()
    {
        if (sedangProsesAnimasi) return;

        if (isPenyuapan)
        {
            // KASUS 4: Menolak Suap
            Debug.Log("TEPAT: Kamu menolak suap dari klien berdokumen palsu. Integritas terjaga!");
        }
        else if (!klienValid)
        {
            // KASUS 5: Menolak Dokumen Salah
            Debug.Log("TEPAT: Dokumen palsu berhasil di-Reject.");
        }
        else
        {
            // KASUS 6: Salah Tolak
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

        // [TAMBAHAN] Reset script amplop bawah agar bisa diklik lagi oleh klien berikutnya!
        if (scriptAmplopBawah != null) 
        {
            scriptAmplopBawah.ResetAmplop(); 
        }

        // 1. Bersihkan berkas dari meja
        foreach (GameObject berkas in berkasDiMeja) {
            Destroy(berkas);
        }
        berkasDiMeja.Clear();

        // 2. Klien keluar ke kanan (Titik Spawn)
        yield return StartCoroutine(GerakLerp(karakterKlien, titikTengahKlien.position, titikSpawnKlien.position));

        // 3. Jeda sejenak, lalu panggil klien berikutnya
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
}