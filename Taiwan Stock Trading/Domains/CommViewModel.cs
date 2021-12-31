namespace TaiwanStockTrading
{
    public class CommViewModel : ViewModelBase
    {
        private string _symbol;
        private string _clientOrderTime;
        private string _reportTime;
        private string _side;
        private string _func;
        private string _price;
        private string _beforeQty;
        private string _afterQty;
        private string _errCode;
        private string _errMsg;
        private string _orderID;

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

        public string BeforeQty
        {
            get => _beforeQty;
            set => SetProperty(ref _beforeQty, value);
        }

        public string AfterQty
        {
            get => _afterQty;
            set => SetProperty(ref _afterQty, value);
        }
        
        public string ErrCode
        {
            get => _errCode;
            set => SetProperty(ref _errCode, value);
        }

        public string ErrMsg
        {
            get => _errMsg;
            set => SetProperty(ref _errMsg, value);
        }

        public string OrderID
        {
            get => _orderID;
            set => SetProperty(ref _orderID, value);
        }
    }

}