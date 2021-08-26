using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NodaTime;

namespace folderintegrity
{
    public partial class SettingsForm : Form
    {
        Manager n;
        public SettingsForm(Manager m)
        {
            InitializeComponent();
            n = m;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> par = maskedTextBox1.Text.Split(':').ToList();
                List<int> time = new List<int> { Convert.ToInt32(par[0]), Convert.ToInt32(par[1]), Convert.ToInt32(par[2]) };
                LocalTime t = new LocalTime(time[0], time[1], time[2]);
                User u = new User(textBox1.Text, EncH.Hash(textBox2.Text));
                bool log = checkBox1.Checked;
                bool renew = checkBox2.Checked;
                n.Update(u, t, log, renew);
                pictureBox1.BackColor = Color.Aquamarine;
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Введено невозможное значение времени");
                pictureBox1.BackColor = Color.Coral;
            }
            catch (ArgumentNullException) 
            {
                MessageBox.Show("Необходимо заполнить поля");
                pictureBox1.BackColor = Color.Coral;
            }


        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = n.Uselogs;
            textBox1.Text = n.User.login;
            textBox2.Text = n.User.pass;
            string time;
            if (n.Roundcheck.Hour > 9) time = n.Roundcheck.Hour.ToString();
            else time = "0" + n.Roundcheck.Hour.ToString();
            if (n.Roundcheck.Minute > 9) time += n.Roundcheck.Minute.ToString();
            else time += "0" + n.Roundcheck.Minute.ToString();
            if (n.Roundcheck.Second > 9) time += n.Roundcheck.Second.ToString();
            else time += "0" + n.Roundcheck.Second.ToString();
            maskedTextBox1.Text = time;
        }
    }
}
