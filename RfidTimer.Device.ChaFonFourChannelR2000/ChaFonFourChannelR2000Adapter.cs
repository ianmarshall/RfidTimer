using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;
using RaceTimer.Common;
using RaceTimer.Data;
using UHF;
using Timer = System.Windows.Forms.Timer;

namespace RfidTimer.Device.ChaFonFourChannelR2000
{
    public class ChaFonFourChannelR2000Adapter : DeviceAdapterBase, IDeviceAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ReaderProfile _readerProfile;
        private static System.Timers.Timer _aTimer;
        private bool _readerConnected;

        private Timer timer_Buff = new Timer();

        public const int USER = 0x0400;
        public const int WM_SENDTAG = USER + 101;
        public const int WM_SENDTAGSTAT = USER + 102;
        public const int WM_SENDSTATU = USER + 103;
        public const int WM_SENDBUFF = USER + 104;
        public const int WM_MIXTAG = USER + 105;
        public const int WM_SHOWNUM = USER + 106;

        private byte fComAdr = 0xff; //当前操作的ComAdr
        private int ferrorcode;
        private byte fBaud;
        private double fdminfre;
        private double fdmaxfre;
        private int fCmdRet = 30; //所有执行指令的返回值
        private bool fisinventoryscan_6B;
        private int FrmPortIndex;
        private byte[] fOperEPC = new byte[100];
        private byte[] fPassWord = new byte[4];
        private byte[] fOperID_6B = new byte[10];
        ArrayList list = new ArrayList();
        private List<string> epclist = new List<string>();
        private List<string> tidlist = new List<string>();
        private int CardNum1 = 0;
        private string fInventory_EPC_List; //存贮询查列表（如果读取的数据没有变化，则不进行刷新）
        private int frmcomportindex;
        private bool SeriaATflag = false;
        private byte Target = 0;
        private byte InAnt = 0;
        private byte Scantime = 0;
        private byte FastFlag = 0;
        private byte Qvalue = 0;
        private byte Session = 0;
        private int total_turns = 0;//轮数
        private int total_tagnum = 0;//标签数量
        private int CardNum = 0;
        private int total_time = 0;//总时间
        private int targettimes = 0;
        private byte TIDFlag = 0;
        public static byte antinfo = 0;
        private int AA_times = 0;
        private int CommunicationTime = 0;
        //public DeviceClass SelectedDevice;
        //private static List<DeviceClass> DevList;
        //private static SearchCallBack searchCallBack = new SearchCallBack(searchCB);
        private string ReadTypes = "";

        private volatile bool fIsBuffScan = false;
        private volatile bool toStopThread = false;
        private Thread mythread = null;
        private volatile bool fIsInventoryScan = false;

        ~ChaFonFourChannelR2000Adapter()
        {
            CloseConnection();
        }

        public void Setup(ReaderProfile readerProfile)
        {
            _readerProfile = readerProfile;
        }

        public bool BeginReading()
        {


            RecentTags.Clear();
            TagsInView.Clear();



            fCmdRet = RWDev.SetRfPower(ref fComAdr, Convert.ToByte(_readerProfile.PowerDbm), frmcomportindex);

            if (_readerProfile.InventoryMode == InventoryMode.Buffer)
            {

                StartBufferRead();
            }

            // Create a timer with a two second interval.
            _aTimer = new System.Timers.Timer(200);
            // Hook up the Elapsed event for the timer. 
            _aTimer.Elapsed += TimerTick;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = false;
            // _aTimer.Enabled = true;

            StartReadDelay();

            return true;
        }

        private void TimerTick(object sender, EventArgs e)
        {


            if (fIsInventoryScan) return;
            fIsInventoryScan = true;


            switch (_readerProfile.InventoryMode)
            {
                case InventoryMode.Answer:
                    flash_G2();
                    break;
                case InventoryMode.Buffer:
                    ReadBufferData();
                    //    ClearBufferData();
                    break;

                case InventoryMode.RealTime:
                    GetRealtiemeData();
                    //    ClearBufferData();
                    break;
            }

            fIsInventoryScan = false;

            //  ReportTags();
        }

        public bool CloseConnection()
        {
            var result = false;

            try
            {
                if (_readerProfile.ConnectionType == ConnectionType.Serial)
                {
                    fCmdRet = RWDev.CloseSpecComPort(frmcomportindex);
                    result = true;
                }
                else
                {
                    fCmdRet = RWDev.CloseNetPort(FrmPortIndex);
                    result = true;
                }

                _readerConnected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        public bool OpenConnection()
        {
            return _readerProfile.ConnectionType == ConnectionType.Serial ? OpenSerial() : OpenEthernet();
        }

        private bool OpenSerial()
        {
            try
            {
                int portNum = (int)_readerProfile.ComPort;
                FrmPortIndex = 0;
                string strException = string.Empty;
                fBaud = Convert.ToByte(3);
                if (fBaud > 2)
                    fBaud = Convert.ToByte(fBaud + 2);
                fComAdr = 255; //Broadcast address to open the device
                fCmdRet = RWDev.OpenComPort(portNum, ref fComAdr, fBaud, ref FrmPortIndex);
                if (fCmdRet != 0)
                {
                    string strLog = "Connect reader failed: " + GetReturnCodeDesc(fCmdRet);
                    // WriteLog(lrtxtLog, strLog, 1);
                    return false;
                }
                else
                {
                    frmcomportindex = FrmPortIndex;

                    fCmdRet = RWDev.SetRfPower(ref fComAdr, Convert.ToByte(_readerProfile.PowerDbm), frmcomportindex);

                    string strLog = "Connect: "; // + ComboBox_COM.Text + "@" + ComboBox_baud2.Text;
                    //  WriteLog(lrtxtLog, strLog, 0);
                    _readerConnected = true;
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("Serial Communication Error or Occupied - exception message " + e.Message);
            }

            return false;
        }

        private bool OpenEthernet()
        {
            string strException = string.Empty;
            string ipAddress = _readerProfile.IpAddress;
            int nPort = Convert.ToInt32("27011");
            fComAdr = 255;
            FrmPortIndex = 0;
            fCmdRet = RWDev.OpenNetPort(nPort, ipAddress, ref fComAdr, ref FrmPortIndex);
            if (fCmdRet != 0)
            {
                string strLog = "Connect reader failed: " + GetReturnCodeDesc(fCmdRet);
                MessageBox.Show(strLog);
                return false;
            }
            else
            {
                frmcomportindex = FrmPortIndex;
                string strLog = "Connect: " + ipAddress + "@" + nPort.ToString();
                _readerConnected = true;
                return true;
            }
        }

        public bool StopReading()
        {
            switch (_readerProfile.InventoryMode)
            {
                case InventoryMode.Answer:
                    _aTimer.Enabled = false;
                    break;
                case InventoryMode.RealTime:
                    _aTimer.Enabled = false;
                    break;
                case InventoryMode.Buffer:
                    stopBufferRead();
                    break;
            }

            return true;
        }

        public bool UpdateSettings(ReaderProfile readerProfile)
        {
            _readerProfile = readerProfile;

            Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);

            if (_readerConnected)
            {
                RWDev.SetRfPower(ref fComAdr, Convert.ToByte(_readerProfile.PowerDbm), frmcomportindex);
            }

            //   byte dminfre = 0, dmaxfre = 0;
            //   int band = 2;
            //   band = 4;
            //  /// dminfre = Convert.ToByte(((band & 3) << 6) | (ComboBox_dminfre.SelectedIndex & 0x3F));
            ////   dmaxfre = Convert.ToByte(((band & 0x0c) << 4) | (ComboBox_dmaxfre.SelectedIndex & 0x3F));
            //   fCmdRet = RWDev.SetRegion(ref fComAdr, dmaxfre, dminfre, frmcomportindex);
            //   if (fCmdRet != 0)
            //   {
            //       string strLog = "Set region failed: " + GetReturnCodeDesc(fCmdRet);
            //       logger.Log(LogLevel.Error,  strLog);
            //       return false;
            //   }
            //   else
            //   {
            //       string strLog = "Set region success ";
            //       return true;

            //   }
            return true;

        }

        public event EventHandler<EventArgs> OnRecordTag;

        public event EventHandler<EventArgs> OnAssignTag;
        public event EventHandler<EventArgs> OnReportTags;

        private void setWorkMode()
        {
            byte ReadMode = 0;

            if (_readerProfile.InventoryMode == InventoryMode.Answer || _readerProfile.InventoryMode == InventoryMode.Buffer)
            {
                ReadMode = 0;
            }
            else
            {
                ReadMode = 6;
            }
            fCmdRet = RWDev.SetWorkMode(ref fComAdr, ReadMode, frmcomportindex);
        }

        private void ReportTags()
        {
            if (_readerProfile.ReadingMode == ReadingMode.Desktop)
            {
                return;
            }

            onReportTags(new TagsReports($"{RecentTags.Count} recent tags and {TagsInView.Count} tags in view"));

            DateTime currentTime = DateTime.Now.ToLocalTime();

            logger.Log(LogLevel.Trace, "RecentTags count: " + RecentTags.Count);

            logger.Log(LogLevel.Trace, $"TagsInView count: {TagsInView.Count}");

            foreach (KeyValuePair<string, Split> recentTag in RecentTags)
            {
                TimeSpan difference = currentTime - recentTag.Value.DateTimeOfDay;

                if (difference.TotalSeconds > _readerProfile.GatingTime)
                {
                    Split removedTag;

                    if (RecentTags.TryRemove(recentTag.Key, out removedTag))
                    {
                        logger.Log(LogLevel.Trace, "removed " + recentTag.Key);

                        if (removedTag != null)
                        {
                            List<Split> tags;

                            if (TagsInView.TryRemove(removedTag.Epc, out tags))
                            {
                                logger.Log(LogLevel.Trace, $"TagsInView for {removedTag.Epc} count: {tags.Count}");

                                Split nearestTag = tags.OrderByDescending(x => x.Rssi).FirstOrDefault();

                                onRecordTag(nearestTag);
                            }
                            else
                            {
                                onRecordTag(removedTag);
                            }
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Trace, "failed to remove recent tag: " + recentTag.Key);
                    }
                }
            }

        }

        private void flash_G2()
        {
            Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);
            Qvalue = Convert.ToByte(4);
            byte Ant = 0x80;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen, m;
            byte[] EPC = new byte[50000];
            int CardIndex;
            string temps, temp;
            temp = "";
            string sEPC;
            byte MaskMem = 0;
            byte[] MaskAdr = new byte[2];
            byte MaskLen = 0;
            byte[] MaskData = new byte[100];
            byte MaskFlag = 0;
            byte AdrTID = 0;
            byte LenTID = 0;
            AdrTID = 0;
            LenTID = 6;
            MaskFlag = 0;
            int cbtime = System.Environment.TickCount;
            CardNum = 0;
            fCmdRet = RWDev.Inventory_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref CardNum, frmcomportindex);
            int cmdTime = System.Environment.TickCount - cbtime;//命令时间

            if ((fCmdRet == 0x30) || (fCmdRet == 0x37))
            {
                if (_readerProfile.ConnectionType == ConnectionType.Ethernet)
                {
                    if (frmcomportindex > 1023)
                    {
                        fCmdRet = RWDev.CloseNetPort(frmcomportindex);
                        if (fCmdRet == 0) frmcomportindex = -1;
                        Thread.Sleep(1000);
                    }
                    fComAdr = 255;
                    string ipAddress = _readerProfile.IpAddress;
                    int nPort = Convert.ToInt32("27011");
                    fCmdRet = RWDev.OpenNetPort(nPort, ipAddress, ref fComAdr, ref frmcomportindex);
                }
            }
            if (CardNum == 0)
            {
                if (Session > 1)
                    AA_times = AA_times + 1;//没有得到标签只更新状态栏
                IntPtr ptrWnd = IntPtr.Zero;
                // ptrWnd = FindWindow(null, "UHFReader288 Demo V1.16");
                if (ptrWnd != IntPtr.Zero)         // 检查当前统计窗口是否打开
                {
                    string para = fCmdRet.ToString();
                    //  SendMessage(ptrWnd, WM_SENDSTATU, IntPtr.Zero, para);
                }
                return;
            }
            AA_times = 0;

            if ((fCmdRet == 1) || (fCmdRet == 2) || (fCmdRet == 0x26)) //代表已查找结束，
            {
                byte[] daw = new byte[Totallen];
                Array.Copy(EPC, daw, Totallen);
                temps = ByteArrayToHexString(daw);
                if (fCmdRet == 0x26)
                {
                    string SDCMD = temps.Substring(0, 12);
                    temps = temps.Substring(12);
                    daw = HexStringToByteArray(temps);
                    byte[] datas = new byte[6];
                    datas = HexStringToByteArray(SDCMD);
                    int tagrate = datas[0] * 256 + datas[1];
                    int tagnum = datas[2] * 256 * 256 * 256 + datas[3] * 256 * 256 + datas[4] * 256 + datas[5];
                    total_tagnum = total_tagnum + tagnum;
                    IntPtr ptrWnd = IntPtr.Zero;
                    // ptrWnd = FindWindow(null, "UHFReader288 Demo V1.16");
                    if (ptrWnd != IntPtr.Zero) // 检查当前统计窗口是否打开
                    {
                        string para = tagrate.ToString() + "," + total_tagnum.ToString() + "," + cmdTime.ToString();
                        //   SendMessage(ptrWnd, WM_SENDTAGSTAT, IntPtr.Zero, para);
                    }
                }
                m = 0;
                for (CardIndex = 0; CardIndex < CardNum; CardIndex++)
                {
                    EPClen = daw[m] + 1;
                    temp = temps.Substring(m * 2 + 2, EPClen * 2);
                    sEPC = temp.Substring(0, temp.Length - 2);
                    int RSSI = Convert.ToInt32(temp.Substring(temp.Length - 2, 2), 16);
                    m = m + EPClen + 1;
                    if (sEPC.Length != (EPClen - 1) * 2)
                    {
                        return;
                    }
                    IntPtr ptrWnd = IntPtr.Zero;
                    //  ptrWnd = FindWindow(null, "UHFReader288 Demo V1.16");
                    if (ptrWnd != IntPtr.Zero) // 检查当前统计窗口是否打开
                    {
                        string para = sEPC + "," + RSSI.ToString() + " ";
                        //   SendMessage(ptrWnd, WM_SENDTAG, IntPtr.Zero, para);
                    }
                    var readTime = DateTime.Now;

                    var tag = new Split
                    {
                        DateTimeOfDay = readTime,
                        TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                        Epc = sEPC,
                        Rssi = RSSI,
                        SplitName = _readerProfile.Name,
                        SplitDeviceId = _readerProfile.Id,
                        InventorySearchMode = _readerProfile.InventorySearchMode,
                        Antenna = Ant.ToString()
                    };

                    if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                    {
                        onAssignTag(tag);
                        continue;

                    }

                    onRecordTag(tag);
                    continue;

                    TagsInView.AddOrUpdate(sEPC, new List<Split> { tag }, (key, tagsInView) =>
                          {
                              if (tagsInView.Count > 100)
                              {
                                  tagsInView.Clear();
                              }

                              tagsInView.Add(tag);
                              return tagsInView;
                          }
                    );

                    RecentTags.AddOrUpdate(sEPC, tag, (key, oldTag) => tag);
                }
            }
        }

        private void GetRealtiemeData()
        {
            byte[] ScanModeData = new byte[40960];
            int nLen, NumLen;
            string temp1 = "";
            string binarystr1 = "";
            string binarystr2 = "";
            string RSSI = "";
            string AntStr = "";
            string lenstr = "";
            string EPCStr = "";
            int ValidDatalength;
            string temp;
            ValidDatalength = 0;
            DataGridViewRow rows = new DataGridViewRow();
            int xtime = System.Environment.TickCount;
            fCmdRet = RWDev.ReadActiveModeData(ScanModeData, ref ValidDatalength, frmcomportindex);
            if (fCmdRet == 0)
            {
                try
                {
                    byte[] daw = new byte[ValidDatalength];
                    Array.Copy(ScanModeData, 0, daw, 0, ValidDatalength);
                    temp = ByteArrayToHexString(daw);
                    fInventory_EPC_List = fInventory_EPC_List + temp;//把字符串存进列表
                    nLen = fInventory_EPC_List.Length;
                    while (fInventory_EPC_List.Length > 18)
                    {
                        string FlagStr = Convert.ToString(fComAdr, 16).PadLeft(2, '0') + "EE00";//查找头位置标志字符串
                        int nindex = fInventory_EPC_List.IndexOf(FlagStr);
                        if (nindex > 1)
                            fInventory_EPC_List = fInventory_EPC_List.Substring(nindex - 2);
                        else
                        {
                            fInventory_EPC_List = fInventory_EPC_List.Substring(2);
                            continue;
                        }
                        NumLen = Convert.ToInt32(fInventory_EPC_List.Substring(0, 2), 16) * 2 + 2;//取第一个帧的长度
                        if (fInventory_EPC_List.Length < NumLen)
                        {
                            break;
                        }
                        temp1 = fInventory_EPC_List.Substring(0, NumLen);
                        fInventory_EPC_List = fInventory_EPC_List.Substring(NumLen);
                        if (!CheckCRC(temp1)) continue;
                        AntStr = Convert.ToString(Convert.ToInt32(temp1.Substring(8, 2), 16), 2).PadLeft(4, '0');
                        lenstr = Convert.ToString(Convert.ToInt32(temp1.Substring(10, 2), 16), 10);
                        EPCStr = temp1.Substring(12, temp1.Length - 18);
                        RSSI = temp1.Substring(temp1.Length - 6, 2);

                        var readTime = DateTime.Now;

                        var tag = new Split
                        {
                            DateTimeOfDay = readTime,
                            TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                            Epc = EPCStr,
                            Rssi = int.Parse(RSSI),
                            SplitName = _readerProfile.Name,
                            SplitDeviceId = _readerProfile.Id,
                            InventorySearchMode = _readerProfile.InventorySearchMode,
                            Antenna = AntStr
                        };

                        if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                        {
                            onAssignTag(tag);
                            continue;

                        }

                        onRecordTag(tag);

                    }
                }
                catch (System.Exception ex)
                {
                    ex.ToString();
                }
            }

        }

        private bool CheckCRC(string s)
        {
            int i, j;
            int current_crc_value;
            byte crcL, crcH;
            byte[] data = HexStringToByteArray(s);
            current_crc_value = 0xFFFF;
            for (i = 0; i <= (data.Length - 1); i++)
            {
                current_crc_value = current_crc_value ^ (data[i]);
                for (j = 0; j < 8; j++)
                {
                    if ((current_crc_value & 0x01) != 0)
                        current_crc_value = (current_crc_value >> 1) ^ 0x8408;
                    else
                        current_crc_value = (current_crc_value >> 1);
                }
            }
            crcL = Convert.ToByte(current_crc_value & 0xFF);
            crcH = Convert.ToByte((current_crc_value >> 8) & 0xFF);
            if (crcH == 0 && crcL == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private Thread ReadThread = null;

        private void StartBufferRead()
        {
            ClearBufferData();

            TIDFlag = 0;

            total_time = Environment.TickCount;
            total_tagnum = 0;

            toStopThread = false;
            if (fIsBuffScan == false)
            {
                ReadThread = new Thread(InventoryBufferProcess);
                ReadThread.Start();
            }
            timer_Buff.Enabled = true;
            //  }
            //else
            //{
            //    btStartBuff.BackColor = Color.Transparent;
            //    btStartBuff.Text = "Start";
            //    if (fIsBuffScan)
            //    {
            //        toStopThread = true;//标志，接收数据线程判断stop为true，正常情况下会自动退出线程                                
            //        if (ReadThread.Join(3000))
            //        {
            //            try
            //            {
            //                ReadThread.Abort();//若线程无法退出，强制结束
            //            }
            //            catch (Exception exp)
            //            {
            //                MessageBox.Show(exp.Message, "Thread error");
            //            }
            //        }
            //        fIsBuffScan = false;
            //    }
            //    timer_Buff.Enabled = false;
            //}
        }

        private void stopBufferRead()
        {
            ClearBufferData();

            if (fIsBuffScan)
            {
                toStopThread = true;//标志，接收数据线程判断stop为true，正常情况下会自动退出线程                                
                if (ReadThread.Join(3000))
                {
                    try
                    {
                        ReadThread.Abort();//若线程无法退出，强制结束
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message, "Thread error");
                    }
                }
                fIsBuffScan = false;
            }
            timer_Buff.Enabled = false;
        }


        private void InventoryBufferProcess()
        {
            fIsBuffScan = true;
            while (!toStopThread)
            {
                //if (BAnt1.Checked)
                //{
                //    InAnt = 0x80;
                //    GetBuffData();
                //}
                //if (BAnt2.Checked)
                //{
                //    InAnt = 0x81;
                //    GetBuffData();
                //}
                //if (BAnt3.Checked)
                //{
                //    InAnt = 0x82;
                //    GetBuffData();
                //}
                //if (BAnt4.Checked)
                //{
                //    InAnt = 0x83;
                //    GetBuffData();
                //}

                InAnt = 0x80;
                InventoryBufferData();
            }
            fIsBuffScan = false;
        }

        private void InventoryBufferData()
        {
            Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);
            Qvalue = Convert.ToByte(4);
            int TagNum = 0;
            int BufferCount = 0;
            byte MaskMem = 0;
            byte[] MaskAdr = new byte[2];
            byte MaskLen = 0;
            byte[] MaskData = new byte[100];
            byte MaskFlag = 0;
            byte AdrTID = 0;
            byte LenTID = 0;
            AdrTID = 0;
            LenTID = 6;
            MaskFlag = 0;
            int cbtime = System.Environment.TickCount;
            TagNum = 0;
            BufferCount = 0;
            Target = 0;
            Scantime = 0x14;
            Qvalue = 6;
            if (TIDFlag == 0)
                Session = 255;
            else
                Session = 0;
            FastFlag = 1;

            //            fCmdRet = RWDev.Inventory_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref CardNum, frmcomportindex);
            fCmdRet = RWDev.InventoryBuffer_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, ref BufferCount, ref TagNum, frmcomportindex);
            int x_time = System.Environment.TickCount - cbtime;//命令时间
            string strLog = "InventoryBuffer error： " + GetReturnCodeDesc(fCmdRet);
            //WriteLog(lrtxtLog, strLog, 0);
            ///////////设置网络断线重连
            if (fCmdRet == 0)//代表已查找结束，
            {
                IntPtr ptrWnd = IntPtr.Zero;

                total_tagnum = total_tagnum + TagNum;
                int tagrate = (TagNum * 1000) / x_time;//速度等于张数/时间
                string para = BufferCount.ToString() + "," + x_time.ToString() + "," + tagrate.ToString() + "," + total_tagnum.ToString();
                // SendMessage(ptrWnd, WM_SENDBUFF, IntPtr.Zero, para);



            }

            else
            {
                MessageBox.Show(strLog);
            }
        }


        private void ReadBufferData()
        {
            int Totallen = 0;
            int CardNum = 0;
            byte[] pEPCList = new byte[30000];
            //lxLed_BNum.Text = "0";
            //lxLed_Bcmdsud.Text = "0";
            //lxLed_Btoltag.Text = "0";
            //lxLed_Btoltime.Text = "0";
            //lxLed_cmdTime.Text = "0";
            string temp = "";
            fCmdRet = RWDev.ReadBuffer_G2(ref fComAdr, ref Totallen, ref CardNum, pEPCList, frmcomportindex);
            if (fCmdRet == 1)
            {
                int m = 0;
                byte[] daw = new byte[Totallen];
                Array.Copy(pEPCList, daw, Totallen);
                for (int i = 0; i < CardNum; i++)
                {
                    string ant = Convert.ToString(daw[m], 2).PadLeft(4, '0');
                    int len = daw[m + 1];
                    byte[] EPC = new byte[len];
                    Array.Copy(daw, m + 2, EPC, 0, len);
                    string sEPC = ByteArrayToHexString(EPC);
                    int RSSI = daw[m + 2 + len];
                    string sCount = daw[m + 3 + len].ToString();
                    m = m + 4 + len;

                    var readTime = DateTime.Now;

                    var tag = new Split
                    {
                        DateTimeOfDay = readTime,
                        TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                        Epc = sEPC,
                        Rssi = RSSI,
                        SplitName = _readerProfile.Name,
                        SplitDeviceId = _readerProfile.Id,
                        InventorySearchMode = _readerProfile.InventorySearchMode,
                        Antenna = ant
                    };

                    if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                    {
                        onAssignTag(tag);
                        continue;

                    }

                    onRecordTag(tag);

                }

                string strLog = "Read buffer success ";
                //  WriteLog(lrtxtLog, strLog, 0);
            }
            else
            {



                string strLog = "Read buffer failed!： " + GetReturnCodeDesc(fCmdRet);

                MessageBox.Show(strLog);
            }
        }

        private void ClearBufferData()
        {
            fCmdRet = RWDev.ClearBuffer_G2(ref fComAdr, frmcomportindex);
            if (fCmdRet == 0)
            {
                string strLog = "Clear buffer success ";
                //   WriteLog(lrtxtLog, strLog, 0);
            }
            else
            {
                string strLog = "Clear buffer failed";
                //  WriteLog(lrtxtLog, strLog, 0);
            }
        }

        private void btMSetParameter_Click(object sender, EventArgs e)
        {
            //byte[] Parameter = new byte[200];
            //byte MaskMem = 0;
            //byte[] MaskAdr = new byte[2];
            //byte MaskLen = 0;
            //byte[] MaskData = new byte[200];
            //byte MaskFlag = 0;
            //byte AdrTID = 0;
            //byte LenTID = 0;
            //byte TIDFlag = 0;
            //if (MRB_G2.Checked)
            //{
            //    Parameter[0] = 0;
            //}
            //else
            //{
            //    Parameter[0] = 1;
            //}

            //Parameter[1] = (byte)COM_MRPTime.SelectedIndex;
            //Parameter[2] = (byte)com_MFliterTime.SelectedIndex;
            //Parameter[3] = (byte)com_MQ.SelectedIndex;
            //Parameter[4] = (byte)com_MS.SelectedIndex;
            //if (Parameter[4] > 3) Parameter[4] = 255;
            //if (checkBox_mask.Checked)
            //{
            //    if (RBM_EPC.Checked)
            //    {
            //        MaskMem = 1;
            //    }
            //    else if (RBM_TID.Checked)
            //    {
            //        MaskMem = 2;
            //    }
            //    else if (RBM_USER.Checked)
            //    {
            //        MaskMem = 3;
            //    }
            //    if ((txt_Maddr.Text.Length != 4) || (txt_Mlen.Text.Length != 2) || (txt_Mdata.Text.Length % 2 != 0))
            //    {
            //        MessageBox.Show("Mask error!", "information");
            //        return;
            //    }
            //    MaskAdr = HexStringToByteArray(txt_Maddr.Text);
            //    int len = Convert.ToInt32(txt_Mlen.Text, 16);
            //    MaskLen = (byte)len;
            //    MaskData = HexStringToByteArray(txt_Mdata.Text);
            //    MaskFlag = 1;
            //}
            //if (checkBox_tid.Checked)
            //{
            //    AdrTID = Convert.ToByte(txt_mtidaddr.Text, 16);
            //    LenTID = Convert.ToByte(txt_Mtidlen.Text, 16);
            //    TIDFlag = 1;
            //}
            //fCmdRet = RWDev.SetReadParameter(ref fComAdr, Parameter, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, frmcomportindex);
            //if (fCmdRet != 0)
            //{
            //    string strLog = "Set read parameter failed: " + GetReturnCodeDesc(fCmdRet);
            //    WriteLog(lrtxtLog, strLog, 1);
            //}
            //else
            //{
            //    string strLog = "Set read parameter success ";
            //    WriteLog(lrtxtLog, strLog, 0);
            //}

        }

        private void onRecordTag(EventArgs e)
        {
            OnRecordTag?.Invoke(this, e);
        }

        private void onAssignTag(EventArgs e)
        {
            OnAssignTag?.Invoke(this, e);
        }

        private void onReportTags(EventArgs e)
        {
            OnReportTags?.Invoke(this, e);
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }

        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }




        private string GetReturnCodeDesc(int cmdRet)
        {
            switch (cmdRet)
            {
                case 0x00:
                case 0x26:
                    return "success";
                case 0x01:
                    return "Return before Inventory finished";
                case 0x02:
                    return "the Inventory-scan-time overflow";
                case 0x03:
                    return "More Data";
                case 0x04:
                    return "Reader module MCU is Full";
                case 0x05:
                    return "Access Password Error";
                case 0x09:
                    return "Destroy Password Error";
                case 0x0a:
                    return "Destroy Password Error Cannot be Zero";
                case 0x0b:
                    return "Tag Not Support the command";
                case 0x0c:
                    return "Use the commmand,Access Password Cannot be Zero";
                case 0x0d:
                    return "Tag is protected,cannot set it again";
                case 0x0e:
                    return "Tag is unprotected,no need to reset it";
                case 0x10:
                    return "There is some locked bytes,write fail";
                case 0x11:
                    return "can not lock it";
                case 0x12:
                    return "is locked,cannot lock it again";
                case 0x13:
                    return "Parameter Save Fail,Can Use Before Power";
                case 0x14:
                    return "Cannot adjust";
                case 0x15:
                    return "Return before Inventory finished";
                case 0x16:
                    return "Inventory-Scan-Time overflow";
                case 0x17:
                    return "More Data";
                case 0x18:
                    return "Reader module MCU is full";
                case 0x19:
                    return "'Not Support Command Or AccessPassword Cannot be Zero";
                case 0x1A:
                    return "Tag custom function error";
                case 0xF8:
                    return "Check antenna error";
                case 0xF9:
                    return "Command execute error";
                case 0xFA:
                    return "Get Tag,Poor Communication,Inoperable";
                case 0xFB:
                    return "No Tag Operable";
                case 0xFC:
                    return "Tag Return ErrorCode";
                case 0xFD:
                    return "Command length wrong";
                case 0xFE:
                    return "Illegal command";
                case 0xFF:
                    return "Parameter Error";
                case 0x30:
                    return "Communication error";
                case 0x31:
                    return "CRC checksummat error";
                case 0x32:
                    return "Return data length error";
                case 0x33:
                    return "Communication busy";
                case 0x34:
                    return "Busy,command is being executed";
                case 0x35:
                    return "ComPort Opened";
                case 0x36:
                    return "ComPort Closed";
                case 0x37:
                    return "Invalid Handle";
                case 0x38:
                    return "Invalid Port";
                case 0xEE:
                    return "Return Command Error";
                default:
                    return "";
            }
        }

        private void StartReadDelay()
        {
            if (_readerProfile.StartReadDelay == RaceTimer.Data.StartReadDelay.None)
            {
                _aTimer.Enabled = true;
                return;
            }

            int delayMiliSeconds = (int)_readerProfile.StartReadDelay * 1000;

            Timer timer = new Timer { Interval = delayMiliSeconds };
            timer.Tick += (s, e) =>
            {
                _aTimer.Enabled = true;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
