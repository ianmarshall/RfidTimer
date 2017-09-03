using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceTimer.Data;
using ReaderB;

namespace RaceTimer.Device.IntegratedReaderR2000
{
   public class Adapter
    {
        private int port, comPortIndex, openComIndex;
        private byte baudRate;
        private byte comAddress = 0xff;
        private bool comOpen;
        private int cmdReturn = 30;
        private bool isInventoryScan = false;
        private int openedPort;
        private byte fComAdr = 0xff;
        private TagRepo _tagRepo;
   

        private static System.Timers.Timer aTimer;

        public Adapter()
        {
           _tagRepo = new TagRepo();
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               // throw;
            }

            return false;
        }

        public bool CloseConnection()
        {
            StaticClassReaderB.CloseSpecComPort(comPortIndex);
            return true;
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

        private void Inventory()
        {
            byte Qvalue = Convert.ToByte(4);
            byte Session = Convert.ToByte(0);
            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            byte[] EPC = new byte[5000];
            int CardNum = 0;
            int Totallen = 0;
            string temps;

            int fCmdRet = StaticClassReaderB.Inventory_G2(ref fComAdr, Qvalue, Session, AdrTID, LenTID, TIDFlag, EPC,
                ref Totallen, ref CardNum, comPortIndex);

            if ((fCmdRet == 1) | (fCmdRet == 2) | (fCmdRet == 3) | (fCmdRet == 4) | (fCmdRet == 0xFB)) //代表已查找结束
            {
                byte[] daw = new byte[Totallen];
                Array.Copy(EPC, daw, Totallen);
                temps = ByteArrayToHexString(daw);

                _tagRepo.Add(new Tag
                {
                    TagId = temps
                });

            }
        }

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
