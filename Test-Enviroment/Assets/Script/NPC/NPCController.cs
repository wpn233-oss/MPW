using UnityEngine;

public class NPCController : MonoBehaviour
{
    public string npcName;
    public Sprite portrait;
    public DialogueData dialogue;
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionPrompt;

    private bool playerInRange = false;
    private bool isInDialogue = false; // 标记是否正在对话

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !isInDialogue)
        {
            StartDialogue();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null && !isInDialogue)
                interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    void StartDialogue()
    {
        if (dialogue == null || dialogue.lines.Length == 0)
        {
            Debug.LogWarning("NPC " + npcName + " 没有设置对话数据");
            return;
        }

        isInDialogue = true; // 标记为对话中
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false); // 隐藏交互提示

        DialogueManager.Instance.StartDialogue(dialogue, npcName, portrait, this);
    }

    // 对话结束回调
    public void OnDialogueEnd()
    {
        isInDialogue = false;

        // 如果玩家还在范围内，重新显示交互提示
        if (playerInRange)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }
}