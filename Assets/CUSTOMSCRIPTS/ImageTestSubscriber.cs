using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ImageTestSubscriber : MonoBehaviour
{
    void Start()
    {
        var ros = FindObjectOfType<ROSConnection>();
        if (ros == null) { Debug.LogError("ROSConnection not found"); return; }
        ros.Subscribe<ImageMsg>("/sensing/camera/traffic_light/image_binary", OnImage);
    }
    void OnImage(ImageMsg msg) { Debug.Log("1"); }
}
