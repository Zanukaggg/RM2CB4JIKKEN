using UnityEngine;

namespace Awsim.Common
{
    public class FollowCameraRigidAlt : MonoBehaviour
    {
        [SerializeField] private Transform targetAlt;

        [SerializeField] private KeyCode comboKeyAlt = KeyCode.LeftShift;
        [SerializeField] private float yawSensitivityAlt = 3f;
        [SerializeField] private float pitchSensitivityAlt = 3f;

        [SerializeField] private float pitchMinAlt = -80f;
        [SerializeField] private float pitchMaxAlt = 80f;

        [SerializeField] private float returnSpeedDegPerSecAlt = 180f;

        private Vector3 initialLocalPosAlt;
        private Quaternion initialLocalRotAlt;

        private float yawOffsetAlt = 0f;
        private float pitchOffsetAlt = 0f;

        void Start()
        {
            if (targetAlt == null && transform.parent != null)
                targetAlt = transform.parent;

            if (targetAlt != null)
            {
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

            bool joyBack = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + 20));
            bool joyLeft = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + 22));
            bool joyRight = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + 21));
            bool joyForward = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + 23));

            // mutual exclusion for opposite directions
            if (joyForward && joyBack) { joyForward = false; joyBack = false; }
            if (joyLeft && joyRight) { joyLeft = false; joyRight = false; }

            bool joyAny = joyBack || joyLeft || joyRight || joyForward;

            if (comboHeld || joyAny)
            {
                if (comboHeld)
                {
                    float mx = Input.GetAxis("Mouse X");
                    float my = Input.GetAxis("Mouse Y");

                    yawOffsetAlt += mx * yawSensitivityAlt;
                    pitchOffsetAlt -= my * pitchSensitivityAlt;

                    if (Input.GetKeyDown(KeyCode.UpArrow)) yawOffsetAlt = 0f;
                    if (Input.GetKeyDown(KeyCode.DownArrow)) yawOffsetAlt = 180f;
                    if (Input.GetKeyDown(KeyCode.LeftArrow)) yawOffsetAlt = 90f;
                    if (Input.GetKeyDown(KeyCode.RightArrow)) yawOffsetAlt = 270f;
                }

                if (joyAny)
                {
                    if (joyForward && joyLeft) yawOffsetAlt = 45f;
                    else if (joyForward && joyRight) yawOffsetAlt = 315f;
                    else if (joyBack && joyLeft) yawOffsetAlt = 135f;
                    else if (joyBack && joyRight) yawOffsetAlt = 225f;
                    else if (joyForward) yawOffsetAlt = 0f;
                    else if (joyBack) yawOffsetAlt = 180f;
                    else if (joyLeft) yawOffsetAlt = 90f;
                    else if (joyRight) yawOffsetAlt = 270f;
                }

                pitchOffsetAlt = Mathf.Clamp(pitchOffsetAlt, pitchMinAlt, pitchMaxAlt);
            }
            else
            {
                float step = returnSpeedDegPerSecAlt * Time.deltaTime;
                yawOffsetAlt = Mathf.MoveTowardsAngle(yawOffsetAlt, 0f, step);
                pitchOffsetAlt = Mathf.MoveTowards(pitchOffsetAlt, 0f, step);
            }

            Quaternion offset = Quaternion.Euler(pitchOffsetAlt, yawOffsetAlt, 0f);

            transform.position = targetAlt.TransformPoint(initialLocalPosAlt);
            transform.rotation = targetAlt.rotation * initialLocalRotAlt * offset;
        }
    }
}
