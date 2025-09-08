using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneNavigator : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "Game";
    [SerializeField] private string instructionsScene = "Instructions";
    [SerializeField] private string scoreTableScene = "ScoreTable";

    public void LoadMainMenu() => SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    public void LoadGame() => SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    public void LoadInstructions() => SceneManager.LoadScene(instructionsScene, LoadSceneMode.Single);
    public void LoadScoreTable() => SceneManager.LoadScene(scoreTableScene, LoadSceneMode.Single);

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}
