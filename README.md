# Elevator LCD Simulator

日立製の乗用エレベーター、アーバンエース(第6世代,第7世代)の液晶画面をWPFを用いて再現するプロジェクトです。

## 重要 Twitter APIについて

Twitter APIが今後有料化されるため、場合によってはAPI連携機能を**廃止**する可能性があります。


## アーバンエースってなんぞや
これ↓
![アーバンエース(第6世代,第7世代)](https://github.com/Lausiv1024/H-ElevatorLCDEmulator/blob/master/doc/urban_Ace.png)

## 対応環境
Windows8以降

.Net Framework 4.7.2

## 注意事項
本ソフトによって発生した損害などにつきましては開発者は責任を負いかねます。

また、TwitterAPIについて開発者に質問しないでください。

## 必須フォント(多分プリインストール済み)
- BIZ UDゴシック
- BIZ UDPゴシック
- Segoe UI


## 使い方
### 導入
まず、[Releases](https://github.com/Lausiv1024/H-ElevatorLCDSimulator/releases)にアクセスし最新版のH-ElevatorLCDSimulator.zipをダウンロードします。

その後ダウンロードしたzipを解凍してください
### 実行について
このアプリは署名されていないので初回実行時「WindowsによってPCが保護されました」と警告が出ることがあります。

詳細を押せば「実行する」ボタンが出てくるのでそれを押してください。

### 起動

起動するとまず、設定画面が表示されます。

### 設定
#### 階層設定
![階層設定UI](https://github.com/Lausiv1024/H-ElevatorLCDEmulator/blob/master/doc/SettingUI_0.png)

まず階の設定を行う必要があります。

追加欄にある連番に任意の数字を入力して追加を押すと連番で入力されます。

連番の下の欄に任意の文字を入力して追加を押すとその名前の階が追加されます。

仕様上リストの最初が最下階という扱いになるため注意が必要です。

階を右クリックして消去を押すことで不要な階を削除できます。

▲/▼ボタンで選択した階を上下に入れ替えすることができます。

開始位置をクリックすると上から開始するか下から開始するか選択できます。

往復運転にチェックを入れない限り往復はしませんのでご注意ください。

無限ループにチェックを入れると途中でエスケープキーを押されない限り無限ループします。

インポートで設定をjsonからインポート、エクスポートでjsonファイルへ設定をエクスポートします。

#### コンテンツ設定
![コンテンツ設定UI](https://github.com/Lausiv1024/H-ElevatorLCDEmulator/blob/master/doc/SettingUI_0_0.png)

リストの下の「+」ボタンを押すとコンテキストメニューが表示され追加したいコンテンツをクリックすることで追加できます。

使用できるコンテンツは以下の通りです

<details><summary>カスタムニュース</summary>



独自のニュースを表示できます。

![実際のニュース表示](https://github.com/Lausiv1024/H-ElevatorLCDEmulator/blob/master/doc/News.png)

本文は改行が可能です。

提供元は下に表示されます。
</details>

<details><summary>メディア</summary>


基本的にWebViewで表示できるものは何でも表示できます。

例：ページやメディア
</details>

<details><summary>Youtube動画埋め込み</summary>


http://youtube.com/watch?v=動画ID
の動画IDを入力します

一部再生できない動画がありますがご了承ください
</details>

<details><summary>TwitterのTLからランダムでツイートを取得</summary>


API認証が**必須**です。

タイムライン上からランダムでツイートを取得します。

メディア付きツイートやリツイート、文字数が多いツイートは取得しません。
</details>

<details><summary>設定したユーザーのツイートを取得</summary>


API認証が**必須**です

ユーザーIDの欄にユーザーのIDを入力します。

設定したユーザーのツイートをランダムで取得します。

メディア付きツイートやリツイート、文字数が多いツイートは取得しません。
</details>


#### TwitterAPI認証設定
設定する前にTwitterAPI v1.1の使用をするための申請を行い以下のKeyを取得してください。

- Consumer Key
- Consumer Secret
- AccessToken
- AccessToken Secret

ツイート取得機能を使うためには**必ず設定する必要があります**。

入力が完了した後は**必ず**下にある検証ボタンを押してください。

正しいKeyが入力されていれば検証が完了した旨を伝えるダイアログが表示され検証が完了します。

失敗した場合エラーダイアログが表示されるのでKeyが間違っていないか、APIの申請が通っているか確認してください。

TwitterAPIに関することで**私に質問しないでください**。~~大事なことなので2回言いました~~

### 再現実行
下の開始ボタンで開始できます。

開始遅延に秒数を入力することで、動作開始までの時間を調整できます。

無限ループを有効化した場合エスケープキーを押すことで次の到着時に設定画面に戻ります。

## 音声ファイルについて
音声ファイルは権利上の問題でリポジトリに入れることができないため自前で用意する必要があります。

音声ファイルはH-ElevatorLCDEmulator.exeのあるフォルダに初回実行時に作成されるresourcesフォルダに入れます。

使用できるファイル形式はMP3のみです。

到着階アナウンスは、階の名前と同じ名前で保存します。

また、他のアナウンスは以下の通り対応します。

|アナウンス|対応ファイル|
|----------|------------|
|上へ参ります|Up.mp3|
|下へ参ります|Down.mp3|
|ドアが開きます|Open.mp3|
|ドアが閉まります|Close.mp3|

ファイルの名前が間違っている場合、再生されませんので確認してください。

## Used Library
- [Microsoft.Web.WebView2](https://aka.ms/webview)
- [NAudio](https://github.com/naudio/NAudio)
- [Tweetinvi](https://github.com/linvi/tweetinvi)

## 今後実装したい機能

- 途中停止階の設定機能
- Youtube埋め込みの再生時間の設定
- コンテンツ設定のプリセット保存