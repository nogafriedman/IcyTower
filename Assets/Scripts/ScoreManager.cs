using UnityEngine;
using System;
using System.Xml;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviour
{
    // public TextMeshProUGUI scoreText;

    [SerializeField] private float comboTimeout = 3f;

    public int HighestFloor { get; private set; } = 0;
    public int LastLandedFloor { get; private set; } = 0;

    // combo state
    public int comboJumpCount = 0; // how many multi-floor jumps in current combo
    public int comboFloorsTotal = 0; // sum of floors skipped in this combo
    public float comboTimerLeft = 0f; // countdown to auto-confirm/end
    public int confirmedComboScore = 0; // sum(comboFloorsTotal^2) for all confirmed combos
    [SerializeField] public ParticleSystem comboEffect;
    [SerializeField] private Transform fxAnchor;

    public int totalScore = 0;
    public int CurrentScore => HighestFloor * 10 + confirmedComboScore;

    // effects
    // public event Action OnComboStarted;                                   // first qualifying jump in a chain
    // public event Action<bool, int> OnComboEnded;                          // (confirmed, addedScore)
    // public event Action<int, int> OnComboProgress;                        // (jumpCount, floorsTotal)
    public event Action<int> OnScoreChanged; // emits new CurrentScore

    void Update() => UpdateComboTimeout();
    public bool ComboActive => comboJumpCount > 0;
    // private void RaiseScoreChanged() => OnScoreChanged?.Invoke(CurrentScore);

    private void RaiseScoreChanged()
    {
        LogScore("Update");
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void UpdateComboTimeout()
    {

        if (!ComboActive) return;

        comboTimerLeft -= Time.deltaTime;
        if (comboTimerLeft <= 0f)
        {
            Debug.Log("Combo Timeout, ending combo");
            EndCombo();
        }
    }

    public void SetHighestFloor(int floor)
    {
        HighestFloor = floor;
        RaiseScoreChanged();
    }

    // called on each valid landing on a floor
    public void UpdateState(int floor)
    {
        if (floor > HighestFloor)
        {
            SetHighestFloor(floor);
        }

        UpdateCombo(floor);
        LastLandedFloor = floor;
    }

    public void UpdateCombo(int floor)
    {
        int diff = floor - LastLandedFloor;

        if (diff >= 2)
        {
            Debug.Log($"[Combo] Started at floor {LastLandedFloor} -> {floor}");

            if (!ComboActive)
            {
                PlayComboStartFX();
            }
            
            comboJumpCount++;
            comboFloorsTotal += diff;
            comboTimerLeft = comboTimeout;
            Debug.Log($"[Combo] Jump#{comboJumpCount}: +{diff}, runningTotal={comboFloorsTotal}, timer={comboTimerLeft:0.00}");

            // // if (!ComboActive)
            // // {
            // //     // OnComboStarted?.Invoke();
            // // }
            // // 
            // // OnComboProgress?.Invoke(comboJumpCount, comboFloorsTotal);
        }
        else
        {
            if (ComboActive)
            {
                EndCombo();
            }
        }
    }

    private void ResetCombo()
    {
        Debug.Log($"[Combo] Reset (jumpCount={comboJumpCount}, total={comboFloorsTotal})");
        comboJumpCount = 0;
        comboFloorsTotal = 0;
        comboTimerLeft = 0f;
    }

    private void EndCombo()
    {
        if (comboJumpCount >= 2)
        {
            // confirmedComboScore += comboFloorsTotal * comboFloorsTotal;
            // Debug.Log("End of combo, confirmedComboScore: " + confirmedComboScore);
            // RaiseScoreChanged();
            int add = comboFloorsTotal * comboFloorsTotal;
            confirmedComboScore += add;
            LogScore($"+{add} from combo (sum={comboFloorsTotal}, jumps={comboJumpCount})");
            RaiseScoreChanged();
        }
        else
        {
            Debug.Log($"[Combo] End (discarded, jumps={comboJumpCount}, sum={comboFloorsTotal})");
        }

        ResetCombo();
    }

    public int GameOverScore() => CurrentScore;

    private void PlayComboStartFX()
{
    if (comboEffect == null) return;

    // If the assigned particle is a prefab (not in scene), instantiate a one-shot
    if (!comboEffect.gameObject.scene.IsValid())
    {
        var pos = fxAnchor ? fxAnchor.position : Vector3.zero;
        var ps = Instantiate(comboEffect, pos, Quaternion.identity);
        ps.Play();

        // auto-destroy after it finishes
        var main = ps.main;
        float life = main.duration + main.startLifetime.constantMax + 0.5f;
        Destroy(ps.gameObject, life);
    }
    else
    {
        // Scene particle: move to anchor (if any), restart cleanly
        if (fxAnchor) comboEffect.transform.position = fxAnchor.position;
        comboEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        comboEffect.Play(true);
    }
}



    //helper
    private void LogScore(string context = "")
    {
        string msg = $"[SCORE] {context} | Floor={HighestFloor}, Base={HighestFloor * 10}, " +
                     $"Combos={confirmedComboScore}, Total={CurrentScore}";
        Debug.Log(msg);
    }
}