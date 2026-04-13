using UnityEngine;

public class VehicleYellowBlinkerArray : MonoBehaviour
{
    [Header("受控黄灯（Spot Light）")]
    [Tooltip("把已经调好的黄灯 Light 组件拖进来")]
    public Light[] yellowLights;

    [Header("闪烁控制")]
    public bool enableBlink = true;

    [Tooltip("亮灯时间（秒），现实常见约 0.4~0.6")]
    public float onDuration = 0.5f;

    [Tooltip("灭灯时间（秒），现实常见约 0.4~0.6")]
    public float offDuration = 0.5f;

    [Header("亮灯强度覆盖（<=0 表示不改原强度）")]
    public float onIntensity = -1f;

    private float timer = 0f;
    private bool isOn = false;

    void Start()
    {
        // 初始化：全部关闭
        SetLights(false);
    }

    void Update()
    {
        if (!enableBlink)
        {
            if (isOn)
                SetLights(false);
            return;
        }

        timer += Time.deltaTime;

        if (isOn)
        {
            if (timer >= onDuration)
                SwitchOff();
        }
        else
        {
            if (timer >= offDuration)
                SwitchOn();
        }
    }

    void SwitchOn()
    {
        isOn = true;
        timer = 0f;
        SetLights(true);
    }

    void SwitchOff()
    {
        isOn = false;
        timer = 0f;
        SetLights(false);
    }

    void SetLights(bool enable)
    {
        if (yellowLights == null) return;

        foreach (var light in yellowLights)
        {
            if (light == null) continue;

            light.enabled = enable;

            if (enable && onIntensity > 0f)
                light.intensity = onIntensity;
        }
    }
}
