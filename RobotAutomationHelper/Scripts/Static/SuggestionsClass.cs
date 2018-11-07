﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RobotAutomationHelper.Scripts
{
    internal static class SuggestionsClass
    {
        internal static List<Lib> Suggestions = new List<Lib>();
        private static List<Keyword> Selenium = new List<Keyword>();
        private static List<Keyword> BuiltIn = new List<Keyword>();

        internal static void CleanUp()
        {
            Suggestions = new List<Lib>();
            Selenium = new List<Keyword>();
            BuiltIn = new List<Keyword>();
        }

        internal static bool IsInSuggestionsList(string name)
        {
            foreach (Lib lib in Suggestions)
                if (lib.ToInclude)
                    foreach (Keyword SuggestedKeyword in lib.LibKeywords)
                    if (SuggestedKeyword.Name.Trim().ToLower().Equals(name.Trim().ToLower()))
                    {
                        return true;
                    }
            return false;
        }

        internal static bool ContainsName(string name, bool toInclude, bool isPopulating)
        {
            string[] arrLine;
            string fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "Preferences.txt";
            if (File.Exists(fileName))
            {
                arrLine = File.ReadAllLines(fileName);
                for (int i = 0; i < arrLine.Count(); i++)
                {
                    if (arrLine[i].StartsWith(name + "="))
                    {
                        string[] tempString = arrLine[i].Split(new string[]{ "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempString[tempString.Count() - 1].ToLower().Equals(toInclude.ToString().ToLower()))
                            return toInclude;
                        else
                        {
                            if (!isPopulating)
                            {
                                tempString[tempString.Count() - 1] = toInclude.ToString().ToLower();
                                arrLine[i] = tempString[0] + "=" + tempString[tempString.Count() - 1];
                                File.WriteAllLines(fileName, arrLine);
                            }
                            return !toInclude;
                        }
                    }
                }

                List<string> tempList = new List<string>();
                tempList.AddRange(arrLine);
                tempList.Add(name + "=" + toInclude.ToString().ToLower());
                File.WriteAllLines(fileName, tempList);
                return toInclude;
            }
            else
            {
                var myFile = File.Create(fileName);
                myFile.Close();
                File.WriteAllLines(fileName, new string[] { name + "=" + toInclude.ToString().ToLower() });
                return toInclude;
            }
        }

        internal static void PopulateSuggestionsList(bool Selenium, bool BuiltIn)
        {
            Lib lib = new Lib
            {
                Name = "CUSTOM",
                LibKeywords = new List<Keyword>(),
                ToInclude = ContainsName("CUSTOM", true, true),
                keyType = KeywordType.CUSTOM
            };
            Suggestions.Add(lib);

            DirectoryInfo d = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"RobotKeywords\Standard libraries\"));
            foreach (var file in d.GetFiles("*.xlsx", SearchOption.AllDirectories))
            {
                lib = new Lib
                {
                    Name = file.Name.Replace(".xlsx", ""),
                    ToInclude = ContainsName(file.Name.Replace(".xlsx", ""), true, true),
                    LibKeywords = ExcelLibsGetter.ReadAllKeywordsFromExcelSecondType(file.FullName, KeywordType.STANDARD),
                    keyType = KeywordType.STANDARD
                };
                Suggestions.Add(lib);
            }

            d = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"RobotKeywords\External libraries\"));
            foreach (var file in d.GetFiles("*.xlsx", SearchOption.AllDirectories))
            {
                KeywordType type = KeywordType.STANDARD;
                foreach (KeywordType temp in Enum.GetValues(typeof(KeywordType)))
                {
                    if (temp.ToString().ToLower().Equals(file.Name.Replace(".xlsx","").ToLower()))
                    {
                        type = temp;
                        break;
                    }
                }

                lib = new Lib
                {
                    Name = file.Name.Replace(".xlsx", ""),
                    ToInclude = ContainsName(file.Name.Replace(".xlsx", ""), true, true),
                    LibKeywords = ExcelLibsGetter.ReadAllKeywordsFromExcelSecondType(file.FullName, type),
                    keyType = type
                };
                Suggestions.Add(lib);
            }

            PopulateForLoops();
        }

        private static void PopulateForLoops()
        {
            List<Param> currentKeywordParams = new List<Param>
            {
                new Param("Param name", ""),
                new Param("Initial value", ""),
                new Param("End value", "")
            };
            Keyword ForLoopInRange = new Keyword("ForLoopInRange",
                "For loop from the initial value to the end value ( excluding ) using the param.", null, "",
                currentKeywordParams, "", false,
                KeywordType.FOR_LOOP_IN_RANGE, -1, "ForLoop", null);

            Lib lib = new Lib
            {
                Name = "FOR_LOOP_IN_RANGE",
                LibKeywords = new List<Keyword>(),
                ToInclude = true,
                keyType = KeywordType.FOR_LOOP_IN_RANGE
            };
            lib.LibKeywords.Add(ForLoopInRange);
            Suggestions.Add(lib);

            currentKeywordParams = new List<Param>
            {
                new Param("Param name", ""),
                new Param("List", "")
            };

            Keyword ForLoopElements = new Keyword("ForLoopElements",
                "Loops through all the values in the provided list.", null, "",
                currentKeywordParams, "", false,
                KeywordType.FOR_LOOP_ELEMENTS, -1, "ForLoop", null);

            Lib lib1 = new Lib
            {
                Name = "FOR_LOOP_ELEMENTS",
                LibKeywords = new List<Keyword>(),
                ToInclude = true,
                keyType = KeywordType.FOR_LOOP_ELEMENTS
            };
            lib1.LibKeywords.Add(ForLoopElements);
            Suggestions.Add(lib1);
        }

        internal static Keyword GetForLoop(KeywordType keywordType)
        {
            foreach (Lib lib in Suggestions)
                if (lib.ToInclude)
                    foreach (Keyword temp in lib.LibKeywords)
                        if (temp.Type.Equals(keywordType))
                            return temp;
            return null;
        }

        internal static List<Keyword> GetCustomLibKeywords()
        {
            foreach (Lib lib in Suggestions)
                if (lib.Name.Equals("CUSTOM"))
                    return lib.LibKeywords;
            return null;
        }
    }

    internal class Lib
    {
        internal List<Keyword> LibKeywords;
        internal string Name { get; set; }
        internal bool ToInclude;
        internal KeywordType keyType;

        public override string ToString()
        {
            return Name;
        }
    }
}