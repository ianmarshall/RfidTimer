﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceTimer.Device.UhfReader18
{
    public static class StaticClassReaderB
    {
        private const string DLLNAME = "Basic2.dll";

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int OpenComPort(int Port, ref byte ComAddr, byte Baud, ref int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CloseComPort();

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int AutoOpenComPort(ref int Port, ref byte ComAddr, byte Baud, ref int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CloseSpecComPort(int Port);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetReaderInformation(ref byte ConAddr, byte[] VersionInfo, ref byte ReaderType, byte[] TrType, ref byte dmaxfre, ref byte dminfre, ref byte powerdBm, ref byte ScanTime, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteComAdr(ref byte ConAddr, ref byte ComAdrData, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPowerDbm(ref byte ConAddr, byte powerDbm, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Writedfre(ref byte ConAddr, ref byte dmaxfre, ref byte dminfre, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Writebaud(ref byte ConAddr, ref byte baud, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteScanTime(ref byte ConAddr, ref byte ScanTime, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int InSelfTestMode(ref byte ConAddr, bool IsSelfTestMode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int RfOutput(ref byte ConAddr, byte onoff, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPWM(ref byte ConAddr, byte PWM, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadPWM(ref byte ConAddr, ref byte PWM, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPowerParameter(ref byte ConAddr, ref byte power, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Getpower(ref byte ConAddr, ref byte power, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CheckPowerParameter(ref byte ConAddr, ref int code, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetStartInformation(ref byte ConAddr, ref byte ADF7020E, ref byte FreE, ref byte addrE, ref byte scnE, ref byte xpwrE, ref byte pwmE, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SolidifyPWMandPowerlist(ref byte ConAddr, byte[] dBm_list, ref int code, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Inventory_G2(ref byte ConAddr, byte AdrTID, byte LenTID, byte TIDFlag, byte[] EPClenandEPC, ref int Totallen, ref int CardNum, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadCard_G2(ref byte ConAddr, byte[] EPC, byte Mem, byte WordPtr, byte Num, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte[] Data, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteCard_G2(ref byte ConAddr, byte[] EPC, byte Mem, byte WordPtr, byte Writedatalen, byte[] Writedata, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, int WrittenDataNum, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteBlock_G2(ref byte ConAddr, byte[] EPC, byte Mem, byte WordPtr, byte Writedatalen, byte[] Writedata, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, int WrittenDataNum, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int EraseCard_G2(ref byte ConAddr, byte[] EPC, byte Mem, byte WordPtr, byte Num, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetCardProtect_G2(ref byte ConAddr, byte[] EPC, byte select, byte setprotect, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DestroyCard_G2(ref byte ConAddr, byte[] EPC, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteEPC_G2(ref byte ConAddr, byte[] Password, byte[] WriteEPC, byte WriteEPClen, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetReadProtect_G2(ref byte ConAddr, byte[] EPC, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetMultiReadProtect_G2(ref byte ConAddr, byte[] Password, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int RemoveReadProtect_G2(ref byte ConAddr, byte[] Password, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CheckReadProtected_G2(ref byte ConAddr, ref byte readpro, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetEASAlarm_G2(ref byte ConAddr, byte[] EPC, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte EAS, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CheckEASAlarm_G2(ref byte ConAddr, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int LockUserBlock_G2(ref byte ConAddr, byte[] EPC, byte[] Password, byte maskadr, byte maskLen, byte maskFlag, byte BlockNum, byte EPClength, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Inventory_6B(ref byte ConAddr, byte[] ID_6B, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int inventory2_6B(ref byte ConAddr, byte Condition, byte StartAddress, byte mask, byte[] ConditionContent, byte[] ID_6B, ref int Cardnum, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadCard_6B(ref byte ConAddr, byte[] ID_6B, byte StartAddress, byte Num, byte[] Data, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WriteCard_6B(ref byte ConAddr, byte[] ID_6B, byte StartAddress, byte[] Writedata, byte Writedatalen, ref int writtenbyte, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int LockByte_6B(ref byte ConAddr, byte[] ID_6B, byte Address, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CheckLock_6B(ref byte ConAddr, byte[] ID_6B, byte Address, ref byte ReLockState, ref int errorcode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWGParameter(ref byte ConAddr, byte Wg_mode, byte Wg_Data_Inteval, byte Wg_Pulse_Width, byte Wg_Pulse_Inteval, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWorkMode(ref byte ConAddr, byte[] Parameter, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetWorkModeParameter(ref byte ConAddr, byte[] Parameter, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadActiveModeData(byte[] ModeData, ref int Datalength, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetAccuracy(ref byte ConAddr, byte Accuracy, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetOffsetTime(ref byte ConAddr, byte OffsetTime, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetFhssMode(ref byte ConAddr, byte FhssMode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFhssMode(ref byte ConAddr, ref byte FhssMode, int PortHandle);

        [DllImport("Basic2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetTriggerTime(ref byte ConAddr, ref byte TriggerTime, int PortHandle);
    }
}
