using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerInspect : MonoBehaviour
{
    [Header("Referensi Objek")]
    public GameObject karakterKlien;
    public GameObject prefabBerkas;
    public Button btnApprove;
    public Button btnReject;

    [Header("Titik Transform")]
    public Transform titikSpawnKlien; // Luar layar kanan
    public Transform titikTengahKlien; // Tengah layar
    public Transform titikMeja; // Posisi dasar dokumen di meja

    [Header("Pengaturan")]
    public float kecepatanGerak = 5f;

    // List untuk menampung dokumen yang ada di meja saat ini
    private List<GameObject> berkasDiMeja = new List<GameObject>();
    private bool klienValid; // Status apakah klien ini layak di-approve
    private bool sedangProsesAnimasi = false;

    void Start()
    {
        // Setup tombol

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

        // 3. Generate dan tampilkan 4 berkas
        GenerateBerkasDiMeja();

        sedangProsesAnimasi = false;
        SetTombolAktif(true);
    }

    void GenerateBerkasDiMeja()
    {
        klienValid = true; // Asumsi awal benar, akan berubah jika ada yang salah

        // 4 Macam Berkas yang harus ada
        Files.TipeDokumen[] dokumenWajib = { 
            Files.TipeDokumen.SuratPengantar, 
            Files.TipeDokumen.KTP, 
            Files.TipeDokumen.IzinUsaha, 
            Files.TipeDokumen.LaporanKeuangan 
        };

        // Simulasi kemungkinan klien kurang berkas (10% chance)
        int jumlahAkanDibuat = Random.value > 0.9f ? 3 : 4; 
        if (jumlahAkanDibuat < 4) klienValid = false;

        for (int i = 0; i < jumlahAkanDibuat; i++)
        {
            // Posisi di-offset sedikit agar dokumen menumpuk tapi terlihat
            Vector3 posisiBerkas = titikMeja.position + new Vector3(i * 0.5f, i * -0.2f, 0);
            GameObject berkasBaru = Instantiate(prefabBerkas, posisiBerkas, Quaternion.identity);
            
            Files data = berkasBaru.GetComponent<Files>();

            // Simulasi kemungkinan berkas palsu (20% chance)
            bool isAsli = Random.value > 0.2f;
            if (!isAsli) klienValid = false;

            data.SetupBerkas(dokumenWajib[i], isAsli);
            berkasDiMeja.Add(berkasBaru);
        }

        Debug.Log($"Klien Baru. Status Seharusnya: {(klienValid ? "APPROVE" : "REJECT")}");
    }

    public void PilihApprove()
    {
        if (sedangProsesAnimasi) return;
        
        if (klienValid) Debug.Log("KERJA BAGUS: Klien valid di-approve!");
        else Debug.Log("PELANGGARAN: Dokumen palsu/kurang tapi di-approve!");
        
        StartCoroutine(SiklusKlienKeluar());
    }

    public void PilihReject()
    {
        if (sedangProsesAnimasi) return;

        if (!klienValid) Debug.Log("KERJA BAGUS: Klien bermasalah di-reject!");
        else Debug.Log("PELANGGARAN: Dokumen lengkap dan asli tapi di-reject!");

        StartCoroutine(SiklusKlienKeluar());
    }

    IEnumerator SiklusKlienKeluar()
    {
        sedangProsesAnimasi = true;
        SetTombolAktif(false);

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