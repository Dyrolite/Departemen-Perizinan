// using UnityEngine;

// public class Files : MonoBehaviour
// {
//     public enum TipeDokumen { SuratPengantar, KTP, IzinUsaha, LaporanKeuangan }
    
//     public TipeDokumen tipe;
//     public bool isAsli = true;

//     // Opsional: Untuk mengubah visual jika dokumen palsu
//     public SpriteRenderer spriteRenderer;
//     public Color warnaPalsu = Color.red; 

//     public void SetupBerkas(TipeDokumen tipeBerkas, bool asli)
//     {
//         tipe = tipeBerkas;
//         isAsli = asli;

//         // Visual feedback sederhana untuk prototype: 
//         // Dokumen palsu diberi warna kemerahan (nanti bisa diganti dengan teks/sprite beda)
//         if (!isAsli && spriteRenderer != null)
//         {
//             spriteRenderer.color = warnaPalsu;
//         }
//     }
// }


using UnityEngine;
using TMPro;

// 

public class Files : MonoBehaviour
{
    public enum TipeDokumen { KTP, NPWP, SKU, AmplopUang }
    
    [Header("Status Dokumen Saat Ini")]
    public TipeDokumen tipe;
    public string namaDiBerkas;
    public string alamatDiBerkas;

    [Header("Referensi Komponen (Isi di Prefab)")]
    public SpriteRenderer spriteRenderer;
    public TextMeshPro textNama; 
    public TextMeshPro textAlamat; 

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupBerkasDinamis(TipeDokumen tipeBerkas, Sprite gambarDipilih, string namaTeks, string alamatTeks)
    {
        tipe = tipeBerkas;
        namaDiBerkas = namaTeks;
        alamatDiBerkas = alamatTeks;

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

            if (textAlamat != null && (tipe == TipeDokumen.KTP || tipe == TipeDokumen.NPWP))
            {
                textAlamat.gameObject.SetActive(true);
                textAlamat.text = alamatDiBerkas;
            }
        }
    }
}