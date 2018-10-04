﻿using System.Collections.Generic;
using System.IO;

namespace RobotAutomationHelper.Scripts
{
    public static class WriteToRobot
    {
        public static void AddTestCasesToRobot(TestCase testCase)
        {
            string fileName = testCase.GetOutputFilePath();
            int index = RobotFileHandler.GetLineAfterLastTestCase(fileName);

            //Add test case to robot file
            index = AddName(testCase.GetTestName().Trim(), fileName, index, "test cases");

            //adds documentation
            index = AddTagsDocumentationArguments("[Documentation]", testCase.GetTestDocumentation(), fileName, index);

            //adds tags
            index = AddTagsDocumentationArguments("[Tags]", testCase.GetTestCaseTags(), fileName, index);

            index = AddKeyword(testCase.GetTestSteps(), fileName, index);
        }

        public static void AddKeywordToRobot(Keyword keyword)
        {
            string fileName = keyword.GetOutputFilePath();
            int index = RobotFileHandler.GetLineAfterLastKeyword(fileName);

            //Add keyword to robot file
            index = AddName(keyword.GetKeywordName().Trim(), fileName, index, "keywords");

            //adds documentation
            index = AddTagsDocumentationArguments("[Documentation]", keyword.GetKeywordDocumentation(), fileName, index);

            //adds arguments
            index = AddTagsDocumentationArguments("[Arguments]", keyword.GetKeywordArguments(), fileName, index);

            index = AddKeyword(keyword.GetKeywordKeywords(), fileName, index);
        }

        // add newText on new line to file fileName after specified line
        public static void FileLineAdd(string newText, string fileName, int line_to_add_after)
        {
            string[] arrLine;
            if (File.Exists(fileName))
                arrLine = File.ReadAllLines(fileName);
            else
            {
                string directory = fileName.Replace(fileName.Split('\\')[fileName.Split('\\').Length - 1], "");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                var myFile = File.Create(fileName);
                myFile.Close();
                arrLine = File.ReadAllLines(fileName);
            }

            List<string> temp = new List<string>();
            temp.AddRange(arrLine);
            temp.Insert(line_to_add_after, newText);
            File.WriteAllLines(fileName, temp);
        }

        // add newText on new line to file fileName after specified line
        public static void FileLineReplace(string newText, string fileName, int line_to_add_after)
        {
            string[] arrLine;
            if (File.Exists(fileName))
                arrLine = File.ReadAllLines(fileName);
            else
            {
                string directory = fileName.Replace(fileName.Split('\\')[fileName.Split('\\').Length - 1], "");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                var myFile = File.Create(fileName);
                myFile.Close();
                arrLine = File.ReadAllLines(fileName);
            }

            List<string> temp = new List<string>();
            temp.AddRange(arrLine);
            temp[line_to_add_after] = newText;
            File.WriteAllLines(fileName, temp);
        }

        //adds Tags / Documentation / Arguments
        private static int AddTagsDocumentationArguments(string type, string addString, string fileName, int index)
        {
            if (addString == null)
                addString = "";
            if (!addString.Replace(type, "").Trim().Equals(""))
            {
                index++;
                FileLineAdd(addString, fileName, index);
            }
            return index;
        }

        //adds Keywords
        private static int AddKeyword(List<Keyword> keywordKeywords, string fileName, int index)
        {
            if (keywordKeywords != null)
                foreach (Keyword keywordKeyword in keywordKeywords)
                {
                    //adds test steps
                    index++;
                    FileLineAdd(keywordKeyword.GetKeywordName() + keywordKeyword.ParamsToString(), fileName, index);
                    AddKeywordToRobot(keywordKeyword);
                }
            return index;
        }

        //Add test case / keyword name to robot file
        private static int AddName(string name, string fileName, int index, string tag)
        {
            int tempTagIndex = RobotFileHandler.HasTag(fileName, tag);
            if (tempTagIndex == -1)
            {
                if (tag.Equals("keywords"))
                    FileLineAdd("*** Keywords ***", fileName, index);
                else
                {
                    if (tag.Equals("test cases"))
                    {
                        int tempKeywordsIndex = RobotFileHandler.HasTag(fileName, "keywords");
                        if (tempKeywordsIndex != -1)
                        {
                            FileLineAdd("*** Test Cases ***", fileName, tempKeywordsIndex);
                            index = tempKeywordsIndex;
                        }
                        else
                            FileLineAdd("*** Test Cases ***", fileName, index);
                    }
                }
                index++;
            }else
                if (tempTagIndex + 1 != index)
                {
                    FileLineAdd("", fileName, index);
                    index++;
                }
            FileLineAdd(name, fileName, index);
            return index;
        }

    }
}
