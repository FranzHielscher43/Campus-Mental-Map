using UnityEngine;

public class ProximityOutlineGlow : MonoBehaviour
{
    [Header("Verknüpfung (WICHTIG)")]
    public GameObject targetPlane; // Zieh hier dein Info-Panel rein!

    [Header("Farbe & Distanz")]
    public Color glowColor = Color.red; 
    public float maxDistance = 5f;  
    public float minDistance = 1.5f; 

    [Header("Animation")]
    public float pulseSpeed = 2f;

    private Material glowMat;
    private Transform mainCam;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

        if (Camera.main != null) mainCam = Camera.main.transform;

        // Dein Original-Code für den Shader und das schwarze Material
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        
        glowMat = new Material(shader);
        
        // Macht das Objekt schwarz, wie du es wolltest
        glowMat.SetColor("_BaseColor", Color.black); 
        glowMat.SetColor("_Color", Color.black);
        
        glowMat.EnableKeyword("_EMISSION");
        glowMat.renderQueue = 4000; 
        
        renderer.material = glowMat;
    }

    void Update()
    {
        // --- NEU: DER STOPP-SCHALTER ---
        // Wenn das Panel zugewiesen UND gerade sichtbar ist:
        if (targetPlane != null && targetPlane.activeInHierarchy)
        {
            // Glow sofort ausschalten (Schwarz)
            glowMat.SetColor("_EmissionColor", Color.black);
            return; // Update hier abbrechen, damit er nicht weiter rechnet
        }
        // --------------------------------

        if (mainCam == null) return;

        // Ab hier läuft dein originaler Glow-Code weiter, wenn das Panel ZU ist:
        
        float dist = Vector3.Distance(transform.position, mainCam.position);

        float proximityFactor = 1f - Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        float intensity = proximityFactor * (2f + pulse * 3f); 
        Color finalColor = glowColor * intensity;

        glowMat.SetColor("_EmissionColor", finalColor);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}