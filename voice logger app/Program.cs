using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using application_space;
using Microsoft.Win32;
using wav_audio;
using NAudio;
using WinUsbDemo;
using Microsoft.Win32.SafeHandles;
using MySql.Data.MySqlClient;

namespace voice_logger_app
{
    class Program// : ServiceBase
    {
        static Thread[] analyzethreadsArray;
        static Thread[] receivethreadsArray;
        static Thread[] transmitthreadsArray;
        static public logger_total logger_all_handle = new logger_total();
        static public bool continue_scan = true, scanning = false;
        static public bool all_logger_configured_F = false;
        static int flag_start = 1;
        static int configure_timeout_cnt;

        // static public WaveLib.FifoStream test_Fifo = new WaveLib.FifoStream();

        // [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            LogFile.WriteToLogFile("App Restarted.......\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
            start();
            //if (args.Length > 0)
            //{
            //    for (int ii = 0; ii < args.Length; ii++)
            //    {
            //        switch (args[ii].ToUpper())
            //        {
            //            case "/I":
            //                InstallService();
            //                return;
            //            case "/U":
            //                UninstallService();
            //                return;
            //            default:
            //                break;
            //        }
            //    }
            //}

            //else
            //    System.ServiceProcess.ServiceBase.Run(new Program());
        }
        static void configure_complete()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            scanning = false;
            configure_timeout_cnt = -1;
            if (((logger_all_handle.no_of_logger == logger_all_handle.no_of_logger_configured) && (logger_all_handle.no_of_logger != 0)))
            {
                all_logger_configured_F = true;
                MYSQL_Handling_obt.reply_logger_config_command("CONFIGURED", logger_all_handle.no_of_logger);
            }
            else if (logger_all_handle.no_of_logger_configured != 0)
            {
                MYSQL_Handling_obt.reply_logger_config_command(logger_all_handle.no_of_logger_configured.ToString(), logger_all_handle.no_of_logger);

            }
            else
            {
                MYSQL_Handling_obt.reply_logger_config_command("TIMEOUT", 0);
            }


            int[] logger_id_list = new int[logger_all_handle.no_of_logger];
            int[] sorted_index = new int[logger_all_handle.no_of_logger];
            for (int i = 0; i < logger_all_handle.no_of_logger; i++)
            {
                logger_all_handle.logger[i].path = (logger_all_handle.path);
                logger_all_handle.logger[i].AddDeviceArrivedHandler();
                logger_all_handle.logger[i].AddDeviceRemovedHandler();
                logger_id_list[i] = logger_all_handle.logger[i].ID;
                if (logger_all_handle.logger[i].status == 1)
                {
                    for (int j = 0; j < logger_all_handle.logger[i].no_of_channel; j++)
                    {
                        logger_all_handle.logger[i].channel[j].path = (logger_all_handle.path);
                        logger_all_handle.logger[i].channel[j].logger_id = logger_all_handle.logger[i].ID;
                        logger_all_handle.logger[i].channel[j].channel_no = j;
                        logger_all_handle.logger[i].channel[j].Logger_index = i;
                    }
                }
            }
            Array.Sort(logger_id_list);
            for (int i = 0; i < logger_all_handle.no_of_logger; i++)
            {
                for (int j = 0; j < logger_all_handle.no_of_logger; j++)
                {
                    if (logger_all_handle.logger[j].ID == logger_id_list[i])
                    {
                        sorted_index[i] = j; break;
                    }
                }
            }

            for (int i = 0; i < logger_all_handle.no_of_logger; i++)
            {
                MYSQL_Handling_obt.Insert_logger_table(logger_all_handle.logger[sorted_index[i]].ID.ToString(), logger_all_handle.logger[sorted_index[i]].signature, logger_all_handle.logger[sorted_index[i]].status.ToString(), logger_all_handle.logger[sorted_index[i]].no_of_channel.ToString());
                if (logger_all_handle.logger[sorted_index[i]].status == 1)
                {
                    for (int j = 0; j < logger_all_handle.logger[sorted_index[i]].no_of_channel; j++)
                    {
                        MYSQL_Handling_obt.Insert_channel_table(logger_all_handle.logger[sorted_index[i]].ID.ToString(), j.ToString(), logger_all_handle.logger[sorted_index[i]].status.ToString(), "-1", "-1", "-1", "-1", "-1", "-1");
                    }
                }
                logger_all_handle.logger[sorted_index[i]].fully_ready = true;
            }

            get_channel_parameters_configurations();

        }
        static void update()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();

            if (logger_all_handle.no_of_logger != 0)
            {
                string frwrdr_no = "";
                for (int i = 0; i < logger_all_handle.no_of_logger_configured; i++)
                {
                    for (int j = 0; j < logger_all_handle.logger[i].no_of_channel; j++)
                    {
                        MYSQL_Handling_obt.get_channel_table_info(logger_all_handle.logger[i].ID.ToString(), j.ToString(), out logger_all_handle.logger[i].channel[j].call_forward_flag, out frwrdr_no, out logger_all_handle.logger[i].channel[j].call_auto_pick_flag, out logger_all_handle.logger[i].channel[j].live_play);
                        if (logger_all_handle.logger[i].channel[j].FRDRDR_NO_DIALED == false)
                        {
                            logger_all_handle.logger[i].channel[j].FRDRDR_No = frwrdr_no;
                        }
                        else if (logger_all_handle.logger[i].channel[j].FRDRDR_NO_DIALED == true)
                        {
                            if (frwrdr_no != "")
                            {
                                if (logger_all_handle.logger[i].channel[j].FRDRDR_No != frwrdr_no)
                                {
                                    logger_all_handle.logger[i].channel[j].FRDRDR_NO_DIALED = false;
                                    logger_all_handle.logger[i].channel[j].FRDRDR_No = frwrdr_no;
                                    logger_all_handle.logger[i].channel[j].Last_cmd_time = System.Environment.TickCount;
                                }
                            }
                            else
                            {
                                logger_all_handle.logger[i].channel[j].FRDRDR_NO_DIALED = false;
                            }
                        }
                    }
                }
            }

            MYSQL_Handling_obt.reply_logger_config_command("UPDATED", logger_all_handle.no_of_logger_configured);
        }

        static void get_channel_parameters_configurations()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            string a, b, c, d, e, f, g, h, k, l, m, n, o, p, q, r;
            if (logger_all_handle.no_of_logger != 0)
                for (int i = 0; i < logger_all_handle.no_of_logger_configured; i++)
                {
                    for (int j = 0; j < logger_all_handle.logger[i].no_of_channel; j++)
                    {
                        MYSQL_Handling_obt.get_channel_configurations(logger_all_handle.logger[i].ID.ToString(), j.ToString(), out a, out b, out c, out d, out e, out f, out g, out h, out k, out l, out m, out n, out o, out p, out q, out r);

                        if (a == "Digital") { logger_all_handle.logger[i].channel[j].channel_type_digital = 0; }
                        else if (a == "Analog") { logger_all_handle.logger[i].channel[j].channel_type_digital = 1; }
                        else if (a == "Audio") { logger_all_handle.logger[i].channel[j].channel_type_digital = 2; }
                        if (b != "") logger_all_handle.logger[i].channel[j].number_capturing_duration = Convert.ToInt32(b);
                        if (c != "") logger_all_handle.logger[i].channel[j].threshold_energy_incoming = Convert.ToInt32(c);
                        if (d != "") logger_all_handle.logger[i].channel[j].threshold_energy_outgoing = Convert.ToInt32(d);
                        if (e != "") logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_threshold = Convert.ToInt32(e);
                        if (f != "") logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_frequency = Convert.ToInt32(f);
                        if (g != "") logger_all_handle.logger[i].channel[j].ring_pulses_before_DTMF = Convert.ToInt32(g);
                        if (h != "") logger_all_handle.logger[i].channel[j].ring_frequency = Convert.ToInt32(h);
                        if (k != "") logger_all_handle.logger[i].channel[j].silence_cut_enabled = k;
                        if (l != "") logger_all_handle.logger[i].channel[j].silence_threshold = Convert.ToInt32(l);
                        if (m != "") logger_all_handle.logger[i].channel[j].silence_time_threshold = Convert.ToInt32(m);
                        if (n != "") logger_all_handle.logger[i].channel[j].sound_activation_threshold1 = Convert.ToInt32(n);
                        if (o != "") logger_all_handle.logger[i].channel[j].call_picking_threshold = Convert.ToInt32(o);
                        if (p != "") logger_all_handle.logger[i].channel[j].off_hook_sensing_threshold = Convert.ToInt32(p);
                        if (q != "") logger_all_handle.logger[i].channel[j].sound_activation_time_threshold = Convert.ToInt32(q);
                        if (r != "") logger_all_handle.logger[i].channel[j].sound_activation_threshold2 = Convert.ToInt32(r);
                        logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_coefficient = 2 * Math.Cos((2 * 3.1415926535897932384626433832795 * logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_frequency / 8000));//2*cos(2*pi*freq/samp freq)


                        //        string query = "insert into tbl_test (logger_id,channel_no,channel_name,channel_type_digital,No_capture_duration,Incoming_DTMF_Threshold,Outgoing_DTMF_Threshold,Busy_Tone_Threshold," +
                        //"Busy_Tone_Frequency,Ring_Pulses_Before_DTMF,Ring_Frequency,Silent_Based_Cut_En,Silence_Threshold,Silent_time_Threshold,Sound_Activation_Threshold1,Sound_Activation_Threshold2,Pick_Up_Time_Delay,Off_Hook_Duration_ms_threshold,sound_activation_time_threshold)" +
                        //" values ('" + logger_all_handle.logger[i].channel[j].logger_id + "','" + logger_all_handle.logger[i].channel[j].channel_no + "','" + logger_all_handle.logger[i].channel[j].channel_type_digital + "','" + logger_all_handle.logger[i].channel[j].number_capturing_duration + "','" + logger_all_handle.logger[i].channel[j].threshold_energy_incoming + "','" + logger_all_handle.logger[i].channel[j].threshold_energy_outgoing + "','" + logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_threshold + "','" + logger_all_handle.logger[i].channel[j].dtmfDetector.busy_tone_frequency + "','" + logger_all_handle.logger[i].channel[j].ring_pulses_before_DTMF + "'," +
                        //"'" + logger_all_handle.logger[i].channel[j].ring_frequency + "','" + logger_all_handle.logger[i].channel[j].silence_cut_enabled + "','" + logger_all_handle.logger[i].channel[j].silence_threshold + "','" + logger_all_handle.logger[i].channel[j].silence_time_threshold + "','" + logger_all_handle.logger[i].channel[j].sound_activation_threshold1 + "','" + logger_all_handle.logger[i].channel[j].sound_activation_threshold2 + "','" + logger_all_handle.logger[i].channel[j].call_picking_threshold + "','" + logger_all_handle.logger[i].channel[j].off_hook_sensing_threshold + "','" + logger_all_handle.logger[i].channel[j].sound_activation_time_threshold + "')";
                        //        MYSQL_Handling aa = new MYSQL_Handling();
                        //        aa.truncate_table(query);
                        //        //cn.Open();
                        //        //cmd.ExecuteNonQuery();
                        //        //cn.Dispose();
                        //        //cn.Close();



                    }
                }
        }
        static void configure(string path)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            try
            {

                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                {
                    try
                    {
                        logger_all_handle.logger[i].configured = false;
                        analyzethreadsArray[i].Abort();
                        receivethreadsArray[i].Abort();
                        transmitthreadsArray[i].Abort();
                        WinUsbCommunications a = new WinUsbCommunications();
                        a.CloseDeviceHandle(logger_all_handle.logger[i].deviceHandle, logger_all_handle.logger[i].port);
                        logger_all_handle.logger[i].port.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "6\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    }
                    finally
                    {
                        logger_all_handle.logger[i].configured = false;
                        analyzethreadsArray[i].Abort();
                        receivethreadsArray[i].Abort();
                        transmitthreadsArray[i].Abort();
                        WinUsbCommunications a = new WinUsbCommunications();
                        a.CloseDeviceHandle(logger_all_handle.logger[i].deviceHandle, logger_all_handle.logger[i].port);
                        logger_all_handle.logger[i].port.Dispose();
                    }
                }
                all_logger_configured_F = false;
                logger_all_handle.path = null;
                logger_all_handle.logger = null; logger_all_handle.no_of_logger = 0;
                logger_all_handle.no_of_logger_configured = 0;
                MYSQL_Handling_obt.truncate_table("logger_table");
                MYSQL_Handling_obt.truncate_table("live_table");
                MYSQL_Handling_obt.truncate_table("frdrdr_table");
                MYSQL_Handling_obt.truncate_table("fwt_table");
                delay_ms(1000, 0);

                logger_all_handle.path = (path);
                // scan_available_ports(out forwarding_port_names, out forwarding_ids_names, "0403", "6001");
                scan_available_ports_usb();




                if (logger_all_handle.no_of_logger != 0)
                {
                    analyzethreadsArray = new Thread[logger_all_handle.no_of_logger];
                    receivethreadsArray = new Thread[logger_all_handle.no_of_logger];
                    transmitthreadsArray = new Thread[logger_all_handle.no_of_logger];



                    for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                    {
                        logger_all_handle.logger[i].first = true;
                        logger_all_handle.logger[i].status = 0;
                        logger_all_handle.logger[i].configured = true;





                        analyzethreadsArray[i] = new Thread(logger_all_handle.logger[i].read_fifo_and_analyze);
                        analyzethreadsArray[i].Name = "analyze" + i.ToString();
                        analyzethreadsArray[i].Priority = ThreadPriority.Highest;
                        analyzethreadsArray[i].Start();

                        receivethreadsArray[i] = new Thread(logger_all_handle.logger[i].rx_from_logger);
                        receivethreadsArray[i].Name = "Receive" + i.ToString();
                        receivethreadsArray[i].Priority = ThreadPriority.Highest;
                        receivethreadsArray[i].Start();

                        transmitthreadsArray[i] = new Thread(logger_all_handle.logger[i].tx_to_logger);
                        transmitthreadsArray[i].Name = "Transmit" + i.ToString();
                        transmitthreadsArray[i].Priority = ThreadPriority.Highest;
                        transmitthreadsArray[i].Start();

                        //REALTIME_PRIORITY_CLASS v = 

                    }

                    configure_timeout_cnt = 12;
                    scanning = true;
                    MYSQL_Handling_obt.reply_logger_config_command("CONFIGURING", 0);
                }
                else
                {
                    MYSQL_Handling_obt.reply_logger_config_command("NO LOGGER FOUND", 0);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "7\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                MYSQL_Handling_obt.reply_logger_config_command("UNABLE TO CONFIGURE", 0);
            }
        }
        static void shutdown()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            try
            {

                if (logger_all_handle.no_of_logger != 0)
                {
                    for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                    {
                        logger_all_handle.logger[i].configured = false;
                        analyzethreadsArray[i].Abort();
                        receivethreadsArray[i].Abort();
                        transmitthreadsArray[i].Abort();
                        WinUsbCommunications a = new WinUsbCommunications();
                        a.CloseDeviceHandle(logger_all_handle.logger[i].deviceHandle, logger_all_handle.logger[i].port);
                        logger_all_handle.logger[i].port.Dispose();
                    }
                    MYSQL_Handling_obt.truncate_table("logger_table");
                    MYSQL_Handling_obt.truncate_table("live_table");
                    MYSQL_Handling_obt.truncate_table("fwt_table");
                    MYSQL_Handling_obt.truncate_table("frdrdr_table");
                    continue_scan = false;
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "8\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                MessageBox.Show("unable to shutdown");
            }
            finally
            {
                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                {
                    logger_all_handle.logger[i].configured = false;
                    analyzethreadsArray[i].Abort();
                    receivethreadsArray[i].Abort();
                    transmitthreadsArray[i].Abort();
                    WinUsbCommunications a = new WinUsbCommunications();
                    a.CloseDeviceHandle(logger_all_handle.logger[i].deviceHandle, logger_all_handle.logger[i].port);
                    logger_all_handle.logger[i].port.Dispose();
                }

                MYSQL_Handling_obt.truncate_table("logger_table");
                MYSQL_Handling_obt.truncate_table("frdrdr_table");
                MYSQL_Handling_obt.truncate_table("live_table");
                MYSQL_Handling_obt.truncate_table("fwt_table");
                continue_scan = false;
            }
            MYSQL_Handling_obt.reply_logger_config_command("CLOSED", 0);
        }
        static void start()
        {
            AppDomain.CurrentDomain.UnhandledException +=
          new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException +=
            new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            MYSQL_Handling_obt.Initialize();// for "using" based connection
            while (continue_scan)
            {
                try
                {
                    if (logger_all_handle.restart_ownself == true)
                    {
                        int i = 0,z=0,j=0;
                        for(i=0;i<logger_all_handle.no_of_logger_configured;i++)
                        {
                            for (j = 0; j < 4; j++)
                            {
                                if (logger_all_handle.logger[i].channel[j].type_of_call == "")
                                {
                                    z++;
                                }
                            }
                        }
                       
                        if ((z >= logger_all_handle.no_of_logger_configured*4) &&  (z>0))
                        {
                            restart_process();
                            while (true)
                            {
                                delay_ms(1000, 0);
                            }
                        }                        
                    }
                    if (logger_all_handle.config_cnt_delay >= 4)
                    {
                        logger_all_handle.config_cnt_delay = 0;
                        try
                        {
                            configure_command();
                        }
                        catch (Exception ex)
                        {
                            LogFile.WriteToLogFile(ex.Message + "9\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            // MessageBox.Show("configuration exception occured tell to vipin sir");
                        }
                    }

                    delay_ms(1000, 0);
                    logger_all_handle.config_cnt_delay++;
                    if (configure_timeout_cnt > 0) configure_timeout_cnt--;
                }
                catch (Exception ex)
                {
                    LogFile.WriteToLogFile(ex.Message + "11111\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    // MessageBox.Show("main while loop exception occured tell to vipin sir");
                }
            }
            shutdown();
        }
        static void restart_process()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            MYSQL_Handling_obt.increment_restart_cnt();
            Environment.SetEnvironmentVariable("LOGGER_WDT", "1", EnvironmentVariableTarget.Machine);
            //ProcessStartInfo Info = new ProcessStartInfo();
            //Info.Arguments = "/C ping 127.0.0.1 -n 2 && \"" + Application.ExecutablePath + "\"";
            //Info.WindowStyle = ProcessWindowStyle.Hidden;
            //Info.CreateNoWindow = true;
            //Info.FileName = "cmd.exe";
            //Process.Start(Info);
            //Application.Exit();
        }
        static void configure_command()//vvvvv
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            string path = "";
            string cmd = MYSQL_Handling_obt.get_logger_config_command(ref path);
            logger_all_handle.no_of_logger_configured = 0;

            for (int i = 0; i < logger_all_handle.no_of_logger; i++)
            {
                if (logger_all_handle.logger[i].status == 1) { logger_all_handle.no_of_logger_configured++; }
            }
            if ((cmd.Contains("CONFIGURE") == true) || (flag_start == 1))
            {
                flag_start = 0;
                configure((path));
            }
            else if (cmd.Contains("SHUTDOWN") == true)
            {
                shutdown();
            }
            else if ((scanning == true) && ((configure_timeout_cnt == 0) || ((logger_all_handle.no_of_logger == logger_all_handle.no_of_logger_configured) && (logger_all_handle.no_of_logger != 0))))
            {
                configure_complete();
            }
            else if (cmd.Contains("UPDATE") == true)
            {
                update();
            }
            else if (cmd.Contains("OTA_FWT") == true)
            {
                if (logger_all_handle.no_of_logger_configured >= 1)
                {
                    string[] strrg = cmd.Split('#');
                    logger_all_handle.OTA_FILE_path = strrg[1];
                    if (logger_all_handle.OTA_FILE_path != "")
                    {
                        try
                        {
                            byte[] data = File.ReadAllBytes(logger_all_handle.OTA_FILE_path);
                            if (data != null)
                            {
                                logger_all_handle.OTA_File_in_Raw_Bytes = new byte[data.Length];
                                logger_all_handle.OTA_File_in_Raw_Bytes = data;
                                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                                {
                                    if (logger_all_handle.logger[i].status == 1)
                                    {
                                        logger_all_handle.logger[i].OTA_FILE_subscrpt = 0;
                                        logger_all_handle.logger[i].FWT_OTA_in_progress = true;
                                        logger_all_handle.logger[i].OTA_state = 0;
                                    }
                                }
                                logger_all_handle.OTA_in_progress = true;
                                MYSQL_Handling_obt.reply_logger_config_command("FWT OTA STARTED", logger_all_handle.no_of_logger_configured);
                            }
                            else
                            {
                                MYSQL_Handling_obt.reply_logger_config_command("WRONG PATH OR FILE MISSING", logger_all_handle.no_of_logger_configured);
                            }
                        }
                        catch
                        {
                            MYSQL_Handling_obt.reply_logger_config_command("WRONG PATH OR FILE MISSING1", logger_all_handle.no_of_logger_configured);
                        }
                    }
                    else
                    {
                        MYSQL_Handling_obt.reply_logger_config_command("FILE PATH NOT FOUND", logger_all_handle.no_of_logger_configured);
                    }
                }
                else
                {
                    MYSQL_Handling_obt.reply_logger_config_command("No Logger Configured For OTA", logger_all_handle.no_of_logger_configured);
                }
            }
            else if (cmd.Contains("OTA_FRWRDR") == true)
            {
                if (logger_all_handle.no_of_logger_configured >= 1)
                {
                    string[] strrg = cmd.Split('#');
                    logger_all_handle.OTA_FILE_path = strrg[1];
                    if (logger_all_handle.OTA_FILE_path != "")
                    {
                        try
                        {
                            byte[] data = File.ReadAllBytes(logger_all_handle.OTA_FILE_path);
                            if (data != null)
                            {
                                logger_all_handle.OTA_File_in_Raw_Bytes = new byte[data.Length];
                                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                                {
                                    if (logger_all_handle.logger[i].status == 1)
                                    {
                                        logger_all_handle.logger[i].OTA_FILE_subscrpt = 0;
                                        logger_all_handle.logger[i].FRWRDR_OTA_in_progress = true;
                                        logger_all_handle.logger[i].OTA_state = 0;
                                    }
                                }
                                logger_all_handle.OTA_in_progress = true;
                                MYSQL_Handling_obt.reply_logger_config_command("FRWRDR OTA STARTED", logger_all_handle.no_of_logger_configured);
                            }
                            else
                            {
                                MYSQL_Handling_obt.reply_logger_config_command("WRONG PATH OR FILE MISSING", logger_all_handle.no_of_logger_configured);
                            }
                        }
                        catch
                        {
                            MYSQL_Handling_obt.reply_logger_config_command("WRONG PATH OR FILE MISSING", logger_all_handle.no_of_logger_configured);
                        }
                    }
                    else
                    {
                        MYSQL_Handling_obt.reply_logger_config_command("FILE PATH NOT FOUND", logger_all_handle.no_of_logger_configured);
                    }
                }
                else
                {
                    MYSQL_Handling_obt.reply_logger_config_command("No Logger Configured For OTA", logger_all_handle.no_of_logger_configured);
                }

            }
            else if (cmd.Contains("STOP_OTA") == true)
            {
                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                {
                    logger_all_handle.logger[i].OTA_FILE_subscrpt = 0;
                    logger_all_handle.logger[i].OTA_state = 0;
                    logger_all_handle.logger[i].FRWRDR_OTA_in_progress = false;
                }
                logger_all_handle.OTA_File_in_Raw_Bytes = null;
                logger_all_handle.OTA_in_progress = false;
                logger_all_handle.OTA_FILE_path = null;
                MYSQL_Handling_obt.reply_logger_config_command("OTA STOPPED", logger_all_handle.no_of_logger_configured);
                Thread.Sleep(1000);
            }
            if (logger_all_handle.OTA_in_progress == true)
            {
                string str = null;
                for (int i = 0; i < logger_all_handle.no_of_logger; i++)
                {
                    if ((logger_all_handle.logger[i].FWT_OTA_in_progress == true) || (logger_all_handle.logger[i].FRWRDR_OTA_in_progress == true))
                    {
                        str += ("L" + i.ToString() + "-" + (((float)logger_all_handle.logger[i].OTA_FILE_subscrpt / (float)logger_all_handle.OTA_File_in_Raw_Bytes.Length) * 100).ToString());
                        if (logger_all_handle.logger[i].OTA_FILE_subscrpt >= logger_all_handle.OTA_File_in_Raw_Bytes.Length)
                        {
                            logger_all_handle.logger[i].OTA_FILE_subscrpt = 0;
                            logger_all_handle.logger[i].OTA_state = 0;
                            logger_all_handle.logger[i].FRWRDR_OTA_in_progress = false;
                            logger_all_handle.logger[i].FWT_OTA_in_progress = false;
                        }
                        if (i != logger_all_handle.no_of_logger - 1)
                        {
                            str += ",";
                        }
                    }
                }
                if (str != null)
                    MYSQL_Handling_obt.reply_logger_config_command(str, logger_all_handle.no_of_logger_configured);
            }
        }
        static void open_port(string port_name, string ids_name, int logger_no)//vvvvv
        {
            try
            {
                // logger_all_handle.logger[logger_no].port.PortName = port_name;
                // logger_all_handle.logger[logger_no].instance_id = ids_name;
                // logger_all_handle.logger[logger_no].port.Parity = Parity.None;
                // logger_all_handle.logger[logger_no].port.StopBits = StopBits.One;
                // logger_all_handle.logger[logger_no].port.BaudRate = 115200;
                // logger_all_handle.logger[logger_no].port.DataBits = 8;
                // logger_all_handle.logger[logger_no].port.WriteTimeout = 60;
                // logger_all_handle.logger[logger_no].port.ReadTimeout = 60;
                // logger_all_handle.logger[logger_no].port.ReadBufferSize = 65000;
                //// logger_all_handle.logger[logger_no].port.Encoding = Encoding.GetEncoding("iso-8859-1"); 
                //// logger_all_handle.logger[logger_no].port.DataReceived += new SerialDataReceivedEventHandler(logger_all_handle.logger[logger_no].port_DataReceived);
                // logger_all_handle.logger[logger_no].port.Open();
                // logger_all_handle.logger[logger_no].port.DtrEnable = true;
                // logger_all_handle.logger[logger_no].port.RtsEnable = true;
            }
            catch
            {
                //MessageBox.Show("unable to open port");
            }
        }
        static void scan_available_ports_usb()
        {
            try
            {
                Guid guid;

                guid = new Guid("58D07210-27C1-11DD-BD0B-0800200C9A66");
                List<WinUsbCommunications.SafeWinUsbHandle> winusb_handles = new List<WinUsbCommunications.SafeWinUsbHandle>();
                List<SafeFileHandle> handles = new List<SafeFileHandle>();
                List<WinUsbCommunications.DeviceInfo> devices_info = new List<WinUsbCommunications.DeviceInfo>();
                List<string> devices_path_names = new List<string>();
                DeviceManagement my_manager = new DeviceManagement();
                bool a = my_manager.FindDevicesFromGuid(guid, ref winusb_handles, ref handles, ref devices_info, ref devices_path_names);
                if (a == true)
                {
                    logger_all_handle.no_of_logger = winusb_handles.Count;
                    logger_all_handle.logger = new logger_unit[winusb_handles.Count];

                    for (int i = 0; i < winusb_handles.Count; i++)
                    {
                        logger_all_handle.logger[i] = new logger_unit();
                        logger_all_handle.logger[i].port = winusb_handles[i];
                        logger_all_handle.logger[i].port_info = devices_info[i];
                        logger_all_handle.logger[i].deviceHandle = handles[i];
                        logger_all_handle.logger[i].devicepathname = devices_path_names[i];
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "22222\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
            }
        }
        static void scan_available_ports(out List<string> port_names, out List<string> ids_name, string vid, string pid)
        {
            List<string> ids;
            List<string> ports = ComPortNames(vid, pid, out ids);
            List<string> port_names_loc = new List<string>();
            List<string> ids_name_loc = new List<string>();
            port_names_loc.Clear();
            ids_name_loc.Clear();
            if (ports.Count > 0)
            {
                foreach (String s in SerialPort.GetPortNames())
                {

                    if (ports.Contains(s))
                    {
                        port_names_loc.Add(s);
                        int i = ports.IndexOf(s);
                        ids_name_loc.Add(ids[i]);
                    }
                }
            }
            else
            {
                port_names_loc.Clear();
            }
            port_names = port_names_loc;
            ids_name = ids_name_loc;
        }
        static public void delay_ms(int time, int mode)
        {
            //if (mode == 0)
            //{
            Thread.Sleep(time);
            //}
            //else
            //{
            //    int dwStartTime = System.Environment.TickCount;
            //    while (true)
            //    {
            //        if (System.Environment.TickCount - dwStartTime > time) break;
            //    }
            //}
        }
        static List<string> ComPortNames(String VID, String PID, out List<string> ids)
        {
            List<string> comports = new List<string>();
            ids = new List<string>();
            try
            {

                String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
                Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
                RegistryKey rk1 = Registry.LocalMachine;
                RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
                string s3 = "USB";
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                String s = "VID_1D11&PID_0001";
                RegistryKey rk4 = rk3.OpenSubKey(s);
                if (rk4 != null)
                {
                    foreach (String s2 in rk4.GetSubKeyNames())
                    {
                        RegistryKey rk5 = rk4.OpenSubKey(s2);
                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                        comports.Add((string)rk6.GetValue("PortName"));
                        ids.Add(s2);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "33333\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                //MessageBox.Show("unable to get com port names");
            }
            return comports;
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                MessageBox.Show("Whoops! Please contact the developers with "
                   + "the following information:\n\n" + ex.Message + ex.StackTrace + new StackTrace(ex, true).GetFrame(0).GetFileLineNumber() + " " + new StackTrace(ex, true).GetFrame(0).GetMethod(),
                   "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Application.Exit();
            }
        }
        public static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            DialogResult result = DialogResult.Abort;
            try
            {
                result = MessageBox.Show("Whoops! Please contact the developers "
                  + "with the following information:\n\n" + e.Exception.Message
                  + e.Exception.StackTrace, "Application Error",
                  MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
            }
            finally
            {
                if (result == DialogResult.Abort)
                {
                    Application.Exit();
                }
            }
        }






















        //protected override void OnStart(string[] args)
        //{
        //    Thread t = new Thread(() => start());
        //    t.Start();
        //    Console.ReadLine();

        //}

        //protected override void OnStop()
        //{
        //    base.OnStop();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    //clean your resources if you have to
        //    base.Dispose(disposing);
        //}

        //private static void InstallService()
        //{
        //    if (IsServiceInstalled())
        //    {
        //        UninstallService();
        //    }

        //    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        //}

        //private static bool IsServiceInstalled()
        //{
        //    return ServiceController.GetServices().Any(s => s.ServiceName == "voicelogger");
        //}

        //private static void UninstallService()
        //{

        //    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        //}


    }
}

