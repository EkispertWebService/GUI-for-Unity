using UnityEngine;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;

public class EkispertWebService : MonoBehaviour
{
    //Webサービスに関する設定
    public string URL;
    public string key;

    // 取得した情報のリスト
    public enum dataType
    {
        Corporation,
        Rail,
        Station
    }
    public dataType API;

    // 入力した内容を処理する部分
    public string Name;
    private string oldName;

    // 結果を格納する部分
    public string[] resultList;

    // フラグ、メッセージなど
    public string message;
    public bool isSuccess;

    // 負荷軽減のためのタイマー
    private int timer = 0;

    // 結果を格納するためのArray
    private ArrayList resultArray;

    // Use this for initialization
    void Start()
    {
        isSuccess = false;
        message = "";
    }

    // Update is called once per frame
    void Update()
    {
        // 自動的に検索を実行する
        if (oldName != Name && Name != "" && timer == 0)
        {
            timer = 120;
            oldName = Name;
            if (API == dataType.Corporation)
            {
                searchCorporation(Name);
            }
            else if (API == dataType.Rail)
            {
                searchRail(Name);
            }
            else if (API == dataType.Station)
            {
                searchStation(Name);
            }
        }
        else if (timer > 0)
        {
            timer--;
        }
    }

    public void searchCorporation(string inputText)
    {
        // 会社名の取得を押した際の処理
        API = dataType.Corporation;
        isSuccess = false;
        message = "";
        oldName = Name = inputText;
        StartCoroutine(getEkispert(inputText, 1));
    }

    public void searchRail(string inputText)
    {
        // 路線名の取得を押した際の処理
        API = dataType.Rail;
        isSuccess = false;
        message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            isSuccess = true;
            message = "1文字以上指定してください。";
        }
    }

    public void searchStation(string inputText)
    {
        // 駅名の取得を押した際の処理
        API = dataType.Station;
        isSuccess = false;
        message = "";
        oldName = Name = inputText;
        if (inputText.Length > 0)
        {
            StartCoroutine(getEkispert(inputText, 1));
        }
        else
        {
            isSuccess = true;
            message = "1文字以上指定してください。";
        }
    }

    // アクセス用の汎用処理
    private IEnumerator getEkispert(string inputText, int offset)
    {
        // 検索用URIの作成
        string uri = "";
        if (API == dataType.Corporation)
        {
            uri = URL + "xml/corporation?key=" + key;
            if (inputText != "")
            {
                uri += "&name=" + WWW.EscapeURL(inputText);
            }
        }
        else if (API == dataType.Rail)
        {
            uri = URL + "xml/rail?key=" + key + "&name=" + WWW.EscapeURL(inputText);
        }
        else if (API == dataType.Station)
        {
            uri = URL + "xml/station?key=" + key + "&name=" + WWW.EscapeURL(inputText);
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

            if (API == dataType.Corporation)
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
            else if (API == dataType.Rail)
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
            else if (API == dataType.Station)
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
            resultList = (string[])resultArray.ToArray(typeof(string));

            // ヒットした情報を確認
            if (resultList.Length == 0)
            {
                // 0件の場合は該当する情報がないのでメッセージを表示する
                message = "「" + inputText + "」に該当する";
                if (API == dataType.Corporation)
                {
                    message += "会社名";
                }
                else if (API == dataType.Rail)
                {
                    message += "路線名";
                }
                else if (API == dataType.Station)
                {
                    message += "駅";
                }

                message += "が見つかりませんでした。";
            }

            // maxに届かなかった場合は更に取得する
            if (max > (offset + 100 - 1))
            {
                StartCoroutine(getEkispert(inputText, offset + 100));
            }
            else
            {
                // 処理が完了した際にtrueにする
                isSuccess = true;
            }
        }
        else
        {
            // Webサービスとの通信に失敗した時の処理
        }
    }
}
