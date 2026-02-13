using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 简单的单例模式
    public static GameManager Instance;
    void Awake()
    {
        Instance = this;


    }


    void Start()
    {
        Load();
        SwitchSecne(false); // 默认进入主界面
    }


    # region 存档

    public int maxScore { get; private set; } = 0;

    public void Save(int score)
    {
        if (score > maxScore)
        {
            maxScore = score;
            PlayerPrefs.SetInt("MaxScore", maxScore);
            PlayerPrefs.Save();
        }
    }


    public void Load()
    {
        maxScore = PlayerPrefs.GetInt("MaxScore", 0);
    }

    # endregion



    #region 状态

    private bool IsBattle = false;
    public GameObject battleUI;
    public GameObject mainUI;

    public void SwitchSecne(bool isBattle)
    {
        // 这里只涉及两个场景的切换
        // 并且直接用MAin挡住Battle

        IsBattle = isBattle;

        if (IsBattle) // 进入Battle
        {
            // battleUI.gameObject.SetActive(true);
            mainUI.gameObject.SetActive(false);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBattleBGM();

            BattleManager.Instance.StartBattle();
        }
        else  // 回到主界面
        {
            // battleUI.gameObject.SetActive(false);
            mainUI.gameObject.SetActive(true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTitleBGM();
            }
            else
            {
                Debug.LogWarning("[GameManager] AudioManager.Instance is null when switching to Title!");
                // Try to find it manually in case Awake hasn't set Instance yet (unlikely in Start, but possible)
                var am = FindObjectOfType<AudioManager>();
                if (am != null)
                {
                    am.PlayTitleBGM();
                }
                else
                {
                     Debug.LogWarning("[GameManager] AudioManager not found in scene. Creating one.");
                     var go = new GameObject("AudioManager");
                     go.AddComponent<AudioManager>().PlayTitleBGM();
                }
            }
        }
    }

    #endregion
}
