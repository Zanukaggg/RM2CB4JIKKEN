using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    [Header("灯名称数组")]
    public string[] lightNames = { "LightL", "LightR" }; // 可以在Inspector里添加更多灯

    [Header("控制键")]
    public KeyCode toggleKey = KeyCode.I;
    public int joystickButton = 8;

    private Light[] lights;
    private bool isOn = false;

    void Awake()
    {
        // 根据lightNames数组找到对应的Light组件
        lights = new Light[lightNames.Length];
        for (int i = 0; i < lightNames.Length; i++)
        {
            lights[i] = FindChildLight(lightNames[i]);
        }
    }

    void Start()
    {
        SetLights(isOn);
    }

    void Update()
    {
        bool keyboard = Input.GetKeyDown(toggleKey);
        bool joystick = Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + joystickButton));

        if (keyboard || joystick)
        {
            isOn = !isOn;
            SetLights(isOn);
        }
    }

    private void SetLights(bool on)
    {
        foreach (var light in lights)
        {
            if (light) light.enabled = on;
        }
    }

    private Light FindChildLight(string name)
    {
        Transform t = transform.Find(name);
        if (t != null)
        {
            Light l = t.GetComponent<Light>();
            if (l != null) return l;
        }

        Light[] allLights = GetComponentsInChildren<Light>(true);
        foreach (var l in allLights)
        {
            if (l.name == name) return l;
        }

        return null;
    }
}
