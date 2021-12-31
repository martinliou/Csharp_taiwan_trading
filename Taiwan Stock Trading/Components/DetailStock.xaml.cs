using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using WebSocketSharp;

namespace TaiwanStockTrading
{
    /// <summary>
    /// Interaction logic for DetailStock.xaml
    /// </summary>
    public partial class DetailStock : UserControl
    {
        private Boolean abort = false;
        public DetailStock(StockViewModel model)
        {
            InitializeComponent();
            MainWindow.mainWindow.mainContentControl.Content = DetailGrid;
            Task.Factory.StartNew(() =>
            {
                RunWebSocket(model.Symbol);
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

            Task.Factory.StartNew(() =>
            {
                RunLeftTimer(Convert.ToDateTime(model.CreatedAt));
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);


            Symbol.Text = Convert.ToString(model.Symbol);
            Name.Text = Convert.ToString(model.Name);
            Open.Text = Convert.ToString(model.Open);
            BuyPrice.Text = Convert.ToString(model.BuyPrice);
            SellPrice.Text = Convert.ToString(model.SellPrice);
            Close.Text = Convert.ToString(model.Close);
            AmpDiff.Text = Convert.ToString(model.AmpDiff);
            AmpPert.Text = Convert.ToString(model.AmpPert);
            Single.Text = Convert.ToString(model.Single);
            Volume.Text = Convert.ToString(model.Volume);
            PreClose.Text = Convert.ToString(model.PreClose);
            High.Text = Convert.ToString(model.High);
            Low.Text = Convert.ToString(model.Low);
            StatusDesc.Text = Convert.ToString(model.StatusDesc);
            CurUpperBid.Text = Convert.ToString(model.CurUpperBid);
            MaxUpperBid.Text = Convert.ToString(model.MaxUpperBid);

            int entryOption = Convert.ToInt32(model.EntryOption);
            int entryValue = Convert.ToInt32(model.EntryValue);
            string preStr = string.Empty;
            if (entryOption == 0)
                preStr = "漲停價委買張數";
            else if (entryOption == 1)
                preStr = "市值";
            else if (entryOption == 2)
                preStr = "委買第一檔漲停轉市價";

            EntryOption.Text = string.Format("進場條件：{0}", preStr);
            int entryOrderOption = Convert.ToInt32(model.EntryOrderOption);

            if (entryOrderOption == 0)
            {
                EntryPV.Text = string.Format("條件觸發時，掛買市價單且總買入金額為{0}萬元", model.EntryTotalPrice);
            }
            else
            {
                EntryPV.Text = string.Format("條件觸發時，掛買限價單，其價格為距漲停{0}個檔次，且總買入金額為{1}萬元", model.EntryOrderPrice, model.EntryTotalPrice);
            }

            int cancelPert = Convert.ToInt32(model.LeaveCancel);
            int sellPert = Convert.ToInt32(model.LeaveSell);
            double delayTime = Math.Round(Convert.ToDouble(model.DelayTime), 2);
            LeaveOption.Text = string.Format("出場條件：取消為最大委託之{0}%、賣出為最大委託之{1}%", cancelPert, sellPert);
            int leaveOrderOption = Convert.ToInt32(model.LeaveOrderOption);

            if (leaveOrderOption == 0)
            {
                LeavePV.Text = string.Format("條件觸發時，取消當前所有委買單並掛賣市價單且總賣出為{0}%庫存，並在延遲{1}秒內判斷是否需出場", model.LeaveTotalPrice, model.DelayTime);
            }
            else
            {
                LeavePV.Text = string.Format("條件觸發時，取消當前所有委買單並掛賣限價單，其價格為距漲停{0}個檔次，且總賣出為{1}%庫存，並在延遲{2}秒內判斷是否需出場", model.LeaveOrderPrice, model.LeaveTotalPrice, model.DelayTime);
            }

            MainWindow.AddTranDetails(TranDetail, model);
        }

        private void RunWebSocket(string symbol)
        {
            var ws = new WebSocket(Config.WS_HOST);
            ws.Connect();
            ws.OnMessage += (sender, e) =>
            {
                if (abort) ws.Close();

                var results = (JObject)JsonConvert.DeserializeObject(e.Data);
                var symbolNumber = results["response"]["originReq"]["symbol"];
                var detail = results["response"]["detail"];

                if ((string)symbolNumber == symbol)
                {
                    double close = Convert.ToDouble(detail["close"]);
                    double preClose = Convert.ToDouble(detail["pre_close"]);
                    double ampDiff = Math.Round(close - preClose, 2);
                    double ampPertNum = preClose != 0 ? Math.Round((100 * ampDiff / preClose), 2) : 0;
                    string ampPert = string.Format("{0}%", ampPertNum);
                    double buyPrice = Convert.ToDouble(detail["bid_price"]);
                    double sellPrice = Convert.ToDouble(detail["ask_price"]);
                    string buyPriceInStr = string.Empty;
                    string sellPriceInStr = string.Empty;

                    if (buyPrice == -1)
                    {
                        buyPriceInStr = "-";
                    }
                    else if (buyPrice == 0)
                    {
                        buyPriceInStr = "市價";
                    }
                    else
                    {
                        buyPriceInStr = Convert.ToString(buyPrice);
                    }

                    if (sellPrice == -1)
                    {
                        sellPriceInStr = "-";
                    }
                    else if (sellPrice == 0)
                    {
                        sellPriceInStr = "市價";
                    }
                    else
                    {
                        sellPriceInStr = Convert.ToString(sellPrice);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Symbol.Text = Convert.ToString(symbol);
                        Name.Text = Convert.ToString(detail["name"]);
                        Open.Text = Convert.ToString(detail["open"]);
                        BuyPrice.Text = buyPriceInStr;
                        SellPrice.Text = sellPriceInStr;
                        Close.Text = Convert.ToString(close);
                        AmpDiff.Text = Convert.ToString(ampDiff);
                        AmpPert.Text = Convert.ToString(ampPert);
                        Single.Text = Convert.ToString(detail["volume"]);
                        Volume.Text = Convert.ToString(detail["turnover"]);
                        PreClose.Text = Convert.ToString(preClose);
                        High.Text = Convert.ToString(detail["high"]);
                        Low.Text = Convert.ToString(detail["low"]);
                    });
                    
                }
            };

            List<WebSocketSendType> webSocketSendType = new();
            webSocketSendType.Add(new WebSocketSendType
            {
                period = "",
                source = "twse",
                symbol = Convert.ToString(symbol)
            });

            var sendData = JsonConvert.SerializeObject(webSocketSendType);
            ws.Send(sendData);
        }

        private void RunLeftTimer(DateTime datetime)
        {
            while(true)
            {
                if (abort) break;

                DateTime now = DateTime.UtcNow;
                DateTime createdAt = Convert.ToDateTime(datetime);
                int leftTime = Config.VALID_TIMER - Convert.ToInt32((now - createdAt).TotalSeconds);

                Dispatcher.Invoke(() =>
                {
                    LeftTime.Text = leftTime < 0 ? "已過期" : string.Format("有效期限：{0}秒", leftTime);
                });

                Thread.Sleep(1000);
            }
        }

        private void ReturnBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            abort = true;
            _ = new ListStock();
        }

    }
}
