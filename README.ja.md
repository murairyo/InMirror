# InMirror 日本語版

English version is available in [README.md](README.md).

このリポジトリは Unity で作成されたシンプルな AR/VR ポータルデモを含んでいます。ポータル効果を通じて拡張現実と仮想現実の世界を切り替える方法を示します。

## プロジェクトを開く手順
1. **Unity 6000.0.36f1** 以降をインストールします。
2. このリポジトリをクローンし、Unity Hub を起動します。
3. Unity Hub で **Open** を選択し、クローンしたディレクトリを指定して開きます。初回起動時に必要なパッケージがインポートされます。

## 必要なパッケージ
必須パッケージは [`Packages/manifest.json`](Packages/manifest.json) で定義されています。主なものは以下の通りです。
- `com.meta.xr.sdk.all` (Meta XR Integration)
- `com.unity.xr.management`
- `com.unity.xr.oculus`
- `com.unity.inputsystem`
- `com.unity.visualscripting`

Unity から依存関係について確認を求められたら、これらのパッケージがインストールされていることを確認してください。

## ビルドと実行
1. Unity の **Build Settings** (File > Build Settings...) を開きます。
2. ビルドに含めるシーンとして `Main.unity` などを追加します。
3. 対象プラットフォームを選択します (Oculus デバイスの場合は **Android** を選択)。
4. **Build** もしくは **Build and Run** を実行します。

サンプルは Oculus デバイスでテストされていますが、Unity エディタ上でも動作します。

## 主なシーンとスクリプト
`Assets/Scenes` フォルダーには次の 3 つのシーンがあります。
- `Main.unity`
- `BuildTest.unity`
- `MRTK_scene.unity`

ポータルの動作に関わる主要なスクリプトは `Assets/Script` フォルダーにあります。
- `PortalManager.cs` – ポータルの出入りと AR/VR の切り替えを管理します。
- `ClippingPlane_Origin.cs` – ポータル通過時にオブジェクトを隠す／表示するためのクリッピングプレーンを制御します。

以上を参考に、ポータル処理をカスタマイズしてみてください。
