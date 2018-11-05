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
                foreach (Keyword SuggestedKeyword in lib.LibKeywords)
                    if (SuggestedKeyword.Name.Trim().ToLower().Equals(name.Trim().ToLower()))
                    {
                        return true;
                    }
            return false;
        }

        internal static void PopulateSuggestionsList(bool Selenium, bool BuiltIn)
        {
            Lib lib = new Lib
            {
                Name = "CUSTOM"
            };

            DirectoryInfo d = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"RobotKeywords\Standard libraries\"));
            foreach (var file in d.GetFiles("*.xlsx", SearchOption.AllDirectories))
            {
                lib = new Lib
                {
                    Name = file.Name.Replace(".xlsx", ""),
                    LibKeywords = ExcelLibsGetter.ReadAllKeywordsFromExcelSecondType(file.FullName, KeywordType.STANDARD)
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
                    LibKeywords = ExcelLibsGetter.ReadAllKeywordsFromExcelSecondType(file.FullName, type)
                };
                Suggestions.Add(lib);
            }
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
                KeywordType.FOR_LOOP_IN_RANGE, -1);

            Lib lib = new Lib();
            lib.LibKeywords.Add(ForLoopInRange);
            lib.Name = "FOR_LOOP_IN_RANGE";
            Suggestions.Add(lib);

            currentKeywordParams = new List<Param>
            {
                new Param("Param name", ""),
                new Param("List", "")
            };
            Keyword ForLoopElements = new Keyword("ForLoopElements",
                "Loops through all the values in the provided list.", null, "",
                currentKeywordParams, "", false,
                KeywordType.FOR_LOOP_ELEMENTS, -1);

            Lib lib1 = new Lib();
            lib1.LibKeywords.Add(ForLoopElements);
            lib1.Name = "FOR_LOOP_ELEMENTS";
            Suggestions.Add(lib1);
        }

        internal static Keyword GetForLoop(KeywordType keywordType)
        {
            foreach (Lib lib in Suggestions)
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
        internal List<Keyword> LibKeywords = new List<Keyword>();
        internal string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}