﻿using RobotAutomationHelper.Scripts;
using System;
using System.Collections.Generic;

namespace RobotAutomationHelper
{
    [Serializable]
    internal class Keyword
    {
        internal List<Keyword> Keywords { get; set; }
        internal List<Keyword> ForLoopKeywords { get; set; }
        internal string Arguments { get; set; }
        internal List<Param> Params { get; set; }
        internal string Name { get; set; }
        internal string Documentation { get; set; }
        internal string OutputFilePath { get; set; }
        internal bool Implemented { get; set; }
        internal bool Saved { get; private set; }
        internal KeywordType Type { get; set; }
        internal int SuggestionIndex { get; set; }
        internal bool Overwrite { get; set; }
        internal bool Recursive { get; set; }

        internal Keyword(string Name, string Documentation, List<Keyword> Keywords, string Arguments, List<Param> Params, string OutputFilePath, bool Saved, KeywordType Type, int SuggestionIndex)
        {
            this.Name = Name;
            this.Documentation = Documentation;
            if (Keywords != null)
                this.Keywords = ExtensionMethods.DeepClone(Keywords);
            else
                this.Keywords = new List<Keyword>();

            if (ForLoopKeywords != null)
                ForLoopKeywords = ExtensionMethods.DeepClone(ForLoopKeywords);
            else
                ForLoopKeywords = new List<Keyword>();
            
            if (Params != null)
                this.Params = ExtensionMethods.DeepClone(Params);
            else
                this.Params = new List<Param>();

            this.Arguments = Arguments;
            this.OutputFilePath = OutputFilePath;
            Implemented = true;
            Overwrite = false;
            this.Saved = Saved;
            this.Type = Type;
            this.SuggestionIndex = SuggestionIndex;
            Recursive = false;
        }

        internal Keyword()
        {
        }

        internal void CopyKeyword(Keyword keyword)
        {
            Name = keyword.Name;
            Documentation = keyword.Documentation;

            if (keyword.Keywords != null)
                Keywords = ExtensionMethods.DeepClone(keyword.Keywords);
            else
                Keywords = new List<Keyword>();

            if (keyword.ForLoopKeywords != null)
                ForLoopKeywords = ExtensionMethods.DeepClone(keyword.ForLoopKeywords);
            else
                ForLoopKeywords = new List<Keyword>();

            if (keyword.Params != null)
                Params = ExtensionMethods.DeepClone(keyword.Params);
            else
                Params = new List<Param>();

            Arguments = keyword.Arguments;
            OutputFilePath = keyword.OutputFilePath;
            Implemented = keyword.Implemented;
            Saved = keyword.Saved;
            Type = keyword.Type;
            SuggestionIndex = keyword.SuggestionIndex;
            Overwrite = keyword.Overwrite;
        }

        internal Keyword(string Name, string OutputFilePath)
        {
            this.Name = Name;
            this.OutputFilePath = OutputFilePath;
            Documentation = "";
            SuggestionIndex = -1;
            Recursive = false;
        }

        // convert keyword string taken from file into keyword
        internal Keyword(string KeywordString, string OutputFilePath, bool IsKeywordString)
        {
            if (!KeywordString.Equals(""))
            {
                Implemented = true;
                string[] splitKeyword;
                KeywordString = KeywordString.Trim();
                if (!StringAndListOperations.StartsWithVariable(KeywordString))
                    splitKeyword = KeywordString.Split(new string[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                else
                    splitKeyword = new string[] { KeywordString };

                bool found = false;
                foreach (Keyword key in SuggestionsClass.Suggestions)
                {
                    if (splitKeyword[0].ToLower().Trim().Equals(key.Name.ToLower().Trim()))
                    {
                        CopyKeyword(key);
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    for (int i = 1; i < splitKeyword.Length; i++)
                    {
                        if (!splitKeyword[i].Contains("="))
                            Params[i - 1].Value = splitKeyword[i];
                        else
                        {
                            // check if after spliting the first string matches any param name
                            string[] temp = splitKeyword[i].Split('=');
                            foreach (Param tempParam in Params)
                            {
                                if (tempParam.Name.ToLower().Trim().Equals(temp[0].ToLower().Trim()))
                                    tempParam.Value = splitKeyword[i].Replace(temp[0] + "=", "");
                            }
                        }
                    }
                }
                else
                {
                    Name = splitKeyword[0];
                    if (splitKeyword.Length > 1)
                        Params = new List<Param>();
                    for (int i = 1; i < splitKeyword.Length; i++)
                    {
                        if (!splitKeyword[i].Contains("="))
                            Params.Add(new Param("", splitKeyword[i]));
                        else
                        {
                            // check if after spliting the first string matches any param name
                            string[] temp = splitKeyword[i].Split('=');
                            Params.Add(new Param(temp[0], temp[1]));
                        }
                    }
                    this.OutputFilePath = OutputFilePath;
                    Documentation = "";
                    SuggestionIndex = -1;
                }
            }
            else
            {
                Name = "";
                this.OutputFilePath = OutputFilePath;
                Documentation = "";
                SuggestionIndex = -1;
            }
        }

        internal string ParamsToString()
        {
            string paramsString = "";
            if (Params != null)
                foreach (Param tempParam in Params)
                    paramsString += "  " + tempParam.Value;
            return paramsString;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case KeywordType.SELENIUM: return "[S] " + Name;
                case KeywordType.BUILT_IN: return "[B] " + Name;
            }
            return Name;
        }
    }

    internal enum KeywordType
    {
        CUSTOM, SELENIUM, BUILT_IN, FOR_LOOP_IN_RANGE, FOR_LOOP_ELEMENTS
    }
}
