using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Port485Reader
{
    class Program
    {
        static SerialPort serialP = new SerialPort();
        static List<string> portsList = new List<string>();
        static string selectedPort = "";
        static int selectedSpeed = 9600;
        static bool flag = true;
        public static Stopwatch sw = new Stopwatch();
        public static long tim = 0;


        static void Main(string[] args)
        {
            GetPorts();
            Timer t = new Timer(tmrReceive_Tick, null, 0, 500);
            string tmp = "";
            while (true)
            {
                Thread.Sleep(500);
                tmp = Console.ReadLine();
                // tmp = "4D 33 33 37 32 32 0D";
                //tmp = "33722";
                sw.Restart();
                SendMsg(tmp);
                if (tmp == "stop")
                {
                    Console.WriteLine("Для получения данных нужно: " + tim + " миллисекунд");
                }

            }

            Console.ReadKey();
        }

        static private void GetPorts(/*object sender, EventArgs e*/)
        {
            try
            {
                string[] ports = SerialPort.GetPortNames(); //получаем список доступных СОМ-портов
                int i;

                for (i = 0; i < ports.Length; i++)
                    portsList.Add(ports[i]); //заполняем ими список

                if (ports.Length >= 1)
                {
                    selectedPort = ports[0];
                    //cmbPortNumber.Text = ports[0];
                    //cmbPortSpeed.Text = "19200";		//устанавливаем первый в списке СОМ-порт по умолчанию
                    //cmbPortParity.Text = "None";
                    //cmbSendPeriod.SelectedIndex = 0;
                }
                else
                {
                    //MessageBox.Show("COM port is not available!", "COM port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Console.WriteLine("COM port is not available!");
                    //Application.Exit();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private static void tmrReceive_Tick(/*object sender, EventArgs e*/ Object o)
        {
            if (flag == true)
            try
            {
                byte[] inp;
                int inpQty = 0;
                //ListViewItem lvi;
                string sReceiveDT = "";
                string message = "";

                // Если изменились настройки порта, перенастраиваем порт
                if (serialP.PortName != selectedPort)
                {
                    serialP.Close();
                    serialP.PortName = selectedPort;
                }
                if (serialP.BaudRate != selectedSpeed)
                {
                    serialP.Close();
                    serialP.BaudRate = selectedSpeed;
                }
                //if ((cmbPortParity.Text == "Odd") && (serialP.Parity != System.IO.Ports.Parity.Odd))
                //{
                //    serialP.Close();
                //    serialP.Parity = System.IO.Ports.Parity.Odd;
                //}
                //if ((cmbPortParity.Text == "Even") && (serialP.Parity != System.IO.Ports.Parity.Even))
                //{
                //    serialP.Close();
                //    serialP.Parity = System.IO.Ports.Parity.Even;
                //}
                //if ((cmbPortParity.Text == "None") && (serialP.Parity != System.IO.Ports.Parity.None))
                if (serialP.Parity != System.IO.Ports.Parity.None)
                {
                    serialP.Close();
                    serialP.Parity = System.IO.Ports.Parity.None;
                }


                // Если порт закрыт, открываем
                if (!serialP.IsOpen)
                    serialP.Open();

                if (serialP.IsOpen)
                {
                        //flag = false;
                    inp = new Byte[4096];
                    inpQty = 0;

                    if (serialP.BytesToRead > 0)	//если пришли данные
                    {
                        sReceiveDT = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "." + DateTime.Now.Millisecond.ToString("000");
                        inpQty = serialP.BytesToRead;				//определяем количество байт, которые пришли
                        serialP.Read(inp, 0, serialP.BytesToRead);	//считываем данные


                        // ПОКАЗ В СПИСКЕ
                        if (inpQty > 0)
                        {
                            message = "";

                            for (Int32 i = 0; i < inpQty; i++)
                                message += " " + ByteToStrHex(inp[i]);      //формируем сообщение для отображения в ListView

                                //lvi = new ListViewItem();
                                //lvi.ImageKey = "RECEIVE";
                                //lvi.Text = sReceiveDT;
                                //lvi.SubItems.Add(inpQty.ToString());
                                //lvi.SubItems.Add(message);
                                //listProcess.Items.Add(lvi);
                                //listProcess.Items[listProcess.Items.Count - 1].EnsureVisible();
                                // flag = false;
                                sw.Stop();
                                Console.WriteLine("HEX: " + message);
                                message = Encoding.ASCII.GetString(inp, 0, inpQty); // GatewayServer
                                Console.WriteLine("ASCII: " + message);
                                Console.WriteLine("Пришли за: " + sw.ElapsedMilliseconds);
                                if (tim < sw.ElapsedMilliseconds) tim = sw.ElapsedMilliseconds;
                            }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public static void SendMsg(string msg)
        {
            //string temp = "33722";
            string pre = "4D ", post = " 0D";
            byte[] ggg = Encoding.ASCII.GetBytes(msg);
            string gggg = "";
            for (Int32 i = 0; i < ggg.Length; i++)
                gggg += " " + ByteToStrHex(ggg[i]);
            gggg = pre + gggg + post;
            byte[] bmsg = StrHexToByte(gggg.Replace(" ", ""));
            // = Encoding.ASCII.GetBytes("M33722");
            int bl = bmsg.Length;
            serialP.Write(bmsg, 0, bl);
        }
        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        //функция преобразования Байта в 16-ричную строку
        private static string ByteToStrHex(byte b)
        {
            try
            {
                int iTmpH = b / (byte)16;
                int iTmpL = b % (byte)16;
                string ret = "";

                if (iTmpH < 10)
                    ret = iTmpH.ToString();
                else
                {
                    if (iTmpH == 10) ret = "A";
                    if (iTmpH == 11) ret = "B";
                    if (iTmpH == 12) ret = "C";
                    if (iTmpH == 13) ret = "D";
                    if (iTmpH == 14) ret = "E";
                    if (iTmpH == 15) ret = "F";
                }

                if (iTmpL < 10)
                    ret += iTmpL.ToString();
                else
                {
                    if (iTmpL == 10) ret += "A";
                    if (iTmpL == 11) ret += "B";
                    if (iTmpL == 12) ret += "C";
                    if (iTmpL == 13) ret += "D";
                    if (iTmpL == 14) ret += "E";
                    if (iTmpL == 15) ret += "F";
                }

                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static byte[] StrHexToByte(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}
