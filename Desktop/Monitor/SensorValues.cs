using System;
namespace monitor
{
    public class SensorValues
    {
        private static string _SensorString;
        public static string SensorString{ get { return _SensorString; } set { _SensorString = SensorString; } }
        private static string[] _words;
        //public static string[] words { get { return _Words; } set { _Words = Words; } }
        private SensorValues() { }

        public static event EventHandler Opening_Door_Detect;
        public static event EventHandler Raining_Detect;
        public static event EventHandler Heat_Detect;
        public static event EventHandler Gas_Detect;
        public static event EventHandler Fire_Detect;
        public static event EventHandler Body_Detect;

        public static void Event_Triger(string s) {
            _words = s.Split(',');
            if (Convert.ToInt32(_words[0]) > 50) {
                Heat_Detect(null,EventArgs.Empty);
            }
            if (Convert.ToInt32(_words[1]) > 100) {
                Gas_Detect(null, EventArgs.Empty);
            }
            if (Convert.ToInt32(_words[2]) > 100) {
                Fire_Detect(null, EventArgs.Empty);
            }
            if (Convert.ToInt32(_words[3]) == 0) {
                Raining_Detect(null, EventArgs.Empty);
            }
            if (Convert.ToInt32(_words[4]) < 15) {
                Opening_Door_Detect(null, EventArgs.Empty);
            }
            if (Convert.ToInt32(_words[5]) == 1) {
                Body_Detect(null, EventArgs.Empty);
            }
        }
        private void splitAndDisplay()
        {
            //SensorValues.words = SensorValues.SensorString.Split(',');
            /*
            this.Invoke(new Action(() =>
            {
                label1.Text = Global.words[0];
                label2.Text = Global.words[1];
                label3.Text = Global.words[2];
                label4.Text = Global.words[3];
                label5.Text = Global.words[4];
                label6.Text = Global.words[5];


                if (Convert.ToInt32(Global.words[0]) > 50)
                {
                    Global.state[0] = true;
                    pictureBox1.Visible = true;
                    label1.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + temperature_pic.Name + "','" + "溫度過高" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_tem();
                }
                else
                {
                    Global.state[0] = false;
                    pictureBox1.Visible = false;
                    label1.ForeColor = Color.Black;
                }
                if (Convert.ToInt32(Global.words[1]) > 100)
                {

                    Global.state[1] = true;
                    pictureBox2.Visible = true;
                    label2.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + gas_pic.Name + "','" + "瓦斯值異常" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_Gas();
                }
                else
                {
                    Global.state[1] = false;
                    pictureBox2.Visible = false;
                    label2.ForeColor = Color.Black;
                }
                if (Convert.ToInt32(Global.words[2]) > 100)
                {
                    Global.state[2] = true;
                    pictureBox3.Visible = true;
                    label3.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + fire_pic.Name + "','" + "火光反映" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_Fire();
                }
                else
                {
                    Global.state[2] = false;
                    pictureBox3.Visible = false;
                    label3.ForeColor = Color.Black;
                }
                if (Convert.ToInt32(Global.words[3]) ==0)
                {

                    Global.state[3] = true;
                    pictureBox4.Visible = true;
                    label4.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + rain_pic.Name + "','" + "有雨" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_Rain();
                }
                else
                {
                    Global.state[3] = false;
                    pictureBox4.Visible = false;
                    label4.ForeColor = Color.Black;
                }
                if (Convert.ToInt32(Global.words[4]) < 15)
                {
                    Global.warn[0] = true;
                    Global.state[4] = true;
                    pictureBox5.Visible = true;
                    label5.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + distance_pic.Name + "','" + "門開啟" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_Door();
                }
                else
                {
                    Global.warn[0] = false;
                    Global.state[4] =false;
                    pictureBox5.Visible = false;
                    label5.ForeColor = Color.Black;
                }

                if (Convert.ToInt32(Global.words[5]) == 1)
                {
                    Global.warn[1] = true;
                    Global.state[5] = true;
                    pictureBox6.Visible = true;
                    label6.ForeColor = Color.Red;
                    string insertstr = "INSERT INTO [arduinodata] (偵測項目,異常狀況,發生時間) VALUES ('" + body_pic.Name + "','" + "偵測到人體" + "','" + now + "')";
                    SqlConnection conn = new SqlConnection(connectstr);
                    SqlCommand cmd = new SqlCommand(insertstr, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    reload();
                    SavePicture_Human ();
                }
                else
                {
                    Global.warn[1] = false;
                    Global.state[5] = false;
                    pictureBox6.Visible = false;
                    label6.ForeColor = Color.Black;
                }
            }
            ));
            */
        }
    }
}
