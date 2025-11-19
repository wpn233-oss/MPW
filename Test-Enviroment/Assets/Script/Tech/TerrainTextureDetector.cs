using UnityEngine;

public class TerrainTextureDetector : MonoBehaviour
{
    [Header("检测设置")]
    public float checkDistance = 0.5f;
    public LayerMask terrainLayer;

    private Terrain currentTerrain;
    private int terrainTextureIndex = -1;

    public string CurrentTextureName { get; private set; } = "Grass";

    void Update()
    {
        DetectTerrainTexture();
    }

    private void DetectTerrainTexture()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, terrainLayer))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                currentTerrain = terrain;
                terrainTextureIndex = GetMainTextureAtPosition(hit.point);
                CurrentTextureName = GetTextureNameFromIndex(terrainTextureIndex);
            }
        }
    }

    private int GetMainTextureAtPosition(Vector3 worldPos)
    {
        if (currentTerrain == null || currentTerrain.terrainData == null)
            return -1;

        // 将世界坐标转换为地形纹理坐标
        Vector3 terrainPos = worldPos - currentTerrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            terrainPos.x / currentTerrain.terrainData.size.x,
            0,
            terrainPos.z / currentTerrain.terrainData.size.z
        );

        // 获取纹理混合图
        int x = (int)(normalizedPos.x * currentTerrain.terrainData.alphamapWidth);
        int z = (int)(normalizedPos.z * currentTerrain.terrainData.alphamapHeight);

        float[,,] alphaMap = currentTerrain.terrainData.GetAlphamaps(x, z, 1, 1);

        // 找到权重最大的纹理索引
        float maxWeight = 0;
        int maxIndex = 0;
        for (int i = 0; i < alphaMap.GetLength(2); i++)
        {
            if (alphaMap[0, 0, i] > maxWeight)
            {
                maxWeight = alphaMap[0, 0, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    private string GetTextureNameFromIndex(int index)
    {
        if (currentTerrain == null || index < 0)
            return "Grass";

        // 获取地形纹理名称
        if (index < currentTerrain.terrainData.terrainLayers.Length)
        {
            return currentTerrain.terrainData.terrainLayers[index].name;
        }

        return "Grass";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);
    }
}