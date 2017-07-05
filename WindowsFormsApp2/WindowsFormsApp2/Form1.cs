using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;



namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public string Between { set; get; }
        public Label Label2
        {
            set
            {
                label2 = value;
            }
            get
            {
                return label2;
            }
        }
        /*SynchronizationContext UIContxt;
        bool Conn = false;
        SerialPort ComPort;
        Thread RThread;
        Thread CThread;*/

        //public static SerialPort port;
        public static bool fil = false;
       // public static bool Data.transmission = false;
        public static string file_name;
        public static string path;
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
                for (int i = 0; i < 15; i++)
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
                    SendCOM(Convert.ToChar((vectors[i - 1] ) >> 8));
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
                    for (int i = 1; i < cnt; i++)
                    {
                        SendCOM(Convert.ToChar((vectors[254 * (c - 1) + i - 1] & 32512) >> 8));
                        SendCOM(Convert.ToChar(vectors[254 * (c - 1) + i - 1] & 255));
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

        static void frame_release(int[] vectors, int count, int order)
        {
            if (count > 0)//Если пришли данные
            {
                int i = 0;
                char[] vector = new char[254];
                byte[] byt = new byte[254];
                if (fil || !fil)
                {
                    while (i < (count) * 2)
                    {
                        if (vectors.Length <= i)
                        i = 0;
                        vectors[i] = vectors[i] << 8;
                        vectors[i + 1] = vectors[i + 1];
                        int var = vectors[i] + vectors[i + 1];
                        var = decode(var);
                        if (var == -1)
                        {
                            MessageBox.Show("Ошибка файла");
                            SpecialFrame(8);
                            SpecialFrame(4);
                            break;
                        }
                        vector[i / 2] = Convert.ToChar(var);
                        byt[i / 2] = Convert.ToByte(vector[i / 2]);

                        i += 2;
                    }
                    int q = 0;
                    if (!Data.transmission)
                    {
                        int var = 0;
                        List<char> vec2 = new List<char> { vector[var] };
                        var++;
                        while (to_int(vector[var]) > 0)
                        {
                            vec2.Add(vector[var]);
                            var++;
                        }
                        char[] vec = vec2.ToArray<char>();
                        file_name = new string(vec);
                        //Form3 f3 = new Form3(file_name);
                        DialogResult result=MessageBox.Show("Приянять файл "+file_name,"Warning",MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            SpecialFrame(3);
                            Data.transmission = true;
                        }
                        else
                        {
                            SpecialFrame(4);
                            Data.transmission = false;
                        }
                        // Data.transmission = true;
                    }
                    else
                    {
                        path = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        FileStream f = new FileStream(path+"\\Downloads\\" + file_name, FileMode.Append, FileAccess.Write);
                        f.Write(byt, 0, (i / 2));
                        
                        f.Close();
                    }
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
                        Data.transmission = true;
                        MessageBox.Show("Получено разрешение на передачу");
                        break;
                    case 4:
                        //cout << "Deny";
                        if (Data.transmission)
                        {
                            MessageBox.Show("Файл принят");
                        }
                        else
                        {
                            MessageBox.Show("Отказ");
                        }
                        Data.transmission = false;
                        fil = true;
                        
                        break;
                    case 8:
                        MessageBox.Show("Ошибка файла");
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
                // iSize= Data.port.Read(sReceivedChar, 0, 1);  // получаем 1 байт
                try
                {
                    iSize = Data.port.ReadByte();
                }
                catch (Exception)
                {
                    iSize = 0;
                }
                if (iSize > 0)
                {   // если что-то принято, выводим
                    int byt = iSize;
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
                        if (i < (length * 2))
                        {
                            data1[i] = byt;
                            i++;
                        }
                        else
                        {
                            data = false;
                            if (byt == 54)
                            {
                                frame_release(data1, length, counter);
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
                                frame_release(data1, length, counter);
                            }
                            else
                            {
                                int[] cadr = new int[1];
                                cadr[0] = type;
                                frame_release(cadr, 0, 0);
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
            while (true) { 
                try
                {
                    ReadCOM();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
        
        public Form1()
        {
            Thread thr = new Thread(func);
            thr.Start();
            InitializeComponent();
        }

        private void выбратьПортToolStripMenuItem_Click(object sender, EventArgs e)
        {

            new Form2().Show(this);
        }

        private void разъединитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Data.port.Close();
            label2.Text = "Соединение отсутсвует";
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Когда закончил отправлять файл, нужно будет еще раз вызвать функцию get()(Special_frame())
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //try
                //{
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            List<int> bytes = new List<int> { 0 };
                            int b = myStream.ReadByte();
                            int i = 0;
                            while (b != -1)
                            {
                                bytes[i] = b;
                                i += 1;
                                bytes.Add(0);
                                b = myStream.ReadByte();
                            }
                            int[] bytes2 = bytes.ToArray<int>();
                            char[] name = openFileDialog1.SafeFileName.ToCharArray();//По идее чистое название файла берет без пути
                            //Денис, тебе нужно вытянуть от сюда только имя, без полного пути
                            int[] name2 = new int[openFileDialog1.SafeFileName.Length];
                            for (int q = 0; q < openFileDialog1.SafeFileName.Length; q++)
                            {
                                name2[q] = to_int(name[q]);
                            }
                            get(name2, openFileDialog1.SafeFileName.Length);
                            while (!(Data.transmission) || (fil))
                            {
                                Thread.Sleep(100);
                            }
                            int var = 0;
                            int stop = 0;
                            double counter = 0;
                            double d = i / 255;
                            double Kost = Math.Ceiling(d);
                            while (counter <= Kost)
                            {
                                int[] tmp = new int[253];
                                for (int cnt = var; cnt < var + 253; cnt++)
                                {
                                    if (cnt < bytes2.Length)
                                    {
                                        tmp[cnt-var] = bytes2[cnt];
                                        stop = cnt;
                                    }
                                    else
                                    {
                                        stop = cnt-var-1;
                                        break;
                                    }
                                    
                                }
                                var+=254;
                                counter++;
                                get(tmp, stop);
                            }
                            SpecialFrame(4);
                            fil = false;

                        }
                    }
                //}
                //catch (Exception ex)
                //{
                  // MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                //}

            }
        }

            private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Разработать протоколы взаимодействия объектов до прикладного уровня локальной сети, состоящей из 2-х ПК, соединенных через интерфейс RS232C нульмодемным кабелем, и реализующей функцию передачи файлов. Скорость обмена и параметры COM-порта выбираются пользователями ПК. Имя передаваемого файла задается источником. При передаче файла защитить передаваемую информацию циклическим [15,11]-кодом.", "Условие задачи");
        }

        private void создателиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Студенты факультета РТ. \nГруппа РТ5-61. \nПетропавлов Д.М., Семенов А.А.,Коньшин К.И.", "Разработали");
        }

        public void label2_Click(object sender, EventArgs e)
        {
            label2.Text = Label2.Text;
        }
    }
}
