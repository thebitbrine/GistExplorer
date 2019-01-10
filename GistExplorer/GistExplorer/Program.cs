using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace GistExplorer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Program p = new Program();
            Console.Title = "GistExplorer v1.0";
            Console.Clear();
            if (args.Length == 0)
            {
                Console.Write("Usage: \"GistExplorer.exe USERNAME\"");
                Console.ReadKey();
            }
            else
            while (true)
            {
                p.Run(args);
                Console.ReadKey();
                Console.Clear();
            }

        }

        public string User = "NOT SET";
        public void Run(string[] args)
        {
            User = args[0];
            Banner();
            List<Snippet> SnippList = new List<Snippet>();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.Write("Enter query: ");
            string Query = Console.ReadLine().ToLower();
            string Response = "";
            if (!string.IsNullOrWhiteSpace(Query))
                Response = GetWebString($"http://gist.github.com/search?utf8=%E2%9C%93&q=user%3A{User}+filename%3A{Query}&ref=searchresults");
            else
                Response = GetWebString($"http://gist.github.com/search?utf8=%E2%9C%93&q=user%3A{User}&ref=searchresults");
            HtmlDocument Doc = new HtmlDocument();
            Doc.LoadHtml(Response);
            var Snippets = Doc.DocumentNode.SelectNodes("//div[@class='gist-snippet']");
            int Index = 0;
            if (Snippets != null)
            {
                foreach (var Snippet in Snippets)
                {
                    if (Snippets[Index].Name == "div")
                    {
                        Snippet InnerSnippet = new Snippet();
                        InnerSnippet.Creator = Snippet.SelectNodes("//span[contains(@class, 'creator')]/a[1]")[Index].InnerText;
                        InnerSnippet.Name = Snippet.SelectNodes("//span[contains(@class, 'creator')]/a[2]")[Index].InnerText;
                        InnerSnippet.LastActive = Snippet.SelectNodes("//div[contains(@class, 'extra-info')]/time-ago[1]")[Index].InnerText;
                        InnerSnippet.Description = Snippet.SelectNodes("//span[contains(@class, 'description')]")[Index].InnerText.Replace("\n", "").TrimEnd(new char[] { ' ' }).TrimStart(new char[] { ' ' });


                        InnerSnippet.IDHash = Snippet.SelectNodes("//div[contains(@class, 'file-box')]/a[1]")[Index].Attributes[1].Value;
                        //string CodeLinkResponse = GetWebString($"{CodeLink}/raw");
                        SnippList.Add(InnerSnippet);
                        Index++;
                    }
                }

                int SnipSelect = -1;
                do
                {
                    Console.Clear();
                    Banner();
                    Console.WriteLine("Query results:");
                    int qIndex = 1;
                    foreach (var Snipp in SnippList)
                    {
                        Console.WriteLine($"{qIndex.ToString().PadLeft(2,'0')}) {Snipp.Name.PadRight(25,' ')}\t| {Snipp.Creator},\t{NeatDate(Snipp.LastActive)},\t{StringLimit(Snipp.Description, 50)}");
                        qIndex++;
                    }
                    Console.Write("Select snippet: ");
                    int.TryParse(Console.ReadLine(), out SnipSelect);
                } while (SnipSelect == -1 || SnipSelect == 0 || SnipSelect > SnippList.Count);
                Console.Clear();
                Banner();
                Console.WriteLine($"Downloading {SnippList[SnipSelect - 1].Name}...");
                Console.Clear();
                Banner();
                string RawSnippet = GetWebString($"{SnippList[SnipSelect - 1].IDHash}/raw");
                Clipboard.SetText(RawSnippet);
                Console.WriteLine(SnippList[SnipSelect - 1].Name + " copied to clipboard.");
            }
            else
            {
                Console.WriteLine("Nothing found.");
            }
        }
        
        public string NeatDate(string Date)
        {
            if(Date[5] == ',')
            {
                return Date.Substring(0, 4) + "0" + Date.Substring(4);
            }
            return Date;
        }

        public string StringLimit(string Text, int Limit)
        {
            if (Text.Length > Limit)
            {
                if(Text.Substring(0, Limit - 3).Last() == ' ') return Text.Substring(0, Limit - 4) + "...";
                else return Text.Substring(0, Limit - 3) + "..";
            }
            return Text;
        }

        public string GetWebString(string URL)
        {
            return new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 }.DownloadString(URL);
        }

        public void Banner()
        {
            Console.WriteLine(CenterString($"[{User}]", 40));
            Console.WriteLine("  GistExplorer v1.0 (Ultimate Edition)  ");
            Console.WriteLine("========================================");
        }
        
        public string CenterString(string stringToCenter, int totalLength)
        {
            return stringToCenter.PadLeft(((totalLength - stringToCenter.Length) / 2)
                                + stringToCenter.Length, '=')
                       .PadRight(totalLength, '=');
        }

        #region Essentials
        public string LogPath = @"data\Logs.txt";
        public bool NoConsolePrint = false;
        public bool NoFilePrint = false;
        public void Print(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "")));
            if (!NoConsolePrint) Console.Write(Tag(String));
        }
        public void Print(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", ""))); if (!NoConsolePrint) Console.Write(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "")); if (!NoConsolePrint) Console.Write(String); }
        }
        public void PrintLine(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
            if (!NoConsolePrint) Console.WriteLine(Tag(String));
        }
        public void PrintLine(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
        }
        public void PrintLine()
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Environment.NewLine);
            if (!NoConsolePrint) Console.WriteLine();
        }
        public void PrintLines(string[] StringArray)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
                if (!NoConsolePrint) Console.WriteLine(Tag(String));
            }
        }
        public void PrintLines(string[] StringArray, bool DoTag)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
                else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
            }
        }
        public void Check()
        {
            if (!NoFilePrint && !System.IO.File.Exists(LogPath)) Touch(LogPath);
        }
        private bool WriteLock = false;
        public void WaitWrite(string Path, string Data)
        {
            while (WriteLock) { System.Threading.Thread.Sleep(20); }
            WriteLock = true;
            System.IO.File.AppendAllText(Path, Data);
            WriteLock = false;
        }
        public string[] ReadData(string DataDir)
        {
            if (System.IO.File.Exists(DataDir))
            {
                List<string> Data = System.IO.File.ReadAllLines(DataDir).ToList<string>();
                foreach (var Line in Data)
                {
                    if (Line == "\n" || Line == "\r" || Line == "\t" || string.IsNullOrWhiteSpace(Line))
                        Data.Remove(Line);
                }
                return Data.ToArray();
            }
            else
                return null;
        }
        public string ReadText(string TextDir)
        {
            if (System.IO.File.Exists(TextDir))
            {
                return System.IO.File.ReadAllText(TextDir);
            }
            return null;
        }
        public string SafeJoin(string[] Array)
        {
            if (Array != null && Array.Length != 0)
                return string.Join("\r\n", Array);
            else return "";
        }
        public void CleanLine()
        {
            Console.Write("\r");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Console.Write("\r");
        }
        public void CleanLastLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            CleanLine();
        }
        public string Rooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public static string StaticRooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public string Tag(string Text)
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + Text;
        }
        public string Tag()
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] ";
        }
        public bool Touch(string Path)
        {
            try
            {
                System.Text.StringBuilder PathCheck = new System.Text.StringBuilder();
                string[] Direcories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (var Directory in Direcories)
                {
                    PathCheck.Append(Directory);
                    string InnerPath = PathCheck.ToString();
                    if (System.IO.Path.HasExtension(InnerPath) == false)
                    {
                        PathCheck.Append("\\");
                        if (System.IO.Directory.Exists(InnerPath) == false) System.IO.Directory.CreateDirectory(InnerPath);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(InnerPath, "");
                    }
                }
                if (IsDirectory(Path) && System.IO.Directory.Exists(PathCheck.ToString())) { return true; }
                if (!IsDirectory(Path) && System.IO.File.Exists(PathCheck.ToString())) { return true; }
            }
            catch (Exception ex) { PrintLine("ERROR: Failed touching \"" + Path + "\". " + ex.Message, true); }
            return false;
        }
        public bool IsDirectory(string Path)
        {
            try
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(Path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                if (System.IO.Path.HasExtension(Path)) return true;
                else return false;
            }
        }
        #endregion

        public class Snippet
        {
            public string Creator { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string LastActive { get; set; }
            public string IDHash { get; set; }
        }
    }
}
