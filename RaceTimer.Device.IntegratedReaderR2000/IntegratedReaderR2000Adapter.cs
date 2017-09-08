using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using RaceTimer.Common;
using RaceTimer.Data;
using ReaderB;
using MessageBox = System.Windows.Forms.MessageBox;


namespace RaceTimer.Device.IntegratedReaderR2000
{
    public class IntegratedReaderR2000Adapter : IDeviceAdapter
    {
        private int port, comPortIndex, openComIndex;
        private byte baudRate;
        private byte comAddress = 0xff;
        private bool comOpen;
        private int cmdReturn = 30;
        private bool isInventoryScan = false;
        private int openedPort;
        private byte fComAdr = 0xff;
        private TagRepository _tagRepo;
        private DateTime _raceTime;
        private ReaderProfile _readerProfile;


        private static System.Timers.Timer aTimer;

        public IntegratedReaderR2000Adapter()
        {
            //   _raceTime = raceTime;
            _tagRepo = new TagRepository();
        }

        ~IntegratedReaderR2000Adapter()  // destructor
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

                port = 0;
                int openResult;

                openResult = 30;
                //  Cursor = Cursors.WaitCursor;

                comAddress = Convert.ToByte("FF", 16);
                baudRate = Convert.ToByte(5);
                openResult = StaticClassReaderB.AutoOpenComPort(ref port, ref comAddress, baudRate, ref comPortIndex);//automatically detects a com port and connects it with the reader
                openComIndex = comPortIndex;

                if (openResult == 0)
                {
                    return true;
                }
                DialogResult result =
                    MessageBox.Show("Serial Communication Error or Occupied - result code: " + openResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DialogResult result = MessageBox.Show("Serial Communication Error or Occupied - exception message " + e.Message);
            }

            return false;
        }

        public bool CloseConnection()
        {
            try
            {
                StaticClassReaderB.CloseSpecComPort(comPortIndex);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               
            }
            return false;
        }

        
        public void BeginReading()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(100);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += TimerTick;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public void StopReading()
        {
            StaticClassReaderB.CloseSpecComPort(comPortIndex);
        }

        private void Inventory()
        {
            byte Qvalue = Convert.ToByte(4);
            byte Session = Convert.ToByte(2);
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

            int fCmdRet = StaticClassReaderB.Inventory_G2(ref fComAdr, Qvalue, Session, AdrTID, LenTID, TIDFlag, EPC,
                ref Totallen, ref CardNum, comPortIndex);

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
                    lastEPC = sEPC;
                    string RSSI = temps.Substring(m * 2 + 2 + EPClen * 2, 2);
                    m = m + EPClen + 2;
                    if (sEPC.Length != EPClen * 2)
                    {
                        return;
                    }

                    var tag = new Tag
                    {
                        Time = DateTime.Now,
                        TagId = sEPC,
                        Rssi = Convert.ToInt32(RSSI, 16).ToString(),
                        SplitName = _readerProfile.Name
                        
                    };
                    onRecordTag(tag);
                }


                // _tagRepo.Add
            }
        }

        private void onRecordTag(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnRecordTag;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> OnRecordTag;

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
