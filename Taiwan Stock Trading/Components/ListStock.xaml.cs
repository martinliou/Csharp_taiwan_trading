using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using WebSocketSharp;
using System.Collections;
using System.Windows.Media;
using Intelligence;
using System.Linq;

namespace TaiwanStockTrading
{
    /// <summary>
    /// Interaction logic for ListStock.xaml
    /// </summary>
    public partial class ListStock : UserControl
    {
        private static readonly HttpClient client = new HttpClient();
        private Boolean abort = false;
        private MainWindow window;
        private WebSocket ws;
        public ListStock()
        {
            InitializeComponent();
            TradeComboBox.SelectionChanged += ComboBox_DropDownClosed;
            TradeComboBox.SelectedIndex = MainWindow.selIndex;

            MainWindow.mainWindow.mainContentControl.Content = ListGrid;
            DataContext = new StockListViewModel();
            window = MainWindow.mainWindow;
        }

        private void AddStkSetting_Click(object sender, RoutedEventArgs e)
        {
            _ = new AddStock();
        }

        private void AddPostSetting_Click(object sender, RoutedEventArgs e)
        {
            _ = new AddPostStock();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var rowItem = (sender as Button).DataContext as StockViewModel;
            MainWindow.delId = rowItem.Id;
            string delMsg = string.Format("確定刪除編號{0}之項目？", MainWindow.delId);

            MainWindow.mainWindow.MainDbConfirmDialogText.Text = delMsg;
            MainWindow.mainWindow.MainDbConfirmDialog.IsOpen = true;
            MainWindow.delFlag = true;
        }

        private void DetailBtn_Click(object sender, RoutedEventArgs e)
        {
            var rowItem = (sender as Button).DataContext as StockViewModel;
            if (rowItem.TradeType == 0)
            {
                _ = new DetailStock(rowItem);
            }
            else
            {
                _ = new DetailPostStock(rowItem);
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var rowItem = (sender as Button).DataContext as StockViewModel;
            if (rowItem.TradeType == 0)
            {
                _ = new EditStock(rowItem);
            }
            else
            {
                _ = new EditPostStock(rowItem);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDownOnPost(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string clickObj = e.OriginalSource.GetType().Name;
            if (clickObj != "TextBlock" && clickObj != "Border") return;

            var rowItem = (sender as ListViewItem).DataContext as StockViewModel;
            if (rowItem == null)
            {
                return;
            }

            MainWindow.AddPostTranDetails(TranDetail, rowItem);
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string clickObj = e.OriginalSource.GetType().Name;
            if (clickObj != "TextBlock" && clickObj != "Border") return;

            var rowItem = (sender as ListViewItem).DataContext as StockViewModel;
            if (rowItem == null)
            {
                return;
            }

            MainWindow.AddTranDetails(TranDetail, rowItem);
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var item = sender as ComboBox;
            int curSelected = item.SelectedIndex;
            MainWindow.selIndex = curSelected;
             int[] selArr = { 2, 3, 4, 5 };
            Stocks.Visibility = curSelected == 0 ? Visibility.Visible : Visibility.Collapsed;
            PostStocks.Visibility = curSelected == 1 ? Visibility.Visible : Visibility.Collapsed;
            RepoSummary.Visibility = curSelected == 2 ? Visibility.Visible : Visibility.Collapsed;
            AISelectPanel.Visibility = curSelected == 3 ? Visibility.Visible : Visibility.Collapsed;
            CommSummary.Visibility = curSelected == 4 ? Visibility.Visible : Visibility.Collapsed;
            DealSummary.Visibility = curSelected == 5 ? Visibility.Visible : Visibility.Collapsed;
            DetailView.Visibility = selArr.Contains(curSelected) ? Visibility.Collapsed : Visibility.Visible;

            if (curSelected == 3)
            {
                Dispatcher.Invoke(async () =>
                {
                    await FetchAISelectResponseAsync(0, Convert.ToInt32(10 - AmpSelectBox.SelectedIndex));
                });
            }
        }

        private void AISelectBox_DropDownClosed(object sender, EventArgs e)
        {
            var item = sender as ComboBox;
            int curSelected = item.SelectedIndex;

            AmpSelectBox.IsEnabled = curSelected == 0;

            Dispatcher.Invoke(async () =>
            {
                await FetchAISelectResponseAsync(curSelected, Convert.ToInt32(10 - AmpSelectBox.SelectedIndex));
            });
        }

        private async Task FetchAISelectResponseAsync(int choice, int amp)
        {
            MainWindow.AISelect.Clear();
            string url = string.Format("{0}thirdparty/tw_stock_customized_sort?type={1}&amp={2}",
                Config.REST_HOST,
                choice,
                amp);
            string responseString = await client.GetStringAsync(url);
            try
            {
                JObject jsonObject = JObject.Parse(responseString);
                JToken allData = jsonObject["response"]["data"];
                foreach (var item in allData)
                {
                    try
                    {
                        double preClose = Convert.ToDouble(item["pre_close"]);
                        double curPrice = Convert.ToDouble(item["last_price"]);
                        double ampPert = Math.Round(100 * ((curPrice / preClose) - 1), 2);
                        double buyPrice = Convert.ToDouble(item["bid_1st_price"]);
                        double sellPrice = Convert.ToDouble(item["ask_1st_price"]);
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

                        MainWindow.AISelect.Add(new StockViewModel
                        {
                            Symbol = Convert.ToString(item["symbol"]),
                            Open = Convert.ToDouble(item["open_price"]),
                            High = Convert.ToDouble(item["highest"]),
                            Low = Convert.ToDouble(item["lowest"]),
                            Close = curPrice,
                            PreClose = preClose,
                            Single = Convert.ToInt32(item["single"]),
                            Volume = Convert.ToInt32(item["volume"]),
                            AmpDiff = Math.Round(curPrice - preClose, 2),
                            AmpPert = string.Format("{0}%", ampPert),
                            Name = Convert.ToString(item["name"]),
                            BuyPrice = buyPriceInStr,
                            SellPrice = sellPriceInStr,
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private void AmpSelectBox_DropDownClosed(object sender, EventArgs e)
        {
            var item = sender as ComboBox;
            int curSelected = item.SelectedIndex;

            Dispatcher.Invoke(async () =>
            {
                await FetchAISelectResponseAsync(0, Convert.ToInt32(10 - curSelected));
            });
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            StockViewModel item = (sender as Button).DataContext as StockViewModel;
            string symbol = item.Symbol;

            Task.Factory.StartNew(() =>
            {
                RunWebSocket(symbol);
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

            AddTrade.IsOpen = true;
            StockDesc.Header = string.Format("委託交易 - {0} {1} ", Convert.ToString(item.Symbol), Convert.ToString(item.Name));
            StockDesc.DataContext = item.Symbol;
        }

        private void RunWebSocket(string symbol)
        {
            ws = new WebSocket(Config.WS_HOST);
            ws.Connect();
            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    var results = (JObject)JsonConvert.DeserializeObject(e.Data);
                    var symbolNumber = results["response"]["originReq"]["symbol"];
                    var orders = results["response"]["orders"];
                    IEnumerable enumerable = orders;

                    List<string> orderArrs = new(new string[10]);
                    double lMax = -1;
                    double rMax = -1;

                    foreach (var item in orders)
                    {
                        int position = Convert.ToInt32(item["position"]);
                        double price = Convert.ToDouble(item["price"]);
                        int side = Convert.ToInt32(item["side"]);
                        double size = Convert.ToDouble(item["size"]);
                        orderArrs[(side == 1 ? 0 : 1) + (2 * (position - 1))] = string.Format("{0},{1}", price, size);

                        if (side == 0)
                            lMax = Math.Max(lMax, size);
                        else
                            rMax = Math.Max(rMax, size);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var bookElem = Book;
                            int count = 0;

                            // Handle with click event and data for each button
                            foreach (var btn in FindVisualChildren<Button>(bookElem))
                            {
                                double maxSize = count % 2 == 1 ? lMax : rMax;
                                string mixStr = orderArrs[count];
                                string[] tokens = mixStr.Split(',');
                                double curSize = Convert.ToDouble(tokens[1]);
                                string curSizeInStr = Convert.ToString(curSize);
                                double width = curSize > 0 && maxSize > 0 ?
                                120 * (curSize / maxSize) :
                                -1;

                                if (width > 0)
                                {
                                    width = width < 15 * curSizeInStr.Length ? 15 * curSizeInStr.Length : width;
                                    btn.Width = width;
                                    btn.Content = curSizeInStr;
                                }
                                else
                                {
                                    btn.Width = 0;
                                    btn.Content = string.Empty;
                                }

                                btn.DataContext = tokens[0];
                                btn.Click += new RoutedEventHandler(CommBtnClickHandler);
                                ++count;
                            }

                            count = 0;
                            foreach (var textBlock in FindVisualChildren<TextBlock>(bookElem))
                            {
                                if (Convert.ToString(textBlock.Tag) == "price")
                                {
                                    string mixStr = orderArrs[count++];
                                    string[] tokens = mixStr.Split(',');

                                    double price = Convert.ToDouble(tokens[0]);
                                    string text = string.Empty;

                                    if (price == -1)
                                    {
                                        text = "-";
                                    }
                                    else if (price == 0)
                                    {
                                        text = "市價";
                                    }
                                    else
                                    {
                                        text = tokens[0];
                                    }
                                    textBlock.Text = text;
                                }
                            }
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine(ex1.Message);
                        }

                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            List<WebSocketSendType> webSocketSendType = new();
            webSocketSendType.Add(new WebSocketSendType
            {
                period = "book",
                source = "twse",
                symbol = Convert.ToString(symbol)
            });

            var sendData = JsonConvert.SerializeObject(webSocketSendType);
            ws.Send(sendData);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private void CommBtnClickHandler(object sender, RoutedEventArgs e)
        {
            string price = Convert.ToString((sender as Button).DataContext);

            if (Convert.ToDouble(price) == 0)
            {
                TradePrice.Text = string.Empty;
                TradePriceFlag.SelectedIndex = 1;
            }
            else
            {
                TradePrice.Text = price;
                TradePriceFlag.SelectedIndex = 0;
            }
        }

        private void ClearSettings()
        {
            if (ws.IsAlive)
            {
                ws.Close();
            }

            TradeType.SelectedIndex = 0;
            TradePriceFlag.SelectedIndex = 0;
            TradeKind.SelectedIndex = 0;
            TradeCond.SelectedIndex = 0;
            TradePrice.Text = "";
            TradeCount.Text = "";
            ConfirmCheckbox.IsChecked = false;
        }

        private void CancelCommClick(object sender, RoutedEventArgs e)
        {
            ClearSettings();
        }

        private void AddRepoClick(object sender, RoutedEventArgs e)
        {
            RepoViewModel item = (sender as Button).DataContext as RepoViewModel;
            string symbol = item.Symbol;
            _ = new AddRepoStock(symbol);
        }

        private void AddCommClick(object sender, RoutedEventArgs e)
        {

            Security_OrdType ot = Security_OrdType.OT_NEW;  // 新建單
            Security_Lot sl = Security_Lot.Even_Lot;        // 整股
            int tradeKind = TradeKind.SelectedIndex;
            Security_Class sc;

            switch (tradeKind)
            {
                case 0:
                    sc = Security_Class.SC_Ordinary;
                    break;
                case 1:
                    sc = Security_Class.SC_SelfMargin;
                    break;
                case 2:
                    sc = Security_Class.SC_SelfShort;
                    break;
                case 3:
                    sc = Security_Class.SC_DayMargin;
                    break;
                case 4:
                    sc = Security_Class.SC_DayShort;
                    break;
                case 5:
                    sc = Security_Class.SC_DayTrade;
                    break;
                default:
                    sc = Security_Class.SC_Ordinary;
                    break;
            }

            Security_PriceFlag sp = TradePriceFlag.SelectedIndex == 0 ?
                Security_PriceFlag.SP_FixedPrice : Security_PriceFlag.SP_MarketPrice;
            SIDE_FLAG sf = TradeType.SelectedIndex == 0 ?
                SIDE_FLAG.SF_BUY : SIDE_FLAG.SF_SELL;
            TIME_IN_FORCE TIF;
            int tradeCond = TradeCond.SelectedIndex;

            if (tradeCond == 0)
            {
                TIF = TIME_IN_FORCE.TIF_ROD;
            }
            else if (tradeCond == 1)
            {
                TIF = TIME_IN_FORCE.TIF_IOC;
            }
            else
            {
                TIF = TIME_IN_FORCE.TIF_FOK;
            }

            string sizeInStr = TradeCount.Text;
            bool isInt = int.TryParse(sizeInStr, out _);

            if (!isInt)
            {
                return;
            }

            UInt16 size = Convert.ToUInt16(sizeInStr);

            double price = 0;
            if (sp == Security_PriceFlag.SP_FixedPrice)
            {
                string priceInStr = TradePrice.Text;
                bool isDouble = double.TryParse(priceInStr, out _);

                if (!isDouble)
                {
                    return;
                }

                price = Convert.ToDouble(priceInStr);
            }

            if (!Convert.ToBoolean(ConfirmCheckbox.IsChecked))
            {
                return;
            }

            string symbol = Convert.ToString(StockDesc.DataContext);

            long requestId = MainWindow.tfcom.GetRequestId();

            long ret = MainWindow.tfcom.SecurityOrder(requestId,
                ot,
                sl,
                sc,
                MainWindow.brokerID,
                MainWindow.accountID,
                symbol,
                sf,
                size,
                decimal.Parse(Convert.ToString(price)),
                sp,
                string.Empty,
                string.Empty,
                string.Empty,
                TIF
                );

            ClearSettings();

            AddTrade.IsOpen = false;

            Dispatcher.Invoke(() =>
            {
                ListDialog.IsOpen = true;
                if (ret == 0)
                {
                    ListDialogText.Text = "已成功送出委託!";
                }
                else
                {
                    ListDialogText.Text = string.Format("送出委託失敗，原因：{0}", MainWindow.tfcom.GetOrderErrMsg(ret));
                }
            });

        }
    }
}
