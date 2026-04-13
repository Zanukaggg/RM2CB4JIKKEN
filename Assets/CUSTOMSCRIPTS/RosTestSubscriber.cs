using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class AutoRosTestSubscriber : MonoBehaviour
{
    ROSConnection rosConnection;

    void Start()
    {
        // 自动查找场景里的 ROSConnection
        rosConnection = FindObjectOfType<ROSConnection>();
        if (rosConnection == null)
        {
            Debug.LogError("场景中没有 ROSConnection 对象！");
            return;
        }

        // 订阅 /test topic
        rosConnection.Subscribe<StringMsg>("/test", ReceiveMessage);
        Debug.Log("已订阅 /test topic");
    }

    void ReceiveMessage(StringMsg msg)
    {
        Debug.Log("收到 ROS2 消息: " + msg.data);
    }
}
