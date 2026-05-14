using UnityEngine;

// Только анимация открытия. Вызывается через ConditionalInteractable → On Success.
public class Chest : MonoBehaviour
{
    private Animator animator;
    private bool opened;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Open()
    {
        if (opened) return;
        opened = true;
        animator?.SetTrigger("open");
    }

    public void Reset()
    {
        opened = false;
        animator?.Rebind();
        animator?.Update(0f);
    }
}
