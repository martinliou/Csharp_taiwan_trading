namespace TaiwanStockTrading
{
    public class DealViewModel : ViewModelBase
    {
        private string _symbol;
        private string _clientOrderTime;
        private string _reportTime;
        private string _side;
        private string _func;
        private string _price;
        private string _qty;
        private string _orderID;
        private string _marketID;
        private string _ordClass;
        private string _tradeDate;

        public string TradeDate
        {
            get => _tradeDate;
            set => SetProperty(ref _tradeDate, value);
        }

        public string Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }

        public string OrderTime
        {
            get => _clientOrderTime;
            set => SetProperty(ref _clientOrderTime, value);
        }

        public string ReportTime
        {
            get => _reportTime;
            set => SetProperty(ref _reportTime, value);
        }

        public string Side
        {
            get => _side;
            set => SetProperty(ref _side, value);
        }

        public string Func
        {
            get => _func;
            set => SetProperty(ref _func, value);
        }

        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public string Qty
        {
            get => _qty;
            set => SetProperty(ref _qty, value);
        }

        public string OrderID
        {
            get => _orderID;
            set => SetProperty(ref _orderID, value);
        }

        public string MarketID
        {
            get => _marketID;
            set => SetProperty(ref _marketID, value);
        }

        public string OrdClass
        {
            get => _ordClass;
            set => SetProperty(ref _ordClass, value);
        }
    }

}