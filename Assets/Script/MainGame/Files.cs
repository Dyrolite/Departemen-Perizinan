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

    [Header("Referensi Komponen (Isi di Prefab)")]
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI textNama; 
    public TextMeshProUGUI textAlamat; 

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