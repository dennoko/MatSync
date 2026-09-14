using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.MatSync
{
    internal sealed class TransferPlan
    {
        internal Material Target;
        internal readonly List<PropertyValue> Changes = new List<PropertyValue>();
        internal readonly List<string> SkippedProperties = new List<string>();
    }

    internal sealed class TransferResult
    {
        internal int Changed, Unchanged, Skipped, Failed;
        internal readonly List<string> Details = new List<string>();
        internal string Summary => $"変更 {Changed}件 ／ 変更なし {Unchanged}件 ／ 除外 {Skipped}件 ／ 失敗 {Failed}件";
        internal string Text => Summary + (Details.Count == 0 ? "" : "\n" + string.Join("\n", Details));
    }

    internal static class MaterialTransfer
    {
        internal static string Exclusion(PropertyValue value, MaterialSnapshot source,
            MaterialSnapshot target, bool includeTextures)
        {
            var definition = value.Definition;
            if (!string.IsNullOrEmpty(definition.Exclusion)) return definition.Exclusion;
            if (definition.IsTexture && !includeTextures) return "テクスチャ保持（参照・Tiling・Offset）";
            if (!target.Values.TryGetValue(definition.Name, out var destination)) return "適用先に存在しません";
            if (!string.IsNullOrEmpty(destination.Definition.Exclusion)) return destination.Definition.Exclusion;
            if (!MaterialSchema.Compatible(definition, destination.Definition)) return "型／テクスチャ次元が非互換です";
            if (definition.Block == RenderBlock) return RenderStateMismatch(source, target);
            return null;
        }

        internal const string RenderBlock = "透過・描画設定";

        // Render settings only make sense for the same shader and rendering mode. lilToonMulti keeps the
        // mode in _TransparentMode on one shader, and SetupMultiMaterial does not rebuild blend/ZWrite state.
        internal static string RenderStateMismatch(MaterialSnapshot a, MaterialSnapshot b)
        {
            if (a.Shader != b.Shader) return "描画設定は同じシェーダー間のみ対応します";
            bool hasA = a.Values.TryGetValue("_TransparentMode", out var modeA);
            bool hasB = b.Values.TryGetValue("_TransparentMode", out var modeB);
            if (hasA != hasB || (hasA && !modeA.SameAs(modeB))) return "描画設定は同じ描画モード間のみ対応します";
            return null;
        }

        internal static TransferPlan Plan(MaterialSnapshot source, MaterialSnapshot target,
            IEnumerable<string> names, bool includeTextures)
        {
            var plan = new TransferPlan { Target = target.Material };
            foreach (string name in names.Distinct())
            {
                if (!source.Values.TryGetValue(name, out var value)) continue;
                string reason = Exclusion(value, source, target, includeTextures);
                if (reason != null)
                {
                    plan.SkippedProperties.Add(name + ": " + reason);
                    continue;
                }
                if (!value.SameAs(target.Values[name])) plan.Changes.Add(value);
            }
            return plan;
        }

        internal static TransferResult Copy(Material source, IEnumerable<Material> targets,
            bool includeTextures, HashSet<string> changedNames = null, bool joinUndoGroup = false,
            Action<Material> recordUndo = null)
        {
            var result = new TransferResult();
            try
            {
                string invalid = LilToonBridge.Validate(source, false);
                if (invalid != null) { result.Failed++; result.Details.Add(invalid); return result; }
                var snapshot = MaterialSnapshot.Capture(source);
                var plans = new List<TransferPlan>();
                foreach (var target in targets.Distinct())
                {
                    if (target == source) { result.Skipped++; result.Details.Add(source.name + ": 元と同一のため除外。"); continue; }
                    invalid = LilToonBridge.Validate(target, true);
                    if (invalid != null)
                    {
                        result.Skipped++;
                        result.Details.Add((target ? target.name : "参照切れ") + ": " + invalid);
                        continue;
                    }
                    plans.Add(Plan(snapshot, MaterialSnapshot.Capture(target),
                        changedNames ?? (IEnumerable<string>)snapshot.Values.Keys, includeTextures));
                }
                Apply(plans, includeTextures, joinUndoGroup ? "MatSync 同時編集" : "MatSync コピー",
                    result, joinUndoGroup, recordUndo);
            }
            catch (Exception exception)
            {
                result.Failed++;
                result.Details.Add("変更計画の作成に失敗しました: " + exception.GetBaseException().Message);
            }
            return result;
        }

        internal static void Apply(IEnumerable<TransferPlan> plans, bool includeTextures, string label,
            TransferResult result, bool joinUndoGroup = false, Action<Material> recordUndo = null)
        {
            int group = -1;
            try
            {
                foreach (var plan in plans)
                {
                    string invalid = LilToonBridge.Validate(plan.Target, true);
                    if (invalid != null)
                    {
                        result.Skipped++;
                        result.Details.Add((plan.Target ? plan.Target.name : "参照切れ") + ": " + invalid);
                        continue;
                    }
                    if (plan.SkippedProperties.Count > 0)
                        result.Details.Add(plan.Target.name + $": 対象外プロパティー {plan.SkippedProperties.Count}件\n  " +
                            string.Join("\n  ", plan.SkippedProperties));
                    if (plan.Changes.Count == 0) { result.Unchanged++; continue; }
                    if (group < 0)
                    {
                        if (!joinUndoGroup) Undo.IncrementCurrentGroup();
                        group = Undo.GetCurrentGroup();
                        if (!joinUndoGroup) Undo.SetCurrentGroupName(label);
                    }
                    if (recordUndo != null) recordUndo(plan.Target);
                    else Undo.RegisterCompleteObjectUndo(plan.Target, label);
                    // Count before writing so an exception in reconciliation is reported as partial work.
                    result.Changed++;
                    result.Details.Add(plan.Target.name + $": {plan.Changes.Count}項目を書き込み（Undo対象）。");
                    try
                    {
                        foreach (var value in plan.Changes) value.Write(plan.Target);
                        LilToonBridge.Reconcile(plan.Target,
                            new HashSet<string>(plan.Changes.Select(v => v.Definition.Name)), includeTextures);
                    }
                    finally { if (plan.Target) EditorUtility.SetDirty(plan.Target); }
                }
            }
            catch (Exception exception)
            {
                result.Failed++;
                result.Details.Add("途中で停止しました。上記の変更済み範囲はUndoで戻せます。\n" +
                    exception.GetBaseException().Message);
            }
            finally
            {
                if (group >= 0 && !joinUndoGroup)
                {
                    Undo.CollapseUndoOperations(group);
                    Undo.IncrementCurrentGroup();
                }
            }
        }
    }
}
