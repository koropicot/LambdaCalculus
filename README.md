これは何？
---------
型なしラムダ計算の単純な処理系です。
Visual Studioのソリューションになってます。
ラムダ式をパースして最外最左(たぶん)簡約します。
速度は遅いし、スタックを食います。

----
読み取れる構文
--------------
パースする構文はだいたい以下の様な感じ(正確には実装(LambdaParserクラス)参照)。
####ラムダ式
    変数 / ラムダ抽象 / 関数適用 / let式
####変数
    [a-zA-Z_] [0-9a-zA-Z_]*
英数とアンダースコアの文字列(先頭文字は数字以外)
####ラムダ抽象
    "λ" 変数* "." ラムダ式
"λ"は"^"でも大丈夫。
 ("\"がいい!って人はOr追加するなりして適当にパーサ変更してください。
  "lambda"ガーって人は識別子が被らないようにKeyWordにも追加しておくとたぶん行けます。)
関数のbodyに当たるラムダ式は取れるだけ長くとります。
仮引数に当たる変数は複数並べることができます(間に空白を挟んで分離)。
例えば

    λx y.M
は

    λx.(λy.M)
と認識されます(カリー化)。
####関数適用
    ラムダ式+
ラムダ式の並びは関数適用です。
    
    L M N
のように3個以上並べることもでき
	
    (L M) N
と認識されます。
####let式
    "let" 変数 "=" ラムダ式 "in" ラムダ式
変数束縛を表します。
let x=M in N は (λx.N) M の糖衣構文です。
####数値リテラル
数値はその数値に対応するチャーチ数になります。

---

パーサについて
--------------
PEG(Parsing Expression Grammar)的に書けるパーサコンビネータです。
モナドの糖衣構文であるクエリ式を活用して気持ち悪くパーサが書けます。
入力がcharのリストであることすら要求しないので文字列以外でもパースできますが遅いので実用性はありません。
Dataプロジェクトの色々に依存してます。