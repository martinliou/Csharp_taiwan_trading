using System;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace TaiwanStockTrading
{
    /// <summary>
    /// Interaction logic for AddStock.xaml
    /// </summary>
    public partial class AddStock : UserControl
    {
        Boolean complete = false;

        public AddStock()
        {
            InitializeComponent();
            MainWindow.mainWindow.mainContentControl.Content = MainGrid;
            DataContext = new FieldsViewModel();
            foreach(var item in MainWindow.Stocks)
            {
                DtSelectBox.Items.Add(string.Format("編號{0} {1}", item.Id, item.Name));
            }

            Delay.Maximum = Config.DT_DELAY_MAXIMUM;
        }

        private void ReturnBtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = new ListStock();
        }

        private void DbConfirmClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // 自選股設定
            string stockNumber = StockNumber.Text;

            // 進場條件設定
            string bookBuy = BookBuy.Text;
            string capital = Capital.Text;
            bool openRise = (bool)OpenRise.IsChecked;
            bool mBuyChk = (bool)MarketBuy.IsChecked;
            bool lBuyChk = (bool)LimitBuy.IsChecked;
            string buyPrice = PriceBuy.Text;
            string buyTotalPrice = BuyTotalPrice.Text;

            // 出場條件設定
            string cancelPert = CancelPert.Text;
            string sellPert = SellPert.Text;
            bool mSellChk = (bool)MarketSell.IsChecked;
            bool lSellChk = (bool)LimitSell.IsChecked;
            string sellPrice = PriceSell.Text;
            string sellTotalPrice = SellTotalPrice.Text;

            int entryOption = 0;
            if (capital.Length != 0)
                entryOption = 1;
            else if (openRise)
                entryOption = 2;

            int entryValue = -1;
            if (entryOption == 0)
                entryValue = Convert.ToInt32(bookBuy);
            else if (entryOption == 1)
                entryValue = Convert.ToInt32(capital);

            var insertedData = new
            {
                uid = MainWindow.encryptUID,
                symbol = stockNumber,
                entry_option = entryOption,
                entry_value = entryValue,
                entry_order_option = mBuyChk ? 0 : 1,
                entry_order_price = lBuyChk ? Convert.ToDouble(buyPrice) : 0,
                entry_total_price = Convert.ToInt64(buyTotalPrice),
                leave_cancel = Convert.ToInt32(cancelPert),
                leave_sell = Convert.ToInt32(sellPert),
                leave_order_option = mSellChk ? 0 : 1,
                leave_order_price = lSellChk ? Convert.ToDouble(sellPrice) : 0,
                leave_total_price = Convert.ToInt64(sellTotalPrice),
                delay_time = Convert.ToDouble(Delay.Value),
            };

            // 資料寫入資料庫
            DBConnection conn = new();
            if (conn.IsConnect())
                conn.InsertSetting(insertedData);

            this.complete = true;

            OpenDialogWithText("新增完成");

        }

        private void NotifyClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.complete)
            {
                this.complete = false;
                MainWindow.refreshFlag = true;
                _ = new ListStock();
            }
        }

        private void ConfirmClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // 自選股設定
            string stockNumber = StockNumber.Text;

            if (stockNumber.Length == 0)
            {
                OpenDialogWithText("請輸入股票代碼，例如: 2330");
                return;
            }

            // 進場條件設定
            string bookBuy = BookBuy.Text;
            string capital = Capital.Text;
            bool openRise = (bool)OpenRise.IsChecked;
            bool mBuyChk = (bool)MarketBuy.IsChecked;
            bool lBuyChk = (bool)LimitBuy.IsChecked;
            string buyPrice = PriceBuy.Text;
            string buyTotalPrice = BuyTotalPrice.Text;

            if (bookBuy.Length == 0 && capital.Length == 0 && !openRise)
            {
                OpenDialogWithText("請輸入漲停價委買張數 或 市值 或 勾選開盤漲停轉市價");
                return;
            }

            if ((bookBuy.Length == 0 && capital.Length != 0 && openRise) ||
                (bookBuy.Length != 0 && capital.Length != 0 && !openRise) ||
                (bookBuy.Length != 0 && capital.Length == 0 && openRise)
                )
            {
                OpenDialogWithText("漲停價委買張數、市值、開盤漲停轉市價僅能三擇一");
                return;
            }

            if (!mBuyChk && !lBuyChk)
            {
                OpenDialogWithText("請選擇買進市價或限價(二擇一)");
                return;
            }

            if (lBuyChk && buyPrice.Length == 0)
            {
                OpenDialogWithText("請輸入買進價");
                return;
            }

            if (buyTotalPrice.Length == 0)
            {
                OpenDialogWithText("請輸入總買進金額");
                return;
            }

            // 出場條件設定
            string cancelPert = CancelPert.Text;
            string sellPert = SellPert.Text;
            bool mSellChk = (bool)MarketSell.IsChecked;
            bool lSellChk = (bool)LimitSell.IsChecked;
            string sellPrice = PriceSell.Text;
            string sellTotalPrice = SellTotalPrice.Text;

            if (cancelPert.Length == 0 || sellPert.Length == 0)
            {
                OpenDialogWithText("請輸入取消 與 賣出%數");
                return;
            }

            bool isInt = int.TryParse(cancelPert, out _);

            if (!isInt)
            {
                OpenDialogWithText("取消%數需為正整數");
                return;
            }

            isInt = int.TryParse(sellPert, out _);

            if (!isInt)
            {
                OpenDialogWithText("賣出%數需為正整數");
                return;
            }

            int cancelPertNum = Convert.ToInt32(cancelPert);
            if (cancelPertNum < 0 || cancelPertNum >= 100)
            {
                OpenDialogWithText("取消%數需介於0-99之間");
                return;
            }

            int sellPertNum = Convert.ToInt32(sellPert);
            if (sellPertNum < 0 || sellPertNum >= 100)
            {
                OpenDialogWithText("賣出%數需介於0-99之間");
                return;
            }

            if (!mSellChk && !lSellChk)
            {
                OpenDialogWithText("請選擇賣出市價或限價(二擇一)");
                return;
            }

            if (lSellChk && sellPrice.Length == 0)
            {
                OpenDialogWithText("請輸入賣出價");
                return;
            }

            if (sellTotalPrice.Length == 0)
            {
                OpenDialogWithText("請輸入今餘%數");
                return;
            }
            isInt = int.TryParse(sellTotalPrice, out _);

            if (!isInt)
            {
                OpenDialogWithText("今餘%數需為正整數");
                return;
            }

            int sellTotalPriceInNum = Convert.ToInt32(sellTotalPrice);

            if (sellTotalPriceInNum < 0 || sellTotalPriceInNum >= 100)
            {
                OpenDialogWithText("今餘%數需介於0-99之間");
                return;
            }

            // 檢查是否有重複單，若為當天必須確認前單已完成整套進出場流程
            foreach(StockViewModel model in MainWindow.Stocks)
            {
                if (model.Symbol == stockNumber &&
                    DateTime.Now.ToString("yyyyMMdd") == Convert.ToDateTime(model.CreatedAt).ToString("yyyyMMdd"))
                {
                    
                    if (model.Status == Config.TRANS_BUY_CANCEL_SUC || model.Status == Config.TRANS_BUY_CANCEL_FAILED)
                    {
                        int status = Config.TRANS_FINISHED;
                        model.Status = status;
                        model.StatusDesc = MainWindow.statusMaps[status];
                        DBConnection db = new();
                        var editedData = new
                        {
                            id = Convert.ToInt32(model.Id),
                            status = Convert.ToInt32(status)
                        };

                        if (db.IsConnect())
                        {
                            db.UpdateSetting(editedData);
                        }
                    }

                    if (model.Status != Config.TRANS_SELL_ORDER_DEAL &&
                        model.Status != Config.TRANS_SELL_ORDER_FAILED &&
                        model.Status != Config.TRANS_FINISHED)
                    {
                        OpenDialogWithText(string.Format("當前已有重複單(編號:{0})，請選擇其他標的!", model.Id));
                        return;
                    }
                }
            }

            AddDbConfirmDialog.IsOpen = true;
        }

        private void OpenDialogWithText(String context)
        {
            DialogText.Text = context;
            Dialog.IsOpen = true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void NumberWithDotValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9\\.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DtSelectBox_DropDownClosed(object sender, EventArgs e)
        {
            var item = sender as ComboBox;
            int curSelIndex = item.SelectedIndex - 1;
            if (curSelIndex >= 0 && curSelIndex < MainWindow.Stocks.Count)
            {
                StockViewModel model = MainWindow.Stocks[curSelIndex];
                StockNumber.Text = model.Symbol;
                int entryOption = Convert.ToInt32(model.EntryOption);

                if (entryOption == 0)
                {
                    BookBuy.Text = Convert.ToString(model.EntryValue);
                }
                else if (entryOption == 1)
                {
                    Capital.Text = Convert.ToString(model.EntryValue);
                }
                else if (entryOption == 2)
                {
                    OpenRise.IsChecked = true;
                }

                int entryOrderOption = Convert.ToInt32(model.EntryOrderOption);

                if (entryOrderOption == 0)
                {
                    MarketBuy.IsChecked = true;
                }
                else if (entryOrderOption == 1)
                {
                    LimitBuy.IsChecked = true;
                    PriceBuy.Text = Convert.ToString(model.EntryOrderPrice);
                }

                BuyTotalPrice.Text = Convert.ToString(model.EntryTotalPrice);

                CancelPert.Text = Convert.ToString(model.LeaveCancel);
                SellPert.Text = Convert.ToString(model.LeaveSell);

                int leaveOrderOption = Convert.ToInt32(model.LeaveOrderOption);

                if (leaveOrderOption == 0)
                {
                    MarketSell.IsChecked = true;
                }
                else if (leaveOrderOption == 1)
                {
                    LimitSell.IsChecked = true;
                    PriceSell.Text = Convert.ToString(model.LeaveOrderPrice);
                }

                SellTotalPrice.Text = Convert.ToString(model.LeaveTotalPrice);
                Delay.Value = Convert.ToDouble(model.DelayTime);
            } else
            {
                StockNumber.Text = string.Empty;
                MarketBuy.IsChecked = true;
                PriceBuy.Text = string.Empty;
                BuyTotalPrice.Text = string.Empty;

                CancelPert.Text = string.Empty;
                SellPert.Text = string.Empty;
                MarketSell.IsChecked = true;
                PriceSell.Text = string.Empty;
                SellTotalPrice.Text = string.Empty;
                Delay.Value = 0;
            }
        }
    }
}
