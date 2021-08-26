using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace folderintegrity
{
    public class User
    {
        public string pass { get; }
        public string login { get; }
        private bool auth = false;
        public User() { pass = ""; login = ""; }
        public User(string log, string pas) 
        {
            if (pass == "" || log == "") throw new ArgumentNullException();
            pass = pas;
            login = log;
        }
        public bool Autentificate(string l, string passnothashed) 
        {
            if (login == "") { auth = true; return true; }
            if (login == l)
            {
                if (EncH.Hash(passnothashed) == pass)
                {
                    auth = true;
                    return true;
                }
                else return false;
            }
            else return false;
        }
        public bool IsGuest() 
        {
            if (login == "" && pass == "") return true;
            else return false;
        }
        public bool UserAutentificationsucces() 
        {
            if (auth == true)
            {
                auth = false;
                return true;
            }
            else return false;
        }
    }
}
