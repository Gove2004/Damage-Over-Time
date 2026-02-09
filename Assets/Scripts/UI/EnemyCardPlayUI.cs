using System;
using DG.Tweening;
using UnityEngine;

public class EnemyCardPlayUI : MonoBehaviour
{
    private Action disposablePlay;
    private Action disposableDraw;
    private CardUIItem cardUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        disposablePlay = EventCenter.Register("Enemy_PlayedCard", obj => OnEnemyPlayCard((BaseCard)obj));
        disposableDraw = EventCenter.Register("Enemy_DrewCard", obj => OnEnemyDrawCard((BaseCard)obj));

        cardUI = GetComponent<CardUIItem>();
        cardUI.gameObject.SetActive(false);

    }

    private void OnEnemyPlayCard(BaseCard cardObj)
    {
        cardUI.SetData(cardObj);
        // 显示卡牌UI
        ShowPlayAnimation();
    }

    private void OnEnemyDrawCard(BaseCard cardObj)
    {
        cardUI.SetData(cardObj);
        // 显示卡牌UI
        ShowDrawAnimation();
    }

    private void ShowPlayAnimation()
    {
        // 这里可以添加显示动画等效果
        cardUI.gameObject.SetActive(true);

        // 动画持续1秒， 展开0.33秒，存在0.33秒，收回0.33秒
        cardUI.transform.localScale = Vector3.zero;
        cardUI.transform.DOScale(Vector3.one, 0.33f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.33f, () =>
            {
                cardUI.transform.DOScale(Vector3.zero, 0.33f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    cardUI.gameObject.SetActive(false);
                });
            });
        });
    }

    private void ShowDrawAnimation()
    {
        // 这里可以添加显示动画等效果
        cardUI.gameObject.SetActive(true);

        // 动画0.5秒，旋转360度后消失
        cardUI.transform.localScale = Vector3.one;
        cardUI.transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(() =>
        {
            cardUI.gameObject.SetActive(false);
        });
    }

    void OnDestroy()
    {
        disposablePlay?.Invoke();
        disposableDraw?.Invoke();
    }
}
