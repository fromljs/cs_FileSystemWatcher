using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace cs_FileSystemWatcher
{
    public class MariaDB
    {
        private MySqlConnection m_MySqlConnection;
        private MySqlCommand m_MySqlCommand = new MySqlCommand();

        private struct Account_Info
        {
            private int m_ntAuthority;
            private string m_strUser;
        }

        public MariaDB(string pstr_IP, string pstr_Port, string pstr_ID, string pstr_Pwd, string pstr_Database)
        {
            m_MySqlConnection = new MySqlConnection("Server=" + pstr_IP + ";" + "Port=" + pstr_Port + ";" + "Database=" + pstr_Database + ";" + "Uid=" + pstr_ID + ";" + "Pwd=" + pstr_Pwd + ";");
        }

        public bool GetDBConnectTest()
        {
            try
            {
                DBConnect();

                if (GetServerState() == 1)
                {
                    DBDisconnect();
                    return true;
                }
                else
                {
                    DBDisconnect();
                    return false;
                }
            }
            catch
            {
                DBDisconnect();
                return false;
            }
        }

        public void ExecuteQuery(string pstr_Query)
        {
            try
            {
                //DBConnect();

                m_MySqlCommand.Connection = m_MySqlConnection;
                m_MySqlCommand.CommandText = pstr_Query;

                m_MySqlCommand.ExecuteNonQuery();

                //DBDisconnect();
            }
            catch (Exception exc)
            {
            }
        }

        // CSV

        public bool ExecuteCSV(string pstr_CSVPath)
        {
            DBConnect();
            MySqlTransaction mySqlTransaction = m_MySqlConnection.BeginTransaction();

            try
            {
                MySqlCommand command = new MySqlCommand("DELETE FROM product_tb", m_MySqlConnection);
                int deleteResult = command.ExecuteNonQuery();

                if (deleteResult != -1)
                {
                    MySqlBulkLoader l_MySqlBulkLoader = new MySqlBulkLoader(m_MySqlConnection);

                    l_MySqlBulkLoader.TableName = "product_tb";
                    l_MySqlBulkLoader.FieldTerminator = ",";
                    l_MySqlBulkLoader.LineTerminator = "\r\n";
                    l_MySqlBulkLoader.FileName = pstr_CSVPath;
                    l_MySqlBulkLoader.NumberOfLinesToSkip = 2;

                    int l_ntResult = l_MySqlBulkLoader.Load();

                    if (l_ntResult != -1)
                    {
                        mySqlTransaction.Commit();
                        DBDisconnect();
                        mySqlTransaction.Dispose();
                        return true;
                    }
                    else
                    {
                        mySqlTransaction.Rollback();
                        DBDisconnect();
                        mySqlTransaction.Dispose();
                        return false;
                    }
                }
                else
                {
                    mySqlTransaction.Rollback();
                    DBDisconnect();
                    mySqlTransaction.Dispose();
                    return false;
                }
            }
            catch (Exception exc)
            {
                try
                {
                    mySqlTransaction.Rollback();
                    DBDisconnect();
                    mySqlTransaction.Dispose();
                }
                catch
                {
                }

                return false;
            }
        }

        // Account
        public bool ExecuteCheckID(string pstr_ID, string pstr_Password, out string pstr_User, out int pnt_Authority)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand("spCheckUser", m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.StoredProcedure;

                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_ID", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_ID"].Value = pstr_ID;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_Password", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_Password"].Value = pstr_Password;

                l_MySqlCommand.Parameters.Add("OUT_Authority", MySqlDbType.Int16).Direction = ParameterDirection.Output;
                l_MySqlCommand.Parameters.Add("OUT_User", MySqlDbType.String).Direction = ParameterDirection.Output;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    pnt_Authority = Convert.ToInt32(l_MySqlCommand.Parameters["OUT_Authority"].Value.ToString());
                    pstr_User = l_MySqlCommand.Parameters["OUT_User"].Value.ToString();

                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                pnt_Authority = -9999;
                pstr_User = "NULL";
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                pnt_Authority = -9999;
                pstr_User = "NULL";
                return false;
            }
        }

        public bool ExecuteInsertID(string pstr_ID, string pstr_Password, string pstr_User, int pnt_Authority)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "INSERT INTO account_tb(ID, Password, User, Authority) VALUES (@ID, @Password, @User, @Authority)";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("ID", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@ID"].Value = pstr_ID;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("Password", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Password"].Value = pstr_Password;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("User", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@User"].Value = pstr_User;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("Authority", MySqlDbType.Int16));
                l_MySqlCommand.Parameters["@Authority"].Value = pnt_Authority;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteDeleteID(string pstr_ID)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "DELETE from account_tb where ID = '" + pstr_ID + "'";

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteFixID(string pstr_ID, string pstr_Password, string pstr_User, int pnt_Authority)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "UPDATE account_tb SET Password=@Password, User=@User, Authority=@Authority WHERE ID=@ID";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("@ID", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@ID"].Value = pstr_ID;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Password", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Password"].Value = pstr_Password;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@User", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@User"].Value = pstr_User;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Authority", MySqlDbType.Int16));
                l_MySqlCommand.Parameters["@Authority"].Value = pnt_Authority;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteGetAccount(ListView plsV_Account)
        {
            try
            {
                plsV_Account.Items.Clear();

                DataSet l_DS = new DataSet();

                MySqlDataAdapter l_MySqlDataAdapter = new MySqlDataAdapter("Select * from account_tb where Authority not in ('0')", m_MySqlConnection);

                l_MySqlDataAdapter.Fill(l_DS);

                DBDisconnect();

                if (l_DS.Tables.Count <= 0)
                    return false;

                for (int i = 0; i < l_DS.Tables[0].Rows.Count; i++)
                {
                    ListViewItem l_LVI = new ListViewItem((i + 1).ToString());

                    // ID
                    l_LVI.SubItems.Add(l_DS.Tables[0].Rows[i].ItemArray[0].ToString());
                    // Password
                    l_LVI.SubItems.Add(l_DS.Tables[0].Rows[i].ItemArray[1].ToString());
                    // User
                    l_LVI.SubItems.Add(l_DS.Tables[0].Rows[i].ItemArray[2].ToString());
                    // Authority
                    if (l_DS.Tables[0].Rows[i].ItemArray[3].ToString() == "1")
                        l_LVI.SubItems.Add("관리자");
                    else
                        l_LVI.SubItems.Add("작업자");

                    plsV_Account.Items.Add(l_LVI);
                }

                plsV_Account.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                return true;
            }
            catch
            {
                DBDisconnect();
                return false;
            }
        }

        // Product

        public bool ExecuteCheckProduct(string pstr_Barcode)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand("spCheckProduct", m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.StoredProcedure;

                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_Barcode", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_Barcode"].Value = pstr_Barcode;

                l_MySqlCommand.Parameters.Add("OUT_Count", MySqlDbType.Int16).Direction = ParameterDirection.Output;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();

                    if (Convert.ToInt32(l_MySqlCommand.Parameters["OUT_Count"].Value.ToString()) != -9999)
                        return true;
                    else
                        return false;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public DataSet ExecuteGetProduct(string pstr_Barcode)
        {
            try
            {
                DataSet l_DS = new DataSet();

                MySqlDataAdapter l_MySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM product_tb WHERE Barcode='" + pstr_Barcode + "'", m_MySqlConnection);

                l_MySqlDataAdapter.Fill(l_DS);

                DBDisconnect();

                if (l_DS.Tables.Count <= 0)
                    return null;
                else
                {
                    if (l_DS.Tables[0].Rows.Count > 0)
                        return l_DS;
                    else
                        return null;
                }
            }
            catch
            {
                DBDisconnect();
                return null;
            }
        }

        public bool ExecuteInsertProduct(string pstr_No, string pstr_Name, string pstr_Barcode, string pstr_Stigma, string pstr_Std_ExpirationDate)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "INSERT INTO product_tb(No, Name, Barcode, Stigma, Std_ExpirationDate) VALUES (@No, @Name, @Barcode, @Stigma, @Std_ExpirationDate)";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("@No", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@No"].Value = pstr_No;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Name"].Value = pstr_Name;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Barcode", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Barcode"].Value = pstr_Barcode;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Stigma", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Stigma"].Value = pstr_Stigma;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Std_ExpirationDate", MySqlDbType.Int16));
                l_MySqlCommand.Parameters["@Std_ExpirationDate"].Value = pstr_Std_ExpirationDate;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteDeleteProduct(string barcode)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "DELETE from product_tb where Barcode = '" + barcode + "'";

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteFixProduct(string pstr_No, string pstr_Name, string pstr_Barcode, string pstr_Stigma, string pstr_Std_ExpirationDate)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "UPDATE product_tb SET No=@No, Name=@Name, Std_ExpirationDate=@Std_ExpirationDate WHERE Barcode=@Barcode;";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("@No", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@No"].Value = pstr_No;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Name"].Value = pstr_Name;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Barcode", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Barcode"].Value = pstr_Barcode;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Stigma", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@Stigma"].Value = pstr_Stigma;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Std_ExpirationDate", MySqlDbType.Int16));
                l_MySqlCommand.Parameters["@Std_ExpirationDate"].Value = pstr_Std_ExpirationDate;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteGetProduct(ListView plsV_Account)
        {
            try
            {
                plsV_Account.Items.Clear();

                DataSet l_DS = new DataSet();

                MySqlDataAdapter l_MySqlDataAdapter = new MySqlDataAdapter("Select * from product_tb", m_MySqlConnection);

                l_MySqlDataAdapter.Fill(l_DS);

                DBDisconnect();

                if (l_DS.Tables.Count <= 0)
                    return false;

                for (int i = 0; i < l_DS.Tables[0].Rows.Count; i++)
                {
                    ListViewItem l_LVI = new ListViewItem((i + 1).ToString());

                    for (int j = 0; j < l_DS.Tables[0].Rows[i].ItemArray.Length; j++)
                        l_LVI.SubItems.Add(l_DS.Tables[0].Rows[i].ItemArray[j].ToString());

                    plsV_Account.Items.Add(l_LVI);
                }

                plsV_Account.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                return true;
            }
            catch
            {
                DBDisconnect();
                return false;
            }
        }

        // Log
        public bool ExecuteWorkHistory(string pstr_No, string pstr_User, int pnt_Result)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand("spWorkHistory", m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.StoredProcedure;

                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_User", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_User"].Value = pstr_User;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_No", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_No"].Value = pstr_No;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("IN_Result", MySqlDbType.Text));
                l_MySqlCommand.Parameters["IN_Result"].Value = pnt_Result;

                l_MySqlCommand.Parameters.Add("OUT_Result", MySqlDbType.Int16).Direction = ParameterDirection.Output;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    int l_ntResult_Out = Convert.ToInt32(l_MySqlCommand.Parameters["OUT_Result"].Value.ToString());

                    if (l_ntResult_Out == -9999)
                    {
                        DBDisconnect();
                        return false;
                    }
                    else
                    {
                        DBDisconnect();
                        return true;
                    }
                }

                DBDisconnect();
                return false;
            }
            catch (Exception exc)
            {
                DBDisconnect();
                return false;
            }
        }

        public bool ExecuteRecordLogin(string user, DateTime loginTime)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "INSERT INTO loginhistory_tb (User, Login_Time) VALUES (@User, @Login_TIme)";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("@User", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@User"].Value = user;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Login_TIme", MySqlDbType.DateTime));
                l_MySqlCommand.Parameters["@Login_TIme"].Value = loginTime;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ExecuteRecordLogout(string user, DateTime loginTime, DateTime logoutTime)
        {
            try
            {
                DBConnect();

                MySqlCommand l_MySqlCommand = new MySqlCommand(null, m_MySqlConnection);
                l_MySqlCommand.CommandType = CommandType.Text;

                l_MySqlCommand.CommandText = "UPDATE loginhistory_tb SET User = @User, Logout_Time = @Logout_Time WHERE Login_TIme = '" + loginTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                l_MySqlCommand.Parameters.Add(new MySqlParameter("@User", MySqlDbType.Text));
                l_MySqlCommand.Parameters["@User"].Value = user;
                l_MySqlCommand.Parameters.Add(new MySqlParameter("@Logout_Time", MySqlDbType.DateTime));
                l_MySqlCommand.Parameters["@Logout_Time"].Value = logoutTime;

                int l_ntResult = l_MySqlCommand.ExecuteNonQuery();

                if (l_ntResult != -1)
                {
                    DBDisconnect();
                    return true;
                }

                DBDisconnect();
                return false;
            }
            catch
            {
                DBDisconnect();
                return false;
            }
        }

        public void DBConnect()
        {
            try
            {
                if (m_MySqlConnection.State != ConnectionState.Open)
                    m_MySqlConnection.Open();
            }
            catch
            {
            }
        }

        public void DBDisconnect()
        {
            try
            {
                m_MySqlConnection.Close();
            }
            catch
            {
            }
        }

        public int GetServerState()
        {
            return (int)m_MySqlConnection.State;
        }
    }
}