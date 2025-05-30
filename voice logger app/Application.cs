
using voice_logger_app;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Management;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using wav_audio;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using application_space;
using WinUsbDemo;
using Microsoft.Win32.SafeHandles;
using NAudio.Wave;
using System.Threading.Tasks;
using Xabe.FFmpeg;
namespace application_space
{
    public enum Call_State { LINE_IDLE, IN_CALL_STRT = 1, IN_CALL_END, IN_CALL_PICKED, UHF_IN_CALL_Progress, OUT_CALL_START, OUT_CALL_END, MISSED_CALL, IN_CALL_PROGRESS, OUT_CALL_PROGRESS };
    public partial class logger_total
    {
        public logger_unit[] logger;
        public string path;
        public int no_of_logger;
        public int hanging_test_cnt;
        public int no_of_logger_configured;
        public int config_cnt_delay;
        public bool live_call_playing;
        public string OTA_FILE_path;
        public byte[] OTA_File_in_Raw_Bytes;
        public bool OTA_in_progress;
        public NAudio.Wave.BufferedWaveProvider buffered_wave_provider;
        public WaveOut player;
        public string DOT_NET_APP_VERSION;
        public bool restart_ownself;
        public int restart_ownself_cnt;
        public MYSQL_Handling MYSQL_Handling_obt;
        public logger_total()
        {
            DOT_NET_APP_VERSION = "VL_ENGINE_2.3";
            NAudio.Wave.WaveFormat fmt = new NAudio.Wave.WaveFormat(8000, 16, 1);
            buffered_wave_provider = new NAudio.Wave.BufferedWaveProvider(fmt);
            buffered_wave_provider.BufferDuration = TimeSpan.FromSeconds((double)80000 / fmt.AverageBytesPerSecond); // taking audio bufferof 40000
            buffered_wave_provider.DiscardOnBufferOverflow = true;
            player = new WaveOut();
            if (NAudio.Wave.WaveOut.DeviceCount >= 1)
            {
                player.Init(buffered_wave_provider);
            }
           restart_ownself = false;
            path = ("C:\\");
            logger = new logger_unit[0];
            config_cnt_delay = 4;
            hanging_test_cnt = 0;
            live_call_playing = false;
            OTA_in_progress = false;
            OTA_File_in_Raw_Bytes = new byte[102400];//100kb buffer for holding file
        }
    }
    public class logger_channel
    {
        public int Logger_index;
        public string path;
        public int id;
        public int channel_no;
        public string type_of_call;
        public int logger_id;
        public string caller_id;
        public string digital_caller_id;
        public string caller_id_missed_call;
        public bool event_happened;
        public string file_caller_id, waiting_number_list;
        public byte status;
        public byte update_db_F;
        public bool ringing_state;
        public bool channel_cut_call_F;
        public byte channel_call_cut_reason_F;
        public bool offhook_state;
        public bool number_lock;
        public bool last_io_value, last_ring_value;
        public long number_capturing_duration;
        public long incoming_call_start_tick;
        public long in_or_out_call_start_tick;
        public uint dwChunkSize;
        public long silent_start_tick, sound_activation_ticks, sound_activation_time_threshold;
        public uint noise_cntr;
        public bool call_picked_F;
        public long outgoing_incoming_call_last_digit_tick;
        public WaveLib.FifoStream IO_Fifo;
        public WaveLib.FifoStream ring_Fifo;
        public WaveLib.FifoStream ch_Fifo;
        public DtmfDetector dtmfDetector;
        public WaveLib.FifoStream m_Fifo;
        public byte[] m_PlayBuffer;
        byte[] array_ulaw;
        byte[] array_pcm;
        short pcm_sample;
        public string live_play;
        public Call_State call_state;
        public bool pick_command_sent;
        public byte live_playing;//, update_db_for_incoming_F;
        public string call_start_time;
        public int threshold_energy_incoming;
        public int threshold_energy_outgoing;
        public string call_picking_time;
        public string call_end_time;
        public string call_ST_for_file_saving;
        public string call_auto_pick_flag;
        public int pre_channel;
        public int pre_channel_odd;
        public string FWT_IMEI, FWT_APP_VERSION, FWT_CORE_VERSION;
        public bool FWT_ACK;
        public bool FRDRDR_ACK;
        public string FWT_CCID;
        public long fwt_last_db_entry_tick;
        public long frdrdr_last_db_entry_tick;
        public long fwt_frdr_db_write_gap_ms;
        public int FWT_SS, ring_pulses_before_DTMF, ring_frequency, Last_cmd_time;
        public bool FWT_RS;
        public int FWT_PRE_SS;
        public bool FWT_PRE_RS;
        public bool FWT_table_exist;
        public byte[] FWT_MSG;
        public string FRDRDR_IMEI, FRDRDR_APP_VERSION, FRDRDR_CORE_VERSION;
        public string FRDRDR_No;
        public bool FRDRDR_NO_DIALED;
        public string call_forward_flag, silence_cut_enabled;
        public string FRDRDR_CCID;
        public int FRDRDR_SS;
        public bool FRDRDR_RS;
        public byte hook_pre_value, ring_pre_value;
        public int FRDRDR_PRE_SS;
        public bool FRDRDR_PRE_RS;
        public bool FRDRDR_table_exist;
        public int FRDRDR_CS;
        public int FRDRDR_PRE_CS;
        public int delay_cnt, call_picking_threshold, sound_activation_threshold1, sound_activation_threshold2, off_hook_sensing_threshold;
        public int silence_time_threshold, silence_threshold;
        public int channel_type_digital, FWT_Call_IN_OUT_Digital_Pick_Tick;
        public logger_channel()
        {
            Logger_index = 0;
            pick_command_sent = false;
            channel_type_digital = 0;// 0 digital,1 analog,2 audio
            logger_id = 0;
            id = 0;
            live_playing = 0;
            silent_start_tick = 0;
            status = 0;
            digital_caller_id = "";
            call_picking_time = "";
            file_caller_id = "";
            call_forward_flag = "";
            Last_cmd_time = 0;
            channel_cut_call_F = false;
            channel_call_cut_reason_F = 0;
            fwt_last_db_entry_tick = 0;
            frdrdr_last_db_entry_tick = 0;
            fwt_frdr_db_write_gap_ms = 50000;
            //update_db_for_incoming_F = 0;
            array_ulaw = new byte[3500];
            array_pcm = new byte[7000];
            pcm_sample = 0;
            number_lock = true;
            IO_Fifo = new WaveLib.FifoStream();
            IO_Fifo.Change_Max_In_Ram_Size(20000);
            ring_Fifo = new WaveLib.FifoStream();
            ring_Fifo.Change_Max_In_Ram_Size(20000);
            FWT_table_exist = false;
            FRDRDR_table_exist = false;
            call_picked_F = false;
            ringing_state = false;
            dwChunkSize = 0;
            last_io_value = false;
            last_ring_value = false;
            FWT_ACK = false;
            FRDRDR_ACK = false;
            event_happened = false;
            FWT_IMEI = "";
            FWT_CCID = "";
            FRDRDR_APP_VERSION = ""; FRDRDR_CORE_VERSION = "";
            FWT_APP_VERSION = ""; FWT_CORE_VERSION = "";
            delay_cnt = 0;

            FRDRDR_No = ""; FRDRDR_NO_DIALED = false;
            incoming_call_start_tick = 0;
            in_or_out_call_start_tick = 0;
            outgoing_incoming_call_last_digit_tick = 0;
            // outgoing_call_detected_F = 0;

            type_of_call = "";
            call_state = 0;
            caller_id = "";
            waiting_number_list = "";
            hook_pre_value = 0;
            ring_pre_value = 0;
            m_Fifo = new WaveLib.FifoStream(); m_Fifo.Change_Max_In_Ram_Size(100000);

            //ch_Fifo size is set to 600000 to make it read the bytes faster, earler it was 200000
            ch_Fifo = new WaveLib.FifoStream(); ch_Fifo.Change_Max_In_Ram_Size(600000);
            dtmfDetector = new DtmfDetector();
            dtmfDetector.incoming_going_F = false;

            //default_values for configurable  parameters
            call_picking_threshold = 10000;
            number_capturing_duration = 1000;
            silence_time_threshold = 60000;
            silence_threshold = 500;
            sound_activation_time_threshold = 30000;
            silence_cut_enabled = "true";
            threshold_energy_outgoing = 600;
            threshold_energy_incoming = 300;
            dtmfDetector.busy_tone_threshold = 6000;
            dtmfDetector.busy_tone_frequency = 400;
            ring_pulses_before_DTMF = 50;// for mcu
            ring_frequency = 400;// for mcu or app
            sound_activation_threshold1 = 36;
            sound_activation_threshold2 = 208;
            off_hook_sensing_threshold = 100;//for mcu
            dtmfDetector.busy_tone_coefficient = 2 * Math.Cos((2 * 3.1415926535897932384626433832795 * dtmfDetector.busy_tone_frequency / 8000));//2*cos(2*pi*freq/samp freq)
        }
        public void channel_off_on_hook_detection(byte value)
        {
            if (channel_type_digital == 1)
            {
                if (value != hook_pre_value)
                {
                    hook_pre_value = value;
                    try
                    {
                        MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                        if (value == (byte)1)
                        {
                            if (offhook_state == false)
                            {
                                offhook_state = true;
                                if (type_of_call == "")
                                {
                                    Program.delay_ms(0, 0);
                                    DateTime now = DateTime.Now;
                                    string format = "yyyy-MM-dd HH:mm:ss";
                                    string format1 = "yyyy_MM_dd_HH_mm_ss_ffffff";
                                    call_ST_for_file_saving = now.ToString(format1);
                                    call_start_time = now.ToString(format);
                                    call_state = Call_State.OUT_CALL_START;//outgoing start
                                    dtmfDetector.engage_tone_F = 0;
                                    number_lock = false;
                                    file_caller_id = "";
                                    silent_start_tick = 0;
                                    call_picked_F = false;
                                    waiting_number_list = "";
                                    in_or_out_call_start_tick = System.Environment.TickCount;
                                    number_capturing_duration = 15000;
                                    dtmfDetector.powerThreshold = threshold_energy_outgoing;
                                    type_of_call = "Outgoing Call";
                                    caller_id = "";
                                    digital_caller_id = "";
                                    FRDRDR_No = ""; FRDRDR_NO_DIALED = false;
                                    pick_command_sent = false;
                                    MYSQL_Handling_obt.update_channel_table_call_type(logger_id.ToString(), channel_no.ToString(), "Outgoing Call");
                                    MYSQL_Handling_obt.update_channel_table_call_st(logger_id.ToString(), channel_no.ToString(), call_start_time.ToString());
                                    MYSQL_Handling_obt.update_channel_table_file_path(logger_id.ToString(), channel_no.ToString(), (logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving));
                                    MYSQL_Handling_obt.update_channel_name(logger_id.ToString(), channel_no.ToString());
                                    outgoing_incoming_call_last_digit_tick = System.Environment.TickCount;
                                    channel_cut_call_F = false;
                                    channel_call_cut_reason_F = 0;
                                }
                                else if (type_of_call == "Incoming Call")
                                {
                                    ringing_state = false;
                                    DateTime now = DateTime.Now;
                                    string format = "yyyy-MM-dd HH:mm:ss";
                                    call_picking_time = now.ToString(format);
                                    call_state = Call_State.IN_CALL_PICKED;//incoming picked
                                    type_of_call = " Incoming Call";
                                    number_lock = true;
                                    call_picked_F = true;
                                    dtmfDetector.incoming_going_F = true;
                                    in_or_out_call_start_tick = 0;
                                    silent_start_tick = 0;
                                    FRDRDR_No = ""; FRDRDR_NO_DIALED = false;
                                    MYSQL_Handling_obt.update_channel_table_call_type(logger_id.ToString(), channel_no.ToString(), "IN PICKED");
                                    MYSQL_Handling_obt.update_channel_table_call_pt(logger_id.ToString(), channel_no.ToString(), call_picking_time.ToString());
                                }
                            }
                        }
                        else if (value == (byte)0)
                        {
                            if (offhook_state == true)
                            {
                                offhook_state = false;
                                if (type_of_call == "Outgoing Call")
                                {
                                    DateTime now = DateTime.Now;
                                    string format = "yyyy-MM-dd HH:mm:ss";
                                    channel_cut_call_F = true;
                                    channel_call_cut_reason_F = 0;
                                    call_end_time = now.ToString(format);
                                    call_state = Call_State.OUT_CALL_END;//outgoing end 
                                    live_play = "";
                                    type_of_call = "";
                                    in_or_out_call_start_tick = 0;
                                    number_lock = true;
                                    silent_start_tick = 0;
                                    pick_command_sent = false;
                                    call_picked_F = false;
                                    FRDRDR_No = ""; FRDRDR_NO_DIALED = false;
                                    if (file_caller_id != "")
                                    {
                                        MYSQL_Handling_obt.update_channel_table_call_et(logger_id.ToString(), channel_no.ToString(), call_end_time.ToString());

                                        MYSQL_Handling_obt.move_channel_table_log_table(logger_id.ToString(), channel_no.ToString());
                                    }
                                    MYSQL_Handling_obt.update_channel_table(logger_id.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
                                    caller_id = "";
                                    digital_caller_id = "";
                                    waiting_number_list = "";
                                    dtmfDetector.engage_tone_F = 0;
                                }
                                else if (type_of_call == " Incoming Call")
                                {
                                    DateTime now = DateTime.Now;
                                    string format = "yyyy-MM-dd HH:mm:ss";

                                    call_end_time = now.ToString(format);
                                    call_state = Call_State.IN_CALL_END;//incoming end
                                    type_of_call = "";
                                    in_or_out_call_start_tick = 0;
                                    live_play = "";
                                    number_lock = true;
                                    channel_cut_call_F = true;

                                    silent_start_tick = 0;
                                    pick_command_sent = false;
                                    call_picked_F = false;
                                    FRDRDR_No = ""; FRDRDR_NO_DIALED = false;
                                    dtmfDetector.incoming_going_F = false;
                                    MYSQL_Handling_obt.update_channel_table_call_et(logger_id.ToString(), channel_no.ToString(), call_end_time.ToString());
                                    if (channel_call_cut_reason_F == 1)
                                    {
                                        waiting_number_list = "BUSY_TONE";
                                    }
                                    else if (channel_call_cut_reason_F == 2)
                                    {
                                        waiting_number_list = "DIGITAL CUT";
                                    }
                                    else if (channel_call_cut_reason_F == 3)
                                    {
                                        waiting_number_list = "SILENCE CUT";
                                    }
                                    else
                                    {
                                        waiting_number_list = "FALSE ON HOOK CUT";
                                    }


                                    MYSQL_Handling_obt.move_channel_table_log_table(logger_id.ToString(), channel_no.ToString());
                                    MYSQL_Handling_obt.update_channel_table(logger_id.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
                                    caller_id = "";
                                    digital_caller_id = "";
                                    waiting_number_list = "";
                                    dtmfDetector.engage_tone_F = 0;
                                    channel_call_cut_reason_F = 0;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "channel_off_on_hook_detection\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        //MessageBox.Show("off_on hook exception at logger" + logger_id.ToString() + channel_no.ToString() + "occured tell to vipin sir");
                    }
                }
            }
        }
        public void channel_ring_detection(byte value)
        {
            if (channel_type_digital == 1)
            {
                if (value != ring_pre_value)
                {
                    Trace.Write("RING\n");
                    ring_pre_value = value;
                    try
                    {
                        MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                        if (value == (byte)1)
                        {
                            if (ringing_state == false)
                            {
                                //ringing_state = true;  TO AVOID MISSED MARKING OF PICKED CALLS.
                                if (call_state == Call_State.LINE_IDLE)
                                {
                                    ringing_state = true;
                                    number_lock = false;
                                    outgoing_incoming_call_last_digit_tick = System.Environment.TickCount;
                                    caller_id_missed_call = "";
                                    caller_id = "";

                                    DateTime now = DateTime.Now;
                                    string format = "yyyy-MM-dd HH:mm:ss";
                                    string format1 = "yyyy_MM_dd_HH_mm_ss_ffffff";
                                    call_ST_for_file_saving = now.ToString(format1);
                                    call_state = Call_State.IN_CALL_STRT;//incoming start                                   
                                    silent_start_tick = 0;
                                    file_caller_id = "";
                                    number_capturing_duration = 1000;
                                    waiting_number_list = "";
                                    dtmfDetector.powerThreshold = threshold_energy_incoming;
                                    dtmfDetector.space_limit_cnt = 1;
                                    incoming_call_start_tick = System.Environment.TickCount;
                                    in_or_out_call_start_tick = System.Environment.TickCount;
                                    call_start_time = now.ToString(format);
                                    type_of_call = "Incoming Call";
                                    dtmfDetector.engage_tone_F = 0;

                                    channel_cut_call_F = false;
                                    pick_command_sent = false;
                                    channel_call_cut_reason_F = 0;
                                    call_picked_F = false;
                                    MYSQL_Handling_obt.update_channel_table_call_type(logger_id.ToString(), channel_no.ToString(), "RINGING");
                                    MYSQL_Handling_obt.update_channel_table_call_st(logger_id.ToString(), channel_no.ToString(), call_start_time.ToString());
                                    MYSQL_Handling_obt.update_channel_table_file_path(logger_id.ToString(), channel_no.ToString(), (logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving));
                                    MYSQL_Handling_obt.update_channel_name(logger_id.ToString(), channel_no.ToString());
                                }
                            }
                        }
                        else if (value == (byte)0)
                        {
                            if (ringing_state == true)
                            {
                                ringing_state = false;
                                dtmfDetector.incoming_going_F = false;
                                DateTime now = DateTime.Now;
                                string format = "yyyy-MM-dd HH:mm:ss";
                                call_end_time = now.ToString(format);
                                call_state = Call_State.MISSED_CALL;// missed call
                                type_of_call = "";
                                number_lock = true;
                                silent_start_tick = 0;
                                live_play = "";
                                in_or_out_call_start_tick = 0;
                                call_picked_F = false;

                                if (caller_id_missed_call != "") caller_id = caller_id_missed_call;

                                if (caller_id != "")
                                {
                                    MYSQL_Handling_obt.update_channel_table_call_type(logger_id.ToString(), channel_no.ToString(), "MISSED");
                                    MYSQL_Handling_obt.update_channel_table_call_et(logger_id.ToString(), channel_no.ToString(), call_end_time);
                                    MYSQL_Handling_obt.update_channel_table_caller_id(logger_id.ToString(), channel_no.ToString(), (caller_id/*caller_id_missed_call*/ + "__" + digital_caller_id)); caller_id_missed_call = "";


                                    MYSQL_Handling_obt.move_channel_table_log_table(logger_id.ToString(), channel_no.ToString());
                                    MYSQL_Handling_obt.update_channel_table(logger_id.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
                                }
                                pick_command_sent = false;
                                digital_caller_id = "";
                                caller_id = "";
                                dtmfDetector.engage_tone_F = 0;
                                channel_cut_call_F = true;
                                channel_call_cut_reason_F = 0;
                                waiting_number_list = "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "channel_ring_detection\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        //MessageBox.Show("ring exception at logger" + logger_id.ToString() + channel_no.ToString() + "occured tell to vipin sir");
                    }
                }
            }
        }
        public void Save_digital(string filePath, int logger_no, int channel_no, byte[] data, int last_element)
        {
            try
            {
                MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                if ((call_state == Call_State.OUT_CALL_START) || (call_state == Call_State.IN_CALL_STRT))
                {
                    try
                    {
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + "_" + file_caller_id + ".wav";
                        
                        if (Directory.Exists(filePath))
                        {
                            File.Delete((loc_path));
                            // Create a file (it always overwrites)
                            FileStream fileStream = new FileStream((loc_path), FileMode.Create);

                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            WaveHeader header = new WaveHeader();
                            WaveFormatChunk format = new WaveFormatChunk();
                            if ((call_state == Call_State.OUT_CALL_START))
                            {
                                call_state = Call_State.OUT_CALL_PROGRESS;
                            }
                            else if ((call_state == Call_State.IN_CALL_STRT))
                            {
                                call_state = Call_State.IN_CALL_PROGRESS;
                            }
                            // Write the header
                            writer.Write(header.sGroupID.ToCharArray());
                            writer.Write(header.dwFileLength);
                            writer.Write(header.sRiffType.ToCharArray());

                            // Write the format chunk
                            writer.Write(format.sChunkID.ToCharArray());
                            writer.Write(format.dwChunkSize);
                            writer.Write(format.wFormatTag);
                            writer.Write(format.wChannels);
                            writer.Write(format.dwSamplesPerSec);
                            writer.Write(format.dwAvgBytesPerSec);
                            writer.Write(format.wBlockAlign);
                            writer.Write(format.wBitsPerSample);
                            writer.Write(format.dummy);
                            // Write the data chunk
                            writer.Write("data".ToCharArray());
                            dwChunkSize = 0;
                            writer.Write(dwChunkSize);
                            for (int i = 0; i < last_element; i++)
                            {
                                writer.Write(data[i]);
                                dwChunkSize++;
                            }

                            writer.Close();
                            fileStream.Close();
                        }
                        else 
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save digital start error\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                    }
                }
                else if ((call_state == Call_State.IN_CALL_PROGRESS) || (call_state == Call_State.OUT_CALL_PROGRESS))
                {
                    try
                    {
                        
                         string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + "_" + file_caller_id + ".wav";
                        if (Directory.Exists(filePath))
                        {
                            FileStream fileStream = new FileStream((loc_path), FileMode.Append);
                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            for (int i = 0; i < last_element; i++)
                            {
                                writer.Write(data[i]);
                                dwChunkSize++;
                            }
                            writer.Close();
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save_ digital call progress error\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                    }
                }
                else if ((call_state == Call_State.OUT_CALL_END) || (call_state == Call_State.IN_CALL_END))
                {
                    try
                    {
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + "_" + file_caller_id + ".wav";
                        string loc_path1 = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + "_" + file_caller_id;

                        if (Directory.Exists(filePath))
                        {
                            // Create a file (it always overwrites)
                            FileStream fileStream = new FileStream((loc_path), FileMode.Open);
                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            call_state = Call_State.LINE_IDLE;
                            writer.Seek(4, SeekOrigin.Begin);
                            uint filesize = (uint)writer.BaseStream.Length;
                            writer.Write(filesize - 8);
                            writer.Seek(42, SeekOrigin.Begin);
                            writer.Write(dwChunkSize);
                            writer.Close();
                            fileStream.Close();

                            //var outputFilePath = loc_path1 + "_NF" + ".wav";
                            //var success = await RemoveNoiseUsingFFmpeg(loc_path, outputFilePath);
                            var outputFilePath = loc_path1 + "_NF" + ".wav";
                            remove_noise(loc_path, outputFilePath);
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save_digital call end error\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                        //  MessageBox.Show(channel_no.ToString() + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "Save_digital5\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                call_state = Call_State.LINE_IDLE;
                // MessageBox.Show(channel_no.ToString() + e.Message);
            }
        }
        public void Save(string filePath, int logger_no, int channel_no, byte[] data, int last_element)
        {
            try
            {
                MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                if ((call_state == Call_State.OUT_CALL_START) || (call_state == Call_State.IN_CALL_STRT))
                {
                    try
                    {
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + ".wav";
                        if (Directory.Exists(filePath))
                        {
                            File.Delete((loc_path));
                            //  Debug.Write("START,");
                            // Create a file (it always overwrites)
                            FileStream fileStream = new FileStream((loc_path), FileMode.Create);

                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            WaveHeader header = new WaveHeader();
                            WaveFormatChunk format = new WaveFormatChunk();
                            if ((call_state == Call_State.OUT_CALL_START))
                            {
                                call_state = Call_State.OUT_CALL_PROGRESS;
                            }
                            else if ((call_state == Call_State.IN_CALL_STRT))
                            {
                                if (channel_type_digital == 1)
                                {
                                    call_state = Call_State.IN_CALL_PROGRESS;
                                }
                                else if (channel_type_digital == 2)
                                {
                                    call_state = Call_State.UHF_IN_CALL_Progress;
                                }
                            }
                            // Write the header
                            writer.Write(header.sGroupID.ToCharArray());
                            writer.Write(header.dwFileLength);
                            writer.Write(header.sRiffType.ToCharArray());

                            // Write the format chunk
                            writer.Write(format.sChunkID.ToCharArray());
                            writer.Write(format.dwChunkSize);
                            writer.Write(format.wFormatTag);
                            writer.Write(format.wChannels);
                            writer.Write(format.dwSamplesPerSec);
                            writer.Write(format.dwAvgBytesPerSec);
                            writer.Write(format.wBlockAlign);
                            writer.Write(format.wBitsPerSample);
                            writer.Write(format.dummy);
                            // Write the data chunk
                            writer.Write("data".ToCharArray());
                            dwChunkSize = 0;
                            writer.Write(dwChunkSize);
                            for (int i = 0; i < last_element; i++)
                            {
                                writer.Write(data[i]);
                                dwChunkSize++;
                            }

                            writer.Close();
                            fileStream.Close();
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save1\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                    }
                }
                else if ((call_state == Call_State.IN_CALL_PICKED) || (call_state == Call_State.UHF_IN_CALL_Progress) || (call_state == Call_State.OUT_CALL_PROGRESS))
                {
                    try
                    {
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + ".wav";
                        if (Directory.Exists(filePath))
                        {
                            FileStream fileStream = new FileStream((loc_path), FileMode.Append);
                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            for (int i = 0; i < last_element; i++)
                            {
                                writer.Write(data[i]);
                                dwChunkSize++;
                            }
                            writer.Close();
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save2\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                    }
                }
                else if ((call_state == Call_State.OUT_CALL_END) || (call_state == Call_State.IN_CALL_END))
                {
                    try
                    {
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + ".wav";
                        string loc_path1 = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving;

                        if (Directory.Exists(filePath))
                        {
                            // Create a file (it always overwrites)
                            FileStream fileStream = new FileStream((loc_path), FileMode.Open);
                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            call_state = Call_State.LINE_IDLE;
                            writer.Seek(4, SeekOrigin.Begin);
                            uint filesize = (uint)writer.BaseStream.Length;
                            writer.Write(filesize - 8);
                            writer.Seek(42, SeekOrigin.Begin);
                            writer.Write(dwChunkSize);
                            writer.Close();

                            fileStream.Close();

                            // <-- Noise Cleaning Process -->
                            var outputFilePath = loc_path1 + "_NF" + ".wav";
                            remove_noise(loc_path, outputFilePath);
                          
                            //var success = await RemoveNoiseUsingFFmpeg(loc_path, outputFilePath);
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save3\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                        //  MessageBox.Show(channel_no.ToString() + e.Message);
                    }
                }
                else if (call_state == Call_State.MISSED_CALL)
                {
                    try
                    {
                        //id = MYSQL_Handling_obt.GetLogID() + 1;
                        string loc_path = (filePath) + logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving + ".wav";
                        if (Directory.Exists(filePath))
                        {
                            // string s = filePath + logger_no.ToString() + "_" + channel_no.ToString();
                            File.Delete((loc_path));
                            // Create a file (it always overwrites)
                            FileStream fileStream = new FileStream((loc_path), FileMode.Create);
                            // Use BinaryWriter to write the bytes to the file
                            BinaryWriter writer = new BinaryWriter(fileStream);
                            WaveHeader header = new WaveHeader();
                            WaveFormatChunk format = new WaveFormatChunk();

                            call_state = Call_State.LINE_IDLE;

                            // Write the header
                            writer.Write(header.sGroupID.ToCharArray());
                            writer.Write(header.dwFileLength);
                            writer.Write(header.sRiffType.ToCharArray());

                            // Write the format chunk
                            writer.Write(format.sChunkID.ToCharArray());
                            writer.Write(format.dwChunkSize);
                            writer.Write(format.wFormatTag);
                            writer.Write(format.wChannels);
                            writer.Write(format.dwSamplesPerSec);
                            writer.Write(format.dwAvgBytesPerSec);
                            writer.Write(format.wBlockAlign);
                            writer.Write(format.wBitsPerSample);

                            // Write the data chunk
                            writer.Write("data".ToCharArray());
                            dwChunkSize = 0;
                            writer.Write(dwChunkSize);
                            for (int i = 0; i < 5000; i++)
                            {
                                writer.Write(i);
                                dwChunkSize++;
                            }


                            writer.Seek(4, SeekOrigin.Begin);
                            uint filesize = (uint)writer.BaseStream.Length;
                            writer.Write(filesize - 8);
                            writer.Seek(40, SeekOrigin.Begin);
                            writer.Write(dwChunkSize);
                            writer.Close();
                            fileStream.Close();

                            //System.IO.FileInfo fi = new System.IO.FileInfo(loc_path);
                            //if (fi.Exists)
                            //{
                            //    loc_path = loc_path.Substring(0, loc_path.IndexOf('.')) + "_" + caller_id + loc_path.Substring(loc_path.IndexOf('.'));
                            //    // Move file with a new name. Hence renamed.  
                            //    fi.MoveTo(loc_path);                            
                            //}  
                            //caller_id = "";
                            //Debug.Write("MISSED CALL");
                        }
                        else
                        {
                            LogFile.WriteToLogFile(loc_path + "Path Does Not Exist\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            call_state = Call_State.LINE_IDLE;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "Save4\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        call_state = Call_State.LINE_IDLE;
                        // MessageBox.Show(channel_no.ToString() + e.Message);
                    }
                    //catch
                    //{
                    //    call_state = Call_State.LINE_IDLE;
                    //    MessageBox.Show("Missed Call Entry Error");
                    //}
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "Save5\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                call_state = Call_State.LINE_IDLE;
                // MessageBox.Show(channel_no.ToString() + e.Message);
            }
            //catch
            //{
            //    call_state = Call_State.LINE_IDLE;
            //    MessageBox.Show("File Saving Error");
            //}

        }

        void remove_noise(string inputFile, string outputFile)
        {
            // Set the path to ffmpeg.exe
            string ffmpegPath = @"C:\ffmpeg\ffmpeg\bin\ffmpeg.exe";

            // Construct the command with full executable path
            string command = $"\"{ffmpegPath}\" -i \"{inputFile}\" -af \"afftdn=nf=-25,anlmdn=3000\" \"{outputFile}\"";

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            try
            {
                cmd.Start();

                // Run the command
                cmd.StandardInput.WriteLine(command);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();

                string output = cmd.StandardOutput.ReadToEnd();
                string error = cmd.StandardError.ReadToEnd();
                cmd.WaitForExit();

                // Optionally log or print outputs
                Console.WriteLine("FFmpeg Output:\n" + output);
                Console.WriteLine("FFmpeg Error:\n" + error);

                if (File.Exists(outputFile))
                {
                    Console.WriteLine("Noise removal complete. Output file created.");
                }
                else
                {
                    Console.WriteLine("Output file not created. Something may have gone wrong.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while removing noise using FFmpeg: " + ex.Message);
            }
        }




        //void remove_noise(string inputFile, string outputFile)
        //{
        //    string command = AppDomain.CurrentDomain.BaseDirectory + $"-i \"{inputFile}\" -af \"afftdn=nf=-25,anlmdn=3000\" \"{outputFile}\"";
        //    Process cmd = new Process();

        //    cmd.StartInfo.FileName = "cmd.exe";
        //    cmd.StartInfo.RedirectStandardInput = true;
        //    cmd.StartInfo.RedirectStandardOutput = true;
        //    cmd.StartInfo.CreateNoWindow = true;
        //    cmd.StartInfo.UseShellExecute = false;
        //    cmd.Start();
        //    cmd.StandardInput.WriteLine(command);
        //    cmd.StandardInput.Flush();
        //    cmd.StandardInput.Close();
        //}
        //private async Task<bool> RemoveNoiseUsingFFmpeg(string inputFile, string outputFile)
        //{
        //    try
        //    {
        //        FFmpeg.SetExecutablesPath(@"C:\ffmpeg\ffmpeg\bin");
        //        // Apply the anlmdn filter with appropriate strength value
        //        await FFmpeg.Conversions.New()
        //                                .AddParameter($"-i \"{inputFile}\" -af \"afftdn=nf=-25,anlmdn=3000\" \"{outputFile}\"")
        //                                .Start();

        //        return File.Exists(outputFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception for debugging purposes
        //        Console.WriteLine($"FFmpeg error: {ex.Message}");
        //        return false;
        //    }
        //}

        public int detect_silence_for_sound_based_call(byte[] array, int last_element)
        {
            int ret = 0;
            if (channel_type_digital == 2)
            {
                if (get_line_voltage_current_range(array, last_element))
                {
                    sound_activation_ticks = System.Environment.TickCount;
                    ret = 1;
                }
                else
                {
                    if (System.Environment.TickCount - sound_activation_ticks >= sound_activation_time_threshold)
                    {
                        sound_activation_ticks = 0;
                        ret = 0;
                    }
                    else
                    {
                        ret = 2;
                    }
                }
            }
            return (ret);
        }
        public bool get_line_voltage_current_range(byte[] array, int last_element)
        {
            bool ret = false;

            for (int i = 0; i < last_element; i++)
            {
                if (((array[i] >= 128) && (array[i] < 175)) || (array[i] < 30))// in mulaw output from MCU value for 0V is 0 2.2 V is around 50 and for 2.4V it is 255 and for 5V it is 128, see mulaw table in MCU
                {
                    ret = true; break;
                }
            }
            return (ret);
        }
        public void detect_silence(byte[] array, int last_element)
        {
            if (channel_type_digital == 1)
            {
                if (get_line_voltage_current_range(array, last_element))
                {
                    silent_start_tick = System.Environment.TickCount;
                }
                else
                {
                    if (System.Environment.TickCount - silent_start_tick >= silence_time_threshold)
                    {
                        silent_start_tick = 0;
                        dtmfDetector.engage_tone_F = 3;
                    }
                }
            }
        }
        public void dtmf_detection_live_calling_wave_file_saving_channel()
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            try
            {
                long len = 0;

                len = ch_Fifo.Length;

                if (len != 0)
                {
                    for (int p = 0; p < 3400; p++)
                    {
                        array_ulaw[p] = 0xFE;
                    }
                    if (len > 600000)
                    {
                        ch_Fifo.Read(array_ulaw, 0, 3400);

                        ch_Fifo.Close();
                        // MessageBox.Show("FIFO LENGTH EXCEPTION TELL TO VIPIN SIR");
                    }
                    else
                    {
                        ch_Fifo.Read(array_ulaw, 0, 3400);
                    }

                    int last_element = Array.IndexOf(array_ulaw, (byte)0xFE);
                    if (last_element < 0)
                    {
                        last_element = 3399;
                    }
                    if (channel_type_digital == 1)
                    {
                        try
                        {
                            dtmfDetector.dtmfDetecting(array_ulaw, last_element);
                            if ((dtmfDetector.incoming_going_F == true) && (silence_cut_enabled == "true"))
                            { detect_silence(array_ulaw, last_element); }

                        }
                        catch (Exception ex)
                        {
                            LogFile.WriteToLogFile(ex.Message + "dtmf_detection_live_calling_wave_file_saving_channel\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            // MessageBox.Show("Detection Error");
                        }

                        if ((dtmfDetector.indexForDialButtons > 0))
                        {
                            for (int k = 0; k < dtmfDetector.indexForDialButtons; k++)
                            {
                                caller_id += dtmfDetector.pDialButtons[k];
                                outgoing_incoming_call_last_digit_tick = System.Environment.TickCount;
                                if ((number_capturing_duration != 5000) && (type_of_call == "Outgoing Call"))
                                {
                                    number_capturing_duration = 5000;
                                }
                            }
                            dtmfDetector.indexForDialButtons = 0;
                        }

                        if ((System.Environment.TickCount - outgoing_incoming_call_last_digit_tick > number_capturing_duration) && (System.Environment.TickCount - incoming_call_start_tick > 5000) && (number_lock == false))
                        {
                            if (caller_id != "")
                            {
                                file_caller_id = caller_id;
                                caller_id_missed_call = caller_id;
                                MYSQL_Handling_obt.update_channel_table_caller_id(logger_id.ToString(), channel_no.ToString(), ((caller_id) + "__" + (digital_caller_id)));
                                // digital_caller_id = "";
                            }
                            number_lock = true;
                        }
                    }
                    else if (channel_type_digital == 2)
                    {
                        //dtmfDetector.dtmfDetecting(array, last_element);
                        if ((detect_silence_for_sound_based_call(array_ulaw, last_element) == 1) && (call_state == Call_State.LINE_IDLE))
                        {
                            if ((call_state != Call_State.IN_CALL_STRT) && (call_state != Call_State.IN_CALL_PROGRESS))
                            {
                                DateTime now = DateTime.Now;
                                string format = "yyyy-MM-dd HH:mm:ss";
                                string format1 = "yyyy_MM_dd_HH_mm_ss_ffffff";
                                string time = "";
                                call_ST_for_file_saving = now.ToString(format1);
                                time = now.ToString(format);
                                call_state = Call_State.IN_CALL_STRT;//incoming start   
                                MYSQL_Handling_obt.update_channel_table_call(logger_id.ToString(), channel_no.ToString(), "NA", "UHF/HF", time, (logger_id.ToString() + "_" + channel_no.ToString() + "_" + call_ST_for_file_saving));
                            }
                        }
                        else if ((call_state != Call_State.LINE_IDLE) && (detect_silence_for_sound_based_call(array_ulaw, last_element) == 0))
                        {
                            DateTime now = DateTime.Now;
                            string format = "yyyy-MM-dd HH:mm:ss";
                            string time = now.ToString(format);
                            live_play = "";
                            call_state = Call_State.IN_CALL_END;//incoming end
                            MYSQL_Handling_obt.update_channel_table_call_et(logger_id.ToString(), channel_no.ToString(), time);
                            MYSQL_Handling_obt.move_channel_table_log_table(logger_id.ToString(), channel_no.ToString());
                            MYSQL_Handling_obt.update_channel_table(logger_id.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
                        }
                    }
                    try
                    {
                        if ((live_play == "true"))
                        {
                            if (Program.logger_all_handle.live_call_playing == false)
                            {
                                Program.logger_all_handle.buffered_wave_provider.ClearBuffer();
                                live_playing = 1;
                                Program.logger_all_handle.live_call_playing = true;
                                if (NAudio.Wave.WaveOut.DeviceCount >= 1)
                                {
                                    Program.logger_all_handle.player.Init(Program.logger_all_handle.buffered_wave_provider);
                                    Program.logger_all_handle.player.Play();
                                }
                                
                            }
                            
                            if (live_playing == 1)
                            {
                                for (int ii = 0, jj = 0; ii < last_element; ii++)
                                {
                                    pcm_sample = NAudio.Codecs.MuLawDecoder.MuLawToLinearSample(array_ulaw[ii]);
                                    //pcm_sample = (short)(pcm_sample >> 4);
                                    array_pcm[jj] = (byte)(pcm_sample & 0xFF); jj++;
                                    array_pcm[jj] = (byte)(pcm_sample >> 8); jj++;
                                }
                                Program.logger_all_handle.buffered_wave_provider.AddSamples(array_pcm, 0, (last_element * 2));

                            }
                        }
                        else if (live_playing == 1)
                        {
                            live_playing = 0;
                            Program.logger_all_handle.live_call_playing = false;
                            Program.logger_all_handle.player.Stop();
                            Program.logger_all_handle.player.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.WriteToLogFile(ex.Message + "dtmf_detection_live_calling_wave_file_saving_channel1\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                        // MessageBox.Show("Live Playing Error");
                    }
                    if (channel_type_digital == 0)
                    {
                        Save_digital(path, logger_id, channel_no, array_ulaw, last_element);
                    }
                    else if (channel_type_digital == 1)
                    {
                        if (System.Environment.TickCount - in_or_out_call_start_tick > 3000)
                            Save(path, logger_id, channel_no, array_ulaw, last_element);
                    }
                    else if (channel_type_digital == 2)
                    {
                        Save(path, logger_id, channel_no, array_ulaw, last_element);
                    }
                }
                else
                {
                    // MessageBox.Show("FIFO EMPTY");
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "dtmf_detection_live_calling_wave_file_saving_channel2\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                // MessageBox.Show("DTMF Detection Error");
            }
        }
        public void Filler(IntPtr data, int size)
        {
            try
            {
                if (m_PlayBuffer == null || m_PlayBuffer.Length < size)
                    m_PlayBuffer = new byte[size];
                if (m_Fifo.Length >= size)
                    m_Fifo.Read(m_PlayBuffer, 0, size);
                else
                {
                    m_Fifo.Read(m_PlayBuffer, 0, (int)m_Fifo.Length);
                    size = (int)m_Fifo.Length;
                    //for (int i = 0; i < m_PlayBuffer.Length; i++)
                    //    m_PlayBuffer[i] = 0;
                }
                System.Runtime.InteropServices.Marshal.Copy(m_PlayBuffer, 0, data, size);
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "filler\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                // MessageBox.Show("Filler Error");
            }
        }


    }
    public partial class logger_unit
    {
        public string path;
        //  public int port_rx_hang_cnt;  
        public bool event_happened;
        public int no_of_channel;
        //  object SpinLock = new object();
        public byte[] msg_buffer;
        public int msg_subscript, FWT_SS_update_flag, FWT_SS_update_flag_pre, FWT_SS_update_flag_tick, FRDRDR_SS_update_flag, FRDRDR_SS_update_flag_pre, FRDRDR_SS_update_flag_tick;
        public int sms_part_subscript, sms_channel_no, sms_no_len, sms_part_receive_tick;
        public char[] sms_CCID;
        public char[] sms_DATE;
        public char[] sms_TIME;
        public char[] sms_sender_NO;
        public string MCU_Version;
        public char[] sms_data;
        public byte[] tx_buffer;
        public byte[] array;
        public byte[] infobytes;
        public byte[] signature;
        public UInt32 tx_buffer_len;
        public int ID;
        public bool configured;
        private readonly WinUsbCommunications _myWinUsbCommunications;
        public WinUsbCommunications.SafeWinUsbHandle port;
        public WinUsbCommunications.DeviceInfo port_info;
        public SafeFileHandle deviceHandle;
        public string devicepathname;
        public string instance_id;
        public byte type;
        public bool first, stop_tx_rx, success_rec, fully_ready;
        public byte status;
        public int fault_cnt;
        public int refresh_logger_cnt;
        public byte update_db_F;
        // public WaveLib.FifoStream logger_Fifo;
        public logger_channel[] channel;
        public uint pre_offset;
        public BackgroundWorker _rxw;
        public int last_time, MSG_BUFF_LEN;
        public int digital_communication_hang_cnt;
        uint bytes_received;
        int alert_sound_ticks;
        public byte last_data_received_F;
        public ManagementEventWatcher _deviceArrivedWatcher;
        public ManagementEventWatcher _deviceRemovedWatcher;
        public bool FWT_OTA_in_progress;
        public bool FRWRDR_OTA_in_progress;
        public int OTA_FILE_subscrpt;
        public int OTA_state;

        public logger_unit()
        {
            path = "";
            MCU_Version = "";
            bytes_received = 0;
            no_of_channel = 0;
            FWT_SS_update_flag = -1;
            FRDRDR_SS_update_flag = -1;
            FWT_SS_update_flag_pre = 0; FWT_SS_update_flag_tick = 0; FRDRDR_SS_update_flag = 0; FRDRDR_SS_update_flag_pre = 0; FRDRDR_SS_update_flag_tick = 0;
            configured = false;
            event_happened = false;
            MSG_BUFF_LEN = 220;
            digital_communication_hang_cnt = 0;
            _myWinUsbCommunications = new WinUsbCommunications();
            tx_buffer = new byte[200];
            sms_channel_no = 0;
            alert_sound_ticks = 0;
            sms_no_len = 0;
            sms_CCID = new char[20];
            sms_DATE = new char[6];
            sms_TIME = new char[6];
            sms_sender_NO = new char[15];
            sms_data = new char[400];
            array = new byte[3500];
            infobytes = new byte[8];
            tx_buffer[0] = (byte)0xFF;
            tx_buffer[1] = (byte)0x01;
            tx_buffer_len = 2;
            pre_offset = 0;
            ID = 0;
            type = 0;
            status = 0;
            msg_buffer = new byte[MSG_BUFF_LEN];
            signature = new byte[4];
            signature[0] = (byte)'A'; signature[1] = (byte)'T'; signature[2] = (byte)'L';
            msg_subscript = 0;
            sms_part_subscript = 0;
            stop_tx_rx = false;
            channel = new logger_channel[0];
            _rxw = null;
            last_data_received_F = 0;

            refresh_logger_cnt = 0;
            success_rec = false;
            fully_ready = false;
            // open write endpoint 1.


            FWT_OTA_in_progress = false;
            FRWRDR_OTA_in_progress = false;
            OTA_FILE_subscrpt = 0;
            OTA_state = 0;








        }

        public void AddDeviceArrivedHandler()
        {
            const Int32 pollingIntervalSeconds = 1;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, pollingIntervalSeconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceArrivedWatcher = new ManagementEventWatcher(scope, q);
                _deviceArrivedWatcher.EventArrived += DeviceAdded;

                _deviceArrivedWatcher.Start();
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "AddDeviceArrivedHandler\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                // Console.WriteLine(e.Message);
                if (_deviceArrivedWatcher != null)
                    _deviceArrivedWatcher.Stop();
            }
        }

        ///  <summary>
        ///  Add a handler to detect removal of devices.
        ///  </summary>

        public void AddDeviceRemovedHandler()
        {
            const Int32 pollingIntervalSeconds = 1;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, pollingIntervalSeconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceRemovedWatcher = new ManagementEventWatcher(scope, q);
                _deviceRemovedWatcher.EventArrived += DeviceRemoved;
                _deviceRemovedWatcher.Start();
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "AddDeviceRemovedHandler\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                // Console.WriteLine(e.Message);
                if (_deviceRemovedWatcher != null)
                    _deviceRemovedWatcher.Stop();
            }
        }

        public void DeviceAdded(object sender, EventArrivedEventArgs e)
        {
            try
            {
                //Boolean success = false;
                //deviceHandle = _myWinUsbCommunications.GetDeviceHandle(devicepathname);
                //_myWinUsbCommunications.InitializeDevice(deviceHandle, ref port, ref port_info, 200);
                //Program.delay_ms(500, 1);
                //_myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkOutPipe, ref success);
                //_myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkInPipe, ref success);
                //refresh_logger = 1;
                // Console.WriteLine("A USB device has been inserted");

                //  _deviceDetected = FindDeviceUsingWmi();
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "DeviceAdded\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                throw;
            }
        }
        public void DeviceRemoved(object sender, EventArgs e)
        {
            try
            {
                //Boolean success=false;

                //_myWinUsbCommunications.Sendfree_usb(port, ref success);

                //stop_tx_rx = true;

                // Console.WriteLine("A USB device has been removed");

                // _deviceDetected = FindDeviceUsingWmi();
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "DeviceRemoved\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");

                throw;
            }
        }
        // 9654376568
        public void receive()
        {
            //success_rec = false;
            //_myWinUsbCommunications.ReceiveDataViaBulkTransfer(port, port_info, (UInt32)3328, ref array, ref bytes_received, ref success_rec);

        }
        public void tx()//vvvvv
        {
            try
            {
                if (stop_tx_rx == false)
                {
                    UInt32 bytes_written = 0;
                    bool success = false;
                    if ((FWT_OTA_in_progress == true) || (FRWRDR_OTA_in_progress == true))
                    {
                        make_ota_pkt();
                        if ((tx_buffer_len == 64) || (tx_buffer_len == 600))
                        {
                            if (tx_buffer_len == 600) tx_buffer_len = 6;
                            _myWinUsbCommunications.SendDataViaBulkTransfer(port, port_info, tx_buffer_len, tx_buffer, ref bytes_written, ref success);
                        }
                        else
                        {
                            if (tx_buffer_len != 0)
                            {
                                _myWinUsbCommunications.SendDataViaBulkTransfer(port, port_info, tx_buffer_len, tx_buffer, ref bytes_written, ref success);
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {

                        // port.PipeStreams[1].Write(tx_buffer, 0, tx_buffer_len); 
                        _myWinUsbCommunications.SendDataViaBulkTransfer(port, port_info, tx_buffer_len, tx_buffer, ref bytes_written, ref success);
                        //if (success != true) fault_cnt_fuc();
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "tx\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                // fault_cnt_fuc();
            }
        }
        public void make_ota_pkt()
        {
            try
            {
                if (OTA_state == 0)
                {
                    tx_buffer_len = 600;// it is 6 only.
                    if (FWT_OTA_in_progress == true)
                    {
                        tx_buffer[0] = (byte)'O'; tx_buffer[1] = (byte)'T'; tx_buffer[2] = (byte)'A'; tx_buffer[3] = (byte)'F'; tx_buffer[4] = (byte)'*'; tx_buffer[5] = (byte)'#';
                    }
                    else
                    {
                        tx_buffer[0] = (byte)'O'; tx_buffer[1] = (byte)'T'; tx_buffer[2] = (byte)'A'; tx_buffer[3] = (byte)'R'; tx_buffer[4] = (byte)'*'; tx_buffer[5] = (byte)'#';
                    }
                    OTA_state = 1;
                }
                else
                {

                    if ((Program.logger_all_handle.OTA_File_in_Raw_Bytes.Length - OTA_FILE_subscrpt) >= 64)
                    {
                        Array.Copy(Program.logger_all_handle.OTA_File_in_Raw_Bytes, OTA_FILE_subscrpt, tx_buffer, 0, 64);
                        OTA_FILE_subscrpt += 64;
                        tx_buffer_len = 64;
                    }
                    else
                    {
                        Array.Copy(Program.logger_all_handle.OTA_File_in_Raw_Bytes, OTA_FILE_subscrpt, tx_buffer, 0, (Program.logger_all_handle.OTA_File_in_Raw_Bytes.Length - OTA_FILE_subscrpt));
                        tx_buffer_len = (UInt32)(Program.logger_all_handle.OTA_File_in_Raw_Bytes.Length - OTA_FILE_subscrpt);
                        OTA_FILE_subscrpt += (int)tx_buffer_len;
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "make_ota_pkt\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
            }
        }
        public void fault_cnt_fuc()
        {

            fault_cnt++;
            bool success = false;
            _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkOutPipe, ref success);
            _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkInPipe, ref success);
            if ((fault_cnt > 200) && (refresh_logger_cnt == 0) && (fully_ready == true))
            {
                stop_tx_rx = true;

                // while (true)
                // {

                //for (int i = 0; i < 1000; i++)
                //{
                //    _myWinUsbCommunications.ReceiveDataViaBulkTransfer(port, port_info, (UInt32)3328, ref array, ref bytes_received, ref success);
                //    if (success == true) break;
                //    if (i % 40 == 0)
                //    {
                //        _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkOutPipe, ref success);
                //        _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkInPipe, ref success);
                //    }
                //}
                // if (success != true)
                {
                    deviceHandle = _myWinUsbCommunications.GetDeviceHandle(devicepathname);

                    //_myWinUsbCommunications.Sendfree_usb(port, ref success);

                    // port.SetHandleAsInvalid();





                    _myWinUsbCommunications.InitializeDevice(deviceHandle, ref port, ref port_info, 200);



                    _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkOutPipe, ref success);
                    _myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkInPipe, ref success);




                }

                // }
                fault_cnt = 0;
                stop_tx_rx = false;
                refresh_logger_cnt = 3;
                // Trace.Write("\nUSB DC=>> " + ID.ToString() + "\n");
            }
        }
        public void tx_to_logger()
        {
            while (true)
            {
                try
                {
                    byte x = 0, fl = 0;
                    System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
                    myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    Program.delay_ms(250, 0);
                    while (configured == true)
                    {
                        digital_communication_hang_cnt++;
                        if (digital_communication_hang_cnt >= 34)// around 5 sec
                        {
                            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                            digital_communication_hang_cnt = 0;
                            for (int g = 0; g < 4; g++)
                            {
                                if (channel[g].FWT_table_exist == true)
                                {
                                    if (channel[g].FWT_PRE_SS != -1)
                                    {
                                        MYSQL_Handling_obt.update_FWT_table(channel[g].logger_id.ToString(), g.ToString(), channel[g].FWT_IMEI, channel[g].FWT_CCID, channel[g].FWT_APP_VERSION, channel[g].FWT_CORE_VERSION, MCU_Version + "dotnet_app_ver->" + Program.logger_all_handle.DOT_NET_APP_VERSION, "-1", "-1", "", "");
                                        channel[g].fwt_last_db_entry_tick = System.Environment.TickCount;
                                    }
                                }
                                channel[g].FWT_PRE_SS = -1;
                                if (channel[g].FRDRDR_table_exist == true)
                                {
                                    if (channel[g].FRDRDR_PRE_SS != -1)
                                    {
                                        MYSQL_Handling_obt.update_FRDRDR_table(channel[g].logger_id.ToString(), g.ToString(), channel[g].FRDRDR_IMEI, channel[g].FRDRDR_CCID, channel[g].FRDRDR_APP_VERSION, channel[g].FRDRDR_CORE_VERSION, "-1", "-1", "-1", "-1");
                                        channel[g].frdrdr_last_db_entry_tick = System.Environment.TickCount;
                                    }
                                }
                                channel[g].FRDRDR_PRE_SS = -1;
                            }
                        }
                        if ((FWT_OTA_in_progress == true) || (FRWRDR_OTA_in_progress == true))
                        {
                            Program.delay_ms(100, 0);
                        }
                        else
                        {
                            Program.delay_ms(500, 0);
                        }


                        // loc_last_data_received_f = last_data_received_F;
                        // if ((loc_last_data_received_f == 0))
                        {
                            fl = 0; x = 0;

                            for (int k = 0; k < no_of_channel && k < 4; k++)
                            {
                                if ((channel[k].call_auto_pick_flag == "true") && (channel[k].channel_type_digital == 1))
                                {
                                    if ((channel[k].call_state == Call_State.IN_CALL_PICKED))
                                    {
                                        x |= (byte)(0x01 << k);
                                    }
                                    if (/*(channel[k].event_happened == true) &&*/  (channel[k].dtmfDetector.engage_tone_F != 0) || (channel[k].channel_cut_call_F == true))
                                    {
                                        if (channel[k].dtmfDetector.engage_tone_F != 0)
                                        {
                                            channel[k].channel_call_cut_reason_F = channel[k].dtmfDetector.engage_tone_F;
                                        }
                                        else
                                        {
                                            channel[k].channel_call_cut_reason_F = 4;
                                        }
                                        channel[k].pick_command_sent = false;
                                        fl = 1; channel[k].channel_cut_call_F = false;
                                        channel[k].dtmfDetector.engage_tone_F = 0;
                                        x &= (byte)(~(0x01 << k));
                                    }
                                    else if (((System.Environment.TickCount - channel[k].incoming_call_start_tick) >= channel[k].call_picking_threshold) && (channel[k].pick_command_sent == false) && (((channel[k].call_state == Call_State.IN_CALL_PROGRESS) && (channel[k].number_lock == true) && (channel[k].ringing_state == true))))
                                    {
                                        fl = 1;
                                        channel[k].pick_command_sent = true;
                                        x |= (byte)(0x01 << k);
                                    }
                                }
                            }

                            if ((fl == 1) && (status == 1))
                            {
                                fl = 0;
                                tx_buffer[0] = 0xFF;
                                tx_buffer[1] = (byte)((byte)0x70 | x);
                                tx_buffer_len = 2;
                            }
                            else
                            {

                                char[] arr;
                                int flg = 0;

                                for (int k = 0; k < no_of_channel && k < 4; k++)
                                {
                                    if (((channel[k].FWT_ACK == true) || (channel[k].FRDRDR_ACK == true)) && (System.Environment.TickCount - channel[k].Last_cmd_time >= 0))//2000
                                    {
                                        if (flg == 0)
                                        {
                                            flg = 1;
                                            tx_buffer[1] = (byte)0;
                                        }
                                        tx_buffer[0] = 0xFF;
                                        int cn;
                                        if (channel[k].FWT_ACK == true)
                                        {
                                            channel[k].FWT_ACK = false;
                                            cn = k;
                                        }
                                        else
                                        {
                                            channel[k].FRDRDR_ACK = false;
                                            cn = k + 4;
                                        }
                                        tx_buffer[1] = (byte)((byte)0x48 | (byte)((byte)cn));
                                        tx_buffer_len = 2;
                                        channel[k].Last_cmd_time = System.Environment.TickCount;
                                        break;
                                    }
                                    else if ((channel[k].FRDRDR_No != "") && (channel[k].FRDRDR_NO_DIALED == false) && (System.Environment.TickCount - channel[k].Last_cmd_time >= 3000))
                                    {
                                        tx_buffer[0] = 0xFF;
                                        tx_buffer[1] = (byte)((byte)0x60 | k);
                                        tx_buffer_len = 2;
                                        channel[k].FRDRDR_NO_DIALED = true;
                                        int jj = 0;
                                        arr = channel[k].FRDRDR_No.ToCharArray();
                                        foreach (char chhh in arr)
                                        {
                                            tx_buffer[2 + jj] = (byte)chhh; jj++;
                                            tx_buffer_len++;
                                        }
                                        tx_buffer[2 + jj] = 0x00; tx_buffer_len++;// termination 
                                        channel[k].FRDRDR_No = "";
                                        channel[k].Last_cmd_time = System.Environment.TickCount;
                                        break;
                                    }
                                }
                            }
                            try
                            {
                                if (status == 0)
                                {
                                    tx_buffer[0] = 0xFF;
                                    tx_buffer[1] = 0x01;
                                    tx_buffer_len = 2;
                                }
                                if ((refresh_logger_cnt != 0) || (first == true) && (status == 1))
                                {
                                    if (refresh_logger_cnt != 0)
                                        refresh_logger_cnt--;
                                    if (refresh_logger_cnt == 0) fault_cnt = 0;
                                    first = false;
                                    tx_buffer[0] = 0xFF;
                                    tx_buffer[1] = (byte)((byte)0x70 | x);
                                    tx_buffer_len = 2;
                                }
                                tx();

                                if (status == 1)// command correction after sending once
                                {

                                    //tx_buffer[0] = 0xFF;
                                    //tx_buffer[1] = 0x90;
                                    //tx_buffer[2] = (byte)'4';
                                    //tx_buffer[3] = (byte)'4';
                                    //tx_buffer[4] = (byte)'5';
                                    //tx_buffer[5] = (byte)'1';
                                    //tx_buffer[6] = (byte)'0';
                                    //tx_buffer_len = 7;

                                    tx_buffer[0] = 0xFF;
                                    tx_buffer[1] = 0x80;
                                    tx_buffer_len = 2;
                                    for (int k = 0; k < no_of_channel && k < 4; k++)
                                    {
                                        if (channel[k].FRDRDR_NO_DIALED == true)
                                        {
                                            tx_buffer[1] |= (byte)((byte)0x01 << k);
                                        }
                                        else
                                        {
                                            tx_buffer[1] &= (byte)(~((byte)((byte)0x01 << k)));
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                LogFile.WriteToLogFile(ex.Message + "tx_to_logger\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                                //MessageBox.Show("transmit exception at logger" + ID.ToString() + "occured tell to vipin sir");
                            }

                            last_data_received_F = 1;
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogFile.WriteToLogFile(ex.Message + "tx_to_logger1\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //  MessageBox.Show("1transmit exception at logger" + ID.ToString() + "occured tell to vipin sir");
                }
            }
        }


         public void read_fifo_and_analyze()
         {
            while (true)
            {
                try
                {
                    for (int j = 0; j < no_of_channel; j++)
                    {
                        channel[j].call_auto_pick_flag = "true";//debug
                        channel[j].path = (path);
                    }
                    int a = 0;
                    Program.delay_ms(250, 0);
                    while (status == 0) { Program.delay_ms(100, 0); }
                    while (configured == true)
                    {
                        //Earlier instead of 1ms delay it was 5ms delay. It was made to 1ms for faster FIFO read.
                        Program.delay_ms(1, 0);
                        for (int j = 0; j < no_of_channel; j++)
                        {
                            //q loop is set to 40 times, now it will read the fifo 40 times in a single loop, earlier it was 20
                            for (int q = 0; (q < 40) && (channel[j].ch_Fifo.Length != 0); q++)
                            {
                                channel[j].dtmfDetector.channel_no = j;
                                channel[j].dtmf_detection_live_calling_wave_file_saving_channel();
                                if (channel[j].channel_type_digital == 1)
                                {
                                    if (channel[j].ring_Fifo.Length > 19999)
                                    {
                                        channel[j].ring_Fifo.Close();
                                    }
                                    if (channel[j].IO_Fifo.Length > 19999)
                                    {
                                        channel[j].IO_Fifo.Close();
                                    }

                                    byte dd = 0;
                                    a = 0;
                                    while ((channel[j].IO_Fifo.Length != 0) && (a < 55))
                                    {
                                        dd = (byte)channel[j].IO_Fifo.ReadByte();
                                        if (dd != (byte)(0xFF))
                                            channel[j].channel_off_on_hook_detection(dd);
                                        a++;
                                    }
                                    Program.delay_ms(1, 0);
                                    a = 0;
                                    while ((channel[j].ring_Fifo.Length != 0) && (a < 55))
                                    {
                                        dd = (byte)channel[j].ring_Fifo.ReadByte();
                                        if (dd != (byte)(0xFF))
                                            channel[j].channel_ring_detection(dd);
                                        a++;
                                    }
                                }
                            }
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.WriteToLogFile(ex.Message + "read_fifo_and_analyze\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //  MessageBox.Show("read and analyze exception at logger" + ID.ToString() + "occured tell to vipin sir");
                }
            }
        }



        //public void read_fifo_and_analyze()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            int a = 0;
        //            Program.delay_ms(250, 0);
        //            while (status == 0) { Program.delay_ms(100, 0); }
        //            while (configured == true)
        //            {
        //                Program.delay_ms(5, 0);
        //                for (int j = 0; j < no_of_channel; j++)
        //                {
        //                    for (int q = 0; (q < 20) && (channel[j].ch_Fifo.Length != 0); q++)
        //                    {
        //                        channel[j].dtmfDetector.channel_no = j;
        //                        channel[j].dtmf_detection_live_calling_wave_file_saving_channel();
        //                        if (channel[j].channel_type_digital == 1)
        //                        {
        //                            if (channel[j].ring_Fifo.Length > 19999)
        //                            {
        //                                channel[j].ring_Fifo.Close();
        //                            }
        //                            if (channel[j].IO_Fifo.Length > 19999)
        //                            {
        //                                channel[j].IO_Fifo.Close();
        //                            }

        //                            byte dd = 0;
        //                            a = 0;
        //                            while ((channel[j].IO_Fifo.Length != 0) && (a < 55))
        //                            {
        //                                dd = (byte)channel[j].IO_Fifo.ReadByte();
        //                                if (dd != (byte)(0xFF))
        //                                    channel[j].channel_off_on_hook_detection(dd);
        //                                a++;
        //                            }
        //                            Program.delay_ms(1, 0);
        //                            a = 0;
        //                            while ((channel[j].ring_Fifo.Length != 0) && (a < 55))
        //                            {
        //                                dd = (byte)channel[j].ring_Fifo.ReadByte();
        //                                if (dd != (byte)(0xFF))
        //                                    channel[j].channel_ring_detection(dd);
        //                                a++;
        //                            }
        //                        }
        //                    }
        //                    channel[j].call_auto_pick_flag = "true";//debug
        //                    channel[j].path = (path);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile.WriteToLogFile(ex.Message + "read_fifo_and_analyze\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
        //            //  MessageBox.Show("read and analyze exception at logger" + ID.ToString() + "occured tell to vipin sir");
        //        }
        //    }
        //}
        public void rx_from_logger()
        {
            long timeout_tick = 0;
            while (true)
            {
                try
                {
                    System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
                    myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    Program.delay_ms(250, 0);
                    while (configured == true)
                    {
                        // Program.delay_ms(1, 0);
                        // loc_last_data_received_f = last_data_received_F;
                        //if ((loc_last_data_received_f == 1))
                        {
                            bytes_received = 0;
                            try
                            {
                                Array.Clear(array, 0, 3500);

                                if (stop_tx_rx == false)
                                {
                                    success_rec = false;
                                    timeout_tick = System.Environment.TickCount;

                                    _myWinUsbCommunications.ReceiveDataViaBulkTransfer(port, port_info, (UInt32)3328, ref array, ref bytes_received, ref success_rec);
                                    if (success_rec == false)
                                    {
                                        Program.delay_ms(5, 0);
                                    }

                                    //if ((System.Environment.TickCount - timeout_tick)>=)
                                    //_myWinUsbCommunications.Sendreset_pipe(port, port_info.BulkInPipe, ref success_rec);

                                    //    Trace.Write(System.Environment.TickCount - timeout_tick); Trace.Write("\n");



                                    //  _myWinUsbCommunications.ReceiveDataViaBulkTransfer(port, port_info, (UInt32)3328, ref array, ref bytes_received, ref success);

                                    //  receive();
                                    //timeout_tick = System.Environment.TickCount;
                                    //Thread t = new Thread(receive);
                                    //t.Start();

                                    //while ((System.Environment.TickCount) < timeout_tick + 10000)
                                    //{
                                    //    if (t.IsAlive == false) { break; }
                                    //    Program.delay_ms(1, 0);
                                    //}
                                    //if ((System.Environment.TickCount) >= timeout_tick + 10000)
                                    //{
                                    //    t.Abort();
                                    //    fault_cnt += 400;
                                    //}
                                    //if (!t.Join(TimeSpan.FromSeconds(4)))//4
                                    //{
                                    //    t.Abort();
                                    //    fault_cnt += 400;
                                    //}

                                }
                                else
                                {
                                    Program.delay_ms(1, 0);
                                }


                                if (success_rec != true)
                                {

                                    fault_cnt_fuc();
                                    bytes_received = 0;

                                    if ((System.Environment.TickCount - alert_sound_ticks) >= 4000)
                                    {
                                        alert_sound_ticks = System.Environment.TickCount;

                                        playExclamation();

                                    }
                                }
                                else
                                {
                                    //refresh_logger_cnt = 0;
                                    fault_cnt = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogFile.WriteToLogFile(ex.Message + "rx_from_logger1\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                                fault_cnt_fuc(); bytes_received = 0;
                            }
                            if (status == 1)
                            {
                                try
                                {
                                    if (bytes_received != 0)
                                        put_logger_fifo_in_channel_fifo(bytes_received - 1);
                                }
                                catch (Exception ex)
                                {
                                    LogFile.WriteToLogFile(ex.Message + "rx_from_logger2\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                                    //MessageBox.Show("receive exception at logger" + ID.ToString() + "occured tell to vipin sir");
                                };
                            }
                            else
                            {
                                try
                                {
                                    if (bytes_received != 0)
                                        check_for_registering_logger(bytes_received);
                                }
                                catch (Exception ex)
                                {
                                    LogFile.WriteToLogFile(ex.Message + "rx_from_logger3\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                                    //MessageBox.Show("receive exception at logger" + ID.ToString() + "occured tell to vipin sir");
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.WriteToLogFile(ex.Message + "rx_from_logger4\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                    //MessageBox.Show("receive exception at logger" + ID.ToString() + "occured tell to vipin sir");
                }
            }


        }
        public void playExclamation()
        {
            SystemSounds.Beep.Play();
        }
        public void check_for_registering_logger(uint len)
        {
            try
            {
                if (len == 0)
                {
                    return;
                }
                int index = Array.IndexOf(array, (byte)0xFE);

                if ((index >= 0) && (array[index + 1] == (byte)'A') && (array[index + 2] == (byte)'T') && (array[index + 3] == (byte)'L'))
                {
                    char[] IDD = new char[5];

                    no_of_channel = array[index + 4] - (byte)0x30;

                    if (array[index + 5] == (byte)'1')
                    {
                        type = (byte)'A';
                    }
                    else
                    {
                        type = (byte)'P';
                    }
                    for (int j = 0; j < 5; j++)
                    {
                        IDD[j] = (char)array[index + 6 + j];
                    }
                    string charsStr = new string(IDD);
                    ID = int.Parse(charsStr);
                    channel = new logger_channel[no_of_channel];
                    for (int k = 0; k < no_of_channel; k++)
                    {
                        channel[k] = new logger_channel();
                    }
                    tx_buffer[0] = (byte)0xFF;
                    tx_buffer[1] = (byte)0x80;
                    tx_buffer_len = 2;
                    MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                    string ch_name = "", channel_type = "";
                    for (int j = 0; j < no_of_channel; j++)
                    {
                        channel[j].status = 1;
                        MYSQL_Handling_obt.get_channel_name(ID.ToString(), j.ToString(), out ch_name, out channel_type);
                        //if (channel_type == "true")
                        //{
                        //    channel[j].channel_type_digital = true;
                        //}
                        //else
                        //{
                        //    channel[j].channel_type_digital = false;
                        //}
                    }
                    status = 1;
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "check_for_registering_logger\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
            }
        }
        public byte get_FWT_CS(byte data)
        {
            byte ret;

            if ((data & (byte)0x08) != 0)
            {
                ret = 1;
            }
            else
            {
                ret = 0;
            }
            return (ret);
        }
        public int get_SS(byte data)
        {
            byte temp = (byte)((byte)(data & (byte)0x07));
            if (temp == (byte)1)
            {
                return (1);//1
            }
            return ((int)temp);
        }
        public int get_CS(byte data)
        {
            return ((int)data);
        }
        public bool get_RS(byte data)
        {
            bool ret;
            int ss = get_SS(data);


            if (ss > 1)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            return (ret);
        }
        public bool check_Numeric_array(char[] ary)
        {
            foreach (char c in ary)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
        public bool check_Numeric(string s)
        {
            foreach (char c in s)
            {
                if ((!char.IsDigit(c)) && (c != '+'))
                {
                    return false;
                }
            }
            return true;
        }
        public void fetch_digital_data()
        {
            if (fully_ready == true)
            {
                if (infobytes[0] == (byte)0xFF)
                {
                    if (infobytes[1] == (byte)'$')
                    {
                        digital_communication_hang_cnt = 0;
                        MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                        msg_subscript = 0;
                        channel[0].FWT_SS = get_SS((byte)(infobytes[2] & (byte)0x0F)); channel[0].FWT_RS = get_RS((byte)(infobytes[2] & (byte)0x0F));
                        channel[0].FRDRDR_SS = get_SS((byte)((byte)(infobytes[2] & (byte)0xF0) >> 4)); channel[0].FRDRDR_RS = get_RS((byte)((byte)(infobytes[2] & (byte)0xF0) >> 4));
                        if (channel[0].channel_type_digital == 0)
                        {
                            if ((channel[0].type_of_call == "Incoming Call") && (get_FWT_CS((byte)(infobytes[2] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[0].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_incoming_call(0);
                            }
                            else if ((channel[0].type_of_call == "Outgoing Call") && (get_FWT_CS((byte)(infobytes[2] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[0].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_outgoing_call(0);
                            }
                        }
                        channel[1].FWT_SS = get_SS((byte)(infobytes[3] & (byte)0x0F)); channel[1].FWT_RS = get_RS((byte)(infobytes[3] & (byte)0x0F));
                        channel[1].FRDRDR_SS = get_SS((byte)((byte)(infobytes[3] & (byte)0xF0) >> 4)); channel[1].FRDRDR_RS = get_RS((byte)((byte)(infobytes[3] & (byte)0xF0) >> 4));
                        if (channel[1].channel_type_digital == 0)
                        {
                            if ((channel[1].type_of_call == "Incoming Call") && (get_FWT_CS((byte)(infobytes[3] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[1].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_incoming_call(1);
                            }
                            else if ((channel[1].type_of_call == "Outgoing Call") && (get_FWT_CS((byte)(infobytes[3] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[1].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_outgoing_call(1);
                            }
                        }
                        channel[2].FWT_SS = get_SS((byte)(infobytes[4] & (byte)0x0F)); channel[2].FWT_RS = get_RS((byte)(infobytes[4] & (byte)0x0F));
                        channel[2].FRDRDR_SS = get_SS((byte)((byte)(infobytes[4] & (byte)0xF0) >> 4)); channel[2].FRDRDR_RS = get_RS((byte)((byte)(infobytes[4] & (byte)0xF0) >> 4));
                        if (channel[2].channel_type_digital == 0)
                        {
                            if ((channel[2].type_of_call == "Incoming Call") && (get_FWT_CS((byte)(infobytes[4] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[2].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_incoming_call(2);
                            }
                            else if ((channel[2].type_of_call == "Outgoing Call") && (get_FWT_CS((byte)(infobytes[4] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[2].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_outgoing_call(2);
                            }
                        }
                        channel[3].FWT_SS = get_SS((byte)(infobytes[5] & (byte)0x0F)); channel[3].FWT_RS = get_RS((byte)(infobytes[5] & (byte)0x0F));
                        channel[3].FRDRDR_SS = get_SS((byte)((byte)(infobytes[5] & (byte)0xF0) >> 4)); channel[3].FRDRDR_RS = get_RS((byte)((byte)(infobytes[5] & (byte)0xF0) >> 4));
                        if (channel[3].channel_type_digital == 0)
                        {
                            if ((channel[3].type_of_call == "Incoming Call") && (get_FWT_CS((byte)(infobytes[5] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[3].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_incoming_call(3);
                            }
                            else if ((channel[3].type_of_call == "Outgoing Call") && (get_FWT_CS((byte)(infobytes[5] & (byte)0x0F)) == 0) && (System.Environment.TickCount - channel[3].FWT_Call_IN_OUT_Digital_Pick_Tick >= 2500))
                            {
                                save_end_outgoing_call(3);
                            }
                        }
                        channel[0].FRDRDR_CS = get_CS((byte)((byte)(infobytes[7] & (byte)0x03) >> 0));
                        channel[1].FRDRDR_CS = get_CS((byte)((byte)(infobytes[7] & (byte)0x0C) >> 2));
                        channel[2].FRDRDR_CS = get_CS((byte)((byte)(infobytes[7] & (byte)0x30) >> 4));
                        channel[3].FRDRDR_CS = get_CS((byte)((byte)(infobytes[7] & (byte)0xC0) >> 6));

                        byte[] aa = new byte[2];

                        if (FWT_SS_update_flag != -1)
                        {
                            if ((channel[FWT_SS_update_flag].FWT_SS != channel[FWT_SS_update_flag].FWT_PRE_SS) || (channel[FWT_SS_update_flag].FWT_RS != channel[FWT_SS_update_flag].FWT_PRE_RS))
                            {
                                if ((configured == true) && (channel[FWT_SS_update_flag].FWT_table_exist == true))
                                {
                                    // MYSQL_Handling_obt.update_FWT_table(channel[FWT_SS_update_flag].logger_id.ToString(), FWT_SS_update_flag.ToString(), channel[FWT_SS_update_flag].FWT_IMEI, channel[FWT_SS_update_flag].FWT_CCID, channel[FWT_SS_update_flag].FWT_APP_VERSION, channel[FWT_SS_update_flag].FWT_CORE_VERSION, MCU_Version, channel[FWT_SS_update_flag].FWT_SS.ToString(), channel[FWT_SS_update_flag].FWT_RS.ToString(), "", "");
                                    channel[FWT_SS_update_flag].FWT_PRE_RS = channel[FWT_SS_update_flag].FWT_RS;
                                    channel[FWT_SS_update_flag].FWT_PRE_SS = channel[FWT_SS_update_flag].FWT_SS;
                                    channel[FWT_SS_update_flag].fwt_last_db_entry_tick = System.Environment.TickCount;
                                }
                            }
                            FWT_SS_update_flag = -1;
                        }
                        else if (System.Environment.TickCount - FWT_SS_update_flag_tick >= 200)
                        {
                            FWT_SS_update_flag_tick = System.Environment.TickCount;
                            FWT_SS_update_flag = FWT_SS_update_flag_pre++;
                            if (FWT_SS_update_flag_pre >= 4) FWT_SS_update_flag_pre = 0;
                        }

                        if (FRDRDR_SS_update_flag != -1)
                        {
                            if ((channel[FRDRDR_SS_update_flag].FRDRDR_SS != channel[FRDRDR_SS_update_flag].FRDRDR_PRE_SS) || (channel[FRDRDR_SS_update_flag].FRDRDR_RS != channel[FRDRDR_SS_update_flag].FRDRDR_PRE_RS))
                            {
                                if ((configured == true) && (channel[FRDRDR_SS_update_flag].FRDRDR_table_exist == true))
                                {
                                    MYSQL_Handling_obt.update_FRDRDR_table(channel[FRDRDR_SS_update_flag].logger_id.ToString(), FRDRDR_SS_update_flag.ToString(), channel[FRDRDR_SS_update_flag].FRDRDR_IMEI, channel[FRDRDR_SS_update_flag].FRDRDR_CCID, channel[FRDRDR_SS_update_flag].FRDRDR_APP_VERSION, channel[FRDRDR_SS_update_flag].FRDRDR_CORE_VERSION, channel[FRDRDR_SS_update_flag].FRDRDR_SS.ToString(), channel[FRDRDR_SS_update_flag].FRDRDR_RS.ToString(), "", "");
                                    channel[FRDRDR_SS_update_flag].FRDRDR_PRE_RS = channel[FRDRDR_SS_update_flag].FRDRDR_RS;
                                    channel[FRDRDR_SS_update_flag].FRDRDR_PRE_SS = channel[FRDRDR_SS_update_flag].FRDRDR_SS;
                                    channel[FRDRDR_SS_update_flag].frdrdr_last_db_entry_tick = System.Environment.TickCount;
                                }
                            }
                            FRDRDR_SS_update_flag = -1;
                        }
                        else if (System.Environment.TickCount - FRDRDR_SS_update_flag_tick >= 200)
                        {
                            FRDRDR_SS_update_flag_tick = System.Environment.TickCount;
                            FRDRDR_SS_update_flag = FRDRDR_SS_update_flag_pre++;
                            if (FRDRDR_SS_update_flag_pre >= 4) FRDRDR_SS_update_flag_pre = 0;
                        }


                        for (int g = 0; g < 4; g++)
                        {
                            if ((configured == true) && (channel[g].FRDRDR_CS != channel[g].FRDRDR_PRE_CS))
                            {
                                if (channel[g].FRDRDR_table_exist == true)
                                {
                                    channel[g].frdrdr_last_db_entry_tick = System.Environment.TickCount;
                                    MYSQL_Handling_obt.update_FRDRDR_table(channel[g].logger_id.ToString(), g.ToString(), channel[g].FRDRDR_IMEI, channel[g].FRDRDR_CCID, channel[FRDRDR_SS_update_flag].FRDRDR_APP_VERSION, channel[FRDRDR_SS_update_flag].FRDRDR_CORE_VERSION, (channel[g].FRDRDR_SS).ToString(), (channel[g].FRDRDR_RS).ToString(), (channel[g].FRDRDR_CS).ToString(), channel[g].FRDRDR_No);
                                    if ((channel[g].FRDRDR_CS != channel[g].FRDRDR_PRE_CS))
                                    {
                                        if ((channel[g].call_state != Call_State.LINE_IDLE) && (channel[g].FRDRDR_No != ""))
                                        {
                                            MYSQL_Handling_obt.update_channel_table_forward_status(channel[g].logger_id.ToString(), g.ToString(), (channel[g].FRDRDR_CS).ToString());
                                        }
                                    }
                                    channel[g].FRDRDR_PRE_RS = channel[g].FRDRDR_RS;
                                    channel[g].FRDRDR_PRE_SS = channel[g].FRDRDR_SS;
                                    channel[g].FRDRDR_PRE_CS = channel[g].FRDRDR_CS;
                                }
                            }
                            if (channel[g].channel_type_digital == 1)
                            {
                                if (((byte)infobytes[6] & (byte)(0x10 << g)) != 0)
                                {
                                    if (channel[g].last_io_value != true)
                                    {
                                        channel[g].last_io_value = true;
                                        aa[0] = (byte)1;
                                        channel[g].IO_Fifo.Write(aa, 0, 1);
                                    }
                                }
                                else
                                {
                                    if (channel[g].last_io_value != false)
                                    {
                                        channel[g].last_io_value = false;
                                        aa[0] = (byte)0;
                                        channel[g].IO_Fifo.Write(aa, 0, 1);
                                    }
                                }

                                if (((byte)infobytes[6] & (byte)(0x01 << g)) != 0)
                                {
                                    if (channel[g].last_ring_value != true)
                                    {
                                        channel[g].last_ring_value = true;
                                        aa[0] = (byte)1;
                                        channel[g].ring_Fifo.Write(aa, 0, 1);
                                    }
                                }
                                else
                                {
                                    if (channel[g].last_ring_value != false)
                                    {
                                        channel[g].last_ring_value = false;
                                        aa[0] = (byte)0;
                                        channel[g].ring_Fifo.Write(aa, 0, 1);
                                    }
                                }
                            }
                        }

                    }
                    else if (infobytes[1] == (byte)'#')
                    {
                        msg_subscript = 0;
                        for (int i = 0; i < 6; i++)
                        {
                            msg_buffer[msg_subscript++] = infobytes[2 + i];
                        }
                    }
                    else if (infobytes[1] == (byte)'%')
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            msg_buffer[msg_subscript++] = infobytes[2 + i];
                        }
                    }
                    else if (infobytes[1] == (byte)'&')
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if ((infobytes[2 + i] == (byte)0x00)) break;
                            msg_buffer[msg_subscript++] = infobytes[2 + i];
                            if (msg_subscript >= 159) break;
                        }

                        process_msg();
                        msg_subscript = 0;
                    }
                }
            }
        }
        public void process_msg()
        {
            if (msg_buffer[1] == (byte)'W')
            {
                digital_communication_hang_cnt = 0;
                if (msg_buffer[2] == (byte)'S')
                {
                    MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();

                    int sms_part_index = msg_buffer[4] - (byte)0x30;


                    if (sms_part_index == 0)
                    {
                        int kk = 5;
                        sms_part_subscript = 0; sms_no_len = 0;
                        for (int g = 0; g < 15; g++)
                        {
                            if (msg_buffer[kk] == (byte)0x00) { kk++; break; }
                            sms_sender_NO[g] = (char)msg_buffer[kk++]; sms_no_len++;
                        }

                        sms_channel_no = msg_buffer[3] - (byte)0x30;
                        for (int g = 0; g < 20; g++)
                        {
                            sms_CCID[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 6; g++)
                        {
                            sms_DATE[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 6; g++)
                        {
                            sms_TIME[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 160; g++)
                        {
                            if (msg_buffer[kk] == (byte)0x00) { kk++; break; }
                            sms_data[g] = (char)msg_buffer[kk++]; sms_part_subscript++;
                        }
                        char[] sms_data_truncated = new char[sms_part_subscript];
                        for (int hh = 0; hh < sms_part_subscript; hh++)
                        {
                            sms_data_truncated[hh] = sms_data[hh];
                        }
                        char[] sms_sender_NO_truncated = new char[sms_no_len];
                        for (int hh = 0; hh < sms_no_len; hh++)
                        {
                            sms_sender_NO_truncated[hh] = sms_sender_NO[hh];
                        }
                        sms_part_subscript = 0; sms_no_len = 0;
                        string str_sms_sender_NO = new string(sms_sender_NO_truncated);
                        string str_CCID = new string(sms_CCID);
                        string str_MSG = new string(sms_data_truncated);
                        string str_DATE = new string(sms_DATE);
                        string str_TIME = new string(sms_TIME);



                        if (check_Numeric(str_TIME))
                        {
                            if (check_Numeric(str_DATE))
                            {
                                if (check_Numeric(str_sms_sender_NO))
                                {
                                    string ch_name = "", channel_type = "";
                                    MYSQL_Handling_obt.get_channel_name(channel[sms_channel_no].logger_id.ToString(), sms_channel_no.ToString(), out ch_name, out channel_type);
                                    MYSQL_Handling_obt.Insert_SMS_table(channel[sms_channel_no].logger_id.ToString(), sms_channel_no.ToString(), ch_name, channel[sms_channel_no].FWT_IMEI, str_sms_sender_NO, str_CCID, str_MSG, str_DATE, str_TIME);
                                }
                                channel[sms_channel_no].FWT_ACK = true;
                            }
                        }
                    }
                    else if (sms_part_index == 1)
                    {
                        sms_part_receive_tick = System.Environment.TickCount;
                        int kk = 5;
                        sms_part_subscript = 0; sms_no_len = 0;
                        for (int g = 0; g < 15; g++)
                        {
                            if (msg_buffer[kk] == (byte)0x00) { kk++; break; }
                            sms_sender_NO[g] = (char)msg_buffer[kk++]; sms_no_len++;
                        }

                        sms_channel_no = msg_buffer[3] - (byte)0x30;
                        for (int g = 0; g < 20; g++)
                        {
                            sms_CCID[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 6; g++)
                        {
                            sms_DATE[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 6; g++)
                        {
                            sms_TIME[g] = (char)msg_buffer[kk++];
                        }


                        for (int g = 0; g < 160; g++)
                        {
                            if (msg_buffer[kk] == (byte)0x00) { kk++; break; }
                            sms_data[g] = (char)msg_buffer[kk++]; sms_part_subscript++;
                        }

                        string str_DATE = new string(sms_DATE);
                        string str_TIME = new string(sms_TIME);



                        if (check_Numeric(str_TIME))
                        {
                            if (check_Numeric(str_DATE))
                            {
                                channel[sms_channel_no].FWT_ACK = true;
                            }
                        }
                    }
                    else if ((sms_part_index == 2) && (System.Environment.TickCount - sms_part_receive_tick) <= 25000)
                    {
                        for (int g = 5; g < 160; g++)
                        {
                            if (msg_buffer[g] == (byte)0x00) { break; }
                            sms_data[sms_part_subscript] = (char)msg_buffer[g]; sms_part_subscript++;
                        }

                        char[] sms_data_truncated = new char[sms_part_subscript];
                        for (int hh = 0; hh < sms_part_subscript; hh++)
                        {
                            sms_data_truncated[hh] = sms_data[hh];
                        }
                        char[] sms_sender_NO_truncated = new char[sms_no_len];
                        for (int hh = 0; hh < sms_no_len; hh++)
                        {
                            sms_sender_NO_truncated[hh] = sms_sender_NO[hh];
                        }
                        sms_part_subscript = 0; sms_no_len = 0;
                        string str_sms_sender_NO = new string(sms_sender_NO_truncated);
                        string str_CCID = new string(sms_CCID);
                        string str_MSG = new string(sms_data_truncated);
                        string str_DATE = new string(sms_DATE);
                        string str_TIME = new string(sms_TIME);



                        if (check_Numeric(str_TIME))
                        {
                            if (check_Numeric(str_DATE))
                            {
                                if (check_Numeric(str_sms_sender_NO))
                                {
                                    string ch_name = "", channel_type = "";
                                    MYSQL_Handling_obt.get_channel_name(channel[sms_channel_no].logger_id.ToString(), sms_channel_no.ToString(), out ch_name, out channel_type);
                                    MYSQL_Handling_obt.Insert_SMS_table(channel[sms_channel_no].logger_id.ToString(), sms_channel_no.ToString(), ch_name, channel[sms_channel_no].FWT_IMEI, str_sms_sender_NO, str_CCID, str_MSG, str_DATE, str_TIME);
                                }
                                channel[sms_channel_no].FWT_ACK = true;
                            }
                        }
                    }
                }
                else if (msg_buffer[2] == (byte)'I')
                {
                    MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                    char[] IMEI = new char[15];
                    char[] CCID = new char[20];
                    char[] FWT_APP_VER_TEMP = new char[35];
                    char[] FWT_CORE_VER_TEMP = new char[80];
                    char[] MCU_VER_TEMP = new char[5];
                    int kk = 4, channel_no = 0, IMEI_len = 0, CCID_len = 0, FWT_APP_VER_len = 0, FWT_CORE_VER_len = 0, MCU_VER_len = 0;
                    for (int g = 0; g < 15; g++)
                    {
                        if (msg_buffer[kk] == ',') { kk++; break; }
                        IMEI[g] = (char)msg_buffer[kk++]; IMEI_len++;
                    }
                    if (msg_buffer[kk] == ',') kk++;
                    for (int g = 0; g < 20; g++)
                    {
                        if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                        CCID[g] = (char)msg_buffer[kk++]; CCID_len++;
                    }
                    if (msg_buffer[kk] == ',') kk++;

                    for (int g = 0; g < 35; g++)
                    {
                        if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                        FWT_APP_VER_TEMP[g] = (char)msg_buffer[kk++]; FWT_APP_VER_len++;
                    }
                    if (msg_buffer[kk] == ',') kk++;

                    for (int g = 0; g < 80; g++)
                    {
                        if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                        FWT_CORE_VER_TEMP[g] = (char)msg_buffer[kk++]; FWT_CORE_VER_len++;
                    }
                    if (msg_buffer[kk] == ',') kk++;

                    for (int g = 0; g < 4; g++)
                    {
                        if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                        MCU_VER_TEMP[g] = (char)msg_buffer[kk++]; MCU_VER_len++;
                    }
                    if (msg_buffer[kk] == ',') kk++;
                    // FWT APP VERSION
                    // FWT CORE VERSION
                    // MCU VERSION
                    char[] IMEI_temmp = new char[IMEI_len];
                    char[] CCID_temmp = new char[CCID_len];
                    char[] FWT_APP_VER_temmp = new char[FWT_APP_VER_len];
                    char[] FWT_CORE_VER_temmp = new char[FWT_CORE_VER_len];
                    char[] MCU_VER_temmp = new char[MCU_VER_len];




                    Array.Copy(IMEI, 0, IMEI_temmp, 0, IMEI_len);
                    Array.Copy(CCID, 0, CCID_temmp, 0, CCID_len);
                    Array.Copy(FWT_APP_VER_TEMP, 0, FWT_APP_VER_temmp, 0, FWT_APP_VER_len);
                    Array.Copy(FWT_CORE_VER_TEMP, 0, FWT_CORE_VER_temmp, 0, FWT_CORE_VER_len);
                    Array.Copy(MCU_VER_TEMP, 0, MCU_VER_temmp, 0, MCU_VER_len);


                    channel_no = (int)(msg_buffer[3] - (byte)0x30);
                    channel[channel_no].FWT_ACK = true;
                    string str_imei = new string(IMEI_temmp);
                    string str_ccid = new string(CCID_temmp);
                    string FWT_APP_VER = new string(FWT_APP_VER_temmp);
                    string FWT_CORE_VER = new string(FWT_CORE_VER_temmp);
                    string MCU_VER = new string(MCU_VER_temmp);
                    if (check_Numeric(str_imei) && (string.Compare(str_imei, channel[channel_no].FWT_IMEI) != 0) || (string.Compare(str_ccid, channel[channel_no].FWT_CCID) != 0))
                    {
                        char[] tt = new char[1];
                        tt[0] = (char)((byte)channel_no + (byte)0x30);
                        string Channel_No = new string(tt);

                        if ((channel[channel_no].FWT_table_exist == false))
                        {
                            channel[channel_no].FWT_IMEI = str_imei;
                            channel[channel_no].FWT_CCID = str_ccid;
                            channel[channel_no].FWT_APP_VERSION = FWT_APP_VER;
                            channel[channel_no].FWT_CORE_VERSION = FWT_CORE_VER;
                            MCU_Version = MCU_VER;
                            channel[channel_no].FWT_table_exist = true;
                            MYSQL_Handling_obt.Insert_FWT_table(channel[channel_no].logger_id.ToString(), Channel_No, channel[channel_no].FWT_IMEI, channel[channel_no].FWT_CCID, FWT_APP_VER, FWT_CORE_VER, MCU_VER + "dotnet_app_ver->" + Program.logger_all_handle.DOT_NET_APP_VERSION, "0", "0");
                        }
                        else
                        {
                            channel[channel_no].FWT_APP_VERSION = FWT_APP_VER;
                            channel[channel_no].FWT_CORE_VERSION = FWT_CORE_VER;
                            MCU_Version = MCU_VER;
                            MYSQL_Handling_obt.update_FWT_table(channel[channel_no].logger_id.ToString(), Channel_No, channel[channel_no].FWT_IMEI, channel[channel_no].FWT_CCID, FWT_APP_VER, FWT_CORE_VER, MCU_VER + "dotnet_app_ver->" + Program.logger_all_handle.DOT_NET_APP_VERSION, channel[channel_no].FWT_SS.ToString(), channel[channel_no].FWT_RS.ToString(), str_imei, str_ccid);
                            channel[channel_no].fwt_last_db_entry_tick = System.Environment.TickCount;
                            channel[channel_no].FWT_IMEI = str_imei;
                            channel[channel_no].FWT_CCID = str_ccid;
                        }
                    }
                }
                else if (msg_buffer[2] == (byte)'C')
                {
                    MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                    char[] number = new char[15];
                    char[] pkt_type = new char[3];
                    int kk = 4, channel_no = 0;
                    channel_no = msg_buffer[3] - (byte)0x30;



                    for (int g = 0; g < 3; g++)
                    {
                        pkt_type[g] = (char)msg_buffer[kk++];
                    }

                    string type = new string(pkt_type);
                    string no = "";
                    if ((type != "INE") && (type != "OTE"))
                    {
                        int p = 0;
                        for (int g = 0; g < 15; g++)
                        {
                            if ((int)msg_buffer[kk] == 0) break;
                            number[g] = (char)msg_buffer[kk++];
                            p++;
                        }
                        char[] pppp = new char[p];
                        Array.Copy(number, 0, pppp, 0, p);
                        no = new string(pppp);
                    }
                    if (channel[channel_no].channel_type_digital == 0)
                    {
                        if (type == "INN")
                        {
                            save_incoming_call(no, channel_no);
                        }
                        else if ((type == "INP") && (channel[channel_no].type_of_call == "Incoming Call"))
                        {
                            save_parallel_call(no, channel_no);
                        }
                        else if ((type == "INE") && (channel[channel_no].type_of_call == "Incoming Call"))
                        {
                            save_end_incoming_call(channel_no);
                        }
                        else if (type == "MIS")
                        {
                            save_missed_call(no, channel_no);
                        }
                        else if (type == "OUT")
                        {
                            save_outgoing_call(no, channel_no);
                        }
                        else if ((type == "OTP") && (channel[channel_no].type_of_call == "Outgoing Call"))
                        {
                            save_parallel_call(no, channel_no);
                        }
                        else if ((type == "OTE") && (channel[channel_no].type_of_call == "Outgoing Call"))
                        {
                            save_end_outgoing_call(channel_no);
                        }
                    }
                    channel[channel_no].FWT_ACK = true;
                }
                //else if (msg_buffer[2] == (byte)'B')
                //{
                //    MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                //    char[] number = new char[15];
                //    int kk = 4, channel_no = 0;
                //    channel_no = msg_buffer[3] - (byte)0x30;
                //    int g = 0;
                //    for (g = 0; g < 15; g++)
                //    {
                //        if ((int)msg_buffer[kk] == 0) break;
                //        number[g] = (char)msg_buffer[kk++];
                //    }
                //    char[] number1 = new char[g];
                //    for (int kl = 0; kl < g; kl++)
                //    {
                //        number1[kl] = number[kl];
                //    }
                //    string Call_No = new string(number1);
                //    channel[channel_no].FWT_ACK = true;
                //    if (channel[channel_no].waiting_number_list == "")
                //    {
                //        channel[channel_no].waiting_number_list = Call_No;
                //    }
                //    else
                //    {
                //        channel[channel_no].waiting_number_list += "_";
                //        channel[channel_no].waiting_number_list +=  Call_No;
                //    }

                //}
            }
            else if (msg_buffer[1] == (byte)'R')
            {
                MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                digital_communication_hang_cnt = 0;
                int channel_no = (int)(msg_buffer[3] - (byte)0x30);
                char[] IMEI = new char[15];
                char[] CCID = new char[20];
                char[] FRDRDR_APP_VER_TEMP = new char[35];
                char[] FRDRDR_CORE_VER_TEMP = new char[80];
                char[] MCU_VER_TEMP = new char[5];
                int kk = 4, IMEI_len = 0, CCID_len = 0, FRDRDR_APP_VER_len = 0, FRDRDR_CORE_VER_len = 0, MCU_VER_len = 0;
                for (int g = 0; g < 15; g++)
                {
                    if (msg_buffer[kk] == ',') { kk++; break; }
                    IMEI[g] = (char)msg_buffer[kk++]; IMEI_len++;
                }
                if (msg_buffer[kk] == ',') kk++;
                for (int g = 0; g < 20; g++)
                {
                    if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                    CCID[g] = (char)msg_buffer[kk++]; CCID_len++;
                }
                if (msg_buffer[kk] == ',') kk++;

                for (int g = 0; g < 35; g++)
                {
                    if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                    FRDRDR_APP_VER_TEMP[g] = (char)msg_buffer[kk++]; FRDRDR_APP_VER_len++;
                }
                if (msg_buffer[kk] == ',') kk++;

                for (int g = 0; g < 80; g++)
                {
                    if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                    FRDRDR_CORE_VER_TEMP[g] = (char)msg_buffer[kk++]; FRDRDR_CORE_VER_len++;
                }
                if (msg_buffer[kk] == ',') kk++;

                for (int g = 0; g < 4; g++)
                {
                    if ((msg_buffer[kk] == ',') || (msg_buffer[kk] == 0x00)) { kk++; break; }
                    MCU_VER_TEMP[g] = (char)msg_buffer[kk++]; MCU_VER_len++;
                }
                char[] IMEI_temmp = new char[IMEI_len];
                char[] CCID_temmp = new char[CCID_len];
                char[] FRDRDR_APP_VER_temmp = new char[FRDRDR_APP_VER_len];
                char[] FRDRDR_CORE_VER_temmp = new char[FRDRDR_CORE_VER_len];
                char[] MCU_VER_temmp = new char[MCU_VER_len];




                Array.Copy(IMEI, 0, IMEI_temmp, 0, IMEI_len);
                Array.Copy(CCID, 0, CCID_temmp, 0, CCID_len);
                Array.Copy(FRDRDR_APP_VER_TEMP, 0, FRDRDR_APP_VER_temmp, 0, FRDRDR_APP_VER_len);
                Array.Copy(FRDRDR_CORE_VER_TEMP, 0, FRDRDR_CORE_VER_temmp, 0, FRDRDR_CORE_VER_len);
                Array.Copy(MCU_VER_TEMP, 0, MCU_VER_temmp, 0, MCU_VER_len);
                channel[channel_no].FRDRDR_ACK = true;
                string str_imei = new string(IMEI_temmp);
                string str_ccid = new string(CCID_temmp);
                string FRDRDR_APP_VER = new string(FRDRDR_APP_VER_TEMP);
                string FRDRDR_CORE_VER = new string(FRDRDR_CORE_VER_TEMP);
                string MCU_VER = new string(MCU_VER_temmp);
                if (check_Numeric(str_imei) && (string.Compare(str_imei, channel[channel_no].FRDRDR_IMEI) != 0) || (string.Compare(str_ccid, channel[channel_no].FRDRDR_CCID) != 0))
                {
                    char[] tt = new char[1];
                    tt[0] = (char)((byte)channel_no + (byte)0x30);
                    string Channel_No = new string(tt);

                    if ((channel[channel_no].FRDRDR_table_exist == false))
                    {
                        channel[channel_no].FRDRDR_IMEI = str_imei;
                        channel[channel_no].FRDRDR_CCID = str_ccid;
                        channel[channel_no].FRDRDR_APP_VERSION = FRDRDR_APP_VER;
                        channel[channel_no].FRDRDR_CORE_VERSION = FRDRDR_CORE_VER;
                        MCU_Version = MCU_VER;
                        channel[channel_no].FRDRDR_table_exist = true;
                        MYSQL_Handling_obt.Insert_FRDRDR_table(channel[channel_no].logger_id.ToString(), Channel_No, channel[channel_no].FRDRDR_IMEI, channel[channel_no].FRDRDR_CCID, FRDRDR_APP_VER, FRDRDR_CORE_VER, "0", "0", "0", "0");
                    }
                    else
                    {
                        channel[channel_no].FRDRDR_APP_VERSION = FRDRDR_APP_VER;
                        channel[channel_no].FRDRDR_CORE_VERSION = FRDRDR_CORE_VER;
                        MCU_Version = MCU_VER;
                        MYSQL_Handling_obt.update_FRDRDR_table(channel[channel_no].logger_id.ToString(), Channel_No, channel[channel_no].FRDRDR_IMEI, channel[channel_no].FRDRDR_CCID, FRDRDR_APP_VER, FRDRDR_CORE_VER, channel[channel_no].FRDRDR_SS.ToString(), channel[channel_no].FRDRDR_RS.ToString(), str_imei, str_ccid);
                        channel[channel_no].frdrdr_last_db_entry_tick = System.Environment.TickCount;
                        channel[channel_no].FRDRDR_IMEI = str_imei;
                        channel[channel_no].FRDRDR_CCID = str_ccid;
                    }
                }
            }
            Array.Clear(msg_buffer, 0, MSG_BUFF_LEN);
        }
        public void save_incoming_call(string no, int channel_no)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();

            DateTime now = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";
            string format1 = "yyyy_MM_dd_HH_mm_ss_ffffff";
            channel[channel_no].call_ST_for_file_saving = now.ToString(format1);
            channel[channel_no].call_state = Call_State.IN_CALL_STRT;//incoming start                                   
            channel[channel_no].FWT_Call_IN_OUT_Digital_Pick_Tick = System.Environment.TickCount;
            channel[channel_no].file_caller_id = no;
            channel[channel_no].waiting_number_list = "";
            channel[channel_no].FRDRDR_No = ""; channel[channel_no].FRDRDR_NO_DIALED = false;
            channel[channel_no].call_start_time = now.ToString(format);
            channel[channel_no].type_of_call = "Incoming Call";
            MYSQL_Handling_obt.update_channel_table_call(ID.ToString(), channel_no.ToString(), no, "PICKED", channel[channel_no].call_start_time, (ID.ToString() + "_" + channel_no.ToString() + "_" + channel[channel_no].call_ST_for_file_saving + "_" + no));
        }
        public void save_parallel_call(string no, int channel_no)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            channel[channel_no].waiting_number_list = channel[channel_no].waiting_number_list + "_" + no;
            MYSQL_Handling_obt.update_channel_table_for_waiting_list(ID.ToString(), channel_no.ToString(), channel[channel_no].waiting_number_list);
        }
        public void save_end_incoming_call(int channel_no)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            DateTime now = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";

            channel[channel_no].call_end_time = now.ToString(format);
            channel[channel_no].call_state = Call_State.IN_CALL_END;//incoming end
            channel[channel_no].type_of_call = "";
            channel[channel_no].live_play = "";
            channel[channel_no].caller_id = "";
            channel[channel_no].waiting_number_list = "";
            channel[channel_no].FRDRDR_No = ""; channel[channel_no].FRDRDR_NO_DIALED = false;
            MYSQL_Handling_obt.update_channel_table_call_et(ID.ToString(), channel_no.ToString(), channel[channel_no].call_end_time.ToString());
            MYSQL_Handling_obt.move_channel_table_log_table(ID.ToString(), channel_no.ToString());
            MYSQL_Handling_obt.update_channel_table(ID.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
        }
        public void save_missed_call(string no, int channel_no)
        {
            if (channel[channel_no].type_of_call != "Incoming Call")
            {
                MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
                DateTime now = DateTime.Now;
                string format = "yyyy-MM-dd HH:mm:ss";
                MYSQL_Handling_obt.update_channel_table_call(ID.ToString(), channel_no.ToString(), no, "Missed Call", now.ToString(format), "");
                MYSQL_Handling_obt.move_channel_table_log_table(ID.ToString(), channel_no.ToString());
                MYSQL_Handling_obt.update_channel_table(ID.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
            }
            else
            {
                save_end_incoming_call(channel_no);
            }
        }
        public void save_outgoing_call(string no, int channel_no)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            DateTime now = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";
            string format1 = "yyyy_MM_dd_HH_mm_ss_ffffff";
            channel[channel_no].call_ST_for_file_saving = now.ToString(format1);
            channel[channel_no].call_start_time = now.ToString(format);
            channel[channel_no].call_state = Call_State.OUT_CALL_START;//outgoing start
            channel[channel_no].file_caller_id = no;
            channel[channel_no].FWT_Call_IN_OUT_Digital_Pick_Tick = System.Environment.TickCount;
            channel[channel_no].waiting_number_list = "";
            channel[channel_no].type_of_call = "Outgoing Call";
            channel[channel_no].caller_id = no;
            channel[channel_no].FRDRDR_No = ""; channel[channel_no].FRDRDR_NO_DIALED = false;
            MYSQL_Handling_obt.update_channel_table_call(ID.ToString(), channel_no.ToString(), no, "Outgoing Call", channel[channel_no].call_start_time, (ID.ToString() + "_" + channel_no.ToString() + "_" + channel[channel_no].call_ST_for_file_saving + "_" + no));
        }
        public void save_end_outgoing_call(int channel_no)
        {
            MYSQL_Handling MYSQL_Handling_obt = new MYSQL_Handling();
            DateTime now = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";

            channel[channel_no].call_end_time = now.ToString(format);
            channel[channel_no].call_state = Call_State.OUT_CALL_END;//outgoing end
            channel[channel_no].type_of_call = "";
            channel[channel_no].live_play = "";
            channel[channel_no].caller_id = "";
            channel[channel_no].waiting_number_list = "";
            channel[channel_no].FRDRDR_No = ""; channel[channel_no].FRDRDR_NO_DIALED = false;
            MYSQL_Handling_obt.update_channel_table_call_et(ID.ToString(), channel_no.ToString(), channel[channel_no].call_end_time.ToString());

            MYSQL_Handling_obt.move_channel_table_log_table(ID.ToString(), channel_no.ToString());
            MYSQL_Handling_obt.update_channel_table(ID.ToString(), channel_no.ToString(), "-1", "-1", "-1", "-1", "-1", "-1", "-1", "", true);
        }


        public void put_logger_fifo_in_channel_fifo(uint last_element)
        {
            try
            {
                uint h = 0, k = 0, j = 0;
                byte flag = 0;

                int[] v = array.Select((b, i) => (b == 0xFF) && (i < last_element) ? i : -1)
                               .Where(i => i != -1).ToArray();

                if (pre_offset == 0)
                {
                    for (uint p = 0; p < no_of_channel; p++)
                    {
                        if (channel[p].pre_channel == 1)
                        {
                            channel[p].pre_channel = 0;
                            h = p;
                            flag = 1;
                        }
                        channel[p].pre_channel = 0;
                    }
                }
                else
                {
                    Array.Copy(array, 0, infobytes, 8 - pre_offset, pre_offset);
                    fetch_digital_data();
                    Array.Clear(infobytes, 0, 8);
                    h = 0;
                }

                if (v.Length > 0)
                {
                    if ((v[0] > 0) && (flag == 1))
                    {
                        try
                        {
                            for (k = pre_offset; k < v[0]; k++, h++)
                            {
                                channel[h % no_of_channel].ch_Fifo.WriteByte(array[k]);
                            }
                        }
                        catch (OutOfMemoryException ex)
                        {
                            LogFile.WriteToLogFile("OutOfMemoryException: " + ex.Message + " in initial write block\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            GC.Collect();
                            return;
                        }
                    }

                    h = 0;

                    for (j = 0; j < v.Length - 1; j++)
                    {
                        Array.Copy(array, v[j], infobytes, 0, 8);
                        fetch_digital_data();
                        Array.Clear(infobytes, 0, 8);

                        try
                        {
                            for (k = 0; k < ((v[j + 1] - v[j]) - 8); k++, h++)
                            {
                                channel[h % no_of_channel].ch_Fifo.WriteByte(array[(v[j] + 8) + k]);
                            }
                        }
                        catch (OutOfMemoryException ex)
                        {
                            LogFile.WriteToLogFile("OutOfMemoryException: " + ex.Message + " in middle write block\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            GC.Collect();
                            return;
                        }
                    }

                    if ((last_element - v[v.Length - 1]) > 8)
                    {
                        Array.Copy(array, v[j], infobytes, 0, 8);
                        fetch_digital_data();
                        Array.Clear(infobytes, 0, 8);

                        try
                        {
                            for (k = 0; k <= ((last_element - v[v.Length - 1]) - 8); k++, h++)
                            {
                                channel[h % no_of_channel].ch_Fifo.WriteByte(array[(v[j] + 8) + k]);
                            }
                            pre_offset = 0;
                        }
                        catch (OutOfMemoryException ex)
                        {
                            LogFile.WriteToLogFile("OutOfMemoryException: " + ex.Message + " in final write block\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                            GC.Collect();
                            return;
                        }
                    }
                    else
                    {
                        pre_offset = (uint)(8 - ((last_element - v[v.Length - 1]) + 1));
                        Array.Copy(array, v[j], infobytes, 0, ((last_element - v[v.Length - 1]) + 1));
                    }

                    channel[h % no_of_channel].pre_channel = 1;
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteToLogFile("Exception: " + ex.Message + " in put_logger_fifo_in_channel_fifo\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
            }
        }





        //public void put_logger_fifo_in_channel_fifo(uint last_element)
        //{
        //    try
        //    {
        //        uint h = 0, k = 0, j = 0;
        //        byte flag = 0;
        //        int[] v = array.Select((b, i) => (b == 0xFF) && (i < last_element) ? i : -1).Where(i => i != -1).ToArray();

        //        if (pre_offset == 0)
        //        {
        //            for (uint p = 0; p < no_of_channel; p++)
        //            { 
        //                if (channel[p].pre_channel == 1)
        //                {
        //                    channel[p].pre_channel = 0;
        //                    h = p;
        //                    flag = 1;
        //                }
        //                channel[p].pre_channel = 0;
        //            }
        //        }
        //        else
        //        {
        //            Array.Copy(array, 0, infobytes, 8 - pre_offset, pre_offset);
        //            fetch_digital_data();
        //            Array.Clear(infobytes, 0, 8);
        //            h = 0;
        //        }

        //        if (v.Length > 0)
        //        {
        //            if ((v[0] > 0) && (flag == 1))
        //            {
        //                //try catch to catch the out of memory exception
        //                try
        //                {
        //                    for (k = pre_offset; k < v[0]; k++, h++)
        //                    {
        //                        channel[h % (no_of_channel)].ch_Fifo.WriteByte(array[k]);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    LogFile.WriteToLogFile(ex.Message + "put_logger_fifo_in_channel_fifo (Line 2962)\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
        //                }
        //            }
        //            h = 0;

        //            for (j = 0; j < v.Length - 1; j++)
        //            {
        //                Array.Copy(array, v[j], infobytes, 0, 8);
        //                fetch_digital_data();
        //                Array.Clear(infobytes, 0, 8);

        //                //try catch to catch the out of memory exception
        //                try
        //                {
        //                    for (k = 0; k < ((v[j + 1] - v[j]) - 8); k++, h++)
        //                    {
        //                        channel[h % (no_of_channel)].ch_Fifo.WriteByte(array[(v[j] + 8) + k]);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    LogFile.WriteToLogFile(ex.Message + "put_logger_fifo_in_channel_fifo (Line 2974)\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
        //                }
        //            }

        //            if ((last_element - v[v.Length - 1]) > 8)
        //            {
        //                Array.Copy(array, v[j], infobytes, 0, 8);
        //                fetch_digital_data();
        //                Array.Clear(infobytes, 0, 8);
        //                for (k = 0; k <= ((last_element - v[v.Length - 1]) - 8); k++, h++)
        //                {
        //                    channel[h % (no_of_channel)].ch_Fifo.WriteByte(array[(v[j] + 8) + k]);
        //                }
        //                pre_offset = 0;
        //            }
        //            else
        //            {
        //                pre_offset = (uint)(8 - ((last_element - v[v.Length - 1]) + 1));
        //                Array.Copy(array, v[j], infobytes, 0, ((last_element - v[v.Length - 1]) + 1));
        //            }
        //            channel[(h) % (no_of_channel)].pre_channel = 1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile.WriteToLogFile(ex.Message + "put_logger_fifo_in_channel_fifo\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
        //    }
        //}       
    }
}