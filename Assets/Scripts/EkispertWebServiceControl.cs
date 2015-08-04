using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EkispertWebServiceControl : MonoBehaviour
{

    // 駅すぱあとWebサービスのオブジェクト
    public GameObject ews_go;
    private EkispertWebService ews;
    private bool load = false;

    // 入力した駅名の取得とリスト出力のサンプル
    public Text staionInput;
    public Text staionListView;

    // Use this for initialization
    void Start()
    {
        ews = ews_go.GetComponent<EkispertWebService>();
    }

    // Update is called once per frame
    void Update()
    {
        if (load && ews.isSuccess)
        {
            load = false;
            viewStationList();
        }
    }

    // 駅名の検索実行
    public void searchStation()
    {
        ews.searchStation(staionInput.text);
        load = true;
    }

    // 駅名の出力
    private void viewStationList()
    {
        if (ews.stationList.Length == 0)
        {
            staionListView.text = ews.message;
        }
        else
        {
            string buffer = "";
            // 1件以上ヒットした場合はテキスト出力する
            for (int i = 0; i < ews.stationList.Length; i++)
            {
                if (i != 0) { buffer += "\n"; }
                buffer += ews.stationList[i];
            }
            // 出力する
            staionListView.text = buffer;
        }
    }
}
