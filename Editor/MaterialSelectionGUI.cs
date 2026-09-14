using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    internal static class MaterialSelectionGUI
    {
        internal static void Choose(ExtractionResult result, Action<Material> set, Action<string> message)
        {
            if (result.Notes.Count > 0) message(string.Join("\n", result.Notes));
            if (result.Materials.Count == 0)
            {
                if (result.Notes.Count == 0) message("追加できるマテリアルがありません。");
                return;
            }
            if (result.Materials.Count == 1) { set(result.Materials[0]); return; }
            var menu = new GenericMenu();
            menu.AddDisabledItem(new GUIContent("1件を選択（閉じると指定を維持）"));
            for (int i = 0; i < result.Materials.Count; i++)
            {
                var material = result.Materials[i];
                string path = AssetDatabase.GetAssetPath(material).Replace('/', '／');
                menu.AddItem(new GUIContent($"{i + 1}. {material.name}  [{path}]"), false, () => set(material));
            }
            menu.ShowAsContext();
        }

        internal static void SetupDropArea(UnityEngine.UIElements.VisualElement area, Action<ExtractionResult> onDrop)
        {
            if (area == null) return;
            area.RegisterCallback<UnityEngine.UIElements.DragUpdatedEvent>(_ =>
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    area.AddToClassList("matsync-drop-area--active");
                }
            });
            area.RegisterCallback<UnityEngine.UIElements.DragLeaveEvent>(_ =>
            {
                area.RemoveFromClassList("matsync-drop-area--active");
            });
            area.RegisterCallback<UnityEngine.UIElements.DragPerformEvent>(_ =>
            {
                area.RemoveFromClassList("matsync-drop-area--active");
                DragAndDrop.AcceptDrag();
                var result = MaterialSelection.Extract(DragAndDrop.objectReferences);
                onDrop?.Invoke(result);
            });
        }

        internal static void Add(ExtractionResult result, List<Material> materials, Material source,
            Action beforeChange, Action<string> message)
        {
            var added = result.Materials.Where(m => m != source && !materials.Contains(m)).ToList();
            if (added.Count > 0) { beforeChange(); materials.AddRange(added); }
            message($"{added.Count}件を追加しました（重複・元と同じ参照は除外）。" +
                (result.Notes.Count > 0 ? "\n" + string.Join("\n", result.Notes) : ""));
        }
    }
}
