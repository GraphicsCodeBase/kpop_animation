using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch-mode entry point (editor closed): rebuild the pool and capture review images.
//   Unity.exe -batchmode -projectPath unity -executeMethod BatchCapture.Goal01Pool -quit -logFile -
// Writes the ref overlay plus extra views to captures/_overlay/ (git-ignored); copy keepers to captures/goal01_pool/.
public static class BatchCapture
{
    const string ScenePath = "Assets/_Project/Scenes/Goal01_Pool.unity";

    public static void Goal01Pool()
    {
        // Otherwise the first frames render before new shader variants finish compiling.
        ShaderUtil.allowAsyncCompilation = false;
        EditorSceneManager.OpenScene(ScenePath);
        PoolGreyBoxBuilder.Build();
        RefOverlay.Capture();

        var main = Camera.main;
        Shot(main, "wide_1080", 1920, 1080);
        // Eye-level look across the floor toward the back wall, for tile detail.
        Shot(main, "closeup_floor", 1280, 720, new Vector3(9.5f, 1.4f, 4.5f), Quaternion.Euler(18f, 40f, 0f), 45f);
        // Close on the back wall and a lane T end.
        Shot(main, "closeup_wall", 1280, 720, new Vector3(13.5f, 1.2f, 7.5f), Quaternion.Euler(10f, 15f, 0f), 35f);
    }

    static void Shot(Camera cam, string name, int w, int h, Vector3? pos = null, Quaternion? rot = null, float? fov = null)
    {
        var t = cam.transform;
        Vector3 p0 = t.position; Quaternion r0 = t.rotation; float f0 = cam.fieldOfView;
        if (pos.HasValue) t.position = pos.Value;
        if (rot.HasValue) t.rotation = rot.Value;
        if (fov.HasValue) cam.fieldOfView = fov.Value;

        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.aspect = (float)w / h;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        cam.ResetAspect();
        Object.DestroyImmediate(rt);

        t.position = p0; t.rotation = r0; cam.fieldOfView = f0;

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../captures/_overlay"));
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
        Debug.Log("BatchCapture: wrote " + name + ".png");
    }
}
