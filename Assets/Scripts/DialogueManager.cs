using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool IsActive { get; private set; }

    private DialogueSO current;
    private Transform anchor;
    private int lineIndex;
    private bool inChoice;
    private int choiceIndex;
    private Action onEnd;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartDialogue(DialogueSO dialogue, Transform speakerAnchor, Action onComplete = null)
    {
        if (IsActive) return;
        IsActive = true;
        current = dialogue;
        anchor = speakerAnchor;
        lineIndex = 0;
        inChoice = false;
        onEnd = onComplete;
        ShowCurrentLine();
    }

    void Update()
    {
        if (!IsActive) return;

        if (inChoice)
        {
            UpdateChoice();
        }
        else
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
                AdvanceLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (lineIndex >= current.lines.Length)
        {
            // Все строки показаны
            if (current.choices != null && current.choices.Length == 2)
                BeginChoice();
            else
                EndDialogue();
            return;
        }

        InteractionUI.Instance?.Show(current.lines[lineIndex].text, anchor);
    }

    private void AdvanceLine()
    {
        lineIndex++;
        ShowCurrentLine();
    }

    // ─── Choice ───────────────────────────────────────────────

    private void BeginChoice()
    {
        inChoice = true;
        choiceIndex = 0;
        ShowChoiceText();
    }

    private void ShowChoiceText()
    {
        var choice = current.choices[choiceIndex];
        string arrow = choiceIndex == 0 ? "◄ " : " ►";
        string other = current.choices[1 - choiceIndex].text;
        // Показываем активный вариант крупнее, второй как подсказку
        InteractionUI.Instance?.Show($"{(choiceIndex == 0 ? "► " : "  ")}{current.choices[0].text}\n{(choiceIndex == 1 ? "► " : "  ")}{current.choices[1].text}", anchor);
    }

    private void UpdateChoice()
    {
        bool left  = Keyboard.current.aKey.wasPressedThisFrame ||
                     Keyboard.current.leftArrowKey.wasPressedThisFrame;
        bool right = Keyboard.current.dKey.wasPressedThisFrame ||
                     Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (left || right)
        {
            choiceIndex = 1 - choiceIndex; // переключаем между 0 и 1
            ShowChoiceText();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
            ConfirmChoice();
    }

    private void ConfirmChoice()
    {
        inChoice = false;
        var chosen = current.choices[choiceIndex];

        if (chosen.nextDialogue != null)
        {
            // Продолжаем диалог следующим SO
            current = chosen.nextDialogue;
            lineIndex = 0;
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    // ─── End ──────────────────────────────────────────────────

    private void EndDialogue()
    {
        IsActive = false;
        InteractionUI.Instance?.Hide();
        onEnd?.Invoke();
    }
}
