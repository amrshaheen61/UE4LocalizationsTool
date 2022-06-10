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
         $"{AppDomain.CurrentDomain.FriendlyName}  export     <(Locres/Uasset) FilePath>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  import     <(txt) FilePath>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} -import     <(txt) FilePath>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  exportall  <Folder> <TxtFile>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName}  importall  <Folder> <TxtFile>\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} -importall  <Folder> <TxtFile>\n\n" +
          "- for import without rename file be careful with this command.";



        [STAThread]

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.SetCursorPosition(0, Console.CursorTop + 1);

                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid number of arguments.\n" + commandlines);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                try
                {

                    if (args[0].ToLower() == "importall" || args[0].ToLower() == "-importall" || args[0].ToLower() == "exportall")
                    {
                        if (args.Length < 3)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid number of arguments.\n" + commandlines);
                            Console.ForegroundColor = ConsoleColor.White;
                            return;
                        }

                        new Commads(args[0], args[1] + "*" + args[2]);
                    }
                    else
                    {
                        new Commads(args[0], args[1]);
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
