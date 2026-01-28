using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

public class TrafficLightNPCControllerMultiSpline : MonoBehaviour
{
    [Header("Spline 设置")]
    public SplineContainer[] splineContainers; // 多条行人路线
    public GameObject npcPrefab;               // NPC 预制体

    [Header("生成参数")]
    public float spawnIntervalMin = 2f;        
    public float spawnIntervalMax = 5f;
    public float speedMin = 1.5f;
    public float speedMax = 3f;
    public float yellowSpeedMultiplier = 1.2f;

    [Header("红绿灯时间 (秒)")]
    public float redDuration = 5f;     // NPC 生成时间
    public float greenDuration = 10f;  // 停止生成
    public float yellowDuration = 3f;  // NPC 加速

    private enum LightState { RED, YELLOW, GREEN }
    private LightState currentState = LightState.RED;
    private float stateTimer = 0f;

    private float nextSpawnTime = 0f;
    private List<GameObject> activeNPCs = new List<GameObject>();

    void Start()
    {
        SetLightState(LightState.RED);
    }

    void Update()
    {
        UpdateTrafficLight();

        // 生成控制：只在 RED 时生成
        if (currentState == LightState.RED && Time.time >= nextSpawnTime)
        {
            SpawnNPC();
            nextSpawnTime = Time.time + UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
        }

        // 更新所有 NPC
        for (int i = activeNPCs.Count - 1; i >= 0; i--)
        {
            var npc = activeNPCs[i];
            if (npc == null)
            {
                activeNPCs.RemoveAt(i);
                continue;
            }

            var walker = npc.GetComponent<NPCWalker>();
            if (walker == null) continue;

            // 黄灯加速
            float speedMultiplier = (currentState == LightState.YELLOW) ? yellowSpeedMultiplier : 1f;
            walker.UpdateSpeedMultiplier(speedMultiplier);
            walker.MoveAlongSpline();

            if (walker.ReachedEnd)
            {
                Destroy(npc);
                activeNPCs.RemoveAt(i);
            }
        }
    }

    void UpdateTrafficLight()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case LightState.RED:
                if (stateTimer >= redDuration) SetLightState(LightState.YELLOW);
                break;
            case LightState.YELLOW:
                if (stateTimer >= yellowDuration) SetLightState(LightState.GREEN);
                break;
            case LightState.GREEN:
                if (stateTimer >= greenDuration) SetLightState(LightState.RED);
                break;
        }
    }

    void SetLightState(LightState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        gameObject.tag = newState.ToString();
        Debug.Log($"Traffic Light switched to {newState}");
    }

    void SpawnNPC()
    {
        if (splineContainers == null || splineContainers.Length == 0 || npcPrefab == null) return;

        // 随机选择一条 splineContainer
        SplineContainer container = splineContainers[UnityEngine.Random.Range(0, splineContainers.Length)];
        if (container == null || container.Spline == null) return;

        // 生成位置 = Spline 起点 + Container Transform
        Vector3 spawnPos = container.transform.TransformPoint((Vector3)container.Spline.EvaluatePosition(0f));

        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        float randomSpeed = UnityEngine.Random.Range(speedMin, speedMax);

        var walker = npc.AddComponent<NPCWalker>();
        walker.Init(container.Spline, randomSpeed, container.transform);
        activeNPCs.Add(npc);
    }
}
