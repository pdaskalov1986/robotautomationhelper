﻿using System;

namespace RobotAutomationHelper.Scripts
{
    internal static class TestCasesListOperations
    {
        internal static string IsPresentInTheTestCasesTree(string name, string fileName, TestCase thisTestCase)
        {
            foreach (TestCase test in RobotAutomationHelper.TestCases)
                if (test.Name.Trim().ToLower().Equals(name.ToLower()))
                    if (!ReferenceEquals(test, thisTestCase))
                        if (test.OutputFilePath.ToLower().Equals(fileName.ToLower()))
                    return fileName + " | " + name;
            return "";
        }

        internal static string IsPresentInTheKeywordTree(string name, string fileName, Keyword thisKeyword)
        {
            foreach (TestCase test in RobotAutomationHelper.TestCases)
                if (test.Steps != null)
                    foreach (Keyword keyword in test.Steps)
                    {
                        if (keyword.Saved && !ReferenceEquals(keyword, thisKeyword)
                            && keyword.Name.Trim().ToLower().Equals(name.ToLower())
                            && keyword.OutputFilePath.ToLower().Equals(fileName.ToLower()))
                            return test.Name + " | " + keyword.Name.Trim();

                        if (keyword.Keywords != null)
                            foreach (Keyword key in keyword.Keywords)
                            {
                                if (!key.Recursive)
                                {
                                    string temp = IsPresentInChildrenKeywords(name, key, fileName, thisKeyword, test.Name + " | " + keyword.Name.Trim());
                                    if (!temp.Equals(""))
                                        return temp;
                                }
                            }
                    }
            return "";
        }

        private static string IsPresentInChildrenKeywords(string name, Keyword keyword, string fileName, Keyword thisKeyword, string path)
        {
            if (!ReferenceEquals(keyword, thisKeyword) && keyword.Implemented
                && keyword.Name.Trim().ToLower().Equals(name.ToLower())
                && keyword.OutputFilePath.ToLower().Equals(fileName.ToLower()))
                return path + " | " + keyword.Name.Trim();

            if (keyword.Keywords != null)
                foreach (Keyword key in keyword.Keywords)
                {
                    if (!key.Recursive)
                    {
                        string temp = IsPresentInChildrenKeywords(name, key, fileName, thisKeyword, path + " | " + keyword.Name.Trim());
                        if (!temp.Equals(""))
                            return temp;
                    }
                }
            return "";
        }
    }
}