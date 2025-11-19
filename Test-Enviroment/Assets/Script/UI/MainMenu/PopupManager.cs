using UnityEngine;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    [Header("弹窗内容")]
    public GameObject popupStart;

    [Header("遮罩动画")]
    public Animator maskAnimator;

    [Header("弹窗内容动画")]
    public Animator popupAnimator;

    [Header("弹窗延迟时间")]
    public float popupDelay = 0.25f;

    [Header("关闭动画时间")]
    public float closeAnimationTime = 0.5f;

    private bool isClosing = false;
    private Coroutine currentCoroutine;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void OpenStartPopup()
    {
        isClosing = false;
        gameObject.SetActive(true);

        if (popupStart != null)
        {
            popupStart.SetActive(false);
        }

        // 播放遮罩打开动画
        if (maskAnimator != null)
        {
            maskAnimator.SetTrigger("In");
        }

        // 使用协程延迟显示弹窗内容
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ShowPopupAfterDelay());
    }

    private IEnumerator ShowPopupAfterDelay()
    {
        yield return new WaitForSeconds(popupDelay);

        if (!isClosing && popupStart != null)
        {
            popupStart.SetActive(true);

            // 播放弹窗内容的打开动画
            if (popupAnimator != null)
            {
                popupAnimator.SetTrigger("In");
            }
        }
    }

    // 取消按钮调用的方法
    public void OnCancelButtonClick()
    {
        if (isClosing) return;

        isClosing = true;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ClosePopupWithAnimation());
    }

    private IEnumerator ClosePopupWithAnimation()
    {
        // 先播放弹窗内容的关闭动画（但不要立即禁用）
        if (popupAnimator != null)
        {
            popupAnimator.SetTrigger("Out");
        }

        // 播放遮罩关闭动画
        if (maskAnimator != null)
        {
            maskAnimator.SetTrigger("Out");
        }

        // 等待所有动画播放完成
        yield return new WaitForSeconds(closeAnimationTime);

        // 动画播放完毕后再禁用弹窗内容
        if (popupStart != null)
        {
            popupStart.SetActive(false);
        }

        // 关闭整个弹窗Canvas
        gameObject.SetActive(false);
        isClosing = false;
        currentCoroutine = null;
    }
}