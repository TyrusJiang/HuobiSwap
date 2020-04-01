using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuobiSwap
{
    public class MyApiEntrance
    {
        public MyApiEntrance()
        {
            var _shbrs = new HuobiSwapRest("your access key", "your secrect key");
            var _shb_cntrct_info = _shbrs.GetContractInfo("BTC-USD");
            var shb_oi = _shbrs.GetOpenInterest();
            var shb_mkttrades = _shbrs.GetMarketTrades("BTC-USD");
            var shb_histrades = _shbrs.GetHistoryTrades("BTC-USD", "30");
            var shb_depth = _shbrs.GetMarketDepth("BTC-USD", "step6");
            var shb_riskinfo = _shbrs.GetRiskInfo("BTC-USD");
            var shb_kline = _shbrs.GetKline("BTC-USD", "30min", "150");
            var shb_accinfo = _shbrs.GetAccountInfo("BTC-USD");
            var shb_posinfo = _shbrs.GetPositionInfo("BTC-USD");
            var shb_subacc = _shbrs.GetSubAccountList();
            var shb_place_order = _shbrs.PlaceOrder("BTC-USD", "6300.0", "1", "buy", "open", "1", "limit");
            var shb_cancel = _shbrs.CancelOrder("BTC-USD", "123456");
            var shb_cancelall = _shbrs.CancelAllOrder("BTC-USD");
            var shb_orderinfo = _shbrs.GetOrderinfo("BTC-USD", "123456");
            var shb_orderdetail = _shbrs.GetOrderDetail("BTC-USD", "123456", "1");
            var shb_openorders = _shbrs.GetOpenOrders("BTC-USD");
            var shb_transfer = _shbrs.Transfer("spot", "swap", "btc", "0.01");
            Console.ReadKey();
        }
    }
}
