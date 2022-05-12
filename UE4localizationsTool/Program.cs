using System;
using System.Windows.Forms;

namespace UE4localizationsTool
{
    internal static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]


        static void Main()
        {

            // Uasset Uasset = new Uasset(@"E:\Games\SIFU\Sifu\Content\Paks\Exports\Sifu\Content\Localization\StringTables\Menu\SkillTree\ST_UI_SkillTree.uasset");
            //MemoryList memoryList = new MemoryList(@"E:\win10\Downloads\Compressed\FModel\Output\Exports\Sifu\Content\Localization\Dialogues\es\amr");
            //  MemoryList memoryList = new MemoryList(Uasset.Exports_Directory[0].ExportData);
            // new Uexp(Uasset);
            //  Console.WriteLine(Uasset.GetExportPropertyName(Uasset.Exports_Directory[0].ExportClass));
            //  Console.WriteLine(memoryList.GetSize());
            //Console.WriteLine(memoryList.GetByteValue(true,1));
            //Console.WriteLine(memoryList.GetUByteValue());



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
