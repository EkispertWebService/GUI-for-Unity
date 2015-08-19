using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Nandoku : MonoBehaviour
{
    // 駅すぱあとWebサービスのオブジェクト
    public GameObject ews_go;
    private EkispertWebService ews;
    private bool load = false;
    private bool gameInit = false;
    private bool gameOver = false;

    // メッセージ関連のオブジェクト
    public GameObject StartButton;
    public GameObject EndMessage;
    public Text EndMessageText;
    public GameObject CorrectMessage;
    private float CorrectMessageTimer;
    public GameObject NowLoadingMessage;

    // 入力した文字列の取得とリスト出力のサンプル
    public Text ewsInput;

    // 表示するテキストデータ
    public Text stationNameText;
    public Text timerText;
    public Text scoreText;

    // 内部の処理用変数
    private float timer;
    private string selectRailName;
    private int stationCount;
    private int stationMatchCount;
    private int stationPos;
    private string stationYomi;
    private string stationInputYomi;

    // 初期の時間設定
    public float initTimeLimit = 20.0F;

    // 駅すぱあとから取得した駅リスト
    private string[] stationList;

    // 指定する路線名を設定
    public string[] railName;

    // Use this for initialization
    void Start()
    {
        // 駅すぱあとのオブジェクト
        ews = ews_go.GetComponent<EkispertWebService>();
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームが始まっているかを判定
        if (!gameOver && ews.IsSuccess && gameInit)
        {
            // 初期化が終わっていない場合は変数を設定
            if (!load)
            {
                // 各種変数を初期化
                stationList = ews.ResultList;
                timer = initTimeLimit;
                stationCount = stationList.Length;
                stationMatchCount = 0;
                stationPos = 1;
                stationMatchCount = 0;
                CorrectMessageTimer = 0.0F;
                // 駅名をセット
                setStationName();
                // 入力値を初期化
                ewsInput.text = "";
                // 読み込み中メッセージを削除
                NowLoadingMessage.SetActive(false);
                // 初期化完了
                load = true;
            }
            else if (load)
            {
                //正解チェック
                if (stationInputYomi == ewsInput.text)
                {
                    // 次の問題に行く
                    stationMatchCount++;
                    stationPos++;
                    if (stationPos <= stationList.Length)
                    {
                        // 駅が残っていたら次の問題
                        setStationName();
                        ewsInput.text = "";
                        // 正解表示
                        CorrectMessage.SetActive(true);
                        CorrectMessageTimer = 0.5F;
                        // 制限時間を延長
                        timer += stationYomi.Length * 2;
                    }
                    else
                    {
                        // すべての駅を入力したら終了
                        finish();
                    }
                }
                else
                {
                    // 正解していない場合は時間をマイナスする
                    timer -= 1f * Time.deltaTime;
                    timerText.text = "残り時間：" + timer.ToString("#,#0.0") + "秒";
                    if (timer <= 0.0f)
                    {
                        // 時間がなくなったら終わり
                        finish();
                    }
                }
                // 正解数の表示
                scoreText.text = "正解数：" + stationMatchCount + "/" + stationCount;

                // 正解メッセージの表示カウント
                if (CorrectMessageTimer > 0.0F)
                {
                    CorrectMessageTimer -= 1f * Time.deltaTime;
                    if (CorrectMessageTimer <= 0.0F)
                    {
                        CorrectMessageTimer = 0.0F;
                        CorrectMessage.SetActive(false);
                    }
                }
            }
        }
    }

    // 開始した時の処理
    public void gameStart(int level)
    {
        StartButton.SetActive(false);
        gameInit = true;
        selectRailName = railName[level - 1];
        ews.SearchRailToStation(selectRailName);
    }

    // 終了時の結果出力とメッセージ表示
    private void finish()
    {
        gameOver = true;
        EndMessageText.text = "「" + selectRailName + "」\n";
        EndMessageText.text += "正解は" + stationCount + "駅中" + stationMatchCount + "駅でした。\n";
        EndMessageText.text += "遊んでくれてありがとう！";
        EndMessage.SetActive(true);
    }

    // 解答する駅名をセットする
    private void setStationName()
    {
        // 「駅名,よみ」を分解
        string[] station = stationList[stationPos - 1].Split(","[0]);
        // 表示する駅名
        stationNameText.text = shortStationName(station[0]);
        // 駅名のよみ
        stationYomi = station[1];
        // 入力するための補正入りよみ
        stationInputYomi = convertStation(station[1]);
    }

    // 駅名のカッコつきは不要なので削除する
    private string shortStationName(string name)
    {
        if (name.IndexOf("(") != -1)
        {
            return name.Substring(0, name.IndexOf("("));
        }
        else
        {
            return name;
        }
    }

    // 補正する駅名
    private string convertStation(string name)
    {
        name = name.Replace("ぁ", "あ");
        name = name.Replace("ぃ", "い");
        name = name.Replace("ぅ", "う");
        name = name.Replace("ぇ", "え");
        name = name.Replace("ぉ", "お");
        name = name.Replace("っ", "つ");
        name = name.Replace("ゃ", "や");
        name = name.Replace("ゅ", "ゆ");
        name = name.Replace("ょ", "よ");
        return name;
    }

    // 入力値を追加する
    public void addString(string j_char)
    {
        if (!gameOver && ews.IsSuccess && gameInit)
        {
            string check_stationYomi = ewsInput.text;
            switch (j_char)
            {
                case "゛":
                    // 濁点を入力した時の処理
                    if (check_stationYomi.Length > 0)
                    {
                        string lastChar = check_stationYomi.Substring(check_stationYomi.Length - 1, 1);
                        if (lastChar == "か") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "が";
                        if (lastChar == "き") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぎ";
                        if (lastChar == "く") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぐ";
                        if (lastChar == "け") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "げ";
                        if (lastChar == "こ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ご";
                        if (lastChar == "さ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ざ";
                        if (lastChar == "し") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "じ";
                        if (lastChar == "す") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ず";
                        if (lastChar == "せ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぜ";
                        if (lastChar == "そ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぞ";
                        if (lastChar == "た") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "だ";
                        if (lastChar == "ち") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぢ";
                        if (lastChar == "つ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "づ";
                        if (lastChar == "て") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "で";
                        if (lastChar == "と") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ど";
                        if (lastChar == "は") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ば";
                        if (lastChar == "ひ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "び";
                        if (lastChar == "ふ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぶ";
                        if (lastChar == "へ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "べ";
                        if (lastChar == "ほ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぼ";
                        if (lastChar == "う") ewsInput.text += j_char;// 「う」は濁点付けられないので、文字として追加
                    }
                    break;
                case "゜":
                    // 半濁点を入力した時の処理
                    if (check_stationYomi.Length > 0)
                    {
                        string lastChar = check_stationYomi.Substring(check_stationYomi.Length - 1, 1);
                        if (lastChar == "は") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぱ";
                        if (lastChar == "ひ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぴ";
                        if (lastChar == "ふ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぷ";
                        if (lastChar == "へ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぺ";
                        if (lastChar == "ほ") ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1) + "ぽ";
                    }
                    break;
                case " ":
                case "　":
                    // 空のボタンを押した時は何も処理をしない
                    break;
                case "←":
                    // 文字の削除ボタンを押した時は最後の文字を消す
                    if (check_stationYomi.Length > 0)
                    {
                        ewsInput.text = check_stationYomi.Substring(0, check_stationYomi.Length - 1);
                    }
                    break;
                default:
                    // 上記以外のボタンを押した時は文字を追加する
                    ewsInput.text += j_char;
                    break;
            }
        }
    }
}
