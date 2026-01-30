using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    public string leftLightName = "LightL";
    public string rightLightName = "LightR";

    public KeyCode toggleKey = KeyCode.I;
    public int joystickButton = 8;

    private Light leftLight;
    private Light rightLight;

    private bool isOn = true;

    void Awake()
    {
        leftLight = FindChildLight(leftLightName);
        rightLight = FindChildLight(rightLightName);
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
        if (leftLight) leftLight.enabled = on;
        if (rightLight) rightLight.enabled = on;
    }

    private Light FindChildLight(string name)
    {
        Transform t = transform.Find(name);
        if (t != null)
        {
            Light l = t.GetComponent<Light>();
            if (l != null) return l;
        }

        Light[] lights = GetComponentsInChildren<Light>(true);
        foreach (var l in lights)
        {
            if (l.name == name) return l;
        }

        return null;
    }
}
