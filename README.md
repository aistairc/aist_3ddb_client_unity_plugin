# AIST 3DDB Client for Unity Plugin

AIST 3DDB Client for Unity Plugin は、[国立研究開発法人 産業技術総合研究所](https://www.aist.go.jp/) (以後、産総研と呼称します) で開発している、屋内外の 3 次元メッシュデータ、3 次元点群データ、3 次元構造データ (以後、3D データと呼称します) を管理するシステム (3DDB システム) の API に、[Unity](https://unity.com/) からアクセスし 3D データを表示するためのプログラムです。

基本的な機能は、先に公開している [AIST 3DDB Client](https://github.com/aistairc/aist_3ddb_client) と同等の機能を備えています。

![Operation screen](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/main.png)

## 動作環境

以下の環境で動作確認を実施しています。

| | |
|--|--|
|CPU| 12th Gen Intel(R) Core(TM) i7-12650H |
|GPU| NVIDIA(R) GeForce RTX(TM) 4070 Laptop GPU |
|RAM| DDR5 64GB |
|OS| Windows 11 Enterprise 23H2, Windows Feature Experience Pack 1000.22700.1047.0 |
|Unity| 2022.3.41f1 |

上記以外の環境での動作確認は未実施です。

## 事前準備

本プログラムの動作に際し、[Cesium](https://www.cesium.com/) アカウント、Unity アカウント (Unity ID)・Unity のインストールが必要となります。

これらの準備の方法を、以下に記します。

### Cesium アカウント

本プログラムは、3D データを表示と併せて表示するベースマップに Cesium が配信する地図を利用しているため、Cesium アカウントが必要となります。

Cesium アカウントをお持ちでない場合、[このページ](https://www.cesium.com/) の右上の「SIGN UP FREE」からアカウントを作成します。

### Unity ID・Unity のインストール

1. Unity ID の準備

   本プログラムは、Unity を利用するため Unity ID が必要となります。

   Unity ID をお持ちでない場合、[このページ](https://unity.com/) の右上右端のアイコンをクリックし、「Create a Unity ID」を選択し Unity ID を作成します。

1. Unity Hub のダウンロード・インストール

   Unity ID の準備ができたら、[ここ](https://unity.com/ja/download) から Unity Hub をダウンロードしインストールします。
   Unity Hub は、Unity (Unity Editor) のインストールから起動までを、まとめて管理するツールです。

1. Unity のインストール

   Unity Hub へ準備した Unity ID でサインインし、Unity をインストールします。

   開発時に使用した Unity のバージョンは `Unity 2022.3.16f1` です。
   当該バージョンがインストールできない場合は、近いバージョンをインストールしてください。

   インストールは、[このページ](https://unity.com/releases/editor/archive) を開き `Unity 2022.X` から `Unity 2022.3.16f1` を選択し「Unity Hub」ボタンをクリックします。
   これで Unity Hub が起動し Unity のインストールが実行されます。

## 利用方法

本プログラムの利用方法を、以下に記します。

### 新規プロジェクトの作成

Unity Hub を起動して、新規にプロジェクトを作成してください。

使用するテンプレートは [Unity Universal Render Pipeline (URP)](https://learning.unity3d.jp/tag/urp/) の空のプロジェクトです。
Unity Hub では `Universal 3D` のテンプレートが該当します。

### 必要パッケージのインポート

####  Newtonsoft.Json パッケージ

本プラグインで利用しています。

- Unity Editorのメニューから、Window > Package Manager を起動します。

- Package Managerウィンドウの上部にある「＋」ボタンをクリックして、「Add package from git URL...」を選択します。

  ![Add package from git URL...](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_pkg_01.png)

- com.unity.nuget.newtonsoft-json と入力して「Add」ボタンをクリックして当該パッケージを追加します。

  ![Add pakcage](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_pkg_02.png)

#### Splines パッケージ

Cesium for Unity 1.13.0 の動作に必要となるパッケージですが、依存関係の設定が行われていないため手動でのインポートが必要となります。

- Unity Editor のメニューから、Window > Package Manager を起動します。

- Package Manager ウィンドウの上部にある Packages ドロップダウンメニュー (「Packages: ＊＊＊＊＊」と表示されている) をクリックし、「Unity Registry」を選択を選択します。

- 左ペインから「Splines」を選択し Install ボタンをクリックします。

  ![Install package](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_pkg_03.png)

####  Cesium for Unity のインポート

- Project Settings > Package Manager を開きます。 (**これまでの操作にあった Package Manager とは別物ですので注意してください。**)

  ![Setting project](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/setting_project.png)

- スコープ レジストリに以下の内容を入力して追加する。

  ```
  Name: Cesium
  URL: https://unity.pkg.cesium.com
  Scope(s): com.cesium.unity
  ```

  ![Import cesium for unity](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_cfu_02.png)

- Unity Editor のメニューから、Window > Package Manager を起動します。

- Packages ドロップダウンメニューを開いて、「My Registries」を選択します。

- パッケージリストに「Cesium for Unity」が表示されるので、「Install」ボタンをクリックします。

  ![Install cesium for unity](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_cfu_03.png)

**開発時に使用した Cesium for Unity のバージョンは `1.13.0` です。**

### Cesium ion への接続

1. Unity Editor のメニューから Cesium > Cesium を実行して Cesium ウィンドウを表示します。
1. Connect to Cesium ion ボタンをクリックします。
1. ポップアップ ブラウザ ウィンドウが開きます。サインインしていない場合は、Cesium ion にサインインしてください。Epic Games、Github、または Google アカウントでサインインすることも可能です。
1. サインイン後に、Cesium for Unity がアセットにアクセスするのを許可するかどうかを尋ねるプロンプトが表示されます。[Allow] (許可)ボタンをクリックし、Unity Editor に戻ります。
1. プロジェクトのデフォルトのアクセストークンを作成します。ここでは、すべてのアセットで使用されるプロジェクト全体のアクセストークンを設定してください。
1. トークンを設定するための新しいウィンドウが表示されます。

   利用可能なトークンが存在する場合、[Use an existing token] から選択します。
   トークンが存在しない場合、以下の手順でトークンを作成し、作成したトークンを選択します。

   - [Create a new token] オプションを選択し、必要に応じてトークンの名前を変更します。
   - [Create New Project Default Token] ボタンを押します。

**プロジェクトを開くと Cesium へのサインインとトークンの設定が求められます。**
**詳しくは、[このページ](https://cesium.com/learn/unity/unity-quickstart/) の `Step 2: Connect to Cesium ion` を参照してください。**

### AIST 3DDB Client for Unity Plugin パッケージのインポート

1. Unity Editor のメニューから、Window > Package Manager を起動します。

1. Package Manager ウィンドウの上部にある「＋」ボタンをクリックして、「Add package from git URL...」を選択します。

1. `https://github.com/aistairc/aist_3ddb_client_unity_plugin.git` と入力して「Add」ボタンをクリックしインポートします。

   ![Import 3ddb client](https://github.com/aistairc/aist_3ddb_client_unity_plugin/blob/main/screenshot/import_this.png)

### サンプルシーンのインポート

- Unity Editor のメニューから、Package Manager を起動し AIST-3DDB-Client-Unity-Plugin を検索します。
- 右ペインから Sample をクリックします。
- サンプルシーンが表示されるので「Import」ボタンをクリックします。

サンプルシーンでは、以下のフォントセットを使用しています。

[フォントセット名: Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP)

[ライセンス: SIL Open Font License バージョン 1.1](https://fonts.google.com/noto/specimen/Noto+Sans+JP/license)

## 操作方法

- マウス左ボタン：平行移動
- マウス右ボタン：高度調整（上下の動きのみ）
- マウス中ボタン押し込み：回転（左右の動きのみ）
- 検索欄：検索キーワードを入力
- データの種別：

  - 検索（表示領域内）ボタン：画面の四隅の範囲内にある3Dデータのリストを調べます。このとき、画面の四隅が地面を表示するように調整してください。

- 検索結果一覧内のボタン

  - 「i」ボタン：データに関する著作者や情報源などの情報ダイアログを表示
  - 「+」ボタン：お気に入りに追加して3Dデータの読み込む

- お気に入り3Dデータ一覧内のボタン

  - 「-」ボタン：一覧から削除

## 利用データー

本プログラムは、以下のデータを利用しています。

- 3DDB API

  3DDB API の利用における免責事項および、利用条件は AIST 3DDB Client に準拠します。
  詳細は [こちら](https://github.com/aistairc/aist_3ddb_client#3ddb-api) を参照してください。

- 国土地理院
  - 地理院タイル
    - 写真（https://cyberjapandata.gsi.go.jp/xyz/seamlessphoto/{z}/{x}/{y}.jpg）
      - 標高（https://cyberjapandata.gsi.go.jp/xyz/dem_png/{z}/{x}/{y}.png)
      - 参考: [地理院地図｜地理院タイル一覧](https://maps.gsi.go.jp/development/ichiran.html)
  - ジオイド高
    - public/proj/jp_gsi_gsigeo2011.tif
    - 参考: public/proj/jp_gsi_README.txt, [ジオイド・モデルの提供｜基盤地図情報ダウンロードサービス](https://fgd.gsi.go.jp/download/geoid.php)

## 実装

本プログラムは、全体を C# で実装しています。
地図およびレイヤー (地物) の表示処理は Cesium for Unity を用いています。

### 各コンポーネントについて

#### Runtime/Scripts/CameraController

このフォルダーにあるプログラムは、カメラ制御関連のスクリプトをまとめています。

- AistCameraController.cs

  マウスによるカメラ操作用スクリプトです。

- ViewportCoordinatesFetcher.cs

  Cesium API を用いて、画面の四隅の位置情報を取得します。

#### Runtime/Scripts/Services

このフォルダーにあるプログラムは 3DDB API が扱うデーターセット (service_name) の一覧を取得するプログラムです。

適当な GameObject に ServiceController.cs を追加して、「Service Dropdown」に UGUI のDropdown-TextMeshPro を設定すると、シーン開始時に AIST 3DDB にアクセスしてデーターセット一覧を取得して設定します。

- Service.cs

  コルチーンでサービス一覧を取得します。

- ServiceController.cs

  サービス一覧を取得して Dropdown に反映します。
  Unity の Component として動作します。

- ServiceModel.cs

  サービス一覧の JSON 解析を実施します。

- ServiceRepository.cs

  3DDB API を利用してサービス一覧を取得します。

- SurfaceFeaturesFetcher.cs

  3DDB API を利用してデータ検索を実施します。

- CoroutineRunner.cs

  常駐しているコルーチン制御クラスです。

#### Runtime/Scripts/Feature

このフォルダーにあるプログラムは 3DDB API が扱う地物データを検索します。

- SurfaceFeaturesFetcher.cs

  3DDB APIにて、地物データを検索します。

- FeatureModel.cs

  3DDB API の地物データのレスポンスを処理します。

#### Runtime/Scripts/Scroll_Item

このフォルダーにあるプログラムは 3DDB API が扱う地物データを検索した結果を表示するために使用しているスクロールビューの制御に関するクラスです。

- SearchItem.cs

  検索結果の各種制御用スクリプトです。

- FavItem.cs

  検索結果から「＋」ボタンでお気に入りに追加された 3D データーのアイテムの制御用スクリプトです。

#### Runtime/Scripts/Balloon

このフォルダーにあるプログラムは 3DDB API が扱う地物データを検索一覧に置いて、それぞれのデータの追加情報を表示するためのポップアップダイアログ (バルーン) の表示関するクラスです。

- BalloonManager.cs

  バルーンの制御をするシングルトンクラスです。

- Balloon.cs

  バルーンに表示データを設定するクラスです。

## ライセンス

- 本ソフトウェアは、MITライセンスのもとで提供されるオープンソースソフトウエアです。
- ソースコードおよび関連ドキュメントの著作権は産業技術総合研究所に帰属します。
- 本ソフトウェアの開発は [株式会社シーディングソフテック](https://seedingsoftech.jp/) が行っています。

## 免責事項

- 本リポジトリおよびソフトウェアは動作の保証は行っておりません。
- 本リポジトリおよび本ソフトウェアの利用により生じた損失及び損害等について、開発者および産業技術総合研究所はいかなる責任も負わないものとします。
- 本リポジトリの内容は予告なく変更・削除する場合があります。

## 履歴

- 2024 年 12 月: 初期リリース
