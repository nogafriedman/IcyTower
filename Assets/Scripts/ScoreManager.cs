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

    public int CurrentScore => HighestFloor * 10 + confirmedComboScore;

    // effects
    // public event Action OnComboStarted;                                   // first qualifying jump in a chain
    // public event Action<bool, int> OnComboEnded;                          // (confirmed, addedScore)
    // public event Action<int, int> OnComboProgress;                        // (jumpCount, floorsTotal)

    public bool ComboActive => comboJumpCount > 0;
    
    void Start()
    {
        // init score UI text at 0
        // scoreText.text = "Score: " + CurrentScore.ToString();
    }

    void Update()
    {
        // scoreText.text = "Score: " + score.ToString();
        Debug.Log("Score: " + CurrentScore);
        Debug.Log("comboJumpCount: " + comboJumpCount);
        Debug.Log("comboFloorsTotal: " + comboFloorsTotal);
        Debug.Log("confirmedComboScore: " + confirmedComboScore);


        UpdateComboTimeout();
    }

    public void UpdateComboTimeout()
    {

        if (!ComboActive) return;

        comboTimerLeft -= Time.deltaTime;
        if (comboTimerLeft <= 0f)
        {
            EndCombo();
        }
    }

    public void SetHighestFloor(int floor)
    {
        HighestFloor = floor;
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
            comboJumpCount++;
            comboFloorsTotal += diff;
            comboTimerLeft = comboTimeout;

            // if (!ComboActive)
            // {
            //     // OnComboStarted?.Invoke();
            // }
// 
            // OnComboProgress?.Invoke(comboJumpCount, comboFloorsTotal);
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
        comboJumpCount = 0;
        comboFloorsTotal = 0;
        comboTimerLeft = 0f;
    }

    private void EndCombo()
    {
        if (comboJumpCount >= 2)
        {
            confirmedComboScore += comboFloorsTotal * comboFloorsTotal;
        }
        ResetCombo();
    }

    public int GameOverScore() => CurrentScore;
}