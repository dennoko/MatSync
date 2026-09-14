using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace DennokoWorks.MatSync
{
    internal sealed class PropertyDefinition
    {
        internal string Name, Label, Block, Exclusion;
        internal ShaderPropertyType Type;
        internal TextureDimension Dimension;
        internal bool IsTexture => Type == ShaderPropertyType.Texture;
    }

    internal sealed class PropertyValue
    {
        internal PropertyDefinition Definition;
        internal float Number;
        internal int Integer;
        internal Vector4 Vector;
        internal Texture Texture;
        internal Vector2 Scale, Offset;

        internal static PropertyValue Read(Material material, PropertyDefinition definition)
        {
            var value = new PropertyValue { Definition = definition };
            switch (definition.Type)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range: value.Number = material.GetFloat(definition.Name); break;
                case ShaderPropertyType.Int: value.Integer = material.GetInteger(definition.Name); break;
                case ShaderPropertyType.Color: value.Vector = material.GetColor(definition.Name); break;
                case ShaderPropertyType.Vector: value.Vector = material.GetVector(definition.Name); break;
                case ShaderPropertyType.Texture:
                    value.Texture = material.GetTexture(definition.Name);
                    value.Scale = material.GetTextureScale(definition.Name);
                    value.Offset = material.GetTextureOffset(definition.Name);
                    break;
            }
            return value;
        }

        internal bool SameAs(PropertyValue other)
        {
            if (other == null || !MaterialSchema.Compatible(Definition, other.Definition)) return false;
            switch (Definition.Type)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range: return Number.Equals(other.Number);
                case ShaderPropertyType.Int: return Integer == other.Integer;
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector: return Vector.Equals(other.Vector);
                case ShaderPropertyType.Texture:
                    return Texture == other.Texture && Scale.Equals(other.Scale) && Offset.Equals(other.Offset);
                default: return false;
            }
        }

        internal void Write(Material material)
        {
            var name = Definition.Name;
            switch (Definition.Type)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range: material.SetFloat(name, Number); break;
                case ShaderPropertyType.Int: material.SetInteger(name, Integer); break;
                case ShaderPropertyType.Color: material.SetColor(name, (Color)Vector); break;
                case ShaderPropertyType.Vector: material.SetVector(name, Vector); break;
                case ShaderPropertyType.Texture:
                    material.SetTexture(name, Texture);
                    material.SetTextureScale(name, Scale);
                    material.SetTextureOffset(name, Offset);
                    break;
            }
        }

        private static string F(float value) => value.ToString("G9", CultureInfo.InvariantCulture);
        internal string Display()
        {
            switch (Definition.Type)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range: return F(Number);
                case ShaderPropertyType.Int: return Integer.ToString(CultureInfo.InvariantCulture);
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector: return $"({F(Vector.x)}, {F(Vector.y)}, {F(Vector.z)}, {F(Vector.w)})";
                case ShaderPropertyType.Texture:
                    return $"{(Texture ? Texture.name : "未設定")}  T({F(Scale.x)}, {F(Scale.y)}) O({F(Offset.x)}, {F(Offset.y)})";
                default: return "未対応の型";
            }
        }
    }

    internal sealed class MaterialSnapshot
    {
        internal Material Material;
        internal Shader Shader;
        internal string SerializedState;
        internal readonly Dictionary<string, PropertyValue> Values = new Dictionary<string, PropertyValue>();

        internal static MaterialSnapshot Capture(Material material, bool fullState = false)
        {
            var snapshot = new MaterialSnapshot { Material = material, Shader = material.shader };
            foreach (var definition in MaterialSchema.Get(material.shader).Values)
                snapshot.Values[definition.Name] = PropertyValue.Read(material, definition);
            if (fullState) snapshot.SerializedState = EditorJsonUtility.ToJson(material);
            return snapshot;
        }

        internal bool IsCurrent() => Material && Material.shader == Shader &&
            SerializedState == EditorJsonUtility.ToJson(Material);
    }

    internal static class MaterialSchema
    {
        private static readonly Dictionary<Shader, Dictionary<string, PropertyDefinition>> Cache =
            new Dictionary<Shader, Dictionary<string, PropertyDefinition>>();

        internal static void ClearCache() => Cache.Clear();

        internal static Dictionary<string, PropertyDefinition> Get(Shader shader)
        {
            if (Cache.TryGetValue(shader, out var definitions)) return definitions;
            definitions = new Dictionary<string, PropertyDefinition>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string name = shader.GetPropertyName(i);
                var definition = new PropertyDefinition
                {
                    Name = name, Label = LilToonBridge.Label(shader.GetPropertyDescription(i)), Type = shader.GetPropertyType(i),
                    Block = PropertyCatalog.BlockFor(name), Exclusion = PropertyCatalog.ExclusionFor(name)
                };
                if (definition.IsTexture) definition.Dimension = shader.GetPropertyTextureDimension(i);
                if ((shader.GetPropertyFlags(i) & ShaderPropertyFlags.HideInInspector) != 0 &&
                    string.IsNullOrEmpty(definition.Exclusion))
                    definition.Exclusion = "非表示の内部管理値";
                definitions[name] = definition;
            }
            Cache[shader] = definitions;
            return definitions;
        }

        internal static bool Compatible(PropertyDefinition a, PropertyDefinition b)
        {
            bool numeric = (a.Type == ShaderPropertyType.Float || a.Type == ShaderPropertyType.Range) &&
                           (b.Type == ShaderPropertyType.Float || b.Type == ShaderPropertyType.Range);
            return (a.Type == b.Type || numeric) && (!a.IsTexture || a.Dimension == b.Dimension);
        }
    }

    // Optional reflection bridge keeps the window compilable when lilToon is absent.
    internal static class LilToonBridge
    {
        private static readonly Type Utils = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("lilToon.lilMaterialUtils", false)).FirstOrDefault(t => t != null);
        private static readonly MethodInfo SetupMulti = Utils?.GetMethod("SetupMultiMaterial",
            BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Material) }, null);
        private static readonly MethodInfo RemoveKeywords = Utils?.GetMethod("RemoveShaderKeywords",
            BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Material) }, null);
        private static readonly MethodInfo Localize = Utils?.Assembly.GetType("lilToon.lilLanguageManager")?
            .GetMethod("GetLoc", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);

        internal static string Label(string description)
        {
            try { return (Localize?.Invoke(null, new object[] { description }) as string ?? description).Split('|')[0]; }
            catch { return description; }
        }

        internal static string Validate(Material material, bool writable)
        {
            if (!material) return "マテリアルが未指定、または参照切れです。";
            if (!material.shader || !PropertyCatalog.ShaderNames.Contains(material.shader.name))
                return "未対応シェーダーです（lilToon 2.3.4の標準バリアントが対象）。";
            if (Utils == null || SetupMulti == null || RemoveKeywords == null)
                return "lilToonのEditor APIが見つかりません。lilToon 2.3.4を確認してください。";
            string path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path) || !EditorUtility.IsPersistent(material))
                return "保存されたMaterialアセットを指定してください。";
            if (!writable) return null;
            if (!path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase) || AssetDatabase.IsSubAsset(material))
                return "埋め込みマテリアルには書き込めません。.matアセットを指定してください。";
            if (!AssetDatabase.IsOpenForEdit(material, StatusQueryOptions.UseCachedIfPossible))
                return "アセットが書き込み可能な状態ではありません。";
            return FileWriteState(path);
        }

        private const double FileStateLifetime = 1.0;
        private static readonly Dictionary<string, (double Time, string Reason)> FileStates =
            new Dictionary<string, (double, string)>();

        // Validate runs from UI refreshes and sync ticks; avoid touching the file system on every call.
        private static string FileWriteState(string path)
        {
            double now = EditorApplication.timeSinceStartup;
            if (FileStates.TryGetValue(path, out var cached) && now - cached.Time < FileStateLifetime)
                return cached.Reason;
            string reason = null;
            try
            {
                if (!File.Exists(path)) reason = "アセットファイルが見つかりません。";
                else if ((File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)
                    reason = "アセットが読み取り専用です。";
            }
            catch (Exception e) { reason = "書き込み可否を確認できません: " + e.Message; }
            FileStates[path] = (now, reason);
            return reason;
        }

        internal static void Reconcile(Material material, HashSet<string> changed, bool includeTextures)
        {
            // Follow lilInspector's post-edit path, without changing shader/rendering mode.
            string last = material.shader.name.Substring(material.shader.name.LastIndexOf('/') + 1);
            (last.Contains("Multi") ? SetupMulti : RemoveKeywords).Invoke(null, new object[] { material });
            if (changed.Contains("_Color") && material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", material.GetColor("_Color"));
            if (includeTextures && changed.Contains("_MainTex"))
            {
                foreach (string alias in new[] { "_BaseMap", "_BaseColorMap" })
                    if (material.HasProperty(alias)) material.SetTexture(alias, material.GetTexture("_MainTex"));
            }
        }
    }

    internal sealed class ExtractionResult
    {
        internal readonly List<Material> Materials = new List<Material>();
        internal readonly List<string> Notes = new List<string>();
    }

    internal static class MaterialSelection
    {
        internal static ExtractionResult Extract(IEnumerable<Object> objects)
        {
            var result = new ExtractionResult();
            var seen = new HashSet<Material>();
            foreach (var obj in objects)
            {
                if (obj is Material material)
                {
                    if (seen.Add(material)) result.Materials.Add(material);
                    continue;
                }
                var go = obj as GameObject;
                if (!go && obj is Component component) go = component.gameObject;
                if (!go) { result.Notes.Add($"{(obj ? obj.name : "空の参照")}: Materialまたはメッシュを指定してください。"); continue; }
                int slots = 0, empty = 0;
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                    foreach (var candidate in renderer.sharedMaterials)
                    {
                        slots++;
                        if (!candidate) { empty++; continue; }
                        if (seen.Add(candidate)) result.Materials.Add(candidate);
                    }
                }
                if (slots == 0) result.Notes.Add(go.name + ": 対象Renderer／マテリアルスロットがありません。");
                if (empty != 0) result.Notes.Add(go.name + $": 空のスロット{empty}件を除外しました。");
            }
            return result;
        }
    }
}
