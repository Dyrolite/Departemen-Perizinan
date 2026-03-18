using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 

public class Files : MonoBehaviour
{


    public enum TipeDokumen { KTP, NPWP, SKU, AmplopUang }
    
    
    [Header("Status Dokumen Saat Ini")]
    public TipeDokumen tipe;
    public string namaDiBerkas;
    public string alamatDiBerkas;

    [Header("Teks Pelengkap (Boleh Kosong)")]
    public TextMeshProUGUI textTTL;
    public TextMeshProUGUI textJenisKelamin;
    public TextMeshProUGUI textAgama;
    public TextMeshProUGUI textStatus;
    public TextMeshProUGUI textPekerjaan;

    [Header("Referensi Komponen (Isi di Prefab)")]
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI textNama; 
    public TextMeshProUGUI textAlamat;
    public TextMeshProUGUI textKotaAtas;
    public TextMeshProUGUI textKotaBawahFoto; 

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

   public void SetupBerkasDinamis(TipeDokumen tipeBerkas, Sprite gambarDipilih, string namaTeks, string alamatTeks, string ttl, string jk, string agama, string status, string kerja)
    {
        tipe = tipeBerkas;
        namaDiBerkas = namaTeks;
        alamatDiBerkas = alamatTeks;

        if (textKotaAtas && textKotaBawahFoto != null) 
        {
            textKotaAtas.gameObject.SetActive(true);
            textKotaAtas.text = alamatDiBerkas.ToUpper();

            textKotaBawahFoto.gameObject.SetActive(true);
            textKotaBawahFoto.text = alamatDiBerkas.ToUpper(); 
        }

        // Tampilkan kota di Bawah Foto (Otomatis huruf besar semua)
        // if (textKotaBawahFoto != null) 
        // {
        //     textKotaBawahFoto.gameObject.SetActive(true);
        //     textKotaBawahFoto.text = alamatDiBerkas.ToUpper();
        // }

        // Pasang gambarnya (Template KTP/NPWP biasa, atau SKU dengan ttd asli/palsu)
        if (spriteRenderer != null && gambarDipilih != null)
        {
            spriteRenderer.sprite = gambarDipilih;
        }

        // Reset tampilan teks
        if (textNama != null) textNama.gameObject.SetActive(false);
        if (textAlamat != null) textAlamat.gameObject.SetActive(false);

        // Atur teks khusus dokumen resmi (Bukan Amplop)
        if (tipe != TipeDokumen.AmplopUang)
        {
            if (textNama != null)
            {
                textNama.gameObject.SetActive(true);
                textNama.text = namaDiBerkas;
            }

            if (textAlamat != null )
            {
                textAlamat.gameObject.SetActive(true);
                textAlamat.text = alamatDiBerkas;
            }
        }


        if (textTTL != null) textTTL.text = ttl;
        if (textJenisKelamin != null) textJenisKelamin.text = jk;
        if (textAgama != null) textAgama.text = agama;
        if (textStatus != null) textStatus.text = status;
        if (textPekerjaan != null) textPekerjaan.text = kerja;
    }
}