using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class NPCWalker : MonoBehaviour
{
    private Spline spline;
    private float baseSpeed;
    private float speedMultiplier = 1f;
    private float t = 0f;
    private Transform splineTransform; // SplineContainer Transform

    public bool ReachedEnd { get; private set; } = false;

    /// <summary>
    /// 初始化行人
    /// </summary>
    /// <param name="spline">Spline 路线</param>
    /// <param name="speed">基础速度</param>
    /// <param name="containerTransform">SplineContainer Transform，用于世界坐标转换</param>
    public void Init(Spline spline, float speed, Transform containerTransform)
    {
        this.spline = spline;
        this.baseSpeed = speed;
        this.splineTransform = containerTransform;
        t = 0f;
        ReachedEnd = false;
    }

    public void UpdateSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void MoveAlongSpline()
    {
        if (ReachedEnd || spline == null || splineTransform == null) return;

        t += baseSpeed * speedMultiplier * Time.deltaTime / spline.GetLength();

        if (t >= 1f)
        {
            t = 1f;
            ReachedEnd = true;
        }

        // 世界坐标位置
        float3 floatPos = spline.EvaluatePosition(t);
        transform.position = splineTransform.TransformPoint((Vector3)floatPos);

        // 世界坐标朝向
        float3 floatDir = spline.EvaluateTangent(t);
        Vector3 dir = ((Vector3)floatDir).normalized;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
    }
}
