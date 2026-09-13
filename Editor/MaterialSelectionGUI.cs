using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    internal static class MaterialSelectionGUI
    {
        internal static void Single(string label, Material current, Action<Material> set,
            Action<string> message, bool writable = false)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            // Reserve the rect first so multi-object drops are intercepted before ObjectField takes the first object.
            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            bool handled = HandleDrop(rect, result => Choose(result, set, message));
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!handled)
                {
                    EditorGUI.BeginChangeCheck();
                    var next = (Material)EditorGUI.ObjectField(rect, current, typeof(Material), false);
                    if (EditorGUI.EndChangeCheck()) set(next);
                }
                if (GUILayout.Button("選択中から指定", EditorStyles.miniButton))
                    Choose(MaterialSelection.Extract(Selection.objects), set, message);
                using (new EditorGUI.DisabledScope(!current))
                    if (GUILayout.Button("Projectで表示", EditorStyles.miniButton)) EditorGUIUtility.PingObject(current);
            }
            DropArea("Material／Hierarchyのメッシュをドロップ", result => Choose(result, set, message));
            if (current)
            {
                string invalid = LilToonBridge.Validate(current, writable);
                if (invalid != null) EditorGUILayout.HelpBox(invalid, MessageType.Warning);
            }
        }

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

        internal static void Multiple(List<Material> materials, Material source, ref Vector2 scroll,
            Action beforeChange, Action<string> message)
        {
            EditorGUILayout.LabelField($"対象リスト: {materials.Count}件", EditorStyles.boldLabel);
            DropArea("複数Material／複数メッシュをここへドロップ", result =>
                Add(result, materials, source, beforeChange, message));
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("選択中を追加"))
                    Add(MaterialSelection.Extract(Selection.objects), materials, source, beforeChange, message);
                using (new EditorGUI.DisabledScope(materials.Count == 0))
                    if (GUILayout.Button("全件クリア", GUILayout.Width(90))) { beforeChange(); materials.Clear(); }
            }
            var add = (Material)EditorGUILayout.ObjectField("1件追加", null, typeof(Material), false);
            if (add) Add(MaterialSelection.Extract(new[] { add }), materials, source, beforeChange, message);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(65), GUILayout.MaxHeight(240));
            int remove = -1;
            for (int i = 0; i < materials.Count; i++)
            {
                var material = materials[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField(material, typeof(Material), false);
                    using (new EditorGUI.DisabledScope(!material))
                        if (GUILayout.Button("表示", GUILayout.Width(38))) EditorGUIUtility.PingObject(material);
                    if (GUILayout.Button("×", GUILayout.Width(24))) remove = i;
                }
                string reason = material && material == source ? "元と同一のため対象外" : LilToonBridge.Validate(material, true);
                if (reason != null) EditorGUILayout.LabelField(reason, EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndScrollView();
            if (remove >= 0) { beforeChange(); materials.RemoveAt(remove); }
        }

        internal static void Add(ExtractionResult result, List<Material> materials, Material source,
            Action beforeChange, Action<string> message)
        {
            var added = result.Materials.Where(m => m != source && !materials.Contains(m)).ToList();
            if (added.Count > 0) { beforeChange(); materials.AddRange(added); }
            message($"{added.Count}件を追加しました（重複・元と同じ参照は除外）。" +
                (result.Notes.Count > 0 ? "\n" + string.Join("\n", result.Notes) : ""));
        }

        private static void DropArea(string label, Action<ExtractionResult> accept)
        {
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.helpBox,
                GUILayout.Height(34), GUILayout.ExpandWidth(true));
            GUI.Box(rect, label, EditorStyles.helpBox);
            HandleDrop(rect, accept);
        }

        private static bool HandleDrop(Rect rect, Action<ExtractionResult> accept)
        {
            var evt = Event.current;
            if (!GUI.enabled || !rect.Contains(evt.mousePosition) ||
                (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)) return false;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                accept(MaterialSelection.Extract(DragAndDrop.objectReferences));
            }
            evt.Use();
            return true;
        }
    }
}
