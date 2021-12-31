using System.Collections.ObjectModel;
using System;

namespace TaiwanStockTrading
{
    class StockListViewModel: ViewModelBase
    {
        public StockListViewModel()
        {
            // 取得當沖資料
            if (MainWindow.Stocks.Count == 0 || MainWindow.PostStocks.Count == 0 || MainWindow.refreshFlag)
            {
                MainWindow.refreshFlag = false;
                DBConnection conn = new();

                if (conn.IsConnect())
                {
                    Stocks = conn.QuerySettings(0);
                    PostStocks = conn.QuerySettings(1);
                }

                MainWindow.Stocks = Stocks;
                MainWindow.PostStocks = PostStocks;
                RepoSummary = MainWindow.RepoSummary;
                CommSummary = MainWindow.CommSummary;
                DealSummary = MainWindow.DealSummary;
                AISelect = MainWindow.AISelect;
                MainWindow.refreshWS = true;
            }
        }
        public static ObservableCollection<StockViewModel> Stocks { get; set; }
        public static ObservableCollection<StockViewModel> PostStocks { get; set; }
        public static ObservableCollection<RepoViewModel> RepoSummary { get; set; }
        public static ObservableCollection<StockViewModel> AISelect { get; set; }
        public static ObservableCollection<CommViewModel> CommSummary { get; set; }
        public static ObservableCollection<DealViewModel> DealSummary { get; set; }

    }
}
