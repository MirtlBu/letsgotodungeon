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
    private Transform playerAnchor;
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
        playerAnchor = GameObject.FindWithTag("Player")?.transform;
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
            if (current.choices != null && current.choices.Length == 2)
                BeginChoice();
            else if (current.choices != null && current.choices.Length == 1)
                BeginSingleChoice();
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

    private void BeginSingleChoice()
    {
        inChoice = true;
        choiceIndex = 0;
        InteractionUI.Instance?.Hide();
        InteractionUI.Instance?.ShowPlayer(current.choices[0].text, playerAnchor);
    }

    private void BeginChoice()
    {
        inChoice = true;
        choiceIndex = 0;
        InteractionUI.Instance?.Hide();
        ShowChoiceText();
    }

    private void ShowChoiceText()
    {
        var choice = current.choices[choiceIndex];
        string arrow = choiceIndex == 0 ? "◄ " : " ►";
        string other = current.choices[1 - choiceIndex].text;
        // Показываем активный вариант крупнее, второй как подсказку
        InteractionUI.Instance?.ShowPlayer($"{(choiceIndex == 0 ? "► " : "  ")}{current.choices[0].text}\n{(choiceIndex == 1 ? "► " : "  ")}{current.choices[1].text}", playerAnchor);
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
        InteractionUI.Instance?.HidePlayer();
        var chosen = current.choices[choiceIndex];

        if (chosen.nextDialogue != null)
        {
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
        InteractionUI.Instance?.HidePlayer();
        onEnd?.Invoke();
    }
}
