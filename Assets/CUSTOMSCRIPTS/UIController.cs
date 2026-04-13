using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("要控制的UI对象")]
    public GameObject[] uiObjects; // 可以在Inspector里添加多个UI对象，默认可以只加一个Canvas

    [Header("控制按键")]
    public KeyCode toggleKey = KeyCode.I; // 键盘按键
    public int joystickButton = 8;        // 手柄按键

    private bool isVisible = false; // 默认显示

    void Start()
    {
        // 根据初始状态设置UI显示
        SetUIVisibility(isVisible);
    }

    void Update()
    {
        bool keyboard = Input.GetKeyDown(toggleKey);
        bool joystick = Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + joystickButton));

        if (keyboard || joystick)
        {
            isVisible = !isVisible;
            SetUIVisibility(isVisible);
        }
    }

    private void SetUIVisibility(bool visible)
    {
        foreach (var obj in uiObjects)
        {
            if (obj) obj.SetActive(visible);
        }
    }
}
