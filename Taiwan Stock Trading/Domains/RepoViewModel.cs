using System.Collections.Generic;

namespace TaiwanStockTrading
{
    public class RepoViewModel : ViewModelBase
    {
        // 當冲設定
        private string? _symbol;
        private string? _name;
        private int? _pastLeft;
        private int? _addComm;
        private int? _addDeal;
        private int? _sellComm;
        private int? _sellDeal;

        private string _dtQty;      // 現股當沖數量
        private string _asset;      // 庫存市值
        private string _rlprice;    // 即時價
        private string _assetreal;  // 即時損益
        private string _netpl;      // 未實現損益
        private string _oavgPrice;   // 成交均價

        public string? Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int? PastLeft
        {
            get => _pastLeft;
            set => SetProperty(ref _pastLeft, value);
        }

        public int? AddComm
        {
            get => _addComm;
            set => SetProperty(ref _addComm, value);
        }

        public int? AddDeal
        {
            get => _addDeal;
            set => SetProperty(ref _addDeal, value);
        }

        public int? SellComm
        {
            get => _sellComm;
            set => SetProperty(ref _sellComm, value);
        }

        public int? SellDeal
        {
            get => _sellDeal;
            set => SetProperty(ref _sellDeal, value);
        }

        public string? DtQty
        {
            get => _dtQty;
            set => SetProperty(ref _dtQty, value);
        }

        public string? Asset
        {
            get => _asset;
            set => SetProperty(ref _asset, value);
        }

        public string? Rlprice
        {
            get => _rlprice;
            set => SetProperty(ref _rlprice, value);
        }

        public string? AssetReal
        {
            get => _assetreal;
            set => SetProperty(ref _assetreal, value);
        }

        public string? Netpl
        {
            get => _netpl;
            set => SetProperty(ref _netpl, value);
        }

        public string? OavgPrice
        {
            get => _oavgPrice;
            set => SetProperty(ref _oavgPrice, value);
        }
    }
}