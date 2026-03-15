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

public class Files : MonoBehaviour
{
    // Sesuaikan dengan daftar berkasmu
    public enum TipeDokumen { KTP, NPWP, SKU, AmplopUang }
    
    // Status benar atau salah
    public enum StatusDokumen { Benar, Salah1, Salah2 }

    [Header("Informasi Berkas Saat Ini")]
    public TipeDokumen tipe;
    public StatusDokumen status;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Fungsi ini akan dipanggil oleh Manager untuk mengatur tipe, status, dan gambarnya
    public void SetupBerkas(TipeDokumen tipeBerkas, StatusDokumen statusBerkas, Sprite gambarBerkas)
    {
        tipe = tipeBerkas;
        status = statusBerkas;

        // Ganti gambar visualnya sesuai dengan gambar yang dikirim dari Manager
        if (spriteRenderer != null && gambarBerkas != null)
        {
            spriteRenderer.sprite = gambarBerkas;
            spriteRenderer.color = Color.white; // Pastikan warnanya normal
        }
    }
}