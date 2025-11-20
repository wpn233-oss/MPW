using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [SerializeField] private DialogueUI dialogueUI;
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private NPCController currentNPC;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(DialogueData dialogue, string npcName, Sprite portrait, NPCController npc = null)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        currentNPC = npc;

        // 安全访问PlayerController单例
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.DisableMovement();
        }
        else
        {
            Debug.LogWarning("PlayerController instance not found!");
        }

        dialogueUI.SetCurrentNPC(npc);
        dialogueUI.ShowDialogue(npcName, portrait, dialogue.lines[0].text);
    }


    public void OnOptionSelected(DialogueData.DialogueOption option)
    {
        // 触发选项的事件
        option.onSelect.Invoke();

        // 如果有后续对话，开始新的对话
        if (option.nextDialogue != null)
        {
            StartDialogue(option.nextDialogue, currentNPC.npcName, currentNPC.portrait, currentNPC);
        }
        else
        {
            EndDialogue();
        }
    }

    public void NextLine()
    {
        if (currentDialogue == null) return;

        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Length)
        {
            var line = currentDialogue.lines[currentLineIndex];
            dialogueUI.ShowDialogue(line.speakerName, line.speakerPortrait, line.text);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        currentDialogue = null;
        currentNPC = null;

        // 安全访问PlayerController单例
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.EnableMovement();
        }
        else
        {
            Debug.LogWarning("PlayerController instance not found!");
        }

        dialogueUI.HideDialogue();
    }
}