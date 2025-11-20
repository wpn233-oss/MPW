using UnityEngine;
using UnityEngine.Events;

public class UIAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class UIAnimationEvent
    {
        public string animationName;
        public UnityEvent onAnimationStart;
        public UnityEvent onAnimationEnd;
    }

    public UIAnimationEvent[] animationEvents;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 通过代码触发动画
    public void PlayAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    // 动画事件回调 - 动画开始
    public void OnAnimationStart(string animationName)
    {
        foreach (var animEvent in animationEvents)
        {
            if (animEvent.animationName == animationName)
            {
                animEvent.onAnimationStart.Invoke();
                break;
            }
        }
    }

    // 动画事件回调 - 动画结束
    public void OnAnimationEnd(string animationName)
    {
        foreach (var animEvent in animationEvents)
        {
            if (animEvent.animationName == animationName)
            {
                animEvent.onAnimationEnd.Invoke();
                break;
            }
        }
    }
}