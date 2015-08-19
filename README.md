# 駅すぱあとWebサービス for Unity

## はじめに

 * **お申込み**

 はじめてご利用いただく場合は[駅すぱあとワールド](https://ekiworld.net/)でのお申込み(無償/有償)とキーの発行が必要になります。
 
 なお、無償版のお申し込みは下記のページから行ってください。
 
 [駅すぱあとWebサービス無償提供API](https://ekiworld.net/service/sier/webservice/free_provision.html)
 
 また、後日、認証キーをメールにてお送りいたします。

## ご利用方法

 駅すぱあとWebサービスのURLと認証キーの設定が必要になります。<br>
 下記の手順にてURLとキーを設定してください。<br>

 * **プロジェクトを新規作成する場合**

  1. Unityのプロジェクトを新規作成します
  2. Assets以下をプロジェクトにコピーします
  3. Assets/EkispertWebService/Scenes/EkispertWebService.unityを開きます
  4. Hierarchyの中から「EkispertWebService」を選択します
  5. Inspector内の「Ekispert Web Service (Script)」のUrlとKeyを書き換えます

 * **URLの設定内容**

 | プラン | URLの値 |
 | --- | --- |
 | 無償版 | http://api.ekispert.com/v1/ |
 | 有償版(http) | http://api.ekispert.jp/v1/ |
 | 有償版(https) | https://api.ekispert.jp/v1/ |

## ご注意

 * 既存のプロジェクトに追加することも可能ですが、既存のファイルを上書きしてしまうなど、十分ご注意ください。

