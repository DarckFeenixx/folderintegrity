using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace folderintegrity
{
    public class IntegrityLog 
    {
        public List<string> log;
        public int errors;
        public IntegrityLog() 
        {
            log = new List<string>();
            errors = 0;
        }
        public void Addlog(IntegrityLog il) 
        {
            errors += il.errors;
            for (int i = 0; i < il.log.Count; i++) 
            {
                log.Add(il.log[i]);
            }
        }
        public void Addlog(string il)
        {
            log.Add(il);
        }
    }
    public class HashTreeNode
    {
        public string fileName = "";
        public string fileHash = "";
        public HashTreeNode() { }
        public HashTreeNode(string name, string hash = "")
        {
            fileName = name;
            fileHash = hash;
        }
    }
    public class FolderNode
    {
        public List<HashTreeNode> Items;
        public string parentfolder = "";
        public string folderhashsumm = "";
        public FolderNode()
        {
            Items = new List<HashTreeNode>();
        }
        public FolderNode(HashTreeNode hs, string fold)
        {
            Items = new List<HashTreeNode>();
            Items.Add(hs);
            parentfolder = fold;
        }
        public FolderNode(string fold, List<HashTreeNode> hs)
        {
            Items = hs;
            parentfolder = fold;
        }
    }
    public class HashTree
    {
        public List<FolderNode> Folders;
        public HashTree()
        {
            Folders = new List<FolderNode>();
        }
        public HashTree(List<string> ls) 
        {
            Folders = new List<FolderNode>();
            InitHashTree(ls);
        }
        private void CleanUpTree() 
        {
            for (int i = 0; i < Folders.Count; i++) 
            {
                if (Folders[i].Items.Count == 1) 
                {
                    if (Folders[i].Items[0].fileName == Folders[i].parentfolder) continue;
                } 
                for (int j = 0; j < Folders[i].Items.Count; j++) 
                {
                    for (int iii = 0; iii < Folders.Count; iii++)
                    {
                        for (int jj = 0; jj < Folders[iii].Items.Count; jj++)
                        {
                            if (Folders[i].Items[j].fileName == Folders[iii].Items[jj].fileName && i != iii) 
                            {
                                Folders[iii].Items.RemoveAt(jj);
                                if (Folders[iii].Items.Count == 0) 
                                {
                                    Folders.RemoveAt(iii);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void InitHash()
        {
            foreach (FolderNode fn in Folders)
            {
                if (fn.Items.Count == 1)
                {
                    if (fn.Items[0].fileName == fn.parentfolder)
                    {
                        fn.parentfolder = EncH.Hash(fn.parentfolder);
                        byte[] byt = File.ReadAllBytes(fn.Items[0].fileName);
                        fn.Items[0].fileHash = EncH.Hash(byt);
                        continue;
                    }
                }
                string temp = "";
                List<string> newdirs = Directory.GetDirectories(fn.parentfolder).ToList();
                List<string> newfiles = Directory.GetFiles(fn.parentfolder).ToList();
                for (int i = 0; i < newdirs.Count; i++) temp += newdirs[i];
                for (int i = 0; i < newfiles.Count; i++) temp += newfiles[i];
                fn.folderhashsumm = EncH.Hash(temp);
                foreach (HashTreeNode htn in fn.Items) 
                {
                    byte[] byt = File.ReadAllBytes(htn.fileName);
                    htn.fileHash = EncH.Hash(byt);
                }
            }
        }
        private void InitFolder(string fold)
        {
            List<string> newdirs = Directory.GetDirectories(fold).ToList();
            List<string> newfiles = Directory.GetFiles(fold).ToList();
            List<HashTreeNode> temp = new List<HashTreeNode>();
            foreach (string s in newfiles) temp.Add(new HashTreeNode(s));
            Folders.Add(new FolderNode(fold, temp));
            if (newdirs.Count != 0) foreach (string s in newdirs) InitFolder(s);
        }
        public void InitHashTree(List<string> dir)
        {
            foreach (string path in dir)
            {
                if (File.Exists(path)) Folders.Add(new FolderNode(new HashTreeNode(path), path));
                else if (Directory.Exists(path))
                {
                    InitFolder(path);
                }
                else throw new ArgumentException("файл не существует");
            }
            InitHash();
            CleanUpTree();
        }
        private static string ParseFileName(string filename) 
        {
            var temp = filename.Split('\\').ToList();
            return temp[temp.Count - 1];
        }
        public static HashTree HTXMLimput(string filename) 
        {
            var temp = new HashTree();
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(HashTree));
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    temp = (HashTree)formatter.Deserialize(fs);
                }
            }
            catch (Exception) { }
            return temp;
        }
        public void HTXMLoutput(string filename) 
        {
            if (Folders.Count != 0)
            {
                XmlSerializer formatter = new XmlSerializer(typeof(HashTree));
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, this);
                }
            }
        }
        private IntegrityLog Checkfilename(HashTreeNode htn, HashTree ht , int folderindex, out bool needextendedcheck, out bool namefound)
        {
            int i;
            needextendedcheck = false;
            namefound = false;
            IntegrityLog il = new IntegrityLog();
            string name = ParseFileName(htn.fileName);
            for (i = 0; i < ht.Folders[folderindex].Items.Count; i++)
            {
                if (name == ParseFileName(ht.Folders[folderindex].Items[i].fileName))
                {
                    namefound = true;
                    break;
                }
            }
            if (i < ht.Folders[folderindex].Items.Count)
            {
                if (htn.fileHash == ht.Folders[folderindex].Items[i].fileHash)
                {
                    il.log.Add($"{DateTime.Now} => проверен файл {htn.fileName} изменений не обнаружено");
                    return il;
                }
                else
                {
                    il.errors++;
                    needextendedcheck = true;
                    il.log.Add($"{DateTime.Now} => проверен файл {htn.fileName} содержимое было изменено или перенесено в другой файл");
                    return il;
                }
            }
            else 
            {
                il.errors++;
                il.log.Add("имя не найдено");
                return il; 
            }
        }
        private IntegrityLog CheckfileHash(HashTreeNode htn, HashTree ht, int folderindex, out bool needextendedcheck, out bool hashcorrect) 
        {
            int i;
            hashcorrect = false;
            needextendedcheck = false;
            IntegrityLog il = new IntegrityLog();
            il.errors++;
            for (i = 0; i < ht.Folders[folderindex].Items.Count; i++)
            {
                if (htn.fileHash == ht.Folders[folderindex].Items[i].fileHash)
                {
                    hashcorrect = true;
                    break;
                }
            }
            if (hashcorrect)
            {
                il.log.Add($"{DateTime.Now} => проверен файл {htn.fileName} было изменено название файла на {ht.Folders[folderindex].Items[i].fileName}");
                return il;
            }
            else
            {
                needextendedcheck = true;
                il.log.Add($"{DateTime.Now} => проверен файл {htn.fileName} файл был удален, перемещен или изменен вместе с названием");
                return il;
            }
        }
        private IntegrityLog CheckFile(HashTreeNode htn, HashTree ht, int folderindex, out bool needextendedcheck) 
        {
            needextendedcheck = false;
            IntegrityLog temp = Checkfilename(htn, ht, folderindex, out needextendedcheck, out bool namefound);
            if(namefound) 
            {
                return temp;
            }
            else
            {
                return CheckfileHash(htn, ht, folderindex, out needextendedcheck, out bool hashcorrect);
            }
        }
        private List<string> ExtendedCheckFile(HashTreeNode htn, HashTree ht) 
        {
            bool here = false;
            List<string> log = new List<string>();
            for (int i = 0; i < ht.Folders.Count; i++) 
            {
                IntegrityLog temp = Checkfilename(htn, ht, i, out here, out bool namefound);
                IntegrityLog temp1 = CheckfileHash(htn, ht, i, out here, out bool hashcorrect);
                if (namefound && !here)
                {
                    if (hashcorrect && !here)
                    {
                        log.Add(temp.log[0]);
                    }
                    else
                    {
                        log.Add($"{DateTime.Now} => проверен файл {htn.fileName} файл был перемещен и его название изменено");
                    }
                }
                else 
                {
                    if (hashcorrect && !here) 
                    {
                        log.Add(temp1.log[0] + ", файл перемещен в другую папку");
                    }
                }
            }
            if (log.Count == 0) log.Add($"{DateTime.Now}: дополнительная проверка файла {htn.fileName} не дала результатов");
            return log;
        }
        private List<string> ExtrafolderCheck(FolderNode fn) 
        {
            var res = new List<string>();
            double totalintscore = 0;
            bool[] isnew = new bool[fn.Items.Count];
            foreach (FolderNode old in Folders) 
            {
                int filecount = old.Items.Count;
                double intgscore;
                intgscore = 0;
                for (int newit = 0; newit < fn.Items.Count; newit++)
                {
                    for (int j = 0; j < old.Items.Count; j++)
                    {
                        if (ParseFileName(fn.Items[newit].fileName) == ParseFileName(old.Items[j].fileName))
                        {
                            isnew[newit] = true;
                            if (old.Items[j].fileHash == fn.Items[newit].fileHash)
                            {
                                intgscore += 1;
                                totalintscore += 1;
                                res.Add($"{DateTime.Now}: файл {old.Items[j].fileName} из папки {old.parentfolder} обнаружен в папке {fn.parentfolder}");
                            }
                            else
                            {
                                intgscore += 0.5;
                                totalintscore += 0.5;
                                res.Add($"{DateTime.Now}: файл c названием {old.Items[j].fileName} из папки {old.parentfolder} обнаружен в папке {fn.parentfolder}, содержимое не совпадает");
                            }
                        }
                        else if(old.Items[j].fileHash == fn.Items[newit].fileHash)
                        {
                            isnew[newit] = true;
                            intgscore += 0.7;
                            totalintscore += 0.7;
                            res.Add($"{DateTime.Now}: файл {old.Items[j].fileName} из папки {old.parentfolder} обнаружен в папке {fn.parentfolder}, имя изменено");
                        }
                    }
                }
                if (intgscore == filecount)
                {
                    res.Add($"{DateTime.Now}: содержимое папки {old.parentfolder} перенесено в папку {fn.parentfolder}");
                }
                if (filecount / intgscore - 0.6 <= 0.05)
                {
                    if(old.parentfolder != fn.parentfolder) res.Add($"{DateTime.Now}: содержимое папки {old.parentfolder} перенесено в папку {fn.parentfolder}, в папке появились новые файлы");
                }
                else if (filecount / intgscore - 0.3 <= 0.05)
                {
                    if (old.parentfolder != fn.parentfolder) res.Add($"{DateTime.Now}: содержимое папки {old.parentfolder} частично перенесено в папку {fn.parentfolder}");
                }
            }
            if (totalintscore == 0) res.Add($"{DateTime.Now}: была найдена новая созданная папка {fn.parentfolder}");
            for (int i = 0; i < isnew.Length; i++) if (!isnew[i]) res.Add($"{DateTime.Now}: найден новый файл {fn.parentfolder}");
            return res;
        }
        public IntegrityLog PerformIntegrityCheck(HashTree currenthashtree) 
        {
            IntegrityLog il = new IntegrityLog();
            List<FolderNode> needfushercheck = new List<FolderNode>();
            var newfolder = new bool[currenthashtree.Folders.Count];
            for (int i = 0; i < newfolder.Length; i++) newfolder[i] = true;
            for (int j = 0; j < Folders.Count; j++)
            {
                bool folderchecksumcorrect = false;
                int i;
                for (i = 0; i < currenthashtree.Folders.Count; i++)
                {
                    if (currenthashtree.Folders[i].folderhashsumm == Folders[j].folderhashsumm)
                    {
                        folderchecksumcorrect = true;
                        newfolder[i] = false;
                        break;
                    }
                    if (currenthashtree.Folders[i].parentfolder == Folders[j].parentfolder)
                    {
                        break;
                    }
                }
                if (!folderchecksumcorrect)
                {
                    il.log.Add($"{DateTime.Now} => обнаружены изменения в папке: {Folders[j].parentfolder}");
                }
                if (i < currenthashtree.Folders.Count) 
                { 
                foreach (HashTreeNode htn in Folders[j].Items)
                {
                    il.Addlog(CheckFile(htn, currenthashtree, i, out bool extracheck));
                    if (extracheck)
                    {
                        Task.Run(() =>
                        {
                            var temp = ExtendedCheckFile(htn, currenthashtree);
                            foreach (string s in temp) il.log.Add(s);
                        });
                    }
                } 
                }
            }
            for (int i = 0; i < newfolder.Length; i++) if (newfolder[i]) needfushercheck.Add(currenthashtree.Folders[i]);
            foreach (FolderNode fn in needfushercheck) 
            {
                var temp = ExtrafolderCheck(fn);
                foreach (string s in temp) il.Addlog(s);
            }
            return il;
        }
        public IntegrityLog PerformLiteIntegrityCheck(HashTree currenthashtree)
        {
            IntegrityLog il = new IntegrityLog();
            for (int j = 0; j < Folders.Count; j++)
            {
                bool folderchecksumcorrect = false;
                int i;
                for (i = 0; i < currenthashtree.Folders.Count; i++)
                {
                    if (currenthashtree.Folders[i].folderhashsumm == Folders[j].folderhashsumm) folderchecksumcorrect = true;
                }
                if (!folderchecksumcorrect)
                {
                    il.Addlog($"{DateTime.Now} => обнаружены изменения в папке: {Folders[j].parentfolder}");
                }
                foreach (HashTreeNode htn in Folders[j].Items)
                {
                    il.Addlog(CheckFile(htn, currenthashtree, i, out bool extracheck));
                }
            }
            return il;
        }
    }
}
