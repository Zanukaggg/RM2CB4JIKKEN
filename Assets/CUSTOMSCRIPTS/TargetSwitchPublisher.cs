using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class TargetSwitchPublisher : MonoBehaviour
{
    [Header("ROS Topics")]
    public string nextTopic = "/next_target";
    public string prevTopic = "/prev_target";

    [Header("Key Bindings")]
    public KeyCode nextKey = KeyCode.Plus;
    public KeyCode prevKey = KeyCode.Minus;

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<EmptyMsg>(nextTopic);
        ros.RegisterPublisher<EmptyMsg>(prevTopic);
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey))
        {
            ros.Publish(nextTopic, new EmptyMsg());
            Debug.Log("Published next target");
        }
        if (Input.GetKeyDown(prevKey))
        {
            ros.Publish(prevTopic, new EmptyMsg());
            Debug.Log("Published previous target");
        }
    }
}