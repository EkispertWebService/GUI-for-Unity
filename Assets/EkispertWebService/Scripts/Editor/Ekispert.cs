using UnityEngine;
using UnityEditor;
using System.Collections;

public class Ekispert : EditorWindow
{
    private string[] DateYearItems;
    private int DateYearIndex = -1;
    private string[] DateMonthItems;
    private int DateMonthIndex = -1;
    private string[] DateDayItems;
    private int DateDayIndex = -1;

    private string[] DateHourItems;
    private int DateHourIndex = -1;
    private string[] DateMinuteItems;
    private int DateMinuteIndex = -1;

    private string StationName1 = "";
    private string StationName2 = "";
    private string StationName3 = "";
    private bool Plane = true;
    private bool Shinkansen = true;
    private bool LimitedExpress = true;
    private bool Bus = true;
    private int SearchTypeIndex = 0;
    private string[] SearchTypeItems = { "出発時刻探索", "到着時刻探索", "終電探索", "始発探索" };

    private string Key = "";
    private string ApiUrl = "http://api.ekispert.com/";

    [@MenuItem("Window/駅すぱあと")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(Ekispert));
    }

    void OnGUI()
    {
        GUILayout.Label("駅すぱあとWebサービス", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("探索種別", EditorStyles.label);
        SearchTypeIndex = EditorGUILayout.Popup(SearchTypeIndex, SearchTypeItems);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("日付", EditorStyles.label);
        int index;
        // 年
        ArrayList YearArray = new ArrayList();
        index = 0;
        for (int i = System.DateTime.Now.Year - 1; i <= System.DateTime.Now.Year + 1; i++)
        {
            if (DateYearIndex == -1 && i == System.DateTime.Now.Year)
            {
                DateYearIndex = index;
            }
            YearArray.Add(string.Format("{0}年", i));
            index++;
        }
        DateYearIndex = EditorGUILayout.Popup(DateYearIndex, (string[])YearArray.ToArray(typeof(string)));
        // 月
        index = 0;
        ArrayList MonthArray = new ArrayList();
        for (int i = 1; i <= 12; i++)
        {
            if (DateMonthIndex == -1 && i == System.DateTime.Now.Month)
            {
                DateMonthIndex = index;
            }
            MonthArray.Add(string.Format("{0}月", i));
            index++;
        }
        DateMonthIndex = EditorGUILayout.Popup(DateMonthIndex, (string[])MonthArray.ToArray(typeof(string)));
        // 日
        index = 0;
        ArrayList DayArray = new ArrayList();
        int LastDay = 31;
        if (DateMonthIndex + 1 == 4 || DateMonthIndex + 1 == 6 || DateMonthIndex + 1 == 9 || DateMonthIndex + 1 == 11)
        {
            LastDay = 30;
        }
        else if (DateMonthIndex + 1 == 2)
        {
            LastDay = 28;
            if ((System.DateTime.Now.Year - 1 + DateYearIndex) % 4 == 0)
            {
                if (!((System.DateTime.Now.Year - 1 + DateYearIndex) % 100 == 0 && (System.DateTime.Now.Year - 1 + DateYearIndex) % 400 != 0))
                {
                    LastDay = 29;
                }
            }
        }
        for (int i = 1; i <= LastDay; i++)
        {
            if (DateDayIndex == -1 && i == System.DateTime.Now.Day)
            {
                DateDayIndex = index;
            }
            DayArray.Add(string.Format("{0}日", i));
            index++;
        }
        DateDayIndex = EditorGUILayout.Popup(DateDayIndex, (string[])DayArray.ToArray(typeof(string)));
        EditorGUILayout.EndHorizontal();

        if (SearchTypeIndex == 0 || SearchTypeIndex == 1)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("時刻", EditorStyles.label);
            // 時
            ArrayList HourArray = new ArrayList();
            for (int i = 0; i <= 23; i++)
            {
                if (DateHourIndex == -1 && i == System.DateTime.Now.Hour)
                {
                    DateHourIndex = i;
                }
                HourArray.Add(string.Format("{0}時", i));
            }
            DateHourIndex = EditorGUILayout.Popup(DateHourIndex, (string[])HourArray.ToArray(typeof(string)));
            // 分
            ArrayList MinuteArray = new ArrayList();
            for (int i = 0; i <= 59; i++)
            {
                if (DateMinuteIndex == -1 && i == System.DateTime.Now.Minute)
                {
                    DateMinuteIndex = i;
                }
                MinuteArray.Add(string.Format("{00}分", i));
            }
            DateMinuteIndex = EditorGUILayout.Popup(DateMinuteIndex, (string[])MinuteArray.ToArray(typeof(string)));
            EditorGUILayout.EndHorizontal();
        }

        StationName1 = EditorGUILayout.TextField("出発地", StationName1);
        StationName2 = EditorGUILayout.TextField("目的地", StationName2);
        StationName3 = EditorGUILayout.TextField("経由地", StationName3);

        Plane = EditorGUILayout.Toggle("飛行機", Plane);
        Shinkansen = EditorGUILayout.Toggle("新幹線", Shinkansen);
        LimitedExpress = EditorGUILayout.Toggle("特急", LimitedExpress);
        Bus = EditorGUILayout.Toggle("バス", Bus);

        Key = EditorGUILayout.TextField("アクセスキー", Key);

        if (Key == "")
        {
            EditorGUILayout.HelpBox("アクセスキーを入力してください", MessageType.Warning);
        }
        else
        {
            if (StationName1 == "" || StationName2 == "")
            {
                EditorGUILayout.HelpBox("出発地と目的地を入力して探索を行ってください", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("探索結果を表示", EditorStyles.miniButton))
                {
                    if (StationName1 != "" && StationName2 != "")
                    {
                        Application.OpenURL(getEWSUrl());
                    }
                }
            }
        }
    }

    private string getEWSUrl()
    {
        string url = ApiUrl + "v1/xml/search/course/light";
        url += "?key=" + Key;
        url += "&from=" + WWW.EscapeURL(StationName1);
        url += "&to=" + WWW.EscapeURL(StationName2);
        if (StationName3 != "")
        {
            url += "&via=" + WWW.EscapeURL(StationName3);
        }
        url += "&date=" + (System.DateTime.Now.Year - 1 + DateYearIndex) + (DateMonthIndex < 9 ? "0" : "") + (DateMonthIndex + 1) + (DateDayIndex < 9 ? "0" : "") + (DateDayIndex + 1);
        switch (SearchTypeIndex)
        {
            case 0:
                url += "&searchType=departure";
                url += "&time=" + (DateHourIndex < 10 ? "0" : "") + DateHourIndex + (DateMinuteIndex < 10 ? "0" : "") + DateMinuteIndex;
                break;
            case 1:
                url += "&searchType=arrival";
                url += "&time=" + (DateHourIndex < 10 ? "0" : "") + DateHourIndex + (DateMinuteIndex < 10 ? "0" : "") + DateMinuteIndex;
                break;
            case 2:
                url += "&searchType=lastTrain";
                break;
            case 3:
                url += "&searchType=firstTrain";
                break;
        }
        url += "&plane=" + (Plane ? "true" : "false");
        url += "&shinkansen=" + (Shinkansen ? "true" : "false");
        url += "&limitedExpress=" + (LimitedExpress ? "true" : "false");
        url += "&bus=" + (Bus ? "true" : "false");
        url += "&redirect=true";
        return url;
    }
}
