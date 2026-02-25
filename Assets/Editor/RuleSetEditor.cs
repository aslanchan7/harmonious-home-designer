using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RuleSet))]
class RuleSetEditor : Editor
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
        RuleSet ruleSet = target as RuleSet;

        if (ruleSet == null || ruleSet.zones == null)
            return;

        foreach (FixedItemEntry fixedItemEntry in ruleSet.fixedItems)
        {
            if (!fixedItemEntry.showInEditor)
                continue;
            DirectedBox zone = fixedItemEntry.boundingBox;
            Vector3[] vertices = new Vector3[]
            {
                ToWorldSpace(zone.position),
                ToWorldSpace(new Vector2(zone.oppositeX, zone.y)),
                ToWorldSpace(zone.opposite),
                ToWorldSpace(new Vector2(zone.x, zone.oppositeY)),
            };
            Handles.DrawSolidRectangleWithOutline(
                vertices,
                Color.green,
                Color.black
            );
            Handles.DrawLine(
                ToWorldSpace(zone.center),
                ToWorldSpace(
                    zone.center
                        + (Vector2)
                            DirectionExtension
                                .FromAngle(zone.rotation)
                                .ToVector() / 2.0f
                ),
                2
            );
        }

        foreach (ZoneEntry zoneEntry in ruleSet.zones)
        {
            if (!zoneEntry.showInEditor)
                continue;
            BoundingBox zone = zoneEntry.zone;
            Vector3[] vertices = new Vector3[]
            {
                ToWorldSpace(zone.position),
                ToWorldSpace(new Vector2(zone.oppositeX, zone.y)),
                ToWorldSpace(zone.opposite),
                ToWorldSpace(new Vector2(zone.x, zone.oppositeY)),
            };
            Handles.DrawSolidRectangleWithOutline(
                vertices,
                Color.green,
                Color.black
            );
        }
    }

    Vector3 ToWorldSpace(Vector2 position)
    {
        return new(position.x, 0.05f, position.y);
    }
}
