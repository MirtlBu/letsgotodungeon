using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ToBeContiunedScreen : MonoBehaviour
{
    [SerializeField] private string returnScene = "Overworld";

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SceneTransition.Instance?.GoToScene(returnScene);
        }
    }
}
