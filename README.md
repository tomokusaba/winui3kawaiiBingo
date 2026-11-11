# かわいいビンゴ

うさぎと一緒に楽しめる、WinUI 3 のビンゴ抽選アプリです。

## あそびかた

- 「くじを ひく！」で 1〜75 の番号を重複なく抽選します。出た番号はボードに色が付き、最新の番号には枠が付きます。
- 「ひとつ もどす」で直前の抽選を取り消せます。「はじめから」は確認後に履歴をリセットします。
- 抽選履歴は自動保存され、次回も続きから遊べます。75 個すべて出ると抽選は終了します。

保存先は `%LOCALAPPDATA%\KawaiiBingo\game.json` です。保存データが壊れて読み込めない場合はエラーを表示し、履歴を勝手に上書きしません。復旧する場合はアプリを閉じ、ファイルをバックアップしてから移動してください。

## 実行

Windows、.NET 11 SDK、Windows App Runtime 2.6.5 が必要です。既定ではアンパッケージ版として実行します。

```powershell
dotnet run --project src\KawaiiBingo\KawaiiBingo.csproj --no-launch-profile
```

テストは次のコマンドで実行します。

```powershell
dotnet test tests\KawaiiBingo.Tests\KawaiiBingo.Tests.csproj
```

## 構成

起動時に Generic Host で依存関係を登録します。依存は内側に向け、画面やファイル保存の方法に抽選ルールが依存しないようにしています。

| プロジェクト | 役割 |
|---|---|
| `KawaiiBingo.Domain` | 抽選の規則と状態 |
| `KawaiiBingo.UseCases` | 抽選・取り消し・リセットと保存の調整 |
| `KawaiiBingo.Infrastructure` | 乱数の生成と JSON ファイルへの保存 |
| `KawaiiBingo` | WinUI 3 の画面と Generic Host |

## フォント

画面には [Mochiy Pop One](https://fonts.google.com/specimen/Mochiy+Pop+One) と [Fredoka](https://fonts.google.com/specimen/Fredoka) を使用しています。どちらも SIL Open Font License で、ライセンス文は `src\KawaiiBingo\Assets\Fonts` に含まれています。
