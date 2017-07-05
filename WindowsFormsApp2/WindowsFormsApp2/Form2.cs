using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WindowsFormsApp2
{
    public partial class Form2 : Form
    {
            //public static SerialPort port;
            static int to_int(char a)
            {
                int b = a;
                if (b < 0)
                {
                    b = 256 + b;
                }
                return b;
            }
            static int LenBin(int a)
            {
                if (a < 2)
                    return 1;
                else if (a < 4)
                    return 2;
                else if (a < 8)
                    return 3;
                else if (a < 16)
                    return 4;
                else if (a < 32)
                    return 5;
                else if (a < 64)
                    return 6;
                else if (a < 128)
                    return 7;
                else if (a < 256)
                    return 8;
                else if (a < 512)
                    return 9;
                else if (a < 1024)
                    return 10;
                else if (a < 2048)
                    return 11;
                else if (a < 4096)
                    return 12;
                else if (a < 8192)
                    return 13;
                else return 14;
            }
            static int division(int a, int b)
            {

                bool end = false;
                int ostatok = 0;
                int c;
                int sdvig;
                while (!end)
                {
                    if (LenBin(a) > LenBin(b))
                    {
                        c = b << (LenBin(a) - LenBin(b));
                        ostatok = a ^ c;
                        sdvig = LenBin(a) - LenBin(b);
                        a = ostatok;
                    }
                    else
                    {
                        ostatok = a ^ b;
                    }
                    if (LenBin(ostatok) < LenBin(b))
                    {
                        end = true;
                    }

                }
                return ostatok;
            }

            static int decode(int vector)
            {
                int polinom = 19;
                int[][] sindrom = new int[][]{
                new int[2]{ 1, 1 },
                new int[2]{ 2, 2 },
                new int[2]{ 4, 4 },
                new int[2]{ 8, 8 },
                new int[2]{ 3, 16 },
                new int[2]{ 6, 32 },
                new int[2]{ 12, 64 },
                new int[2]{ 11, 128 },
                new int[2]{ 5, 256 },
                new int[2]{ 10, 512 },
                new int[2]{ 7, 1024 },
                new int[2]{ 14, 2048 },
                new int[2]{ 15, 4096 },
                new int[2]{ 13, 8192 },
                new int[2]{ 9, 16384 },
            };
                int ostatok = division(vector, polinom);
                if (ostatok > 0)
                {
                    for (int i = 0; i < 14; i++)
                    {
                        if (sindrom[i][0] == ostatok)
                        {
                            //cout << endl << sindrom[i][1];
                            vector = vector ^ sindrom[i][1];
                            vector = vector >> 4;
                            return vector;
                        }
                    }
                    return -1;
                }
                else
                    return vector >> 4;
            }

            static int encode(int vector)
            {

                int polinom = 19;

                int vec1 = vector << 4;
                vec1 = vec1 ^ division(vec1, polinom);
                return vec1;
            }

            static void SendCOM(char b)
            {
            byte[] buf = new byte[1];
            buf[0] = Convert.ToByte(b);
            Data.port.Write(buf, 0, 1);
        }

            static void SpecialFrame(int a)
            {
                SendCOM(Convert.ToChar(54));
                SendCOM(Convert.ToChar(a));
                SendCOM(Convert.ToChar(54));
            }

            static void frame_prepare(int[] vectors, int count)
            {
                if (count < 254)
                {
                    SendCOM(Convert.ToChar(54));
                    SendCOM(Convert.ToChar(7));
                    SendCOM(Convert.ToChar(1));
                    SendCOM(Convert.ToChar(count));
                    for (int i = 1; i < count + 1; i++)
                    {
                        int var = i - 1;
                        SendCOM(Convert.ToChar((vectors[i - 1] & 32512) >> 8));
                        SendCOM(Convert.ToChar(vectors[i - 1] & 255));
                    }
                    SendCOM(Convert.ToChar(54));
                }
                else
                {
                    int c = 1;
                    double e = (double)count / 254;
                    while (c <= Math.Ceiling(e))
                    {
                        SendCOM(Convert.ToChar(54));
                        SendCOM(Convert.ToChar(7));
                        SendCOM(Convert.ToChar(c));
                        int cnt;
                        if (count - (c - 1) * 254 > 254)
                        {
                            cnt = 254;

                        }
                        else
                        {
                            cnt = count - 254 * (c - 1);
                        }
                        SendCOM(Convert.ToChar(cnt));
                        for (int i = 1; i < cnt + 1; i++)
                        {
                            SendCOM(Convert.ToChar((vectors[255 * (c - 1) + i - 1] & 32512) >> 8));
                            SendCOM(Convert.ToChar(vectors[255 * (c - 1) + i - 1] & 255));
                        }
                        SendCOM(Convert.ToChar(54));
                        c++;
                    }
                }
            }

            static void get(int[] bytes, int count)
            {

                int[] byt = new int[count];
                for (int i = 0; i < count; i++)
                {
                    byt[i] = encode(bytes[i]);
                }
                frame_prepare(byt, count);
            }

            static void frame_release(int[] vectors, int count,int order)
            {
                if (count > 0)//Если пришли данные
                {
                    int i = 0;
                    int[] vector = new int[255];
                    while (i < count * 2)
                    {
                        vector[i / 2] = decode((vectors[i] << 8) + vectors[i + 1]);
                        MessageBox.Show(Convert.ToChar(vector[i / 2]).ToString());
                    //Записывать в конец файла полученный байт.
                    //Если ордер =1, то пришел новый файл
                        i += 2;
                    }
                }
                else//Если специальный кадр
                {
                    switch (vectors[0])
                    {
                        case 0:
                            //Request;
                            break;
                        case 1:
                            //Success
                            break;
                        case 2:
                            //cout << "Break";
                            break;
                        case 3:
                            //cout << "Allow";
                            break;
                        case 4:
                            //cout << "Deny";
                            break;
                    }
                    //TO DO Денис, а здесь ты будешь обрабатывать различную хрень типо Запроса соединения и прочего
                }
            }

            static void ReadCOM()
            {
                int iSize;
                char[] sReceivedChar = new char[1];
                bool frame = false;
                bool data = false;
                int type = 0;
                int counter = 0;
                int length = 0;
                int data_count = 0;
                int[] data1 = new int[510];
                int i = 0;
                while (true)
                {
                    // iSize= port.Read(sReceivedChar, 0, 1);  // получаем 1 байт
                    try
                    {
                        iSize = Data.port.Read(sReceivedChar, 0, 1);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    if (iSize > 0)
                    {   // если что-то принято, выводим
                        int byt = to_int(sReceivedChar[0]);
                        if (frame & !data & byt != 54)
                        {
                            if (type == 0)
                            {
                                type = byt;
                                if (type != 7)
                                {
                                    counter = 1;
                                    length = 1;
                                }
                            }
                            else if (counter == 0)
                            {
                                counter = byt;
                            }
                            else if (length == 0)
                            {
                                length = byt;
                                data = true;
                            }
                        }
                        else if (data)
                        {
                            if (i < length * 2)
                            {
                                data1[i] = byt;
                                i++;
                            }
                            else
                            {
                                data = false;
                                if (byt == 54)
                                {
                                    frame_release(data1, length,counter);
                                    i = 0;
                                    type = 0;
                                    counter = 0;
                                    length = 0;
                                    data_count = 0;
                                    for (int c = 0; c < length * 2; c++)
                                    {
                                        data1[c] = 0;
                                    }
                                    frame = false;
                                }
                            }
                        }
                        else if (byt == 54)
                        {
                            if (!frame & !data)
                            {
                                frame = true;
                            }
                            else if (frame & !data)
                            {

                                if (type == 7)
                                {
                                    frame_release(data1, length,counter);
                                }
                                else
                                {
                                    int[] cadr = new int[1];
                                    cadr[0] = type;
                                    frame_release(cadr, 0,0);
                                }
                                i = 0;
                                type = 0;
                                counter = 0;
                                length = 0;
                                data_count = 0;
                                for (int c = 0; c < length * 2; c++)
                                {
                                    data1[c] = 0;
                                }
                                frame = false;
                            }
                        }

                    }
                }
            }

            static void func()
        {
            while (true)
            {
                ReadCOM();
            }
        }

            public Form2()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "COM1")
            {
                string[] ports = SerialPort.GetPortNames();//spisok portov

                Data.port = new SerialPort();//new port
                string n = "0";
                int num = int.Parse(n);
                try
                {
                    // настройки порта
                    Data.port.PortName = ports[num];
                    Data.port.BaudRate = 9600;
                    Data.port.Encoding = Encoding.Unicode;
                    Data.port.Open();
                    
                }
                catch (Exception f)
                {
                    MessageBox.Show("ERROR: невозможно открыть порт:" + f.ToString(),"1");
                    return;
                }
                SpecialFrame(0);
                MessageBox.Show("Подключено к COM1", "Информация");

                
            }
            if (comboBox1.Text == "COM2")
            {
                string[] ports = SerialPort.GetPortNames();//spisok portov

                Data.port = new SerialPort();//new port
                string n = "1";
                int num = int.Parse(n);
                try
                {
                    // настройки порта
                    Data.port.PortName = ports[num];
                    Data.port.BaudRate = 9600;
                    Data.port.Encoding = Encoding.Unicode;
                    Data.port.Open();
                }
                catch (Exception f)
                {
                    MessageBox.Show("ERROR: невозможно открыть порт:" + f.ToString(), "1");
                    return;
                }
                SpecialFrame(0);
                MessageBox.Show("Подключено к COM2", "Информация");
            

            }
            if (comboBox1.Text == "COM1")
            {
                Form1 F1 = (Form1)this.Owner;
                F1.Label2.Text = "Соединение активно";
            }

            if (comboBox1.Text == "COM2")
            {
                Form1 F1 = (Form1)this.Owner;
                F1.Label2.Text = "Соединение активно";
            }
            Close();
        }
    }
}
