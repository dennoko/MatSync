using System;
using System.Collections.Generic;
using System.Linq;

namespace DennokoWorks.MatSync
{
    internal sealed class ComparisonBlock
    {
        internal string Name;
        internal List<string> Properties;
        internal int Choice; // 0: keep destination, 1: left, 2: right
        internal bool Expanded;
    }

    internal sealed class MaterialComparison
    {
        internal MaterialSnapshot Left, Right;
        internal readonly List<ComparisonBlock> Blocks = new List<ComparisonBlock>();
        internal int Destination;
        internal bool IncludeTextures;
        internal bool IsCurrent => Left != null && Right != null && Left.IsCurrent() && Right.IsCurrent();

        internal MaterialComparison(UnityEngine.Material left, UnityEngine.Material right)
        {
            Left = MaterialSnapshot.Capture(left, true);
            Right = MaterialSnapshot.Capture(right, true);
            var names = Left.Values.Keys.Union(Right.Values.Keys).OrderBy(n => n, StringComparer.Ordinal);
            foreach (var group in names.GroupBy(n => Definition(n).Block))
                Blocks.Add(new ComparisonBlock { Name = group.Key, Properties = group.ToList() });
            string[] order = { "基本色", "基本色 2nd", "基本色 3rd", "ライティング", "影", "発光", "ノーマル",
                "異方性反射", "反射", "MatCap", "リムライト", "輪郭線", "透過・描画設定", "内部・派生値" };
            Blocks.Sort((a, b) =>
            {
                int ai = Array.IndexOf(order, a.Name), bi = Array.IndexOf(order, b.Name);
                if (ai < 0) ai = order.Length;
                if (bi < 0) bi = order.Length;
                return ai != bi ? ai.CompareTo(bi) : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        internal PropertyDefinition Definition(string name) =>
            Left.Values.TryGetValue(name, out var value) ? value.Definition : Right.Values[name].Definition;

        internal bool Different(string name) => !Left.Values.TryGetValue(name, out var left) ||
            !Right.Values.TryGetValue(name, out var right) || !left.SameAs(right);

        internal string PropertyStatus(string name)
        {
            if (!Left.Values.TryGetValue(name, out var left) || !Right.Values.TryGetValue(name, out var right))
                return "片側に不在・適用不可";
            if (!MaterialSchema.Compatible(left.Definition, right.Definition)) return "型／次元が非互換";
            if (!string.IsNullOrEmpty(left.Definition.Exclusion)) return left.Definition.Exclusion;
            if (!string.IsNullOrEmpty(right.Definition.Exclusion)) return right.Definition.Exclusion;
            if (left.Definition.IsTexture && !IncludeTextures) return "テクスチャ保持・適用対象外";
            if (Left.Shader != Right.Shader && left.Definition.Block == "透過・描画設定") return "描画設定は同一シェーダー間のみ対応";
            return Different(name) ? "差分あり" : "同じ値";
        }

        internal TransferPlan BuildPlan()
        {
            if (Destination == 0) return null;
            var target = Destination == 1 ? Left : Right;
            var combined = new TransferPlan { Target = target.Material };
            foreach (var block in Blocks)
            {
                if (block.Choice == 0 || block.Choice == Destination) continue;
                var source = block.Choice == 1 ? Left : Right;
                var partial = MaterialTransfer.Plan(source, target, block.Properties, IncludeTextures);
                combined.Changes.AddRange(partial.Changes);
                combined.SkippedProperties.AddRange(partial.SkippedProperties);
            }
            return combined;
        }

        internal TransferResult Apply()
        {
            var result = new TransferResult();
            if (!IsCurrent)
            {
                result.Failed++;
                result.Details.Add("比較中にマテリアルが変更されました。再比較してください（採用選択はリセットされます）。");
                return result;
            }
            foreach (var material in new[] { Left.Material, Right.Material })
            {
                string invalid = LilToonBridge.Validate(material, false);
                if (invalid != null) { result.Failed++; result.Details.Add(invalid); return result; }
            }
            var plan = BuildPlan();
            if (plan == null) { result.Failed++; result.Details.Add("適用先を選択してください。"); return result; }
            MaterialSyncSession.Active?.Stop("比較結果の適用のため同期を停止しました。");
            MaterialTransfer.Apply(new[] { plan }, IncludeTextures, "MatSync 比較結果を適用", result);
            return result;
        }
    }
}
