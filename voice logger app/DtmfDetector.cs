 /** Author:       Plyashkevich Viatcheslav <plyashkevich@yandex.ru>
 
  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.
 --------------------------------------------------------------------------
  * All rights reserved.
  */

// coefficient algo  2*cos(2*pi*freq/samp freq)
using System.Diagnostics;
using System;
using voice_logger_app;
public partial class DtmfDetector
{
    
    public const int NUMBER_OF_BUTTONS = 50;
    public char[] pDialButtons = new char[NUMBER_OF_BUTTONS];
    public short indexForDialButtons = 0;
    public double[] Row_Column = new double[14];
    public const int COEFF_NUMBER = 8;
    public static double[] CONSTANTS = { 1.7077378093489, 1.64528103604172, 1.56868698386682, 1.47820456824018, 1.16410402302733, 0.996370210678982, 0.798618389226636, 1.9021130326, 1.881761538, 1.876382671845, 1.9861369139098525912748744956204, 1.9753766811902754523800804953869, 1.9615705608064608982523644722685 };// last three 150,200,250Hz for silence
    public short[] pArraySamples;
    public int SAMPLES = 400; //320
    public int frame_count;
    public char prevDialButton;
    public int space_cnt, space_limit_cnt;
    public char temp_dial_button_prev;
    public int temp_button_cnt, pre_temp_dial_button, temp_dial_button_cnt, number_detect_multiple;                               
    public bool permissionFlag;
    public byte engage_tone_F;
    public bool incoming_going_F;
    public int powerThreshold;
    public int engage_tone_flag;
    public int channel_no;
    public double energy_of_150hz, energy_of_200hz, energy_of_250hz;
    //public string caller_id_dtmf;
    public int engage_tone_pos_neg;
    public long last_engage_event_tick,subscript,total_no_of_bytes;
    public double energy1;
    public double energy2, busy_tone_coefficient;
   //byte[] tempArray = new byte[300];
  //  public bool test=false;
    public int row=0, column=0,locked=0;
    public int pre_row = 0, pre_column = 0;
    public int busy_tone_frequency, busy_tone_threshold;
    // Row0   697    1.707738
    // Row1   770    1.64528
    // Row2   852    1.568687
    // Row3   941    1.47820

    // Column0   1209  1.1641
    // Column1   1336  0.99637
    // Column2   1477  0.79861
    // Column3   0     0

    // 150 -- 1.9861369139098525912748744956204
    // 200 -- 1.9753766811902754523800804953869
    // 250 -- 1.9615705608064608982523644722685
    // 400 -- 1.9021130325903071442328786667588
    // 440 -- 1.881761537908450944648236838194
    // 450 -- 1.876382671844968268904679453372
    // 350 --  1.924910472907
    
    // 512 --  1.840463694738

    /**     
            frameSize_ - The frame's size of the 8 kHz input samples
            public DtmfDetector(int frameSize_)
    */
    public DtmfDetector()
    {
        pDialButtons[0] = '\0';
        pArraySamples = new short[5000 + SAMPLES];
        frame_count = 0;
        prevDialButton = ' ';
        permissionFlag = false;
        busy_tone_threshold = 6000;
        busy_tone_frequency = 400; busy_tone_coefficient = 1.90211303259030714;
        CONSTANTS[10] = 2 * Math.Cos((2 * 3.1415926535897932384626433832795 * 250 / 8000));
        CONSTANTS[11] = 2 * Math.Cos((2 * 3.1415926535897932384626433832795 * 300 / 8000));
        CONSTANTS[12] = 2 * Math.Cos((2 * 3.1415926535897932384626433832795 * 350 / 8000));
    }
    public char dtmfDetection(short[] short_array_samples)
    {
        char ret;        
        goertzel_algo(short_array_samples);
        ret = get_max_power_sub();       
        return(ret);
    }
    
void goertzel_algo(short[] data)
{
    double temp, temp1, temp2, Output, Prev_Output, Prev_Prev_Output;//,total_power;
	int j=0;

    CONSTANTS[7] = busy_tone_coefficient;
    for (int i = 0; i < 10; i++)//13
    {
        Row_Column[i] = 0;
        Prev_Output = 0;
        Prev_Prev_Output = 0;
        temp = 0; temp1 = 0;
        temp2 = 0; Output = 0;
        for (j = 0; j < SAMPLES; j++)
        {            
            temp = ((CONSTANTS[i]) * Prev_Output);
            Output = (double)(data[j]*0.009765625) + temp - Prev_Prev_Output;
            Prev_Prev_Output = Prev_Output;
            Prev_Output = Output;            		
        }
        
        temp = ((CONSTANTS[i]) * (Prev_Prev_Output) * (Prev_Output));
        temp1 = (Prev_Prev_Output * Prev_Prev_Output);
        temp2 = (Prev_Output * Prev_Output);
        Row_Column[i] = (temp1 + temp2 - temp)/100000;  
    }
    energy_of_150hz = Row_Column[10];
    energy_of_200hz = Row_Column[11];
    energy_of_250hz = Row_Column[12];
    if (incoming_going_F == true)
    {
        //Debug.Write("("); Debug.Write(Row_Column[9].ToString()); Debug.Write(")");
        //Trace.Write(Row_Column[7]); Trace.Write("\n");
        if ((Row_Column[7] > busy_tone_threshold/*1*/))//3000
        {
            last_engage_event_tick = System.Environment.TickCount;
            if (engage_tone_flag == 0)
            {
                engage_tone_pos_neg++;
                engage_tone_flag = 1;
            }
        }//5a
        else
        {
            if (((System.Environment.TickCount - last_engage_event_tick) > 400) && (engage_tone_pos_neg != 0))
            {
                last_engage_event_tick = 0;
                engage_tone_pos_neg = 0;
            }           
            if (engage_tone_flag == 1)
            {
                engage_tone_pos_neg++;
            if (engage_tone_pos_neg > 2)
                {
                    engage_tone_pos_neg = 0;
                  engage_tone_F = 1;
                }
                engage_tone_flag = 0;
            }
        }
    }
	
}
char get_max_power_sub()
{
    char return_value=' ';
    double Dbest = 0;
   
    int i=0, j=0,k=0;

    // find best row
	for(i=0; i<4; i++)
	{
        if (Row_Column[i] > Dbest)  // if this is the new best score
		{
            Dbest = Row_Column[i];   // record best score
			j = i;          // and who owns it
		}
	}
    if (space_limit_cnt == 0) { space_limit_cnt = 1; }
    if (powerThreshold == 0) powerThreshold = 800;//800

    energy1 = Dbest;
    row = j;
    Dbest = 0;
     // find best Column
    for (; i < 7; i++)
    {
        if (Row_Column[i] > Dbest)  // if this is the new best score
        {
            Dbest = Row_Column[i];   // record best score
            k = i;          // and who owns it
        }
    }
    energy2 = Dbest;
    column = k;
    
    
   if (energy1 < powerThreshold || energy2 < powerThreshold)
    {
    //if ((channel_no == 0) )
    //  { Trace.Write(energy1); Trace.Write(","); Trace.Write(energy2); Trace.Write("\n"); }
     return (return_value);
    }
        switch (j)
        {
            case 0: switch (k){
                                case 4: return_value='1'; break; 
                                case 5: return_value='2'; break; 
                                case 6: return_value='3'; break; 
                                case 7: return_value='A'; break;
                                   }; 
                                break;
            case 1: switch (k){
                                case 4: return_value='4'; break; 
                                case 5: return_value='5'; break; 
                                case 6: return_value='6'; break; 
                                case 7: return_value='B'; break;
                                    }; 
                                break;
            case 2: switch (k){
                                case 4: return_value='7'; break; 
                                case 5: return_value='8'; break; 
                                case 6: return_value='9'; break; 
                                case 7: return_value='C'; break;
                                    }; 
                                break;
            case 3: switch (k){
                                case 4: return_value='*'; break; 
                                case 5: return_value='0'; break; 
                                case 6: return_value='#'; break; 
                                case 7: return_value='D'; break;
                                    }
                                break;
        }
       //if (channel_no == 3)
       //{ Debug.Write(return_value); Debug.Write("#"); Debug.Write(energy1); Debug.Write(","); Debug.Write(energy2); Debug.Write("\n"); }
       
        return (return_value);	
}

    
 
    // The DTMF detection.
    // The size of a input_frame must be equal of a frameSize, who 
    // was set in constructor.
    public void dtmfDetecting(byte [] input_frame,int size)
    {

        char temp_dial_button;
         int ii;        
         
       for(ii=0; ii < size; ii++)
       {
           pArraySamples[ii + frame_count] =  NAudio.Codecs.MuLawDecoder.MuLawToLinearSample(input_frame[ii]);// should we shift it to right by 4 bits for more accuracy???? try in future
        }
       
       frame_count += size;
       subscript = 0;
       total_no_of_bytes = frame_count;
       int temp_index = 0;
       if(frame_count >= SAMPLES)
        { 
                 while(frame_count >= SAMPLES)
                  {
                              if (temp_index == 0)
                              {
                                  temp_dial_button = dtmfDetection(pArraySamples);
                              }
                              else
                              {
                                  short[] tempArray = new short[SAMPLES];
                                  for (int inc = 0; inc < SAMPLES; ++inc)
                                  {
                                      tempArray[inc] = pArraySamples[temp_index + inc];                                     
                                  }
                                  temp_dial_button = dtmfDetection(tempArray);
                              }
                              if ((prevDialButton != temp_dial_button))// && (temp_dial_button != 'Z'))
                              {
                                  if ((temp_dial_button != ' '))	// Do NOT save space between digits
                                  {
                                        // Trace.Write(temp_dial_button);
                                          pDialButtons[indexForDialButtons++] = temp_dial_button;
                                          pDialButtons[indexForDialButtons] = '\0';
                                          if (indexForDialButtons >= 64)
                                              indexForDialButtons = 0;
                                          prevDialButton = temp_dial_button;
                                     
                                  }
                                  else if (temp_dial_button == ' ')
                                  {
                                      temp_dial_button_cnt = 0;
                                      prevDialButton = temp_dial_button;
                                  }

                              }                       
                       
                        temp_index += SAMPLES;
                        frame_count -= SAMPLES;                            
                  }

                 for(ii=0; ii < frame_count; ii++)
                  {
                   pArraySamples[ii] = pArraySamples[ii + temp_index];
                  }        
        }

    }
   
}		
