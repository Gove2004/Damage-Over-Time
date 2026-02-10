using UnityEngine;
using UnityEngine.UI;

public class DotShowList : MonoBehaviour
{
    // 这里应采用事件驱动的方式来更新显示的Dot列表，而不是每帧都去检查和更新。
    // 但是为了方便快捷，这里暂时使用Update方法来模拟。
    public bool isPlayer; // 用于区分显示玩家还是敌人的Dot列表
    public DotShow dotShowPrefab; // Dot显示的预制体

    public Button zhankaiButton; // 展开/收起按钮
    public GameObject dotListContainer; // Dot列表的容器
    private bool isExpanded = true; // 默认展开

    void Start()
    {
        // 生成16个DotShow预制体作为占位，实际显示时根据Dot数量来启用/禁用这些预制体
        for (int i = 0; i < 16; i++)
        {
            Instantiate(dotShowPrefab, dotListContainer.transform);
        }

        zhankaiButton.onClick.AddListener(() =>
        {
            isExpanded = !isExpanded;
            dotListContainer.SetActive(isExpanded);
        });
    }

    void Update()
    {
        if (BattleManager.Instance == null) return;

        if (isPlayer)
        {
            UpdateDotShows(BattleManager.Instance.player.dotBar);
        }
        else
        {
            UpdateDotShows(BattleManager.Instance.enemy.dotBar);
        }
    }

    private void UpdateDotShows(System.Collections.Generic.List<Dot> dots)
    {
        for (int i = 0; i < dotListContainer.transform.childCount; i++)
        {
            var dotShow = dotListContainer.transform.GetChild(i).GetComponent<DotShow>();
            if (i < dots.Count)
            {
                dotShow.gameObject.SetActive(true);
                dotShow.SetData(dots[i]);
            }
            else
            {
                dotShow.gameObject.SetActive(false);
            }
        }
    }
}
