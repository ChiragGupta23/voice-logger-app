
using System.Windows.Forms;
using System;
using System.Media;
using System.IO;
namespace wav_audio
{
     public class WaveHeader
        {

            public string sGroupID; // RIFF

            public uint dwFileLength; // total file length minus 8, which is taken up by RIFF

            public string sRiffType; // always WAVE

            public string call_start_time;

            public string call_end_time;


            /// <summary>

            /// Initializes a WaveHeader object with the default values.

            /// </summary>

            public WaveHeader()
            {

                dwFileLength = 0;

                sGroupID = "RIFF";

                sRiffType = "WAVE";

            }


        }

         public class WaveFormatChunk
        {

            public string sChunkID;         // Four bytes: "fmt "

            public uint dwChunkSize;        // Length of header in bytes

            public ushort wFormatTag;       // 1 (MS PCM)

            public ushort wChannels;        // Number of channels

            public uint dwSamplesPerSec;    // Frequency of the audio in Hz... 8000

                    public uint dwAvgBytesPerSec;   // for estimating RAM allocation

                    public ushort wBlockAlign;      // sample frame size, in bytes

                    public ushort wBitsPerSample;    // bits per sample
                       public ushort dummy;    // as per wave format mulaw, if chunk size is 18 then we must put 2 extra bytes.


                    /// <summary>

                    /// Initializes a format chunk with the following properties:

                    /// Sample rate: 8000 Hz

                    /// Channels: Mono

                    /// Bit depth: 16-bit

                    /// </summary>

                    public WaveFormatChunk()
                    {

                        sChunkID = "fmt ";

                        dwChunkSize = 18;

                        wFormatTag = 7;// 1;

                        wChannels = 1;

                        dwSamplesPerSec = 8000;

                        wBitsPerSample = 8;

                        wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));

                        dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
                        dummy = 0;
                    }

                }



     /*   public class WaveDataChunk
        {

            public string sChunkID;     // "data"

            public uint dwChunkSize;    // Length of header in bytes

            public byte[] shortArray;



            /// <summary>

            /// Initializes a new data chunk with default values.

            /// </summary>

            public WaveDataChunk()
            {

                shortArray = new byte[0];

                dwChunkSize = 0;

                sChunkID = "data";

            }
        }*/

       /* public class save_wave_file
        {
           static public void Save(string filePath, WaveHeader header, WaveFormatChunk format, WaveDataChunk data,byte mode)
            {
               
                if (mode == 0)
                {
                    // Create a file (it always overwrites)
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);

                    // Use BinaryWriter to write the bytes to the file
                    BinaryWriter writer = new BinaryWriter(fileStream);

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
                    writer.Write(data.sChunkID.ToCharArray());
                    data.dwChunkSize = 0;
                    writer.Write(data.dwChunkSize);
                    for (int i = 0; data.shortArray[i] != (byte)0xFF && i < 3000; i++)
                    {
                        writer.Write(data.shortArray[i]);
                        data.dwChunkSize++;
                    }
                    writer.Close();
                    fileStream.Close();
                }
                else if (mode == 1)
                {

                    // Create a file (it always overwrites)
                    FileStream fileStream = new FileStream(filePath, FileMode.Append);
                    // Use BinaryWriter to write the bytes to the file
                    BinaryWriter writer = new BinaryWriter(fileStream);

                   
                    for (int i = 0; data.shortArray[i] != (byte)0xFF && i < 3000; i++)
                    {
                        writer.Write(data.shortArray[i]);
                        data.dwChunkSize++;
                    }
                    writer.Close();
                    fileStream.Close();
                }
                else if (mode == 2)
                {
                    // Create a file (it always overwrites)
                    FileStream fileStream = new FileStream(filePath, FileMode.Open);
                    // Use BinaryWriter to write the bytes to the file
                    BinaryWriter writer = new BinaryWriter(fileStream);
                    writer.Seek(4, SeekOrigin.Begin);
                    uint filesize = (uint)writer.BaseStream.Length;
                    writer.Write(filesize - 8);
                    writer.Seek(40, SeekOrigin.Begin);
                    writer.Write(data.dwChunkSize);
                    writer.Close();
                    fileStream.Close();
                } 
                else if (mode == 3)
                {
                    // Create a file (it always overwrites)
                    FileStream fileStream = new FileStream(filePath, FileMode.Append);

                    // Use BinaryWriter to write the bytes to the file
                    BinaryWriter writer = new BinaryWriter(fileStream);

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
                    writer.Write(data.sChunkID.ToCharArray());
                    writer.Write(data.dwChunkSize);
                    foreach (byte dataPoint in data.shortArray)
                    {
                        if (dataPoint == (byte)0xFF) break;
                        writer.Write(dataPoint);
                    }

                    writer.Seek(4, SeekOrigin.Begin);
                    uint filesize = (uint)writer.BaseStream.Length;
                    writer.Write(filesize - 8);

                    // Clean up
                    writer.Close();
                    fileStream.Close();
                }
               
            }
        }*/


}



