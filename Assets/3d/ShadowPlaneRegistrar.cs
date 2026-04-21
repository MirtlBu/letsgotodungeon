using UnityEngine;

// Повесь на shadowPlane в сцене Dungeon.
// При загрузке сцены сам находит ShadowController на камере и регистрируется.
public class ShadowPlaneRegistrar : MonoBehaviour
{
    void Start()
    {
        var controller = FindFirstObjectByType<ShadowController>();
        if (controller != null)
            controller.Register(gameObject);
    }

    void OnDestroy()
    {
        var controller = FindFirstObjectByType<ShadowController>();
        if (controller != null)
            controller.Unregister();
    }
}
