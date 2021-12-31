using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WebSocketSharp;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

//***KGI API***
using Intelligence;
using Smart;
using Package;
using System.Windows.Controls;
using System.Diagnostics;

namespace TaiwanStockTrading
{

    public class WebSocketSendType
    {
        public string source { get; set; }
        public string symbol { get; set; }
        public string period { get; set; }
    }

    public partial class MainWindow : Window
    {
        public static ObservableCollection<StockViewModel> Stocks = new();
        public static ObservableCollection<StockViewModel> PostStocks = new();
        public static ObservableCollection<RepoViewModel> RepoSummary = new();
        public static ObservableCollection<StockViewModel> AISelect = new();
        public static ObservableCollection<CommViewModel> CommSummary = new();
        public static ObservableCollection<DealViewModel> DealSummary = new();
        public static List<Dictionary<string, string>> repoObjects = new();
        public static DBConnection dbConnection = new();
        public static MainWindow mainWindow = new();
        private List<string> prevSymbolSet = new();
        public static TaiFexCom tfcom;
        // Below items are used for deleting settings
        public static Boolean delFlag = false;
        public static int? delId = -1;

        public static Boolean refreshFlag = false;
        public static Boolean refreshWS = false;
        public static string encryptUID = string.Empty;
        public static string brokerID = string.Empty;
        public static string accountID = string.Empty;
        public static int selIndex = 0;
        public static List<int> invalidOrders = new();
        public static Dictionary<string, Dictionary<string, int>> bookMaps = new();
        public static Dictionary<string, Dictionary<string, object>> repoMaps = new();
        public static Dictionary<int, Dictionary<string, object>> modelMaps = new();
        public static Dictionary<string, int> dtMaps = new();
        public static readonly Dictionary<int, string> statusMaps = new Dictionary<int, string>
        {
            {Config.TRANS_INIT, Config.TRANS_INIT_DESC },
            {Config.TRANS_BUY_ORDER_DEAL, Config.TRANS_BUY_ORDER_DEAL_DESC },
            {Config.TRANS_BUY_ORDER_SENT, Config.TRANS_BUY_ORDER_SENT_DESC },
            {Config.TRANS_BUY_ORDER_FAILED, Config.TRANS_BUY_ORDER_FAILED_DESC },
            {Config.TRANS_SELL_ORDER_DEAL, Config.TRANS_SELL_ORDER_DEAL_DESC },
            {Config.TRANS_SELL_ORDER_SENT, Config.TRANS_SELL_ORDER_SENT_DESC },
            {Config.TRANS_SELL_ORDER_FAILED, Config.TRANS_SELL_ORDER_FAILED_DESC },
            {Config.TRANS_BUY_CANCEL_SUC, Config.TRANS_BUY_CANCEL_SUC_DESC },
            {Config.TRANS_BUY_CANCEL_FAILED, Config.TRANS_BUY_CANCEL_FAILED_DESC },
            {Config.TRANS_SELL_CANCEL_SUC, Config.TRANS_SELL_CANCEL_SUC_DESC },
            {Config.TRANS_SELL_CANCEL_FAILED, Config.TRANS_SELL_CANCEL_FAILED_DESC },
            {Config.TRANS_FINISHED, Config.TRANS_FINISHED_DESC },
        };
        public MainWindow()
        {
            InitializeComponent();
        }

        public static void AddPostTranDetails(StackPanel TranDetail, StockViewModel item)
        {
            TranDetail.Children.Clear();
            var comms = item.Comm;

            foreach (var comm in comms)
            {
                try
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(0, 0, 0, 4);
                    DateTime o = Convert.ToDateTime(comm["created_at"]);
                    string dtDesc = Common.GmtToTaipei(o);
                    string requestID = Convert.ToString(comm["request_id"]);
                    string orderNo = Convert.ToString(comm["order_no"]);
                    int status = Convert.ToInt32(comm["status"]);
                    string orderDesc = string.Format("委託書號: {0}", orderNo);

                    double dealQty = 0;
                    string dealtime = string.Empty;
                    double dealPrice = 0;

                    if (comm.ContainsKey("deal_qty") && comm.ContainsKey("deal_time") && comm.ContainsKey("deal_avg_price"))
                    {
                        dealQty = Convert.ToDouble(comm["deal_qty"]);
                        dealtime = Convert.ToString(comm["deal_time"]);
                        dealPrice = Convert.ToDouble(comm["deal_avg_price"]);
                    }

                    string dealDesc = string.Format("已在{0}成交{1}張，平均成交價為{2}", dealtime, dealQty, dealPrice);

                    textBlock.Text = string.Format("[狀態:{0}][時間:{1}][電子單號:{2}][{3}] {4}",
                        statusMaps[status],
                        dtDesc,
                        requestID,
                        orderDesc,
                        dealDesc
                        );

                    TranDetail.Children.Add(textBlock);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public static void AddTranDetails(StackPanel TranDetail, StockViewModel item)
        {
            TranDetail.Children.Clear();
            var comms = item.Comm;
            int entryOrderOption = Convert.ToInt32(item.EntryOrderOption);
            int leaveOrderOption = Convert.ToInt32(item.LeaveOrderOption);
            foreach (var comm in comms)
            {
                try
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(0, 0, 0, 4);
                    DateTime o = Convert.ToDateTime(comm["created_at"]);
                    string dtDesc = Common.GmtToTaipei(o);
                    string requestID = Convert.ToString(comm["request_id"]);
                    string orderDesc = string.Empty;
                    string orderNo = Convert.ToString(comm["order_no"]);
                    string orderTime = Convert.ToString(comm["order_time"]);
                    int status = Convert.ToInt32(comm["status"]);

                    if (orderNo.Length == 0 && orderTime.Length == 0)
                    {
                        orderDesc = "目前無委託書號資訊";
                    }
                    else
                    {
                        orderDesc = string.Format("委託書號: {0}, 委託時間: {1}", orderNo, orderTime);
                    }

                    string side = Convert.ToString(comm["side"]);
                    string sideDesc = string.Empty;
                    string price = Convert.ToString(comm["price"]);

                    if (side == "B")
                    {
                        sideDesc = "買進";
                        if (entryOrderOption == 0)
                        {
                            price = "市價";
                        }
                    }
                    else
                    {
                        sideDesc = "賣出";
                        if (leaveOrderOption == 0)
                        {
                            price = "市價";
                        }
                    }

                    string qty = Convert.ToString(comm["qty"]);
                    double dealQty = 0;
                    string dealtime = string.Empty;
                    double dealPrice = 0;

                    if (comm.ContainsKey("deal_qty") && comm.ContainsKey("deal_time") && comm.ContainsKey("deal_avg_price"))
                    {
                        dealQty = Convert.ToDouble(comm["deal_qty"]);
                        dealtime = Convert.ToString(comm["deal_time"]);
                        dealPrice = Convert.ToDouble(comm["deal_avg_price"]);
                    }

                    string dealDesc = string.Empty;
                    if (dealQty == 0 || dealtime.Length == 0 || dealPrice == 0)
                    {
                        dealDesc = "目前無成交部位";
                    }
                    else
                    {
                        dealDesc = string.Format("已在{0}成交{1}張，平均成交價為{2}", dealtime, dealQty, dealPrice);
                    }

                    string errorMsg = string.Empty;
                    if (comm.ContainsKey("err_code") &&
                        comm.ContainsKey("err_msg") &&
                        Convert.ToString(comm["err_code"]).Length != 0 &&
                        Convert.ToString(comm["err_msg"]).Length != 0
                        )
                    {
                        errorMsg = string.Format("錯誤訊息：[代碼{0}] {1}", Convert.ToString(comm["err_code"]), Convert.ToString(comm["err_msg"]));
                    }

                    textBlock.Text = string.Format("[狀態:{0}][時間:{1}][電子單號:{2}][{3}] {4}價:{5} 量:{6} {7} {8}",
                        statusMaps[status],
                        dtDesc,
                        requestID,
                        orderDesc,
                        sideDesc,
                        price,
                        qty,
                        dealDesc,
                        errorMsg
                        );

                    TranDetail.Children.Add(textBlock);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void InitKGI()
        {
            tfcom = new TaiFexCom(Config.KGI_DOMAIN_NAME, Config.KGI_TCP_PORT, "API");
            tfcom.AutoRecoverReportSecurity = true;
            tfcom.AutoSubReportSecurity = true;
            tfcom.OnRcvMessage += OnRcvMessage;
            tfcom.ShowLogin();
        }

        private void OnRcvMessage(object sender, PackageBase package)
        {
            if (!Dispatcher.CheckAccess())
            {
                OnRcvMessage_EventHandler d = new OnRcvMessage_EventHandler(OnRcvMessage);
                Dispatcher.Invoke(d, new object[] { sender, package });
                return;
            }

            switch ((DT)package.DT)
            {
                case DT.LOGIN:
                    P001503 p1503 = (P001503)package;
                    if (p1503.Code != 0)
                    {
                        AddSnackQueueMsg("登入失敗！", 1);
                    }
                    else
                    {
                        var accountLists = p1503.p001503_2;

                        foreach (var item in accountLists)
                        {
                            brokerID = item.BrokeId;
                            accountID = item.Account;
                            break;
                        }

                        AddSnackQueueMsg("登入成功！", 1);
                        encryptUID = p1503.EncryID;
                        DBConnection db = new();
                        if (db.IsConnect())
                        {
                            bool result = db.CheckUserIsValid();
                            if (!result)
                            {
                                MainDialogText.Text = "此帳號尚未開通權限!";
                                MainDialog.IsOpen = true;
                                return;
                            }
                        }

                        _ = new ListStock();

                        Task.Factory.StartNew(() =>
                        {
                            RunWebSocket();
                        }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
                        Task.Factory.StartNew(() =>
                        {
                            //RunSimulator();
                            RunFetchInventory();
                        }, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
                    }
                    break;

                case DT.SECU_ORDER_ACK_N: // 下單第二回覆，主要用來對應CNT <-> RequestID
                    PT06002 p6002 = (PT06002)package;
                    string elecID = p6002.CNT;
                    long requestId = p6002.RequestId;
                    foreach (StockViewModel model in Stocks)
                    {
                        var orders = model.Orders;
                        if (orders.ContainsKey(requestId) && !orders[requestId].ContainsKey("cnt"))
                        {
                            orders[requestId].Add("cnt", elecID);
                        }
                    }

                    foreach (StockViewModel model in PostStocks)
                    {
                        var orders = model.Orders;
                        if (orders.ContainsKey(requestId) && !orders[requestId].ContainsKey("cnt"))
                        {
                            orders[requestId].Add("cnt", elecID);
                        }
                    }
                    break;
                case DT.SECU_DEAL_RPT: //成交回報
                    PT04011 p4011 = (PT04011)package;
                    DealViewModel dealViewModel = new();
                    dealViewModel.Symbol = p4011.StockID;
                    dealViewModel.OrderID = p4011.OrderNo;
                    dealViewModel.MarketID = p4011.MarketNo;
                    dealViewModel.Side = p4011.Side == 'B' ? Config.BUY_DESC : Config.SELL_DESC;
                    dealViewModel.TradeDate = p4011.TradeDate;
                    dealViewModel.Price = p4011.Price;
                    dealViewModel.Qty = p4011.DealQty;
                    dealViewModel.ReportTime = p4011.ReportTime;

                    int orderClass = p4011.OrdClass - '0';
                    string ordClassInStr = string.Empty;

                    switch(orderClass)
                    {
                        case 0:
                            ordClassInStr = "現股";
                            break;
                        case 3:
                            ordClassInStr = "自資";
                            break;
                        case 4:
                            ordClassInStr = "自券";
                            break;
                        case 7:
                            ordClassInStr = "當沖融資";
                            break;
                        case 8:
                            ordClassInStr = "當沖融券";
                            break;
                        case 9:
                            ordClassInStr = "現先賣";
                            break;

                    }

                    dealViewModel.OrdClass = ordClassInStr;
                    DealSummary.Add(dealViewModel);

                    string orderNo = p4011.OrderNo;
                    string cntn = p4011.CNTN;

                    foreach (StockViewModel model in PostStocks)
                    {
                        var orders = model.Orders;

                        foreach (var order in orders)
                        {
                            var details = order.Value;

                            if (details.ContainsKey("cnt") && details.ContainsKey("id"))
                            {
                                if (Convert.ToString(details["cnt"]) == cntn &&
                                    Convert.ToInt32(details["id"]) == Convert.ToInt32(model.Id) &&
                                    !details.ContainsKey("old")
                                    )
                                {
                                    AddSnackQueueMsg(string.Format("隔日沖委賣{0}{1}價格{2}共{3}張成交！",
                                    model.Symbol, model.Name,
                                    p4011.Price, p4011.DealQty));

                                    var orderDetail = new Dictionary<string, object>
                                    {
                                        {"oid", Convert.ToInt32(model.Id) },
                                        {"order_func", Convert.ToString(p4011.OrderFunc)},
                                        {"price_flag", Convert.ToString(p4011.PriceFlagN)},
                                        {"side", Convert.ToString(p4011.Side) },
                                        {"order_no", p4011.OrderNo },
                                        {"request_id", order.Key },
                                        {"cntn", p4011.CNTN },
                                        {"deal_qty",  Convert.ToDouble(p4011.DealQty) },
                                        {"deal_time", Convert.ToString(p4011.ReportTime) },
                                        {"deal_avg_price", Convert.ToDouble(p4011.Price) },
                                        {"status", Config.TRANS_SELL_ORDER_DEAL },
                                        {"created_at", DateTime.UtcNow }
                                    };

                                    long insertID = InsertTransData(orderDetail);
                                    orderDetail.Add("id", insertID);
                                    model.Comm.Add(orderDetail);
                                }
                            }
                        }
                    }

                    foreach (StockViewModel model in Stocks)
                    {
                        foreach (var comm in model.Comm)
                        {
                            try
                            {
                                Dictionary<string, object> pairs = comm;

                                // 若此委託單號已存在並且匹配、並且此單非刪單
                                if (!pairs.ContainsKey("order_no") && !pairs.ContainsKey("cntn"))
                                {
                                    continue;
                                }

                                if (orderNo == Convert.ToString(pairs["order_no"]) &&
                                    cntn == Convert.ToString(pairs["cntn"]) &&
                                    Convert.ToChar(pairs["order_func"]) != 'D')
                                {
                                    int status = -1;

                                    if ((model.Status == Config.TRANS_BUY_ORDER_SENT || model.Status == Config.TRANS_BUY_ORDER_DEAL)
                                        && p4011.Side == 'B')
                                    {
                                        status = Config.TRANS_BUY_ORDER_DEAL;

                                        if (model.Status == Config.TRANS_BUY_ORDER_SENT)
                                        {
                                            UpdateOrderStatus(model, status);
                                        }

                                        if (
                                            !pairs.ContainsKey("deal_qty") ||
                                            (pairs.ContainsKey("deal_qty") && Convert.ToDouble(p4011.DealQty) != Convert.ToDouble(pairs["deal_qty"]))
                                            )
                                        {
                                            AddSnackQueueMsg(string.Format("委買{0}{1}價格{2}共{3}張成交！",
                                               model.Symbol, model.Name,
                                               p4011.Price, p4011.DealQty));
                                        }

                                    }
                                    else if ((model.Status == Config.TRANS_SELL_ORDER_SENT || model.Status == Config.TRANS_SELL_ORDER_DEAL)
                                        && p4011.Side == 'S')
                                    {

                                        status = Config.TRANS_SELL_ORDER_DEAL;

                                        if (model.Status == Config.TRANS_SELL_ORDER_SENT)
                                        {
                                            UpdateOrderStatus(model, status);
                                        }


                                        if (
                                            !pairs.ContainsKey("deal_qty") ||
                                            (pairs.ContainsKey("deal_qty") && Convert.ToDouble(p4011.DealQty) != Convert.ToDouble(pairs["deal_qty"]))
                                            )
                                        {
                                            AddSnackQueueMsg(string.Format("委賣{0}{1}價格{2}共{3}張成交！",
                                           model.Symbol, model.Name,
                                           p4011.Price, p4011.DealQty));
                                        }
                                    }

                                    DBConnection db = new();
                                    if (db.IsConnect() && pairs.ContainsKey("id"))
                                    {
                                        comm["deal_qty"] = Convert.ToDouble(p4011.DealQty);
                                        comm["deal_time"] = Convert.ToString(p4011.ReportTime);
                                        comm["deal_avg_price"] = Convert.ToDouble(p4011.Price);
                                        comm["status"] = status;

                                        db.UpdateTransData(new
                                        {
                                            id = Convert.ToInt32(pairs["id"]),
                                            deal_qty = Convert.ToDouble(p4011.DealQty),
                                            deal_time = Convert.ToString(p4011.ReportTime),
                                            deal_avg_price = Convert.ToDouble(p4011.Price),
                                            status = status
                                        });
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    }

                    break;
                case DT.SECU_ORDER_RPT: //委託回報
                    PT04010 p4010 = (PT04010)package;

                    CommViewModel commViewModel = new();
                    commViewModel.Symbol = p4010.StockID;
                    commViewModel.OrderID = p4010.OrderNo;
                    commViewModel.OrderTime = p4010.ClientOrderTime;
                    commViewModel.Side = p4010.Side == 'B' ? Config.BUY_DESC: Config.SELL_DESC;
                    commViewModel.Func = p4010.OrderFunc == 'I' ? Config.COMM_DESC : Config.DEL_DESC;
                    commViewModel.Price = Convert.ToDouble(p4010.Price) == 0 ? "市價": p4010.Price;
                    commViewModel.BeforeQty = p4010.BeforeQty;
                    commViewModel.AfterQty = p4010.AfterQty;
                    commViewModel.ReportTime = p4010.ReportTime;
                    commViewModel.ErrCode = p4010.ErrCode == "0" ? "-": p4010.ErrCode;
                    commViewModel.ErrMsg = p4010.ErrMsg.Length == 0 ? "-": p4010.ErrMsg;
                    CommSummary.Add(commViewModel);

                    bool isCommitSuc = p4010.ErrCode == "0";

                    foreach (StockViewModel model in PostStocks)
                    {
                        var orders = model.Orders;
                        long requestID = -1;
                        foreach (var order in orders)
                        {
                            var details = order.Value;
                            if (details.ContainsKey("cnt") && details.ContainsKey("id"))
                            {
                                if (Convert.ToInt32(model.Id) != Convert.ToInt32(details["id"]))
                                {
                                    continue;
                                }

                                if (details.ContainsKey("order_no") && details.ContainsKey("order_func"))
                                {
                                    if (Convert.ToString(details["order_no"]) == p4010.OrderNo &&
                                    Convert.ToChar(details["order_func"]) == 'I'
                                    )
                                    {
                                        requestID = order.Key;
                                        if (model.PrevRequestID.Contains(requestID) && p4010.OrderFunc == 'D')
                                        {
                                            model.PrevRequestID.Remove(requestID);
                                        }
                                    }
                                }
                                else
                                {
                                    details.Add("order_no", p4010.OrderNo);
                                    details.Add("order_func", p4010.OrderFunc);
                                }
                            }
                        }

                    }

                    foreach (StockViewModel model in Stocks)
                    {
                        var orders = model.Orders;
                        bool isContainedOrder = false;
                        long requestID = -1;

                        foreach (var order in orders)
                        {
                            var details = order.Value;
                            if (details.ContainsKey("cnt") && details.ContainsKey("id") && !details.ContainsKey("suc"))
                            {
                                if (Convert.ToString(details["cnt"]) == p4010.CNTN &&
                                    Convert.ToInt32(model.Id) == Convert.ToInt32(details["id"]))
                                {
                                    isContainedOrder = true;
                                    requestID = order.Key;
                                    details.Add("suc", isCommitSuc);
                                }
                            }
                        }

                        if (isContainedOrder)
                        {
                            string orderTime = Convert.ToString(p4010.ClientOrderTime);
                            char orderFunc = p4010.OrderFunc;
                            char side = p4010.Side;
                            string price = p4010.PriceFlagN == '1' ? "市價" : p4010.Price;
                            int status = 0;

                            if (side == 'B')
                            {
                                if (orderFunc == 'I')
                                {
                                    if (isCommitSuc)
                                    {
                                        status = Config.TRANS_BUY_ORDER_SENT;
                                        AddSnackQueueMsg(string.Format("委買{1}{2}價格{3}共{4}張成功！",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime));
                                    }
                                    else
                                    {
                                        status = Config.TRANS_BUY_ORDER_FAILED;
                                        AddSnackQueueMsg(string.Format("委買{1}{2}價格{3}共{4}張失敗！原因：{6}",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime, p4010.ErrMsg));
                                    }
                                }
                                else if (orderFunc == 'D')
                                {
                                    if (isCommitSuc)
                                    {
                                        status = Config.TRANS_BUY_CANCEL_SUC;
                                        AddSnackQueueMsg(string.Format("刪除委買單{1}{2}價格{3}共{4}張成功！",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime));
                                    }
                                    else
                                    {
                                        status = Config.TRANS_BUY_CANCEL_FAILED;
                                        AddSnackQueueMsg(string.Format("刪除委買單{1}{2}價格{3}共{4}張失敗！原因：{6}",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime, p4010.ErrMsg));
                                    }
                                }
                            }
                            else
                            {
                                if (orderFunc == 'I')
                                {
                                    if (isCommitSuc)
                                    {
                                        status = Config.TRANS_SELL_ORDER_SENT;
                                        AddSnackQueueMsg(string.Format("委賣{1}{2}價格{3}共{4}張成功！",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime));
                                    }
                                    else
                                    {
                                        status = Config.TRANS_SELL_ORDER_FAILED;
                                        AddSnackQueueMsg(string.Format("委賣{1}{2}價格{3}共{4}張失敗！原因：{6}",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime, p4010.ErrMsg));
                                    }
                                }
                                else if (orderFunc == 'D')
                                {
                                    if (isCommitSuc)
                                    {
                                        status = Config.TRANS_SELL_CANCEL_SUC;
                                        AddSnackQueueMsg(string.Format("刪除委賣單{1}{2}價格{3}共{4}張成功！",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime));
                                    }
                                    else
                                    {
                                        status = Config.TRANS_SELL_CANCEL_FAILED;
                                        AddSnackQueueMsg(string.Format("刪除委賣單{1}{2}價格{3}共{4}張失敗！原因：{6}",
                                            p4010.OrderNo, model.Symbol, model.Name,
                                            price, p4010.Qty, orderTime, p4010.ErrMsg));
                                    }
                                }
                            }

                            var orderDetail = new Dictionary<string, object>
                            {
                                {"oid", Convert.ToInt32(model.Id) },
                                {"qty", Convert.ToDouble(p4010.Qty)},
                                {"before_qty", Convert.ToDouble(p4010.BeforeQty)},
                                {"after_qty", Convert.ToDouble(p4010.AfterQty)},
                                {"price", Convert.ToDouble(p4010.Price)},
                                {"order_func", Convert.ToString(p4010.OrderFunc)},
                                {"price_flag", Convert.ToString(p4010.PriceFlagN)},
                                {"side", Convert.ToString(p4010.Side) },
                                {"order_no", p4010.OrderNo},
                                {"order_time", orderTime },
                                {"request_id", requestID },
                                {"cntn", p4010.CNTN },
                                {"err_code", p4010.ErrCode },
                                {"err_msg", p4010.ErrMsg},
                                {"status", status },
                                {"created_at", DateTime.UtcNow }
                            };

                            long insertID = InsertTransData(orderDetail);
                            orderDetail.Add("id", insertID);
                            model.Comm.Add(orderDetail);

                            if (model.BuyCommFinish)
                            {
                                model.BuyCommFinish = false;
                                bool isSuc = false;

                                // 檢查此model下的委託單，如果有任何一筆成功，則此單代表委託成功
                                foreach (var order in orders)
                                {
                                    var details = order.Value;

                                    if (details.ContainsKey("suc") && Convert.ToBoolean(details["suc"]))
                                    {
                                        isSuc = true;
                                        break;
                                    }
                                }

                                UpdateOrderStatus(model, isSuc ? Config.TRANS_BUY_ORDER_SENT : Config.TRANS_BUY_ORDER_FAILED);
                            }

                            if (model.SellCommFinish)
                            {
                                model.SellCommFinish = false;
                                bool isSuc = false;

                                // 檢查此model下的委託單，如果有任何一筆成功，則此單代表委託成功
                                foreach (var order in orders)
                                {
                                    var details = order.Value;

                                    if (details.ContainsKey("suc") && Convert.ToBoolean(details["suc"]))
                                    {
                                        isSuc = true;
                                        break;
                                    }
                                }

                                UpdateOrderStatus(model, isSuc ? Config.TRANS_SELL_ORDER_SENT : Config.TRANS_SELL_ORDER_FAILED);
                            }

                            if (model.CancelCommFinish)
                            {
                                model.CancelCommFinish = false;
                                bool isSuc = false;

                                // 檢查此model下的委託單，如果有任何一筆取消成功，則此單代表取消成功
                                foreach (var order in orders)
                                {
                                    var details = order.Value;

                                    if (details.ContainsKey("suc") && Convert.ToBoolean(details["suc"]))
                                    {
                                        isSuc = true;
                                        break;
                                    }
                                }

                                UpdateCancelStatus(model, isSuc ? Config.CANCEL_ORDER_SUC : Config.CANCEL_ORDER_FAILED, status);
                            }
                        }

                    }

                    break;
                case DT.FINANCIAL_WSINVENTORYSUM:
                    //證券庫存彙總查詢  
                    //庫存資料量大, 分多筆送回, 故須自行判斷是否傳送完畢(NowCount = TotalCount) ; 
                    //須注意, 若同一時間查詢多次時, 會造成封包穿插送回, 資料會異常
                    try
                    {
                        PT04310 p4310 = (PT04310)package;

                        if (p4310.NowCount == 1)
                        {
                            repoMaps.Clear();
                            RepoSummary.Clear();
                            repoObjects.Clear();
                        }
                        if (p4310.Code != 0)
                        {
                            return;
                        }

                        foreach (var detail in p4310.Detail)
                        {
                            // 檢查是否為同一帳號與公司分行
                            if (detail.BrokerID != brokerID || detail.Account != accountID) continue;

                            if (dtMaps.ContainsKey(detail.Symbol))
                            {
                                dtMaps[detail.Symbol] = Convert.ToInt32(detail.IMATQTY0) - Convert.ToInt32(detail.OMATQTY0);
                            }
                            else
                            {
                                dtMaps.Add(detail.Symbol, Convert.ToInt32(detail.IMATQTY0) - Convert.ToInt32(detail.OMATQTY0));
                            }

                            if (Config.DEBUG)
                            {
                                if (repoMaps.ContainsKey(detail.Symbol))
                                {
                                    repoMaps[detail.Symbol] = new Dictionary<string, object>
                                {
                                    { "leftQty", Convert.ToInt32(detail.IMATQTY0) },
                                    { "dealQty", Convert.ToInt32(detail.OMATQTY0) },
                                    { "commQty", Convert.ToInt32(detail.OORDQTY0) },
                                };
                                }
                                else
                                {
                                    repoMaps.Add(detail.Symbol, new Dictionary<string, object>
                                {
                                    { "leftQty", Convert.ToInt32(detail.IMATQTY0) },
                                    { "dealQty", Convert.ToInt32(detail.OMATQTY0) },
                                    { "commQty", Convert.ToInt32(detail.OORDQTY0) },
                                });
                                }
                            }
                            else
                            {
                                if (repoMaps.ContainsKey(detail.Symbol))
                                {
                                    repoMaps[detail.Symbol] = new Dictionary<string, object>
                                {
                                    { "leftQty", Convert.ToInt32(detail.RQTY0) },
                                    { "dealQty", Convert.ToInt32(detail.OMATQTY0) },
                                    { "commQty", Convert.ToInt32(detail.OORDQTY0) },
                                };
                                }
                                else
                                {
                                    repoMaps.Add(detail.Symbol, new Dictionary<string, object>
                                {
                                    { "leftQty", Convert.ToInt32(detail.RQTY0) },
                                    { "dealQty", Convert.ToInt32(detail.OMATQTY0) },
                                    { "commQty", Convert.ToInt32(detail.OORDQTY0) },
                                });
                                }
                            }

                            try
                            {
                                RepoSummary.Add(new RepoViewModel
                                {
                                    Symbol = Convert.ToString(detail.Symbol),
                                    Name = Convert.ToString(detail.SymbolName),
                                    PastLeft = Convert.ToInt32(detail.RQTY0),
                                    AddComm = Convert.ToInt32(detail.IORDQTY),
                                    AddDeal = Convert.ToInt32(detail.IMATQTY0),
                                    SellComm = Convert.ToInt32(detail.OORDQTY0),
                                    SellDeal = Convert.ToInt32(detail.OMATQTY0),
                                    DtQty = detail.DTQTY0,
                                    Asset = detail.ASSET,
                                    Netpl = detail.NETPL,
                                    OavgPrice = detail.OAVGPRICE0,
                                    AssetReal = detail.ASSETREAL,
                                    Rlprice = detail.RLPRICE
                                });

                                string tableID = string.Format("{0}_{1}_{2}",
                                    MainWindow.encryptUID,
                                    DateTime.UtcNow.ToString("yyyyMMdd"),
                                    Convert.ToString(detail.Symbol)
                                    );

                                repoObjects.Add(new Dictionary<string, string>
                            {
                                {"id", tableID },
                                {"uid", MainWindow.encryptUID },
                                {"symbol", Convert.ToString(detail.Symbol) },
                                {"past_left", Convert.ToString(detail.RQTY0) },
                                {"add_comm", Convert.ToString(detail.IORDQTY) },
                                {"add_deal", Convert.ToString(detail.IMATQTY0) },
                                {"sell_comm", Convert.ToString(detail.OMATQTY0) },
                                {"sell_deal", Convert.ToString(detail.RQTY0) },
                                {"asset", Convert.ToString(detail.ASSET) },
                                {"oavg_price", Convert.ToString(detail.OAVGPRICE0) },
                                {"netpl", Convert.ToString(detail.NETPL) },
                                {"env", Convert.ToString(Config.DEBUG? "dev": "prod") }
                            });
                            }
                            catch (Exception)
                            {
                            }

                            break;

                        }

                        // 檢查是否資料已回傳完畢，如果NowCount == TotalCount 才做後續的掛單、出場演算法
                        if (p4310.NowCount != p4310.TotalCount)
                        {
                            break;
                        }
                        else
                        {
                            try
                            {
                                if (dbConnection.IsConnect())
                                {
                                    dbConnection.InsertRepoData(repoObjects);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }

                        foreach (StockViewModel model in PostStocks)
                        {
                            if (!repoMaps.ContainsKey(model.Symbol))
                            {
                                continue;
                            }

                            string symbol = model.Symbol;
                            int modelID = Convert.ToInt32(model.Id);
                            int repoLeft = Convert.ToInt32(repoMaps[symbol]["leftQty"]) -
                            Convert.ToInt32(repoMaps[symbol]["dealQty"]);
                            model.RepoLeft = repoLeft;
                            model.CommSell = Convert.ToInt32(repoMaps[symbol]["commQty"]);
                            List<Dictionary<string, object>> comms = model.Comm;
                            model.AvgSell = 0;
                            int count = 0;
                            double total = 0;

                            foreach (var comm in comms)
                            {
                                if (comm.ContainsKey("deal_qty") && comm.ContainsKey("deal_avg_price"))
                                {
                                    count += Convert.ToInt32(comm["deal_qty"]);
                                    total += Convert.ToInt32(comm["deal_qty"]) * Convert.ToDouble(comm["deal_avg_price"]);
                                }
                            }

                            if (count > 0)
                            {
                                model.AvgSell = Math.Round(total / count, 2);
                            }

                            DateTime now = DateTime.UtcNow;
                            string todayDate = now.ToString("yyyyMMdd");
                            double ts = now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                            // 若非當天隔日沖單
                            if (todayDate != model.TradeDate) continue;

                            // 若是當天隔日沖單，但已過期 (若未9點前開啟本程式皆算過期!)
                            if (ts % Config.TOTAL_SECONDS_ONEDAY > Config.TWSTOCK_START_TIME &&
                                !Convert.ToBoolean(model.CheckPostExpired) &&
                                Convert.ToInt32(model.Status) == 0)
                            {
                                model.CheckPostExpired = true;

                                if (!invalidOrders.Contains(modelID))
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        AddSnackQueueMsg(string.Format("隔日沖單編號:{0} 標的{1}，因已過開盤時段(AM 9:00)，此單自動失效！",
                                            model.Id,
                                            model.Symbol
                                            ));
                                    });
                                    invalidOrders.Add(modelID);
                                }


                                // 測試階段mark底下continue
                                if (!Config.DEBUG)
                                {
                                    continue;
                                }
                            }

                            bool isOpenLeave = model.OpenLeave != null;

                            // 進入開盤階段
                            bool isValidTimer = false;

                            if (Config.DEBUG)
                            {
                                isValidTimer = ts % Config.TOTAL_SECONDS_ONEDAY >= Config.TWSTOCK_START_TIME;
                            }
                            else
                            {
                                isValidTimer = ts % Config.TOTAL_SECONDS_ONEDAY >= Config.TWSTOCK_START_TIME
                                && ts % Config.TOTAL_SECONDS_ONEDAY < Config.TWSTOCK_END_TIME;
                            }

                            if (isValidTimer)
                            {
                                if (!model.TransLock &&
                                    (model.Status == Config.TRANS_INIT ||
                                    model.Status == Config.TRANS_SELL_ORDER_SENT ||
                                    model.Status == Config.TRANS_SELL_ORDER_DEAL))
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        model.TransLock = true;

                                        try
                                        {
                                            if (!model.Close.HasValue ||
                                                !model.PreClose.HasValue ||
                                                !model.Highest.HasValue)
                                            {
                                                throw new Exception(string.Format("Error Quotes for {0}", model.Symbol));
                                            }

                                            double curPrice = Convert.ToDouble(model.Close);
                                            double preClose = Convert.ToDouble(model.PreClose);
                                            double ampPert = 100 * ((curPrice / preClose) - 1);
                                            double highest = Convert.ToDouble(model.Highest);

                                            if (repoLeft <= 0)
                                            {
                                                UpdateOrderStatus(model, Config.TRANS_SELL_ORDER_DEAL);
                                                throw new Exception(string.Format("Finish sell for {0}", model.Symbol));
                                            }


                                            if (model.Status == Config.TRANS_INIT)
                                            {
                                                int pendCount = 0;
                                                if (isOpenLeave)
                                                {
                                                    Console.WriteLine("開盤出 {0}", model.OpenLeave);
                                                    // 開盤就先出場指定%數
                                                    pendCount = Convert.ToInt32(
                                                        Math.Ceiling(Convert.ToDouble(repoLeft * model.OpenLeave / 100))
                                                        );
                                                }
                                                else
                                                {
                                                    // 開盤後看漲跌幅狀況出場指定%數
                                                    if (ampPert < model.OpenAmp)
                                                    {
                                                        Console.WriteLine("開盤看情況出 {0},{1}", model.OpenAmp, model.OpenBelowLeave);

                                                        pendCount = Convert.ToInt32(
                                                            Math.Ceiling(Convert.ToDouble(repoLeft * model.OpenBelowLeave / 100))
                                                            );
                                                    }
                                                }

                                                long requestId = tfcom.GetRequestId();
                                                Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
                                                Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                                Security_Class sc = Security_Class.SC_Ordinary;
                                                Security_PriceFlag sp = Security_PriceFlag.SP_MarketPrice;
                                                SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
                                                TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

                                                long ret = tfcom.SecurityOrder(requestId,
                                                    ot,
                                                    sl,
                                                    sc,
                                                    brokerID,
                                                    accountID,
                                                    symbol,
                                                    sf,
                                                    Convert.ToUInt16(pendCount),
                                                    0,
                                                    sp,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    TIF
                                                    );

                                                repoLeft -= pendCount;
                                                UpdateOrderStatus(model, Config.TRANS_SELL_ORDER_SENT);
                                            }

                                            Console.WriteLine("剩餘張數為 {0}, 當前狀態為 {1}, 前一送單ID數 {2}", repoLeft, model.Status, model.PrevRequestID.Count);

                                            model.Cycle += 1;
                                            if (modelMaps.ContainsKey(modelID))
                                            {
                                                modelMaps[modelID]["cycle"] = model.Cycle;
                                            }
                                            else
                                            {
                                                modelMaps.Add(modelID, new Dictionary<string, object>
                                            {
                                                { "cycle", model.Cycle },
                                            });
                                            }

                                            if (model.Cycle % Convert.ToInt32(model.DelayTime / Config.DELAY) == 0)
                                            {
                                                model.Cycle = 0;
                                            }
                                            else
                                            {
                                                throw new Exception(string.Format("Current cycle {0} for {1}", model.Cycle, model.Symbol));
                                            }

                                            // 若先前已送出委託單，先取消單後再重新掛單
                                            if (model.PrevRequestID.Count > 0)
                                            {
                                                var preRequestIDs = model.PrevRequestID;
                                                foreach (long preRequestID in preRequestIDs)
                                                {
                                                    string orderNo = string.Empty;

                                                    if (model.Orders.ContainsKey(preRequestID) &&
                                                    model.Orders[preRequestID].ContainsKey("order_no"))
                                                    {
                                                        orderNo = Convert.ToString(model.Orders[preRequestID]["order_no"]);
                                                    }

                                                    // 取消上次掛賣
                                                    long requestId = tfcom.GetRequestId();
                                                    Security_OrdType ot = Security_OrdType.OT_CANCEL; // 取消單
                                                    Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                                    Security_Class sc = Security_Class.SC_Ordinary;
                                                    Security_PriceFlag sp = Security_PriceFlag.SP_FixedPrice;
                                                    SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
                                                    TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

                                                    long ret = tfcom.SecurityOrder(requestId,
                                                        ot,
                                                        sl,
                                                        sc,
                                                        brokerID,
                                                        accountID,
                                                        Convert.ToString(model.Symbol),
                                                        sf,
                                                        0,
                                                        0,
                                                        sp,
                                                        string.Empty,
                                                        string.Empty,
                                                        orderNo,
                                                        TIF
                                                        );
                                                    Console.WriteLine("取消 單號{0}, 結果{1}", orderNo, ret);
                                                }

                                                int retry = 10;

                                                // 等全部單都取消完成，再進行下次掛單，最多等待1秒時間
                                                while (--retry > 0)
                                                {
                                                    if (model.PrevRequestID.Count == 0)
                                                    {
                                                        break;
                                                    }
                                                    Thread.Sleep(100);
                                                }
                                            }

                                            // 若跌幅超過強制出場%數，則全數皆掛市價單出場，否則就進行一般性掛單

                                            if (ampPert < -1 * model.ForceLeave)
                                            {
                                                long requestId = tfcom.GetRequestId();
                                                Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
                                                Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                                Security_Class sc = Security_Class.SC_Ordinary;
                                                Security_PriceFlag sp = Security_PriceFlag.SP_MarketPrice;
                                                SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
                                                TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

                                                long ret = tfcom.SecurityOrder(requestId,
                                                    ot,
                                                    sl,
                                                    sc,
                                                    brokerID,
                                                    accountID,
                                                    symbol,
                                                    sf,
                                                    Convert.ToUInt16(repoLeft),
                                                    0,
                                                    sp,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    TIF
                                                    );
                                            }
                                            else
                                            {
                                                int tmpRepoLeft = repoLeft;
                                                string[] ticks = model.Ticks.Split(',');

                                                //Attention
                                                int tickDiff = 1;
                                                if (Config.DEBUG)
                                                {
                                                    tickDiff = 0;
                                                }

                                                foreach (string tick in ticks)
                                                {
                                                    int tickPert = Convert.ToInt32(tick);
                                                    double calcPrice = PriceTickCalc(curPrice, tickDiff, false);
                                                    int pendCount = Convert.ToInt32(Math.Ceiling(repoLeft * tickPert / 100.0));
                                                    tmpRepoLeft -= pendCount;

                                                    if (tmpRepoLeft < 0)
                                                    {
                                                        break;
                                                    }

                                                    if (calcPrice > highest)
                                                    {
                                                        continue;
                                                    }

                                                    // 若計算之掛單量大於0的情況，則掛賣指定價格之數量
                                                    if (pendCount > 0)
                                                    {
                                                        long requestId = tfcom.GetRequestId();
                                                        model.PrevRequestID.Add(requestId);

                                                        model.Orders.Add(requestId, new Dictionary<string, object>
                                                    {
                                                        { "id", Convert.ToInt32(model.Id) },
                                                        { "qty", pendCount },
                                                        { "price", calcPrice },
                                                    });

                                                        Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
                                                        Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                                        Security_Class sc = Security_Class.SC_Ordinary;
                                                        Security_PriceFlag sp = Security_PriceFlag.SP_FixedPrice;
                                                        SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
                                                        TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

                                                        long ret = tfcom.SecurityOrder(requestId,
                                                            ot,
                                                            sl,
                                                            sc,
                                                            brokerID,
                                                            accountID,
                                                            symbol,
                                                            sf,
                                                            Convert.ToUInt16(pendCount),
                                                            decimal.Parse(Convert.ToString(calcPrice)),
                                                            sp,
                                                            string.Empty,
                                                            string.Empty,
                                                            string.Empty,
                                                            TIF
                                                            );
                                                        Console.WriteLine("掛賣 {0}張 價格{1} 結果{2} 全部{3}", pendCount, calcPrice, ret, repoLeft);

                                                    }

                                                    ++tickDiff;
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }

                                        model.TransLock = false;
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
            }
        }

        private void AddSnackQueueMsg(string msg, int duration = 300)
        {
            Dispatcher.Invoke(() =>
            {
                if (SnackQueueBottom.MessageQueue is { } messageQueue)
                {
                    messageQueue.Enqueue(
                    msg,
                    "OK",
                    param => { },
                    msg,
                    true,
                    false,
                    TimeSpan.FromSeconds(duration));
                }
            });
        }

        /* 
         * 需要隨機新增賣單操作，可跑此方法
         */
        private void RunSimulator()
        {
            Thread.Sleep(2000);
            long requestId = tfcom.GetRequestId();
            Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
            Security_Lot sl = Security_Lot.Even_Lot; // 整股
            Security_Class sc = Security_Class.SC_Ordinary;
            Security_PriceFlag sp = Security_PriceFlag.SP_MarketPrice;
            SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
            TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

            long ret = tfcom.SecurityOrder(requestId,
                ot,
                sl,
                sc,
                brokerID,
                accountID,
                "8261",
                sf,
                10,
                decimal.Parse(Convert.ToString("116.5")),
                sp,
                string.Empty,
                string.Empty,
                string.Empty,
                TIF
                );
            Console.WriteLine("模擬賣出 {0}", ret);
        }

        private void RunFetchInventory()
        {
            while (true)
            {
                repoMaps.Clear();
                long ret = tfcom.RetrieveWsInventorySum("A", brokerID, accountID, string.Empty);
                //Console.WriteLine("庫存請求延遲為:{0}秒，結果為:{1}", Config.DELAY, ret);
                Thread.Sleep(Config.DELAY * 1000);
            }
        }

        private void RunWebSocket()
        {
            var ws = new WebSocket(Config.WS_HOST);

            while (true)
            {
                try
                {
                    ws.Connect();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("WS Connection error {0}", e.Message);
                    Thread.Sleep(5000);
                }

            }

            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("------------------- WS error {0} -------------------", e.Message);
            };

            ws.OnMessage += (sender, e) =>
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        /*
                         * 回應的結構為：
                         * {
                         *     "code": <code>,
                         *     "response": 
                         *     {
                         *         "originReq":
                         *         {
                         *             "symbol": <symbol>,
                         *             "period": <period>,
                         *             "source": <source>
                         *         },
                         *         ...
                         *     }
                         * }
                         */
                        JObject results = (JObject)JsonConvert.DeserializeObject(e.Data);
                        JToken response = results["response"];
                        JToken originReq = response["originReq"];
                        string period = Convert.ToString(originReq["period"]);
                        JToken symbolNumber = originReq["symbol"];

                        if (period == string.Empty)
                        {
                            JToken detail = response["detail"];

                            foreach (StockViewModel model in Stocks)
                            {
                                // 取得即時報價明細，更新列表報價UI
                                if (Convert.ToString(symbolNumber) == model.Symbol)
                                {
                                    UpdateModelQuote(detail, model);

                                }
                            }

                            foreach (StockViewModel model in PostStocks)
                            {
                                // 取得即時報價明細，更新列表報價UI
                                if (Convert.ToString(symbolNumber) == model.Symbol)
                                {
                                    UpdateModelQuote(detail, model);

                                }
                            }
                        }
                        else if (period == "book")
                        {
                            var orders = response["orders"];
                            IEnumerable enumerable = orders;

                            double bidFirstPrice = -1;
                            int bidFirstSize = -1;
                            double bidSecondPrice = -1;
                            int bidSecondSize = -1;

                            // 取得第一、二檔委託價格與量
                            foreach (var item in orders)
                            {
                                int side = Convert.ToInt32(item["side"]);
                                int position = Convert.ToInt32(item["position"]);

                                if (side == 1 && position == 1)
                                {
                                    bidFirstPrice = Convert.ToDouble(item["price"]);
                                    bidFirstSize = Convert.ToInt32(item["size"]);
                                }
                                else if (side == 1 && position == 2)
                                {
                                    bidSecondPrice = Convert.ToDouble(item["price"]);
                                    bidSecondSize = Convert.ToInt32(item["size"]);
                                }
                            }

                            foreach (StockViewModel model in Stocks)
                            {
                                DateTime now = DateTime.UtcNow;
                                DateTime createdAt = Convert.ToDateTime(model.CreatedAt);
                                string nowFmt = now.ToString("yyyyMMdd");
                                string createdAtFmt = createdAt.ToString("yyyyMMdd");

                                // 若非當日單，則直接忽略
                                if (nowFmt != createdAtFmt)
                                {
                                    continue;
                                }

                                // 假設還尚未取得漲停價，就等到有取值才跳出while loop
                                int retryCount = 100;
                                while (!model.Highest.HasValue && --retryCount > 0)
                                {
                                    Thread.Sleep(50);
                                    continue;
                                }

                                int status = Convert.ToInt32(model.Status);
                                int entryOption = Convert.ToInt32(model.EntryOption);
                                double highest = Convert.ToDouble(model.Highest);
                                int entryValue = Convert.ToInt32(model.EntryValue);
                                int leaveCancel = Convert.ToInt32(model.LeaveCancel);
                                int leaveSell = Convert.ToInt32(model.LeaveSell);
                                bool canEntry = false;
                                bool canCancel = false;

                                /* 
                                 * ============== 委託漲停或市價單加總與計算 ==============
                                 */
                                if (Convert.ToString(symbolNumber) == model.Symbol)
                                {
                                    if (bidFirstPrice == highest || bidFirstPrice == 0)
                                    {
                                        if (bidFirstPrice == highest)
                                        {
                                            model.CurUpperBid = bidFirstSize;
                                        }
                                        else if (bidFirstPrice == 0 && bidSecondPrice == highest)
                                        {
                                            model.CurUpperBid = bidFirstSize + bidSecondSize;
                                        }

                                        model.MaxUpperBid = Math.Max((int)model.MaxUpperBid, (int)model.CurUpperBid);
                                    }
                                    else
                                    {
                                        model.CurUpperBid = 0;
                                    }

                                    // Cache current book data
                                    string symbol = model.Symbol;
                                    var bookData = new Dictionary<string, int>
                                    {
                                        { "curUpperBid", Convert.ToInt32(model.CurUpperBid) },
                                        { "maxUpperBid", Convert.ToInt32(model.MaxUpperBid) }
                                    };

                                    bookMaps[symbol] = bookData;
                                }
                                else
                                {
                                    continue;
                                }


                                #region 進場條件判斷
                                /* 
                                 * ============== 進場條件判斷 ==============
                                 * 若尚未進場 且
                                 * 目前委買第一檔價格等於漲停價 或第一檔價格等於市價 且 
                                 * 此委託簿股票代碼一致的情況
                                 */

                                if (status == Config.TRANS_INIT && (bidFirstPrice == highest || bidFirstPrice == 0) && Convert.ToString(symbolNumber) == model.Symbol)
                                {
                                    // 0: 漲停價委買張數 1: 市值 2: 漲停變市價單
                                    if (entryOption == 0)
                                    {
                                        if (bidFirstSize > entryValue)
                                        {
                                            canEntry = true;
                                        }
                                    }
                                    else if (entryOption == 1)
                                    {
                                        double capital = bidFirstPrice * bidFirstSize;
                                        if (capital > entryValue)
                                        {
                                            canEntry = true;
                                        }
                                    }
                                    else if (entryOption == 2)
                                    {
                                        int entryOpenRiseStatus = Convert.ToInt32(model.EntryOpenRiseStatus);
                                        /*
                                         * 如果當前委買第一筆為漲停價，則<EntryOpenRiseStatus> 從0 -> 1
                                         * 如果當前委買第一筆為市價且<EntryOpenRiseStatus>為1，則1 -> 2
                                         */
                                        if (entryOpenRiseStatus == 0 && (bidFirstPrice == highest || bidFirstPrice == 0))
                                        {
                                            model.EntryOpenRiseStatus = 1;
                                            /*
                                             * !!!!!!!!!!! debug usage !!!!!!!!!!!!!
                                             * canEntry = true;
                                             * 
                                             */
                                            canEntry = true;
                                        }
                                        else if (entryOpenRiseStatus == 1 && bidFirstPrice == 0)
                                        {
                                            model.EntryOpenRiseStatus = 2;
                                            canEntry = true;
                                        }

                                    }
                                }

                                // 如果已有執行緒進到critical section, 則不得重複進場!
                                if (canEntry && !model.TransLock)
                                {
                                    model.TransLock = true;
                                    int entryOrderOption = Convert.ToInt32(model.EntryOrderOption);
                                    long entryTotalPrice = Convert.ToInt64(model.EntryTotalPrice) * 10000;
                                    int entryCount = -1;
                                    double entryOrderPrice = 0;
                                    bool isCommSuc = false;

                                    if (entryOrderOption == 0)
                                    {
                                        entryCount = Convert.ToInt32(Math.Floor(entryTotalPrice / (1000 * highest)));
                                    }
                                    else
                                    {
                                        entryOrderPrice = PriceTickCalc(highest, Convert.ToInt32(model.EntryOrderPrice));
                                        entryCount = Convert.ToInt32(Math.Floor(entryTotalPrice / (1000 * entryOrderPrice)));
                                    }

                                    while (entryCount > 0)
                                    {
                                        long requestId = tfcom.GetRequestId();
                                        Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
                                        Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                        Security_Class sc = Security_Class.SC_Ordinary;
                                        Security_PriceFlag sp = entryOrderOption == 0 ? Security_PriceFlag.SP_MarketPrice : Security_PriceFlag.SP_FixedPrice;
                                        SIDE_FLAG sf = SIDE_FLAG.SF_BUY;
                                        TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;
                                        ushort curOrderCount = Convert.ToUInt16(Math.Min(Config.MAX_ORDER_COUNT, entryCount));

                                        //Console.WriteLine(string.Format("準備送出委託:{0}共{1}張", requestId, curOrderCount));

                                        model.Orders.Add(requestId, new Dictionary<string, object>
                                        {
                                            { "id", Convert.ToInt32(model.Id) }
                                        });

                                        long ret = tfcom.SecurityOrder(requestId,
                                            ot,
                                            sl,
                                            sc,
                                            brokerID,
                                            accountID,
                                            Convert.ToString(model.Symbol),
                                            sf,
                                            curOrderCount,
                                            decimal.Parse(Convert.ToString(entryOrderPrice)),
                                            sp,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            TIF
                                            );

                                        /*
                                         * ret 為0僅代表委託單送出，但是否真正委託成功，必須在Receive Handler內去做判斷
                                         * 若任一委託送出成功，則判定為成功
                                         */
                                        if (ret == 0) isCommSuc = true;
                                        entryCount -= Convert.ToInt16(Config.MAX_ORDER_COUNT);

                                        if (entryCount <= 0)
                                        {
                                            UpdateOrderStatus(model,
                                                isCommSuc ? Config.TRANS_BUY_ORDER_SENT : Config.TRANS_BUY_ORDER_FAILED);
                                            model.BuyCommFinish = true;
                                        }

                                        if (ret != 0)
                                        {
                                            var orderDetail = new Dictionary<string, object>
                                            {
                                                { "oid", model.Id },
                                                { "price", entryOrderPrice },
                                                { "qty", curOrderCount },
                                                { "request_id", requestId },
                                                { "err_msg",  tfcom.GetOrderErrMsg(ret) },
                                                { "status",  Config.TRANS_BUY_ORDER_FAILED },
                                                { "created_at",  DateTime.UtcNow }
                                            };

                                            long insertID = InsertTransData(orderDetail);
                                            orderDetail.Add("id", insertID);
                                            model.Comm.Add(orderDetail);

                                            AddSnackQueueMsg(string.Format("未成功送出委託{0}{1} 買進價{2} 共{3}張之請求，原因:{4}",
                                                model.Symbol,
                                                model.Name,
                                                entryOrderOption == 0 ? "市價" : entryOrderPrice,
                                                curOrderCount,
                                                tfcom.GetOrderErrMsg(ret)
                                                ));
                                        }
                                    }

                                    model.TransLock = false;
                                }
                                #endregion

                                #region 取消條件判斷
                                /* 
                                 * ============== 取消條件判斷 ==============
                                 * 若非新建單 且
                                 * 當前委買量 < 取消%數 * 最大委買量
                                 */
                                status = Convert.ToInt32(model.Status);
                                if (status != Config.TRANS_INIT && model.CancelStatus == 0)
                                {
                                    if (model.CurUpperBid <= leaveCancel * model.MaxUpperBid / 100)
                                    {
                                        canCancel = true;
                                    }
                                }

                                // 如果已有執行緒進到critical section, 則不得重複取消!
                                //Console.WriteLine(string.Format("是否能取消: {0}", canCancel));
                                if (canCancel && !model.TransLock)
                                {
                                    model.TransLock = true;
                                    // To-do 取得當前未成交之委託買單，並將所有委買掛單全數取消
                                    var comms = model.Comm;
                                    ushort entryCount = 0;
                                    double entryOrderPrice = -1;
                                    string orderNo = string.Empty;
                                    bool isCancelSuc = false;

                                    foreach (var comm in comms)
                                    {
                                        try
                                        {
                                            Dictionary<string, object> pairs = comm;

                                            if (Convert.ToChar(pairs["side"]) == 'B')
                                            {
                                                entryCount = Convert.ToUInt16(pairs["qty"]);
                                                orderNo = Convert.ToString(pairs["order_no"]);
                                                entryOrderPrice = Convert.ToDouble(pairs["price"]);

                                                Console.WriteLine(string.Format("取消張數為:{0}", entryCount));

                                                if (entryCount > 0)
                                                {
                                                    long requestId = tfcom.GetRequestId();
                                                    Security_OrdType ot = Security_OrdType.OT_CANCEL; // 取消單
                                                    Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                                    Security_Class sc = Security_Class.SC_Ordinary;
                                                    Security_PriceFlag sp = Security_PriceFlag.SP_FixedPrice;
                                                    SIDE_FLAG sf = SIDE_FLAG.SF_BUY;
                                                    TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;

                                                    model.Orders.Add(requestId, new Dictionary<string, object>
                                                    {
                                                        { "id", Convert.ToInt32(model.Id) }
                                                    });

                                                    long ret = tfcom.SecurityOrder(requestId,
                                                        ot,
                                                        sl,
                                                        sc,
                                                        brokerID,
                                                        accountID,
                                                        Convert.ToString(model.Symbol),
                                                        sf,
                                                        entryCount,
                                                        decimal.Parse(Convert.ToString(entryOrderPrice)),
                                                        sp,
                                                        string.Empty,
                                                        string.Empty,
                                                        orderNo,
                                                        TIF
                                                        );

                                                    if (ret != 0)
                                                    {
                                                        AddSnackQueueMsg(string.Format("取消{0}共{1}張失敗!原因:{2}", model.Symbol, entryCount, tfcom.GetOrderErrMsg(ret)));
                                                    }
                                                    else
                                                    {
                                                        isCancelSuc = true;
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }


                                    }

                                    UpdateCancelStatus(model, isCancelSuc ? Config.CANCEL_ORDER_SUC : Config.CANCEL_ORDER_FAILED, Config.TRANS_BUY_CANCEL_FAILED);
                                    model.CancelCommFinish = true;
                                    model.TransLock = false;
                                }
                                #endregion
                            }
                        }
                        else if (period == "details")
                        {
                            var items = response["items"];
                            foreach (StockViewModel model in Stocks)
                            {
                                #region 出場條件判斷

                                DateTime now = DateTime.UtcNow;
                                DateTime createdAt = Convert.ToDateTime(model.CreatedAt);
                                string nowFmt = now.ToString("yyyyMMdd");
                                string createdAtFmt = createdAt.ToString("yyyyMMdd");

                                // 若非當日單，則直接忽略
                                if (nowFmt != createdAtFmt)
                                {
                                    continue;
                                }

                                /*
                                 * 若此明細股票代碼一致，將委託簿寫入model.Details
                                 * 資料結構為：
                                 * ["<timestamp> <price> <buy> <sell> <single> <volume>", ...]
                                 */
                                int status = Convert.ToInt32(model.Status);
                                bool canLeave = false;

                                // 當狀態至少是買進成交，則判斷是否達成出場條件
                                if ((
                                status == Config.TRANS_BUY_ORDER_DEAL ||
                                status == Config.TRANS_BUY_CANCEL_SUC ||
                                status == Config.TRANS_BUY_CANCEL_FAILED ||
                                status == Config.TRANS_BUY_ORDER_FAILED
                                ) &&
                                Convert.ToString(symbolNumber) == model.Symbol &&
                                dtMaps.ContainsKey(model.Symbol))
                                {
                                    List<string> details = items.Values<string>().ToList();
                                    double delayTime = 1000 * Convert.ToDouble(model.DelayTime);
                                    now = DateTime.UtcNow;
                                    double curTs = now.ToUniversalTime().Subtract(
                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                    int leaveSell = Convert.ToInt32(model.LeaveSell);
                                    int accumVolume = 0;

                                    foreach (string detail in details)
                                    {
                                        string[] tokens = detail.Split(null);

                                        if (tokens.Length != 6) continue;

                                        double timestamp = Convert.ToDouble(tokens[0]);
                                        int single = Convert.ToInt32(Math.Round(Convert.ToDouble(tokens[4])));

                                        if (curTs - timestamp < delayTime + Config.DELAY_TIME_TOLERANCE)
                                        {
                                            accumVolume += single;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    int maxUpperBid = Convert.ToInt32(model.MaxUpperBid);

                                    if (accumVolume >= leaveSell * maxUpperBid / 100.0 && maxUpperBid > 0)
                                    {
                                        canLeave = true;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                // 如果已有執行緒進到critical section, 則不得重複出場，需使用LeaveLock因出場跟取消有可能同時間發生！
                                if (canLeave && !model.LeaveLock)
                                {
                                    model.LeaveLock = true;

                                    int leaveOrderOption = Convert.ToInt32(model.LeaveOrderOption);
                                    int leaveTotalPert = Convert.ToInt32(model.LeaveTotalPrice);
                                    int leaveCount = -1;
                                    double leaveOrderPrice = 0;
                                    bool isCommSuc = false;

                                    // 假設還尚未取得漲停價，就等到有取值才跳出while loop
                                    int retryCount = 100;
                                    while (!model.Highest.HasValue && --retryCount > 0)
                                    {
                                        Thread.Sleep(50);
                                    }

                                    double highest = Convert.ToDouble(model.Highest);

                                    if (leaveOrderOption == 0)
                                    {

                                        leaveCount = Convert.ToInt32(Math.Floor(dtMaps[model.Symbol] * leaveTotalPert / 100.0));
                                    }
                                    else
                                    {
                                        leaveOrderPrice = PriceTickCalc(highest, Convert.ToInt32(model.LeaveOrderPrice));
                                        leaveCount = Convert.ToInt32(Math.Floor(dtMaps[model.Symbol] * leaveTotalPert / 100.0));
                                    }

                                    while (leaveCount > 0)
                                    {
                                        long requestId = tfcom.GetRequestId();
                                        Security_OrdType ot = Security_OrdType.OT_NEW; // 新建單
                                        Security_Lot sl = Security_Lot.Even_Lot; // 整股
                                        Security_Class sc = Security_Class.SC_Ordinary;
                                        Security_PriceFlag sp = leaveOrderOption == 0 ? Security_PriceFlag.SP_MarketPrice : Security_PriceFlag.SP_FixedPrice;
                                        SIDE_FLAG sf = SIDE_FLAG.SF_SELL;
                                        TIME_IN_FORCE TIF = TIME_IN_FORCE.TIF_ROD;
                                        ushort curOrderCount = Convert.ToUInt16(Math.Min(Config.MAX_ORDER_COUNT, leaveCount));

                                        //Console.WriteLine(string.Format("準備送出委託:{0}共{1}張", requestId, curOrderCount));

                                        model.Orders.Add(requestId, new Dictionary<string, object>
                                        {
                                            { "id", Convert.ToInt32(model.Id) }
                                        });

                                        long ret = tfcom.SecurityOrder(requestId,
                                            ot,
                                            sl,
                                            sc,
                                            brokerID,
                                            accountID,
                                            Convert.ToString(model.Symbol),
                                            sf,
                                            curOrderCount,
                                            decimal.Parse(Convert.ToString(leaveOrderPrice)),
                                            sp,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            TIF
                                            );

                                        /*
                                         * ret 為0僅代表委託單送出，但是否真正委託成功，必須在Receive Handler內去做判斷
                                         * 若任一委託送出成功，則判定為成功
                                         */
                                        if (ret == 0) isCommSuc = true;
                                        leaveCount -= Convert.ToInt16(Config.MAX_ORDER_COUNT);

                                        if (leaveCount <= 0)
                                        {
                                            UpdateOrderStatus(model,
                                                isCommSuc ? Config.TRANS_SELL_ORDER_SENT : Config.TRANS_SELL_ORDER_FAILED);
                                            model.SellCommFinish = true;
                                        }

                                        if (ret != 0)
                                        {
                                            var orderDetail = new Dictionary<string, object>
                                            {
                                                { "oid", model.Id },
                                                { "price", leaveOrderPrice },
                                                { "qty", curOrderCount },
                                                { "request_id", requestId },
                                                { "err_msg",  tfcom.GetOrderErrMsg(ret) },
                                                { "status",  Config.TRANS_SELL_ORDER_FAILED },
                                                { "created_at",  DateTime.UtcNow }
                                            };

                                            long insertID = InsertTransData(orderDetail);
                                            orderDetail.Add("id", insertID);
                                            model.Comm.Add(orderDetail);

                                            AddSnackQueueMsg(string.Format("未成功送出委託{0}{1} 賣出價{2} 共{3}張之請求，原因:{4}",
                                                model.Symbol,
                                                model.Name,
                                                leaveOrderOption == 0 ? "市價" : leaveOrderPrice,
                                                curOrderCount,
                                                tfcom.GetOrderErrMsg(ret)
                                                ));
                                        }
                                    }

                                    // 送出委託賣單
                                    model.LeaveLock = false;
                                }

                                #endregion
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            };

            while (true)
            {
                ws.Ping();
                List<string> curSymbolSet = new();

                // 當沖標的
                foreach (StockViewModel model in Stocks)
                {
                    if (curSymbolSet.Contains(model.Symbol))
                    {
                        continue;
                    }

                    curSymbolSet.Add(model.Symbol);

                }

                // 隔日沖標的
                foreach (StockViewModel model in PostStocks)
                {
                    if (!curSymbolSet.Contains(model.Symbol))
                    {
                        curSymbolSet.Add(model.Symbol);
                    }
                }

                /*
                 * Compare previous symbol set
                 * If changed, resend websocket request to get latest realtime detail
                 */
                if (!Enumerable.SequenceEqual(prevSymbolSet, curSymbolSet) || refreshWS)
                {
                    // 先關閉前者，再新增連線
                    ws.Close();
                    ws.Connect();
                    refreshWS = false;

                    foreach (var symbol in curSymbolSet)
                    {
                        List<WebSocketSendType> wsst = new();
                        wsst.Add(new WebSocketSendType
                        {
                            period = "",
                            source = "twse",
                            symbol = symbol
                        });
                        ws.Send(JsonConvert.SerializeObject(wsst));
                    }

                    Thread.Sleep(1000);

                    foreach (var symbol in curSymbolSet)
                    {
                        List<WebSocketSendType> wsst = new();
                        wsst.Add(new WebSocketSendType
                        {
                            period = "book",
                            source = "twse",
                            symbol = symbol
                        });
                        wsst.Add(new WebSocketSendType
                        {
                            period = "details",
                            source = "twse",
                            symbol = symbol
                        });
                        ws.Send(JsonConvert.SerializeObject(wsst));
                    }
                }

                prevSymbolSet = curSymbolSet;
                Thread.Sleep(1000);
            }
        }

        private long InsertTransData(object item)
        {
            DBConnection db = new();
            if (db.IsConnect())
            {
                return db.InsertTransData(item);
            }
            return -1;
        }


        private void UpdateCancelStatus(StockViewModel model, int cancelStatus, int status)
        {
            model.CancelStatus = cancelStatus;
            model.Status = status;

            DBConnection db = new();
            var editedData = new
            {
                id = Convert.ToInt32(model.Id),
                cancel_status = Convert.ToInt32(cancelStatus),
                status = Convert.ToInt32(status)
            };

            if (db.IsConnect())
            {
                db.UpdateSetting(editedData);
            }
            else
            {
                AddSnackQueueMsg(string.Format("更新訂單取消狀態錯誤，請聯繫工程師處理！錯誤碼 = {0}", Config.MYSQL_ERROR));
            }
        }

        private void UpdateOrderStatus(StockViewModel model, int status)
        {
            model.Status = status;
            model.StatusDesc = statusMaps[status];
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
            else
            {
                AddSnackQueueMsg(string.Format("更新訂單狀態錯誤，請聯繫工程師處理！錯誤碼 = {0}", Config.MYSQL_ERROR));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;
            InitKGI();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (delFlag)
            {
                delFlag = !delFlag;
                DBConnection conn = new();
                if (conn.IsConnect())
                    conn.deleteSetting(Convert.ToInt32(delId));

                MainWindow.refreshFlag = true;
                _ = new ListStock();
            }
        }

        private double PriceTickCalc(double highest, int tickDiff, bool down = true)
        {
            double curPrice = highest;
            int[] ranges = new int[] { 10, 50, 100, 500, 1000, 5000 };
            double tick = -1;
            if (down)
            {

                while (tickDiff-- > 0)
                {
                    foreach (int range in ranges)
                    {
                        tick = range / 1000.0;

                        if (curPrice <= range)
                        {
                            break;
                        }
                    }

                    curPrice -= tick;
                }
            }
            else
            {
                while (tickDiff-- > 0)
                {
                    foreach (int range in ranges)
                    {
                        if (curPrice == range)
                        {
                            break;
                        }

                        tick = range / 1000.0;

                        if (curPrice <= range)
                        {
                            break;
                        }
                    }

                    curPrice += tick;
                }
            }
            return curPrice;

        }

        private void SnackbarMessage_ActionClick(object sender, RoutedEventArgs e)
        {
            SnackBottom.IsActive = false;
        }

        private void SnackbarQueueMessage_ActionClick(object sender, RoutedEventArgs e)
        {
        }

        private void UpdateModelQuote(JToken detail, StockViewModel model)
        {
            double close = Convert.ToDouble(detail["close"]);
            string name = Convert.ToString(detail["name"]);
            double preClose = Convert.ToDouble(detail["pre_close"]);
            int single = Convert.ToInt32(detail["volume"]);
            int volume = Convert.ToInt32(detail["turnover"]);
            double ampDiff = Math.Round(close - preClose, 2);
            double ampPertNum = preClose != 0 ? Math.Round(100 * ampDiff / preClose, 2) : 0;
            string ampPert = string.Format("{0}%", ampPertNum);

            double open = Convert.ToDouble(detail["open"]);
            double buyPrice = Convert.ToDouble(detail["bid_price"]);
            double sellPrice = Convert.ToDouble(detail["ask_price"]);
            string sellPriceInStr;
            string buyPriceInStr;

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

            double avgPrice = Convert.ToDouble(detail["avg_price"]);
            double high = Convert.ToDouble(detail["high"]);
            double low = Convert.ToDouble(detail["low"]);
            double highest = Convert.ToDouble(detail["highest"]);
            double lowest = Convert.ToDouble(detail["lowest"]);
            model.Close = close;
            model.Name = name;
            model.PreClose = preClose;
            model.Single = single;
            model.Volume = volume;
            model.AmpDiff = ampDiff;
            model.AmpPert = ampPert;
            model.Open = open;
            model.BuyPrice = buyPriceInStr;
            model.SellPrice = sellPriceInStr;
            model.AvgPrice = avgPrice;
            model.High = high;
            model.Low = low;
            model.Highest = highest;
            model.Lowest = lowest;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }

}
