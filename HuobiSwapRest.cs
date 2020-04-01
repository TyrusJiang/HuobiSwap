using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using static HuobiSwap.Extention;
using System.Web;
using System.Security.Cryptography;

namespace HuobiSwap
{
    public class HuobiSwapRest
    {
        public HuobiSwapRest()
        {

        }

        public HuobiSwapRest(string accessKey, string secretKey)
        {
            ACCESS_KEY = accessKey;
            SECRET_KEY = secretKey;
        }

        private const string BASE_URL = "https://api.hbdm.com/";
        private const string BASE_URL2 = "https://api.btcgateway.pro/";
        private string ACCESS_KEY;
        private string SECRET_KEY;

        public string httpget(string req)
        {
            var wc = new WebClient();
            string request = req;
            string result = wc.DownloadString(req);
            wc.Dispose();
            return result;
        }

        public string httppost(string url, string myjson)
        {
            WebClient myWebClient = new WebClient();
            myWebClient.Headers.Add("Content-Type", "application/json");
            myWebClient.Encoding = System.Text.Encoding.UTF8;
            string responsestring = myWebClient.UploadString(url, myjson);
            return responsestring;
        }

        private string GetSignatureStr(Method method, string host, string resourcePath, string parameters)
        {
            var sign = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(method.ToString().ToUpper()).Append("\n")
                .Append(host).Append("\n")
                .Append(resourcePath).Append("\n");
            //参数排序
            var paraArray = parameters.Split('&');
            List<string> parametersList = new List<string>();
            foreach (var item in paraArray)
            {
                parametersList.Add(item);
            }
            parametersList.Sort(delegate (string s1, string s2) { return string.CompareOrdinal(s1, s2); });
            foreach (var item in parametersList)
            {
                sb.Append(item).Append("&");
            }
            sign = sb.ToString().TrimEnd('&');
            //计算签名，将以下两个参数传入加密哈希函数
            sign = CalculateSignature256(sign, SECRET_KEY);
            return UrlEncode(sign);
        }

        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public string GetParamsWithSignature(string paras, string requeststring, Method method)
        {
            string common = GetCommonParameters();
            paras = UriEncodeParameterValue(common + paras);
            var sign = GetSignatureStr(method, "api.hbdm.com", requeststring, paras);
            paras += $"&Signature={sign}";
            return paras;
        }

        public string GetGlobalHBParamsWithSignature(string paras, string requeststring, Method method)
        {
            string common = GetCommonParameters();
            paras = UriEncodeParameterValue(common + paras);
            var sign = GetSignatureStr(method, "api.huobi.pro", requeststring, paras);
            paras += $"&Signature={sign}";
            return paras;
        }

        private static string CalculateSignature256(string text, string secretKey)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hashmessage);
            }
        }

        private string GetCommonParameters()
        {
            return $"AccessKeyId={ACCESS_KEY}&SignatureMethod=HmacSHA256&SignatureVersion=2&Timestamp={DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}";
        }

        private string UriEncodeParameterValue(string parameters)
        {
            var sb = new StringBuilder();
            var paraArray = parameters.Split('&');
            var sortDic = new SortedDictionary<string, string>();
            foreach (var item in paraArray)
            {
                var para = item.Split('=');
                sortDic.Add(para.First(), UrlEncode(para.Last()));
            }
            foreach (var item in sortDic)
            {
                sb.Append(item.Key).Append("=").Append(item.Value).Append("&");
            }
            return sb.ToString().TrimEnd('&');
        }
        #region
        //行情接口

        public string GetContractInfo(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_contract_info";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetIndex(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_index";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetPriceLimit(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_price_limit";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetOpenInterest(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_open_interest";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="type">(150档数据) step0, step1, step2, step3, step4, step5（合并深度1-5）；step0时，不合并深度, 
        /// (20档数据) step6, step7, step8, step9, step10, step11（合并深度7-11）；step6时，不合并深度</param>
        /// <returns></returns>
        public string GetMarketDepth(string contract_code, string type)
        {
            string req = BASE_URL + "swap-ex/market/depth?contract_code=" + contract_code + "&type=" + type;
            string res = httpget(req);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract_code">合约代码</param>
        /// <param name="period">K线类型，取值：1min, 5min, 15min, 30min, 60min,4hour,1day, 1mon</param>
        /// <param name="size">获取数量</param>
        /// <param name="from">开始时间戳 10位 单位S	</param>
        /// <param name="to">结束时间戳 10位 单位S</param>
        /// <returns></returns>
        public string GetKline(string contract_code, string period, string size = "", string from = "", string to = "")
        {
            string req = BASE_URL + "swap-ex/market/history/kline?contract_code=" + contract_code + "&period=" + period;
            req = AddOptionalParas(req, "size", size);
            req = AddOptionalParas(req, "from", from);
            req = AddOptionalParas(req, "to", to);
            string res = httpget(req);
            return res;
        }
        //获取聚合行情
        public string GetMerged(string contract_code)
        {
            string req = BASE_URL + "swap-ex/market/detail/merged?contract_code=" + contract_code;
            string res = httpget(req);
            return res;
        }

        public string GetMarketTrades(string contract_code)
        {
            string req = BASE_URL + "swap-ex/market/trade?contract_code=" + contract_code;
            string res = httpget(req);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="size">交易记录数量</param>
        /// <returns></returns>
        public string GetHistoryTrades(string contract_code, string size)
        {
            string req = BASE_URL + "swap-ex/market/history/trade?contract_code=" + contract_code + "&size=" + size;
            string res = httpget(req);
            return res;
        }

        public string GetRiskInfo(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_risk_info";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetInsuranceFund(string contract_code)
        {
            string req = BASE_URL + "swap-api/v1/swap_insurance_fund?contract_code=" + contract_code;
            string res = httpget(req);
            return res;
        }

        public string GetAdjustFactor(string contract_code = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_adjustfactor";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetOpenInterest(string contract_code, string period, string size = "", string amount_type = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_his_open_interest?contract_code" + contract_code + "&period=" + period;
            req = AddOptionalParas(req, "size", size);
            req = AddOptionalParas(req, "amount_type", amount_type);
            string res = httpget(req);
            return res;
        }

        public string GetEliteAccountRatio(string contract_code, string period)
        {
            string req = BASE_URL + "swap-api/v1/swap_elite_account_ratio?contract_code" + contract_code + "&period=" + period;
            string res = httpget(req);
            return res;
        }

        public string GetElitePositionRatio(string contract_code, string period)
        {
            string req = BASE_URL + "swap-api/v1/swap_elite_position_ratio?contract_code" + contract_code + "&period=" + period;
            string res = httpget(req);
            return res;
        }

        public string GetAPIState(string contract_code)
        {
            string req = BASE_URL + "swap-api/v1/swap_api_state";
            req = AddOptionalParas(req, "contract_code", contract_code);
            string res = httpget(req);
            return res;
        }

        public string GetFundingRate(string contract_code)
        {
            string req = BASE_URL + "swap-api/v1/swap_funding_rate?contract_code=" + contract_code;
            string res = httpget(req);
            return res;
        }

        public string GetHistoricalFundingRate(string contract_code, string page_index = "", string page_size = "")
        {
            string req = BASE_URL + "swap-api/v1/swap_historical_funding_rate?contract_code=" + contract_code;
            req = AddOptionalParas(req, "page_index", page_index);
            req = AddOptionalParas(req, "page_size", page_size);
            string res = httpget(req);
            return res;
        }
        #endregion
        #region
        //资产接口

        public string GetAccountInfo(string contract_code = "")
        {
            string req = "swap-api/v1/swap_account_info";
            string paras = "";
            paras = AddPostOptionalParas(paras, "contract_code", contract_code);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>();
            myparas.AddOptionalParameter("contract_code", contract_code);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }

        public string GetPositionInfo(string contract_code = "")
        {
            string req = "swap-api/v1/swap_position_info";
            string paras = "";
            paras = AddPostOptionalParas(paras, "contract_code", contract_code);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>();
            myparas.AddOptionalParameter("contract_code", contract_code);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }

        public string GetSubAccountList(string contract_code = "")
        {
            string req = "swap-api/v1/swap_sub_account_list";
            string paras = "";
            paras = AddPostOptionalParas(paras, "contract_code", contract_code);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>();
            myparas.AddOptionalParameter("contract_code", contract_code);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 查询用户财务记录
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="type">平多：3，平空：4，开仓手续费-吃单：5，开仓手续费-挂单：6，平仓手续费-吃单：7，平仓手续费-挂单：8，交割平多：9，交割平空：10，交割手续费：
        /// 11，强制平多：12，强制平空：13，从币币转入：14，转出至币币：15，结算未实现盈亏-多仓：16，结算未实现盈亏-空仓：17，穿仓分摊：19，系统：26，活动奖励：
        /// 28，返利：29，资金费-收入：30，资金费-支出：31, 转出到子账号合约账户: 34, 从子账号合约账户转入:35, 转出到母账号合约账户: 36, 从母账号合约账户转入: 37</param>
        /// <param name="create_date"></param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public string GetFinancialRecord(string contract_code, string type = "", string create_date = "", string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_financial_record";
            string paras = $"&contract_code={contract_code}";
            paras = AddPostOptionalParas(paras, "type", type);
            paras = AddPostOptionalParas(paras, "create_date", create_date);
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                { "contract_code", contract_code}
            };
            myparas.AddOptionalParameter("type", type);
            myparas.AddOptionalParameter("create_date", create_date);
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 母子账户划转
        /// </summary>
        /// <param name="sub_uid">子账号uid	</param>
        /// <param name="contract_code">品种代码	</param>
        /// <param name="amount">划转金额</param>
        /// <param name="type">划转类型：master_to_sub：母账户划转到子账户， sub_to_master：子账户划转到母账户</param>
        /// <returns></returns>
        public string MasterSubTransfer(string sub_uid, string contract_code, string amount, string type)
        {
            string req = "swap-api/v1/swap_master_sub_transfer";
            string paras = $"&sub_uid={sub_uid}&contract_code={contract_code}&amount={amount}&type={type}";
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                { "sub_uid",sub_uid},
                { "contract_code",contract_code},
                { "amount",amount},
                { "type",type}
            };
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取母账户下的所有母子账户划转记录
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="create_date">可随意输入正整数，如果参数超过90则默认查询90天的数据</param>
        /// <param name="transfer_type">划转类型，不填查询全部类型,【查询多类型中间用，隔开】34:转出到子账号合约账户 35:从子账号合约账户转入</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public string GetMasterSubTransferRecord(string contract_code, string create_date, string transfer_type = "", string page_index = "", string page_size = "")
        {
            string req = "/swap-api/v1/swap_master_sub_transfer_record";
            string paras = $"&contract_code={contract_code}&create_date={create_date}";
            paras = AddPostOptionalParas(paras, "transfer_type", transfer_type);
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                { "contract_code",contract_code},
                { "create_date",create_date},
            };
            myparas.AddOptionalParameter("transfer_type", transfer_type);
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        #endregion
        #region
        //交易接口

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract_code">合约代码,"BTC-USD"</param>
        /// <param name="price">价格</param>
        /// <param name="volume">委托数量(张)</param>
        /// <param name="direction">"buy":买 "sell":卖</param>
        /// <param name="offset">"open":开 "close":平</param>
        /// <param name="lever_rate">杠杆倍数[“开仓”若有10倍多单，就不能再下20倍多单;首次使用高倍杠杆(>20倍)，请使用主账号登录web端同意高倍杠杆协议后，才能使用接口下高倍杠杆(>20倍)]</param>
        /// <param name="order_price_type">订单报价类型 "limit":限价 "opponent":对手价 "post_only":只做maker单,post only下单只受用户持仓数量限制,optimal_5：最优5档、optimal_10：最优10档、
        /// optimal_20：最优20档，"fok":FOK订单，"ioc":IOC订单, opponent_ioc"： 对手价-IOC下单，"optimal_5_ioc"：最优5档-IOC下单，"optimal_10_ioc"：最优10档-IOC下单，"optimal_20_ioc"：最优20档-IOC下单,
        /// "opponent_fok"： 对手价-FOK下单，"optimal_5_fok"：最优5档-FOK下单，"optimal_10_fok"：最优10档-FOK下单，"optimal_20_fok"：最优20档-FOK下单</param>
        /// <param name="client_order_id">客户自己填写和维护，必须为数字</param>
        /// <returns></returns>
        public string PlaceOrder(string contract_code, string price, string volume, string direction, string offset, string lever_rate, string order_price_type, string client_order_id = "")
        {
            string req = "swap-api/v1/swap_order";
            string paras = $"&contract_code={contract_code}&price={price}&volume={volume}&direction={direction}&offset={offset}&lever_rate={lever_rate}&order_price_type={order_price_type}";
            paras = AddPostOptionalParas(paras, "client_order_id", client_order_id);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"price", price},
                {"volume", volume},
                {"direction", direction},
                {"offset", offset},
                {"lever_rate", lever_rate},
                {"order_price_type",order_price_type }
            };
            myparas.AddOptionalParameter("client_order_id", client_order_id);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract_code">合约代码,"BTC-USD"</param>
        /// <param name="order_id">订单ID(多个订单ID中间以","分隔,一次最多允许撤消10个订单)</param>
        /// <param name="client_order_id">客户订单ID(多个订单ID中间以","分隔,一次最多允许撤消10个订单)</param>
        /// <returns></returns>
        public string CancelOrder(string contract_code, string order_id = "", string client_order_id = "")
        {
            string req = "swap-api/v1/swap_cancel";
            string paras = $"&contract_code={contract_code}";
            paras = AddPostOptionalParas(paras, "order_id", order_id);
            paras = AddPostOptionalParas(paras, "client_order_id", client_order_id);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code}
            };
            myparas.AddOptionalParameter("order_id", order_id);
            myparas.AddOptionalParameter("client_order_id", client_order_id);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }

        public string CancelAllOrder(string contract_code)
        {
            string req = "swap-api/v1/swap_cancelall";
            string paras = $"&contract_code={contract_code}";
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code}
            };
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取合约订单信息
        /// </summary>
        /// <param name="contract_code"></param>
        /// <returns></returns>
        public string GetOrderinfo(string contract_code, string order_id = "", string client_order_id = "")
        {
            string req = "swap-api/v1/swap_order_info";
            string paras = $"&contract_code={contract_code}";
            paras = AddPostOptionalParas(paras, "order_id", order_id);
            paras = AddPostOptionalParas(paras, "client_order_id", client_order_id);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code}
            };
            myparas.AddOptionalParameter("order_id", order_id);
            myparas.AddOptionalParameter("client_order_id", client_order_id);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取订单明细信息
        /// </summary>
        /// <param name="contract_code">合约代码,"BTC-USD"</param>
        /// <param name="order_id">订单id</param>
        /// <param name="order_type">订单类型，1:报单 、 2:撤单 、 3:强平、4:交割</param>
        /// <param name="created_at">下单时间戳</param>
        /// <param name="page_index">第几页,不填第一页</param>
        /// <param name="page_size">不填默认20，不得多于50</param>
        /// <returns></returns>
        public string GetOrderDetail(string contract_code, string order_id, string order_type, string created_at = "", string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_order_detail";
            string paras = $"&contract_code={contract_code}&order_id={order_id}&order_type={order_type}";
            paras = AddPostOptionalParas(paras, "created_at", created_at);
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"order_id", order_id },
                {"order_type",order_type}
            };
            myparas.AddOptionalParameter("created_at", created_at);
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }

        public string GetOpenOrders(string contract_code, string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_openorders";
            string paras = $"&contract_code={contract_code}";
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
            };
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取合约历史委托
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="trade_type">0:全部,1:买入开多,2: 卖出开空,3: 买入平空,4: 卖出平多,5: 卖出强平,6: 买入强平,7:交割平多,8: 交割平空, 11:减仓平多，12:减仓平空</param>
        /// <param name="type">1:所有订单,2:结束状态的订单</param>
        /// <param name="status">可查询多个状态，"3,4,5" , 0:全部,3:未成交, 4: 部分成交,5: 部分成交已撤单,6: 全部成交,7:已撤单</param>
        /// <param name="create_date">可随意输入正整数，如果参数超过90则默认查询90天的数据</param>
        /// <param name="page_index">页码，不填默认第1页</param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public string GetHistoricalOrders(string contract_code, string trade_type, string type, string status, string create_date, string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_hisorders";
            string paras = $"&contract_code={contract_code}&trade_type={trade_type}&type={type}&status={status}&create_date={create_date}";
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"trade_type", trade_type},
                {"type", type},
                {"status", status},
                {"create_date", create_date},
            };
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取历史成交记录
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="trade_type"></param>
        /// <param name="create_date"></param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public string GetMatchResults(string contract_code, string trade_type, string create_date, string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_matchresults";
            string paras = $"&contract_code={contract_code}&trade_type={trade_type}&create_date={create_date}";
            paras = AddPostOptionalParas(paras, "page_index", page_index);
            paras = AddPostOptionalParas(paras, "page_size", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"trade_type", trade_type},
                {"create_date", create_date},
            };
            myparas.AddOptionalParameter("page_index", page_index);
            myparas.AddOptionalParameter("page_size", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 闪电平仓下单
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="volume"></param>
        /// <param name="direction"></param>
        /// <param name="client_order_id"></param>
        /// <param name="order_price_type">不填，默认为“闪电平仓”，"lightning"：闪电平仓，"lightning_fok"：闪电平仓-FOK,"lightning_ioc"：闪电平仓-IOC</param>
        /// <returns></returns>
        public string LightningClosePosition(string contract_code, string volume, string direction, string client_order_id = "", string order_price_type = "")
        {
            string req = "swap-api/v1/swap_lightning_close_position";
            string paras = $"&contract_code={contract_code}&volume={volume}&direction={direction}";
            paras = AddPostOptionalParas(paras, "client_order_id", client_order_id);
            paras = AddPostOptionalParas(paras, "order_price_type", order_price_type);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"volume", volume},
                {"direction", direction},
            };
            myparas.AddOptionalParameter("client_order_id", client_order_id);
            myparas.AddOptionalParameter("order_price_type", order_price_type);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        /// <summary>
        /// 获取强平订单
        /// </summary>
        /// <param name="contract_code"></param>
        /// <param name="trade_type"></param>
        /// <param name="create_date"></param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public string GetLiquidatioOrder(string contract_code, string trade_type, string create_date, string page_index = "", string page_size = "")
        {
            string req = "swap-api/v1/swap_liquidation_orders";
            string paras = $"&contract_code={contract_code}&trade_type={trade_type}&create_date={create_date}";
            paras = AddPostOptionalParas(paras, "client_order_id", page_index);
            paras = AddPostOptionalParas(paras, "order_price_type", page_size);
            paras = GetParamsWithSignature(paras, "/" + req, Method.POST);
            var url = $"{BASE_URL}{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"contract_code", contract_code},
                {"trade_type", trade_type},
                {"page_size", page_size},
            };
            myparas.AddOptionalParameter("client_order_id", page_index);
            myparas.AddOptionalParameter("order_price_type", page_size);
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        #endregion

        #region
        //划转接口
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">来源业务线账户，取值：spot(币币)、swap(反向永续)</param>
        /// <param name="to">目标业务线账户，取值：spot(币币)、swap(反向永续)</param>
        /// <param name="currency">币种,目前仅支持小写</param>
        /// <param name="amount">划转金额</param>
        /// <returns></returns>
        public string Transfer(string from, string to, string currency, string amount)
        {
            string req = "v2/account/transfer";
            string paras = $"&from={from}&to={to}&currency={currency}&amount={amount}";
            paras = GetGlobalHBParamsWithSignature(paras, "/" + req, Method.POST);
            var url = "https://api.huobi.pro/" + $"{req}?{paras}";
            var myparas = new Dictionary<string, object>
            {
                {"from", from},
                {"to", to},
                {"currency", currency},
                {"amount", amount},
            };
            string myjson = JsonConvert.SerializeObject(myparas);
            string res = httppost(url, myjson);
            return res;
        }
        #endregion
    }
    public enum Method
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        DELETE = 3,
        HEAD = 4,
        OPTIONS = 5,
        PATCH = 6,
        MERGE = 7,
        COPY = 8
    }
}
