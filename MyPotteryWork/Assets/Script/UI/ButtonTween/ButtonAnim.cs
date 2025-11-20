using UnityEngine;
using DG.Tweening;


public class ButtonAnim : MonoBehaviour
{
    public RectTransform myButton;

    // 目标位置（按钮最终位置）
    public Vector2 targetPos;

    public void PlayInAnimation()
    {
        // 先把按钮放到右边屏幕外
        myButton.anchoredPosition = new Vector2(800, targetPos.y); // 800根据Canvas宽度调整

        myButton.DOAnchorPos(targetPos, 10f)
                .SetEase(Ease.OutBack, 0.3f); // 0.3 比默认更温和
    }
}
