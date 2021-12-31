using MySql.Data.MySqlClient;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace TaiwanStockTrading
{
    public class DBConnection
    {

        public DBConnection()
        {
        }

        private string Server = Config.MYSQL_HOST;
        private string DatabaseName = Config.MYSQL_DB;
        private string UserName = Config.MYSQL_USER;
        private string Password = Config.MYSQL_PW;

        private MySqlConnection Connection { get; set; }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (string.IsNullOrEmpty(DatabaseName))
                    return false;

                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}; charset=utf8;", Server, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }

            return true;
        }

        public ObservableCollection<StockViewModel> QuerySettings(int tradeType)
        {
            ObservableCollection<StockViewModel> models = new();
            string query = string.Format(
                "SELECT * FROM manuals WHERE trade_type = {0} AND uid = \"{1}\" AND created_at >= DATE_SUB(NOW(), INTERVAL {2} DAY) order by id desc"
                , tradeType, MainWindow.encryptUID, tradeType == 0 ? Config.DAYTRADE_FETCH_DAYS : Config.POSTTRADE_FETCH_DAYS);
            var cmd = new MySqlCommand(query, Connection);

            using (MySqlDataReader mdr = cmd.ExecuteReader())
            {
                while (mdr.Read())
                {
                    int id = Convert.ToInt32(mdr["id"]);
                    int status = Convert.ToInt32(mdr["status"]);
                    string symbol = Convert.ToString(mdr["symbol"]);
                    string statusDesc = MainWindow.statusMaps[status];

                    // trade_type為0代表當沖，1代表隔日沖
                    if (Convert.ToInt32(mdr["trade_type"]) == 0)
                    {
                        int maxUpperBid = 0;
                        int curUpperBid = 0;

                        if (MainWindow.bookMaps.ContainsKey(symbol))
                        {
                            try
                            {
                                maxUpperBid = MainWindow.bookMaps[symbol]["maxUpperBid"];
                                curUpperBid = MainWindow.bookMaps[symbol]["curUpperBid"];
                            }
                            catch (Exception)
                            {
                            }
                        }

                        var stockViewModel = new StockViewModel
                        {
                            Id = id,
                            Symbol = symbol,
                            EntryOption = Convert.ToInt32(mdr["entry_option"]),
                            EntryValue = Convert.ToInt32(mdr["entry_value"]),
                            EntryOrderOption = Convert.ToInt32(mdr["entry_order_option"]),
                            EntryOrderPrice = Convert.ToDouble(mdr["entry_order_price"]),
                            EntryTotalPrice = Convert.ToInt64(mdr["entry_total_price"]),
                            LeaveCancel = Convert.ToInt32(mdr["leave_cancel"]),
                            LeaveSell = Convert.ToInt32(mdr["leave_sell"]),
                            LeaveOrderOption = Convert.ToInt32(mdr["leave_order_option"]),
                            LeaveOrderPrice = Convert.ToDouble(mdr["leave_order_price"]),
                            LeaveTotalPrice = Convert.ToInt64(mdr["leave_total_price"]),
                            DelayTime = Math.Round(Convert.ToDouble(mdr["delay_time"]), 2),
                            Status = status,
                            StatusDesc = statusDesc,
                            CancelStatus = Convert.ToInt32(mdr["cancel_status"]),
                            CreatedAt = Convert.ToDateTime(mdr["created_at"]),
                            TransLock = false,
                            MaxUpperBid = maxUpperBid,
                            CurUpperBid = curUpperBid,
                            TradeType = 0
                        };

                        models.Add(stockViewModel);
                    }
                    else
                    {
                        int cycle = -1;
                        if (MainWindow.modelMaps.ContainsKey(id) && MainWindow.modelMaps[id].ContainsKey("cycle"))
                        {
                            cycle = Convert.ToInt32(MainWindow.modelMaps[id]["cycle"]);
                        }

                        var stockViewModel = new StockViewModel
                        {
                            Id = id,
                            Symbol = symbol,
                            Status = status,
                            StatusDesc = statusDesc,
                            DelayTime = Math.Round(Convert.ToDouble(mdr["delay_time"]), 2),
                            TradeDate = Convert.ToString(mdr["trade_date"]),
                            OpenLeave = Convert.IsDBNull(mdr["open_leave"]) ? null : Convert.ToInt32(mdr["open_leave"]),
                            OpenAmp = Convert.IsDBNull(mdr["open_amp"]) ? null : Convert.ToInt32(mdr["open_amp"]),
                            OpenBelowLeave = Convert.IsDBNull(mdr["open_below_leave"]) ? null : Convert.ToInt32(mdr["open_below_leave"]),
                            ForceLeave = Convert.ToInt32(mdr["force_leave"]),
                            Ticks = Convert.ToString(mdr["ticks"]),
                            CreatedAt = Convert.ToDateTime(mdr["created_at"]),
                            Cycle = cycle,
                            TransLock = false,
                            CheckPostExpired = false,
                            TradeType = 1,
                            PrevRequestID = new List<long>()
                        };

                        models.Add(stockViewModel);
                    }

                }
            }

            foreach (StockViewModel model in models)
            {
                query = string.Format("SELECT * FROM trans WHERE oid = {0}", model.Id);
                var ocmd = new MySqlCommand(query, Connection);
                MySqlDataReader omdr = ocmd.ExecuteReader();
                while (omdr.Read())
                {
                    try
                    {
                        model.Orders.Add(Convert.ToInt64(omdr["request_id"]), new Dictionary<string, object>
                        {
                            { "cnt", omdr["cntn"] },
                            { "suc", Convert.ToInt32(omdr["status"]) == 1 },
                            { "id", Convert.ToInt32(model.Id) },
                            { "old", true }
                        });

                        model.Comm.Add(new Dictionary<string, object>
                        {
                            {"id", Convert.ToInt32(omdr["id"]) },
                            {"oid", Convert.ToInt32(omdr["oid"]) },
                            {"before_qty", Convert.ToDouble(omdr["before_qty"])},
                            {"qty", Convert.ToDouble(omdr["qty"])},
                            {"after_qty", Convert.ToDouble(omdr["after_qty"])},
                            {"price", Convert.ToDouble(omdr["price"])},
                            {"order_func", Convert.ToString(omdr["order_func"])},
                            {"price_flag", Convert.ToString(omdr["price_flag"])},
                            {"side", Convert.ToString(omdr["side"]) },
                            {"order_no", omdr["order_no"]},
                            {"order_time", omdr["order_time"] },
                            {"request_id", omdr["request_id"] },
                            {"cntn", omdr["cntn"] },
                            {"err_code", omdr["err_code"] },
                            {"deal_qty", omdr["deal_qty"] },
                            {"deal_time", omdr["deal_time"] },
                            {"deal_avg_price", omdr["deal_avg_price"] },
                            {"err_msg", Convert.ToString(omdr["err_msg"]) },
                            {"status", omdr["status"]},
                            {"created_at", omdr["created_at"]},
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        continue;
                    }
                }
                omdr.Close();
            }

            return models;
        }

        public long InsertTransData(object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);

            JObject jsonObject = JObject.Parse(jsonData);

            MySqlCommand comm = Connection.CreateCommand();

            int jsonCnt = jsonObject.Count;
            int cnt = 0;
            string sqlKeys = string.Empty;
            string sqlVals = string.Empty;
            string fullSql = "REPLACE INTO trans ({0}) VALUES ({1})";

            foreach (JProperty property in jsonObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                sqlKeys += key;
                sqlVals += string.Format("@{0}", key);

                if (cnt != jsonCnt - 1)
                {
                    sqlKeys += ",";
                    sqlVals += ",";
                }

                var val = property.Value;
                if (key == "created_at") val = Convert.ToDateTime(val).ToString("yyyy-MM-dd HH:mm:ss");
                _ = comm.Parameters.AddWithValue(string.Format("@{0}", property.Name), val);
                ++cnt;
            }

            fullSql = string.Format(fullSql, sqlKeys, sqlVals);
            comm.CommandText = fullSql;
            try
            {
                comm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
            }
            Close();

            return comm.LastInsertedId;
        }


        public void UpdateTransData(object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);

            JObject jsonObject = JObject.Parse(jsonData);

            MySqlCommand comm = Connection.CreateCommand();

            string sqlVals = string.Empty;
            string fullSql = "UPDATE trans SET {0} WHERE id = @id";

            foreach (JProperty property in jsonObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key != "id")
                {
                    sqlVals += string.Format("{0}=@{1},", key, key);
                }

                _ = comm.Parameters.AddWithValue(string.Format("@{0}", property.Name), property.Value);
            }

            if (sqlVals.Length > 0)
            {
                sqlVals = sqlVals.Remove(sqlVals.Length - 1);
            }

            fullSql = string.Format(fullSql, sqlVals);
            comm.CommandText = fullSql;

            try
            {
                comm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
            }

            Close();
        }

        public void UpdateSetting(object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);

            JObject jsonObject = JObject.Parse(jsonData);

            MySqlCommand comm = Connection.CreateCommand();

            string sqlKeys = string.Empty;
            string sqlVals = string.Empty;
            string fullSql = "UPDATE manuals SET {0} WHERE id = @id";

            foreach (JProperty property in jsonObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key != "id")
                {
                    sqlVals += string.Format("{0}=@{1},", key, key);
                }

                _ = comm.Parameters.AddWithValue(string.Format("@{0}", property.Name), property.Value);
            }

            if (sqlVals.Length > 0)
            {
                sqlVals = sqlVals.Remove(sqlVals.Length - 1);
            }
            fullSql = string.Format(fullSql, sqlVals);
            comm.CommandText = fullSql;
            try
            {
                comm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
            }

            Close();
        }

        public void InsertSetting(object data)
        {
            string jsonData = JsonConvert.SerializeObject(data);

            JObject jsonObject = JObject.Parse(jsonData);

            MySqlCommand comm = Connection.CreateCommand();

            int jsonCnt = jsonObject.Count;
            int cnt = 0;
            string sqlKeys = string.Empty;
            string sqlVals = string.Empty;
            string fullSql = "REPLACE INTO manuals ({0}) VALUES ({1})";

            foreach (JProperty property in jsonObject.Properties())
            {
                string key = property.Name;
                JToken value = property.Value;
                sqlKeys += key;
                sqlVals += string.Format("@{0}", key);

                if (cnt != jsonCnt - 1)
                {
                    sqlKeys += ",";
                    sqlVals += ",";
                }

                _ = comm.Parameters.AddWithValue(string.Format("@{0}", property.Name), property.Value);
                ++cnt;
            }

            fullSql = string.Format(fullSql, sqlKeys, sqlVals);
            comm.CommandText = fullSql;
            try
            {
                comm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }

            Close();
        }

        public void InsertRepoData(List<Dictionary<string, string>> repos)
        {
            foreach(object repo in repos)
            {
                string jsonData = JsonConvert.SerializeObject(repo);

                JObject jsonObject = JObject.Parse(jsonData);

                MySqlCommand comm = Connection.CreateCommand();

                int jsonCnt = jsonObject.Count;
                int cnt = 0;
                string sqlKeys = string.Empty;
                string sqlVals = string.Empty;
                string fullSql = "REPLACE INTO repos ({0}) VALUES ({1})";

                foreach (JProperty property in jsonObject.Properties())
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    sqlKeys += key;
                    sqlVals += string.Format("@{0}", key);

                    if (cnt != jsonCnt - 1)
                    {
                        sqlKeys += ",";
                        sqlVals += ",";
                    }

                    _ = comm.Parameters.AddWithValue(string.Format("@{0}", property.Name), property.Value);
                    ++cnt;
                }

                fullSql = string.Format(fullSql, sqlKeys, sqlVals);
                comm.CommandText = fullSql;
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void deleteSetting(int id)
        {
            string fullSql = string.Format("DELETE FROM manuals WHERE id = {0}", id);
            MySqlCommand comm = Connection.CreateCommand();

            comm.CommandText = fullSql;
            comm.ExecuteNonQuery();

            fullSql = string.Format("DELETE FROM trans WHERE oid = {0}", id);
            comm = Connection.CreateCommand();

            comm.CommandText = fullSql;
            comm.ExecuteNonQuery();
            Close();
        }

        public bool CheckUserIsValid()
        {
            string query = string.Format(
            "SELECT * FROM user WHERE uid =\"{0}\"",
            MainWindow.encryptUID);
            var cmd = new MySqlCommand(query, Connection);

            using (MySqlDataReader mdr = cmd.ExecuteReader())
            {
                while (mdr.Read())
                {
                    return Convert.ToBoolean(mdr["valid"]);
                }
            }
            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}