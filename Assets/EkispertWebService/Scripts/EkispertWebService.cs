using UnityEngine;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;

public class EkispertWebService : MonoBehaviour
{
    //Webサービスに関する設定
    public string Url;
    public string Key;

    // 取得した情報のリスト
    public enum dataType
    {
        Corporation,
        Rail,
        Station,
        CorporationToRail,
        RailToStation
    }
    public dataType Api;

    // 交通機関を特定するためのパラメータ
    public string Type;

    // 都道府県を固定するためのパラメータ
    public string PrefectureCode;

    // 入力した内容を処理する部分
    public string Name;
    private string oldName;
    
    // 結果を格納する部分
    public string[] ResultList;

    // フラグ、メッセージなど
    public string Message;
    public bool IsSuccess;

    // 負荷軽減のためのタイマー
    private int timer = 0;

    // 結果を格納するためのArray
    private ArrayList resultArray;

    // Use this for initialization
    void Start()
    {
        IsSuccess = false;
        Message = "";
    }

    // Update is called once per frame
    void Update()
    {
        // 自動的に検索を実行する
        if (oldName != Name && Name != "" && timer == 0)
        {
            timer = 120;
            oldName = Name;
            if (Api == dataType.Corporation)
            {
                SearchCorporation(Name);
            }
            else if (Api == dataType.Rail)
            {
                SearchRail(Name);
            }
            else if (Api == dataType.Station)
            {
                SearchStation(Name);
            }
            else if (Api == dataType.CorporationToRail)
            {
                SearchCorporationToRail(Name);
            }
            else if (Api == dataType.RailToStation)
            {
                SearchRailToStation(Name);
            }
        }
        else if (timer > 0)
        {
            timer--;
        }
    }

    public void SearchCorporation(string inputText)
    {
        // 会社名の取得を押した際の処理
        Api = dataType.Corporation;
        IsSuccess = false;
        Message = "";
        oldName = Name = inputText;
        StartCoroutine(getEkispert(inputText, 1));
    }

    public void SearchRail(string inputText)
    {
        // 路線名の取得を押した際の処理
        Api = dataType.Rail;
        IsSuccess = false;
        Message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            IsSuccess = true;
            Message = "1文字以上指定してください。";
        }
    }

    public void SearchCorporationToRail(string inputText)
    {
        // 会社名から路線名の変換処理
        Api = dataType.CorporationToRail;
        IsSuccess = false;
        Message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            IsSuccess = true;
            Message = "1文字以上指定してください。";
        }
    }

    public void SearchStation(string inputText)
    {
        // 駅名の取得を押した際の処理
        Api = dataType.Station;
        IsSuccess = false;
        Message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            IsSuccess = true;
            Message = "1文字以上指定してください。";
        }
    }

    public void SearchRailToStation(string inputText)
    {
        // 路線名から駅名の変換処理
        Api = dataType.RailToStation;
        IsSuccess = false;
        Message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            IsSuccess = true;
            Message = "1文字以上指定してください。";
        }
    }

    // アクセス用の汎用処理
    private IEnumerator getEkispert(string inputText, int offset)
    {
        // 検索用URIの作成
        string uri = "";
        if (Api == dataType.Corporation)
        {
            uri = Url + "xml/corporation?key=" + Key;
            if (inputText != "")
            {
                uri += "&name=" + WWW.EscapeURL(inputText);
            }
        }
        else if (Api == dataType.CorporationToRail)
        {
            uri = Url + "xml/rail?key=" + Key + "&corporationName=" + WWW.EscapeURL(inputText);
        }
        else if (Api == dataType.Rail)
        {
            uri = Url + "xml/rail?key=" + Key + "&name=" + WWW.EscapeURL(inputText);
        }
        else if (Api == dataType.RailToStation)
        {
            uri = Url + "xml/station?key=" + Key + "&railName=" + WWW.EscapeURL(inputText);
            // 片方向にしか走っていない場合は取得できないため、directionを指定する
            uri += "&direction=none";
        }
        else if (Api == dataType.Station)
        {
            uri = Url + "xml/station?key=" + Key + "&name=" + WWW.EscapeURL(inputText);
        }
        // 都道府県の指定
        if (PrefectureCode != "")
        {
            uri += "&prefectureCode=" + PrefectureCode;
        }
        // 交通種別の指定
        if (Type != "")
        {
            uri += "&type=" + Type;
        }
        if (offset != 1)
        {
            uri += "&offset=" + offset;
        }

        // Webサービスへの問い合わせ処理
        WWW www = new WWW(uri);
        yield return www;

        // 成功
        if (www.error == null)
        {
            // 処理しやすいようにArrayListへ一時的に格納
            if (offset == 1)
            {
                resultArray = new ArrayList();
            }

            // Webサービスから取得したXMLの取得
            string xmlString = www.text;

            // XMLの解析を実行
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xmlString));

            //検索件数を取得
            int max = 0;
            foreach (XmlNode node in xmlDoc.SelectNodes("ResultSet"))
            {
                if (node.Attributes.GetNamedItem("max") != null)
                {
                    max = int.Parse(node.Attributes.GetNamedItem("max").Value);
                }
            }

            if (Api == dataType.Corporation)
            {
                // 会社名が格納されている情報を検索
                XmlNodeList corporationList = xmlDoc.GetElementsByTagName("Corporation");

                // CorporationをNodeに分ける
                foreach (XmlNode corporationNode in corporationList)
                {
                    // 会社の要素内のNodeを解析
                    foreach (XmlNode corporation in corporationNode.ChildNodes)
                    {
                        // Name要素に実際の会社名が格納されている
                        if (corporation.Name == "Name")
                        {
                            // valueに会社名が入っているのでArrayListに格納
                            resultArray.Add(corporation.FirstChild.Value);
                        }
                    }
                }
            }
            else if (Api == dataType.Rail || Api == dataType.CorporationToRail)
            {
                // 路線名が格納されている情報を検索
                XmlNodeList railList = xmlDoc.GetElementsByTagName("Line");

                // LineをNodeに分ける
                foreach (XmlNode railNode in railList)
                {
                    // 路線の要素内のNodeを解析
                    foreach (XmlNode rail in railNode.ChildNodes)
                    {
                        // Name要素に実際の路線名が格納されている
                        if (rail.Name == "Name")
                        {
                            // valueに路線名が入っているのでArrayListに格納
                            resultArray.Add(rail.FirstChild.Value);
                        }
                    }
                }
            }
            else if (Api == dataType.Station || Api == dataType.RailToStation)
            {
                // 駅名が格納されている地点情報を検索
                XmlNodeList pointList = xmlDoc.GetElementsByTagName("Point");

                foreach (XmlNode point in pointList)
                {
                    // 駅名と読み
                    string stationNameText = "";
                    string stationYomiText = "";

                    // 地点に含まれる駅情報をNodeに分ける
                    foreach (XmlNode pointNode in point.ChildNodes)
                    {
                        // NameがStationの要素に駅名が格納されているので比較する
                        if (pointNode.Name == "Station")
                        {
                            // 駅の要素内のNodeを解析
                            foreach (XmlNode stationNode in pointNode.ChildNodes)
                            {
                                // Name要素に実際の駅名が格納されている
                                if (stationNode.Name == "Name")
                                {
                                    // valueに駅名が入っているのでArrayListに格納
                                    stationNameText = stationNode.FirstChild.Value;
                                }
                                else if (stationNode.Name == "Yomi")
                                {
                                    // valueに駅名が入っているのでArrayListに格納
                                    stationYomiText = stationNode.FirstChild.Value;
                                }
                            }
                        }
                    }
                    //データを格納
                    resultArray.Add(stationNameText + "," + stationYomiText);
                }
            }

            // ArrayListを配列に変換してインスペクターで見れるようにする
            ResultList = (string[])resultArray.ToArray(typeof(string));

            // ヒットした情報を確認
            if (ResultList.Length == 0)
            {
                // 0件の場合は該当する情報がないのでメッセージを表示する
                Message = "「" + inputText + "」に該当する";
                if (Api == dataType.Corporation)
                {
                    Message += "会社名";
                }
                else if (Api == dataType.Rail || Api == dataType.CorporationToRail)
                {
                    Message += "路線名";
                }
                else if (Api == dataType.Station || Api == dataType.RailToStation)
                {
                    Message += "駅";
                }
                Message += "が見つかりませんでした。";
            }

            // maxに届かなかった場合は更に取得する
            if (max > (offset + 100 - 1))
            {
                StartCoroutine(getEkispert(inputText, offset + 100));
            }
            else
            {
                // 処理が完了した際にtrueにする
                IsSuccess = true;
                Message += "取得が完了しました。";
            }
        }
        else
        {
            // Webサービスとの通信に失敗した時の処理
            Message += "駅すぱあとWebサービスと通信ができませんでした。";
        }
    }
}
