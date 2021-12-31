﻿using System;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;

namespace TaiwanStockTrading
{
    /// <summary>
    /// Interaction logic for EditStock.xaml
    /// </summary>
    public partial class EditStock : UserControl
    {
        bool complete = false;
        private bool abort = false;
        StockViewModel editedModel = new();
        public EditStock(StockViewModel model)
        {
            InitializeComponent();
            editedModel = model;
            MainWindow.mainWindow.mainContentControl.Content = MainGrid;

            Task.Factory.StartNew(() =>
            {
                RunLeftTimer(Convert.ToDateTime(model.CreatedAt));
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

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
                PriceBuy.Text = Convert.ToString(model.BuyPrice);
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
                PriceSell.Text = Convert.ToString(model.SellPrice);
            }

            SellTotalPrice.Text = Convert.ToString(model.LeaveTotalPrice);
            Delay.Value = Convert.ToDouble(model.DelayTime);
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

            var editedData = new
            {
                id = editedModel.Id,
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
                conn.UpdateSetting(editedData);

            this.complete = true;

            OpenDialogWithText("修改完成");

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

        private void RunLeftTimer(DateTime datetime)
        {
            while (true)
            {
                if (abort) break;

                DateTime now = DateTime.UtcNow;
                DateTime createdAt = Convert.ToDateTime(datetime);
                int leftTime = Config.VALID_TIMER - Convert.ToInt32((now - createdAt).TotalSeconds);

                Dispatcher.Invoke(() =>
                {
                    int status = Convert.ToInt32(editedModel.Status);
                    if (leftTime < 0 || status < Config.TRANS_INIT || status > Config.TRANS_BUY_ORDER_DEAL)

                    {
                        DisableItems(Config.DISABLE_SYMBOL | Config.DISABLE_ENTRY | Config.DISABLE_LEAVE);
                    }

                    if (status == Config.TRANS_BUY_ORDER_SENT || status == Config.TRANS_BUY_ORDER_DEAL)
                    {
                        DisableItems(Config.DISABLE_SYMBOL | Config.DISABLE_ENTRY);
                    }
                });

                Thread.Sleep(1000);
            }
        }

        private void DisableItems(int mask)
        {
            if (Convert.ToBoolean(Config.DISABLE_SYMBOL & mask))
            {
                StockNumber.IsEnabled = false;
            }

            if (Convert.ToBoolean(Config.DISABLE_ENTRY & mask))
            {
                BookBuy.IsEnabled = false;
                Capital.IsEnabled = false;
                OpenRise.IsEnabled = false;
                MarketBuy.IsEnabled = false;
                LimitBuy.IsEnabled = false;
                PriceBuy.IsEnabled = false;
                BuyTotalPrice.IsEnabled = false;
            }

            if (Convert.ToBoolean(Config.DISABLE_LEAVE & mask))
            {
                CancelPert.IsEnabled = false;
                SellPert.IsEnabled = false;
                MarketSell.IsEnabled = false;
                LimitSell.IsEnabled = false;
                PriceSell.IsEnabled = false;
                SellTotalPrice.IsEnabled = false;
                Delay.IsEnabled = false;
            }

            if (mask == (Config.DISABLE_ENTRY | Config.DISABLE_LEAVE | Config.DISABLE_SYMBOL))
            {
                EditButton.IsEnabled = false;
            }
        }
    }
}
