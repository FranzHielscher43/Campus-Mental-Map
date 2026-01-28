#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using GaussianSplatting.Runtime;

public static class AddChunkTriggerCullers
{
    [MenuItem("Tools/Gaussian Splats/Add ChunkCuller to children of selected")]
    public static void Add()
    {
        var root = Selection.activeGameObject;
        if (!root)
        {
            Debug.LogError("Select the root (e.g., GS_Chunks) in the scene.");
            return;
        }

        int added = 0;
        foreach (Transform child in root.transform)
        {
            var col = child.GetComponent<Collider>();
            if (!col)
            {
                Debug.LogWarning($"No Collider on {child.name} (expected BoxCollider). Skipping.");
                continue;
            }

            var c = child.GetComponent<ChunkCuller>();
            if (!c)
            {
                c = child.gameObject.AddComponent<ChunkCuller>();
                added++;
            }

            col.isTrigger = true;

            // ✅ richtig typisiert
            if (!c.splatRenderer)
                c.splatRenderer = child.GetComponentInChildren<GaussianSplatRenderer>(true);
        }

        Debug.Log($"Added/ensured ChunkCuller on {added} chunks under {root.name}");
    }
}
#endif