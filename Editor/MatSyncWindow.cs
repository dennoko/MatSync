using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    public sealed class MatSyncWindow : EditorWindow
    {
        [SerializeField] private int mode;
        [SerializeField] private Material source, left, right;
        [SerializeField] private List<Material> targets = new List<Material>();
        [SerializeField] private bool copyTextures, syncTextures;
        private Vector2 pageScroll, targetScroll, resultScroll;
        private string message;
        private TransferResult result;
        private bool showDetails;
        private MaterialSyncSession sync;

        [MenuItem("Tools/dennokoworks/MatSync")]
        public static void Open()
        {
            bool alreadyOpen = HasOpenInstances<MatSyncWindow>();
            var window = GetWindow<MatSyncWindow>("MatSync");
            window.minSize = new Vector2(340, 360);
            if (!alreadyOpen) window.position = new Rect(window.position.x, window.position.y, 380, 600);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(340, 360);
            sync = new MaterialSyncSession();
            sync.Changed += Repaint;
            Undo.undoRedoPerformed += Repaint;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable()
        {
            sync?.Dispose();
            if (sync != null) sync.Changed -= Repaint;
            Undo.undoRedoPerformed -= Repaint;
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged() { MaterialSchema.ClearCache(); Repaint(); }
        private void Message(string text) { message = text; Repaint(); }
        private void StopForSettings() { sync?.Stop("設定が変更されたため停止しました。再開してください。"); result = null; }
        private void SetSource(Material material)
        {
            if (!this) return;
            StopForSettings(); source = material; targets.RemoveAll(m => m && m == source); Repaint();
        }

        private void OnGUI()
        {
            int nextMode = GUILayout.Toolbar(mode, new[] { "コピー", "比較", "同時編集" });
            if (nextMode != mode) { sync.Stop("モード変更により停止しました。"); mode = nextMode; message = null; result = null; }
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            EditorGUILayout.Space(5);
            if (mode == 1) DrawComparison();
            else DrawTransfer();
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
            DrawResult(mode == 2 ? sync.LastResult : result);
            EditorGUILayout.EndScrollView();
        }

        private void DrawComparison()
        {
            MaterialSelectionGUI.Single("左マテリアル", left, m => { if (this) { left = m; Repaint(); } }, Message);
            EditorGUILayout.Space(5);
            MaterialSelectionGUI.Single("右マテリアル", right, m => { if (this) { right = m; Repaint(); } }, Message);
            string invalid = LilToonBridge.Validate(left, false) ?? LilToonBridge.Validate(right, false);
            if (invalid == null && left == right) invalid = "左右に異なるマテリアルを指定してください。";
            using (new EditorGUI.DisabledScope(invalid != null || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("比較ウィンドウを開く", GUILayout.Height(28))) MatSyncComparisonWindow.Open(left, right);
            if (invalid != null) EditorGUILayout.HelpBox(invalid, MessageType.Info);
        }

        private void DrawTransfer()
        {
            bool syncing = mode == 2;
            MaterialSelectionGUI.Single(syncing ? "同期元" : "コピー元", source, SetSource, Message);
            if (syncing)
            {
                using (new EditorGUI.DisabledScope(!source))
                    if (GUILayout.Button("同期元をInspectorで開く")) { Selection.activeObject = source; EditorGUIUtility.PingObject(source); }
            }
            EditorGUILayout.Space(6);
            MaterialSelectionGUI.Multiple(targets, source, ref targetScroll, StopForSettings, Message);
            bool previous = syncing ? syncTextures : copyTextures;
            bool textures = EditorGUILayout.ToggleLeft("テクスチャを含める（Tiling／Offsetも転送）", previous);
            if (textures != previous)
            {
                StopForSettings();
                if (syncing) syncTextures = textures; else copyTextures = textures;
            }
            int validCount = targets.Count(m => m && m != source && LilToonBridge.Validate(m, true) == null);
            EditorGUILayout.LabelField($"実行可能な対象: {validCount}件");
            EditorGUILayout.HelpBox("Materialアセットを変更します。同じアセットを使う他のオブジェクトにも反映されます。", MessageType.None);
            string invalid = LilToonBridge.Validate(source, false);
            if (invalid == null && validCount == 0) invalid = "有効な対象を1件以上追加してください。";
            if (EditorApplication.isPlayingOrWillChangePlaymode) invalid = "Edit Modeで操作してください。";
            if (syncing)
            {
                EditorGUILayout.HelpBox(sync.Status, sync.Running ? MessageType.Info : MessageType.None);
                if (sync.Running)
                {
                    if (GUILayout.Button("同期を停止", GUILayout.Height(28))) sync.Stop("手動で停止しました。");
                }
                else
                {
                    using (new EditorGUI.DisabledScope(invalid != null))
                        if (GUILayout.Button("同期を開始", GUILayout.Height(28))) sync.Start(source, targets, syncTextures);
                    EditorGUILayout.LabelField("開始だけでは上書きしません。全設定をそろえる場合はコピーを使用します。", EditorStyles.wordWrappedMiniLabel);
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(invalid != null))
                    if (GUILayout.Button("コピー", GUILayout.Height(28)))
                    {
                        MaterialSyncSession.Active?.Stop("コピーの実行により同期を停止しました。");
                        result = MaterialTransfer.Copy(source, targets, copyTextures);
                    }
            }
            if (invalid != null) EditorGUILayout.HelpBox(invalid, MessageType.Info);
        }

        private void DrawResult(TransferResult current)
        {
            if (current == null) return;
            EditorGUILayout.HelpBox(current.Summary, current.Failed > 0 ? MessageType.Warning : MessageType.Info);
            showDetails = EditorGUILayout.Foldout(showDetails, "結果の詳細・除外理由", true);
            if (!showDetails) return;
            resultScroll = EditorGUILayout.BeginScrollView(resultScroll, GUILayout.Height(160));
            EditorGUILayout.SelectableLabel(current.Text, EditorStyles.wordWrappedLabel,
                GUILayout.MinHeight(EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(current.Text), position.width - 60)));
            EditorGUILayout.EndScrollView();
        }
    }
}
