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

namespace folderintegrity
{
    public partial class AuthForm : Form
    {
        User u;
        public AuthForm(User us)
        {
            InitializeComponent();
            u = us;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                string authlogin = textBox1.Text;
                string password = textBox2.Text;
                if (u.Autentificate(authlogin, password))
                {
                    label3.Text = "Аутентификауия успешна";
                    //Thread.Sleep(1500);
                    Close();
                }
                else
                {
                    label3.Text = "Неверно введен логин или пароль";
                }


            }
            catch (ArgumentNullException)
            {
                label3.Text = "Заполните поля";
            }
        }
    }
}
