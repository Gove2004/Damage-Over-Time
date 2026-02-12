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

            BattleManager.Instance.StartBattle();
        }
        else  // 回到主界面
        {
            // battleUI.gameObject.SetActive(false);
            mainUI.gameObject.SetActive(true);

        }
    }

    #endregion
}
