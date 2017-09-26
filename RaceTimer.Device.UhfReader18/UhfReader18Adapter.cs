using System;
using System.Text;
using RaceTimer.Common;
using RaceTimer.Data;
using NLog;
using System.Windows.Forms;

//using MessageBox = System.Windows.Forms.MessageBox;


namespace RaceTimer.Device.UhfReader18
{
    public class UhfReader18Adapter : DeviceAdapterBase, IDeviceAdapter
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


        ~UhfReader18Adapter()  // destructor
        {
            CloseConnection();
        }

        public void Setup(ReaderProfile readerProfile)
        {
            _readerProfile = readerProfile;
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
                    logger.Info("Opened port {0} to UhfReader18", openComIndex);
                    var result = StaticClassReaderB.SetPowerDbm(ref fComAdr, powerDbm, openComIndex);
                    if (result == 0)
                    {
                        //                SetWorkingMode();
                        return true;
                    }


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

        public bool BeginReading()
        {
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

        public bool StopReading()
        {
            _aTimer.Enabled = false;
            return true;
        }


        private void StartReadDelay()
        {
            if (_readerProfile.StartReadDelay == Data.StartReadDelay.None)
            {
                _aTimer.Enabled = true;
                logger.Info("Started reading from UhfReader18");
                return;
            }

            int delayMiliSeconds = (int)_readerProfile.StartReadDelay * 1000;

            Timer timer = new Timer();
            timer.Interval = delayMiliSeconds;
            timer.Tick += (s, e) =>
            {
                _aTimer.Enabled = true;
                timer.Stop();
                logger.Info("Started reading at {0} from UhfReader18 after a {1} delay", DateTime.Now, _readerProfile.StartReadDelay);
            };
            timer.Start();
            
        }


        private bool SetWorkingMode()
        {
            int Reader_bit0;
            int Reader_bit1;
            int Reader_bit2;
            int Reader_bit3;
            byte[] Parameter = new byte[6];
            Parameter[0] = Convert.ToByte(1);// "0=Answer Mode", "1=Active mode"
            if (true) //"EPCC1-G2
                Reader_bit0 = 0; // "EPCC1-G2
            else
                Reader_bit0 = 1;
            if (true)
                Reader_bit1 = 0; //Wiegand Output
            else
                Reader_bit1 = 1;
            if (true)  // buzzer
                Reader_bit2 = 0; // Activate buzzer
            else
                Reader_bit2 = 1;
            if (true)
                Reader_bit3 = 0;  //Word Addr
            else
                Reader_bit3 = 1;

            Parameter[1] = Convert.ToByte(Reader_bit0 * 1 + Reader_bit1 * 2 + Reader_bit2 * 4 + Reader_bit3 * 8);
            //storage area or inquiry condcuted Tags
            if (false)
                Parameter[2] = 0; //Password
            if (true)
                Parameter[2] = 1; //EPC
            if (false)
                Parameter[2] = 2; //TID
            if (false)
                Parameter[2] = 3; //user
            if (false)
                Parameter[2] = 4; //Multi-Tag
            if (false)
                Parameter[2] = 5; //Single-Tag
            //if (textBox3.Text == "")
            //{
            //    MessageBox.Show("Address is NULL!", "Information");
            //    return;
            //}
            Parameter[3] = Convert.ToByte("02", 16); //was textBox3.Text
            Parameter[4] = Convert.ToByte(1 + 1); //real word number
            Parameter[5] = Convert.ToByte(0); ; //single tag filtering itme 


            int resullt = StaticClassReaderB.SetWorkMode(ref fComAdr, Parameter, comPortIndex);
            if (resullt == 0)
            {
                return true;

            }

            return false;
        }

        private void ToggleBuzzer()
        {
            byte activeTime = Convert.ToByte(5);
            byte silentTime = Convert.ToByte(5);
            byte times = Convert.ToByte(2);
            int firmHandle = 2;
            //int result =
            //    StaticClassReaderB.BuzzerAndLEDControl(ref comAddress, activeTime, silentTime, times, firmHandle);
        }


        private void Inventory()
        {
            byte Qvalue = Convert.ToByte(0);
            byte Session = Convert.ToByte(0);
            // Convert.ToByte((int)_readerProfile.InventorySearchMode);

            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            byte[] EPC = new byte[5000];
            int CardIndex;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen, m;
            string s, sEPC;
            string temps;

            int fCmdRet = StaticClassReaderB.Inventory_G2(ref fComAdr, AdrTID, LenTID, TIDFlag, EPC, ref Totallen, ref CardNum, comPortIndex);

            if ((fCmdRet == 1) | (fCmdRet == 2) | (fCmdRet == 3) | (fCmdRet == 4) | (fCmdRet == 0xFB)) //end of read
            {
                byte[] daw = new byte[Totallen];
                Array.Copy(EPC, daw, Totallen);
                temps = ByteArrayToHexString(daw);
                //   fInventory_EPC_List = temps;            //存贮记录
                m = 0;
                if (CardNum == 0)
                {
                    // fIsInventoryScan = false;
                    return;
                }

               
                string lastEPC = "";
                for (CardIndex = 0; CardIndex < CardNum; CardIndex++)
                {

                    EPClen = daw[m];
                    sEPC = temps.Substring(m * 2 + 2, EPClen * 2);
                    m = m + EPClen + 1;
                    if (sEPC.Length != EPClen * 2)
                    {
                        return;
                    }
                    var readTime = DateTime.Now;

                    var tag = new Split
                    {
                        DateTimeOfDay = readTime,
                        TimeOfDay = readTime.ToString("hh.mm.ss.ff"),
                        Epc = sEPC,
                       // Rssi = Convert.ToInt32(RSSI, 16).ToString(),
                        SplitName = _readerProfile.Name,
                        SplitDeviceId = _readerProfile.Id,
                        InventorySearchMode = _readerProfile.InventorySearchMode
                    };

                    if (_readerProfile.ReadingMode == ReadingMode.Desktop)
                    {
                        onAssignTag(tag);
                    }
                    else
                    {
                        onRecordTag(tag);
                    }
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

        public event EventHandler<EventArgs> OnRecordTag;
        public event EventHandler<EventArgs> OnAssignTag;

        private void TimerTick(object sender, EventArgs e)
        {
            //if (fIsInventoryScan)
            //    return;


            Inventory();
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
