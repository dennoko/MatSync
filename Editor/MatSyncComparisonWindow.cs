using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    public sealed class MatSyncComparisonWindow : EditorWindow
    {
        [SerializeField] private Material left, right;
        private MaterialComparison comparison;
        private Vector2 scroll, resultScroll;
        private string message;
        private TransferResult result;
        private bool stale, onlyDifferences, showDetails;
        private static readonly string[] Choices = { "変更しない", "左を採用", "右を採用" };

        internal static void Open(Material left, Material right)
        {
            bool alreadyOpen = HasOpenInstances<MatSyncComparisonWindow>();
            var window = GetWindow<MatSyncComparisonWindow>("MatSync 比較");
            window.left = left;
            window.right = right;
            window.result = null;
            window.RefreshComparison(false);
            window.minSize = new Vector2(780, 420);
            if (!alreadyOpen) window.position = new Rect(window.position.x, window.position.y, 1050, 700);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            minSize = new Vector2(780, 420);
            Undo.undoRedoPerformed += CheckForChanges;
            EditorApplication.projectChanged += CheckForChanges;
            if (left && right) RefreshComparison(false);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= CheckForChanges;
            EditorApplication.projectChanged -= CheckForChanges;
        }

        private void OnInspectorUpdate() => CheckForChanges();
        private void CheckForChanges()
        {
            if (comparison != null)
            {
                bool next = !comparison.IsCurrent;
                if (next != stale) { stale = next; Repaint(); }
            }
        }

        private void RefreshComparison(bool preserveOptions)
        {
            string invalid = LilToonBridge.Validate(left, false) ?? LilToonBridge.Validate(right, false);
            if (invalid != null) { message = invalid; comparison = null; return; }
            int destination = preserveOptions && comparison != null ? comparison.Destination : 0;
            bool textures = preserveOptions && comparison != null && comparison.IncludeTextures;
            MaterialSchema.ClearCache();
            comparison = new MaterialComparison(left, right) { Destination = destination, IncludeTextures = textures };
            stale = false;
            message = null;
            Repaint();
        }

        private void OnGUI()
        {
            if (comparison == null)
            {
                EditorGUILayout.HelpBox(message ?? "基本ウィンドウで左右のマテリアルを指定してください。", MessageType.Info);
                if (GUILayout.Button("基本ウィンドウを開く")) MatSyncWindow.Open();
                return;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("左: " + (left ? left.name : "参照切れ"), EditorStyles.boldLabel);
                EditorGUILayout.LabelField("右: " + (right ? right.name : "参照切れ"), EditorStyles.boldLabel);
                if (GUILayout.Button("再比較", GUILayout.Width(70))) RefreshComparison(true);
                if (GUILayout.Button("比較を終了", GUILayout.Width(90))) { Close(); GUIUtility.ExitGUI(); }
            }
            if (comparison == null) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                comparison.Destination = EditorGUILayout.Popup("結果の適用先", comparison.Destination,
                    new[] { "未選択", "左: " + (left ? left.name : "参照切れ"), "右: " + (right ? right.name : "参照切れ") });
                comparison.IncludeTextures = GUILayout.Toggle(comparison.IncludeTextures, "テクスチャを含める");
                onlyDifferences = GUILayout.Toggle(onlyDifferences, "差分のみ表示");
            }
            EditorGUILayout.LabelField(comparison.IncludeTextures
                ? "採用したブロックのテクスチャ参照・Tiling・Offsetも転送します。"
                : "適用先のテクスチャ参照・Tiling・Offsetを保持します。", EditorStyles.wordWrappedMiniLabel);
            if (stale) EditorGUILayout.HelpBox("比較中に外部変更がありました。再比較してください。採用選択はリセットされます。", MessageType.Warning);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var block in comparison.Blocks) DrawBlock(block);
            EditorGUILayout.EndScrollView();

            var plan = comparison.BuildPlan();
            if (plan != null)
            {
                string changedBlocks = string.Join("、", plan.Changes.Select(v => v.Definition.Block).Distinct());
                EditorGUILayout.LabelField($"適用先: {(plan.Target ? plan.Target.name : "参照切れ")} ／ 変更予定: {plan.Changes.Count}項目 ／ 対象外: {plan.SkippedProperties.Count}項目", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("変更するブロック: " + (changedBlocks.Length == 0 ? "なし" : changedBlocks), EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.LabelField("Materialアセットを変更します。同じアセットを使うオブジェクトにも反映されます。", EditorStyles.wordWrappedMiniLabel);
            string invalidTarget = plan == null ? "適用先を選択してください。" : LilToonBridge.Validate(plan.Target, true);
            if (invalidTarget != null) EditorGUILayout.HelpBox(invalidTarget, MessageType.Info);
            using (new EditorGUI.DisabledScope(stale || invalidTarget != null || plan == null ||
                plan.Changes.Count == 0 || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("選択した結果を適用", GUILayout.Height(28)))
                {
                    result = comparison.Apply();
                    if (result.Failed == 0) RefreshComparison(true);
                    else CheckForChanges();
                }
            }
            if (result != null)
            {
                EditorGUILayout.HelpBox(result.Summary, result.Failed > 0 ? MessageType.Warning : MessageType.Info);
                showDetails = EditorGUILayout.Foldout(showDetails, "適用結果・除外理由", true);
                if (showDetails)
                {
                    resultScroll = EditorGUILayout.BeginScrollView(resultScroll, GUILayout.Height(110));
                    EditorGUILayout.SelectableLabel(result.Text, EditorStyles.wordWrappedLabel,
                        GUILayout.MinHeight(EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(result.Text), position.width - 40)));
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void DrawBlock(ComparisonBlock block)
        {
            int differences = block.Properties.Count(comparison.Different);
            if (onlyDifferences && differences == 0) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    block.Expanded = EditorGUILayout.Foldout(block.Expanded,
                        $"{block.Name}  ─  差分 {differences}/{block.Properties.Count}", true);
                    block.Choice = EditorGUILayout.Popup(block.Choice, Choices, GUILayout.Width(120));
                }
                if (!block.Expanded) return;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("プロパティー / 状態", EditorStyles.miniBoldLabel, GUILayout.Width(260));
                    GUILayout.Label("左", EditorStyles.miniBoldLabel, GUILayout.MinWidth(200));
                    GUILayout.Label("右", EditorStyles.miniBoldLabel, GUILayout.MinWidth(200));
                }
                foreach (string name in block.Properties)
                {
                    if (onlyDifferences && !comparison.Different(name)) continue;
                    var definition = comparison.Definition(name);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(260)))
                        {
                            EditorGUILayout.LabelField(new GUIContent((comparison.Different(name) ? "≠ " : "= ") + name,
                                definition.Label), EditorStyles.miniBoldLabel);
                            EditorGUILayout.LabelField(definition.Label + " · " + comparison.PropertyStatus(name), EditorStyles.wordWrappedMiniLabel);
                        }
                        DrawValue(comparison.Left, name);
                        DrawValue(comparison.Right, name);
                    }
                }
            }
        }

        private static void DrawValue(MaterialSnapshot snapshot, string name)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(200)))
            {
                if (!snapshot.Values.TryGetValue(name, out var value))
                { EditorGUILayout.LabelField("— 存在しません"); return; }
                if (value.Definition.IsTexture)
                {
                    using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField(value.Texture, typeof(Texture), false);
                }
                else if (value.Definition.Type == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    Rect rect = EditorGUILayout.GetControlRect(false, 8);
                    EditorGUI.DrawRect(rect, (Color)value.Vector);
                }
                EditorGUILayout.LabelField(value.Display(), EditorStyles.wordWrappedMiniLabel);
            }
        }
    }
}
