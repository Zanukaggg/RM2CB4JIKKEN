using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
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

    [Header("生成历史上限")]
    [Tooltip("是否启用历史累计生成上限（达到上限后不再生成新的 NPC）")]
    public bool limitMaxSpawnCount = false;
    [Tooltip("启用历史上限时允许的最大累计生成次数")]
    public int maxSpawnCount = 10;

    // 运行时的累计计数（历史计数：生成一次即 +1，销毁不减）
    [SerializeField, HideInInspector]
    private int spawnedCount = 0;

    private enum LightState { RED, YELLOW, GREEN }
    private LightState currentState = LightState.RED;
    private float stateTimer = 0f;

    private float nextSpawnTime = 0f;
    private List<GameObject> activeNPCs = new List<GameObject>();

    void Start()
    {
        SetLightState(LightState.RED);
        // 初始化下一次生成时间，避免一开始就立刻生成（如果需要可改）
        nextSpawnTime = Time.time + UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void Update()
    {
        UpdateTrafficLight();

        // 生成控制：只在 RED 时生成
        if (currentState == LightState.RED && Time.time >= nextSpawnTime)
        {
            // 如果启用了历史上限且达到上限，则不再生成
            if (limitMaxSpawnCount && spawnedCount >= maxSpawnCount)
            {
                // 可选：只在首次达到上限时打印一次
                if (spawnedCount == maxSpawnCount)
                {
                    Debug.Log($"Spawn limit reached: {spawnedCount}/{maxSpawnCount}. No more NPC will be spawned.");
                    // 将 spawnedCount 增加一以避免重复打印，若希望持续打印可删除下一行
                    spawnedCount++; // 用一个超出值标记为已打印（保留历史语义）
                }
            }
            else
            {
                SpawnNPC();
                nextSpawnTime = Time.time + UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
            }
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

        // 历史计数：生成成功后 +1（不可逆）
        spawnedCount++;

        // 如果启用了限制并且刚好到达上限，打印信息（spawnedCount 记录真实生成次数）
        if (limitMaxSpawnCount && spawnedCount == maxSpawnCount)
        {
            Debug.Log($"Spawn limit reached exactly: {spawnedCount}/{maxSpawnCount}.");
        }
    }

    /// <summary>
    /// 在 Inspector 的右键菜单或脚本组件菜单中可调用，重置累计生成计数。
    /// 注意：这只重置计数，不影响已存在的 NPC（除非你手动清理）。
    /// </summary>
    [ContextMenu("Reset Spawned Count")]
    public void ResetSpawnedCount()
    {
        spawnedCount = 0;
        Debug.Log("Spawned count reset to 0.");
    }

    /// <summary>
    /// 供运行时通过脚本设置/查询的接口（可选）
    /// </summary>
    public int GetSpawnedCount()
    {
        return spawnedCount;
    }

    public void SetMaxSpawnCount(int newMax)
    {
        maxSpawnCount = Mathf.Max(0, newMax);
    }
}
