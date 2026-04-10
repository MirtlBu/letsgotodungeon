using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(1, 4)] public string text;
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)] public string text;
    public DialogueSO nextDialogue; // опционально: продолжение после выбора
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "Game/Dialogue")]
public class DialogueSO : ScriptableObject
{
    public DialogueLine[] lines;

    [Header("Choice (optional)")]
    [Tooltip("Оставь пустым если выбора нет")]
    public DialogueChoice[] choices; // 0 или 2 элемента
}
