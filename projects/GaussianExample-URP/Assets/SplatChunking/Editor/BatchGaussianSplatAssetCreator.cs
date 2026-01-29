#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using GaussianSplatting.Runtime;

public static class BatchGaussianSplatAssetCreator
{
    // ================== PATHS ==================
    const string InputPlyFolder = "Assets/SplatChunks/PLY";

    const string OutputFolder   = "Assets/SplatChunks/Assets";
    // ===========================================

    [MenuItem("Tools/Gaussian Splats/Batch Create Assets (Create+Relink+Validate)")]
    public static void Run()
    {
        if (!Directory.Exists(InputPlyFolder))
        {
            Debug.LogError($"Input folder not found: {InputPlyFolder}");
            return;
        }
        Directory.CreateDirectory(OutputFolder);

        var plyAbsPaths = Directory.GetFiles(InputPlyFolder, "*.ply", SearchOption.TopDirectoryOnly)
                                   .Select(p => p.Replace("\\", "/"))
                                   .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                   .ToArray();

        if (plyAbsPaths.Length == 0)
        {
            Debug.LogError($"No .ply files found in: {InputPlyFolder}");
            return;
        }

        var creatorType = FindType("GaussianSplatting.Editor.GaussianSplatAssetCreator");
        if (creatorType == null)
        {
            Debug.LogError("Could not find GaussianSplatting.Editor.GaussianSplatAssetCreator. Is the package imported?");
            return;
        }

        var creator = ScriptableObject.CreateInstance(creatorType);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var fInput   = creatorType.GetField("m_InputFile", flags);
        var fOut     = creatorType.GetField("m_OutputFolder", flags);
        var fImportC = creatorType.GetField("m_ImportCameras", flags);
        var fQuality = creatorType.GetField("m_Quality", flags);

        var mApply   = creatorType.GetMethod("ApplyQualityLevel", flags);
        var mCreate  = creatorType.GetMethod("CreateAsset", flags);

        if (fInput == null || fOut == null || fImportC == null || fQuality == null || mCreate == null)
        {
            Debug.LogError("Reflection failed: expected fields/method not found on GaussianSplatAssetCreator (package version differs).");
            ScriptableObject.DestroyImmediate(creator);
            return;
        }

        int created = 0, relinked = 0, relinkFailed = 0, invalidBytes = 0;

        try
        {
            // 0) Creator konfigurieren
            fOut.SetValue(creator, OutputFolder);
            fImportC.SetValue(creator, false);

            // Qualität setzen (Enum im Package)
            object quality = Enum.Parse(fQuality.FieldType, "High");
            fQuality.SetValue(creator, quality);

            // Wichtig: setzt interne Formate/Bytes-Namen passend
            mApply?.Invoke(creator, null);

            // 1) Create (keine Refresh-Orgien pro Chunk)
            for (int i = 0; i < plyAbsPaths.Length; i++)
            {
                var abs = plyAbsPaths[i];
                var baseName = Path.GetFileNameWithoutExtension(abs);

                float p = (float)i / plyAbsPaths.Length;
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Batch Create GaussianSplatAssets (Create)",
                        $"{baseName} ({i + 1}/{plyAbsPaths.Length})",
                        p))
                {
                    Debug.LogWarning("Batch cancelled.");
                    break;
                }

                fInput.SetValue(creator, abs);
                mCreate.Invoke(creator, null);
                created++;
            }

            // 2) EINMAL synchron importieren
            EditorUtility.DisplayProgressBar("Batch Create GaussianSplatAssets", "Refreshing/Importing .bytes (sync)", 0.98f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 3) Relink + Validate
            for (int i = 0; i < plyAbsPaths.Length; i++)
            {
                var baseName = Path.GetFileNameWithoutExtension(plyAbsPaths[i]);

                if (RelinkOne(baseName, OutputFolder))
                    relinked++;
                else
                    relinkFailed++;

                if (!ValidateBytes(baseName, OutputFolder))
                    invalidBytes++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            ScriptableObject.DestroyImmediate(creator);
        }

        Debug.Log(
            $"Done. Created={created}, Relinked={relinked}, RelinkFailed={relinkFailed}, InvalidBytes={invalidBytes}. Output: {OutputFolder}\n" +
            $"If InvalidBytes > 0: delete OutputFolder and re-run (it indicates truncated/corrupt .bytes imports)."
        );
    }

    static bool RelinkOne(string chunkName, string outputFolderAssetsPath)
    {
        string assetPath = $"{outputFolderAssetsPath}/{chunkName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(assetPath);
        if (!asset) return false;

        string pChk = $"{outputFolderAssetsPath}/{chunkName}_chk.bytes";
        string pPos = $"{outputFolderAssetsPath}/{chunkName}_pos.bytes";
        string pOth = $"{outputFolderAssetsPath}/{chunkName}_oth.bytes";
        string pCol = $"{outputFolderAssetsPath}/{chunkName}_col.bytes";
        string pShs = $"{outputFolderAssetsPath}/{chunkName}_shs.bytes";

        bool ok = AssetExists(pPos) && AssetExists(pOth) && AssetExists(pCol) && AssetExists(pShs);
        if (!ok) return false;

        // Import hier ist ok – aber wir sind bereits nach globalem Refresh, also stabil
        AssetDatabase.ImportAsset(pPos, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(pOth, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(pCol, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(pShs, ImportAssetOptions.ForceSynchronousImport);
        if (AssetExists(pChk))
            AssetDatabase.ImportAsset(pChk, ImportAssetOptions.ForceSynchronousImport);

        var taChk = AssetExists(pChk) ? AssetDatabase.LoadAssetAtPath<TextAsset>(pChk) : null;
        var taPos = AssetDatabase.LoadAssetAtPath<TextAsset>(pPos);
        var taOth = AssetDatabase.LoadAssetAtPath<TextAsset>(pOth);
        var taCol = AssetDatabase.LoadAssetAtPath<TextAsset>(pCol);
        var taShs = AssetDatabase.LoadAssetAtPath<TextAsset>(pShs);

        if (!taPos || !taOth || !taCol || !taShs) return false;

        asset.SetAssetFiles(taChk, taPos, taOth, taCol, taShs);
        EditorUtility.SetDirty(asset);
        return true;
    }

    // Byte-Checks: findet „not multiple of 4“ sofort
    static bool ValidateBytes(string chunkName, string outputFolderAssetsPath)
    {
        bool ok = true;
        ok &= ValidateOne($"{outputFolderAssetsPath}/{chunkName}_pos.bytes", 4);
        ok &= ValidateOne($"{outputFolderAssetsPath}/{chunkName}_oth.bytes", 4);
        ok &= ValidateOne($"{outputFolderAssetsPath}/{chunkName}_col.bytes", 4);
        // SHS kann je nach Format 2/4, wir checken „gerade“
        ok &= ValidateOne($"{outputFolderAssetsPath}/{chunkName}_shs.bytes", 2);

        string chk = $"{outputFolderAssetsPath}/{chunkName}_chk.bytes";
        if (AssetExists(chk))
            ok &= ValidateOne(chk, 4);

        return ok;
    }

    static bool ValidateOne(string unityAssetPath, int multiple)
    {
        if (!AssetExists(unityAssetPath)) return true;

        string abs = Path.GetFullPath(unityAssetPath);
        if (!File.Exists(abs)) return true;

        long len = new FileInfo(abs).Length;
        if (len % multiple != 0)
        {
            Debug.LogError($"[ValidateBytes] BAD: {unityAssetPath} size={len} not multiple of {multiple} (likely truncated import).");
            return false;
        }
        return true;
    }

    static bool AssetExists(string unityAssetPath)
        => !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(unityAssetPath));

    static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }
}
#endif