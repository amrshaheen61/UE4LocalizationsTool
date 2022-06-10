using AssetParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UE4localizationsTool
{
    public class Commads
    {
        private List<List<string>> Strings;
        private int SizeOfRecord = 0;
        public Commads(string Options, string SourcePath)
        {

            string[] Paths;
            string ConsoleText;
            switch (Options.ToLower())
            {
                case "export"://Single File
                    Strings = new List<List<string>>();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    ConsoleText = $"Exporting... '{Path.GetFileName(SourcePath)}' ";
                    Console.WriteLine(ConsoleText);
                    Console.ForegroundColor = ConsoleColor.White;

                    Strings = Export(SourcePath);
                    SizeOfRecord = Strings.Count;
                    SaveTextFile(SourcePath + ".txt");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                    Console.WriteLine("Done");
                    Console.ForegroundColor = ConsoleColor.White;

                    break;
                case "exportall"://Folders
                    Strings = new List<List<string>>();
                    Paths = SourcePath.Split(new char[] { '*' }, 2);
                    ExportFolder(Paths[0]);
                    SaveTextFile(Paths[1]);
                    break;

                case "import"://Single File
                case "-import"://Single File Without rename
                    Console.ForegroundColor = ConsoleColor.Blue;
                    ConsoleText = $"Importing... '{Path.GetFileName(SourcePath)}' ";
                    Console.WriteLine(ConsoleText);
                    Console.ForegroundColor = ConsoleColor.White;

                    if (!SourcePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Invalid text file type: " + Path.GetFileName(SourcePath));
                    }

                    Import(Path.ChangeExtension(SourcePath, null), File.ReadAllLines(SourcePath), Options.ToLower());
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                    Console.WriteLine("Done");
                    Console.ForegroundColor = ConsoleColor.White;


                    break;

                case "importall"://Folders
                case "-importall"://Folders Without rename Files
                    Paths = SourcePath.Split(new char[] { '*' }, 2);

                    if (!Paths[1].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Invalid text file type: " + Path.GetFileName(SourcePath));
                    }

                    ImportFolder(Paths[0], File.ReadAllLines(Paths[1]), Options.ToLower());
                    break;
                default:
                    throw new Exception("Invalid number of arguments.\n" + Program.commandlines);
            }

        }


        private void SaveTextFile(string FilePath)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string ConsoleText = "Saving text file... ";
            Console.WriteLine(ConsoleText);
            Console.ForegroundColor = ConsoleColor.White;

            string[] stringsArray = new string[Strings.Count];
            int i = 0;
            foreach (var item in Strings)
            {
                if (item[0] == "[~PATHFile~]")
                {
                    stringsArray[i] = item[1];
                }
                else
                {
                    stringsArray[i] = item[0] + "=" + item[1];
                }
                i++;
            }
            File.WriteAllLines(FilePath, stringsArray);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
            Console.WriteLine("Done");
            Console.ForegroundColor = ConsoleColor.White;
        }


        private List<List<string>> Export(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                throw new Exception("File not existed: " + FilePath);
            }


            if (FilePath.EndsWith(".locres", StringComparison.OrdinalIgnoreCase))
            {
                locres locres = new locres(FilePath);
                return locres.Strings;
                //  SizeOfRecord = locres.Strings.Count;
            }
            else if (FilePath.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
            {
                Uasset Uasset = new Uasset(FilePath);
                Uexp Uexp = new Uexp(Uasset);
                return Uexp.Strings;
                //  SizeOfRecord = Uexp.Strings.Count;
            }
            else
            {
                throw new Exception("Invalid language file type: " + Path.GetFileName(FilePath));
            }
        }


        private void ExportFolder(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                throw new Exception("Directory not existed: " + FolderPath);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            string ConsoleText = "Scaning for files...";
            Console.WriteLine(ConsoleText);
            Console.ForegroundColor = ConsoleColor.White;

            string[] LanguageFiles = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".locres", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase)).ToArray<string>();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
            Console.WriteLine("Done");
            Console.ForegroundColor = ConsoleColor.White;

            if (LanguageFiles.Count() == 0)
            {
                throw new Exception($"This directory '{FolderPath}' not contine any language files.");
            }

            for (int i = 0; i < LanguageFiles.Count(); i++)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                ConsoleText = $"[{ i + 1}:{LanguageFiles.Count()}] Exporting... '{Path.GetFileName(LanguageFiles[i])}' ";
                Console.WriteLine(ConsoleText);
                Console.ForegroundColor = ConsoleColor.White;
                Strings.Add(new List<string>() { "[~PATHFile~]", "", "[~PATHFile~]" });
                int ThisPosition = Strings.Count - 1;
                try
                {
                    Strings.AddRange(Export(LanguageFiles[i]));
                    SizeOfRecord = Strings.Count;
                    Strings[ThisPosition][1] = "[PATH]" + SizeOfRecord + "*" + LanguageFiles[i].Replace(FolderPath, "") + "[PATH]";
                }
                catch (Exception EX)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                    Console.WriteLine("Fail");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Can't parse it, the tool will skip this file.\n" + EX.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (Strings.Count != 0)
                        Strings.RemoveAt(Strings.Count - 1);

                    continue;

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                Console.WriteLine("Done");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }


        void EditList(List<List<string>> Strings, string[] StringValues)
        {
            if (StringValues.Length < Strings.Count)
            {
                throw new Exception("Text file is too short");
            }
            for (int i = 0; i < StringValues.Length; i++)
            {
                try
                {
                    Strings[i][1] = StringValues[i].Split(new char[] { '=' }, 2)[1];
                }
                catch
                {
                    throw new Exception("Can't parse this line from text file: " + StringValues[i]);
                }
            }
        }



        private void Import(string FilePath, string[] Values, string Option)
        {

            if (!File.Exists(FilePath))
            {
                throw new Exception("File not existed: " + FilePath);
            }

            if (FilePath.EndsWith(".locres", StringComparison.OrdinalIgnoreCase))
            {
                locres locres = new locres(FilePath);
                EditList(locres.Strings, Values);
                if (Option == "-import")
                {
                    locres.SaveFile(FilePath);
                    return;
                }

                FilePath = Path.ChangeExtension(FilePath, null) + "_NEW.locres";
                locres.SaveFile(FilePath);
            }
            else if (FilePath.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
            {
                Uasset Uasset = new Uasset(FilePath);
                Uexp Uexp = new Uexp(Uasset);
                EditList(Uexp.Strings, Values);

                if (Option == "-import")
                {
                    Uexp.SaveFile(FilePath);
                    return;
                }

                FilePath = Path.ChangeExtension(FilePath, null) + "_NEW.uasset";
                Uexp.SaveFile(FilePath);
            }
            else
            {
                throw new Exception("Invalid language file type: " + Path.GetFileName(FilePath));
            }

        }

        private void ImportFolder(string FolderPath, string[] Values, string Option)
        {

            if (!Directory.Exists(FolderPath))
            {
                throw new Exception("Directory not existed: " + FolderPath);
            }


            int[] Indexs = Values.Select((Value, Index) => (Value.StartsWith("[PATH]") && Value.EndsWith("[PATH]")) ? Index : -1).Where(index => index != -1).ToArray();

            if (Indexs.Length == 0)
            {
                throw new Exception("Source text file is corrupted or not contain text or you modified language files path ([PATH]....[PATH]).");
            }

            for (int PathIndex = 0; PathIndex < Indexs.Length; PathIndex++)
            {
                string[] RecordInfo = Values[Indexs[PathIndex]].Replace("[PATH]", "").Trim().Split(new char[] { '*' }, 2);
                int ArraySize = int.Parse(RecordInfo[0]);
                string FilePath = RecordInfo[1];

                if (string.IsNullOrEmpty(FilePath))
                {
                    Console.WriteLine("Can't get path from" + Indexs[PathIndex] + "line");
                    continue;
                }
                FilePath = FolderPath + @"\" + FilePath;
                FilePath = FilePath.Replace(@"\\", @"\");
                Console.ForegroundColor = ConsoleColor.Blue;
                string ConsoleText = $"[{PathIndex + 1}:{Indexs.Length}] Importing... '{Path.GetFileName(FilePath)}' ";
                Console.WriteLine(ConsoleText);
                Console.ForegroundColor = ConsoleColor.White;
                string[] StringArrayValues = new string[ArraySize];
                Array.Copy(Values, Indexs[PathIndex] + 1, StringArrayValues, 0, ArraySize);

                try
                {
                    if (Option == "importall")
                        Import(FilePath, StringArrayValues, "import");
                    else
                        Import(FilePath, StringArrayValues, "-import");
                }
                catch (Exception EX)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                    Console.WriteLine("Fail");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Can't parse it, the tool will skip this file.\n" + EX.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (Strings.Count != 0)
                        Strings.RemoveAt(Strings.Count - 1);

                    continue;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(ConsoleText.Length, Console.CursorTop - 1);
                Console.WriteLine("Done");
                Console.ForegroundColor = ConsoleColor.White;
            }


        }

    }
}
