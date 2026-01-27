using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    [Header("子灯名称")]
    public string leftLightName = "LightL";
    public string rightLightName = "LightR";

    [Header("切换灯光的按键")]
    public KeyCode toggleKey = KeyCode.I;

    private Light leftLight;
    private Light rightLight;

    private bool isOn = true;

    void Awake()
    {
        leftLight = FindChildLight(leftLightName);
        rightLight = FindChildLight(rightLightName);

        if (!leftLight || !rightLight)
            Debug.LogWarning("HeadlightController: 找不到 LightL 或 LightR，请确认名字正确");
    }

    void Start()
    {
        SetLights(isOn);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
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
        // 优先按名字查找
        Transform t = transform.Find(name);
        if (t != null)
        {
            Light l = t.GetComponent<Light>();
            if (l != null) return l;
        }

        // 兜底：遍历所有子对象 Light
        Light[] lights = GetComponentsInChildren<Light>(true);
        foreach (var l in lights)
        {
            if (l.name == name) return l;
        }

        return null;
    }
}
