using System.Collections.Generic;
using System.Threading;

namespace TaiwanStockTrading
{
    public class StockViewModel : ViewModelBase
    {
        // 當冲設定
        private int? _id;
        private string? _symbol;
        private int? _entryOption;
        private int? _entryValue;
        private int? _entryOpenRiseStatus;
        private int? _entryOrderOption;
        private double? _entryOrderPrice;
        private long? _entryTotalPrice;

        private int? _leaveCancel;
        private int? _leaveSell;
        private int? _leaveOrderOption;
        private double? _leaveOrderPrice;
        private long? _leaveTotalPrice;
        private double? _delayTime;

        // 隔日沖額外設定
        private string? _tradeDate;
        private int? _tradeType;
        private int? _openLeave;
        private int? _openAmp;
        private int? _openBelowLeave;
        private int? _forceLeave;
        private string? _ticks;
        private double? _commSell; // 委賣N檔總數
        private double? _avgSell; // 平均成交賣價
        private double? _repoLeft; // 倉部昨餘張數
        private bool? _checkPostExpired; // 檢查此設定是否已過期
        private List<long>? _prevRequestID;
        private int? _cycle;
        private int? _status;
        private string? _statusDesc;
        private int? _cancelStatus;
        private System.DateTime? _createdAt;

        private List<Dictionary<string, object>> _comm = new();
        private Dictionary<long, Dictionary<string, object>> _orders = new();
        private bool _buyCommFinish = false;
        private bool _sellCommFinish = false;
        private bool _cancelCommFinish = false;
        private bool _transLock;
        private bool _leaveLock;


        // Below variables are in Websocket data fields
        private string? _name;
        private double? _open;
        private double? _close;
        private double? _ampDiff;
        private string? _ampPert;
        private int? _single;
        private int? _volume;
        private double? _preclose;
        private string? _buyPrice;
        private string? _sellPrice;
        private double? _avgPrice;
        private double? _high;
        private double? _low;
        private double? _highest;
        private double? _lowest;
        private int? _curUpperBid;
        private int? _maxUpperBid;

        public int? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string? Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }

        public int? EntryOption
        {
            get => _entryOption;
            set => SetProperty(ref _entryOption, value);
        }

        public int? EntryValue
        {
            get => _entryValue;
            set => SetProperty(ref _entryValue, value);
        }

        public int? EntryOrderOption
        {
            get => _entryOrderOption;
            set => SetProperty(ref _entryOrderOption, value);
        }

        public int? EntryOpenRiseStatus
        {
            get => _entryOpenRiseStatus;
            set => SetProperty(ref _entryOpenRiseStatus, value);
        }

        public double? EntryOrderPrice
        {
            get => _entryOrderPrice;
            set => SetProperty(ref _entryOrderPrice, value);
        }

        public long? EntryTotalPrice
        {
            get => _entryTotalPrice;
            set => SetProperty(ref _entryTotalPrice, value);
        }

        public int? LeaveCancel
        {
            get => _leaveCancel;
            set => SetProperty(ref _leaveCancel, value);
        }

        public int? LeaveSell
        {
            get => _leaveSell;
            set => SetProperty(ref _leaveSell, value);
        }

        public int? LeaveOrderOption
        {
            get => _leaveOrderOption;
            set => SetProperty(ref _leaveOrderOption, value);
        }

        public double? LeaveOrderPrice
        {
            get => _leaveOrderPrice;
            set => SetProperty(ref _leaveOrderPrice, value);
        }

        public long? LeaveTotalPrice
        {
            get => _leaveTotalPrice;
            set => SetProperty(ref _leaveTotalPrice, value);
        }

        public double? DelayTime
        {
            get => _delayTime;
            set => SetProperty(ref _delayTime, value);
        }

        public string? TradeDate
        {
            get => _tradeDate;
            set => SetProperty(ref _tradeDate, value);
        }

        public int? TradeType
        {
            get => _tradeType;
            set => SetProperty(ref _tradeType, value);
        }

        public int? OpenLeave
        {
            get => _openLeave;
            set => SetProperty(ref _openLeave, value);
        }

        public int? OpenAmp
        {
            get => _openAmp;
            set => SetProperty(ref _openAmp, value);
        }

        public int? OpenBelowLeave
        {
            get => _openBelowLeave;
            set => SetProperty(ref _openBelowLeave, value);
        }

        public int? ForceLeave
        {
            get => _forceLeave;
            set => SetProperty(ref _forceLeave, value);
        }

        public string? Ticks
        {
            get => _ticks;
            set => SetProperty(ref _ticks, value);
        }

        public double? CommSell
        {
            get => _commSell;
            set => SetProperty(ref _commSell, value);
        }

        public double? AvgSell
        {
            get => _avgSell;
            set => SetProperty(ref _avgSell, value);
        }

        public double? RepoLeft
        {
            get => _repoLeft;
            set => SetProperty(ref _repoLeft, value);
        }

        public bool? CheckPostExpired
        {
            get => _checkPostExpired;
            set => SetProperty(ref _checkPostExpired, value);
        }

        public List<long>? PrevRequestID
        {
            get => _prevRequestID;
            set => SetProperty(ref _prevRequestID, value);
        }

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public double? Close
        {
            get => _close;
            set => SetProperty(ref _close, value);
        }

        public double? AmpDiff
        {
            get => _ampDiff;
            set => SetProperty(ref _ampDiff, value);
        }

        public string? AmpPert
        {
            get => _ampPert;
            set => SetProperty(ref _ampPert, value);
        }
        public int? Single
        {
            get => _single;
            set => SetProperty(ref _single, value);
        }
        public int? Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }
        public double? PreClose
        {
            get => _preclose;
            set => SetProperty(ref _preclose, value);
        }

        public double? Open
        {
            get => _open;
            set => SetProperty(ref _open, value);
        }

        public string? BuyPrice
        {
            get => _buyPrice;
            set => SetProperty(ref _buyPrice, value);
        }

        public string? SellPrice
        {
            get => _sellPrice;
            set => SetProperty(ref _sellPrice, value);
        }

        public double? AvgPrice
        {
            get => _avgPrice;
            set => SetProperty(ref _avgPrice, value);
        }

        public double? High
        {
            get => _high;
            set => SetProperty(ref _high, value);
        }

        public double? Low
        {
            get => _low;
            set => SetProperty(ref _low, value);
        }

        public double? Highest
        {
            get => _highest;
            set => SetProperty(ref _highest, value);
        }

        public double? Lowest
        {
            get => _lowest;
            set => SetProperty(ref _lowest, value);
        }

        public int? CurUpperBid
        {
            get => _curUpperBid;
            set => SetProperty(ref _curUpperBid, value);
        }

        public int? MaxUpperBid
        {
            get => _maxUpperBid;
            set => SetProperty(ref _maxUpperBid, value);
        }

        public int? Cycle
        {
            get => _cycle;
            set => SetProperty(ref _cycle, value);
        }

        public int? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string? StatusDesc
        {
            get => _statusDesc;
            set => SetProperty(ref _statusDesc, value);
        }

        public int? CancelStatus
        {
            get => _cancelStatus;
            set => SetProperty(ref _cancelStatus, value);
        }

        public System.DateTime? CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public bool TransLock
        {
            get => _transLock;
            set => SetProperty(ref _transLock, value);
        }

        public bool LeaveLock
        {
            get => _leaveLock;
            set => SetProperty(ref _leaveLock, value);
        }

        public bool BuyCommFinish
        {
            get => _buyCommFinish;
            set => SetProperty(ref _buyCommFinish, value);
        }

        public bool SellCommFinish
        {
            get => _sellCommFinish;
            set => SetProperty(ref _sellCommFinish, value);
        }

        public bool CancelCommFinish
        {
            get => _cancelCommFinish;
            set => SetProperty(ref _cancelCommFinish, value);
        }

        public List<Dictionary<string, object>> Comm
        {
            get => _comm;
            set => SetProperty(ref _comm, value);
        }

        public Dictionary<long, Dictionary<string, object>> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

    }
}
