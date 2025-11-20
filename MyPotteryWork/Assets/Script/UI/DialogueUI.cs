using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;

    private NPCController currentNPC; // 当前对话的NPC

    void Start()
    {
        continueButton.onClick.AddListener(ContinueDialogue);
        HideDialogue();
    }

    public void ShowDialogue(string speakerName, Sprite portrait, string text)
    {
        dialogueBox.SetActive(true);
        speakerNameText.text = speakerName;
        portraitImage.sprite = portrait;
        dialogueText.text = text;
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);

        // 对话结束时通知NPC
        if (currentNPC != null)
        {
            currentNPC.OnDialogueEnd();
            currentNPC = null;
        }
    }

    // 设置当前对话的NPC
    public void SetCurrentNPC(NPCController npc)
    {
        currentNPC = npc;
    }

    private void ContinueDialogue()
    {
        DialogueManager.Instance.NextLine();
    }
}