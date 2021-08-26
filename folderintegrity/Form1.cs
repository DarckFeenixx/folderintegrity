using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
/// <summary>
/// добавить таймер, проверку при включении, корректное добавление файлов после инициализации
/// </summary>
namespace folderintegrity
{
    public partial class Form1 : Form
    {
        Manager mag;
        
        public Form1()
        {
            InitializeComponent();
        }
        void TransparetBackground(Control C)
        {
            C.Visible = false;

            C.Refresh();
            Application.DoEvents();

            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            int Right = screenRectangle.Left - this.Left;

            Bitmap bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
            Bitmap bmpImage = new Bitmap(bmp);
            bmp = bmpImage.Clone(new Rectangle(C.Location.X + Right, C.Location.Y + titleHeight, C.Width, C.Height), bmpImage.PixelFormat);
            C.BackgroundImage = bmp;

            C.Visible = true;
        }
        private void AuthShow() 
        {
            if (mag.User.IsGuest())
            {
                Show();
                WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
            else
            {
                AuthForm auth = new AuthForm(mag.User);
                auth.ShowDialog();
                if (mag.User.UserAutentificationsucces())
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                    notifyIcon1.Visible = false;
                }
            }
        }
        private void LoadManager() 
        {
            try
            {
                if (File.Exists(st.magfilename)) 
                {
                    mag = Manager.XMLimput(st.magfilename);
                    listBox1.Items.Clear();
                    for (int i = 0; i < mag.Protectedfiles.Count; i++) listBox1.Items.Add(mag.Protectedfiles[i]);
                }
                else mag = new Manager();
            }
            catch (ArgumentNullException)
            {
                mag = new Manager();
            }
        }
        private void PerformintegCheck() 
        {
            IntegrityLog il = mag.PerformintegCheck();
            if (il.errors == 0) button5.BackColor = Color.Beige;
            else 
            {
                button5.BackColor = Color.Coral;
                if (mag.renewhash)
                {
                    string temp = DateTime.Now.ToString();
                    string temp1 = "";
                    for (int i = 0; i < temp.Length; i++) 
                    {
                        if (temp[i] == ':' || temp[i] == '/') temp1 += ' ';
                        else temp1 += temp[i];
                    }
                    temp1 += st.log;
                    File.WriteAllLines(temp1, il.log); 
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadManager();
            label1.BackColor = Color.Transparent;
            TransparetBackground(label1);
            if (!mag.User.IsGuest()) MinimazeForm();
            mag.LoadHashTree(st.HTfile);
            if (mag.HT.Folders.Count > 0)
            {
                mag.InitTimer(call => PerformintegCheck());
                Task.Run(() => PerformintegCheck());
            }
        }
        private void OpenTSMI_Click(object sender, EventArgs e)
        {
            AuthShow();
        }
        private void CloseTSMI_Click(object sender, EventArgs e)
        {
            if (mag.User.IsGuest()) Close();
            else 
            {
                AuthForm auth = new AuthForm(mag.User);
                auth.ShowDialog();
                if (mag.User.UserAutentificationsucces()) Close();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBox1.Checked == true && notifyIcon1.Visible == false) 
            {
                e.Cancel = true;
                MinimazeForm();
                return; 
            }
            if (!mag.User.IsGuest())
            {
                AuthForm auth = new AuthForm(mag.User);
                auth.ShowDialog();
                if (!mag.User.UserAutentificationsucces()) { e.Cancel = true; return; }
            }
            mag.XMLoutput(st.magfilename);
            mag.HT.HTXMLoutput(st.HTfile);
        }

        private void НастройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form settings = new SettingsForm(mag);
            settings.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true) checkBox1.Checked = false;
            else checkBox1.Checked = true;
            TransparetBackground(label1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var folderselect = new CommonOpenFileDialog
            {
                Title = "Выберите папку"
            };
            if (textBox1.Text == "") folderselect.InitialDirectory = "C:\\";
            folderselect.IsFolderPicker = true;
            folderselect.ShowDialog();
            try
            {
                textBox1.Text = folderselect.FileName;
            }
            catch (Exception) { }
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            var fileselect = new CommonOpenFileDialog
            {
                Title = "Выберите файл"
            };
            if (textBox1.Text == "") fileselect.InitialDirectory = "C:\\";
            fileselect.ShowDialog();
            try
            {
                textBox1.Text = fileselect.FileName;
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (File.Exists(st.log)) Process.Start(st.loglast);
            button5.BackColor = Color.Transparent;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MinimazeForm();
        }
        private void MinimazeForm() 
        {
            WindowState = FormWindowState.Minimized;
            Hide();
            notifyIcon1.Visible = true;
        }
        private int FindCopy(ListBox lb, string s) 
        {
            for (int i = 0; i < lb.Items.Count; i++) if (s == lb.Items[i].ToString()) return i;
            return -1;
        }
        private int FindCopy(List<string> lb, string s)
        {
            for (int i = 0; i < lb.Count; i++) if (s == lb[i].ToString()) return i;
            return -1;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBox1.Text) || Directory.Exists(textBox1.Text))
            {
                if (FindCopy(listBox1, textBox1.Text) == -1)
                {
                    listBox1.Items.Add(textBox1.Text);
                    mag.Protectedfiles.Add(textBox1.Text);
                }
                else MessageBox.Show("Файл уже добавлен");
            }
            else 
            {
                MessageBox.Show("Файл не существует");
            }
            if (mag.HT.Folders.Count != 0) ReInitTree();
        }
        private void ReInitTree()
        {
            mag.StopTimer();
            Task.Run(() =>
            {
                mag.HT.InitHashTree(mag.Protectedfiles);
                mag.StartTimer();
            });
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                int rem = listBox1.SelectedIndex;
                mag.Protectedfiles.RemoveAt(FindCopy(mag.Protectedfiles, listBox1.SelectedItem.ToString()));
                listBox1.Items.RemoveAt(rem);
                if (mag.HT.Folders.Count != 0) ReInitTree();
            }
            catch (Exception) { }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (mag.Protectedfiles.Count == 0) MessageBox.Show("Для начала работы добавьте директории для проверки");
            { 
                if (mag.HT.Folders.Count == 0) Task.Run(() => mag.HT.InitHashTree(mag.Protectedfiles));
                Task.Run(() => PerformintegCheck());
                mag.InitTimer(call => PerformintegCheck());
            }
        }
    }
}
