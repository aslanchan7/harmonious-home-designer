using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Furniture))]
class FurnitureEditor : Editor
{
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Furniture furniture = target as Furniture;
        if (furniture == null)
            return;
        SerializableRow<Vector2>[] stackOffsets = furniture.stackOffsets;
        if (stackOffsets == null)
            return;
        Vector3 basePoint = furniture.transform.position;
        basePoint.y = furniture.Colliders[0].bounds.max.y;
        basePoint.x -= furniture.Size.x / 2.0f - 0.5f;
        basePoint.z -= furniture.Size.y / 2.0f - 0.5f;

        for (int i = 0; i < stackOffsets.Length; i++)
        {
            Vector2[] row = stackOffsets[i].row;
            for (int j = 0; j < row.Length; j++)
            {
                Vector3 lineStart =
                    basePoint
                    + new Vector3(j, 0, i)
                    + new Vector3(row[j].x, 0, row[j].y);
                Handles.DrawLine(lineStart, lineStart + Vector3.up / 2);
            }
        }
    }
}
