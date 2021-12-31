
namespace TaiwanStockTrading
{
    class Config
    {
        public readonly static bool DEBUG = true;

        public readonly static string REST_HOST = "https://restful.finmaster.com/analytics/api/";
        public readonly static string MYSQL_HOST = "18.179.215.178";
        public readonly static string MYSQL_DB = "tw_trade";
        public readonly static string MYSQL_USER = "woohoowash2018@gmail.com";
        public readonly static string MYSQL_PW = "2019Gogogo";
        public readonly static string WS_HOST = "wss://stock-ws.finmaster.com/finmaster";
        public readonly static string KGI_DOMAIN_NAME = DEBUG ? "itradetest.kgi.com.tw" : "tradeapi.kgi.com.tw";
        public readonly static ushort KGI_TCP_PORT = (ushort)(DEBUG ? 8000 : 443);

        public readonly static int VALID_TIMER = 43200;
        public readonly static int DISABLE_SYMBOL = 1;
        public readonly static int DISABLE_ENTRY = 2;
        public readonly static int DISABLE_LEAVE = 4;

        public readonly static int TRANS_INIT = 0;
        public readonly static int TRANS_BUY_ORDER_SENT = 1;
        public readonly static int TRANS_BUY_ORDER_DEAL = 2;
        public readonly static int TRANS_SELL_ORDER_SENT = 3;
        public readonly static int TRANS_SELL_ORDER_DEAL = 4;
        public readonly static int TRANS_BUY_ORDER_FAILED = -1;
        public readonly static int TRANS_SELL_ORDER_FAILED = -2;

        public readonly static int TRANS_BUY_CANCEL_SUC = 11;
        public readonly static int TRANS_BUY_CANCEL_FAILED = -3;
        public readonly static int TRANS_SELL_CANCEL_SUC = 12;
        public readonly static int TRANS_SELL_CANCEL_FAILED = -4;
        public readonly static int TRANS_FINISHED = 13;

        public readonly static string TRANS_INIT_DESC = "新建單";
        public readonly static string TRANS_BUY_ORDER_SENT_DESC = "委買送出";
        public readonly static string TRANS_BUY_ORDER_DEAL_DESC = "委買成交";
        public readonly static string TRANS_SELL_ORDER_SENT_DESC = "委賣送出";
        public readonly static string TRANS_SELL_ORDER_DEAL_DESC = "委賣成交";
        public readonly static string TRANS_BUY_ORDER_FAILED_DESC = "委買失敗";
        public readonly static string TRANS_SELL_ORDER_FAILED_DESC = "委賣失敗";

        public readonly static string TRANS_BUY_CANCEL_SUC_DESC = "委買取消成功";
        public readonly static string TRANS_BUY_CANCEL_FAILED_DESC = "委買取消失敗";
        public readonly static string TRANS_SELL_CANCEL_SUC_DESC = "委賣取消成功";
        public readonly static string TRANS_SELL_CANCEL_FAILED_DESC = "委賣取消失敗";
        public readonly static string TRANS_FINISHED_DESC = "已結束";

        public readonly static int CANCEL_ORDER_SUC = 1;
        public readonly static int CANCEL_ORDER_FAILED = -1;

        public readonly static int MYSQL_ERROR = -1;
        public readonly static int DAYTRADE_FETCH_DAYS = 2;
        public readonly static int POSTTRADE_FETCH_DAYS = 5;

        public readonly static ushort MAX_ORDER_COUNT = 499;
        public readonly static int ORDER_ID_POSTFIX_COUNT = 7;

        public readonly static double DELAY_TIME_TOLERANCE = 100;

        // 隔日沖使用常數
        public readonly static int TWSTOCK_START_TIME = 3600;
        public readonly static int TWSTOCK_END_TIME = 19800;
        public readonly static int TOTAL_SECONDS_ONEDAY = 86400;
        public readonly static int DELAY = DEBUG ? 10 : 5;
        public readonly static int DT_DELAY_MAXIMUM = DEBUG ? 5000 : 60;

        // 委託回報
        public readonly static string BUY_DESC = "買";
        public readonly static string SELL_DESC = "賣";

        public readonly static string COMM_DESC = "委託";
        public readonly static string DEL_DESC = "刪單";
    }
}
