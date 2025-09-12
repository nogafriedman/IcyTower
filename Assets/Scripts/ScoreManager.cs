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
    private bool comboActive => comboJumpCount > 0;

    // effects
    [SerializeField] private int milestoneStep = 100; // every 100 floors
    private int nextMilestone = 100;


    void Update() => UpdateComboTimeout();

    public void UpdateComboTimeout()
    {
        comboTimerLeft -= Time.deltaTime;
        if (comboActive && comboTimerLeft <= 0f)
        {
            Debug.Log("Combo Timeout, ending combo");
            EndCombo();
        }
    }

    public void SetHighestFloor(int floor)
    {
        if (floor > HighestFloor)
        {
            HighestFloor = floor;

            // Trigger sound effect on milestones
            while (HighestFloor >= nextMilestone)
            {
                AudioManager.Instance?.PlayMilestone();
                nextMilestone += milestoneStep;
            }
        }
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
            if (!comboActive)
            {
                Debug.Log($"[Combo] Started at floor {LastLandedFloor} -> to {floor}");
                PlayComboStartFX();
                AudioManager.Instance?.ResetComboTierProgress();
            }

            comboJumpCount++;
            Debug.Log($"ComboJumpCount++: {comboJumpCount}");
            comboFloorsTotal += diff;
            comboTimerLeft = comboTimeout;
            Debug.Log($"[Combo] ComboFloorsTotal: {comboFloorsTotal} | comboJumpCount: {comboJumpCount}");

            // Notify listeners that combo tier changed
            AudioManager.Instance?.OnComboFloorsProgress(comboFloorsTotal);
        }
        else
        {
            if (comboActive)
            {
                Debug.Log("[Combo] END (single/zero step)");
                EndCombo();
            }
        }
    }

    private void ResetCombo()
    {
        Debug.Log($"in ResetCombo()");
        comboJumpCount = 0;
        comboFloorsTotal = 0;
        comboTimerLeft = 0f;
    }

    private void EndCombo()
    {
        Debug.Log($"in EndCombo()");
        if (comboJumpCount >= 2)
        {
            int add = comboFloorsTotal * comboFloorsTotal;
            confirmedComboScore += add;
        }

        ResetCombo();

        // Let listeners know combo ended
        AudioManager.Instance?.ResetComboTierProgress();
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