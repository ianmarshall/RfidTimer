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


            Inventory();
        }

        public bool CloseConnection()
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

        public bool OpenConnection()
        {
            try
            {
                int portNum = (int) _readerProfile.ComPort;
                int FrmPortIndex = 0;
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

        public bool StopReading()
        {
            _aTimer.Enabled = false;
            return true;
        }

        public event EventHandler<EventArgs> OnRecordTag;

        public event EventHandler<EventArgs> OnAssignTag;

        private void Inventory()
        {
            Session = Convert.ToByte((int)_readerProfile.InventorySearchMode);
            byte Ant = 0;
            int TagNum = 0;
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
            fCmdRet = RWDev.Inventory_G2(ref fComAdr, Qvalue, Session, MaskMem, MaskAdr, MaskLen, MaskData, MaskFlag, AdrTID, LenTID, TIDFlag, Target, InAnt, Scantime, FastFlag, EPC, ref Ant, ref Totallen, ref TagNum, frmcomportindex);
            int cmdTime = System.Environment.TickCount - cbtime;//命令时间
            
            if (fCmdRet == 0x30)
            {
                CardNum = 0;
            }
            if (CardNum == 0)
            {
                if (Session > 1)
                    AA_times = AA_times + 1;//没有得到标签只更新状态栏
            }
            else
                AA_times = 0;
            if ((fCmdRet == 1) || (fCmdRet == 2) || (fCmdRet == 0xFB) || (fCmdRet == 0x26))
            {
                if (cmdTime > CommunicationTime)
                    cmdTime = cmdTime - CommunicationTime;//减去通讯时间等于标签的实际时间
                if (cmdTime > 0)
                {
                    int tagrate = (CardNum * 1000) / cmdTime;//速度等于张数/时间
                    IntPtr ptrWnd = IntPtr.Zero;
                    //ptrWnd = FindWindow(null, "UHFReader288 Demo V2.0");
                    //if (ptrWnd != IntPtr.Zero)         // 检查当前统计窗口是否打开
                    //{
                    //    string para = tagrate.ToString() + "," + total_tagnum.ToString() + "," + cmdTime.ToString();
                    //    SendMessage(ptrWnd, WM_SENDTAGSTAT, IntPtr.Zero, para);
                    //}
                }

            }
            IntPtr ptrWnd1 = IntPtr.Zero;
            //ptrWnd1 = FindWindow(null, "UHFReader288 Demo V2.0");
            //if (ptrWnd1 != IntPtr.Zero)         // 检查当前统计窗口是否打开
            //{
            //    string para = fCmdRet.ToString();
            //    SendMessage(ptrWnd1, WM_SENDSTATU, IntPtr.Zero, para);
            //}
            ptrWnd1 = IntPtr.Zero;
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
