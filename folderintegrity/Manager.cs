using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NodaTime;

namespace folderintegrity
{
    public static class st 
    {
        public static readonly string magfilename = "Manager.xml";
        public static readonly string log = "logs.txt";
        public static readonly string loglast = "lastlog.txt";
        public static readonly string HTfile = "HT.xml";
    }
    public class Manager
    {
        public User User { get; private set; }
        public LocalTime Roundcheck { get; private set; }
        public bool Uselogs { get; private set; }
        public List<string> Protectedfiles { get; }
        public bool renewhash = true;
        //     public bool encrypted { get; set; }
        public Timer timer;
        public HashTree HT { get; set; }
        public Manager()
        {
            User = new User();
            Roundcheck = new LocalTime(0, 5, 0);
            Uselogs = true;
            Protectedfiles = new List<string>();
            HT = new HashTree();
        }

        public void Update(User a, LocalTime time, bool l, bool n)
        {
            User = a;
            if (time.TickOfDay > 60 * TimeSpan.TicksPerSecond) Roundcheck = time;
            else Roundcheck = new LocalTime(0, 5, 0);
            try { timer.Change(0, time.TickOfDay / TimeSpan.TicksPerMillisecond); } catch (Exception) { }
            Uselogs = l;
            renewhash = n;
        }
        public Manager(User a, LocalTime time, bool l, List<string> fold, bool n = true)
        {
            User = a;
            if (time.TickOfDay > 60 * TimeSpan.TicksPerSecond) Roundcheck = time;
            else Roundcheck = new LocalTime(0, 5, 0);
            Uselogs = l;
            Protectedfiles = fold;
            HT = new HashTree();
            renewhash = n;
        }
        public void XMLoutput(string file)
        {
            XDocument doc = new XDocument();
            XElement man = new XElement("Manager");
            XElement us = new XElement("user");
            XElement ulog = new XElement("login", User.login);
            XElement uspass = new XElement("password", User.pass);
            us.Add(ulog);
            us.Add(uspass);
            man.Add(us);
            XElement logs = new XElement("uselogs", Uselogs.ToString());
            man.Add(logs);
            XElement timer = new XElement("timer", (Roundcheck.Hour.ToString() + ":" + Roundcheck.Minute + ":" + Roundcheck.Second));
            XElement fold = new XElement("folders");
            foreach (string s in Protectedfiles)
            {
                XElement f = new XElement("folder", s);
                fold.Add(f);
            }
            XElement renew = new XElement("renewhash", renewhash);
            man.Add(renew);
            man.Add(timer);
            man.Add(fold);
            doc.Add(man);
            doc.Save(file);
        }
        public static Manager XMLimput(string file)
        {
            bool logs = true;
            LocalTime t = new LocalTime();
            bool re = true;
            User u = new User();
            List<string> fold = new List<string>();
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            foreach (XmlNode node in root)
            {

                if (node.Name == "user")
                {
                    string l = "";
                    string p = "";
                    foreach (XmlNode childnode in node.ChildNodes)
                    {
                        if (childnode.Name == "login") l = childnode.InnerText;
                        if (childnode.Name == "password") p = childnode.InnerText;
                    }
                    if (p != "" && l != "") u = new User(l, p);
                    else if (!(p == "" && l == "")) throw new ArgumentNullException();
                }
                if (node.Name == "uselogs") logs = Convert.ToBoolean(node.InnerText);
                if (node.Name == "renewhash") re = Convert.ToBoolean(node.InnerText);
                if (node.Name == "timer")
                {
                    string s = node.InnerText;
                    List<string> par = s.Split(':').ToList();
                    List<int> time = new List<int> { Convert.ToInt32(par[0]), Convert.ToInt32(par[1]), Convert.ToInt32(par[2]) };
                    t = new LocalTime(time[0], time[1], time[2]);
                }
                if (node.Name == "folders") foreach (XmlNode chi in node.ChildNodes) fold.Add(chi.InnerText);
            }
            return new Manager(u, t, logs, fold, re);
        }
        public void LoadHashTree(string file)
        {
            HT = HashTree.HTXMLimput(file);
        }
        public void StopTimer()
        {
            timer.Change(-1, -1);
        }
        public void StartTimer()
        {
            timer.Change(Roundcheck.TickOfDay / TimeSpan.TicksPerMillisecond, Roundcheck.TickOfDay/TimeSpan.TicksPerMillisecond);
        }
        public void InitTimer(TimerCallback callback) 
        {
            timer = new Timer(callback, null, Roundcheck.TickOfDay / TimeSpan.TicksPerMillisecond, Roundcheck.TickOfDay / TimeSpan.TicksPerMillisecond);
        }
        public IntegrityLog PerformintegCheck()
        {
            HashTree newht = new HashTree();
            IntegrityLog il = new IntegrityLog();
            StopTimer();
            newht = new HashTree(Protectedfiles);
            il = HT.PerformIntegrityCheck(newht);
            File.WriteAllLines(st.loglast, il.log);
            File.AppendAllLines(st.log, il.log);
            if (renewhash)
            {
                HT.HTXMLoutput(st.HTfile);
                HT = newht;
            }
            StartTimer();
            return il;
        }
    }
}
