# PR 作成

`pr-creator` サブエージェントを起動し、規定フォーマットで Pull Request を作成します。

## 使い方

このコマンドを呼び出したら、以下を行ってください:

1. `pr-creator` エージェントを Agent ツールで起動する（`subagent_type: "pr-creator"`）
2. ユーザーが追加で渡した引数（例: PR の補足説明、Draft 指定、対象 base ブランチ）をエージェントに伝える

## エージェントが必ず実行する手順

`pr-creator` は `~/.claude/rules/git-workflow.md` に従い、以下のフローを実行します:

1. **事前チェック**
   - 現在ブランチが main / master でないか確認
   - `gh auth status` で認証確認
   - `git status` / `git diff HEAD` / 未追跡ファイル一覧を取得

2. **未コミット変更のレビュー**（変更がある場合のみ）
   - コード品質チェック（関数長 / ファイル長 / ネスト深さ / エラーハンドリング / console.log / 絵文字 / ミューテーション）
   - セキュリティチェック（シークレット / SQL インジェクション / XSS / コマンドインジェクション / 入力検証 / .env 混入）
   - CRITICAL / HIGH 検出時は PR 作成を中断
   - 問題なし or MEDIUM/LOW のみの場合、ユーザーにコミット可否を確認

3. **コミット履歴の解析**
   - base ブランチを特定（既定: `origin/HEAD`）
   - `git log [base]..HEAD` で全コミットを取得
   - `git diff [base]...HEAD` で全差分を確認

4. **リモート同期**
   - upstream 未設定の場合は `git push -u origin HEAD`
   - `--force` 系フラグは使用しない

5. **PR 作成**
   - タイトル: Conventional Commits 形式（`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`, `perf:`, `ci:`）
   - 本文セクション: 概要 / 変更内容 / 関連コミット / テスト計画 / セキュリティチェック / 補足
   - Attribution は付与しない
   - HEREDOC で `gh pr create` に渡す

6. **完了報告**
   - PR URL を提示
   - 残存する MEDIUM / LOW 問題があれば通知

## 中断条件

以下の場合、PR は作成されません:

- 現在ブランチが main / master
- `gh` 未認証
- セキュリティレビューで CRITICAL / HIGH を検出
- コミットが 0 件
- 同一ブランチに既存 PR がある（既存 URL を提示）

## オプション引数

ユーザーが指定可能な追加情報:

- `draft`: Draft PR として作成
- `base=<branch>`: base ブランチを明示的に指定
- 自由記述: PR 本文の「補足」セクションに反映

## 例

```
/create-pr
/create-pr draft
/create-pr base=develop
/create-pr このリリースは Feature Flag X が有効な環境にのみ影響します
```
