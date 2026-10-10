using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Goal 1, Stage 1A: grey box of the S2 pool (Zone A) from docs/03_sets.md.
// Re-run from the menu to rebuild; it replaces the existing Pool_GreyBox.
//
// Layout is measured from the back-right corner (L, W), which the ref hides behind the booth.
// Back wall (+Z) holds the seats and stairs; right wall (+X) holds the lounger and parasol.
// Lane lines run parallel to the back wall and end in a T just short of the right wall.
public static class PoolGreyBoxBuilder
{
    // Pool basin, metres.
    const float L = 17.3f, W = 12f, H = 2.0f, T = 0.3f;

    // Stairs: a narrow flight standing out from the back wall, climbing toward the corner.
    const int StepCount = 8;
    const float StepRun = 0.45f, StairWidth = 0.9f, StairTopGap = 0.2f;

    // Lane centre lines, measured out from the back wall.
    static readonly float[] LaneOffsets = { 2.3f, 4.8f, 7.3f, 9.8f };

    static readonly Color Tile = new Color(0.62f, 0.82f, 0.86f);
    static readonly Color Deck = new Color(0.85f, 0.85f, 0.82f);
    static readonly Color Navy = new Color(0.10f, 0.15f, 0.35f);
    static readonly Color Cream = new Color(0.93f, 0.90f, 0.80f);
    static readonly Color Orange = new Color(0.95f, 0.50f, 0.15f);

    [MenuItem("KPop/Goal 1/Build Pool Grey Box")]
    public static void Build()
    {
        var old = GameObject.Find("Pool_GreyBox");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Pool_GreyBox").transform;

        var basin = Group("Basin", root);
        Box("Floor", new Vector3(L / 2, -0.05f, W / 2), new Vector3(L, 0.1f, W), Tile, basin);
        Box("Wall_Back", new Vector3(L / 2, H / 2, W + T / 2), new Vector3(L + 2 * T, H, T), Tile, basin);
        Box("Wall_Right", new Vector3(L + T / 2, H / 2, W / 2), new Vector3(T, H, W), Tile, basin);
        Box("Wall_Left", new Vector3(-T / 2, H / 2, W / 2), new Vector3(T, H, W), Tile, basin);
        Box("Wall_Front", new Vector3(L / 2, H / 2, -T / 2), new Vector3(L + 2 * T, H, T), Tile, basin);

        var deck = Group("Deck", root);
        Box("Deck_Back", new Vector3(L / 2, H - 0.05f, W + T + 2f), new Vector3(L + 8, 0.1f, 4f), Deck, deck);
        Box("Deck_Right", new Vector3(L + T + 2f, H - 0.05f, W / 2), new Vector3(4f, 0.1f, W + 2 * T), Deck, deck);
        Box("Deck_Left", new Vector3(-T - 2f, H - 0.05f, W / 2), new Vector3(4f, 0.1f, W + 2 * T), Deck, deck);
        Box("Deck_Front", new Vector3(L / 2, H - 0.05f, -T - 2f), new Vector3(L + 8, 0.1f, 4f), Deck, deck);

        var lanes = Group("LaneLines", root);
        const float laneW = 0.25f, laneStart = 1.0f, laneEnd = L - 1.8f;
        foreach (var off in LaneOffsets)
        {
            float z = W - off;
            Box("Lane_" + off, new Vector3((laneStart + laneEnd) / 2, 0.005f, z), new Vector3(laneEnd - laneStart, 0.01f, laneW), Navy, lanes);
            Box("LaneT_" + off, new Vector3(laneEnd - laneW / 2, 0.006f, z), new Vector3(laneW, 0.01f, 1.0f), Navy, lanes);
        }

        // Stairs: bottom step on the left, top step StairTopGap short of the corner, at deck level.
        var stairs = Group("PoolStairs", root);
        float rise = H / StepCount, span = StepCount * StepRun;
        float x0 = L - StairTopGap - span, zc = W - StairWidth / 2;
        for (int i = 0; i < StepCount; i++)
        {
            float top = (i + 1) * rise, x = x0 + i * StepRun;
            Box("Step_" + i, new Vector3(x + StepRun / 2, top / 2, zc), new Vector3(StepRun, top, StairWidth), Cream, stairs);
            Box("Nosing_" + i, new Vector3(x + 0.03f, top + 0.005f, zc), new Vector3(0.06f, 0.01f, StairWidth), Orange, stairs);
        }
        // Handrail along the open (pool-side) edge, 0.9 m above the nosings.
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rail.name = "Handrail";
        rail.transform.SetParent(stairs, false);
        rail.transform.localPosition = new Vector3(x0 + span / 2, H / 2 + 0.9f, W - StairWidth + 0.05f);
        rail.transform.localScale = new Vector3(0.05f, Mathf.Sqrt(span * span + H * H) / 2, 0.05f);
        rail.transform.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(span, H) * Mathf.Rad2Deg);

        // Props modelled in Blender (blender/pool/export_props.py), placed by their bottom-centre pivot.
        // Positions were lined up against ref01_pool_01 with the ref overlay.
        var props = Group("Props", root);
        Prop("PoolKiosk", new Vector3(L - 1.6f, 0, W - 1.25f), props);
        Prop("PoolSeats", new Vector3(x0 - 1.0f, 0, W - 0.25f), props);
        Prop("PoolLounger", new Vector3(L - 1.85f, 0, W - 4.75f), props, -35f);   // angled foot-toward-camera, as in the ref
        Prop("PoolSideTable", new Vector3(L - 1.5f, 0, W - 5.0f), props);
        Prop("PoolParasol", new Vector3(L - 0.9f, 0, W - 5.05f), props, 180f);   // pole leans toward the right wall
        Prop("Balloon_Lavender", new Vector3(L - 1.8f, 0, W - 2.1f), props);
        Prop("Balloon_MintA", new Vector3(L - 2.65f, 0, W - 3.4f), props);
        Prop("Balloon_MintB", new Vector3(L - 0.8f, 0, W - 6.0f), props);
        Prop("Balloon_Pink", new Vector3(L - 1.3f, 0, W - 8.0f), props);
        Prop("Balloon_Lilac", new Vector3(L - 0.8f, 0, W - 9.5f), props);

        // 1.65 m stand-ins for the two dancers, between lanes 1 and 2.
        var markers = Group("ScaleMarkers_165cm", root);
        foreach (var p in new[] { new Vector3(L - 4.3f, 0, W - 4.2f), new Vector3(L - 3.4f, 0, W - 5.75f) })
        {
            var cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cap.name = "Dancer_Marker";
            cap.transform.SetParent(markers, false);
            cap.transform.localPosition = p + Vector3.up * 0.825f;
            cap.transform.localScale = new Vector3(0.4f, 0.825f, 0.4f);
        }

        // High-angle wide, matching ref01_pool_01: long lens from the front-left, looking into the corner.
        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            cam = new GameObject("CAM_Wide_HighAngle", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.name = "CAM_Wide_HighAngle";
        cam.transform.position = new Vector3(L - 7.8f, 5.0f, W - 10.5f);
        cam.transform.rotation = Quaternion.Euler(31.8f, 35.3f, 0f);
        cam.fieldOfView = 27f;

        var sun = Object.FindFirstObjectByType<Light>();
        if (sun == null)
        {
            sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.transform.rotation = Quaternion.Euler(70, 30, 0);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    const string PropDir = "Assets/_Project/Sets/Pool/Props/";
    // Blender props import facing +Z; turn them so their fronts face into the pool (-Z).
    const float PropYaw = 180f;

    static void Prop(string file, Vector3 floorPos, Transform parent, float yaw = 0f)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PropDir + file + ".fbx");
        if (asset == null)
        {
            Debug.LogWarning("PoolGreyBoxBuilder: missing prop " + PropDir + file + ".fbx (run blender/pool/export_props.py)");
            return;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        // Single-mesh FBX files import with the mesh offset on the root (e.g. a balloon's height), so keep it.
        var turn = Quaternion.Euler(0, PropYaw + yaw, 0);
        go.transform.localPosition = floorPos + turn * asset.transform.localPosition;
        go.transform.localRotation = turn * asset.transform.localRotation;
    }

    static Transform Group(string name, Transform parent)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    static void Box(string name, Vector3 center, Vector3 size, Color color, Transform parent)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = center;
        g.transform.localScale = size;
        var r = g.GetComponent<Renderer>();
        r.sharedMaterial = new Material(r.sharedMaterial) { color = color };
    }
}
