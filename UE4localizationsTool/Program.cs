using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using AssetParser;
namespace UE4localizationsTool
{
    internal static class Program
    {
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;



        public static string commandlines =
         $"{AppDomain.CurrentDomain.FriendlyName}  export    <(Locres/Uasset) FilePath>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  import    <(txt) FilePath>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} -import    <(txt) FilePath>\n"; 
        // $"{AppDomain.CurrentDomain.FriendlyName} exportall <Folder>\n" +    Lazy to do that :(
        // $"{AppDomain.CurrentDomain.FriendlyName} importall  <Folder>"
      


        public static  void SaveTextFile(string FilePath,List<List<string>> strings)
        {
            string[] stringsArray = new string[strings.Count];
            int i = 0;
            foreach (var item in strings)
            {
                stringsArray[i] = item[1];
                i++;
            }
            File.WriteAllLines(FilePath + ".txt", stringsArray);
        }
        public static void EditList(string FilePath, ref List<List<string>> strings)
        {
           string[] Texts= File.ReadAllLines(FilePath);
            if (Texts.Length < strings.Count)
            {
                throw new Exception("Text file is too short"); 
            }
            for (int i = 0; i < Texts.Length; i++)
            {
                strings[i][1] = Texts[i];
            }
        }


        public static  void commandargs(string[] args)
        {
            string Command = args[0];
            string FilePath = args[1];

            if (Command.ToLower() == "export")
            {
                if (!FilePath.ToLower().EndsWith(".locres") && !FilePath.ToLower().EndsWith(".uasset"))
                {
                    throw new Exception("Invalid file type: " + Path.GetFileName(FilePath));
                }
                if (!File.Exists(FilePath))
                {
                    throw new Exception("File not existed:" + FilePath);

                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Exporting please wait...");
                Console.ForegroundColor = ConsoleColor.White;
                try
                {
                    if (FilePath.ToLower().EndsWith(".locres"))
                    {
                    
                            locres locres =  new locres(FilePath);
                            SaveTextFile(FilePath, locres.Strings);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Done!");
                            Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (args[1].ToLower().EndsWith(".uasset"))
                    {
                           Uasset Uasset = new Uasset(FilePath);
                           Uexp Uexp = new Uexp(Uasset);
                           SaveTextFile(FilePath, Uexp.Strings);
                           Console.ForegroundColor = ConsoleColor.Green;
                           Console.WriteLine("Done!");
                           Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        throw new Exception("Invalid file type: " + Path.GetFileName(FilePath));
                    }
                
                }
                catch
                {
                    throw new Exception("Failed to export: " + Path.GetFileName(FilePath));
                }
            }
            else if (Command.ToLower() == "import"|| Command.ToLower() == "-import")
            {

                if (FilePath.ToLower().EndsWith(".txt"))
                {
                    if (!File.Exists(FilePath))
                    {
                        throw new Exception("File not existed:" + FilePath);
                    }

                    FilePath = Path.ChangeExtension(FilePath, null);
                    if (!FilePath.ToLower().EndsWith(".locres") && !FilePath.ToLower().EndsWith(".uasset"))
                    {
                        throw new Exception("Invalid file type: " +Path.GetFileName(FilePath));
                      
                    }

                    if (!File.Exists(FilePath))
                    {
                        throw new Exception("File not existed:" + FilePath);
                    
                    }


                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Importing please wait...");
                    Console.ForegroundColor = ConsoleColor.White;

                    try
                    {
                        if (FilePath.ToLower().EndsWith(".locres"))
                        {

                            locres locres = new locres(FilePath);
                            EditList(FilePath+".txt", ref locres.Strings);
                            if (Command == "-import")
                            {
                                locres.SaveFile(FilePath);
                            }
                            else
                            {
                                locres.SaveFile(FilePath + ".NEW");
                            }
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Done!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (FilePath.ToLower().EndsWith(".uasset"))
                        {
                            Uasset Uasset = new Uasset(FilePath);
                            Uexp Uexp = new Uexp(Uasset);
                            EditList(FilePath + ".txt", ref Uexp.Strings);

                            if (Command == "-import")
                            {
                                Uexp.SaveFile(FilePath);
                            }
                            else
                            {
                                Uexp.SaveFile(Path.ChangeExtension(FilePath,null)+".NEW");
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Done!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            throw new Exception("Invalid file type: " + Path.GetFileName(FilePath));
                        }

                    }
                    catch
                    {
                        throw new Exception("Failed to import: " + Path.GetFileName(FilePath));
                    }

                }
                else
                {
                    throw new Exception($"This file '{Path.GetFileName(FilePath)}' is not txt file.");
                }
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static  void Main(string[] args)
        {
            
            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.WriteLine();
                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid number of arguments.\n" + commandlines);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                try
                {
                    commandargs(args);
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                return;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
