using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;

using MySql.Data.MySqlClient;

using System.Configuration;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

namespace voice_logger_app
{
    public class MYSQL_Handling
    {
        static public string connectionString;
       // public MySqlConnection connection;
        public string server;
        public string database;
        public string uid;
        public string password;
        //Static int ffff;
        //Constructor
        public void DBConnect()
        {
            Initialize();
        }

        //Initialize values
        public void Initialize()
        {
            try
            {
                //server = "49.50.68.155";
                //database = "logger";
                //uid = "newtrack";
                //password = "qwert@123";
                //string connectionString;
                //connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                //database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
                //connection = new MySqlConnection(connectionString);



                server = "localhost";//"49.50.68.155";
                database = "logger";
                uid = "root";//"newtrack";
                password = "root";//"qwert@123";
                //string connectionString;
                connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
               // connection = new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {                
                LogFile.WriteToLogFile(ex.Message + "333\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                MessageBox.Show("unable to initialize db");
            }
        }

        //open connection to database
        public bool OpenConnection(MySqlConnection connection)
        {
            try
            {
                // Debug.Write("DB CONNECT");
               // DBConnect();
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                LogFile.WriteToLogFile(ex.Message + "444\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection(MySqlConnection connection)
        {
            try
            {
                if (connection != null)
                {
                    connection.Dispose();
                    connection.Close();
                }

                //return true;
            }
            catch (MySqlException ex)
            {
                LogFile.WriteToLogFile(ex.Message + "555\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                //  MessageBox.Show(ex.Message);
                
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close(); //Close the connection
                }

            }
            return true;
        }


        public int get_logger_log_cnt()
        {
            int Count = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Count(*) FROM logger_log";
                
                try
                {
                    //Open Connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Mysql Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);

                        //ExecuteScalar will return one value
                        Count = int.Parse(cmd.ExecuteScalar() + "");

                        //close Connection
                        CloseConnection(connection);

                        return Count;
                    }
                    else
                    {
                        CloseConnection(connection);
                        return Count;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "666\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get logger cnt");
                }
            }
            return Count;
        }


        public void Insert_logger_log(string logger_id, string channel_no, string call_type, string caller_id, string channel_name, string call_st, string call_pt, string call_et, string file_pth, string call_forwarded, string forward_no, string waiting_number_list, string auto_picked, string live_listened)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    if (call_type == "IN PICKED") call_type = "Incoming Call";

                    string query = "INSERT INTO logger_log (logger_id,channel_no,call_type,caller_id,channel_name,call_start_time,call_pick_time,call_end_time,file_path,call_forwarded,forward_no,waiting_number_list) VALUES('" + logger_id + "','" + channel_no + "','" + call_type + "','" + caller_id + "','" + channel_name + "','" + call_st + "','" + call_pt + "','" + call_et + "','" + (file_pth) + "','" + (call_forwarded) + "','" + (forward_no) + "' ,'" + (waiting_number_list) + "')";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);

                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "777\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert logger log");
                }
            }
        }

        public void update_channel_table_channel_status(string logger_id, string channel_no, string channel_status)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set status = '" + channel_status + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "888\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update channel status");
                }
            }
        }

        public void update_channel_table_for_waiting_list(string logger_id, string channel_no, string waiting_number_list)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                // get channel name
                try
                {
                    string query = "UPDATE live_table Set waiting_number_list = '" + waiting_number_list + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "AZAZAZ\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call type");
                }
            }
        }
        public void update_channel_table_call(string logger_id, string channel_no, string call_number, string call_type, string call_start_time, string file_path)
        {
            string ch_name = "", channel_type = "";
            get_channel_name(logger_id, channel_no, out ch_name, out channel_type);
            using (var connection = new MySqlConnection(connectionString))
            {
                // get channel name
                try
                {                    
                    //  int cst = Convert.ToInt32(call_start_time);
                    // cst = cst - 3;// FWT would have picked after 3 rings
                    string query = "UPDATE live_table Set call_type = '" + call_type + "',channel_name = '" + ch_name + "',caller_id = '" + call_number + "',call_type='" + call_type + "',call_start_time='" + call_start_time + "',call_pick_time='" + call_start_time + "',file_path='" + (file_path) + "',call_pick_time='" + call_start_time + "',waiting_number_list = '" + "" + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "AZAZAZ\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call type");
                }
            }
        }
        public void update_channel_table_call_type(string logger_id, string channel_no, string call_type)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set call_type = '" + call_type + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "999\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call type");
                }
            }
        }
        public void update_channel_table_caller_id(string logger_id, string channel_no, string caller_id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set caller_id = '" + caller_id + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "1111\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update caller id");
                }
            }
        }


        public void update_channel_name(string logger_id, string channel_no)
        {
            string ch_name = "", channel_type = "";
            get_channel_name(logger_id, channel_no, out ch_name, out channel_type);
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set channel_name = '" + ch_name + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";


                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "2222\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call st");
                }
            }
        }
        public void move_channel_table_log_table(string logger_id, string channel_no)
        {
            string channel_name = "", call_type = "", caller_id = "", call_st = "", call_pt = "", call_et = "", file_pth = "", call_forwarded = "", forward_no = "", auto_picked = "", live_listened = "", waiting_number_list = "";
            bool flag=false;
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    //Create a list to store the result
                    string command = string.Empty;
                    string query = "SELECT * FROM live_table where logger_id = '" + logger_id + "' && channel_no = '" + channel_no + "'";
                   // string channel_name = "", call_type = "", caller_id = "", call_st = "", call_pt = "", call_et = "", file_pth = "", call_forwarded = "", forward_no = "", auto_picked = "", live_listened = "", waiting_number_list = "";
                    //Open connection

                    if (OpenConnection(connection) == true)
                    {
                        //Create  Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {
                            // string sub_stng="";
                            while (dataReader.Read())
                            {
                                call_type = (string)dataReader["call_type"].ToString();
                                caller_id = (string)dataReader["caller_id"].ToString();
                                call_st = (string)dataReader["call_start_time"].ToString();
                                call_pt = (string)dataReader["call_pick_time"].ToString();
                                call_et = (string)dataReader["call_end_time"].ToString();
                                channel_name = ((string)dataReader["channel_name"].ToString());
                                call_forwarded = ((string)dataReader["call_forward_status"].ToString());
                                //sub_stng = caller_id.Substring(0, caller_id.IndexOf('_'));
                                file_pth = ((string)dataReader["file_path"].ToString());// +"_" + sub_stng;
                                forward_no = ((string)dataReader["call_forward_no"].ToString());
                                waiting_number_list = ((string)dataReader["waiting_number_list"].ToString());
                                auto_picked = "NA";
                                live_listened = "NA";


                            }

                        }


                        //close Data Reader
                        dataReader.Close();
                        //close Connection
                        CloseConnection(connection);
                    }
                    flag = true;
                   // Insert_logger_log(logger_id, channel_no, call_type, caller_id, channel_name, call_st, call_pt, call_et, file_pth, call_forwarded, forward_no, waiting_number_list, auto_picked, live_listened);
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "3333\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to move to log");
                }
            }
            if(flag == true)
            {
                Insert_logger_log(logger_id, channel_no, call_type, caller_id, channel_name, call_st, call_pt, call_et, file_pth, call_forwarded, forward_no, waiting_number_list, auto_picked, live_listened);
            }
        }
        public void update_channel_table(string logger_id, string channel_no, string channel_status, string call_type, string caller_id, string call_st, string call_pt, string call_et, string file_path, string call_fn, bool flag)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string query, cn = "";
                try
                {
                    if (flag == false)
                    {


                        query = "UPDATE live_table Set channel_name = '" + cn + "',call_forward_no = '" + call_fn + "',caller_id = '" + caller_id + "',call_type='" + call_type + "',caller_id='" + caller_id + "',call_start_time='" + call_st + "',call_pick_time='" + call_pt + "',call_end_time='" + call_et + "',file_path='" + (file_path) + "',waiting_number_list = '" + "" + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";
                    }
                    else
                    {
                        query = "UPDATE live_table Set channel_name = '" + cn + "',call_forward_no = '" + call_fn + "',caller_id = '" + caller_id + "',call_type='" + call_type + "',caller_id='" + caller_id + "',call_start_time='" + call_st + "',call_pick_time='" + call_pt + "',call_end_time='" + call_et + "',live_switch='stop',file_path='" + (file_path) + "',waiting_number_list = '" + "" + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";
                    }
                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "4444\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update channel table");
                }
            }
        }
        public void update_channel_table_call_st(string logger_id, string channel_no, string call_st)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set call_start_time = '" + call_st + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "5555\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call st");
                }
            }
        }
        public void update_channel_table_call_pt(string logger_id, string channel_no, string call_pt)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set call_pick_time = '" + call_pt + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "6666\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update pt");
                }
            }
        }
        public void update_channel_table_call_et(string logger_id, string channel_no, string call_et)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set call_end_time = '" + call_et + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'));";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "7777\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update call et");
                }
            }
        }
        public void update_channel_table_file_path(string logger_id, string channel_no, string file_path)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set file_path = '" + file_path + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "8888\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update file path");
                }
            }
        }

        public void update_channel_table_forward_status(string logger_id, string channel_no, string call_forward_status)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE live_table Set call_forward_status = '" + call_forward_status + "' where((logger_id = '" + logger_id + "') && (channel_no = '" + channel_no + "'))";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "9999\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update file path");
                }
            }
        }

        public void Insert_channel_table(string logger_id, string channel_no, string channel_status, string call_type, string caller_id, string call_st, string call_pt, string call_et, string file_path)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "INSERT live_table(logger_id,channel_no,status,call_type,caller_id,call_start_time,call_pick_time,call_end_time,file_path) VALUES('" + logger_id + "','" + channel_no + "','" + channel_status + "','" + call_type + "' ,'" + caller_id + "','" + call_st + "','" + call_pt + "','" + call_et + "','" + (file_path) + "')";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "11\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert channel table");
                }
            }
        }

        public string get_channel_table_info(string logger_id, string channel_no, out string call_fs, out string call_fn, out string aps, out string call_ls)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                //Create a list to store the result
                string command = string.Empty;
                string query = "SELECT * FROM live_table where logger_id = '" + logger_id + "' && channel_no = '" + channel_no + "'";

                call_fn = (string)"NC";
                call_fs = (string)"NC";
                aps = (string)"NC";
                call_ls = (string)"NC";
                try
                {

                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {
                            while (dataReader.Read())
                            {
                                call_fn = dataReader["call_forward_no"].ToString();
                                call_fs = dataReader["call_forward_status"].ToString();
                                aps = dataReader["auto_pick_switch"].ToString();
                                call_ls = dataReader["live_switch"].ToString();
                            }
                        }
                        //close Data Reader
                        dataReader.Close();
                        //close Connection
                        CloseConnection(connection);
                        //return list to be displayed
                        return command;
                    }
                    else
                    {
                        return command;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "22\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get channel table info");
                    return command;
                }
            }
        }


        public string get_channel_name(string logger_id, string channel_no, out string channel_name, out string channel_type_digital)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                //Create a list to store the result
                string command = string.Empty;
                string query = "SELECT * FROM trunk_name where logger_id = '" + logger_id + "' && channel_no = '" + channel_no + "'";

                channel_name = (string)"NC";
                channel_type_digital = "true";
                try
                {

                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {
                            while (dataReader.Read())
                            {
                                channel_name = dataReader["channel_name"].ToString();
                                channel_type_digital = dataReader["channel_type_digital"].ToString();
                            }
                        }
                        //close Data Reader
                        dataReader.Close();
                        //close Connection
                        CloseConnection(connection);
                        //return list to be displayed
                        if ((channel_type_digital != "true") && (channel_type_digital != "false"))
                        {
                            channel_type_digital = "true";
                        }
                        if (channel_name == "NC")
                        {
                            //  MessageBox.Show("Configure Trunk Name Table");
                        }
                        return command;
                    }
                    else
                    {
                        return command;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "33\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get channel table info");
                    return command;
                }
            }
        }

        //Insert statement
        //Insert statement
        public void update_FWT_table(string logger_id, string Channel_No, string FWT_IMEI, string FWT_CCID, string FWT_APP_VER, string FWT_CORE_VER, string MCU_VER, string FWT_SS, string FWT_RS, string imei_new, string ccid_new)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                //ffff++;
                //                   Trace.Write(ffff.ToString());
                //                   Trace.Write("\n");
                try
                {
                    string query;
                    if ((imei_new != "") && (ccid_new != ""))
                    {
                        query = "UPDATE fwt_table Set FWT_IMEI = '" + FWT_IMEI + "' ,FWT_CCID = '" + FWT_CCID + "' ,FWT_SS = '" + FWT_SS + "' ,FWT_RS = '" + FWT_RS + "',FWT_APP_VER = '" + FWT_APP_VER + "',FWT_CORE_VER = '" + FWT_CORE_VER + "',MCU_VER = '" + MCU_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else if ((imei_new != ""))
                    {
                        query = "UPDATE fwt_table Set FWT_IMEI = '" + FWT_IMEI + "' ,FWT_SS = '" + FWT_SS + "' ,FWT_RS = '" + FWT_RS + "',FWT_APP_VER = '" + FWT_APP_VER + "',FWT_CORE_VER = '" + FWT_CORE_VER + "',MCU_VER = '" + MCU_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else if ((ccid_new != ""))
                    {
                        query = "UPDATE fwt_table Set FWT_CCID = '" + FWT_CCID + "' ,FWT_SS = '" + FWT_SS + "' ,FWT_RS = '" + FWT_RS + "',FWT_APP_VER = '" + FWT_APP_VER + "',FWT_CORE_VER = '" + FWT_CORE_VER + "',MCU_VER = '" + MCU_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else
                    {
                        query = "UPDATE fwt_table Set FWT_SS = '" + FWT_SS + "' ,FWT_RS = '" + FWT_RS + "',FWT_APP_VER = '" + FWT_APP_VER + "',FWT_CORE_VER = '" + FWT_CORE_VER + "',MCU_VER = '" + MCU_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "44\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                }
            }
        }
        public void Insert_FWT_table(string logger_id, string Channel_No, string FWT_IMEI, string FWT_CCID, string FWT_APP_VER, string FWT_CORE_VER, string MCU_VER, string FWT_SS, string FWT_RS)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {

                    string query = "INSERT INTO fwt_table(FWT_IMEI,FWT_CCID,FWT_APP_VER,FWT_CORE_VER,MCU_VER,FWT_SS,FWT_RS,Channel_No,logger_id) VALUES(@FWT_IMEI,@FWT_CCID,@FWT_APP_VER,@FWT_CORE_VER,@MCU_VER,@FWT_SS,@FWT_RS,@Channel_No,@logger_id)";


                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        cmd.Parameters.Add("@FWT_IMEI", MySqlDbType.VarChar, FWT_IMEI.Length);
                        cmd.Parameters.Add("@Channel_No", MySqlDbType.VarChar, Channel_No.Length);
                        cmd.Parameters.Add("@FWT_CCID", MySqlDbType.VarChar, FWT_CCID.Length);
                        cmd.Parameters.Add("@FWT_APP_VER", MySqlDbType.VarChar, FWT_APP_VER.Length);
                        cmd.Parameters.Add("@FWT_CORE_VER", MySqlDbType.VarChar, FWT_CORE_VER.Length);
                        cmd.Parameters.Add("@MCU_VER", MySqlDbType.VarChar, MCU_VER.Length);
                        cmd.Parameters.Add("@FWT_SS", MySqlDbType.Blob, FWT_SS.Length);
                        cmd.Parameters.Add("@FWT_RS", MySqlDbType.VarChar, FWT_RS.Length);
                        cmd.Parameters.Add("@logger_id", MySqlDbType.VarChar, logger_id.Length);

                        cmd.Parameters["@FWT_IMEI"].Value = FWT_IMEI;
                        cmd.Parameters["@Channel_No"].Value = Channel_No;
                        cmd.Parameters["@FWT_CCID"].Value = FWT_CCID;
                        cmd.Parameters["@FWT_APP_VER"].Value = FWT_APP_VER;
                        cmd.Parameters["@FWT_CORE_VER"].Value = FWT_CORE_VER;
                        cmd.Parameters["@MCU_VER"].Value = MCU_VER;
                        cmd.Parameters["@FWT_SS"].Value = FWT_SS;
                        cmd.Parameters["@FWT_RS"].Value = FWT_RS;
                        cmd.Parameters["@logger_id"].Value = logger_id;

                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "55\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert in  FWT table");
                }
            }

        }

        public void update_FRDRDR_table(string logger_id, string Channel_No, string FRDRDR_IMEI, string FRDRDR_CCID, string FRDRDR_APP_VER, string FRDRDR_CORE_VER, string FRDRDR_SS, string FRDRDR_RS, string imei_new, string ccid_new)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                //ffff++;
                //                   Trace.Write(ffff.ToString());
                //                   Trace.Write("\n");
                try
                {
                    string query;
                    if ((imei_new != "") && (ccid_new != ""))
                    {
                        query = "UPDATE FRDRDR_table Set FRDRDR_IMEI = '" + FRDRDR_IMEI + "' ,FRDRDR_CCID = '" + FRDRDR_CCID + "' ,FRDRDR_SS = '" + FRDRDR_SS + "' ,FRDRDR_RS = '" + FRDRDR_RS + "',FRDRDR_APP_VER = '" + FRDRDR_APP_VER + "',FRDRDR_CORE_VER = '" + FRDRDR_CORE_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else if ((imei_new != ""))
                    {
                        query = "UPDATE FRDRDR_table Set FRDRDR_IMEI = '" + FRDRDR_IMEI + "' ,FRDRDR_SS = '" + FRDRDR_SS + "' ,FRDRDR_RS = '" + FRDRDR_RS + "',FRDRDR_APP_VER = '" + FRDRDR_APP_VER + "',FRDRDR_CORE_VER = '" + FRDRDR_CORE_VER + "'where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else if ((ccid_new != ""))
                    {
                        query = "UPDATE FRDRDR_table Set FRDRDR_CCID = '" + FRDRDR_CCID + "' ,FRDRDR_SS = '" + FRDRDR_SS + "' ,FRDRDR_RS = '" + FRDRDR_RS + "',FRDRDR_APP_VER = '" + FRDRDR_APP_VER + "',FRDRDR_CORE_VER = '" + FRDRDR_CORE_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    else
                    {
                        query = "UPDATE FRDRDR_table Set FRDRDR_SS = '" + FRDRDR_SS + "' ,FRDRDR_RS = '" + FRDRDR_RS + "',FRDRDR_APP_VER = '" + FRDRDR_APP_VER + "',FRDRDR_CORE_VER = '" + FRDRDR_CORE_VER + "' where logger_id = '" + logger_id + "' && channel_no = '" + Channel_No + "'";
                    }
                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "44\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                }
            }
        }

        public void Insert_FRDRDR_table(string logger_id, string Channel_No, string FRDRDR_IMEI, string FRDRDR_CCID, string FRDRDR_APP_VER, string FRDRDR_CORE_VER, string FRDRDR_SS, string FRDRDR_RS, string FRDRDR_CS, string FRDRDR_No)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {

                    string query = "INSERT INTO FRDRDR_table(FRDRDR_IMEI,FRDRDR_CCID,FRDRDR_SS,FRDRDR_RS,Channel_No,FRDRDR_CS,FRDRDR_No,logger_id,FRDRDR_APP_VER,FRDRDR_CORE_VER) VALUES(@FRDRDR_IMEI,@FRDRDR_CCID,@FRDRDR_SS,@FRDRDR_RS,@Channel_No,@FRDRDR_CS,@FRDRDR_No,@logger_id,@FRDRDR_APP_VER,@FRDRDR_CORE_VER)";


                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);


                        cmd.Parameters.Add("@FRDRDR_IMEI", MySqlDbType.VarChar, FRDRDR_IMEI.Length);
                        cmd.Parameters.Add("@Channel_No", MySqlDbType.VarChar, Channel_No.Length);
                        cmd.Parameters.Add("@FRDRDR_CCID", MySqlDbType.VarChar, FRDRDR_CCID.Length);
                        cmd.Parameters.Add("@FRDRDR_SS", MySqlDbType.Blob, FRDRDR_SS.Length);
                        cmd.Parameters.Add("@FRDRDR_CS", MySqlDbType.Blob, FRDRDR_CS.Length);
                        cmd.Parameters.Add("@FRDRDR_RS", MySqlDbType.VarChar, FRDRDR_RS.Length);
                        cmd.Parameters.Add("@FRDRDR_No", MySqlDbType.VarChar, FRDRDR_No.Length);
                        cmd.Parameters.Add("@logger_id", MySqlDbType.VarChar, logger_id.Length);
                        cmd.Parameters.Add("@FRDRDR_APP_VER", MySqlDbType.VarChar, FRDRDR_APP_VER.Length);
                        cmd.Parameters.Add("@FRDRDR_CORE_VER", MySqlDbType.VarChar, FRDRDR_CORE_VER.Length);
                        cmd.Parameters["@FRDRDR_IMEI"].Value = FRDRDR_IMEI;
                        cmd.Parameters["@Channel_No"].Value = Channel_No;
                        cmd.Parameters["@FRDRDR_CCID"].Value = FRDRDR_CCID;

                        cmd.Parameters["@FRDRDR_SS"].Value = FRDRDR_SS;

                        cmd.Parameters["@FRDRDR_RS"].Value = FRDRDR_RS;
                        cmd.Parameters["@FRDRDR_CS"].Value = FRDRDR_CS;
                        cmd.Parameters["@FRDRDR_No"].Value = FRDRDR_No;
                        cmd.Parameters["@logger_id"].Value = logger_id;
                        cmd.Parameters["@FRDRDR_APP_VER"].Value = FRDRDR_APP_VER;
                        cmd.Parameters["@FRDRDR_CORE_VER"].Value = FRDRDR_CORE_VER;
                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "77\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert in  FRDRDR table");
                }
            }

        }
        public void Insert_SMS_table(string logger_id, string channel_no, string ch_name, string str_IMEI, string str_sms_sender_NO, string str_CCID, string str_MSG, string str_DATE, string str_TIME)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "INSERT INTO smstble (logger_id,channel_no,ch_name,sender_number,date,time,message_body,IMEI,CCID) VALUES('" + logger_id + "','" + channel_no + "','" + ch_name + "','" + str_sms_sender_NO + "','" + str_DATE + "','" + str_TIME + "','" + str_MSG + "','" + str_IMEI + "','" + str_CCID + "')";

                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);

                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "88\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert logger log");
                }
            }
        }




        //Insert statement
        //Insert statement
        public void Insert_logger_table(string logger_id, byte[] logger_signature, string logger_status, string no_of_ch)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {

                    string query = "INSERT INTO logger_table(logger_id,logger_signature,logger_status,number_of_channel) VALUES(@logger_id,@logger_signature,@logger_status,@number_of_channel)";


                    //open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create command and assign the query and connection from the constructor
                        MySqlCommand cmd = new MySqlCommand(query, connection);





                        cmd.Parameters.Add("@logger_id", MySqlDbType.VarChar, logger_id.Length);

                        cmd.Parameters.Add("@logger_signature", MySqlDbType.Blob, logger_signature.Length);

                        cmd.Parameters.Add("@logger_status", MySqlDbType.VarChar, logger_status.Length);

                        cmd.Parameters.Add("@number_of_channel", MySqlDbType.VarChar, no_of_ch.Length);



                        cmd.Parameters["@logger_id"].Value = logger_id;

                        cmd.Parameters["@logger_signature"].Value = logger_signature;

                        cmd.Parameters["@logger_status"].Value = logger_status;

                        cmd.Parameters["@number_of_channel"].Value = no_of_ch;

                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "99\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to insert logger log");
                }
            }

        }
        public void truncate_table(string table_name)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {

                    string query = "TRUNCATE TABLE " + table_name;
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create mysql command
                        MySqlCommand cmd = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd.CommandText = query;
                        //Assign the connection using Connection
                        cmd.Connection = connection;

                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "1\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to truncate table");
                }
            }
        }

        public  void increment_restart_cnt()
        {            
            string restart_cnt="";
            using (var connection = new MySqlConnection(connectionString))
            {

                         
                try
                {
                    //Create a list to store the result

                    string query = "SELECT * FROM config_table";
                    // path = "";
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {

                            while (dataReader.Read())
                            {
                                restart_cnt = dataReader["restart_cnt"].ToString();
                                if(int.TryParse(restart_cnt,out int a) == false)
                                {
                                    restart_cnt = "0";
                                }
                            }

                        }
                        //close Data Reader
                        dataReader.Close();
                        int value = Int32.Parse(restart_cnt);
                        value++;
                        restart_cnt = value.ToString();
                        string query1 = "UPDATE config_table SET restart_cnt = " + restart_cnt + "";
                        //create mysql command
                        MySqlCommand cmd1 = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd1.CommandText = query1;
                        //Assign the connection using Connection
                        cmd1.Connection = connection;
                        //Execute query
                        cmd1.ExecuteNonQuery();
                        
                        //close Connection
                        CloseConnection(connection);

                        //return list to be displayed
                        return;
                    }
                    else
                    {
                        //path = (string)"NC";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "3\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get command");
                }
            }
            //path = (string)"NC";
            return;
        }
        //Update statement
        //Update statement
        public void Update_logger_table(string logger_id, string logger_signature, string logger_status, string no_of_ch)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE logger_table set logger_signature ='" + logger_signature + "',logger_status='" + logger_status + "',number_of_channel='" + no_of_ch + "'";
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create mysql command
                        MySqlCommand cmd = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd.CommandText = query;
                        //Assign the connection using Connection
                        cmd.Connection = connection;


                        //Execute query
                        cmd.ExecuteNonQuery();


                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "2\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to update logger table");
                }
            }
        }



        //Select statement
        public string get_logger_config_command(ref string path)
        {
            string hang_status = "";
            string command = "";
            using (var connection = new MySqlConnection(connectionString))
            {
                

                try
                {
                    //Create a list to store the result

                    string query = "SELECT * FROM config_table";
                    // path = "";
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {

                            while (dataReader.Read())
                            {
                                command = dataReader["command"].ToString();
                                hang_status = dataReader["hang_status"].ToString();
                                path = (dataReader["path"].ToString());
                            }

                        }
                        //close Data Reader
                        dataReader.Close();
                        Program.logger_all_handle.hanging_test_cnt++;
                        if (Program.logger_all_handle.hanging_test_cnt >= 15)
                        {
                            Program.logger_all_handle.hanging_test_cnt = 0;
                            if (hang_status == "0")
                            {
                                string query1 = "UPDATE config_table SET hang_status='1'";
                                //create mysql command
                                MySqlCommand cmd1 = new MySqlCommand();
                                //Assign the query using CommandText
                                cmd1.CommandText = query1;
                                //Assign the connection using Connection
                                cmd1.Connection = connection;
                                //Execute query
                                cmd1.ExecuteNonQuery();
                            }
                        }
                        //close Connection
                        CloseConnection(connection);

                        //return list to be displayed
                        return command;
                    }
                    else
                    {
                        //path = (string)"NC";
                        return command;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "3\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get command");
                }
            }
            //path = (string)"NC";
            return command;
        }
        public void get_channel_configurations(string logger_id, string channel_no, out string channel_type_digital, out string number_capturing_duration, out string Incoming_DTMF_Threshold, out string Outgoing_DTMF_Threshold, out string Busy_Tone_Threshold, out string Busy_Tone_Frequency, out string Ring_Pulses_Before_DTMF, out string Ring_Frequency, out string Silent_Based_Cut_En, out string Silence_Threshold, out string Silence_time_Threshold, out string Sound_Activation_Threshold1, out string Pick_Up_Time_Delay, out string Off_Hook_Duration_ms_threshold, out string sound_activation_time_threshold, out string Sound_Activation_Threshold2)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    //Create a list to store the result              
                    string query = "SELECT * FROM trunk_name where logger_id = '" + logger_id + "' && channel_no = '" + channel_no + "'";
                    channel_type_digital = "";
                    number_capturing_duration = "";
                    Incoming_DTMF_Threshold = "";
                    Outgoing_DTMF_Threshold = "";
                    Busy_Tone_Threshold = "";
                    Busy_Tone_Frequency = "";
                    Ring_Pulses_Before_DTMF = "";
                    Ring_Frequency = "";
                    Silent_Based_Cut_En = "";
                    Silence_Threshold = "";
                    Silence_time_Threshold = "";
                    Sound_Activation_Threshold1 = "";
                    Sound_Activation_Threshold2 = "";
                    Pick_Up_Time_Delay = "";
                    Off_Hook_Duration_ms_threshold = "";
                    sound_activation_time_threshold = "";
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {

                            while (dataReader.Read())
                            {
                                channel_type_digital = dataReader["channel_type_digital"].ToString();
                                number_capturing_duration = (dataReader["No_capture_duration"].ToString());
                                Incoming_DTMF_Threshold = dataReader["Incoming_DTMF_Threshold"].ToString();
                                Outgoing_DTMF_Threshold = dataReader["Outgoing_DTMF_Threshold"].ToString();
                                Busy_Tone_Threshold = dataReader["Busy_Tone_Threshold"].ToString();
                                Busy_Tone_Frequency = dataReader["Busy_Tone_Frequency"].ToString();
                                Ring_Pulses_Before_DTMF = dataReader["Ring_Pulses_Before_DTMF"].ToString();
                                Ring_Frequency = dataReader["Ring_Frequency"].ToString();
                                Silent_Based_Cut_En = dataReader["Silent_Based_Cut_En"].ToString();
                                Silence_Threshold = dataReader["Silence_Threshold"].ToString();
                                Silence_time_Threshold = dataReader["Silent_time_Threshold"].ToString();
                                Sound_Activation_Threshold1 = dataReader["Sound_Activation_Threshold1"].ToString();
                                Sound_Activation_Threshold2 = dataReader["Sound_Activation_Threshold2"].ToString();
                                Pick_Up_Time_Delay = dataReader["Pick_Up_Time_Delay"].ToString();
                                Off_Hook_Duration_ms_threshold = dataReader["Off_Hook_Duration_ms_threshold"].ToString();
                                sound_activation_time_threshold = dataReader["sound_activation_time_threshold"].ToString();
                            }

                        }
                        //close Data Reader
                        dataReader.Close();

                        //close Connection
                        CloseConnection(connection);


                    }
                    else
                    {
                        channel_type_digital = "";
                        number_capturing_duration = "";
                        Incoming_DTMF_Threshold = "";
                        Outgoing_DTMF_Threshold = "";
                        Busy_Tone_Threshold = "";
                        Busy_Tone_Frequency = "";
                        Ring_Pulses_Before_DTMF = "";
                        Ring_Frequency = "";
                        Silent_Based_Cut_En = "";
                        Silence_Threshold = "";
                        Silence_time_Threshold = "";
                        Sound_Activation_Threshold1 = "";
                        Sound_Activation_Threshold2 = "";
                        Pick_Up_Time_Delay = "";
                        Off_Hook_Duration_ms_threshold = "";
                        sound_activation_time_threshold = "";
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    channel_type_digital = "";
                    number_capturing_duration = "";
                    Incoming_DTMF_Threshold = "";
                    Outgoing_DTMF_Threshold = "";
                    Busy_Tone_Threshold = "";
                    Busy_Tone_Frequency = "";
                    Ring_Pulses_Before_DTMF = "";
                    Ring_Frequency = "";
                    Silent_Based_Cut_En = "";
                    Silence_Threshold = "";
                    Silence_time_Threshold = "";
                    Sound_Activation_Threshold1 = "";
                    Sound_Activation_Threshold2 = "";
                    Pick_Up_Time_Delay = "";
                    Off_Hook_Duration_ms_threshold = "";
                    sound_activation_time_threshold = "";
                    LogFile.WriteToLogFile(ex.Message + "3asv\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to get command");
                }
            }
        }

        public void reply_logger_config_command(string response, int no_of_logger)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "UPDATE config_table SET response='" + response + "', command='', no_of_logger =  '" + no_of_logger + "' ";
                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //create mysql command
                        MySqlCommand cmd = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd.CommandText = query;
                        //Assign the connection using Connection
                        cmd.Connection = connection;


                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        CloseConnection(connection);
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "4\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("unable to respond");
                }
            }

        }


        public int GetLogID()
        {
            int id = 0;
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    //Create a list to store the result

                    string query = "select max(id) as id from logger_log";

                    //Open connection
                    if (OpenConnection(connection) == true)
                    {
                        //Create Command
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        //Create a data reader and Execute the command
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows == true)
                        {

                            while (dataReader.Read())
                            {
                                id = Convert.ToInt32(dataReader["id"]);
                            }

                        }
                        //close Data Reader
                        dataReader.Close();

                        //close Connection
                        CloseConnection(connection);
                        //return list to be displayed
                        return id;
                    }
                    else
                    {
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    CloseConnection(connection);
                    LogFile.WriteToLogFile(ex.Message + "5\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //call_state = Call_State.LINE_IDLE;
                    //  MessageBox.Show(e.Message);
                }
                // catch
                // {
                //MessageBox.Show("unable to get command");
                //  }
            }
            return id;
        }
    }
}
