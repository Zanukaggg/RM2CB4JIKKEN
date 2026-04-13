using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ImageBinaryViewer : MonoBehaviour
{
    public RawImage rawImage;  // 场景里拖入
    private Texture2D texture;
    private ROSConnection ros;

    void Start()
    {
        ros = FindObjectOfType<ROSConnection>();
        ros.Subscribe<ImageMsg>("/sensing/camera/traffic_light/image_binary", OnImage);
    }

    void OnImage(ImageMsg msg)
    {
        // 创建 Texture2D（第一次）
        if (texture == null || texture.width != (int)msg.width || texture.height != (int)msg.height)
        {
            texture = new Texture2D((int)msg.width, (int)msg.height, TextureFormat.RGB24, false);
            rawImage.texture = texture;
        }

        // 填充像素
        texture.LoadRawTextureData(msg.data);
        texture.Apply();
    }
}
