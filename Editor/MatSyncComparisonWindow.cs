using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DennokoWorks.MatSync
{
    public sealed class MatSyncComparisonWindow : EditorWindow
    {
        private const string DENNOKO_THEME_USS_PATH = "Assets/dennokoworks/MatSync/Editor/UI/DennokoTheme.uss";
        private const string MATSYNC_THEME_USS_PATH = "Assets/dennokoworks/MatSync/Editor/UI/MatSyncTheme.uss";
        private const string COMPARISON_WINDOW_UXML_PATH = "Assets/dennokoworks/MatSync/Editor/UI/MatSyncComparisonWindow.uxml";

        private const string DENNOKO_THEME_USS_GUID = "617a933f98fa1bd4ebb21e00387f64aa";
        private const string MATSYNC_THEME_USS_GUID = "48189550503d8d749b08c3774eb9a336";
        private const string COMPARISON_WINDOW_UXML_GUID = "8d93c6e164279374c9f551347deb0f6e";

        [SerializeField] private Material left, right;
        private MaterialComparison comparison;
        private TransferResult result;
        private bool stale;
        private bool onlyDifferences = true;

        // UI 参照
        private VisualElement rootContainer;
        private Label leftMatNameLabel, rightMatNameLabel;
        private DropdownField destDropdown;
        private Toggle diffOnlyToggle, includeTexToggle;
        private Button batchLeftBtn, batchRightBtn, batchClearBtn;
        private Button refreshBtn, closeBtn, applyBtn;
        private ScrollView blocksScroll;
        private Label planSummaryLabel, planBlocksLabel, errorMessageLabel;
        private VisualElement resultBox;
        private Label resultSummaryLabel, resultDetailsLabel;
        private Foldout resultFoldout;

        // ブロックごとのUI追跡用データ構造
        private class BlockVisualRefs
        {
            public ComparisonBlock Block;
            public VisualElement Container;
            public VisualElement LeftCard;
            public VisualElement RightCard;
            public Label LeftBadge;
            public Label RightBadge;
            public VisualElement ContentRow;
            public Label FoldoutIcon;
            public Label BlockTitle;
        }

        private readonly List<BlockVisualRefs> blockRefs = new List<BlockVisualRefs>();

        internal static void Open(Material left, Material right)
        {
            bool alreadyOpen = HasOpenInstances<MatSyncComparisonWindow>();
            var window = GetWindow<MatSyncComparisonWindow>("MatSync 比較");
            window.left = left;
            window.right = right;
            window.result = null;
            if (!alreadyOpen) window.onlyDifferences = true;
            window.minSize = new Vector2(760, 480);
            if (!alreadyOpen) window.position = new Rect(window.position.x, window.position.y, 1020, 720);
            window.RefreshComparison(false);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            minSize = new Vector2(760, 480);
            Undo.undoRedoPerformed += CheckForChanges;
            EditorApplication.projectChanged += CheckForChanges;
            if (left && right && comparison == null) RefreshComparison(false);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= CheckForChanges;
            EditorApplication.projectChanged -= CheckForChanges;
        }

        private const double StaleCheckInterval = 1.0;
        private double nextStaleCheck;

        // Serializing both materials is costly; Apply re-validates anyway, so a slower poll is enough here.
        private void OnInspectorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now < nextStaleCheck) return;
            nextStaleCheck = now + StaleCheckInterval;
            CheckForChanges();
        }

        private void CheckForChanges()
        {
            if (comparison != null)
            {
                bool next = !comparison.IsCurrent;
                if (next != stale)
                {
                    stale = next;
                    UpdatePlanDisplay();
                }
            }
        }

        private void RefreshComparison(bool preserveOptions)
        {
            string invalid = LilToonBridge.Validate(left, false) ?? LilToonBridge.Validate(right, false);
            if (invalid != null)
            {
                comparison = null;
                stale = false;
                RebuildUI();
                ShowError(invalid);
                return;
            }

            int destination = preserveOptions && comparison != null ? comparison.Destination : 0;
            bool textures = preserveOptions && comparison != null && comparison.IncludeTextures;

            MaterialSchema.ClearCache();
            comparison = new MaterialComparison(left, right)
            {
                Destination = destination,
                IncludeTextures = textures
            };
            stale = false;

            if (rootContainer != null)
            {
                RebuildUI();
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            root.AddToClassList("dennoko-root");
            root.style.backgroundColor = (Color)new Color32(0x12, 0x12, 0x12, 0xFF);
            root.style.flexGrow = 1;

            DennokoUIFont.Apply(root);

            LoadAndApplyStyles(root);

            var uxml = LoadVisualTreeAsset(COMPARISON_WINDOW_UXML_GUID, COMPARISON_WINDOW_UXML_PATH);
            if (uxml == null)
            {
                root.Add(new Label("MatSyncComparisonWindow.uxml のロードに失敗しました。"));
                return;
            }

            uxml.CloneTree(root);
            rootContainer = root;

            BindElements(root);
            RebuildUI();
        }

        private static void LoadAndApplyStyles(VisualElement root)
        {
            var ussTheme = LoadStyleSheet(DENNOKO_THEME_USS_GUID, DENNOKO_THEME_USS_PATH);
            if (ussTheme != null) root.styleSheets.Add(ussTheme);

            var ussMatSync = LoadStyleSheet(MATSYNC_THEME_USS_GUID, MATSYNC_THEME_USS_PATH);
            if (ussMatSync != null) root.styleSheets.Add(ussMatSync);
        }

        private static StyleSheet LoadStyleSheet(string guid, string fallbackPath)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) path = fallbackPath;
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
        }

        private static VisualTreeAsset LoadVisualTreeAsset(string guid, string fallbackPath)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) path = fallbackPath;
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        }

        private void BindElements(VisualElement root)
        {
            leftMatNameLabel = root.Q<Label>("compare-left-mat-name");
            rightMatNameLabel = root.Q<Label>("compare-right-mat-name");

            refreshBtn = root.Q<Button>("compare-refresh-btn");
            refreshBtn.clicked += () => RefreshComparison(true);

            closeBtn = root.Q<Button>("compare-close-btn");
            closeBtn.clicked += Close;

            destDropdown = root.Q<DropdownField>("compare-dest-dropdown");
            destDropdown.RegisterValueChangedCallback(evt =>
            {
                if (comparison == null) return;
                int idx = destDropdown.index;
                comparison.Destination = idx >= 0 ? idx : 0;
                UpdatePlanDisplay();
            });

            batchLeftBtn = root.Q<Button>("batch-select-left");
            batchLeftBtn.clicked += () => BatchSelect(1);

            batchRightBtn = root.Q<Button>("batch-select-right");
            batchRightBtn.clicked += () => BatchSelect(2);

            batchClearBtn = root.Q<Button>("batch-clear");
            batchClearBtn.clicked += () => BatchSelect(0);

            diffOnlyToggle = root.Q<Toggle>("compare-diff-only-toggle");
            diffOnlyToggle.value = onlyDifferences;
            diffOnlyToggle.RegisterValueChangedCallback(evt =>
            {
                onlyDifferences = evt.newValue;
                FilterBlocksVisibility();
            });

            includeTexToggle = root.Q<Toggle>("compare-include-tex-toggle");
            includeTexToggle.RegisterValueChangedCallback(evt =>
            {
                if (comparison == null) return;
                comparison.IncludeTextures = evt.newValue;
                // Per-property statuses depend on the texture option; choices live on the blocks and survive.
                BuildComparisonBlocks();
                UpdatePlanDisplay();
            });

            blocksScroll = root.Q<ScrollView>("compare-blocks-scroll");

            planSummaryLabel = root.Q<Label>("compare-plan-summary");
            planBlocksLabel = root.Q<Label>("compare-plan-blocks");
            errorMessageLabel = root.Q<Label>("compare-error-message");

            applyBtn = root.Q<Button>("compare-apply-btn");
            applyBtn.clicked += ApplyChanges;

            resultBox = root.Q<VisualElement>("compare-result-box");
            resultSummaryLabel = root.Q<Label>("compare-result-summary");
            resultDetailsLabel = root.Q<Label>("compare-result-details");
            resultFoldout = root.Q<Foldout>("compare-result-foldout");
        }

        private void RebuildUI()
        {
            if (rootContainer == null) return;

            if (comparison == null)
            {
                blocksScroll.Clear();
                var msg = new Label("マテリアルが指定されていません。基本ウィンドウから比較を開始してください。");
                msg.AddToClassList("dennoko-text-disabled");
                msg.style.marginTop = 20;
                msg.style.unityTextAlign = TextAnchor.MiddleCenter;
                blocksScroll.Add(msg);
                blockRefs.Clear();
                planSummaryLabel.text = string.Empty;
                planBlocksLabel.text = string.Empty;
                applyBtn.SetEnabled(false);
                return;
            }

            // マテリアル名表示
            leftMatNameLabel.text = left ? left.name : "参照切れ";
            rightMatNameLabel.text = right ? right.name : "参照切れ";

            // 適用先ドロップダウン初期化（デフォルトは 0: 両マテリアル）
            var choices = new List<string>
            {
                "両マテリアル（相互に反映）",
                $"左のみ: {(left ? left.name : "参照切れ")}",
                $"右のみ: {(right ? right.name : "参照切れ")}"
            };
            destDropdown.choices = choices;
            destDropdown.index = Mathf.Clamp(comparison.Destination, 0, choices.Count - 1);
            includeTexToggle.SetValueWithoutNotify(comparison.IncludeTextures);

            // 各機能ブロックのカード生成
            BuildComparisonBlocks();
            UpdatePlanDisplay();
        }

        private void BuildComparisonBlocks()
        {
            blocksScroll.Clear();
            blockRefs.Clear();

            foreach (var block in comparison.Blocks)
            {
                var refs = CreateBlockCard(block);
                blockRefs.Add(refs);
                blocksScroll.Add(refs.Container);
            }

            FilterBlocksVisibility();
        }

        private BlockVisualRefs CreateBlockCard(ComparisonBlock block)
        {
            int diffCount = block.Properties.Count(comparison.Different);
            int totalCount = block.Properties.Count;

            var container = new VisualElement();
            container.AddToClassList("dennoko-card");

            // ─── 左右2分割カードコンテナ（先に宣言してヘッダークリックから参照可能にする） ─────────────────
            var contentRow = new VisualElement();
            contentRow.AddToClassList("matsync-compare-row");
            contentRow.style.display = block.Expanded ? DisplayStyle.Flex : DisplayStyle.None;

            // ─── ブロックヘッダー ─────────────────────────
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 6;

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;

            var foldoutIcon = new Label(block.Expanded ? "▼" : "▶");
            foldoutIcon.style.fontSize = 10;
            foldoutIcon.style.marginRight = 6;
            foldoutIcon.AddToClassList("dennoko-text-tertiary");

            var titleLabel = new Label($"{block.Name}  ─  差分 {diffCount}/{totalCount}");
            titleLabel.AddToClassList("dennoko-section-title");
            titleLabel.style.fontSize = 12;
            if (diffCount > 0)
            {
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            titleRow.Add(foldoutIcon);
            titleRow.Add(titleLabel);
            header.Add(titleRow);

            // 折りたたみクリック
            header.RegisterCallback<ClickEvent>(evt =>
            {
                block.Expanded = !block.Expanded;
                foldoutIcon.text = block.Expanded ? "▼" : "▶";
                contentRow.style.display = block.Expanded ? DisplayStyle.Flex : DisplayStyle.None;
            });

            container.Add(header);

            // 左側カード
            var leftCard = CreateChoiceColumn(block, 1, comparison.Left, diffCount);
            // 右側カード
            var rightCard = CreateChoiceColumn(block, 2, comparison.Right, diffCount);

            contentRow.Add(leftCard.Column);
            contentRow.Add(rightCard.Column);
            container.Add(contentRow);

            var refs = new BlockVisualRefs
            {
                Block = block,
                Container = container,
                LeftCard = leftCard.Column,
                RightCard = rightCard.Column,
                LeftBadge = leftCard.Badge,
                RightBadge = rightCard.Badge,
                ContentRow = contentRow,
                FoldoutIcon = foldoutIcon,
                BlockTitle = titleLabel
            };

            // クリックハンドラー登録
            leftCard.Column.RegisterCallback<ClickEvent>(evt =>
            {
                // 既に左が選択中なら解除(0)、そうでなければ左(1)
                block.Choice = block.Choice == 1 ? 0 : 1;
                UpdateBlockCardSelection(refs);
                UpdatePlanDisplay();
            });

            rightCard.Column.RegisterCallback<ClickEvent>(evt =>
            {
                // 既に右が選択中なら解除(0)、そうでなければ右(2)
                block.Choice = block.Choice == 2 ? 0 : 2;
                UpdateBlockCardSelection(refs);
                UpdatePlanDisplay();
            });

            UpdateBlockCardSelection(refs);

            return refs;
        }

        private (VisualElement Column, Label Badge) CreateChoiceColumn(
            ComparisonBlock block, int side, MaterialSnapshot snapshot, int diffCount)
        {
            var col = new VisualElement();
            col.AddToClassList("matsync-compare-col");

            // カードトップ行: マテリアル名 + 採用バッジ
            var colHeader = new VisualElement();
            colHeader.style.flexDirection = FlexDirection.Row;
            colHeader.style.justifyContent = Justify.SpaceBetween;
            colHeader.style.alignItems = Align.Center;
            colHeader.style.marginBottom = 6;
            colHeader.style.paddingBottom = 4;
            colHeader.style.borderBottomWidth = 1;
            colHeader.style.borderBottomColor = new Color(1f, 1f, 1f, 0.08f);

            var nameLabel = new Label(side == 1 ? $"左: {(left ? left.name : "")}" : $"右: {(right ? right.name : "")}");
            nameLabel.AddToClassList("dennoko-text-primary");
            nameLabel.style.fontSize = 11;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.textOverflow = TextOverflow.Ellipsis;
            nameLabel.style.maxWidth = 160;

            var badge = new Label("採用する");
            badge.AddToClassList("matsync-badge");
            badge.AddToClassList("matsync-badge--unselected");

            colHeader.Add(nameLabel);
            colHeader.Add(badge);
            col.Add(colHeader);

            // プロパティ一覧
            foreach (string propName in block.Properties)
            {
                bool isDiff = comparison.Different(propName);
                var def = comparison.Definition(propName);

                var propRow = new VisualElement();
                propRow.AddToClassList("matsync-prop-item");
                if (isDiff) propRow.AddToClassList("matsync-prop-item--diff");
                propRow.userData = isDiff; // フィルタ用

                // プロパティ表示名
                var propLabel = new Label((isDiff ? "≠ " : "= ") + def.Label);
                propLabel.AddToClassList("matsync-prop-label");
                if (isDiff) propLabel.AddToClassList("dennoko-text-primary");
                propRow.Add(propLabel);

                // 値コンテナ
                var valContainer = new VisualElement();
                valContainer.style.flexDirection = FlexDirection.Row;
                valContainer.style.alignItems = Align.Center;

                if (snapshot.Values.TryGetValue(propName, out var val))
                {
                    if (val.Definition.Type == UnityEngine.Rendering.ShaderPropertyType.Color)
                    {
                        var chip = new VisualElement();
                        chip.AddToClassList("matsync-color-chip");
                        chip.style.backgroundColor = (Color)val.Vector;
                        valContainer.Add(chip);
                    }

                    var valText = new Label(val.Display());
                    valText.AddToClassList("matsync-prop-value");
                    valContainer.Add(valText);
                }
                else
                {
                    var noneText = new Label("— 不在");
                    noneText.AddToClassList("dennoko-text-disabled");
                    noneText.style.fontSize = 10;
                    valContainer.Add(noneText);
                }

                propRow.Add(valContainer);

                // 差分があっても適用されない理由（テクスチャ保持・不在・内部管理値など）を示す
                string status = comparison.PropertyStatus(propName);
                if (isDiff && status != MaterialComparison.DiffStatus)
                {
                    var statusLabel = new Label(status);
                    statusLabel.AddToClassList("dennoko-text-warning");
                    statusLabel.style.fontSize = 10;
                    propRow.Add(statusLabel);
                }

                col.Add(propRow);
            }

            return (col, badge);
        }

        private void UpdateBlockCardSelection(BlockVisualRefs refs)
        {
            int choice = refs.Block.Choice;

            // 左カードの採用状態（鮮やかなネオングリーンボーダー）
            bool leftSelected = choice == 1;
            refs.LeftCard.EnableInClassList("matsync-compare-col--selected", leftSelected);
            refs.LeftBadge.EnableInClassList("matsync-badge--selected", leftSelected);
            refs.LeftBadge.EnableInClassList("matsync-badge--unselected", !leftSelected);
            refs.LeftBadge.text = leftSelected ? "✓ 採用中" : "採用する";

            // 右カードの採用状態（鮮やかなネオングリーンボーダー）
            bool rightSelected = choice == 2;
            refs.RightCard.EnableInClassList("matsync-compare-col--selected", rightSelected);
            refs.RightBadge.EnableInClassList("matsync-badge--selected", rightSelected);
            refs.RightBadge.EnableInClassList("matsync-badge--unselected", !rightSelected);
            refs.RightBadge.text = rightSelected ? "✓ 採用中" : "採用する";
        }

        private void BatchSelect(int choice)
        {
            if (comparison == null) return;
            foreach (var r in blockRefs)
            {
                r.Block.Choice = choice;
                UpdateBlockCardSelection(r);
            }
            UpdatePlanDisplay();
        }

        private void FilterBlocksVisibility()
        {
            foreach (var r in blockRefs)
            {
                int diffCount = r.Block.Properties.Count(comparison.Different);
                if (onlyDifferences && diffCount == 0)
                {
                    r.Container.style.display = DisplayStyle.None;
                }
                else
                {
                    r.Container.style.display = DisplayStyle.Flex;

                    // ブロック内の各プロパティ行の表示/非表示
                    FilterPropertyItems(r.LeftCard);
                    FilterPropertyItems(r.RightCard);
                }
            }
        }

        private void FilterPropertyItems(VisualElement card)
        {
            foreach (var child in card.Children())
            {
                if (child.userData is bool isDiff)
                {
                    child.style.display = (onlyDifferences && !isDiff) ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }
        }

        private void UpdatePlanDisplay()
        {
            if (comparison == null) return;

            var plans = comparison.BuildPlans();
            int totalChanges = plans.Sum(p => p.Changes.Count);
            int totalSkipped = plans.Sum(p => p.SkippedProperties.Count);

            if (stale)
            {
                errorMessageLabel.text = "比較中にマテリアルが変更されました。再比較してください（採用選択はリセットされます）。";
                errorMessageLabel.style.display = DisplayStyle.Flex;
                applyBtn.SetEnabled(false);
                return;
            }

            string destName;
            if (comparison.Destination == 0) destName = "両マテリアル";
            else if (comparison.Destination == 1) destName = left ? left.name : "左マテリアル";
            else destName = right ? right.name : "右マテリアル";

            string changedBlocks = string.Join("、", plans.SelectMany(p => p.Changes).Select(v => v.Definition.Block).Distinct());
            planSummaryLabel.text = $"適用先: {destName} ／ 変更予定: {totalChanges}項目 ／ 対象外: {totalSkipped}項目";
            planBlocksLabel.text = "変更するブロック: " + (changedBlocks.Length == 0 ? "なし（採用したブロックがありません）" : changedBlocks);

            bool canApply = totalChanges > 0 && !EditorApplication.isPlayingOrWillChangePlaymode;
            applyBtn.SetEnabled(canApply);
            errorMessageLabel.style.display = DisplayStyle.None;
        }

        private void ApplyChanges()
        {
            if (comparison == null) return;

            result = comparison.Apply();
            if (result != null)
            {
                resultBox.style.display = DisplayStyle.Flex;
                resultSummaryLabel.text = result.Summary;
                resultSummaryLabel.EnableInClassList("dennoko-text-error", result.Failed > 0);
                resultSummaryLabel.EnableInClassList("dennoko-text-primary", result.Failed == 0);
                resultDetailsLabel.text = result.Text;

                if (result.Failed == 0)
                {
                    RefreshComparison(true);
                }
                else
                {
                    CheckForChanges();
                }
            }
        }

        private void ShowError(string msg)
        {
            if (errorMessageLabel != null)
            {
                errorMessageLabel.text = msg;
                errorMessageLabel.style.display = DisplayStyle.Flex;
            }
        }
    }
}
