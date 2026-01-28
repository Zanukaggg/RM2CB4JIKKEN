using UnityEngine;

namespace Awsim.Common
{
    /// <summary>
    /// 跟随目标一比一位置旋转，同时支持组合键+鼠标/方向键偏航/俯仰回正
    /// </summary>
    public class FollowCameraRigidAlt : MonoBehaviour
    {
        [Header("目标")]
        [SerializeField] private Transform targetAlt;

        [Header("组合键与灵敏度")]
        [SerializeField] private KeyCode comboKeyAlt = KeyCode.LeftShift;
        [SerializeField] private float yawSensitivityAlt = 3f;   // 鼠标X偏航灵敏度
        [SerializeField] private float pitchSensitivityAlt = 3f; // 鼠标Y俯仰灵敏度

        [Header("俯仰限制")]
        [SerializeField] private float pitchMinAlt = -80f;
        [SerializeField] private float pitchMaxAlt = 80f;

        [Header("放开组合键回正速度 (度/秒)")]
        [SerializeField] private float returnSpeedDegPerSecAlt = 180f;

        // 内部状态
        private Vector3 initialLocalPosAlt;   // 相对目标本地位置偏移
        private Quaternion initialLocalRotAlt; // 相对目标本地旋转偏移

        private float yawOffsetAlt = 0f;   // 当前偏航角
        private float pitchOffsetAlt = 0f; // 当前俯仰角

        void Start()
        {
            if (targetAlt == null && transform.parent != null)
                targetAlt = transform.parent;

            if (targetAlt != null)
            {
                // 使用相对目标的本地偏移（位置+旋转）
                initialLocalPosAlt = targetAlt.InverseTransformPoint(transform.position);
                initialLocalRotAlt = Quaternion.Inverse(targetAlt.rotation) * transform.rotation;
            }
            else
            {
                initialLocalPosAlt = transform.localPosition;
                initialLocalRotAlt = transform.localRotation;
            }
        }

        void Update()
        {
            if (targetAlt == null) return;

            bool comboHeld = Input.GetKey(comboKeyAlt);

            if (comboHeld)
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");

                yawOffsetAlt += mx * yawSensitivityAlt;
                pitchOffsetAlt -= my * pitchSensitivityAlt; // 鼠标向上抬头

                // 方向键快速设定角度
                if (Input.GetKeyDown(KeyCode.UpArrow)) yawOffsetAlt = 180f;
                if (Input.GetKeyDown(KeyCode.DownArrow)) yawOffsetAlt = 0f;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) yawOffsetAlt = 90f;
                if (Input.GetKeyDown(KeyCode.RightArrow)) yawOffsetAlt = 270f;

                pitchOffsetAlt = Mathf.Clamp(pitchOffsetAlt, pitchMinAlt, pitchMaxAlt);
            }
            else
            {
                float step = returnSpeedDegPerSecAlt * Time.deltaTime;
                yawOffsetAlt = Mathf.MoveTowardsAngle(yawOffsetAlt, 0f, step);
                pitchOffsetAlt = Mathf.MoveTowards(pitchOffsetAlt, 0f, step);
            }

            // 偏航/俯仰旋转四元数
            Quaternion offset = Quaternion.Euler(pitchOffsetAlt, yawOffsetAlt, 0f);

            // 更新位置和旋转，一比一跟随目标 + 初始偏移 + 偏航/俯仰偏移
            transform.position = targetAlt.TransformPoint(initialLocalPosAlt);
            transform.rotation = targetAlt.rotation * initialLocalRotAlt * offset;
        }
    }
}
