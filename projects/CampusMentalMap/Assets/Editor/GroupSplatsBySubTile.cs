using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class GroupSplatsBySubTile
{
    // match: chunk_3_3_sub_2_3   (und alles was damit anfängt)
    static readonly Regex rx = new Regex(@"^(chunk_(\d+)_(\d+)_sub_(\d+)_(\d+))", RegexOptions.Compiled);

    [MenuItem("Tools/Gaussian Splats/Group by SubTile (chunk_x_y_sub_u_v)")]
    public static void Run()
    {
        var root = Selection.activeTransform;
        if (!root)
        {
            Debug.LogError("Select your RoomChunks root object in Hierarchy first.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        int moved = 0;
        int createdParents = 0;

        // Wir iterieren über eine Kopie, weil wir Children umhängen
        var children = new Transform[root.childCount];
        for (int i = 0; i < root.childCount; i++) children[i] = root.GetChild(i);

        foreach (var t in children)
        {
            var m = rx.Match(t.name);
            if (!m.Success) continue;

            string tileName = m.Groups[1].Value; // chunk_3_3_sub_2_3

            // Parent suchen/erstellen
            Transform parent = root.Find(tileName);
            if (!parent)
            {
                var go = new GameObject(tileName);
                Undo.RegisterCreatedObjectUndo(go, "Create tile parent");
                go.transform.SetParent(root, false);
                parent = go.transform;
                createdParents++;
            }

            // wenn t schon der Parent ist: skip
            if (t == parent) continue;

            // alles, was mit tileName anfängt, unter den Parent hängen
            if (t.name.StartsWith(tileName))
            {
                Undo.SetTransformParent(t, parent, "Move under tile parent");
                moved++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log($"[GroupSplatsBySubTile] Created parents: {createdParents}, moved objects: {moved}");
    }
}