using UnityEngine;

public class WeaponVisibility : MonoBehaviour
{
    [SerializeField] private GameObject weapon;

    private Animator animator;
    private bool wasAttacking;

    void Start()
    {
        animator = GetComponent<Animator>();
        weapon?.SetActive(false);
    }

    void Update()
    {
        if (animator == null || weapon == null) return;
        if (alwaysVisible) return;
        bool isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        if (wasAttacking && !isAttacking)
            weapon.SetActive(false);
        wasAttacking = isAttacking;
    }

    // Вызывается через Animation Event
    public void ShowWeapon()
    {
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasSword) return;
        weapon?.SetActive(true);
    }
    public void HideWeapon() => weapon?.SetActive(false);

    // Вызови после получения меча — меч остаётся виден постоянно
    public void SetAlwaysVisible()
    {
        alwaysVisible = true;
        weapon?.SetActive(true);
    }

    private bool alwaysVisible;
}
