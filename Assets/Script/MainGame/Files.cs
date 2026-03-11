using UnityEngine;

public class Files : MonoBehaviour
{
    public enum TipeDokumen { SuratPengantar, KTP, IzinUsaha, LaporanKeuangan }
    
    public TipeDokumen tipe;
    public bool isAsli = true;

    // Opsional: Untuk mengubah visual jika dokumen palsu
    public SpriteRenderer spriteRenderer;
    public Color warnaPalsu = Color.red; 

    public void SetupBerkas(TipeDokumen tipeBerkas, bool asli)
    {
        tipe = tipeBerkas;
        isAsli = asli;

        // Visual feedback sederhana untuk prototype: 
        // Dokumen palsu diberi warna kemerahan (nanti bisa diganti dengan teks/sprite beda)
        if (!isAsli && spriteRenderer != null)
        {
            spriteRenderer.color = warnaPalsu;
        }
    }
}