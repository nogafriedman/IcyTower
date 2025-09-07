using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // public TextMeshProUGUI scoreText;

    [SerializeField] private float comboTimeout = 3f;

    public int HighestFloor { get; private set; } = 0;
    public int CurrentScore => HighestFloor * 10 + confirmedComboScore;
    public int LastLandedFloor { get; private set; } = 0;

    // combo state
    public int comboJumpCount = 0; // how many multi-floor jumps in current combo
    public int comboFloorsTotal = 0; // sum of floors skipped in this combo
    public float comboTimerLeft = 0f; // countdown to auto-confirm/end
    public int confirmedComboScore = 0; // sum(comboFloorsTotal^2) for all confirmed combos

    void Start()
    {
        // init score UI text at 0
        // scoreText.text = "Score: " + CurrentScore.ToString();
    }

    void Update()
    {
        // scoreText.text = "Score: " + score.ToString();
        Debug.Log("Score: " + CurrentScore);
    }

    public void UpdateComboTimeout()
    {
        if (comboJumpCount > 0)
        {
            comboTimerLeft -= Time.deltaTime;
            if (comboTimerLeft <= 0f)
                CalculateCombo();
        }
    }

    public void SetHighestFloor(int floor)
    {
        HighestFloor = floor;
    }

    public void UpdateCombo(int floor)
    {
        int diff = floor - LastLandedFloor;
        if (diff >= 2)
        {
            // multi-floor jump - combo:
            comboJumpCount++;
            comboFloorsTotal += diff;
            comboTimerLeft = comboTimeout;
        }
    }

    public void CalculateCombo()
    {
        // valid combo: add squared total floors
        if (comboJumpCount >= 2)
        {
            confirmedComboScore += comboFloorsTotal * comboFloorsTotal;
        }

        // reset
        comboJumpCount = 0;
        comboFloorsTotal = 0;
        comboTimerLeft = 0;
    }

    public void UpdateScore(int floor)
    {
        if (floor > HighestFloor)
        {
            SetHighestFloor(floor);
        }

        UpdateCombo(floor);
        LastLandedFloor = floor;

    }

    public int GameOverScore()
    {
        return HighestFloor * 10 + confirmedComboScore;
    }
}