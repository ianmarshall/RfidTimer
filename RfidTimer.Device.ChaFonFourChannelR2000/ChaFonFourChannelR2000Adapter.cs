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

            // Create a timer with a two second interval.
            _aTimer = new System.Timers.Timer(100);
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
            flash_G2();

            ReportTags();
        }

        public bool CloseConnection()
        {
            var result = false;

            try
            {
                if (_readerProfile.ConnectionType == ConnectionType.Serial)
                {
                    RWDev.CloseSpecComPort((int)_readerProfile.ComPort);
                    result = true;
                }
                else
                {
                    fCmdRet = RWDev.CloseNetPort(FrmPortIndex);
                    result = true;
                }
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

                    //   fCmdRet = RWDev.SetRfPower(ref fComAdr, Convert.ToByte(_readerProfile.PowerDbm), frmcomportindex);

                    string strLog = "Connect: "; // + ComboBox_COM.Text + "@" + ComboBox_baud2.Text;
                    //  WriteLog(lrtxtLog, strLog, 0);
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
                return true;
            }
        }

        public bool StopReading()
        {
            _aTimer.Enabled = false;
            return true;
        }

        public event EventHandler<EventArgs> OnRecordTag;

        public event EventHandler<EventArgs> OnAssignTag;
        public event EventHandler<EventArgs> OnReportTags;

   

        private void ReportTags()
        {
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
                        Antenna = Ant
                    };

                    if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                    {
                        onAssignTag(tag);
                        continue;

                    }

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

            Timer timer = new Timer();
            timer.Interval = delayMiliSeconds;
            timer.Tick += (s, e) =>
            {
                _aTimer.Enabled = true;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
