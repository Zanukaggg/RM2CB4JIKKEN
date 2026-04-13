using UnityEngine;
using TMPro;
using Awsim.Entity;

public class AccelVehicleMeterStandaloneUI : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] AccelVehicle _vehicle;

    [Header("UI")]
    [SerializeField] TMP_Text _speedValueText;
    [SerializeField] TMP_Text _gearValueText;

    void Update()
    {
        if (_vehicle == null)
            return;

        // Speed (m/s → km/h)
        _speedValueText.text = (_vehicle.Speed * 3.6f).ToString("F0");

        // Gear
        switch (_vehicle.Gear)
        {
            case Gear.Drive:
                _gearValueText.text = "D";
                break;
            case Gear.Neutral:
                _gearValueText.text = "N";
                break;
            case Gear.Parking:
                _gearValueText.text = "P";
                break;
            case Gear.Reverse:
                _gearValueText.text = "R";
                break;
        }
    }
}
