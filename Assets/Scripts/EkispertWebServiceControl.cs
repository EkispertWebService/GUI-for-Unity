using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EkispertWebServiceControl : MonoBehaviour
{
    // 駅すぱあとWebサービスのオブジェクト
    public GameObject ews_go;
    private EkispertWebService ews;
    private bool load = false;

    // 入力した文字列の取得とリスト出力のサンプル
    public Text ewsInput;
    public Text ewsListView;

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
            viewResultList();
        }
    }

    // 会社名の検索実行
    public void searchCorporation()
    {
        ews.searchCorporation(ewsInput.text);
        load = true;
    }

    // 路線名の検索実行
    public void searchRail()
    {
        ews.searchRail(ewsInput.text);
        load = true;
    }

    // 駅名の検索実行
    public void searchStation()
    {
        ews.searchStation(ewsInput.text);
        load = true;
    }

    // 取得した情報の出力
    private void viewResultList()
    {
        if (ews.resultList.Length == 0)
        {
            ewsListView.text = ews.message;
        }
        else
        {
            string buffer = "";
            // 1件以上ヒットした場合はテキスト出力する
            for (int i = 0; i < ews.resultList.Length; i++)
            {
                if (i != 0) { buffer += "\n"; }
                buffer += ews.resultList[i];
            }
            // 出力する
            ewsListView.text = buffer;
        }
    }
}
