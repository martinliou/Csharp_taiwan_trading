using System;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TaiwanStockTrading
{
    /// <summary>
    /// Interaction logic for AddPostStock.xaml
    /// </summary>
    public partial class AddPostStock : UserControl
    {
        bool complete = false;
        public AddPostStock()
        {
            InitializeComponent();
            MainWindow.mainWindow.mainContentControl.Content = MainGrid;
            DataContext = new FieldsViewModel();
        }

        private void NotifyClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (complete)
            {
                complete = false;
                MainWindow.selIndex = 1;
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

            if (TradeDate.Text.Length == 0)
            {
                OpenDialogWithText("交易日期不得為空！");
                return;
            }

            try
            {
                string date = Convert.ToDateTime(TradeDate.SelectedDate).ToString("yyyyMMdd");
            }
            catch (Exception)
            {
                OpenDialogWithText("交易日期格式有誤！");
                return;
            }

            // 出場條件
            string openLeave = OpenLeave.Text;
            string openAmp = OpenAmp.Text;
            string openBelowLeave = OpenBelowLeave.Text;

            if (openLeave.Length == 0 && (openAmp.Length == 0 || openBelowLeave.Length == 0))
            {
                OpenDialogWithText("出場條件請二擇一！");
                return;
            }

            if (openLeave.Length != 0)
            {
                if (Convert.ToInt32(openLeave) < 0 || Convert.ToInt32(openLeave) > 100)
                {
                    OpenDialogWithText("請輸入合理的第一出場條件！");
                    return;
                }
            }

            if (openAmp.Length != 0 && openBelowLeave.Length != 0)
            {
                if (Convert.ToInt32(openAmp) < -10 || Convert.ToInt32(openAmp) > 10 ||
                Convert.ToInt32(openBelowLeave) < 0 || Convert.ToInt32(openBelowLeave) > 100)
                {
                    OpenDialogWithText("請輸入合理的第二出場條件！");
                    return;
                }
            }

            string forceLeave = ForceLeave.Text;
            if (forceLeave.Length == 0 || Convert.ToInt32(forceLeave) < 0)
            {
                OpenDialogWithText("請輸入合理強制出場%數（需為正數）！");
                return;
            }

            string upTicks = UpTicks.Text;

            if (upTicks.Length == 0 || Convert.ToInt32(upTicks) < 0)
            {
                OpenDialogWithText("請輸入合理向上掛單檔數（需為正數）！");
                return;
            }

            var tickChildren = TickPanel.Children;
            int totalPendingPert = 0;
            foreach(var tickChild in tickChildren)
            {
                var textBox = tickChild as TextBox;
                string tick = textBox.Text;

                if (tick.Length == 0 || Convert.ToInt32(tick) < 0 || Convert.ToInt32(tick) > 100)
                {
                    OpenDialogWithText("請輸入合理掛單%數（需為正數）！");
                    return;
                }

                totalPendingPert += Convert.ToInt32(tick);
            }

            if (totalPendingPert > 100)
            {
                OpenDialogWithText("累積之掛單%數不得超過100！");
                return;
            }

            string delay = Delay.Text;
            if (Convert.ToInt32(delay) % Config.DELAY != 0)
            {
                OpenDialogWithText(string.Format("取消秒數必須為{0}的倍數！", Config.DELAY));
                return;
            }

            AddDbConfirmDialog.IsOpen = true;
        }

        private void ReturnBtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = new ListStock();
        }

        private void DbConfirmClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // 自選股設定
            string stockNumber = StockNumber.Text;

            // 出場條件
            string openLeave = OpenLeave.Text;
            string openAmp = OpenAmp.Text;
            string openBelowLeave = OpenBelowLeave.Text;
            string forceLeave = ForceLeave.Text;
            string tradeDate = Convert.ToDateTime(TradeDate.SelectedDate).ToString("yyyyMMdd");

            var tickChildren = TickPanel.Children;
            List<string> allTicks = new();
            foreach (var tickChild in tickChildren)
            {
                var textBox = tickChild as TextBox;
                string tick = textBox.Text;
                allTicks.Add(Convert.ToString(tick));
            }

            dynamic insertedData;

            if (openLeave.Length == 0)
            {
                insertedData = new
                {
                    uid = MainWindow.encryptUID,
                    trade_type = 1,
                    trade_date = tradeDate,
                    symbol = stockNumber,
                    open_amp = Convert.ToInt32(openAmp),
                    open_below_leave = Convert.ToInt32(openBelowLeave),
                    force_leave = Convert.ToInt32(forceLeave),
                    delay_time = Convert.ToDouble(Delay.Text),
                    ticks = string.Join(",", allTicks)
                };
            } else
            {
                insertedData = new
                {
                    uid = MainWindow.encryptUID,
                    trade_type = 1,
                    trade_date = tradeDate,
                    symbol = stockNumber,
                    open_leave = Convert.ToInt32(openLeave),
                    force_leave = Convert.ToInt32(forceLeave),
                    delay_time = Convert.ToDouble(Delay.Text),
                    ticks = string.Join(",", allTicks)
                };
            }

            // 資料寫入資料庫
            DBConnection conn = new();
            if (conn.IsConnect())
                conn.InsertSetting(insertedData);

            complete = true;
            OpenDialogWithText("新增完成");
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

        private void OpenDialogWithText(String context)
        {
            DialogText.Text = context;
            Dialog.IsOpen = true;
        }

        private void UpTicks_KeyUp(object sender, KeyEventArgs e)
        {
            var txtBox = sender as TextBox;
            var numObj = txtBox.Text;
            TickPanel.Children.Clear();

            if (!int.TryParse(numObj, out _)) return;

            int num = Convert.ToInt32(numObj);
            if (num > 50)
            {
                OpenDialogWithText("向上檔次設定值過大！");
                UpTicks.Text = string.Empty;
                return;
            }

            for (int i = 0; i < num; i++)
            {
                TextBox textBox = new();
                textBox.Name = string.Format("Tick_{0}", i);
                textBox.Width = 280;
                textBox.HorizontalAlignment = HorizontalAlignment.Left;
                textBox.VerticalAlignment = VerticalAlignment.Center;
                textBox.PreviewTextInput += NumberValidationTextBox;
                HintAssist.SetHint(textBox, string.Format("第{0}檔次%數", i + 1));
                textBox.Style = TryFindResource("MaterialDesignFloatingHintTextBox") as Style;
                TickPanel.Children.Add(textBox);
            }
        }
    }
}
