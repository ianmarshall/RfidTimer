using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RaceTimer.Common;
using RaceTimer.Data;
using UHF;
using Timer = System.Windows.Forms.Timer;

namespace RfidTimer.Device.ChaFonFourChannelR2000
{
    public class ChaFonFourChannelR2000Adapter : DeviceAdapterBase, IDeviceAdapter
    {
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
            //       SetSettings();
            //
            //   fCmdRet = RWDev.OpenNetPort(nPort, ipAddress, ref fComAdr, ref FrmPortIndex);

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
            //if (fIsInventoryScan)
            //    return;

            //GetData();
            flash_G2();

            //  Inventory();
        }

        public bool CloseConnection()
        {
            if (_readerProfile != null && _readerProfile.ConnectionType == ConnectionType.Serial)
            {

                try
                {
                    RWDev.CloseSpecComPort((int)_readerProfile.ComPort);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return false;
            }
            else
            {
                fCmdRet = RWDev.CloseNetPort(FrmPortIndex);
                return true;
            }

        }

        public bool OpenConnection()
        {
            if (_readerProfile.ConnectionType == ConnectionType.Serial)
            {
                return OpenSerial();

            }
            else
            {
                return OpenEthernet();
            }


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

        private void SetSettings()
        {
            byte[] Parameter = new byte[200];
            byte MaskMem = 0;
            byte[] MaskAdr = new byte[2];
            byte MaskLen = 0;
            byte[] MaskData = new byte[200];
            byte MaskFlag = 0;
            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            //if (MRB_G2.Checked)
            //{
            //    Parameter[0] = 0;
            //}
            //else
            //{
            //    Parameter[0] = 1;
            //}

            Parameter[0] = 1;

            Parameter[1] = (byte)0;//COM_MRPTime.SelectedIndex;
            Parameter[2] = (byte)0; //com_MFliterTime.SelectedIndex;
            Parameter[3] = (byte)4;//com_MQ.SelectedIndex;
            Parameter[4] = (byte)0;//com_MS.SelectedIndex;
            if (Parameter[4] > 3) Parameter[4] = 255;
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
            fCmdRet = RWDev.SetReadParameter(ref fComAdr, Parameter, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, frmcomportindex);
            if (fCmdRet != 0)
            {
                string strLog = "Set read parameter failed: " + GetReturnCodeDesc(fCmdRet);
                // WriteLog(lrtxtLog, strLog, 1);
            }
            else
            {
                string strLog = "Set read parameter success ";
                // WriteLog(lrtxtLog, strLog, 0);
            }
        }

        //private void Inventory()
        //{
        //    Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);
        //    byte Ant = 0;
        //    int TagNum = 0;
        //    int Totallen = 0;
        //    int EPClen, m;
        //    byte[] EPC = new byte[50000];
        //    int CardIndex;
        //    string temps, temp;
        //    temp = "";
        //    string sEPC;
        //    byte MaskMem = 0;
        //    byte[] MaskAdr = new byte[2];
        //    byte MaskLen = 0;
        //    byte[] MaskData = new byte[100];
        //    byte MaskFlag = 0;
        //    byte AdrTID = 0;
        //    byte LenTID = 0;
        //    AdrTID = 0;
        //    LenTID = 6;
        //    MaskFlag = 0;
        //    int cbtime = System.Environment.TickCount;
        //    CardNum = 0;
        //    fCmdRet = RWDev.Inventory_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, frmcomportindex);
        //    int cmdTime = System.Environment.TickCount - cbtime;//命令时间

        //    if (fCmdRet == 0x30)
        //    {
        //        CardNum = 0;
        //    }
        //    if (CardNum == 0)
        //    {
        //        if (Session > 1)
        //            AA_times = AA_times + 1;//没有得到标签只更新状态栏
        //    }
        //    else
        //        AA_times = 0;
        //    if ((fCmdRet == 1) || (fCmdRet == 2) || (fCmdRet == 0xFB) || (fCmdRet == 0x26))
        //    {
        //        if (cmdTime > CommunicationTime)
        //            cmdTime = cmdTime - CommunicationTime;//减去通讯时间等于标签的实际时间
        //        if (cmdTime > 0)
        //        {
        //            int tagrate = (CardNum * 1000) / cmdTime;//速度等于张数/时间
        //            IntPtr ptrWnd = IntPtr.Zero;
        //            //ptrWnd = FindWindow(null, "UHFReader288 Demo V2.0");
        //            //if (ptrWnd != IntPtr.Zero)         // 检查当前统计窗口是否打开
        //            //{
        //            //    string para = tagrate.ToString() + "," + total_tagnum.ToString() + "," + cmdTime.ToString();
        //            //    SendMessage(ptrWnd, WM_SENDTAGSTAT, IntPtr.Zero, para);
        //            //}
        //        }

        //    }
        //    IntPtr ptrWnd1 = IntPtr.Zero;
        //    //ptrWnd1 = FindWindow(null, "UHFReader288 Demo V2.0");
        //    //if (ptrWnd1 != IntPtr.Zero)         // 检查当前统计窗口是否打开
        //    //{
        //    //    string para = fCmdRet.ToString();
        //    //    SendMessage(ptrWnd1, WM_SENDSTATU, IntPtr.Zero, para);
        //    //}
        //    ptrWnd1 = IntPtr.Zero;
        //}


        private void GetData()
        {
            byte[] ScanModeData = new byte[40960];
            int nLen, NumLen;
            string temp1 = "";
            string syear = "";
            string smonth = "";
            string sday = "";
            string shour = "";
            string smin = "";
            string ssec = "";
            string Lyear = "";
            string Lmonth = "";
            string Lday = "";
            string Lhour = "";
            string Lmin = "";
            string Lsec = "";
            string binarystr1 = "";
            string binarystr2 = "";
            string CountStr = "";
            string AntStr = "";
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
                    while (fInventory_EPC_List.Length > 34)
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
                        binarystr1 = Convert.ToString(Convert.ToInt32(temp1.Substring(8, 8), 16), 2).PadLeft(32, '0');
                        syear = Convert.ToString(Convert.ToInt32(binarystr1.Substring(0, 6), 2)).PadLeft(2, '0');
                        smonth = Convert.ToString(Convert.ToInt32(binarystr1.Substring(6, 4), 2)).PadLeft(2, '0');
                        sday = Convert.ToString(Convert.ToInt32(binarystr1.Substring(10, 5), 2)).PadLeft(2, '0');
                        shour = Convert.ToString(Convert.ToInt32(binarystr1.Substring(15, 5), 2)).PadLeft(2, '0');
                        smin = Convert.ToString(Convert.ToInt32(binarystr1.Substring(20, 6), 2)).PadLeft(2, '0');
                        ssec = Convert.ToString(Convert.ToInt32(binarystr1.Substring(26, 6), 2)).PadLeft(2, '0');

                        binarystr2 = Convert.ToString(Convert.ToInt32(temp1.Substring(16, 8), 16), 2).PadLeft(32, '0');
                        Lyear = Convert.ToString(Convert.ToInt32(binarystr2.Substring(0, 6), 2)).PadLeft(2, '0');
                        Lmonth = Convert.ToString(Convert.ToInt32(binarystr2.Substring(6, 4), 2)).PadLeft(2, '0');
                        Lday = Convert.ToString(Convert.ToInt32(binarystr2.Substring(10, 5), 2)).PadLeft(2, '0');
                        Lhour = Convert.ToString(Convert.ToInt32(binarystr2.Substring(15, 5), 2)).PadLeft(2, '0');
                        Lmin = Convert.ToString(Convert.ToInt32(binarystr2.Substring(20, 6), 2)).PadLeft(2, '0');
                        Lsec = Convert.ToString(Convert.ToInt32(binarystr2.Substring(26, 6), 2)).PadLeft(2, '0');

                        CountStr = Convert.ToString(Convert.ToInt32(temp1.Substring(24, 4), 16), 10);
                        AntStr = Convert.ToString(Convert.ToInt32(temp1.Substring(28, 2), 16), 2).PadLeft(4, '0');
                        EPCStr = temp1.Substring(30, temp1.Length - 34);

                        var readTime = DateTime.Now;

                        var tag = new Split
                        {
                            DateTimeOfDay = readTime,
                            TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                            Epc = EPCStr,
                            //  Rssi = Convert.ToInt32(RSSI, 16).ToString(),
                            SplitName = _readerProfile.Name,
                            SplitDeviceId = _readerProfile.Id,
                            InventorySearchMode = _readerProfile.InventorySearchMode
                        };
                        onRecordTag(tag);

                        bool isonlistview = false;
                        //for (int i = 0; i < dataGridView2.RowCount; i++)
                        //{
                        //    if ((dataGridView2.Rows[i].Cells[1].Value != null) && (EPCStr == dataGridView2.Rows[i].Cells[1].Value.ToString()))
                        //    {
                        //        rows = dataGridView2.Rows[i];
                        //        rows.Cells[3].Value = "20" + Lyear + "-" + Lmonth + "-" + Lday + " " + Lhour + ":" + Lmin + ":" + Lsec; ;
                        //        rows.Cells[4].Value = AntStr;
                        //        rows.Cells[5].Value = CountStr;
                        //        isonlistview = true;
                        //        break;
                        //    }
                        //}
                        if (!isonlistview)
                        {
                            //string[] arr = new string[6];
                            //arr[0] = (dataGridView2.RowCount + 1).ToString();
                            //arr[1] = EPCStr;
                            //arr[2] = "20" + syear + "-" + smonth + "-" + sday + " " + shour + ":" + smin + ":" + ssec;
                            //arr[3] = "20" + Lyear + "-" + Lmonth + "-" + Lday + " " + Lhour + ":" + Lmin + ":" + Lsec;
                            //arr[4] = AntStr;
                            //arr[5] = CountStr;
                            //dataGridView2.Rows.Insert(dataGridView2.RowCount, arr);
                        }
                        total_tagnum = total_tagnum + 1;////每解析一条记录加一
                        //lxLed_toltag.Text = total_tagnum.ToString();
                        //lxLed_toltime.Text = (System.Environment.TickCount - total_time).ToString();
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ToString();
                }
            }
            xtime = System.Environment.TickCount - xtime;
            //lxLed_Num.Text = dataGridView2.RowCount.ToString();
            //if ((System.Environment.TickCount - total_time) > 0)
            //    lxLed_cmdsud.Text = (total_tagnum * 1000 / (System.Environment.TickCount - total_time)).ToString();
        }

        private void flash_G2()
        {
            Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);
            Qvalue = Convert.ToByte(4);
            byte Ant = 0;
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
                    string RSSI = Convert.ToInt32(temp.Substring(temp.Length - 2, 2), 16).ToString();
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
                        //  Rssi = Convert.ToInt32(RSSI, 16).ToString(),
                        SplitName = _readerProfile.Name,
                        SplitDeviceId = _readerProfile.Id,
                        InventorySearchMode = _readerProfile.InventorySearchMode
                    };

                    string epcDateTime;

                    if (RecentTags.ContainsKey(sEPC))
                    {
                        Split tag1 = RecentTags[sEPC];
                        //convert string to dt

                        //  ltrt = Convert.ToDateTime(epcDateTime);
                        //	  Console.WriteLine("Tag last seen at "+ epcDateTime);
                    }

                    else
                    {
                        RecentTags.TryAdd(sEPC, tag);
                    }


                    //  onRecordTag(tag);
                }
            }

            DateTime currentTime = DateTime.Now.ToLocalTime();

            foreach (KeyValuePair<string, Split> recentTag in RecentTags)
            {
                TimeSpan difference = currentTime - recentTag.Value.DateTimeOfDay;

                if (difference.TotalSeconds >= 3)
                {
                    Split removedTag;
                    RecentTags.TryRemove(recentTag.Key, out removedTag);

                    onRecordTag(removedTag);
                }
            }
            //if ((fCmdRet == 1) || (fCmdRet == 2) || (fCmdRet == 0xFB))
            //{
            //    if (cmdTime > CommunicationTime)
            //        cmdTime = cmdTime - CommunicationTime;//减去通讯时间等于标签的实际时间
            //    int tagrate = (CardNum * 1000) / cmdTime;//速度等于张数/时间
            //    total_tagnum = total_tagnum + CardNum;
            //    IntPtr ptrWnd = IntPtr.Zero;
            //    ptrWnd = FindWindow(null, "UHFReader288 Demo V1.16");
            //    if (ptrWnd != IntPtr.Zero)         // 检查当前统计窗口是否打开
            //    {
            //        string para = tagrate.ToString() + "," + total_tagnum.ToString() + "," + cmdTime.ToString();
            //        SendMessage(ptrWnd, WM_SENDTAGSTAT, IntPtr.Zero, para);
            //    }
            //}
            //else if (fCmdRet != 0x26)
            //{
            //    IntPtr ptrWnd1 = IntPtr.Zero;
            //    ptrWnd1 = FindWindow(null, "UHFReader288 Demo V1.16");
            //    if (ptrWnd1 != IntPtr.Zero)         // 检查当前统计窗口是否打开
            //    {
            //        string para = fCmdRet.ToString();
            //        SendMessage(ptrWnd1, WM_SENDSTATU, IntPtr.Zero, para);
            //    }
            //}
        }

        private void onRecordTag(EventArgs e)
        {
            OnRecordTag?.Invoke(this, e);
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

        //private void inventory()
        //{
        //    fIsInventoryScan = true;
        //    while (!toStopThread)
        //    {
        //        if (Session == 255)
        //        {
        //            FastFlag = 0;
        //            if (rb_mix.Checked)
        //            {
        //                flashmix_G2();
        //            }
        //            else
        //            {
        //                flash_G2();
        //            }

        //        }
        //        else
        //        {
        //            for (int m = 0; m < 4; m++)
        //            {
        //                switch (m)
        //                {
        //                    case 0:
        //                        InAnt = 0x80;
        //                        break;
        //                    case 1:
        //                        InAnt = 0x81;
        //                        break;
        //                    case 2:
        //                        InAnt = 0x82;
        //                        break;
        //                    case 3:
        //                        InAnt = 0x83;
        //                        break;
        //                }
        //                FastFlag = 1;
        //                if (antlist[m] == 1)
        //                {
        //                    if (Session > 1)//s2,s3
        //                    {
        //                        if ((check_num.Checked) && (AA_times + 1 > targettimes))
        //                        {
        //                            Target = Convert.ToByte(1 - Target);  //如果连续2次未读到卡片，A/B状态切换。
        //                        }
        //                    }
        //                    if (rb_mix.Checked)
        //                    {
        //                        flashmix_G2();
        //                    }
        //                    else
        //                    {
        //                        flash_G2();
        //                    }
        //                }
        //            }
        //        }
        //        Thread.Sleep(5);
        //    }
        //    this.Invoke((EventHandler)delegate
        //    {

        //        if (fIsInventoryScan)
        //        {
        //            toStopThread = true;//标志，接收数据线程判断stop为true，正常情况下会自动退出线程           

        //            btIventoryG2.Text = "Start";
        //            mythread.Abort();//若线程无法退出，强制结束
        //            timer_answer.Enabled = false;
        //            fIsInventoryScan = false;
        //        }
        //        timer_answer.Enabled = false;
        //        rb_mix.Enabled = true;
        //        rb_epc.Enabled = true;
        //        rb_tid.Enabled = true;
        //        rb_fastid.Enabled = true;
        //        fIsInventoryScan = false;
        //        btIventoryG2.Enabled = true;
        //    });

        //}

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
        private string GetErrorCodeDesc(int cmdRet)
        {
            switch (cmdRet)
            {
                case 0x00:
                    return "Other error";
                case 0x03:
                    return "Memory out or pc not support";
                case 0x04:
                    return "Memory Locked and unwritable";
                case 0x0b:
                    return "No Power,memory write operation cannot be executed";
                case 0x0f:
                    return "Not Special Error,tag not support special errorcode";
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
