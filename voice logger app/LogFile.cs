﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace voice_logger_app
{
   public class LogFile
    {



        public delegate T RetryOpenDelegate<T>();

        public static T RetryOpen<T>(RetryOpenDelegate<T> action)
        {

            while (true)
            {

                try
                {

                    return action();

                }

                catch (IOException)
                {

                    System.Threading.Thread.Sleep(50);

                }

            }

        }


        public static void WriteToLogFile(string msg, string folderPath, string logFile, bool isAppend = true, bool isDateTime = true)
        {
            if (voice_logger_app.Program.logger_all_handle.restart_ownself_cnt++ >= 10)
            {
                voice_logger_app.Program.logger_all_handle.restart_ownself = true;
            }
            // create our daily directory

            // string dir = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" + DateTime.Now.ToString("ddMMyyyy");

            string dir = folderPath + "\\" + DateTime.Now.ToString("ddMMyyyy");

            if (!Directory.Exists(dir))

                Directory.CreateDirectory(dir);

            if (!Directory.Exists(dir + "\\ReceivedData"))
                Directory.CreateDirectory(dir + "\\ReceivedData");

            if (!Directory.Exists(dir + "\\WrongProtocol"))
                Directory.CreateDirectory(dir + "\\WrongProtocol");


            string LogFile = dir + "\\" + logFile;


            TextWriter tw = null;

            try
            {

                // create a writer and open the file

                tw = RetryOpen<StreamWriter>(delegate()
                {

                    return new StreamWriter(LogFile, isAppend);



                });


                // write a line of text to the file
                if (isDateTime)
                    tw.WriteLine(DateTime.Now + Environment.NewLine + msg);
                else
                    tw.WriteLine(msg);

            }

            catch { }

            finally
            {

                // close the stream

                if (tw != null)
                {

                    tw.Close();

                    tw.Dispose();

                }

            }

        }
    }
}
