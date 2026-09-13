using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DennokoWorks.MatSync
{
    public sealed class MatSyncWindow : EditorWindow
    {
        private const string DENNOKO_THEME_USS_PATH = "Assets/dennokoworks/MatSync/Editor/UI/DennokoTheme.uss";
        private const string MATSYNC_THEME_USS_PATH = "Assets/dennokoworks/MatSync/Editor/UI/MatSyncTheme.uss";
        private const string WINDOW_UXML_PATH = "Assets/dennokoworks/MatSync/Editor/UI/MatSyncWindow.uxml";

        // GUID（インポート後に更新可能）
        private const string DENNOKO_THEME_USS_GUID = "DENNOKO_THEME_USS_GUID";
        private const string MATSYNC_THEME_USS_GUID = "MATSYNC_THEME_USS_GUID";
        private const string WINDOW_UXML_GUID = "MATSYNC_WINDOW_UXML_GUID";

        [SerializeField] private int mode; // 0: コピー, 1: 比較, 2: 同時編集
        [SerializeField] private Material source, left, right;
        [SerializeField] private List<Material> copyTargets = new List<Material>();
        [SerializeField] private List<Material> syncTargets = new List<Material>();
        [SerializeField] private bool copyTextures, syncTextures;

        private TransferResult result;
        private MaterialSyncSession sync;

        // UI 要素参照
        private VisualElement rootContainer;
        private Button tabCopy, tabCompare, tabSync;
        private VisualElement viewCopy, viewCompare, viewSync;

        // コピー UI
        private ObjectField copySourceField;
        private ScrollView copyTargetsList;
        private Label copyTargetsTitle, copyTargetsValidCount, copyValidationMessage;
        private Toggle copyTexturesToggle;
        private Button copyExecuteButton;

        // 比較 UI
        private ObjectField compareLeftField, compareRightField;
        private Button compareOpenWindowButton;
        private Label compareValidationMessage;

        // 同時編集 UI
        private ObjectField syncSourceField;
        private ScrollView syncTargetsList;
        private Label syncTargetsTitle, syncTargetsValidCount, syncStatusText, syncValidationMessage;
        private Toggle syncTexturesToggle;
        private Button syncToggleButton, syncSourcePingButton;

        // 結果 UI
        private VisualElement resultCard;
        private Label resultSummary, resultDetailsText;
        private Foldout resultFoldout;

        // ステータスバー
        private Label statusLabel;
        private IVisualElementScheduledItem statusResetSchedule;

        [MenuItem("Tools/dennokoworks/MatSync")]
        public static void Open()
        {
            bool alreadyOpen = HasOpenInstances<MatSyncWindow>();
            var window = GetWindow<MatSyncWindow>("MatSync");
            window.minSize = new Vector2(340, 380);
            if (!alreadyOpen) window.position = new Rect(window.position.x, window.position.y, 380, 620);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(340, 380);
            sync = new MaterialSyncSession();
            sync.Changed += OnSyncChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable()
        {
            sync?.Dispose();
            if (sync != null) sync.Changed -= OnSyncChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            MaterialSchema.ClearCache();
            UpdateUI();
        }

        private void OnUndoRedo()
        {
            UpdateUI();
        }

        private void OnSyncChanged()
        {
            UpdateUI();
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

            var uxml = LoadVisualTreeAsset(WINDOW_UXML_GUID, WINDOW_UXML_PATH);
            if (uxml == null)
            {
                root.Add(new Label("MatSyncWindow.uxml のロードに失敗しました。"));
                return;
            }

            uxml.CloneTree(root);
            rootContainer = root;

            BindElements(root);
            SwitchMode(mode);
            UpdateUI();
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
            // タブ
            tabCopy = root.Q<Button>("tab-copy");
            tabCompare = root.Q<Button>("tab-compare");
            tabSync = root.Q<Button>("tab-sync");

            tabCopy.clicked += () => SwitchMode(0);
            tabCompare.clicked += () => SwitchMode(1);
            tabSync.clicked += () => SwitchMode(2);

            viewCopy = root.Q<VisualElement>("view-copy");
            viewCompare = root.Q<VisualElement>("view-compare");
            viewSync = root.Q<VisualElement>("view-sync");

            // ─── コピー モード ──────────────────────────
            copySourceField = root.Q<ObjectField>("copy-source-field");
            copySourceField.value = source;
            copySourceField.RegisterValueChangedCallback(evt =>
            {
                SetSource((Material)evt.newValue);
            });

            root.Q<Button>("copy-source-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Choose(MaterialSelection.Extract(Selection.objects), SetSource, ShowStatusError);
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("copy-source-drop"), result =>
            {
                MaterialSelectionGUI.Choose(result, SetSource, ShowStatusError);
            });

            copyTargetsTitle = root.Q<Label>("copy-targets-title");
            copyTargetsValidCount = root.Q<Label>("copy-targets-valid-count");
            copyTargetsList = root.Q<ScrollView>("copy-targets-list");
            copyValidationMessage = root.Q<Label>("copy-validation-message");

            root.Q<Button>("copy-targets-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Add(MaterialSelection.Extract(Selection.objects), copyTargets, source, StopForSettings, SetStatusMessage);
                UpdateUI();
            };

            root.Q<Button>("copy-targets-clear").clicked += () =>
            {
                StopForSettings();
                copyTargets.Clear();
                UpdateUI();
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("copy-targets-drop"), result =>
            {
                MaterialSelectionGUI.Add(result, copyTargets, source, StopForSettings, SetStatusMessage);
                UpdateUI();
            });

            copyTexturesToggle = root.Q<Toggle>("copy-textures-toggle");
            copyTexturesToggle.value = copyTextures;
            copyTexturesToggle.RegisterValueChangedCallback(evt =>
            {
                StopForSettings();
                copyTextures = evt.newValue;
                UpdateUI();
            });

            copyExecuteButton = root.Q<Button>("copy-execute-button");
            copyExecuteButton.clicked += ExecuteCopy;

            // ─── 比較 モード ────────────────────────────
            compareLeftField = root.Q<ObjectField>("compare-left-field");
            compareLeftField.value = left;
            compareLeftField.RegisterValueChangedCallback(evt =>
            {
                left = (Material)evt.newValue;
                UpdateUI();
            });

            root.Q<Button>("compare-left-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Choose(MaterialSelection.Extract(Selection.objects), m => { left = m; compareLeftField.value = m; UpdateUI(); }, ShowStatusError);
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("compare-left-drop"), result =>
            {
                MaterialSelectionGUI.Choose(result, m => { left = m; compareLeftField.value = m; UpdateUI(); }, ShowStatusError);
            });

            compareRightField = root.Q<ObjectField>("compare-right-field");
            compareRightField.value = right;
            compareRightField.RegisterValueChangedCallback(evt =>
            {
                right = (Material)evt.newValue;
                UpdateUI();
            });

            root.Q<Button>("compare-right-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Choose(MaterialSelection.Extract(Selection.objects), m => { right = m; compareRightField.value = m; UpdateUI(); }, ShowStatusError);
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("compare-right-drop"), result =>
            {
                MaterialSelectionGUI.Choose(result, m => { right = m; compareRightField.value = m; UpdateUI(); }, ShowStatusError);
            });

            compareOpenWindowButton = root.Q<Button>("compare-open-window-button");
            compareOpenWindowButton.clicked += () => MatSyncComparisonWindow.Open(left, right);
            compareValidationMessage = root.Q<Label>("compare-validation-message");

            // ─── 同時編集 モード ────────────────────────
            syncSourceField = root.Q<ObjectField>("sync-source-field");
            syncSourceField.value = source;
            syncSourceField.RegisterValueChangedCallback(evt =>
            {
                SetSource((Material)evt.newValue);
            });

            syncSourcePingButton = root.Q<Button>("sync-source-ping");
            syncSourcePingButton.clicked += () =>
            {
                if (source) { Selection.activeObject = source; EditorGUIUtility.PingObject(source); }
            };

            root.Q<Button>("sync-source-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Choose(MaterialSelection.Extract(Selection.objects), SetSource, ShowStatusError);
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("sync-source-drop"), result =>
            {
                MaterialSelectionGUI.Choose(result, SetSource, ShowStatusError);
            });

            syncTargetsTitle = root.Q<Label>("sync-targets-title");
            syncTargetsValidCount = root.Q<Label>("sync-targets-valid-count");
            syncTargetsList = root.Q<ScrollView>("sync-targets-list");
            syncValidationMessage = root.Q<Label>("sync-validation-message");

            root.Q<Button>("sync-targets-from-selection").clicked += () =>
            {
                MaterialSelectionGUI.Add(MaterialSelection.Extract(Selection.objects), syncTargets, source, StopForSettings, SetStatusMessage);
                UpdateUI();
            };

            root.Q<Button>("sync-targets-clear").clicked += () =>
            {
                StopForSettings();
                syncTargets.Clear();
                UpdateUI();
            };

            MaterialSelectionGUI.SetupDropArea(root.Q<VisualElement>("sync-targets-drop"), result =>
            {
                MaterialSelectionGUI.Add(result, syncTargets, source, StopForSettings, SetStatusMessage);
                UpdateUI();
            });

            syncTexturesToggle = root.Q<Toggle>("sync-textures-toggle");
            syncTexturesToggle.value = syncTextures;
            syncTexturesToggle.RegisterValueChangedCallback(evt =>
            {
                StopForSettings();
                syncTextures = evt.newValue;
                UpdateUI();
            });

            syncStatusText = root.Q<Label>("sync-status-text");
            syncToggleButton = root.Q<Button>("sync-toggle-button");
            syncToggleButton.clicked += ToggleSync;

            // ─── 結果 & ステータス ──────────────────────
            resultCard = root.Q<VisualElement>("result-card");
            resultSummary = root.Q<Label>("result-summary");
            resultDetailsText = root.Q<Label>("result-details-text");
            resultFoldout = root.Q<Foldout>("result-foldout");

            statusLabel = root.Q<Label>("status-label");
        }

        private void SwitchMode(int newMode)
        {
            if (newMode != mode)
            {
                sync?.Stop("モード変更により停止しました。");
                mode = newMode;
                result = null;
            }

            tabCopy.EnableInClassList("matsync-tab-button--active", mode == 0);
            tabCompare.EnableInClassList("matsync-tab-button--active", mode == 1);
            tabSync.EnableInClassList("matsync-tab-button--active", mode == 2);

            viewCopy.style.display = mode == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            viewCompare.style.display = mode == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            viewSync.style.display = mode == 2 ? DisplayStyle.Flex : DisplayStyle.None;

            UpdateUI();
        }

        private void SetSource(Material material)
        {
            StopForSettings();
            source = material;
            copyTargets.RemoveAll(m => m && m == source);
            syncTargets.RemoveAll(m => m && m == source);
            if (copySourceField != null) copySourceField.value = source;
            if (syncSourceField != null) syncSourceField.value = source;
            UpdateUI();
        }

        private void StopForSettings()
        {
            sync?.Stop("設定が変更されたため停止しました。再開してください。");
            result = null;
        }

        private void UpdateUI()
        {
            if (rootContainer == null) return;

            // ─── コピー更新 ─────────────────────────────
            if (mode == 0)
            {
                int validCount = copyTargets.Count(m => m && m != source && LilToonBridge.Validate(m, true) == null);
                copyTargetsTitle.text = $"コピー先リスト ({copyTargets.Count}件)";
                copyTargetsValidCount.text = $"実行可能な対象: {validCount}件";
                RebuildMaterialList(copyTargetsList, copyTargets, () => { StopForSettings(); UpdateUI(); });

                string invalid = LilToonBridge.Validate(source, false);
                if (invalid == null && validCount == 0) invalid = "有効な対象を1件以上追加してください。";
                if (EditorApplication.isPlayingOrWillChangePlaymode) invalid = "Edit Modeで操作してください。";

                if (invalid != null)
                {
                    copyValidationMessage.text = invalid;
                    copyValidationMessage.style.display = DisplayStyle.Flex;
                    copyExecuteButton.SetEnabled(false);
                }
                else
                {
                    copyValidationMessage.style.display = DisplayStyle.None;
                    copyExecuteButton.SetEnabled(true);
                }
            }

            // ─── 比較更新 ───────────────────────────────
            if (mode == 1)
            {
                string invalid = LilToonBridge.Validate(left, false) ?? LilToonBridge.Validate(right, false);
                if (invalid == null && left == right) invalid = "左右に異なるマテリアルを指定してください。";
                if (EditorApplication.isPlayingOrWillChangePlaymode) invalid = "Edit Modeで操作してください。";

                if (invalid != null)
                {
                    compareValidationMessage.text = invalid;
                    compareValidationMessage.style.display = DisplayStyle.Flex;
                    compareOpenWindowButton.SetEnabled(false);
                }
                else
                {
                    compareValidationMessage.style.display = DisplayStyle.None;
                    compareOpenWindowButton.SetEnabled(true);
                }
            }

            // ─── 同時編集更新 ───────────────────────────
            if (mode == 2)
            {
                syncSourcePingButton.SetEnabled(source != null);
                int validCount = syncTargets.Count(m => m && m != source && LilToonBridge.Validate(m, true) == null);
                syncTargetsTitle.text = $"同期先リスト ({syncTargets.Count}件)";
                syncTargetsValidCount.text = $"実行可能な対象: {validCount}件";
                RebuildMaterialList(syncTargetsList, syncTargets, () => { StopForSettings(); UpdateUI(); });

                string invalid = LilToonBridge.Validate(source, false);
                if (invalid == null && validCount == 0) invalid = "有効な対象を1件以上追加してください。";
                if (EditorApplication.isPlayingOrWillChangePlaymode) invalid = "Edit Modeで操作してください。";

                syncStatusText.text = sync.Status;
                syncToggleButton.text = sync.Running ? "同期を停止" : "同期を開始";

                if (sync.Running)
                {
                    syncValidationMessage.style.display = DisplayStyle.None;
                    syncToggleButton.SetEnabled(true);
                    syncToggleButton.AddToClassList("dennoko-button-primary");
                }
                else
                {
                    if (invalid != null)
                    {
                        syncValidationMessage.text = invalid;
                        syncValidationMessage.style.display = DisplayStyle.Flex;
                        syncToggleButton.SetEnabled(false);
                    }
                    else
                    {
                        syncValidationMessage.style.display = DisplayStyle.None;
                        syncToggleButton.SetEnabled(true);
                    }
                }
            }

            // ─── 結果カード更新 ─────────────────────────
            var currentResult = mode == 2 ? sync.LastResult : result;
            if (currentResult != null)
            {
                resultCard.style.display = DisplayStyle.Flex;
                resultSummary.text = currentResult.Summary;
                resultSummary.EnableInClassList("dennoko-text-error", currentResult.Failed > 0);
                resultSummary.EnableInClassList("dennoko-text-primary", currentResult.Failed == 0);
                resultDetailsText.text = currentResult.Text;
            }
            else
            {
                resultCard.style.display = DisplayStyle.None;
            }
        }

        private void RebuildMaterialList(ScrollView container, List<Material> list, Action onChanged)
        {
            container.Clear();
            if (list.Count == 0)
            {
                var emptyLabel = new Label("対象マテリアルが登録されていません。");
                emptyLabel.AddToClassList("dennoko-text-disabled");
                emptyLabel.style.fontSize = 11;
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.marginTop = 10;
                container.Add(emptyLabel);
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                int index = i;
                var mat = list[index];

                var row = new VisualElement();
                row.AddToClassList("matsync-list-row");

                var objField = new ObjectField { value = mat, objectType = typeof(Material), allowSceneObjects = false };
                objField.style.flexGrow = 1;
                objField.style.flexShrink = 1;
                objField.SetEnabled(false);
                row.Add(objField);

                var pingBtn = new Button(() => { if (mat) EditorGUIUtility.PingObject(mat); }) { text = "表示" };
                pingBtn.AddToClassList("matsync-icon-btn");
                pingBtn.style.fontSize = 10;
                pingBtn.SetEnabled(mat != null);
                row.Add(pingBtn);

                var removeBtn = new Button(() =>
                {
                    list.RemoveAt(index);
                    onChanged?.Invoke();
                }) { text = "×" };
                removeBtn.AddToClassList("matsync-icon-btn");
                removeBtn.style.fontSize = 11;
                row.Add(removeBtn);

                container.Add(row);

                string reason = mat && mat == source ? "元と同一のため対象外" : LilToonBridge.Validate(mat, true);
                if (reason != null)
                {
                    var reasonLabel = new Label(reason);
                    reasonLabel.AddToClassList("dennoko-text-warning");
                    reasonLabel.style.fontSize = 10;
                    reasonLabel.style.marginLeft = 8;
                    reasonLabel.style.marginBottom = 2;
                    container.Add(reasonLabel);
                }
            }
        }

        private void ExecuteCopy()
        {
            MaterialSyncSession.Active?.Stop("コピーの実行により同期を停止しました。");
            result = MaterialTransfer.Copy(source, copyTargets, copyTextures);
            if (result != null)
            {
                SetStatus(result.Failed > 0 ? "コピー完了（一部失敗・除外あり）" : "コピー完了",
                    result.Failed > 0 ? StatusType.Error : StatusType.Success);
            }
            UpdateUI();
        }

        private void ToggleSync()
        {
            if (sync.Running)
            {
                sync.Stop("手動で停止しました。");
                SetStatus("同期を停止しました。", StatusType.Info);
            }
            else
            {
                sync.Start(source, syncTargets, syncTextures);
                SetStatus("同期を開始しました。", StatusType.Success);
            }
            UpdateUI();
        }

        private enum StatusType { Info, Success, Error }

        private void SetStatus(string message, StatusType type, long autoResetMs = 3500)
        {
            if (statusLabel == null) return;
            statusLabel.text = message;
            statusLabel.EnableInClassList("dennoko-status--success", type == StatusType.Success);
            statusLabel.EnableInClassList("dennoko-status--error", type == StatusType.Error);

            statusResetSchedule?.Pause();
            if (type != StatusType.Info)
            {
                statusResetSchedule = statusLabel.schedule
                    .Execute(() => SetStatus("Ready", StatusType.Info))
                    .StartingIn(autoResetMs);
            }
        }

        private void SetStatusMessage(string msg) => SetStatus(msg, StatusType.Info);
        private void ShowStatusError(string msg) => SetStatus(msg, StatusType.Error);
    }
}
