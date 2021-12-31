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
    public partial class DetailPostStock : UserControl
    {
        private Boolean abort = false;
        public DetailPostStock(StockViewModel model)
        {
            InitializeComponent();
            MainWindow.mainWindow.mainContentControl.Content = DetailGrid;
            Task.Factory.StartNew(() =>
            {
                RunWebSocket(model.Symbol);
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

            CommSell.Text = model.CommSell == null ? string.Empty : Convert.ToString(model.CommSell);
            AvgSell.Text = model.AvgSell == null ? string.Empty : Convert.ToString(model.AvgSell);
            RepoLeft.Text = model.RepoLeft == null ? string.Empty : Convert.ToString(model.RepoLeft);

            string tradeDate = Convert.ToString(model.TradeDate);
            TradeDate.Text = string.Format("交易日期: {0}", tradeDate);
            if (model.OpenLeave == null)
            {
                LeaveOption.Text = string.Format("出場條件: 開盤漲幅低於{0}%時，出場昨餘部位{1}%", model.OpenAmp, model.OpenBelowLeave);
            }
            else
            {
                LeaveOption.Text = string.Format("出場條件: 開盤出場昨餘部位{0}%", model.OpenLeave);
            }

            LeaveForce.Text = string.Format("強制出場: {0}%", model.ForceLeave);
            string[] ticks = model.Ticks.Split(',');
            string tickDesc = string.Format("當前檔次數: {0}{1}", ticks.Length, Environment.NewLine);

            for(int i = 0; i < ticks.Length; i++)
            {
                tickDesc += string.Format("第{0}檔次: {1}%{2}", i + 1, ticks[i], Environment.NewLine);
            }

            LeaveTicks.Text = tickDesc;

            MainWindow.AddPostTranDetails(TranDetail, model);
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

        private void ReturnBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            abort = true;
            _ = new ListStock();
        }

    }
}
