using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using voice_logger_app;
namespace WinUsbDemo
{
	///  <summary>
	///  Routines for detecting devices.
	///  </summary>

	sealed internal partial class DeviceManagement
	{
        
		private const String ModuleName = "Device Management";
       
		///  <summary>
		///  Provides a central mechanism for exception handling.
		///  Displays a message box that describes the exception.
		///  </summary>
		///  
		///  <param name="name"> the module where the exception occurred. </param>
		///  <param name="e"> the exception </param>

		internal static void DisplayException(String name, Exception e)
		{
			try
			{
				//  Create an error message.

				String message = "Exception: " + e.Message + Environment.NewLine + "Module: " + name + Environment.NewLine + "Method: " +
						  e.TargetSite.Name;

				const string caption = "Unexpected Exception";

				MessageBox.Show(message, caption, MessageBoxButtons.OK);
				//Debug.Write(message);
			}
			catch (Exception ex)
			{
                LogFile.WriteToLogFile(ex.Message + "DisplayException\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
				DisplayException(ModuleName, ex);
				throw;
			}
		}
        internal Boolean RefreshDevicesFromGuid(Guid myGuid, string pathname)
        {
            Int32 bufferSize = 0;
            IntPtr detailDataBuffer = IntPtr.Zero;
            var deviceInfoSet = new IntPtr();
            Boolean lastDevice = false;
            const uint pipeTimeout = 200;
            var myDeviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();

            try
            {
                // ***
                //  API function

                //  summary 
                //  Retrieves a device information set for a specified group of devices.
                //  SetupDiEnumDeviceInterfaces uses the device information set.

                //  parameters 
                //  Interface class GUID.
                //  Null to retrieve information for all device instances.
                //  Optional handle to a top-level window (unused here).
                //  Flags to limit the returned information to currently present devices 
                //  and devices that expose interfaces in the class specified by the GUID.

                //  Returns
                //  Handle to a device information set for the devices.
                // ***

                deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

                Boolean deviceFound = false;
                Int32 memberIndex = 0;

                // The cbSize element of the MyDeviceInterfaceData structure must be set to
                // the structure's size in bytes. 
                // The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.

                myDeviceInterfaceData.cbSize = Marshal.SizeOf(myDeviceInterfaceData);

                do
                {
                    // Begin with 0 and increment through the device information set until
                    // no more devices are available.					

                    // ***
                    //  API function

                    //  summary
                    //  Retrieves a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
                    //  On return, MyDeviceInterfaceData contains the handle to a
                    //  SP_DEVICE_INTERFACE_DATA structure for a detected device.

                    //  parameters
                    //  DeviceInfoSet returned by SetupDiGetClassDevs.
                    //  Optional SP_DEVINFO_DATA structure that defines a device instance 
                    //  that is a member of a device information set.
                    //  Device interface GUID.
                    //  Index to specify a device in a device information set.
                    //  Pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.

                    //  Returns
                    //  True on success.
                    // ***

                    Boolean success = NativeMethods.SetupDiEnumDeviceInterfaces
                        (deviceInfoSet,
                         IntPtr.Zero,
                         ref myGuid,
                         memberIndex,
                         ref myDeviceInterfaceData);

                    // Find out if a device information set was retrieved.

                    if (!success)
                    {
                        lastDevice = true;
                    }
                    else
                    {
                        // A device is present.

                        // ***
                        //  API function: 

                        //  summary:
                        //  Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
                        //  containing information about a device.
                        //  To retrieve the information, call this function twice.
                        //  The first time returns the size of the structure.
                        //  The second time returns a pointer to the data.

                        //  parameters
                        //  DeviceInfoSet returned by SetupDiGetClassDevs
                        //  SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
                        //  A returned pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA 
                        //  Structure to receive information about the specified interface.
                        //  The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
                        //  Pointer to a variable that will receive the returned required size of the 
                        //  SP_DEVICE_INTERFACE_DETAIL_DATA structure.
                        //  Returned pointer to an SP_DEVINFO_DATA structure to receive information about the device.

                        //  Returns
                        //  True on success.
                        // ***                     

                        NativeMethods.SetupDiGetDeviceInterfaceDetail
                            (deviceInfoSet,
                             ref myDeviceInterfaceData,
                             IntPtr.Zero,
                             0,
                             ref bufferSize,
                             IntPtr.Zero);

                        // Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.

                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        // Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);


                        // Call SetupDiGetDeviceInterfaceDetail again.
                        // This time, pass a pointer to DetailDataBuffer
                        // and the returned required buffer size.

                        NativeMethods.SetupDiGetDeviceInterfaceDetail
                            (deviceInfoSet,
                             ref myDeviceInterfaceData,
                             detailDataBuffer,
                             bufferSize,
                             ref bufferSize,
                             IntPtr.Zero);

                        // Skip over cbsize (4 bytes) to get the address of the devicePathName.

                        var pDevicePathName = new IntPtr(detailDataBuffer.ToInt64() + 4);
                        WinUsbCommunications _myWinUsbCommunications = new WinUsbCommunications();
                        WinUsbCommunications.DeviceInfo _myDevicesInfo = new WinUsbCommunications.DeviceInfo();
                        // Get the String containing the devicePathName.
                        string devicePathName;
                        SafeFileHandle _deviceHandle = null;
                        devicePathName = Marshal.PtrToStringAuto(pDevicePathName);
                        if (devicePathName == pathname)
                        {
                            _deviceHandle = _myWinUsbCommunications.GetDeviceHandle(devicePathName);

                            WinUsbCommunications.SafeWinUsbHandle han = new WinUsbCommunications.SafeWinUsbHandle();
                            _myWinUsbCommunications.InitializeDevice(_deviceHandle, ref han, ref _myDevicesInfo, pipeTimeout);
                            

                            deviceFound = true;
                            break;
                        }
                    }
                    memberIndex = memberIndex + 1;
                }
                while (lastDevice != true);

                return deviceFound;
            }


            catch (Exception ex)
            {
                LogFile.WriteToLogFile(ex.Message + "RefreshDevicesFromGuid\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
                DisplayException(ModuleName, ex);
                throw;
            }
        }
		///  <summary>
		///  Use SetupDi API functions to retrieve the device path name of an
		///  attached device that belongs to a device interface class.
		///  </summary>
		///  
		///  <param name="myGuid"> an interface class GUID. </param>
		///  <param name="devicePathName"> a pointer to the device path name 
		///  of an attached device. </param>
		///  
		///  <returns>
		///   True if a device is found, False if not. 
		///  </returns>

        internal Boolean FindDevicesFromGuid(Guid myGuid, ref List<WinUsbCommunications.SafeWinUsbHandle> winusb_handles,ref List<SafeFileHandle> handles,ref List<WinUsbCommunications.DeviceInfo> device_info,ref List<string> pathname)
		{
			Int32 bufferSize = 0;
			IntPtr detailDataBuffer = IntPtr.Zero;
			var deviceInfoSet = new IntPtr();
			Boolean lastDevice = false;
            const uint pipeTimeout = 200;
			var myDeviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();

			try
			{
				// ***
				//  API function

				//  summary 
				//  Retrieves a device information set for a specified group of devices.
				//  SetupDiEnumDeviceInterfaces uses the device information set.

				//  parameters 
				//  Interface class GUID.
				//  Null to retrieve information for all device instances.
				//  Optional handle to a top-level window (unused here).
				//  Flags to limit the returned information to currently present devices 
				//  and devices that expose interfaces in the class specified by the GUID.

				//  Returns
				//  Handle to a device information set for the devices.
				// ***

				deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

				Boolean deviceFound = false;
				Int32 memberIndex = 0;

				// The cbSize element of the MyDeviceInterfaceData structure must be set to
				// the structure's size in bytes. 
				// The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.
				
				myDeviceInterfaceData.cbSize = Marshal.SizeOf(myDeviceInterfaceData);
				
				do
				{
					// Begin with 0 and increment through the device information set until
					// no more devices are available.					

					// ***
					//  API function

					//  summary
					//  Retrieves a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
					//  On return, MyDeviceInterfaceData contains the handle to a
					//  SP_DEVICE_INTERFACE_DATA structure for a detected device.

					//  parameters
					//  DeviceInfoSet returned by SetupDiGetClassDevs.
					//  Optional SP_DEVINFO_DATA structure that defines a device instance 
					//  that is a member of a device information set.
					//  Device interface GUID.
					//  Index to specify a device in a device information set.
					//  Pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.

					//  Returns
					//  True on success.
					// ***

					Boolean success = NativeMethods.SetupDiEnumDeviceInterfaces
						(deviceInfoSet,
						 IntPtr.Zero,
						 ref myGuid,
						 memberIndex,
						 ref myDeviceInterfaceData);

					// Find out if a device information set was retrieved.

					if (!success)
					{
                        lastDevice = true;
					}
					else
					{
						// A device is present.

						// ***
						//  API function: 

						//  summary:
						//  Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
						//  containing information about a device.
						//  To retrieve the information, call this function twice.
						//  The first time returns the size of the structure.
						//  The second time returns a pointer to the data.

						//  parameters
						//  DeviceInfoSet returned by SetupDiGetClassDevs
						//  SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
						//  A returned pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA 
						//  Structure to receive information about the specified interface.
						//  The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
						//  Pointer to a variable that will receive the returned required size of the 
						//  SP_DEVICE_INTERFACE_DETAIL_DATA structure.
						//  Returned pointer to an SP_DEVINFO_DATA structure to receive information about the device.

						//  Returns
						//  True on success.
						// ***                     

						NativeMethods.SetupDiGetDeviceInterfaceDetail
							(deviceInfoSet,
							 ref myDeviceInterfaceData,
							 IntPtr.Zero,
							 0,
							 ref bufferSize,
							 IntPtr.Zero);

						// Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.
						
						detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

						// Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.

						Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);
						
						
						// Call SetupDiGetDeviceInterfaceDetail again.
						// This time, pass a pointer to DetailDataBuffer
						// and the returned required buffer size.

						NativeMethods.SetupDiGetDeviceInterfaceDetail
							(deviceInfoSet,
							 ref myDeviceInterfaceData,
							 detailDataBuffer,
							 bufferSize,
							 ref bufferSize,
							 IntPtr.Zero);

						// Skip over cbsize (4 bytes) to get the address of the devicePathName.
                        
						var pDevicePathName = new IntPtr(detailDataBuffer.ToInt64() + 4);
                        WinUsbCommunications _myWinUsbCommunications = new WinUsbCommunications();
                        WinUsbCommunications.DeviceInfo _myDevicesInfo = new WinUsbCommunications.DeviceInfo();
						// Get the String containing the devicePathName.
                        string devicePathName;
                        SafeFileHandle _deviceHandle = null;
						devicePathName = Marshal.PtrToStringAuto(pDevicePathName);
                        pathname.Add(devicePathName);
                        _deviceHandle = _myWinUsbCommunications.GetDeviceHandle(devicePathName);
                        handles.Add(_deviceHandle);
                        WinUsbCommunications.SafeWinUsbHandle han = new WinUsbCommunications.SafeWinUsbHandle();
                        _myWinUsbCommunications.InitializeDevice(_deviceHandle, ref han, ref _myDevicesInfo, pipeTimeout);
                       // _myWinUsbCommunications.SetPipePolicy(han, _myDevicesInfo.BulkInPipe, (uint)2, Convert.ToByte(true));
                        winusb_handles.Add(han);
                        device_info.Add(_myDevicesInfo);
                        deviceFound = true;
					}
					memberIndex = memberIndex + 1;
				}
				while (lastDevice != true);

				return deviceFound;
			}
					
			
			catch (Exception ex)
			{
                LogFile.WriteToLogFile(ex.Message + "111\n", AppDomain.CurrentDomain.BaseDirectory, "Exception.txt");
				DisplayException(ModuleName, ex);
				throw;
			}

			finally
			{
				if (detailDataBuffer != IntPtr.Zero)
				{
					// Free the memory allocated previously by AllocHGlobal.

					Marshal.FreeHGlobal(detailDataBuffer);
				}
				if (deviceInfoSet != IntPtr.Zero)
				{
					// ***
					//  API function

					//  summary
					//  Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.

					//  parameters
					//  DeviceInfoSet returned by SetupDiGetClassDevs.

					//  returns
					//  True on success.
					// ***

					NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
				}
			}
		}			
	}
}


        