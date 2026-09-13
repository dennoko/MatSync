# プロトタイプの実装構成

## ファイルと責務

| ファイル | 責務 | 主な要件 |
| --- | --- | --- |
| `Editor/MaterialModel.cs` | 型付き値、スナップショット、Shader定義キャッシュ、lilToon API接続、共有Materialの抽出、入力検証 | MAT-01〜04、PROP-01〜03 |
| `Editor/PropertyCatalog.cs` | lilToon 2.3.4のプロパティー名・ブロック・除外理由とシェーダー許可リスト | PROP-01、PROP-03、COMP-02 |
| `Editor/MaterialTransfer.cs` | 転送可否・差分の判定、変更計画、まとめて適用、Undo登録、結果・失敗範囲 | PROP-02〜04、COPY-02、NFR-02 |
| `Editor/MaterialComparison.cs` | 左右の比較、ブロック採用、適用先、外部変更検出、適用計画 | COMP-02〜05 |
| `Editor/MaterialSyncSession.cs` | 明示開始・停止、変更された項目の一方向同期、対象の無効化、Undo／Redo後の基準更新 | SYNC-02〜05、NFR-03〜04 |
| `Editor/MaterialSelectionGUI.cs` | 単一欄、複数リスト、ドロップ領域、複数候補の選択 | MAT-01〜04 |
| `Editor/MatSyncWindow.cs` | 小さい基本ウィンドウ、モード切り替え、コピー・同期の実行と結果 | COPY-01、COMP-01、SYNC-01、UI-01 |
| `Editor/MatSyncComparisonWindow.cs` | 比較中の別ウィンドウ、左右の値、ブロック採用、適用前の確認表示 | COMP-01〜05、UI-01 |
| `Editor/dennokoworks.MatSync.Editor.asmdef` | Editor限定のアセンブリ境界 | NFR-01 |
| `Tools~/generate_catalog.py` | 対応バージョンのShader宣言から許可リスト・対応表を再生成 | PROP-03の対応表 |
| `Tools~/compile.ps1` | Unity同梱コンパイラーによる全C#ソースのコンパイル | ユーザー指定の検証範囲 |

初期UIはIMGUI。処理クラスはGUIを描画せず、選択されたMaterial参照・プロパティー集合・オプションを受け取り、変更計画や結果を返す。後続のUI Toolkit／USS移行ではUI側の3ファイルを主に置き換える。

## 実装した暫定仕様

- Hierarchyからの抽出はMeshRenderer／SkinnedMeshRendererの`sharedMaterials`を使用し、子階層・非アクティブも含める。
- 比較は同時1セッション。初期状態の適用先は未選択、全ブロックは「変更しない」。適用先は左右いずれか1つ。
- 比較ではMaterialの全シリアライズ状態を開始時に保持し、適用直前にも外部変更を再検証する。
- 全モードでテクスチャ参照・Tiling・Offsetを1つの保護単位として扱う。
- 同期開始時は基準値の取得だけを行い、その後変更された項目を同期する。
- 同期元を含む複数MaterialのInspectorを検出した場合は、明示的な再開を必要とする停止状態にする。
- 異なるシェーダー間でも許可リストの共通内部名・互換型を転送する。ただし「透過・描画設定」は同一シェーダー間のみ。モード切り替えに必要な項目は別途除外する。

## Undoと更新の扱い

コピー・比較は、実際に差分があるときだけ専用Undoグループを作り、変更対象の完全な状態を登録する。同期ではEditorの更新と`Undo.willFlushUndoRecord`で差分を処理し、現在のInspectorのUndoグループに同期先を登録する。同じグループ内では各同期先を1回だけ登録し、連続編集で登録を繰り返さない。

Undo／Redo処理中は同期せず、完了通知で同期元の基準値を取り直す。関連APIの定義は[Unity 2022.3 Undoリファレンス](https://docs.unity3d.com/ja/2022.3/ScriptReference/Undo.html)に基づく。通常のInspectorによる実際のグループ境界・連続ドラッグ・ロックしたInspectorの動作は、コンパイルだけでは検証できないため手動確認項目に含める。

マテリアル変更時にDirty化するが、毎更新の`SaveAssets`やプロジェクト全体の走査は行わない。同期停止中は更新コールバックを解除する。

## 対応範囲と残る検証

対象環境はUnity 2022.3.22f1、lilToon 2.3.4。プロパティー単位の方針は[対応表](property-support.md)を参照する。lilToonの公開Editor APIはリフレクションで接続し、パッケージ未導入時もツールのコンパイルが壊れない構成とした。

3モードの実装とコンパイルを完了し、ユーザー指定に従って実際のUnity操作は行っていない。[テスト手順](prototype-test.md)のAC対応項目、各バリアントの描画、Inspectorと同期のUndo統合、対象数に対する応答性は未検証。性能の数値目標は代表的な利用環境での測定後に確定する。
