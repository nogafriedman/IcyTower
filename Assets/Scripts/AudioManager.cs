using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    [Header("Jump SFX")]
    public AudioClip jumpLow;
    public AudioClip jumpMid;
    public AudioClip jumpHigh;

    
    public enum ComboTier { None, Good, Sweet, Great, Super, Wow, Amazing, Extreme, Fantastic, Splendid, NoWay }

    [Header("Combo Tier SFX")]
    [SerializeField] private AudioClip sfxGood;
    [SerializeField] private AudioClip sfxSweet;
    [SerializeField] private AudioClip sfxGreat;
    [SerializeField] private AudioClip sfxSuper;
    [SerializeField] private AudioClip sfxWow;
    [SerializeField] private AudioClip sfxAmazing;
    [SerializeField] private AudioClip sfxExtreme;
    [SerializeField] private AudioClip sfxFantastic;
    [SerializeField] private AudioClip sfxSplendid;
    [SerializeField] private AudioClip sfxNoWay;

    [Header("Other SFX")]

    [SerializeField] private AudioClip sfxMilestone;

    private AudioSource _a, _b;
    private ComboTier _lastTier = ComboTier.None;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();
        _a.playOnAwake = _b.playOnAwake = false;
        _a.spatialBlend = _b.spatialBlend = 0f;
    }

    // Combo Sounds
    public void OnComboFloorsProgress(int floorsInCombo)
    {
        ComboTier tier = DetermineTier(floorsInCombo);
        if (tier > _lastTier) // play only when entering a higher tier
        {
            PlayTierSfx(tier);
            _lastTier = tier;
        }
    }

    public void ResetComboTierProgress() => _lastTier = ComboTier.None;

    public void PlayMilestone() => Play(sfxMilestone);

    private static ComboTier DetermineTier(int floors)
    {
        if (floors >= 200) return ComboTier.NoWay;
        if (floors >= 140) return ComboTier.Splendid;
        if (floors >= 100) return ComboTier.Fantastic;
        if (floors >= 70) return ComboTier.Extreme;
        if (floors >= 50) return ComboTier.Amazing;
        if (floors >= 35) return ComboTier.Wow;
        if (floors >= 25) return ComboTier.Super;
        if (floors >= 15) return ComboTier.Great;
        if (floors >= 7) return ComboTier.Sweet;
        if (floors >= 4) return ComboTier.Good;
        return ComboTier.None;
    }

    private void PlayTierSfx(ComboTier tier)
    {
        switch (tier)
        {
            case ComboTier.Good:       Play(sfxGood); break;
            case ComboTier.Sweet:      Play(sfxSweet); break;
            case ComboTier.Great:      Play(sfxGreat); break;
            case ComboTier.Super:      Play(sfxSuper); break;
            case ComboTier.Wow:        Play(sfxWow); break;
            case ComboTier.Amazing:    Play(sfxAmazing); break;
            case ComboTier.Extreme:    Play(sfxExtreme); break;
            case ComboTier.Fantastic:  Play(sfxFantastic); break;
            case ComboTier.Splendid:   Play(sfxSplendid); break;
            case ComboTier.NoWay:      Play(sfxNoWay); break;
        }
    }

    // Jump Sounds
    // public void PlayJumpByForce(float totalForce, float baseForce, float highRefForce)
    // {
    //     if (!jumpLow && !jumpMid && !jumpHigh) return;

    //     // Normalize how "strong" the jump is in [0,1], using base as 0 and highRef as 1
    //     float t = 0f;
    //     if (highRefForce > baseForce)
    //         t = Mathf.InverseLerp(baseForce, highRefForce, totalForce);

    //     // Thresholds—tweak to taste
    //     if (t >= 0.85f) Play(jumpHigh);
    //     else if (t >= 0.35f) Play(jumpMid);
    //     else Play(jumpLow);
    // }
    public void PlayJumpByForce(float totalForce, float baseForce, float highRefForce)
    {
        float midThreshold  = baseForce * 1.2f;      // ~1200
        float highThreshold = baseForce * 1.8f;      // ~1500

        if (totalForce >= highThreshold)       Play(jumpHigh);
        else if (totalForce >= midThreshold)   Play(jumpMid);
        else                                   Play(jumpLow);
    }


    private void Play(AudioClip clip)
    {
        if (!clip) return;
        var src = _a.isPlaying ? _b : _a; // allow overlap
        src.PlayOneShot(clip);
    }
}