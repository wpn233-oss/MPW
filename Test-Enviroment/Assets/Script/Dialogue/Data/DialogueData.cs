using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea(3, 10)]
        public string text;
        public bool hasOptions;
        public DialogueOption[] options;
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string text;
        public DialogueData nextDialogue;
        public UnityEvent onSelect;
    }

    public DialogueLine[] lines;
}