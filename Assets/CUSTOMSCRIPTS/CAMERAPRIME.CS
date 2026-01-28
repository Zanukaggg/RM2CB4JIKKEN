using UnityEngine;

namespace Awsim.Common
{
    /// <summary>
    /// 精简版相机：
    /// - 仅保留：组合键(默认 LeftShift)+鼠标/方向键 的视角偏移
    /// - 放开组合键后自动平滑回到初始朝向（初始朝向以 camera 在 target 下的本地方向为基准）
    /// </summary>
    public class FollowCameraSimpleAlt : MonoBehaviour
    {
        [Header("目标 (若为空且本相机是子物体，将自动设为 parent)")]
        [SerializeField] Transform targetSimple;

        [Header("组合键与灵敏度")]
        [SerializeField] KeyCode comboKey = KeyCode.LeftShift;
        [SerializeField] float yawSensitivity = 3.0f;   // 鼠标 X -> 偏航灵敏度（度 / 单位鼠标输入）
        [SerializeField] float pitchSensitivity = 3.0f; // 鼠标 Y -> 俯仰灵敏度

        [Header("俯仰限制")]
        [SerializeField] float pitchMin = -80f;
        [SerializeField] float pitchMax = 80f;

        [Header("放开组合键后回正速度 (度/秒)")]
        [SerializeField] float returnSpeedDegPerSec = 180f;

        // 内部状态（均为相对于基准的偏移）
        float yawOffsetDeg = 0f;    // 水平偏航角 (度)
        float pitchOffsetDeg = 0f;  // 俯仰角 (度)

        Quaternion baselineLocalRot = Quaternion.identity; // baseline = camera 相对于 target 的本地旋转

        void Start()
        {
            // 如果没有指定 target，而摄像机有 parent，则默认用 parent 作为 target
            if (targetSimple == null && transform.parent != null)
                targetSimple = transform.parent;

            if (targetSimple != null)
            {
                // baselineLocalRot 使得： transform.rotation == target.rotation * baselineLocalRot
                baselineLocalRot = Quaternion.Inverse(targetSimple.rotation) * transform.rotation;
            }
            else
            {
                // 没有 target 时，用当前 localRotation 为 baseline（相对于自身父物体）
                baselineLocalRot = transform.localRotation;
            }
        }

        void Update()
        {
            bool comboHeld = Input.GetKey(comboKey);

            if (comboHeld)
            {
                // 鼠标输入：累积偏移（度）
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");

                yawOffsetDeg += mx * yawSensitivity;
                pitchOffsetDeg -= my * pitchSensitivity; // 习惯：向上移动鼠标通常让视角抬头（你可按需反转）

                // 方向键：直接设为四个预设角（保持与旧脚本一致）
                if (Input.GetKeyDown(KeyCode.UpArrow)) yawOffsetDeg = 180f;
                if (Input.GetKeyDown(KeyCode.DownArrow)) yawOffsetDeg = 0f;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) yawOffsetDeg = 90f;
                if (Input.GetKeyDown(KeyCode.RightArrow)) yawOffsetDeg = 270f;

                // 限制 pitch 范围
                pitchOffsetDeg = Mathf.Clamp(pitchOffsetDeg, pitchMin, pitchMax);
            }
            else
            {
                // 放开组合键：平滑把偏航和俯仰回正到 0（相对于 baseline）
                float step = returnSpeedDegPerSec * Time.deltaTime;
                yawOffsetDeg = Mathf.MoveTowardsAngle(yawOffsetDeg, 0f, step);
                pitchOffsetDeg = Mathf.MoveTowards(pitchOffsetDeg, 0f, step);
            }

            // 计算当前偏移四元数（先俯仰再偏航，这样俯仰是相机本地 X 轴）
            Quaternion offset = Quaternion.Euler(pitchOffsetDeg, yawOffsetDeg, 0f);

            // 应用：如果有 target，则按 world = target.rotation * baselineLocalRot * offset
            if (targetSimple != null)
            {
                transform.rotation = targetSimple.rotation * baselineLocalRot * offset;

                // 保持摄像机与 target 的相对位置不变（不做平移，只修改朝向）
                // 如果要同时保持相对位置，请确保摄像机初始位置就是你期望的相对位置（本脚本不修改位置）。
            }
            else
            {
                // 无 target：用 localRotation 基准
                transform.localRotation = baselineLocalRot * offset;
            }
        }
    }
}
