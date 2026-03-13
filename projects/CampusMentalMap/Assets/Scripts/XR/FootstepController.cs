using UnityEngine;
using UnityEngine.InputSystem;

public class FootstepController : MonoBehaviour
{
    [Header("XR Input")]
    public InputActionProperty moveAction;
    public float moveThreshold = 0.12f;

    [Header("Editor Test (NO VR)")]
    public bool editorKeyboardFallback = true;

    [Header("Debug/Test Switches")]
    public bool ignoreGroundedForTest = true;
    public bool force2DForTest = true;
    public bool debugLogs = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip footstepLoop;
    public float targetVolume = 0.8f;
    public float fadeSpeed = 4f;

    [Header("Upgrades (3/4/5)")]
    [Tooltip("Startet den Loop an einer zufaelligen Stelle, damit es weniger repetitiv klingt.")]
    public bool randomStartOffset = true;

    [Tooltip("Subtile Pitch-Variation beim Start (z.B. 0.97 - 1.03).")]
    public bool randomPitchOnStart = true;
    public Vector2 pitchRange = new Vector2(0.97f, 1.03f);

    [Tooltip("Sehr kurzer Fade-In beim Start (hilft gegen Klicks).")]
    public bool microFadeOnStart = true;
    public float microFadeSeconds = 0.06f; // 40-80ms ist meist gut

    [Header("Ground")]
    public CharacterController characterController;

    float _basePitch = 1f;
    float _startFadeTimer = 0f;
    float _startFadeDuration = 0f;
    float _startFadeTargetVolume = 0f;

    void OnEnable() => moveAction.action?.Enable();
    void OnDisable() => moveAction.action?.Disable();

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0f;
            _basePitch = audioSource.pitch;
        }
    }

    void Update()
    {
        if (!audioSource || !footstepLoop)
        {
            if (debugLogs) Debug.LogWarning("Footsteps: Missing AudioSource or Clip");
            return;
        }

        if (force2DForTest) audioSource.spatialBlend = 0f;

        Vector2 move = Vector2.zero;
        if (moveAction.action != null)
            move = moveAction.action.ReadValue<Vector2>();

#if UNITY_EDITOR
        if (editorKeyboardFallback && move.magnitude < 0.01f)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = (kb.aKey.isPressed ? -1f : 0f) + (kb.dKey.isPressed ? 1f : 0f);
                float y = (kb.sKey.isPressed ? -1f : 0f) + (kb.wKey.isPressed ? 1f : 0f);
                move = new Vector2(x, y);
            }
        }
#endif

        bool isMoving = move.magnitude > moveThreshold;

        bool grounded = ignoreGroundedForTest ||
                        characterController == null ||
                        characterController.isGrounded;

        bool shouldPlay = isMoving && grounded;

        if (debugLogs)
            Debug.Log($"Footsteps: move={move} moving={isMoving} grounded={grounded} vol={audioSource.volume:0.00}");

        // ---------- START ----------
        if (shouldPlay && !audioSource.isPlaying)
        {
            audioSource.clip = footstepLoop;

            // (5) Pitch Variation nur beim Start setzen (subtil)
            if (randomPitchOnStart)
            {
                float pMin = Mathf.Min(pitchRange.x, pitchRange.y);
                float pMax = Mathf.Max(pitchRange.x, pitchRange.y);
                audioSource.pitch = _basePitch * Random.Range(pMin, pMax);
            }
            else
            {
                audioSource.pitch = _basePitch;
            }

            // (4) Random Start Offset (reduziert Wiederholung / hilft auch gegen Loop-Naht-Bemerkbarkeit)
            if (randomStartOffset && footstepLoop.length > 0.1f)
            {
                // Kleine Sicherheitsmarge, damit wir nicht exakt auf das Clip-Ende springen
                float t = Random.Range(0f, Mathf.Max(0f, footstepLoop.length - 0.05f));
                audioSource.time = t;
            }
            else
            {
                audioSource.time = 0f;
            }

            // (3) Micro-Fade beim Start, um Klicks zu vermeiden
            if (microFadeOnStart)
            {
                audioSource.volume = 0f; // Start wirklich bei 0
                _startFadeTimer = 0f;
                _startFadeDuration = Mathf.Max(0.01f, microFadeSeconds);
                _startFadeTargetVolume = targetVolume;
            }

            audioSource.Play();

            if (debugLogs) Debug.Log("Footsteps: PLAY");
        }

        // ---------- VOLUME / FADE ----------
        float desiredVolume = shouldPlay ? targetVolume : 0f;

        // Wenn wir gerade frisch gestartet sind und Micro-Fade aktiv ist:
        if (audioSource.isPlaying && microFadeOnStart && _startFadeTimer < _startFadeDuration)
        {
            _startFadeTimer += Time.deltaTime;
            float k = Mathf.Clamp01(_startFadeTimer / _startFadeDuration);
            // weicher Verlauf (optional): k*k*(3-2k)
            k = k * k * (3f - 2f * k);
            audioSource.volume = Mathf.Lerp(0f, _startFadeTargetVolume, k);
        }
        else
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, desiredVolume, fadeSpeed * Time.deltaTime);
        }

        // ---------- STOP ----------
        if (!shouldPlay && audioSource.isPlaying && audioSource.volume <= 0.01f)
        {
            audioSource.Stop();
            // Pitch wieder zur Basis (optional sauber)
            audioSource.pitch = _basePitch;

            if (debugLogs) Debug.Log("Footsteps: STOP");
        }
    }
}