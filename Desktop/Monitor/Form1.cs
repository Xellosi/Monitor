using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using Emgu.CV;
using Emgu.CV.Structure;

namespace monitor
{
    public partial class Form1 : Form
    {
        string DbName;
        string DbTableName;
        string VirtualArduino;
        VideoCapture Realcam;
        static readonly object _locker= new object();
        Rectangle rect;
        Bitmap Shot;
        //private FilterInfoCollection webcam;
        //private VideoCapture cap;
        UdpClient Listener;
        private Task UDPListen;
        private CancellationTokenSource ctsUDPListen;
        private Task UDPSend;
        private CancellationTokenSource ctsUDPSend;
        TcpListener TCPListener;
        private Task TCPListen;
        private CancellationTokenSource ctsTCPListen;
        //private Task TCPSend;
        SQLiteConnection Conn;
        /*---自動存檔路徑---*/
        string FileFolder;
        DataTable table = new DataTable();
        Action<string> UpdateUI;
        List<Socket> Sockets;
        float[] values;
        Boolean tcpstart;
        int camindex;
        public Form1()
        {
            InitializeComponent();
            DbName = "Database.db";
            VirtualArduino = "VirtualArduino";
            DbTableName = "arduinodata";
            camindex = -1;
            FileFolder = textBox1.Text;
            tcpstart = false;
            Shot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            rect = new Rectangle(0, 0, Shot.Width, Shot.Height);
            ctsUDPListen = new CancellationTokenSource();
            ctsUDPSend = new CancellationTokenSource();
            ctsTCPListen = new CancellationTokenSource();
            Sockets = new List<Socket>();
            //connectstr = "Server=.\\SQLEXPRESS;Database=arduino;Integrated Security=true";
            string[] serialPorts = SerialPort.GetPortNames();
            comboBox1.Items.Add(VirtualArduino);
            comboBox1.SelectedIndex = 0;
            foreach (string serialPort in serialPorts)
            {
                comboBox1.Items.Add(serialPort);
            }
            comboBox2.Items.Add("Desktop");
            comboBox2.SelectedIndex = 0;
            //webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            Realcam = new VideoCapture(0);
            if (Realcam.IsOpened)
            {
                comboBox2.Items.Add(Realcam.BackendName.Clone());
            }
            InitDB();
            Detecting_Events();
            UpdateUI = Update_UI;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            UDPSend = new Task(() => Program.SensorGenerate(ctsUDPSend.Token));
            UDPSend.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();
            ctsUDPSend.Cancel();
            ctsUDPListen.Cancel();
            if (UDPSend != null)
            {
                if (UDPSend.Status == TaskStatus.Running)
                {
                    //UDPSend.Dispose();
                }
            }
            if (Conn != null){
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
            if (UDPListen != null)
            {
                if (UDPListen.Status == TaskStatus.Running)
                {
                    //UDPListen.Join();
                }
            }
            if (Listener != null)
            {
                Listener.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == VirtualArduino)
            {
                if (Start_UDP())
                {
                    label14.Text = "連接";
                    label14.ForeColor = (Color.Green);
                    button1.Enabled = false;
                    button2.Enabled = true;
                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                }
            }
            //專題時步驟
            else
            {
                try
                {
                    serialPort1.PortName = (string)comboBox1.SelectedItem;
                    serialPort1.BaudRate = 9600;
                    Thread.Sleep(1000);
                    serialPort1.Open();
                    label14.Text = "連接";
                    label14.ForeColor = (Color.Green);
                    button1.Enabled = false;
                    button2.Enabled = true;
                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                }
                catch (Exception exc)
                {
                    MessageBox.Show(text: comboBox1.SelectedItem + "無法連線", caption: "error!");
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            ctsUDPListen.Cancel();
            Listener.Close();
            if (serialPort1.IsOpen)
                serialPort1.Close();
            label14.Text = "未連接";
            label14.ForeColor = (Color.Red);
            button1.Enabled = true;
            button2.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
        }
        /*private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender.GetType() == typeof(SerialPort))
            {
                //j = serialPort1.ReadLine();
                //splitAndDisplay();
            }
            else if (sender.GetType() == typeof(string))
            {
                if (tcpstart == true)
                    TCP_Send((string)sender);
            }
        }*/
        public void button3_Click(object sender, EventArgs e)
        {
            try
            {
                TCPListener = new TcpListener(Convert.ToInt32(textBox2.Text));
                TCPListener.Start();
                TCPListen = new Task(new Action(TCP_Listen));
                ctsTCPListen = new CancellationTokenSource();
                TCPListen.Start();
                tcpstart = true;
                label16.Text = "開啟";
                label16.ForeColor = Color.Green;
                button3.Enabled = false;
                button4.Enabled = true;
            }
            catch
            {
                MessageBox.Show("TCP Listner 開啟失敗,試試其它Port");
                return;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Socket s in Sockets)
            {
                if (s != null)
                {
                    s.Close();
                }
            }
            ctsTCPListen.Cancel();
            TCPListener.Stop();
            tcpstart = false;
            label16.Text = "關閉";
            label16.ForeColor = Color.Red;
            label19.Text = null;
            button3.Enabled = true;
            button4.Enabled = false;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            camindex = comboBox2.SelectedIndex;
            button5.Enabled = false;
            comboBox2.Enabled = false;
            button7.Enabled = true;
        }
        private void button7_Click(object sender, EventArgs e){
            pictureBox7.Image = null;
            camindex = -1;
            button5.Enabled = true;
            comboBox2.Enabled = true;
            button7.Enabled = false;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            if (path.SelectedPath != "") {
                this.textBox1.Text = path.SelectedPath;
                FileFolder = path.SelectedPath;
            }
        }
        private void SavePicture(string type_string)
        {
            var event_folder = Path.Combine(FileFolder, type_string);
            if (!Directory.Exists(event_folder))
                try { 
                Directory.CreateDirectory(event_folder);
                }
                catch
                {

                }
            if (Directory.Exists(event_folder))
            {

                //TODO try https://stackoverflow.com/questions/21497537/allow-an-image-to-be-accessed-by-several-threads
                var path = Path.Combine(event_folder, DateTime.Now.ToString("yyddmmss") + ".jpg");
                lock (_locker) {
                    //temporal scheme
                    try
                    {
                        using (Bitmap b = (Bitmap)Shot.Clone())
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawString(type_string, new Font("Arial", 100), Brushes.Red, new PointF(0, Screen.PrimaryScreen.Bounds.Height / 2));
                            b.Save(path, ImageFormat.Jpeg);
                        }
                    }
                    catch(InvalidOperationException e)
                    {
                        Console.WriteLine("AAA"+","+e.Source+","+e.Message+","+e.Data+","+e.StackTrace);
                    }
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            Inquiry frm = new Inquiry();
            DialogResult r = frm.ShowDialog();
            if (r == DialogResult.Yes)
            {
                string sqlcom = frm.query_string;
                DataTable table = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                SQLiteCommand cmd = new SQLiteCommand(sqlcom, Conn);
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
            frm.Dispose();
        }
        private bool Start_UDP(){
            try {
            ctsUDPListen = new CancellationTokenSource();
            UDPListen = new Task(() =>{
                //https://www.codeproject.com/Questions/486669/HowplustopluscloseplusUDPplusport-3f
                Listener = new UdpClient(Program.SimulatedPort);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Program.SimulatedPort);
                while (!ctsUDPListen.Token.IsCancellationRequested)
                {
                    try {
                        byte[] bytes = Listener.Receive(ref groupEP);
                        string s = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                        this.values = Array.ConvertAll(s.Split(','), float.Parse);
                        if (camindex >= 0)
                            Take_Shot();
                        this.Invoke(UpdateUI, s);
                        if (tcpstart == true)
                            TCP_Send(values);
                        SensorValues.Event_Triger(s);
                        Console.WriteLine("2222" + DateTime.Now.ToLongTimeString());
                    }
                    catch{
                        Console.WriteLine("UDP end");
                    }
                }});
            UDPListen.Start();
            }
            catch (Exception exc){
                    MessageBox.Show(exc.ToString(), "啟用虛擬傳輸失敗");
                return false;
            }
            return true;
        }
        private void TCP_Listen()
        {
            while (!ctsTCPListen.Token.IsCancellationRequested)
            {
                try {
                    var Temp_socket = TCPListener.AcceptSocket();
                    Sockets.Add(Temp_socket);
                    Console.WriteLine("Client端已連線");
                }
                catch
                {
                }
            }
        }
        private void TCP_Send(float[] values)
        {
            // SendS 在這裡為 string 型態, 為 Server 要傳給 Client 的字串
            var message = new StringBuilder();
            for (int i=0; i<SensorValues.thresholds.Length; i++)
            {
                message.Append(Convert.ToInt32(values[i]).ToString());
                if (i == 4)
                {
                    if (values[i] > SensorValues.thresholds[i])
                    {
                        message.Append(",0,");
                    }
                    else
                    {
                        message.Append(",1,");
                    }
                }
                else if (values[i] > SensorValues.thresholds[i])
                {
                    message.Append(",1,");
                }
                else
                {
                    message.Append(",0,");
                }
            }
            message.Remove(message.Length-1, 1);
            Byte[] returningByte = System.Text.Encoding.UTF8.GetBytes((message.ToString()+"\n").ToCharArray());
            this.Invoke((MethodInvoker)delegate { label19.Text ="";});
            for (int i = Sockets.Count -1; i>=0; i--)
            {
                if (Sockets[i].Connected == true)
                {
                    Sockets[i].Send(returningByte, returningByte.Length, 0);
                    Console.WriteLine(Sockets[i].LocalEndPoint.ToString() + "," + message.ToString());
                    this.Invoke((MethodInvoker)delegate { label19.Text += "\n" + Sockets[i].RemoteEndPoint.ToString(); });
                }
                else
                {
                    Sockets[i].Close();
                    Sockets.RemoveAt(i);
                }
            }
        }
        private void Update_UI(string str) {
            var s = str.Split(',');
            label1.Text = s[0];
            label2.Text = s[1];
            label3.Text = s[2];
            label4.Text = s[3];
            label5.Text = s[4];
            label6.Text = s[5];
            if (Convert.ToInt32(s[0]) > SensorValues.thresholds[0])
            {
                pictureBox1.Visible = true;
                label1.ForeColor = Color.Red;
            }
            else
            {
                pictureBox1.Visible = false;
                label1.ForeColor = Color.Black;
            }
            if (Convert.ToInt32(s[1]) > SensorValues.thresholds[1])
            {
                pictureBox2.Visible = true;
                label2.ForeColor = Color.Red;
            }
            else
            {
                pictureBox2.Visible = false;
                label2.ForeColor = Color.Black;
            }
            if (Convert.ToInt32(s[2]) > SensorValues.thresholds[2])
            {
                pictureBox3.Visible = true;
                label3.ForeColor = Color.Red;
            }
            else
            {
                pictureBox3.Visible = false;
                label3.ForeColor = Color.Black;
            }
            if (Convert.ToInt32(s[3]) > SensorValues.thresholds[3])
            {
                pictureBox4.Visible = true;
                label4.ForeColor = Color.Red;
            }
            else
            {
                pictureBox4.Visible = false;
                label4.ForeColor = Color.Black;
            }
            if (Convert.ToInt32(s[4]) < SensorValues.thresholds[4])
            {
                pictureBox5.Visible = true;
                label5.ForeColor = Color.Red;
            }
            else
            {
                pictureBox5.Visible = false;
                label5.ForeColor = Color.Black;
            }
            if (Convert.ToInt32(s[5]) > SensorValues.thresholds[5])
            {
                pictureBox6.Visible = true;
                label6.ForeColor = Color.Red;
            }
            else
            {
                pictureBox6.Visible = false;
                label6.ForeColor = Color.Black;
            }
            //video open
            if (camindex>= 0)
            {
                lock (_locker)
                {
                    pictureBox7.Image = Shot;
                }
            }
        }
        private void Take_Shot()
        {
            lock (_locker)
            {
                if (camindex == 0)
                {
                    using (var gfxScreenshot = Graphics.FromImage((Bitmap)Shot))
                    {
                        // Take the screenshot from the upper left corner to the right bottom corner.
                        gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                                Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                    }
                }
                else
                {
                    Realcam.QueryFrame();
                    Shot = Realcam.QueryFrame().ToImage<Bgr, Byte>().Bitmap;
                }
            }
        }
        private void Detecting_Events()
        {
            SensorValues.Heat_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("溫度");
                }
                Insert_Record("溫度", this.values[0],DateTime.Now);
            };
            SensorValues.Gas_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("瓦斯");
                }
                Insert_Record("瓦斯", this.values[1], DateTime.Now);
            };
            SensorValues.Fire_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("火焰");
                }
                Insert_Record("火焰", this.values[2], DateTime.Now);
            };
            SensorValues.Raining_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("下雨");
                }
                Insert_Record("下雨", this.values[3], DateTime.Now);
            };
            SensorValues.Opening_Door_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("門距");
                }
                Insert_Record("門距", this.values[4], DateTime.Now);
            };
            SensorValues.Body_Detect += (object _, EventArgs e) => {
                if (camindex >= 0)
                {
                    SavePicture("人體");
                }
                Insert_Record("人體", this.values[5], DateTime.Now);
            };
        }
        private void InitDB()
        {
            if (!File.Exists(Application.StartupPath + DbName))
            {
                SQLiteConnection.CreateFile(Application.StartupPath + DbName);
            }
            var constr = "Data Source = database.db; Version = 3; New = True; Compress = True;";
            Conn = new SQLiteConnection(constr);
            string CreateTable = "CREATE TABLE IF NOT EXISTS "+ DbTableName + "(ID INTEGER PRIMARY KEY, Event TEXT, Value REAL, Time DateTime);";
            SQLiteCommand command = new SQLiteCommand(CreateTable, Conn);
            try
            {
                //https://dotblogs.com.tw/jgame2012/2015/02/01/148338
                Conn.Open();
                command.ExecuteNonQuery();
            }
            catch (InvalidOperationException exc)
            {
            }
        }
        private void Insert_Record(string title, float value, DateTime time)
        {
            if (Conn.State == ConnectionState.Open)
            {
                String command = "INSERT INTO " + DbTableName + "(Event, Value, Time) Values(@event, @value, @datatime)";
                var sqlcommand = new SQLiteCommand(command, Conn);
                sqlcommand.Parameters.AddWithValue("@event", title);
                sqlcommand.Parameters.AddWithValue("@value", value);
                sqlcommand.Parameters.AddWithValue("@datatime", time);
                sqlcommand.ExecuteNonQuery();
            }
        }



























        private void pa1_Click(object sender, EventArgs e)
        {
            pa1.FlatAppearance.BorderSize = 2;
            pa2.FlatAppearance.BorderSize = 0;
            pa3.FlatAppearance.BorderSize = 0;
            pa4.FlatAppearance.BorderSize = 0;
            tabControl2.SelectTab(0);
        }
        private void pa2_Click(object sender, EventArgs e)
        {
            pa1.FlatAppearance.BorderSize = 0;
            pa2.FlatAppearance.BorderSize = 2;
            pa3.FlatAppearance.BorderSize = 0;
            pa4.FlatAppearance.BorderSize = 0;
            tabControl2.SelectTab(1);
        }
        private void pa3_Click(object sender, EventArgs e)
        {
            pa1.FlatAppearance.BorderSize = 0;
            pa2.FlatAppearance.BorderSize = 0;
            pa3.FlatAppearance.BorderSize = 2;
            pa4.FlatAppearance.BorderSize = 0;
            tabControl2.SelectTab(2);
        }
        private void pa4_Click(object sender, EventArgs e)
        {
            pa1.FlatAppearance.BorderSize = 0;
            pa2.FlatAppearance.BorderSize = 0;
            pa3.FlatAppearance.BorderSize = 0;
            pa4.FlatAppearance.BorderSize = 2;
            tabControl2.SelectTab(3);
        }
    }
}