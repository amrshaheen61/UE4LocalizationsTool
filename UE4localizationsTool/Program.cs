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
          "- for import without rename file be careful with this command.\n\n" +

          "To use last filter you applied before in GUI, add (TRUE) after command line\n" +
          "filter will apply only in name table (Remember to apply the same filter when importing)\n\n" +
          "Examples:\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} export Actions.uasset\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} import Actions.uasset.txt\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} exportall Actions\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} exportall Actions True\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} importall Actions\n" +
         $"{AppDomain.CurrentDomain.FriendlyName} importall Actions True";


        [STAThread]

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                bool UseFilter = false;
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
                        if (args.Length > 3)
                        {
                            bool.TryParse(args[3], out UseFilter);
                        }
                        new Commads(args[0], args[1] + "*" + args[2], UseFilter);
                    }
                    else
                    {
                        if (args.Length > 2)
                        {
                            bool.TryParse(args[2], out UseFilter);
                        }
                        new Commads(args[0], args[1], UseFilter);
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
