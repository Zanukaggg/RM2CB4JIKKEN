using UnityEngine;
using UnityEngine.InputSystem;
using Awsim.Common;
using Unity.Mathematics;

namespace Awsim.Entity
{
    public class AccelVehicleLogitechG29Input : MonoBehaviour, IAccelVehicleInput
    {
        [System.Serializable]
        public class Settings
        {
            [SerializeField] string _devicePath;
            [SerializeField] float _selfAligningTorqueCoeff;

            public string DevicePath => _devicePath;
            public float SteeringTorqueCoeff => _selfAligningTorqueCoeff;

            public Settings() { }
            public Settings(string devicePath, float selfAligningTorqueCoeff)
            {
                _devicePath = devicePath;
                _selfAligningTorqueCoeff = selfAligningTorqueCoeff;
            }
        }

        public void Initialize(Settings settings)
        {
            _devicePath = settings.DevicePath;
            var coeff = Mathf.Clamp01(settings.SteeringTorqueCoeff);

            if (coeff == 0)
            {
                _selfAligningTorqueSpeedCoeff = 0;
                _selfAligningTorqueSteerCoeff = 0;
            }
            else
            {
                _selfAligningTorqueSpeedCoeff = Mathf.Lerp(10f, 4f, coeff);
                _selfAligningTorqueSteerCoeff = Mathf.Lerp(5f, 1f, coeff);
            }

            Initialize();
        }

        public string Name => "Logitech G29 (Unity Input Fallback)";
        public float AccelerationInput { get; private set; } = 0;
        public float SteerAngleInput { get; private set; } = 0;
        public Gear GearInput { get; private set; } = Gear.Parking;
        public TurnIndicators TurnIndicatorsInput { get; private set; } = TurnIndicators.None;
        public HazardLights HazardLightsInput { get; private set; } = HazardLights.Disable;
        public bool SwitchAutonomous { get; private set; } = false;
        public bool Connected { get; private set; }

        [Header("Logitech G29 settings")]
        [SerializeField] string _devicePath = "/dev/input/event6";
        [SerializeField] float _kp = 5f;
        [SerializeField] float _ki = 0.2f;
        [SerializeField] float _kd = 0.05f;
        [SerializeField] float _minNormalizedSteeringTorque = 0.17f;
        [SerializeField] float _selfAligningTorqueSpeedCoeff = 5.2f;
        [SerializeField] float _selfAligningTorqueSteerCoeff = 1.8f;
        [SerializeField, Range(0f, 0.1f)] float _stationarySteeringResistance = 0.1f;

        [Header("Vehicle settings")]
        [SerializeField] Component _readonlyVehicleComponent = null;
        [SerializeField] AccelVehicleControlModeBasedInputter _controlModeBasedInputProvider = null;

        [Header("Unity Input Axis names")]
        [SerializeField] string _steerAxisName = "Joy X";
        [SerializeField] string _throttleAxisName = "Joy 3";
        [SerializeField] string _brakeAxisName = "Joy 4";

        [Header("Paddle shifter buttons")]
        [SerializeField] int _paddleUpButton = 4;
        [SerializeField] int _paddleDownButton = 5;

        [Header("Throttle / Brake multipliers")]
        [SerializeField, Range(0.1f, 3f)] float throttleMultiplier = 1f;
        [SerializeField, Range(0.1f, 3f)] float brakeMultiplier = 1f;

        IReadOnlyAccelVehicle _readonlyVehicle = null;
        PidController _pidController;

        float _throttlePedalInput = 0;
        float _brakePedalInput = 0;
        bool _steeringOverride = false;

        bool _prevPaddleUp = false;
        bool _prevPaddleDown = false;

        public void Initialize()
        {
            _readonlyVehicle = _readonlyVehicleComponent as IReadOnlyAccelVehicle;
            _pidController = new PidController(_kp, _ki, _kd);

            if (IsLinux())
            {
                Connected = LogitechG29Linux.InitDevice(_devicePath);
            }
            else
            {
                Connected = true;
            }
        }

        public bool UpdateInputs()
        {
            if (!Connected)
                return false;

            bool isOverridden = false;
            SwitchAutonomous = false;

            var currentControlMode = _controlModeBasedInputProvider.ControlMode;

            if (currentControlMode == ControlMode.Manual)
            {
                ManuallyInput();
                HandlePaddleShifter();
            }
            else if (currentControlMode == ControlMode.Autonomous)
            {
                if (_steeringOverride)
                {
                    ManuallyInput();
                    HandlePaddleShifter();
                    _steeringOverride = false;
                    isOverridden = true;
                }
            }

            return isOverridden;
        }

        void ManuallyInput()
        {
            if (IsLinux())
            {
                var currentPos = (float)LogitechG29Linux.GetPos();
                SteerAngleInput = _readonlyVehicle.MaxSteerTireAngleInput * currentPos;
            }
            else
            {
                float steerRaw = 0f;
                try { steerRaw = Input.GetAxis(_steerAxisName); } catch { steerRaw = 0f; }
                SteerAngleInput = _readonlyVehicle.MaxSteerTireAngleInput * steerRaw;
            }

            if (!IsLinux())
            {
                float rawThrottle = 0f;
                float rawBrake = 0f;
                try { rawThrottle = Input.GetAxis(_throttleAxisName); } catch { rawThrottle = 0f; }
                try { rawBrake = Input.GetAxis(_brakeAxisName); } catch { rawBrake = 0f; }

                _throttlePedalInput = NormalizePedalValue(rawThrottle);
                _brakePedalInput = NormalizePedalValue(rawBrake);
            }

            AccelerationInput = _readonlyVehicle.MaxAccelerationInput * _throttlePedalInput * throttleMultiplier;
            AccelerationInput += -_readonlyVehicle.MaxDecelerationInput * _brakePedalInput * brakeMultiplier;
        }

        void HandlePaddleShifter()
        {
            bool up = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + _paddleUpButton));
            bool down = Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + _paddleDownButton));

            if (up && !_prevPaddleUp)
                ShiftUp();

            if (down && !_prevPaddleDown)
                ShiftDown();

            _prevPaddleUp = up;
            _prevPaddleDown = down;
        }

        void ShiftUp()
        {
            switch (GearInput)
            {
                case Gear.Parking: GearInput = Gear.Reverse; break;
                case Gear.Reverse: GearInput = Gear.Neutral; break;
                case Gear.Neutral: GearInput = Gear.Drive; break;
            }
        }

        void ShiftDown()
        {
            switch (GearInput)
            {
                case Gear.Drive: GearInput = Gear.Neutral; break;
                case Gear.Neutral: GearInput = Gear.Reverse; break;
                case Gear.Reverse: GearInput = Gear.Parking; break;
            }
        }

        float NormalizePedalValue(float raw)
        {
            if (Mathf.Abs(raw) < 0.02f) return 0f;

            if (raw < -0.1f)
                return Mathf.Clamp01((raw + 1f) * 0.5f);
            else
                return Mathf.Clamp01(raw);
        }

        public void OnSteeringOverrideInput(InputAction.CallbackContext context)
        {
            _steeringOverride = true;
        }

        bool IsLinux()
        {
            return Application.platform == RuntimePlatform.LinuxEditor ||
                   Application.platform == RuntimePlatform.LinuxPlayer;
        }
    }
}
