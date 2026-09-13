# lilToon 2.3.4 プロパティー対応表

同梱環境の標準Shaderディレクトリから抽出した内部名を許可リストとして使用する。未知の内部名は除外する。
各転送時に実際のShaderの型・テクスチャ次元・プロパティーの存在を確認する。FloatとRangeの相互転送を許可し、Intとは区別する。
テクスチャは既定で参照・Tiling・Offsetを保持する。ONの場合だけ転送する。
描画設定は同一シェーダー間のみ対応し、Shader、Render Queue、Override Tagsは転送しない。
通常バリアントはlilToonのRemoveShaderKeywords、MultiはSetupMultiMaterialで採用値から派生状態を整合させる。
非表示のグラデーション編集情報は転送しない。生成済みグラデーションテクスチャはテクスチャONで転送できるが、編集キーは適用先のままとなる。

## プロパティー

| 内部名 | 宣言型 | ブロック | 方針 |
| --- | --- | --- | --- |
| `_AAStrength` | Range | ライティング | 転送 |
| `_AlphaBoostFA` | Range | 透過・描画設定 | 転送 |
| `_AlphaMask` | 2D | アルファマスク | オプションONで転送 |
| `_AlphaMaskMode` | Int | アルファマスク | 転送 |
| `_AlphaMaskScale` | Float | アルファマスク | 転送 |
| `_AlphaMaskValue` | Float | アルファマスク | 転送 |
| `_AlphaToMask` | Int | 透過・描画設定 | 転送 |
| `_Anisotropy2MatCap` | Int | 異方性反射 | 転送 |
| `_Anisotropy2MatCap2nd` | Int | 異方性反射 | 転送 |
| `_Anisotropy2Reflection` | Int | 異方性反射 | 転送 |
| `_Anisotropy2ndBitangentWidth` | Range | 異方性反射 | 転送 |
| `_Anisotropy2ndShift` | Range | 異方性反射 | 転送 |
| `_Anisotropy2ndShiftNoiseScale` | Range | 異方性反射 | 転送 |
| `_Anisotropy2ndSpecularStrength` | Range | 異方性反射 | 転送 |
| `_Anisotropy2ndTangentWidth` | Range | 異方性反射 | 転送 |
| `_AnisotropyBitangentWidth` | Range | 異方性反射 | 転送 |
| `_AnisotropyScale` | Range | 異方性反射 | 転送 |
| `_AnisotropyScaleMask` | 2D | 異方性反射 | オプションONで転送 |
| `_AnisotropyShift` | Range | 異方性反射 | 転送 |
| `_AnisotropyShiftNoiseMask` | 2D | 異方性反射 | オプションONで転送 |
| `_AnisotropyShiftNoiseScale` | Range | 異方性反射 | 転送 |
| `_AnisotropySpecularStrength` | Range | 異方性反射 | 転送 |
| `_AnisotropyTangentMap` | 2D | 異方性反射 | オプションONで転送 |
| `_AnisotropyTangentWidth` | Range | 異方性反射 | 転送 |
| `_ApplyReflection` | Int | 反射 | 転送 |
| `_ApplySpecular` | Int | 反射 | 転送 |
| `_ApplySpecularFA` | Int | 反射 | 転送 |
| `_AsOverlay` | Int | 透過・描画設定 | シェーダー・描画モードの変更が必要な設定は対象外 |
| `_AsUnlit` | Range | ライティング | 転送 |
| `_AudioLink2Emission` | Int | 発光 | 転送 |
| `_AudioLink2Emission2nd` | Int | 発光 | 転送 |
| `_AudioLink2Emission2ndGrad` | Int | 発光 | 転送 |
| `_AudioLink2EmissionGrad` | Int | 発光 | 転送 |
| `_AudioLink2Main2nd` | Int | AudioLink | 転送 |
| `_AudioLink2Main3rd` | Int | AudioLink | 転送 |
| `_AudioLink2Vertex` | Int | AudioLink | 転送 |
| `_AudioLinkAsLocal` | Int | AudioLink | 転送 |
| `_AudioLinkDefaultValue` | Vector | AudioLink | 転送 |
| `_AudioLinkLocalMap` | 2D | AudioLink | オプションONで転送 |
| `_AudioLinkLocalMapParams` | Vector | AudioLink | 転送 |
| `_AudioLinkMask` | 2D | AudioLink | オプションONで転送 |
| `_AudioLinkMask_ScrollRotate` | Vector | AudioLink | 転送 |
| `_AudioLinkMask_UVMode` | Int | AudioLink | 転送 |
| `_AudioLinkStart` | Vector | AudioLink | 転送 |
| `_AudioLinkUVMode` | Int | AudioLink | 転送 |
| `_AudioLinkUVParams` | Vector | AudioLink | 転送 |
| `_AudioLinkVertexStart` | Vector | AudioLink | 転送 |
| `_AudioLinkVertexStrength` | Vector | AudioLink | 転送 |
| `_AudioLinkVertexUVMode` | Int | AudioLink | 転送 |
| `_AudioLinkVertexUVParams` | Vector | AudioLink | 転送 |
| `_BackfaceColor` | Color | ライティング | 転送 |
| `_BackfaceForceShadow` | Range | 影 | 転送 |
| `_BacklightBackfaceMask` | Int | ライティング | 転送 |
| `_BacklightBlur` | Range | ライティング | 転送 |
| `_BacklightBorder` | Range | ライティング | 転送 |
| `_BacklightColor` | Color | ライティング | 転送 |
| `_BacklightColorTex` | 2D | ライティング | オプションONで転送 |
| `_BacklightDirectivity` | Float | ライティング | 転送 |
| `_BacklightMainStrength` | Range | ライティング | 転送 |
| `_BacklightNormalStrength` | Range | ライティング | 転送 |
| `_BacklightReceiveShadow` | Int | 影 | 転送 |
| `_BacklightViewStrength` | Range | ライティング | 転送 |
| `_BaseColor` | Color | 内部・派生値 | 派生値: 採用した基本色／MainTexから必要時だけ再計算 |
| `_BaseColorMap` | 2D | 内部・派生値 | 派生値: 採用した基本色／MainTexから必要時だけ再計算 |
| `_BaseMap` | 2D | 内部・派生値 | 派生値: 採用した基本色／MainTexから必要時だけ再計算 |
| `_BeforeExposureLimit` | Float | ライティング | 転送 |
| `_BlendOp` | Int | 透過・描画設定 | 転送 |
| `_BlendOpAlpha` | Int | 透過・描画設定 | 転送 |
| `_BlendOpAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_BlendOpFA` | Int | 透過・描画設定 | 転送 |
| `_Bump2ndMap` | 2D | ノーマル | オプションONで転送 |
| `_Bump2ndMap_UVMode` | Int | ノーマル | 転送 |
| `_Bump2ndScale` | Range | ノーマル | 転送 |
| `_Bump2ndScaleMask` | 2D | ノーマル | オプションONで転送 |
| `_BumpMap` | 2D | ノーマル | オプションONで転送 |
| `_BumpScale` | Range | ノーマル | 転送 |
| `_Color` | Color | 基本色 | 転送 |
| `_Color2nd` | Color | 基本色 2nd | 転送 |
| `_Color3rd` | Color | 基本色 3rd | 転送 |
| `_ColorMask` | Int | 透過・描画設定 | 転送 |
| `_Cull` | Int | 透過・描画設定 | 転送 |
| `_Cutoff` | Range | 透過・描画設定 | 転送 |
| `_DissolveColor` | Color | ディゾルブ | 転送 |
| `_DissolveMask` | 2D | ディゾルブ | オプションONで転送 |
| `_DissolveNoiseMask` | 2D | ディゾルブ | オプションONで転送 |
| `_DissolveNoiseMask_ScrollRotate` | Vector | ディゾルブ | 転送 |
| `_DissolveNoiseStrength` | float | ディゾルブ | 転送 |
| `_DissolveParams` | Vector | ディゾルブ | 転送 |
| `_DissolvePos` | Vector | ディゾルブ | 転送 |
| `_DistanceFade` | Vector | 距離フェード | 転送 |
| `_DistanceFadeColor` | Color | 距離フェード | 転送 |
| `_DistanceFadeMode` | Int | 距離フェード | 転送 |
| `_DistanceFadeRimColor` | Color | リムライト | 転送 |
| `_DistanceFadeRimFresnelPower` | Range | リムライト | 転送 |
| `_DitherMaxValue` | Float | ディザ | 転送 |
| `_DitherTex` | 2D | ディザ | オプションONで転送 |
| `_DstBlend` | Int | 透過・描画設定 | 転送 |
| `_DstBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_DstBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_DstBlendFA` | Int | 透過・描画設定 | 転送 |
| `_DummyProperty` | Float | 内部・派生値 | シェーダー診断用のダミー値 |
| `_Emission2ndBlend` | Range | 発光 | 転送 |
| `_Emission2ndBlendMask` | 2D | 発光 | オプションONで転送 |
| `_Emission2ndBlendMask_ScrollRotate` | Vector | 発光 | 転送 |
| `_Emission2ndBlendMode` | Int | 発光 | 転送 |
| `_Emission2ndBlink` | Vector | 発光 | 転送 |
| `_Emission2ndColor` | Color | 発光 | 転送 |
| `_Emission2ndFluorescence` | Range | 発光 | 転送 |
| `_Emission2ndGradSpeed` | Float | 発光 | 転送 |
| `_Emission2ndGradTex` | 2D | 発光 | オプションONで転送 |
| `_Emission2ndMainStrength` | Range | 発光 | 転送 |
| `_Emission2ndMap` | 2D | 発光 | オプションONで転送 |
| `_Emission2ndMap_ScrollRotate` | Vector | 発光 | 転送 |
| `_Emission2ndMap_UVMode` | Int | 発光 | 転送 |
| `_Emission2ndParallaxDepth` | float | 発光 | 転送 |
| `_Emission2ndUseGrad` | Int | 発光 | 転送 |
| `_EmissionBlend` | Range | 発光 | 転送 |
| `_EmissionBlendMask` | 2D | 発光 | オプションONで転送 |
| `_EmissionBlendMask_ScrollRotate` | Vector | 発光 | 転送 |
| `_EmissionBlendMode` | Int | 発光 | 転送 |
| `_EmissionBlink` | Vector | 発光 | 転送 |
| `_EmissionColor` | Color | 発光 | 転送 |
| `_EmissionFluorescence` | Range | 発光 | 転送 |
| `_EmissionGradSpeed` | Float | 発光 | 転送 |
| `_EmissionGradTex` | 2D | 発光 | オプションONで転送 |
| `_EmissionMainStrength` | Range | 発光 | 転送 |
| `_EmissionMap` | 2D | 発光 | オプションONで転送 |
| `_EmissionMap_ScrollRotate` | Vector | 発光 | 転送 |
| `_EmissionMap_UVMode` | Int | 発光 | 転送 |
| `_EmissionParallaxDepth` | float | 発光 | 転送 |
| `_EmissionUseGrad` | Int | 発光 | 転送 |
| `_EnvRimBlur` | Range | リムライト | 転送 |
| `_EnvRimBorder` | Range | リムライト | 転送 |
| `_FakeShadowVector` | Vector | 影 | 転送 |
| `_FlipNormal` | Int | ライティング | 転送 |
| `_FurAO` | Range | ファー | 転送 |
| `_FurAlphaToMask` | Int | 透過・描画設定 | 転送 |
| `_FurBlendOp` | Int | 透過・描画設定 | 転送 |
| `_FurBlendOpAlpha` | Int | 透過・描画設定 | 転送 |
| `_FurBlendOpAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_FurBlendOpFA` | Int | 透過・描画設定 | 転送 |
| `_FurColorMask` | Int | 透過・描画設定 | 転送 |
| `_FurCull` | Int | 透過・描画設定 | 転送 |
| `_FurCutoutLength` | Float | ファー | 転送 |
| `_FurDstBlend` | Int | 透過・描画設定 | 転送 |
| `_FurDstBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_FurDstBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_FurDstBlendFA` | Int | 透過・描画設定 | 転送 |
| `_FurGravity` | Range | ファー | 転送 |
| `_FurLayerNum` | Range | ファー | 転送 |
| `_FurLengthMask` | 2D | ファー | オプションONで転送 |
| `_FurMask` | 2D | ファー | オプションONで転送 |
| `_FurNoiseMask` | 2D | ファー | オプションONで転送 |
| `_FurOffsetFactor` | Float | 透過・描画設定 | 転送 |
| `_FurOffsetUnits` | Float | 透過・描画設定 | 転送 |
| `_FurRandomize` | Float | ファー | 転送 |
| `_FurRimAntiLight` | Range | リムライト | 転送 |
| `_FurRimColor` | Color | リムライト | 転送 |
| `_FurRimFresnelPower` | Range | リムライト | 転送 |
| `_FurRootOffset` | Range | ファー | 転送 |
| `_FurSrcBlend` | Int | 透過・描画設定 | 転送 |
| `_FurSrcBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_FurSrcBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_FurSrcBlendFA` | Int | 透過・描画設定 | 転送 |
| `_FurStencilComp` | Float | 透過・描画設定 | 転送 |
| `_FurStencilFail` | Float | 透過・描画設定 | 転送 |
| `_FurStencilPass` | Float | 透過・描画設定 | 転送 |
| `_FurStencilReadMask` | Range | 透過・描画設定 | 転送 |
| `_FurStencilRef` | Range | 透過・描画設定 | 転送 |
| `_FurStencilWriteMask` | Range | 透過・描画設定 | 転送 |
| `_FurStencilZFail` | Float | 透過・描画設定 | 転送 |
| `_FurTouchStrength` | Range | ファー | 転送 |
| `_FurVector` | Vector | ファー | 転送 |
| `_FurVectorScale` | Range | ファー | 転送 |
| `_FurVectorTex` | 2D | ファー | オプションONで転送 |
| `_FurZClip` | Int | 透過・描画設定 | 転送 |
| `_FurZTest` | Int | 透過・描画設定 | 転送 |
| `_FurZWrite` | Int | 透過・描画設定 | 転送 |
| `_GSAAStrength` | Range | 反射 | 転送 |
| `_GemChromaticAberration` | Range | 宝石 | 転送 |
| `_GemEnvColor` | Color | 宝石 | 転送 |
| `_GemEnvContrast` | Float | 宝石 | 転送 |
| `_GemParticleColor` | Color | 宝石 | 転送 |
| `_GemParticleLoop` | Float | 宝石 | 転送 |
| `_GemVRParallaxStrength` | Range | 視差 | 転送 |
| `_GlitterAngleRandomize` | Int | ラメ | 転送 |
| `_GlitterApplyShape` | Int | ラメ | 転送 |
| `_GlitterApplyTransparency` | Int | ラメ | 転送 |
| `_GlitterAtras` | Vector | ラメ | 転送 |
| `_GlitterBackfaceMask` | Int | ラメ | 転送 |
| `_GlitterColor` | Color | ラメ | 転送 |
| `_GlitterColorTex` | 2D | ラメ | オプションONで転送 |
| `_GlitterColorTex_UVMode` | Int | ラメ | 転送 |
| `_GlitterEnableLighting` | Range | ライティング | 転送 |
| `_GlitterMainStrength` | Range | ラメ | 転送 |
| `_GlitterNormalStrength` | Range | ラメ | 転送 |
| `_GlitterParams1` | Vector | ラメ | 転送 |
| `_GlitterParams2` | Vector | ラメ | 転送 |
| `_GlitterPostContrast` | Float | ラメ | 転送 |
| `_GlitterScaleRandomize` | Range | ラメ | 転送 |
| `_GlitterSensitivity` | Float | ラメ | 転送 |
| `_GlitterShadowMask` | Range | 影 | 転送 |
| `_GlitterShapeTex` | 2D | ラメ | オプションONで転送 |
| `_GlitterUVMode` | Int | ラメ | 転送 |
| `_GlitterVRParallaxStrength` | Range | ラメ | 転送 |
| `_IDMask1` | Int | IDマスク | 転送 |
| `_IDMask2` | Int | IDマスク | 転送 |
| `_IDMask3` | Int | IDマスク | 転送 |
| `_IDMask4` | Int | IDマスク | 転送 |
| `_IDMask5` | Int | IDマスク | 転送 |
| `_IDMask6` | Int | IDマスク | 転送 |
| `_IDMask7` | Int | IDマスク | 転送 |
| `_IDMask8` | Int | IDマスク | 転送 |
| `_IDMaskCompile` | Int | IDマスク | 転送 |
| `_IDMaskControlsDissolve` | Int | IDマスク | 転送 |
| `_IDMaskFrom` | Int | IDマスク | 転送 |
| `_IDMaskIndex1` | Int | IDマスク | 転送 |
| `_IDMaskIndex2` | Int | IDマスク | 転送 |
| `_IDMaskIndex3` | Int | IDマスク | 転送 |
| `_IDMaskIndex4` | Int | IDマスク | 転送 |
| `_IDMaskIndex5` | Int | IDマスク | 転送 |
| `_IDMaskIndex6` | Int | IDマスク | 転送 |
| `_IDMaskIndex7` | Int | IDマスク | 転送 |
| `_IDMaskIndex8` | Int | IDマスク | 転送 |
| `_IDMaskIsBitmap` | Int | IDマスク | 転送 |
| `_IDMaskPrior1` | Int | IDマスク | 転送 |
| `_IDMaskPrior2` | Int | IDマスク | 転送 |
| `_IDMaskPrior3` | Int | IDマスク | 転送 |
| `_IDMaskPrior4` | Int | IDマスク | 転送 |
| `_IDMaskPrior5` | Int | IDマスク | 転送 |
| `_IDMaskPrior6` | Int | IDマスク | 転送 |
| `_IDMaskPrior7` | Int | IDマスク | 転送 |
| `_IDMaskPrior8` | Int | IDマスク | 転送 |
| `_Invisible` | Int | 透過・描画設定 | 転送 |
| `_LightDirectionOverride` | Vector | ライティング | 転送 |
| `_LightMaxLimit` | Range | ライティング | 転送 |
| `_LightMinLimit` | Range | ライティング | 転送 |
| `_Main2ndBlendMask` | 2D | 基本色 2nd | オプションONで転送 |
| `_Main2ndDissolveColor` | Color | 基本色 2nd | 転送 |
| `_Main2ndDissolveMask` | 2D | 基本色 2nd | オプションONで転送 |
| `_Main2ndDissolveNoiseMask` | 2D | 基本色 2nd | オプションONで転送 |
| `_Main2ndDissolveNoiseMask_ScrollRotate` | Vector | 基本色 2nd | 転送 |
| `_Main2ndDissolveNoiseStrength` | float | 基本色 2nd | 転送 |
| `_Main2ndDissolveParams` | Vector | 基本色 2nd | 転送 |
| `_Main2ndDissolvePos` | Vector | 基本色 2nd | 転送 |
| `_Main2ndDistanceFade` | Vector | 距離フェード | 転送 |
| `_Main2ndEnableLighting` | Range | ライティング | 転送 |
| `_Main2ndTex` | 2D | 基本色 2nd | オプションONで転送 |
| `_Main2ndTexAlphaMode` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexAngle` | Float | 基本色 2nd | 転送 |
| `_Main2ndTexBlendMode` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexDecalAnimation` | Vector | 基本色 2nd | 転送 |
| `_Main2ndTexDecalSubParam` | Vector | 基本色 2nd | 転送 |
| `_Main2ndTexIsDecal` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexIsLeftOnly` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexIsMSDF` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexIsRightOnly` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexShouldCopy` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexShouldFlipCopy` | Int | 基本色 2nd | 転送 |
| `_Main2ndTexShouldFlipMirror` | Int | 基本色 2nd | 転送 |
| `_Main2ndTex_Cull` | Int | 基本色 2nd | 転送 |
| `_Main2ndTex_ScrollRotate` | Vector | 基本色 2nd | 転送 |
| `_Main2ndTex_UVMode` | Int | 基本色 2nd | 転送 |
| `_Main3rdBlendMask` | 2D | 基本色 3rd | オプションONで転送 |
| `_Main3rdDissolveColor` | Color | 基本色 3rd | 転送 |
| `_Main3rdDissolveMask` | 2D | 基本色 3rd | オプションONで転送 |
| `_Main3rdDissolveNoiseMask` | 2D | 基本色 3rd | オプションONで転送 |
| `_Main3rdDissolveNoiseMask_ScrollRotate` | Vector | 基本色 3rd | 転送 |
| `_Main3rdDissolveNoiseStrength` | float | 基本色 3rd | 転送 |
| `_Main3rdDissolveParams` | Vector | 基本色 3rd | 転送 |
| `_Main3rdDissolvePos` | Vector | 基本色 3rd | 転送 |
| `_Main3rdDistanceFade` | Vector | 距離フェード | 転送 |
| `_Main3rdEnableLighting` | Range | ライティング | 転送 |
| `_Main3rdTex` | 2D | 基本色 3rd | オプションONで転送 |
| `_Main3rdTexAlphaMode` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexAngle` | Float | 基本色 3rd | 転送 |
| `_Main3rdTexBlendMode` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexDecalAnimation` | Vector | 基本色 3rd | 転送 |
| `_Main3rdTexDecalSubParam` | Vector | 基本色 3rd | 転送 |
| `_Main3rdTexIsDecal` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexIsLeftOnly` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexIsMSDF` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexIsRightOnly` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexShouldCopy` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexShouldFlipCopy` | Int | 基本色 3rd | 転送 |
| `_Main3rdTexShouldFlipMirror` | Int | 基本色 3rd | 転送 |
| `_Main3rdTex_Cull` | Int | 基本色 3rd | 転送 |
| `_Main3rdTex_ScrollRotate` | Vector | 基本色 3rd | 転送 |
| `_Main3rdTex_UVMode` | Int | 基本色 3rd | 転送 |
| `_MainColorAdjustMask` | 2D | 基本色 | オプションONで転送 |
| `_MainGradationStrength` | Range | 基本色 | 転送 |
| `_MainGradationTex` | 2D | 基本色 | オプションONで転送 |
| `_MainTex` | 2D | 基本色 | オプションONで転送 |
| `_MainTexHSVG` | Vector | 基本色 | 転送 |
| `_MainTex_ScrollRotate` | Vector | 基本色 | 転送 |
| `_MatCap2ndApplyTransparency` | Int | MatCap | 転送 |
| `_MatCap2ndBackfaceMask` | Int | MatCap | 転送 |
| `_MatCap2ndBlend` | Range | MatCap | 転送 |
| `_MatCap2ndBlendMask` | 2D | MatCap | オプションONで転送 |
| `_MatCap2ndBlendMode` | Int | MatCap | 転送 |
| `_MatCap2ndBlendUV1` | Vector | MatCap | 転送 |
| `_MatCap2ndBumpMap` | 2D | MatCap | オプションONで転送 |
| `_MatCap2ndBumpScale` | Range | MatCap | 転送 |
| `_MatCap2ndColor` | Color | MatCap | 転送 |
| `_MatCap2ndCustomNormal` | Int | MatCap | 転送 |
| `_MatCap2ndEnableLighting` | Range | MatCap | 転送 |
| `_MatCap2ndLod` | Range | MatCap | 転送 |
| `_MatCap2ndMainStrength` | Range | MatCap | 転送 |
| `_MatCap2ndNormalStrength` | Range | MatCap | 転送 |
| `_MatCap2ndPerspective` | Int | MatCap | 転送 |
| `_MatCap2ndShadowMask` | Range | 影 | 転送 |
| `_MatCap2ndTex` | 2D | MatCap | オプションONで転送 |
| `_MatCap2ndVRParallaxStrength` | Range | MatCap | 転送 |
| `_MatCap2ndZRotCancel` | Int | MatCap | 転送 |
| `_MatCapApplyTransparency` | Int | MatCap | 転送 |
| `_MatCapBackfaceMask` | Int | MatCap | 転送 |
| `_MatCapBlend` | Range | MatCap | 転送 |
| `_MatCapBlendMask` | 2D | MatCap | オプションONで転送 |
| `_MatCapBlendMode` | Int | MatCap | 転送 |
| `_MatCapBlendUV1` | Vector | MatCap | 転送 |
| `_MatCapBumpMap` | 2D | MatCap | オプションONで転送 |
| `_MatCapBumpScale` | Range | MatCap | 転送 |
| `_MatCapColor` | Color | MatCap | 転送 |
| `_MatCapCustomNormal` | Int | MatCap | 転送 |
| `_MatCapEnableLighting` | Range | MatCap | 転送 |
| `_MatCapLod` | Range | MatCap | 転送 |
| `_MatCapMainStrength` | Range | MatCap | 転送 |
| `_MatCapMul` | Int | MatCap | 転送 |
| `_MatCapNormalStrength` | Range | MatCap | 転送 |
| `_MatCapPerspective` | Int | MatCap | 転送 |
| `_MatCapShadowMask` | Range | 影 | 転送 |
| `_MatCapTex` | 2D | MatCap | オプションONで転送 |
| `_MatCapVRParallaxStrength` | Range | MatCap | 転送 |
| `_MatCapZRotCancel` | Int | MatCap | 転送 |
| `_Metallic` | Range | 反射 | 転送 |
| `_MetallicGlossMap` | 2D | 反射 | オプションONで転送 |
| `_MonochromeLighting` | Range | ライティング | 転送 |
| `_OffsetFactor` | Float | 透過・描画設定 | 転送 |
| `_OffsetUnits` | Float | 透過・描画設定 | 転送 |
| `_OutlineAlphaToMask` | Int | 透過・描画設定 | 転送 |
| `_OutlineBlendOp` | Int | 透過・描画設定 | 転送 |
| `_OutlineBlendOpAlpha` | Int | 透過・描画設定 | 転送 |
| `_OutlineBlendOpAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineBlendOpFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineColor` | Color | 輪郭線 | 転送 |
| `_OutlineColorMask` | Int | 透過・描画設定 | 転送 |
| `_OutlineCull` | Int | 透過・描画設定 | 転送 |
| `_OutlineDeleteMesh` | Int | 輪郭線 | 転送 |
| `_OutlineDisableInVR` | Int | 輪郭線 | 転送 |
| `_OutlineDstBlend` | Int | 透過・描画設定 | 転送 |
| `_OutlineDstBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_OutlineDstBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineDstBlendFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineEnableLighting` | Range | 輪郭線 | 転送 |
| `_OutlineFixWidth` | Range | 輪郭線 | 転送 |
| `_OutlineLitApplyTex` | Int | 輪郭線 | 転送 |
| `_OutlineLitColor` | Color | 輪郭線 | 転送 |
| `_OutlineLitOffset` | Float | 輪郭線 | 転送 |
| `_OutlineLitScale` | Float | 輪郭線 | 転送 |
| `_OutlineLitShadowReceive` | Int | 輪郭線 | 転送 |
| `_OutlineOffsetFactor` | Float | 透過・描画設定 | 転送 |
| `_OutlineOffsetUnits` | Float | 透過・描画設定 | 転送 |
| `_OutlineSrcBlend` | Int | 透過・描画設定 | 転送 |
| `_OutlineSrcBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_OutlineSrcBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineSrcBlendFA` | Int | 透過・描画設定 | 転送 |
| `_OutlineStencilComp` | Float | 透過・描画設定 | 転送 |
| `_OutlineStencilFail` | Float | 透過・描画設定 | 転送 |
| `_OutlineStencilPass` | Float | 透過・描画設定 | 転送 |
| `_OutlineStencilReadMask` | Range | 透過・描画設定 | 転送 |
| `_OutlineStencilRef` | Range | 透過・描画設定 | 転送 |
| `_OutlineStencilWriteMask` | Range | 透過・描画設定 | 転送 |
| `_OutlineStencilZFail` | Float | 透過・描画設定 | 転送 |
| `_OutlineTex` | 2D | 輪郭線 | オプションONで転送 |
| `_OutlineTexHSVG` | Vector | 輪郭線 | 転送 |
| `_OutlineTex_ScrollRotate` | Vector | 輪郭線 | 転送 |
| `_OutlineVectorScale` | Range | 輪郭線 | 転送 |
| `_OutlineVectorTex` | 2D | 輪郭線 | オプションONで転送 |
| `_OutlineVectorUVMode` | Int | 輪郭線 | 転送 |
| `_OutlineVertexR2Width` | Int | 輪郭線 | 転送 |
| `_OutlineWidth` | Range | 輪郭線 | 転送 |
| `_OutlineWidthMask` | 2D | 輪郭線 | オプションONで転送 |
| `_OutlineZBias` | Float | 輪郭線 | 転送 |
| `_OutlineZClip` | Int | 透過・描画設定 | 転送 |
| `_OutlineZTest` | Int | 透過・描画設定 | 転送 |
| `_OutlineZWrite` | Int | 透過・描画設定 | 転送 |
| `_Parallax` | float | 視差 | 転送 |
| `_ParallaxMap` | 2D | 視差 | オプションONで転送 |
| `_ParallaxOffset` | float | 視差 | 転送 |
| `_PreAlphaToMask` | Int | 透過・描画設定 | 転送 |
| `_PreBlendOp` | Int | 透過・描画設定 | 転送 |
| `_PreBlendOpAlpha` | Int | 透過・描画設定 | 転送 |
| `_PreBlendOpAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_PreBlendOpFA` | Int | 透過・描画設定 | 転送 |
| `_PreColor` | Color | 透過・描画設定 | 転送 |
| `_PreColorMask` | Int | 透過・描画設定 | 転送 |
| `_PreCull` | Int | 透過・描画設定 | 転送 |
| `_PreCutoff` | Range | 透過・描画設定 | 転送 |
| `_PreDstBlend` | Int | 透過・描画設定 | 転送 |
| `_PreDstBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_PreDstBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_PreDstBlendFA` | Int | 透過・描画設定 | 転送 |
| `_PreOffsetFactor` | Float | 透過・描画設定 | 転送 |
| `_PreOffsetUnits` | Float | 透過・描画設定 | 転送 |
| `_PreOutType` | Int | 透過・描画設定 | 転送 |
| `_PreSrcBlend` | Int | 透過・描画設定 | 転送 |
| `_PreSrcBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_PreSrcBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_PreSrcBlendFA` | Int | 透過・描画設定 | 転送 |
| `_PreStencilComp` | Float | 透過・描画設定 | 転送 |
| `_PreStencilFail` | Float | 透過・描画設定 | 転送 |
| `_PreStencilPass` | Float | 透過・描画設定 | 転送 |
| `_PreStencilReadMask` | Range | 透過・描画設定 | 転送 |
| `_PreStencilRef` | Range | 透過・描画設定 | 転送 |
| `_PreStencilWriteMask` | Range | 透過・描画設定 | 転送 |
| `_PreStencilZFail` | Float | 透過・描画設定 | 転送 |
| `_PreZClip` | Int | 透過・描画設定 | 転送 |
| `_PreZTest` | Int | 透過・描画設定 | 転送 |
| `_PreZWrite` | Int | 透過・描画設定 | 転送 |
| `_Ramp` | 2D | 影 | オプションONで転送 |
| `_Reflectance` | Range | 反射 | 転送 |
| `_ReflectionApplyTransparency` | Int | 反射 | 転送 |
| `_ReflectionBlendMode` | Int | 反射 | 転送 |
| `_ReflectionColor` | Color | 反射 | 転送 |
| `_ReflectionColorTex` | 2D | 反射 | オプションONで転送 |
| `_ReflectionCubeColor` | Color | 反射 | 転送 |
| `_ReflectionCubeEnableLighting` | Range | 反射 | 転送 |
| `_ReflectionCubeOverride` | Int | 反射 | 転送 |
| `_ReflectionCubeTex` | Cube | 反射 | オプションONで転送 |
| `_ReflectionNormalStrength` | Range | 反射 | 転送 |
| `_RefractionColor` | Color | 屈折 | 転送 |
| `_RefractionColorFromMain` | Int | 屈折 | 転送 |
| `_RefractionFresnelPower` | Range | 屈折 | 転送 |
| `_RefractionStrength` | Range | 屈折 | 転送 |
| `_RimApplyTransparency` | Int | リムライト | 転送 |
| `_RimBackfaceMask` | Int | リムライト | 転送 |
| `_RimBlendMode` | Int | リムライト | 転送 |
| `_RimBlur` | Range | リムライト | 転送 |
| `_RimBorder` | Range | リムライト | 転送 |
| `_RimColor` | Color | リムライト | 転送 |
| `_RimColorTex` | 2D | リムライト | オプションONで転送 |
| `_RimDirRange` | Range | リムライト | 転送 |
| `_RimDirStrength` | Range | リムライト | 転送 |
| `_RimEnableLighting` | Range | リムライト | 転送 |
| `_RimFresnelPower` | Range | リムライト | 転送 |
| `_RimIndirBlur` | Range | リムライト | 転送 |
| `_RimIndirBorder` | Range | リムライト | 転送 |
| `_RimIndirColor` | Color | リムライト | 転送 |
| `_RimIndirRange` | Range | リムライト | 転送 |
| `_RimMainStrength` | Range | リムライト | 転送 |
| `_RimNormalStrength` | Range | リムライト | 転送 |
| `_RimShadeBlur` | Range | 影 | 転送 |
| `_RimShadeBorder` | Range | 影 | 転送 |
| `_RimShadeColor` | Color | 影 | 転送 |
| `_RimShadeFresnelPower` | Range | 影 | 転送 |
| `_RimShadeMask` | 2D | 影 | オプションONで転送 |
| `_RimShadeNormalStrength` | Range | 影 | 転送 |
| `_RimShadowMask` | Range | 影 | 転送 |
| `_RimVRParallaxStrength` | Range | リムライト | 転送 |
| `_Shadow2ndBlur` | Range | 影 | 転送 |
| `_Shadow2ndBorder` | Range | 影 | 転送 |
| `_Shadow2ndColor` | Color | 影 | 転送 |
| `_Shadow2ndColorTex` | 2D | 影 | オプションONで転送 |
| `_Shadow2ndNormalStrength` | Range | 影 | 転送 |
| `_Shadow2ndReceive` | Range | 影 | 転送 |
| `_Shadow3rdBlur` | Range | 影 | 転送 |
| `_Shadow3rdBorder` | Range | 影 | 転送 |
| `_Shadow3rdColor` | Color | 影 | 転送 |
| `_Shadow3rdColorTex` | 2D | 影 | オプションONで転送 |
| `_Shadow3rdNormalStrength` | Range | 影 | 転送 |
| `_Shadow3rdReceive` | Range | 影 | 転送 |
| `_ShadowAOShift` | Vector | 影 | 転送 |
| `_ShadowAOShift2` | Vector | 影 | 転送 |
| `_ShadowBlur` | Range | 影 | 転送 |
| `_ShadowBlurMask` | 2D | 影 | オプションONで転送 |
| `_ShadowBlurMaskLOD` | Range | 影 | 転送 |
| `_ShadowBorder` | Range | 影 | 転送 |
| `_ShadowBorderColor` | Color | 影 | 転送 |
| `_ShadowBorderMask` | 2D | 影 | オプションONで転送 |
| `_ShadowBorderMaskLOD` | Range | 影 | 転送 |
| `_ShadowBorderRange` | Range | 影 | 転送 |
| `_ShadowColor` | Color | 影 | 転送 |
| `_ShadowColorTex` | 2D | 影 | オプションONで転送 |
| `_ShadowColorType` | Int | 影 | 転送 |
| `_ShadowEnvStrength` | Range | 影 | 転送 |
| `_ShadowFlatBlur` | Range | 影 | 転送 |
| `_ShadowFlatBorder` | Range | 影 | 転送 |
| `_ShadowMainStrength` | Range | 影 | 転送 |
| `_ShadowMaskType` | Int | 影 | 転送 |
| `_ShadowNormalStrength` | Range | 影 | 転送 |
| `_ShadowPostAO` | Int | 影 | 転送 |
| `_ShadowReceive` | Range | 影 | 転送 |
| `_ShadowStrength` | Range | 影 | 転送 |
| `_ShadowStrengthMask` | 2D | 影 | オプションONで転送 |
| `_ShadowStrengthMaskLOD` | Range | 影 | 転送 |
| `_ShiftBackfaceUV` | Int | 基本色 | 転送 |
| `_Smoothness` | Range | 反射 | 転送 |
| `_SmoothnessTex` | 2D | 反射 | オプションONで転送 |
| `_SpecularBlur` | Range | 反射 | 転送 |
| `_SpecularBorder` | Range | 反射 | 転送 |
| `_SpecularNormalStrength` | Range | 反射 | 転送 |
| `_SpecularToon` | Int | 反射 | 転送 |
| `_SrcBlend` | Int | 透過・描画設定 | 転送 |
| `_SrcBlendAlpha` | Int | 透過・描画設定 | 転送 |
| `_SrcBlendAlphaFA` | Int | 透過・描画設定 | 転送 |
| `_SrcBlendFA` | Int | 透過・描画設定 | 転送 |
| `_StencilComp` | Float | 透過・描画設定 | 転送 |
| `_StencilFail` | Float | 透過・描画設定 | 転送 |
| `_StencilPass` | Float | 透過・描画設定 | 転送 |
| `_StencilReadMask` | Range | 透過・描画設定 | 転送 |
| `_StencilRef` | Range | 透過・描画設定 | 転送 |
| `_StencilWriteMask` | Range | 透過・描画設定 | 転送 |
| `_StencilZFail` | Float | 透過・描画設定 | 転送 |
| `_SubpassCutoff` | Range | 透過・描画設定 | 転送 |
| `_TessEdge` | Range | テッセレーション | 転送 |
| `_TessFactorMax` | Range | テッセレーション | 転送 |
| `_TessShrink` | Range | テッセレーション | 転送 |
| `_TessStrength` | Range | テッセレーション | 転送 |
| `_TransparentMode` | Int | 透過・描画設定 | シェーダー・描画モードの変更が必要な設定は対象外 |
| `_TriMask` | 2D | リムライト | オプションONで転送 |
| `_UDIMDiscardCompile` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardMode` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow0_0` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow0_1` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow0_2` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow0_3` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow1_0` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow1_1` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow1_2` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow1_3` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow2_0` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow2_1` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow2_2` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow2_3` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow3_0` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow3_1` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow3_2` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardRow3_3` | Int | UDIM破棄 | 転送 |
| `_UDIMDiscardUV` | Int | UDIM破棄 | 転送 |
| `_UseAnisotropy` | Int | 異方性反射 | 転送 |
| `_UseAudioLink` | Int | AudioLink | 転送 |
| `_UseBacklight` | Int | ライティング | 転送 |
| `_UseBump2ndMap` | Int | ノーマル | 転送 |
| `_UseBumpMap` | Int | ノーマル | 転送 |
| `_UseClippingCanceller` | Int | 透過・描画設定 | シェーダー・描画モードの変更が必要な設定は対象外 |
| `_UseDither` | Int | ディザ | 転送 |
| `_UseEmission` | Int | 発光 | 転送 |
| `_UseEmission2nd` | Int | 発光 | 転送 |
| `_UseGlitter` | Int | ラメ | 転送 |
| `_UseMain2ndTex` | Int | 基本色 2nd | 転送 |
| `_UseMain3rdTex` | Int | 基本色 3rd | 転送 |
| `_UseMatCap` | Int | MatCap | 転送 |
| `_UseMatCap2nd` | Int | MatCap | 転送 |
| `_UseOutline` | Int | 輪郭線 | シェーダー・描画モードの変更が必要な設定は対象外 |
| `_UsePOM` | Int | 視差 | 転送 |
| `_UseParallax` | Int | 視差 | 転送 |
| `_UseReflection` | Int | 反射 | 転送 |
| `_UseRim` | Int | リムライト | 転送 |
| `_UseRimShade` | Int | 影 | 転送 |
| `_UseShadow` | Int | 影 | 転送 |
| `_VertexColor2FurVector` | Int | ファー | 転送 |
| `_VertexLightStrength` | Range | ライティング | 転送 |
| `_ZClip` | Int | 透過・描画設定 | 転送 |
| `_ZTest` | Int | 透過・描画設定 | 転送 |
| `_ZWrite` | Int | 透過・描画設定 | 転送 |
| `_e2ga0` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga1` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga2` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga3` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga4` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga5` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga6` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2ga7` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gai` | Int | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc0` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc1` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc2` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc3` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc4` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc5` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc6` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gc7` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_e2gci` | Int | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega0` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega1` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega2` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega3` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega4` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega5` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega6` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_ega7` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egai` | Int | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc0` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc1` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc2` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc3` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc4` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc5` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc6` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egc7` | Color | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_egci` | Int | 発光 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |
| `_lilDirectionalLightStrength` | Range | ライティング | 転送 |
| `_lilShadowCasterBias` | Float | 影 | 転送 |
| `_lilToonVersion` | Int | 内部・派生値 | 内部管理値（グラデーション編集情報・バージョン等）は対象外 |

## シェーダー許可リスト

内部名と型が共通のプロパティーを処理する。実際の描画とバリアントごとの差異はUnity上で確認する。

- `Hidden/lilToonCutout`
- `Hidden/lilToonCutoutOutline`
- `Hidden/lilToonFur`
- `Hidden/lilToonFurCutout`
- `Hidden/lilToonFurTwoPass`
- `Hidden/lilToonGem`
- `Hidden/lilToonLite`
- `Hidden/lilToonLiteCutout`
- `Hidden/lilToonLiteCutoutOutline`
- `Hidden/lilToonLiteOnePassTransparent`
- `Hidden/lilToonLiteOnePassTransparentOutline`
- `Hidden/lilToonLiteOutline`
- `Hidden/lilToonLiteTransparent`
- `Hidden/lilToonLiteTransparentOutline`
- `Hidden/lilToonLiteTwoPassTransparent`
- `Hidden/lilToonLiteTwoPassTransparentOutline`
- `Hidden/lilToonMultiFur`
- `Hidden/lilToonMultiGem`
- `Hidden/lilToonMultiOutline`
- `Hidden/lilToonMultiRefraction`
- `Hidden/lilToonOnePassTransparent`
- `Hidden/lilToonOnePassTransparentOutline`
- `Hidden/lilToonOutline`
- `Hidden/lilToonRefraction`
- `Hidden/lilToonRefractionBlur`
- `Hidden/lilToonTessellation`
- `Hidden/lilToonTessellationCutout`
- `Hidden/lilToonTessellationCutoutOutline`
- `Hidden/lilToonTessellationOnePassTransparent`
- `Hidden/lilToonTessellationOnePassTransparentOutline`
- `Hidden/lilToonTessellationOutline`
- `Hidden/lilToonTessellationTransparent`
- `Hidden/lilToonTessellationTransparentOutline`
- `Hidden/lilToonTessellationTwoPassTransparent`
- `Hidden/lilToonTessellationTwoPassTransparentOutline`
- `Hidden/lilToonTransparent`
- `Hidden/lilToonTransparentOutline`
- `Hidden/lilToonTwoPassTransparent`
- `Hidden/lilToonTwoPassTransparentOutline`
- `_lil/[Optional] lilToonFakeShadow`
- `_lil/[Optional] lilToonFurOnlyCutout`
- `_lil/[Optional] lilToonFurOnlyTransparent`
- `_lil/[Optional] lilToonFurOnlyTwoPass`
- `_lil/[Optional] lilToonLiteOverlay`
- `_lil/[Optional] lilToonLiteOverlayOnePass`
- `_lil/[Optional] lilToonOutlineOnly`
- `_lil/[Optional] lilToonOutlineOnlyCutout`
- `_lil/[Optional] lilToonOutlineOnlyTransparent`
- `_lil/[Optional] lilToonOverlay`
- `_lil/[Optional] lilToonOverlayOnePass`
- `_lil/lilToonMulti`
- `lilToon`
