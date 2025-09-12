using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CharacterVoiceSet defaultVoiceSet;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AudioManager.Instance.SetCharacterVoice(defaultVoiceSet);
    }


    // UI Character Selection
    public void SelectCharacter(CharacterVoiceSet chosen)
    {
        GameManager.Instance.defaultVoiceSet = chosen;
        AudioManager.Instance.SetCharacterVoice(chosen);
    }



    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
