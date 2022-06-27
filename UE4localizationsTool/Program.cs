using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace UE4localizationsTool
{
    internal static class Program
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;


        public static string commandlines =
         $"{AppDomain.CurrentDomain.FriendlyName}  export     <(Locres/Uasset) FilePath>  <Options>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  import     <(txt) FilePath>  <Options>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} -import     <(txt) FilePath>  <Options>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  exportall  <Folder> <TxtFile> <Options>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  importall  <Folder> <TxtFile>  <Options>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} -importall  <Folder> <TxtFile>  <Options>\n\n" +
          "- for import without rename file be careful with this command.\n\n" +

          "Options:\n" +
          "To use last filter you applied before in GUI, add (-f \\ -filter) after command line\n" +
          "filter will apply only in name table (Remember to apply the same filter when importing)\n\n" +

          "To export file without including the names use (-nn \\ -NoName)" +
          "\n(Remember to use this command when importing)\n\n" +

          "Examples:\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} export Actions.uasset\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} import Actions.uasset.txt\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} exportall Actions text.txt\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} importall Actions text.txt\n";

        public static (bool UseFilter, bool NoName) GetArgs(int Index, string[] args)
        {
            bool usefilter = false;
            bool noname = false;

            for (int n = Index; n < args.Length; n++)
            {
                switch (args[n].ToLower())
                {
                    case "-f":
                    case "-filter":
                        usefilter = true;
                        break;
                    case "-nn":
                    case "-noname":
                        noname = true;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Invalid command: " + args[n]);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
            return (usefilter, noname);
        }


        public static void CheckArges(int Index, string[] args)
        {
            for (int n = 0; n < Index; n++)
            {
                switch (args[n].ToLower())
                {
                    case "-f":
                    case "-filter":
                        throw new Exception("Invalid number of arguments.\n\n" + commandlines);
                    case "-nn":
                    case "-noname":
                        throw new Exception("Invalid number of arguments.\n\n" + commandlines);
                }
            }
        }



        [STAThread]

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                bool UseFilter = false;
                bool NoName = false;
                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid number of arguments.\n\n" + commandlines);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                try
                {

                    if (args[0].ToLower() == "importall" || args[0].ToLower() == "-importall" || args[0].ToLower() == "exportall")
                    {
                        if (args.Length < 3)
                        {
                            throw new Exception("Invalid number of arguments.\n\n" + commandlines);
                        }

                        CheckArges(3, args);
                        (UseFilter, NoName) = GetArgs(3, args);
                        new Commads(args[0], args[1] + "*" + args[2], UseFilter, NoName);
                    }
                    else
                    {
                        CheckArges(2, args);
                        (UseFilter, NoName) = GetArgs(2, args);
                        new Commads(args[0], args[1], UseFilter, NoName);
                    }

                }
                catch (Exception ex)
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
