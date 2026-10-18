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

    // Mosaic tiles, sampled from ref01_pool_01: a pale aqua field with scattered darker teal tiles.
    // 2.5 cm tiles. In the ref the dark tiles sit on every other tile (a 5 cm lattice) and never touch,
    // so a 25 cm lane spans 5 dot rows. Lane edges and T ends all fall on the 2.5 cm grid.
    const float TileSize = 0.025f;
    const float DotChance = 0.9f;
    static readonly (string hex, float weight)[] PoolPalette = { ("#CCE6E3", 42), ("#BEDDDA", 30), ("#5FA3AA", 18), ("#4A93A0", 10) };
    static readonly (string hex, float weight)[] LanePalette = { ("#2B4C7E", 55), ("#36598C", 25), ("#22406E", 12), ("#5C82B0", 8) };
    const string PoolGrout = "#DDEDEB", LaneGrout = "#7E9FBE";

    static readonly Color Deck = new Color(0.85f, 0.85f, 0.82f);
    static readonly Color Cream = new Color(0.93f, 0.90f, 0.80f);
    static readonly Color Orange = new Color(0.95f, 0.50f, 0.15f);

    [MenuItem("KPop/Goal 1/Build Pool Grey Box")]
    public static void Build()
    {
        var old = GameObject.Find("Pool_GreyBox");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Pool_GreyBox").transform;

        var poolTiles = MosaicMaterial("M_PoolMosaic", PoolPalette, PoolGrout, 0f, dotLattice: true);
        var laneTiles = MosaicMaterial("M_LaneMosaic", LanePalette, LaneGrout, 17f, dotLattice: false);

        var basin = Group("Basin", root);
        Box("Floor", new Vector3(L / 2, -0.05f, W / 2), new Vector3(L, 0.1f, W), poolTiles, basin);
        Box("Wall_Back", new Vector3(L / 2, H / 2, W + T / 2), new Vector3(L + 2 * T, H, T), poolTiles, basin);
        Box("Wall_Right", new Vector3(L + T / 2, H / 2, W / 2), new Vector3(T, H, W), poolTiles, basin);
        Box("Wall_Left", new Vector3(-T / 2, H / 2, W / 2), new Vector3(T, H, W), poolTiles, basin);
        Box("Wall_Front", new Vector3(L / 2, H / 2, -T / 2), new Vector3(L + 2 * T, H, T), poolTiles, basin);

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
            Box("Lane_" + off, new Vector3((laneStart + laneEnd) / 2, 0.005f, z), new Vector3(laneEnd - laneStart, 0.01f, laneW), laneTiles, lanes);
            Box("LaneT_" + off, new Vector3(laneEnd - laneW / 2, 0.006f, z), new Vector3(laneW, 0.01f, 1.0f), laneTiles, lanes);
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
        Balloon(Prop("Balloon_Lavender", new Vector3(L - 1.8f, 0, W - 2.1f), props), "#C7B8E8");
        Balloon(Prop("Balloon_MintA", new Vector3(L - 4.18f, 0, W - 3.4f), props), "#BFE7B8");   // moved left by the director in the scene (2026-10-17)
        Balloon(Prop("Balloon_MintB", new Vector3(L - 0.8f, 0, W - 6.0f), props), "#BCE6C8");
        Balloon(Prop("Balloon_Pink", new Vector3(L - 1.3f, 0, W - 8.0f), props), "#F2C4D6");
        Balloon(Prop("Balloon_Lilac", new Vector3(L - 0.8f, 0, W - 9.5f), props), "#CFC4EE");

        // Bunting: two strings fitted to ref01_pool_01 by casting rays from the matched camera.
        // They hang over the pool from tie points just above the walls and meet near (9.8, 2.8, 11.05).
        var bunting = Group("Bunting", root);
        Bunting("Bunting_Back", new Vector3(8.79f, 3.19f, W), new Vector3(L, 3.19f, 4.03f), 0.87f, bunting);   // diagonal across the back-right corner
        Bunting("Bunting_Near", new Vector3(10.74f, 2.99f, 0f), new Vector3(9.82f, 2.99f, W), 0.59f, bunting);   // lengthwise, mostly out of frame

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

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    const string PropDir = "Assets/_Project/Sets/Pool/Props/";
    // Blender props import facing +Z; turn them so their fronts face into the pool (-Z).
    const float PropYaw = 180f;

    static GameObject Prop(string file, Vector3 floorPos, Transform parent, float yaw = 0f)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PropDir + file + ".fbx");
        if (asset == null)
        {
            Debug.LogWarning("PoolGreyBoxBuilder: missing prop " + PropDir + file + ".fbx (run blender/pool/export_props.py)");
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        // Single-mesh FBX files import with the mesh offset on the root (e.g. a balloon's height), so keep it.
        var turn = Quaternion.Euler(0, PropYaw + yaw, 0);
        go.transform.localPosition = floorPos + turn * asset.transform.localPosition;
        go.transform.localRotation = turn * asset.transform.localRotation;
        return go;
    }

    // Swaps a balloon's imported (opaque) material for a see-through KPop/Balloon one.
    static void Balloon(GameObject go, string hex)
    {
        if (go == null) return;
        var mat = MaterialAsset("M_" + go.name, "KPop/Balloon");
        mat.SetColor("_BaseColor", Hex(hex));
        mat.SetFloat("_AlphaCenter", 0.65f);
        mat.SetFloat("_AlphaEdge", 0.95f);
        foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
    }

    // Bunting flags in the ref's order along the string.
    static readonly string[] FlagColours = { "#F28DB2", "#3D7FD1", "#2BB3C0", "#F5C842", "#B79CE0" };
    const float FlagWidth = 0.27f, FlagDrop = 0.32f, FlagSpacing = 0.36f, StringRadius = 0.006f;

    // A sagging string from a to b (parabolic sag at the middle), with triangle flags hanging from it.
    // Meshes are saved as assets so the scene keeps them.
    static void Bunting(string name, Vector3 a, Vector3 b, float sag, Transform parent)
    {
        const int seg = 60;
        var pts = new Vector3[seg + 1];
        for (int i = 0; i <= seg; i++)
        {
            float t = (float)i / seg;
            pts[i] = Vector3.Lerp(a, b, t) + Vector3.down * (sag * 4 * t * (1 - t));
        }

        var group = Group(name, parent);
        var stringMat = LitMaterial("M_BuntingString", "#F4F2EE");
        Child(name + "_String", TubeMesh(name + "_String", pts, StringRadius), stringMat, group);

        // Walk the string at a fixed spacing and give each flag the next colour.
        var flags = new System.Collections.Generic.List<Vector3>[FlagColours.Length];
        for (int c = 0; c < flags.Length; c++) flags[c] = new System.Collections.Generic.List<Vector3>();
        float next = FlagSpacing / 2, walked = 0;
        int n = 0;
        for (int i = 0; i < seg; i++)
        {
            float len = Vector3.Distance(pts[i], pts[i + 1]);
            while (next <= walked + len)
            {
                Vector3 p = Vector3.Lerp(pts[i], pts[i + 1], (next - walked) / len);
                Vector3 side = (pts[i + 1] - pts[i]).normalized * (FlagWidth / 2);
                flags[n % flags.Length].AddRange(new[] { p - side, p + side, p + Vector3.down * FlagDrop });
                n++;
                next += FlagSpacing;
            }
            walked += len;
        }
        for (int c = 0; c < flags.Length; c++)
            Child(name + "_Flags_" + c, TriangleMesh(name + "_Flags_" + c, flags[c]), LitMaterial("M_Flag_" + c, FlagColours[c]), group);
    }

    const string GeneratedDir = "Assets/_Project/Sets/Pool/Generated/";

    static Mesh SaveMesh(Mesh mesh, string name)
    {
        System.IO.Directory.CreateDirectory(GeneratedDir);
        string path = GeneratedDir + name + ".asset";
        AssetDatabase.DeleteAsset(path);
        mesh.name = name;
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // Double-sided triangles (both windings), so flags read from either side.
    static Mesh TriangleMesh(string name, System.Collections.Generic.List<Vector3> verts)
    {
        var all = new System.Collections.Generic.List<Vector3>(verts);
        all.AddRange(verts);
        var tris = new int[verts.Count * 2];
        for (int i = 0; i < verts.Count; i += 3)
        {
            tris[i] = i; tris[i + 1] = i + 1; tris[i + 2] = i + 2;
            int j = verts.Count + i;
            tris[verts.Count + i] = j; tris[verts.Count + i + 1] = j + 2; tris[verts.Count + i + 2] = j + 1;
        }
        var mesh = new Mesh { vertices = all.ToArray(), triangles = tris };
        mesh.RecalculateNormals();
        return SaveMesh(mesh, name);
    }

    static Mesh TubeMesh(string name, Vector3[] path, float radius, int sides = 6)
    {
        var verts = new Vector3[path.Length * sides];
        var tris = new int[(path.Length - 1) * sides * 6];
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 fwd = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
            Vector3 u = Vector3.Cross(fwd, Vector3.up).normalized, v = Vector3.Cross(u, fwd);
            for (int s = 0; s < sides; s++)
            {
                float ang = s * Mathf.PI * 2 / sides;
                verts[i * sides + s] = path[i] + (u * Mathf.Cos(ang) + v * Mathf.Sin(ang)) * radius;
            }
        }
        int k = 0;
        for (int i = 0; i < path.Length - 1; i++)
            for (int s = 0; s < sides; s++)
            {
                int a0 = i * sides + s, a1 = i * sides + (s + 1) % sides, b0 = a0 + sides, b1 = a1 + sides;
                tris[k++] = a0; tris[k++] = b0; tris[k++] = a1;
                tris[k++] = a1; tris[k++] = b0; tris[k++] = b1;
            }
        var mesh = new Mesh { vertices = verts, triangles = tris };
        mesh.RecalculateNormals();
        return SaveMesh(mesh, name);
    }

    static void Child(string name, Mesh mesh, Material mat, Transform parent)
    {
        var g = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        g.transform.SetParent(parent, false);
        g.GetComponent<MeshFilter>().sharedMesh = mesh;
        g.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // Creates or updates a material asset on the given shader.
    static Material MaterialAsset(string name, string shader)
    {
        string path = MaterialDir + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            System.IO.Directory.CreateDirectory(MaterialDir);
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // Matte URP Lit material (bunting). Back-face culled: the flag meshes carry both windings already.
    static Material LitMaterial(string name, string hex)
    {
        var mat = MaterialAsset(name, "Universal Render Pipeline/Lit");
        mat.SetColor("_BaseColor", Hex(hex));
        mat.SetFloat("_Smoothness", 0.15f);
        mat.SetFloat("_Cull", 2f);
        return mat;
    }

    static Transform Group(string name, Transform parent)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    const string MaterialDir = "Assets/_Project/Sets/Pool/Materials/";

    // Creates or updates a KPop/Mosaic material asset, so the palette here stays the source of truth.
    static Material MosaicMaterial(string name, (string hex, float weight)[] palette, string grout, float seed, bool dotLattice)
    {
        string path = MaterialDir + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            System.IO.Directory.CreateDirectory(MaterialDir);
            mat = new Material(Shader.Find("KPop/Mosaic"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetFloat("_TileSize", TileSize);
        mat.SetVector("_GridOffset", Vector4.zero);
        mat.SetFloat("_DotLattice", dotLattice ? 1f : 0f);
        mat.SetFloat("_DotChance", DotChance);
        mat.SetFloat("_Seed", seed);
        mat.SetColor("_GroutColor", Hex(grout));
        for (int i = 0; i < 4; i++) mat.SetColor("_Color" + i, Hex(palette[i].hex));
        mat.SetVector("_Weights", new Vector4(palette[0].weight, palette[1].weight, palette[2].weight, palette[3].weight));
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        return mat;
    }

    static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

    static void Box(string name, Vector3 center, Vector3 size, Color color, Transform parent)
    {
        var r = Box(name, center, size, parent);
        r.sharedMaterial = new Material(r.sharedMaterial) { color = color };
    }

    static void Box(string name, Vector3 center, Vector3 size, Material mat, Transform parent)
    {
        Box(name, center, size, parent).sharedMaterial = mat;
    }

    static Renderer Box(string name, Vector3 center, Vector3 size, Transform parent)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = center;
        g.transform.localScale = size;
        return g.GetComponent<Renderer>();
    }
}
