using System.IO;
using UnityEditor;
using UnityEngine;

// Renders the main camera at a reference frame's size and blends it 50/50 over the ref,
// so set geometry and camera can be lined up by eye. Output goes to captures/_overlay/ (git-ignored).
public static class RefOverlay
{
    const string RefPath = "../docs/refs/pool/ref01_pool_01_wide_highangle_0023.jpg";

    [MenuItem("KPop/Goal 1/Capture Ref Overlay")]
    public static void Capture()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string refFile = Path.GetFullPath(Path.Combine(projectRoot, RefPath));
        var refTex = new Texture2D(2, 2);
        refTex.LoadImage(File.ReadAllBytes(refFile));
        int w = refTex.width, h = refTex.height;

        var cam = Camera.main;
        var rt = new RenderTexture(w, h, 24);
        var prevTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.aspect = (float)w / h;
        cam.Render();
        RenderTexture.active = rt;
        var render = new Texture2D(w, h, TextureFormat.RGB24, false);
        render.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        render.Apply();
        RenderTexture.active = null;
        cam.targetTexture = prevTarget;
        cam.ResetAspect();
        Object.DestroyImmediate(rt);

        var a = render.GetPixels();
        var b = refTex.GetPixels();
        var blend = new Color[a.Length];
        for (int i = 0; i < a.Length; i++) blend[i] = Color.Lerp(a[i], b[i], 0.5f);
        var outTex = new Texture2D(w, h, TextureFormat.RGB24, false);
        outTex.SetPixels(blend);
        outTex.Apply();

        string outDir = Path.GetFullPath(Path.Combine(projectRoot, "../captures/_overlay"));
        Directory.CreateDirectory(outDir);
        File.WriteAllBytes(Path.Combine(outDir, "ref_overlay.png"), outTex.EncodeToPNG());
        File.WriteAllBytes(Path.Combine(outDir, "ref_render.png"), render.EncodeToPNG());
        Debug.Log("RefOverlay: wrote ref_overlay.png and ref_render.png to " + outDir + " (" + w + "x" + h + ")");
    }
}
