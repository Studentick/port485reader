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
            // TopWindowSet.setTop();
            GetPorts();
            Timer t = new Timer(tmrReceive_Tick, null, 0, 500);
            string tmp = "";
            while (true)
            {
                Thread.Sleep(500);
                //tmp = Console.ReadLine();
                //sw.Restart();
                //SendMsg(tmp);
                //if (tmp == "stop")
                //{
                //    Console.WriteLine("Для получения данных нужно: " + tim + " миллисекунд");
                //}

            }

            Console.ReadKey();
        }

        static private void GetPorts()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames(); //получаем список доступных СОМ-портов
                int i;

                for (i = 0; i < ports.Length; i++)
                    portsList.Add(ports[i]); //заполняем ими список

                if (ports.Length >= 1)
                {
                    Console.WriteLine("Выберите порт:");

                    // выводим список портов
                    for (int counter = 0; counter < ports.Length; counter++)
                    {
                        Console.WriteLine("[" + counter.ToString() + "] " + ports[counter].ToString());
                    }
                    int selected_p = int.Parse(Console.ReadLine());
                    selectedPort = ports[selected_p];
                }
                else
                {
                    Console.WriteLine("COM port is not available!");
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private static void tmrReceive_Tick(Object o)
        {
            if (flag == true)
            try
            {
                byte[] inp;
                int inpQty = 0;
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
                            
                                sw.Stop();
                                message = Encoding.ASCII.GetString(inp, 0, inpQty); // GatewayServer
                                Console.WriteLine("ASCII: " + message);
                                string pre = "4D";
                                if (message == "44")
                                {
                                    // Console.WriteLine("zbs");
                                    // SendMsg("The message has been recived");
                                    SendMsg(message + "N0=+111=11111=11111=094");
                                }
                                else if(message == "56")
                                {
                                    // Console.WriteLine("zbs");
                                    // SendMsg("The message has been recived");
                                    SendMsg(message + "N0=+222=22222=22222=094");
                                }
                            }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public static void SendMsg(string msg)
        {
            //string pre = "4D ", post = " 0D";
            byte[] ggg = Encoding.ASCII.GetBytes(msg);
            string gggg = "";
            for (Int32 i = 0; i < ggg.Length; i++)
                gggg += " " + ByteToStrHex(ggg[i]);
            // gggg = pre + gggg + post;
            byte[] bmsg = StrHexToByte(gggg.Replace(" ", ""));
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
