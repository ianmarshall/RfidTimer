using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using RaceTimer.Common;
using RaceTimer.Data;
using ReaderB;


namespace RfidTimer.Device.CF_RU5102_USB_Desktop
{
    public class Cfru5102UsbDesktop : DeviceAdapterBase, IDeviceAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int port, comPortIndex, openComIndex;
        private byte baudRate;
        private byte comAddress = 0xff;
        private bool comOpen;
        private int cmdReturn = 30;
        private bool isInventoryScan = false;
        private int openedPort;
        private byte fComAdr = 0xff;
        private byte powerDbm;
        private DateTime _raceTime;
        private ReaderProfile _readerProfile;
        private static System.Timers.Timer _aTimer;

      
        private int fCmdRet = 30; //所有执行指令的返回值
        private int fOpenComIndex; //打开的串口索引号
        private bool fIsInventoryScan;
        private bool fisinventoryscan_6B;
        private byte[] fOperEPC = new byte[36];
        private byte[] fPassWord = new byte[4];
        private byte[] fOperID_6B = new byte[8];
        private int CardNum1 = 0;
        ArrayList list = new ArrayList();
        private bool fTimer_6B_ReadWrite;
        private string fInventory_EPC_List; //存贮询查列表（如果读取的数据没有变化，则不进行刷新）
        private int frmcomportindex;
        private bool ComOpen = false;


        public void Setup(ReaderProfile readerProfile)
        {
            _readerProfile = readerProfile;
        }

        public bool BeginReading()
        {
            // Create a timer with a two second interval.
            _aTimer = new System.Timers.Timer(200);
            // Hook up the Elapsed event for the timer. 
            _aTimer.Elapsed += TimerTick;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;

            return true;
        }

        private void TimerTick(object sender, EventArgs e)
        {

            if (fIsInventoryScan)
                return;
            Inventory();

            //  ReportTags();
        }

        public bool CloseConnection()
        {
            try
            {
                StaticClassReaderB.CloseSpecComPort(openComIndex);
                logger.Info("Closed port {0} to UhfReader18", openComIndex);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Close connection: {0}, {1}", ex.Message, ex.StackTrace);

            }
            return false;
        }

        public bool OpenConnection()
        {
            try
            {
                comAddress = Convert.ToByte("FF", 16);
                baudRate = Convert.ToByte(5);

                powerDbm = Convert.ToByte(_readerProfile.PowerDbm);

                port = 0;
                int openResult;

                openResult = 30;
                //  Cursor = Cursors.WaitCursor;

                comAddress = Convert.ToByte("FF", 16);
                baudRate = Convert.ToByte(5);
                if (_readerProfile.ComPort == ComPort.Auto)
                {
                    openResult =
                        StaticClassReaderB.AutoOpenComPort(ref port, ref comAddress, baudRate,
                            ref comPortIndex); //automatically detects a com port and connects it with the reader
                }
                else
                {
                    comPortIndex = (int)_readerProfile.ComPort;
                    openResult = StaticClassReaderB.OpenComPort(comPortIndex, ref comAddress, baudRate,
                        ref comPortIndex);
                }
                openComIndex = comPortIndex;

                if (openResult == 0)
                {
                    return true;

                }
                logger.Error("Serial Communication Error or Occupied - result code : {0}", openResult);
                MessageBox.Show("Serial Communication Error or Occupied - result code: " + openResult);
            }
            catch (Exception ex)
            {
                logger.Error("Open connection: {0}, {1}", ex.Message, ex.ToString());
                MessageBox.Show("Serial Communication Error or Occupied - exception message " + ex.Message);
            }

            return false;
        }


        public bool StopReading()
        {
            _aTimer.Enabled = false;
            return true;
        }

        public bool UpdateSettings()
        {
          //  throw new NotImplementedException();

            return true;
        }

        public event EventHandler<EventArgs> OnRecordTag;
        public event EventHandler<EventArgs> OnAssignTag;
        public event EventHandler<EventArgs> OnReportTags;


        private void Inventory()
        {
            int i;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen, m;
            byte[] EPC = new byte[5000];
            int CardIndex;
            string temps;
            string s, sEPC;
            bool isonlistview;
            fIsInventoryScan = true;
            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            //if (CheckBox_TID.Checked)
            //{
            //    AdrTID = Convert.ToByte(textBox4.Text, 16);
            //    LenTID = Convert.ToByte(textBox5.Text, 16);
            //    TIDFlag = 1;
            //}

            AdrTID = 0;
            LenTID = 0;
            TIDFlag = 0;

            fCmdRet = StaticClassReaderB.Inventory_G2(ref fComAdr, AdrTID, LenTID, TIDFlag, EPC, ref Totallen,
                ref CardNum, openComIndex);
            if ((fCmdRet == 1) | (fCmdRet == 2) | (fCmdRet == 3) | (fCmdRet == 4) | (fCmdRet == 0xFB)) //代表已查找结束，
            {
                byte[] daw = new byte[Totallen];
                Array.Copy(EPC, daw, Totallen);
                temps = ByteArrayToHexString(daw);
                fInventory_EPC_List = temps; //存贮记录
                m = 0;

                /*   while (ListView1_EPC.Items.Count < CardNum)
                  {
                      aListItem = ListView1_EPC.Items.Add((ListView1_EPC.Items.Count + 1).ToString());
                      aListItem.SubItems.Add("");
                      aListItem.SubItems.Add("");
                      aListItem.SubItems.Add("");
                 * 
                  }*/
                if (CardNum == 0)
                {
                    fIsInventoryScan = false;
                    return;
                }
                for (CardIndex = 0; CardIndex < CardNum; CardIndex++)
                {
                    EPClen = daw[m];
                    sEPC = temps.Substring(m * 2 + 2, EPClen * 2);
                    m = m + EPClen + 1;
                    if (sEPC.Length != EPClen * 2)
                        return;
                    var readTime = DateTime.Now;

                    var tag = new Split
                    {
                        DateTimeOfDay = readTime,
                        TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                        Epc = sEPC,
                     //   Rssi = RSSI,
                        SplitName = _readerProfile.Name,
                        SplitDeviceId = _readerProfile.Id,
                        InventorySearchMode = _readerProfile.InventorySearchMode,
                      //  Antenna = Ant
                    };

                    if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                    {
                        onAssignTag(tag);

                      
                       


                    }


                }

                fIsInventoryScan = false;
            }
        }

        private void onAssignTag(EventArgs e)
        {
            OnAssignTag?.Invoke(this, e);
        }

        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }

    }
}
