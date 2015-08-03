using UnityEngine;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using UnityEngine.UI;

public class EkispertWebService : MonoBehaviour
{
    //Webサービスに関する設定
    public string URL;
    public string key;

    //取得した駅名リスト(インスペクターに表示)
    public string[] stationList;

    //入力した駅名の取得とリスト出力のサンプル
    public Text staionInput;
    public Text staionListView;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void searchStation()
    {
        //駅名の取得を押した際の処理
        string inputStaionText = staionInput.text;
        if (inputStaionText.Length > 0)
        {
            StartCoroutine(getStation(inputStaionText));
        }
    }

    private IEnumerator getStation(string stationName)
    {
        //駅名検索用URIの作成
        string uri = URL + "xml/station/light?key=" + key + "&name=" + WWW.EscapeURL(stationName);

        //Webサービスへの問い合わせ処理
        WWW www = new WWW(uri);
        yield return www;

        // 成功
        if (www.error == null)
        {
            Debug.Log("Success");

            //処理しやすいようにArrayListへ一時的に格納
            ArrayList tmp_Station = new ArrayList();

            //Webサービスから取得したXMLの取得
            string xmlString = www.text;

            //XMLの解析を実行
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xmlString));

            //駅名が格納されている地点情報を検索
            XmlNodeList pointList = xmlDoc.GetElementsByTagName("Point");

            foreach (XmlNode point in pointList)
            {
                //地点に含まれる駅情報をNodeに分ける
                foreach (XmlNode pointNode in point.ChildNodes)
                {
                    //NameがStationの要素に駅名が格納されているので比較する
                    if (pointNode.Name == "Station")
                    {
                        //駅の要素内のNodeを解析
                        foreach (XmlNode stationNode in pointNode.ChildNodes)
                        {
                            //Name要素に実際の駅名が格納されている
                            if (stationNode.Name == "Name")
                            {
                                //valueに駅名が入っているのでArrayListに格納
                                tmp_Station.Add(stationNode.FirstChild.Value);
                            }
                        }
                    }
                }
            }
            //ArrayListを配列に変換してインスペクターで見れるようにする
            stationList = (string[])tmp_Station.ToArray(typeof(string));

            //ヒットした駅名を確認
            if (stationList.Length == 0)
            {
                //0件の場合は該当する駅がないのでメッセージを表示する
                staionListView.text = "「" + stationName + "」に該当する駅が見つかりませんでした。";
            }
            else
            {
                //1件以上ヒットした場合はテキスト出力する
                staionListView.text = "";
                for (int i = 0; i < stationList.Length; i++)
                {
                    if (i != 0) { staionListView.text += "\n"; }
                    staionListView.text += stationList[i];
                }
            }
        }
        else
        {
            //Webサービスとの通信に失敗した時の処理
            Debug.Log("Failure");
        }
    }
}
