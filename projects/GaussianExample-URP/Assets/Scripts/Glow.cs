using UnityEngine;

public class ProximityOutlineGlow : MonoBehaviour
{
    [Header("Farbe & Distanz")]
    public Color glowColor = Color.red; 
    public float maxDistance = 5f;  // Ab hier fängt er an zu leuchten
    public float minDistance = 1.5f; // Maximale Helligkeit bei dieser Nähe

    [Header("Animation")]
    public float pulseSpeed = 2f;

    private Material glowMat;
    private Transform mainCam;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

        // Kamera finden (Tag "MainCamera" muss gesetzt sein!)
        if (Camera.main != null) mainCam = Camera.main.transform;

        // Wir nutzen den Standard-Shader, aber optimiert für den Glow
        // Falls du URP nutzt, sucht das Skript automatisch den richtigen Shader
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        
        glowMat = new Material(shader);
        
        // Den Würfel selbst schwarz/dunkel machen, damit nur der Glow auffällt
        glowMat.SetColor("_BaseColor", Color.black); 
        glowMat.SetColor("_Color", Color.black);
        
        // Emission (Leuchten) aktivieren
        glowMat.EnableKeyword("_EMISSION");
        glowMat.renderQueue = 4000; // Vor dem Splat anzeigen
        
        renderer.material = glowMat;
    }

    void Update()
    {
        if (mainCam == null) return;

        // 1. Distanz berechnen
        float dist = Vector3.Distance(transform.position, mainCam.position);

        // 2. Faktor berechnen (0 = weit weg, 1 = ganz nah)
        float proximityFactor = 1f - Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));

        // 3. Pulsieren berechnen
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // 4. Intensität bestimmen (wird stärker, je näher man kommt)
        // Wir multiplizieren die Farbe mit einem hohen Wert für den HDR-Glow
        float intensity = proximityFactor * (2f + pulse * 3f); 
        Color finalColor = glowColor * intensity;

        // Farbe zuweisen (Emission erzeugt den Outline-Glow-Effekt)
        glowMat.SetColor("_EmissionColor", finalColor);
    }

    // Hilfskreise im Editor anzeigen
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}
 