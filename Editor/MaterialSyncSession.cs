using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    // Session lifetime is deliberately not serialized or restored after domain reload.
    internal sealed class MaterialSyncSession : IDisposable
    {
        internal static MaterialSyncSession Active { get; private set; }
        internal bool Running { get; private set; }
        internal string Status { get; private set; } = "停止中";
        internal TransferResult LastResult { get; private set; }
        internal int TargetCount => targets.Count;
        internal event Action Changed;
        private Material source;
        private List<Material> targets = new List<Material>();
        private MaterialSnapshot baseline;
        private bool includeTextures, busy;
        private int recordedGroup = -1;
        private const double ValidationInterval = 0.5;
        private double nextValidation;
        private readonly HashSet<Material> recordedTargets = new HashSet<Material>();

        internal void Start(Material from, IEnumerable<Material> to, bool textures)
        {
            Stop("停止中");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Status = "Play Modeでは同期できません。"; return; }
            string invalid = LilToonBridge.Validate(from, false);
            if (invalid != null) { Status = invalid; return; }
            var valid = to.Where(m => m != from && LilToonBridge.Validate(m, true) == null).Distinct().ToList();
            if (valid.Count == 0) { Status = "有効な同期先がありません。"; return; }
            Active?.Stop("別の同期セッションが開始されたため停止しました。");
            source = from;
            targets = valid;
            includeTextures = textures;
            baseline = MaterialSnapshot.Capture(source);
            LastResult = null;
            recordedGroup = -1;
            recordedTargets.Clear();
            Running = true;
            Active = this;
            Status = $"同期中: {targets.Count}件（開始後に変更した項目だけ反映）";
            EditorApplication.update += Tick;
            // Flush occurs in the Inspector's Undo group; propagate before that group ends.
            Undo.willFlushUndoRecord += Tick;
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.playModeStateChanged += OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.quitting += BeforeQuit;
            Changed?.Invoke();
        }

        internal void Stop(string reason = "停止中")
        {
            Running = false;
            if (Active == this) Active = null;
            EditorApplication.update -= Tick;
            Undo.willFlushUndoRecord -= Tick;
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            EditorApplication.quitting -= BeforeQuit;
            Status = reason;
            Changed?.Invoke();
        }

        private void OnPlayMode(PlayModeStateChange state) => Stop("Play Modeの遷移により停止しました。");
        private void BeforeReload() => Stop("スクリプト再読み込みにより停止しました。");
        private void BeforeQuit() => Stop();

        private void OnUndoRedo()
        {
            if (!Running) return;
            if (!source) { Stop("同期元が失われました。"); return; }
            baseline = MaterialSnapshot.Capture(source);
            recordedGroup = -1;
            recordedTargets.Clear();
            Changed?.Invoke();
        }

        private bool IsMultiEdit()
        {
            // Includes locked Inspectors, whose targets need not match Selection.objects.
            foreach (var tracker in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (tracker.GetType().Name != "InspectorWindow") continue;
                var property = tracker.GetType().GetProperty("tracker",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                var activeTracker = property?.GetValue(tracker) as ActiveEditorTracker;
                if (activeTracker != null && activeTracker.activeEditors.Any(e => e && e.targets.Length > 1 && e.targets.Contains(source)))
                    return true;
            }
            return Selection.objects.Contains(source) && Selection.objects.OfType<Material>().Count() > 1;
        }

        private void RecordTarget(Material target)
        {
            int group = Undo.GetCurrentGroup();
            if (recordedGroup != group) { recordedTargets.Clear(); recordedGroup = group; }
            if (recordedTargets.Add(target)) Undo.RegisterCompleteObjectUndo(target, "MatSync 同時編集");
        }

        private void Tick()
        {
            if (!Running || busy || Undo.isProcessing) return;
            busy = true;
            try
            {
                if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                { Stop("コンパイル／Play Modeへの遷移により停止しました。"); return; }
                if (!source || !source.shader) { Stop("同期元が失われました。"); return; }
                if (source.shader != baseline.Shader)
                { Stop("同期元のシェーダー変更を検出しました。対象を確認して再開してください。"); return; }
                var current = MaterialSnapshot.Capture(source);
                var names = new HashSet<string>(current.Values.Where(pair =>
                    !baseline.Values.TryGetValue(pair.Key, out var previous) || !pair.Value.SameAs(previous))
                    .Select(pair => pair.Key));

                // Asset validation and the Inspector scan are costly; run them on edits or at an interval only.
                double now = EditorApplication.timeSinceStartup;
                if (names.Count == 0 && now < nextValidation) return;
                nextValidation = now + ValidationInterval;

                string invalid = LilToonBridge.Validate(source, false);
                if (invalid != null) { Stop("同期元: " + invalid); return; }
                if (IsMultiEdit()) { Stop("同期元を含む複数マテリアル編集を検出したため停止しました。単一選択で再開してください。"); return; }
                var removed = new List<string>();
                targets.RemoveAll(target =>
                {
                    string reason = LilToonBridge.Validate(target, true);
                    if (reason == null) return false;
                    removed.Add((target ? target.name : "参照切れ") + ": " + reason);
                    return true;
                });
                if (removed.Count > 0)
                {
                    LastResult = new TransferResult { Skipped = removed.Count };
                    LastResult.Details.AddRange(removed);
                    Status = $"同期中: {targets.Count}件（無効な同期先を除外）";
                    Changed?.Invoke();
                }
                if (targets.Count == 0) { Stop("有効な同期先がなくなったため停止しました。"); return; }
                baseline = current;
                if (names.Count == 0) return;
                LastResult = MaterialTransfer.Copy(source, targets, includeTextures, names, true, RecordTarget);
                if (LastResult.Failed > 0) Stop("同期処理でエラーが発生したため停止しました。結果を確認してください。");
                Changed?.Invoke();
            }
            catch (Exception exception)
            {
                Stop("同期を停止しました: " + exception.GetBaseException().Message);
            }
            finally { busy = false; }
        }

        public void Dispose() => Stop();
    }
}
