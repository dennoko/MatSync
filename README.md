# MatSync プロトタイプ

lilToonマテリアルのコピー、機能ブロック単位の比較・適用、通常のInspectorからの同時編集を行うUnity Editor拡張です。

Unityの **Tools → dennokoworks → MatSync** から開きます。

## 使用環境

- 実装・コンパイル対象: **Unity 2022.3.22f1 / lilToon 2.3.4**。
- Editor専用です。VRChat SDKへの直接依存やランタイムコンポーネントはありません。
- lilToonがない環境でもツール本体はコンパイルでき、操作時に未導入・未対応を表示します。
- lilToon 2.3.4の標準・Lite・Multi・Outline・Fur等、[対応表](Docs/Impl/property-support.md)の52シェーダーを判定対象にします。各バリアントの描画・操作確認は未実施です。

## 基本操作

1. **コピー**: コピー元と複数のコピー先を指定し、「コピー」を押します。
2. **比較**: 左右を指定して比較ウィンドウを開きます。適用先を選び、ブロックごとに「左を採用」「右を採用」を選択して適用します。
3. **同時編集**: 同期元と同期先を指定して開始し、通常のMaterial Inspectorで同期元を編集します。開始後に変更した項目だけを同期します。

指定欄へMaterialアセット、またはHierarchyのメッシュをドロップできます。複数メッシュ、子階層、非アクティブなオブジェクトも対象です。元の欄へ複数候補を渡すと、1件を選ぶメニューが開きます。

全モードでテクスチャ参照・Tiling・Offsetを既定で保持します。「テクスチャを含める」をONにすると転送します。色、強度、使用有無、独立したUV関連の数値は通常の設定として転送します。

変更はMaterialアセットに反映され、同じアセットを使用するオブジェクトにも影響します。保存はUnityの通常の保存操作を使用してください。

## プロトタイプの範囲

- UIは簡易IMGUIです。マテリアル抽出・差分計算・変更計画・適用・同期状態はUIから分離しており、後でUI Toolkit／USSへ移行する構成です。
- Shader自体、Render Queue、Override Tags、描画モードの切り替えはコピーしません。対応表にない項目や非互換の項目は除外理由を表示します。
- 通常バリアントはlilToonの`RemoveShaderKeywords`、Multiは`SetupMultiMaterial`を呼び、採用した設定に合わせて派生状態を調整します。
- 非表示のグラデーション編集キー・バージョン等は転送しません。生成済みテクスチャはテクスチャONで転送できますが、適用先のグラデーション編集キーは維持します。
- 同期元を含む複数MaterialのInspector編集は対象外です。検出時に同期を停止します。
- 同期は設定変更、モード切り替え、ウィンドウ終了、Play Mode移行、コンパイル・ドメインリロードで停止します。自動再開しません。
- Undo／Redoを実装しています。同期ではInspectorと同じUndoグループへ同期先の状態を登録し、復元後の値を再同期で上書きしないようにしています。実際のInspector操作との統合確認は、下記の手順で行ってください。

## 確認資料

- [Unity上のテスト手順・コンパイル結果](Docs/Impl/prototype-test.md)
- [詳細要件](Docs/Impl/requirements.md)
- [プロパティー・シェーダー対応表](Docs/Impl/property-support.md)
- [実装構成と仕様の対応](Docs/Impl/prototype-implementation.md)

コンパイルだけを再実行する場合は、MatSyncディレクトリでPowerShellから次を実行します。Unity Editorの起動やアセットの操作は行いません。

```powershell
& ./Tools~/compile.ps1
```

コンパイル出力はUnityがインポートしない`Temp~/Compile`に生成します。Unityのインストール先が異なる場合は`-UnityEditor '…/Editor'`を指定してください。
