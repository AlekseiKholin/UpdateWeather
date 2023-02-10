using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Threading;

namespace UpdateWeather2._0
{
    public partial class Form1 : Form
    {
        static string id;  
        static string path; //default path C:/Users/Aleksei/Documents/Rainmeter/Skins/Harmattan_custom/Weather/Individual/Today/RSXX6200.xml
        static double temp;
        static int period;
        static bool exit;
        static bool connect;
        static int errorcount;


        public Form1()
        {
            InitializeComponent();

            // делаем невидимой нашу иконку в трее
            notifyIcon1.Visible = false;
            // добавляем Эвент или событие по 2му клику мышки, 
            //вызывая функцию  notifyIcon1_MouseDoubleClick
            this.notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);

            // добавляем событие на изменение окна
            this.Resize += new System.EventHandler(this.Form1_Resize);

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // проверяем наше окно, и если оно было свернуто, делаем событие        
            if (WindowState == FormWindowState.Minimized)
            {
                // прячем наше окно из панели
                this.ShowInTaskbar = false;
                // делаем нашу иконку в трее активной
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // делаем нашу иконку скрытой
            notifyIcon1.Visible = false;
            // возвращаем отображение окна в панели
            this.ShowInTaskbar = true;
            // Application
            this.FormBorderStyle = FormBorderStyle.Sizable; 
            //разворачиваем окно
            WindowState = FormWindowState.Normal;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Minimized;
            // прячем наше окно из панели
            this.ShowInTaskbar = false;
            // делаем нашу иконку в трее активной
            notifyIcon1.Visible = true;
            //в процессах
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            period = 61; // 120 sec 

            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;

            id = Properties.Settings.Default.ID;
            textBox1.Text = Properties.Settings.Default.ID;

            path = Properties.Settings.Default.PATH;

            connect = false;
            errorcount = 0;

            update_tmp();

            if (connect == true)
            {
                label3.Text = DateTime.Now.ToString() + " -> " + temp + "°C";
            }
            else
            {
                label3.Text = "Не удалось обновить данные";
            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = 100;
            progressBar1.Step = 1;
            progressBar1.Maximum = period * 10;
            progressBar1.PerformStep();

            if (progressBar1.Value == period * 10)
            {
                progressBar1.Value = 0;
                update_tmp();
                if (connect == true)
                {
                    label3.Text = DateTime.Now.ToString() + " -> " + temp + "°C";
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exit == true)
            {
                
                Application.Exit();
            }
            else
            {
                exit = false;

                this.WindowState = FormWindowState.Minimized;
                // прячем наше окно из панели
                this.ShowInTaskbar = false;
                // делаем нашу иконку в трее активной
                notifyIcon1.Visible = true;

                ShowBalloon();

                notifyIcon1.Text = temp.ToString() + "°C";

                //в процессах
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

                e.Cancel = true;
            }


        }

        private void ShowBalloon()
        {
            // задаем текст подсказки
            notifyIcon1.BalloonTipText = "Нажмите, чтобы отобразить окно";
            // устанавливаем зголовк
            notifyIcon1.BalloonTipTitle = "Подсказка";
            // отображаем подсказку 12 секунд
            notifyIcon1.ShowBalloonTip(5);
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }

        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            exit = true;

            notifyIcon1.Visible = false;
            Application.Exit();
            
        }

        static void update_tmp()
        {

                try
                {
                    // Запрос инф. на сервере Narodmon 

                    string Urlstart = "http://narodmon.ru/api/sensorsValues?sensors=";
                    string Urlend = "ваш ключ";
                    string Url = Urlstart + id + Urlend;

                    WebRequest req = WebRequest.Create(Url);
                    WebResponse resp = req.GetResponse();
                    Stream stream = resp.GetResponseStream();
                    StreamReader sr = new StreamReader(stream);
                    string Out = sr.ReadToEnd();
                    sr.Close();

                    // Обработка запроса 
                    string val = Out.Split(':')[4];
                    val = val.Substring(0, val.Length - 7);
                    val = val.Replace('.', ',');
                    double valnum = double.Parse(val);
                    temp = Math.Round(valnum, 0);
                    

                // Редактирование XML файла 
                    XDocument doc = XDocument.Load(path);
                    XElement weather = doc.Element("weather");
                    XElement cc = weather.Element("cc");
                    XElement tmp = cc.Element("tmp");
                    tmp.SetValue(temp);
                    doc.Save(path);

                    connect = true;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Ошибка записи файла");
                    MessageBox.Show(e.Message + "\r\n" + "Приложение будет остановлено");

                    errorcount = errorcount + 1;

                    if (errorcount == 1)
                    {
                        MessageBox.Show(e.Message);
                    }

                    connect = false;
                    exit = true;
                    Application.Exit();

                } 

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            id = textBox1.Text;
            Properties.Settings.Default.ID = textBox1.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Id датчика изменен на: " + id);
        }

    }
}
