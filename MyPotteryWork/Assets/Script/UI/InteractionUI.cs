// InteractionUI.cs
using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public CanvasGroup promptGroup;
    public float floatHeight = 0.3f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
        Hide();
    }

    void Update()
    {
        // ¸¡¶¯¶¯»­
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startPosition + new Vector3(0, yOffset, 0);
    }

    public void SetInteractionName(string name)
    {
        promptText.text = $"[E] {name}";
    }

    public void Show()
    {
        promptGroup.alpha = 1;
        promptGroup.gameObject.SetActive(true);
    }

    public void Hide()
    {
        promptGroup.alpha = 0;
        promptGroup.gameObject.SetActive(false);
    }
}