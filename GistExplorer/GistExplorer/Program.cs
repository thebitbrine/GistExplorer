using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using QuickType;

namespace GistExplorer
{
    class Program
    {
        public Gists[] GlobalGists;
        [STAThread]
        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Program p = new Program();
            Console.Title = "GistExplorer v1.2";
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
                Console.Clear();
            }

        }

        public void GetAllGists(string Username)
        {
            var Response = GetWebString($"https://api.github.com/users/{Username}/gists?per_page=10000");
            GlobalGists = Gists.FromJson(Response);
        }

        public Gists[] Search(string Query)
        {
            List<Gists> Results = new List<Gists>();

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.StartsWith(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().StartsWith(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.Contains(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.EndsWith(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().EndsWith(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Value.Language.HasValue && File.Value.Language.Value.ToString().ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Value.Language.HasValue && Query.ToLower().Contains(File.Value.Language.Value.ToString().ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (Gist.Description.ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            return Results.ToArray();
        }

        public string User = "NOT SET";
        public void Run(string[] args)
        {
            User = args[0];
            if(GlobalGists == null || GlobalGists?.Count() <= 0)
                new Thread(() => GetAllGists(User)) { IsBackground = true}.Start();
            Banner();
            Console.Write("Enter query: ");
            string Query = Console.ReadLine();
            while (GlobalGists == null) Thread.Sleep(250);
            var SearchResults = Search(Query);

            if (SearchResults.Count() > 0)
            {
                int SnipSelect = -1;
                do
                {
                    Console.Clear();
                    Banner();
                    Console.WriteLine("Query results:");
                    int qIndex = 1;
                    foreach (var Snipp in SearchResults)
                    {
                        Console.WriteLine($" {qIndex.ToString().PadLeft(2, '0')}) {StringLimit(Snipp.Files.First().Key, 27).PadRight(26, ' ')}\t| {User},\t{NeatDate(Snipp.UpdatedAt)},\t{StringLimit(Snipp.Description, 50)}");
                        qIndex++;
                    }
                    Console.Write("Select snippet: ");
                    int.TryParse(Console.ReadLine(), out SnipSelect);
                } while (SnipSelect == -1);
                if (SnipSelect > 0)
                {
                    Console.Clear();
                    Banner();
                    Console.WriteLine($"Downloading {SearchResults[SnipSelect - 1].Files.First().Key}...");
                    Console.Clear();
                    Banner();
                    string RawSnippet = GetWebString(SearchResults[SnipSelect - 1].Files.First().Value.RawUrl.ToString());
                    Clipboard.SetText(RawSnippet);
                    Console.WriteLine(SearchResults[SnipSelect - 1].Files.First().Key + " copied to clipboard.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Nothing found.");
                Console.ReadKey();
            }
        }
        
        public string NeatDate(DateTimeOffset Date)
        {
            string Month = Date.ToString("MMM");
            string Day = Date.Day.ToString("D2");
            string Year = Date.Year.ToString();

            return $"{Month} {Day} {Year}";
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

        public string GetWebString1(string URL)
        {
            var client = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 };
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 520)");
            client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            client.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            client.Headers.Add(HttpRequestHeader.Host, "gist.github.com");
            return client.DownloadString(URL);
        }



        public static string GetWebString(string URI)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), URI))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Mobile Safari/537.36");
                    return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                }
            }
        }


        public void Banner()
        {
            Console.WriteLine(CenterString($"[{User}]", 40));
            Console.WriteLine("  GistExplorer v1.2 (Ultimate Edition)  ");
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
    }
}
