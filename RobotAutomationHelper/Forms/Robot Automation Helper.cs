﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RobotAutomationHelper.Scripts.Objects;
using RobotAutomationHelper.Scripts.Static;
using RobotAutomationHelper.Scripts.Static.Readers;
using RobotAutomationHelper.Scripts.Static.Writers;

namespace RobotAutomationHelper.Forms
{
    internal partial class RobotAutomationHelper : BaseKeywordAddForm
    {
        internal static List<TestCase> TestCases = new List<TestCase>();
        internal static List<SuiteSettings> SuiteSettingsList = new List<SuiteSettings>();
        private static int _numberOfTestCases;
        private object _realSender;

        internal static bool Log = false;
        // index of the test case to be implemented
        private int _indexOfTheTestCaseToBeImplemented;

        internal RobotAutomationHelper(BaseKeywordAddForm parent) : base(parent)
        {
            InitializeComponent();
            ActiveControl = TestCaseNameLabel;
        }

        private void ApplicationMain_Load(object sender, EventArgs e)
        {

        }

        // open file click
        private void ToolStripMenuOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void OpenExistingProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                BrowseFolderButtonExistingProject();
            }
        }

        private void NewProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog3.ShowDialog() == DialogResult.OK)
            {
                BrowseFolderButtonNewProject();
            }
        }

        // browse folders for output directory after file has been opened
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                BrowseFolderButtonOpenExcel();
            }
        }

        private void BrowseFolderButtonNewProject()
        {
            FormSetup();
            TestCases = new List<TestCase>();
            CheckForExistingCodeAndShowAlert();
            AddTestCaseToFormAndShow();
        }

        private void BrowseFolderButtonOpenExcel()
        {
            FormSetup();
            TestCases = ReadExcel.ReadAllTestCasesFromExcel(openFileDialog.FileName);
            TestCases.Sort();
            CheckForExistingCodeAndShowAlert();
            AddTestCaseToFormAndShow();
        }

        private void BrowseFolderButtonExistingProject()
        {
            FormSetup();

            TestCases = ReadRobotFiles.ReadAllTests();
            if (TestCases.Count != 0)
            {
                var suiteSettingsKeywords = ReadRobotFiles.ReadAllSettings();

                if (suiteSettingsKeywords.Count != 0)
                {
                    foreach (var tempKeyword in suiteSettingsKeywords)
                    {
                        KeywordToSuggestions(tempKeyword);
                    }
                }

                foreach (var testCase in TestCases)
                {
                    if (testCase.Steps == null) continue;
                    foreach (var tempKeyword in testCase.Steps)
                    {
                        KeywordToSuggestions(tempKeyword);
                    }
                }

                TestCases.Sort();

                SuggestionsClass.UpdateSuggestionsToIncludes(TestCases, suiteSettingsKeywords);
                AddTestCaseToFormAndShow();
            }
            else
            {
                MessageBox.Show(@"No test cases in the selected folder!",
                    @"Alert",
                    MessageBoxButtons.OK);
            }
        }

        internal void AddTestCaseToFormAndShow()
        {
            AddTestCasesToMainForm();
            ShowTestCasePanels();
        }

        internal void FormSetup()
        {
            Cache.ClearCache();
            SuggestionsClass.PopulateSuggestionsList();
            ClearDynamicElements();
            settingsToolStripMenuItem.Visible = true;
            librariesToolStripMenuItem.Visible = true;
            SetStructureFolder(folderBrowserDialog2.SelectedPath);
        }

        internal void CheckForExistingCodeAndShowAlert()
        {
            var projectTestCases = ReadRobotFiles.ReadAllTests();
            var suiteSettingsKeywordList = ReadRobotFiles.ReadAllSettings();

            if (projectTestCases.Count == 0) return;
            var result = MessageBox.Show(@"Use existing Test Cases in project folder?",
                @"Alert",
                MessageBoxButtons.YesNo);
            if (!result.Equals(DialogResult.Yes)) return;
            TestCases = projectTestCases;

            foreach (var testCase in projectTestCases)
            {
                if (testCase.Steps == null) continue;
                foreach (var keyword in testCase.Steps)
                    KeywordToSuggestions(keyword);
            }

            if (suiteSettingsKeywordList.Count != 0) return;
            foreach (var tempKeyword in suiteSettingsKeywordList)
                KeywordToSuggestions(tempKeyword);

            SuggestionsClass.UpdateSuggestionsToIncludes(TestCases, suiteSettingsKeywordList);
        }

        private static void KeywordToSuggestions(Keyword tempKeyword)
        {
            if (tempKeyword.SuggestionIndex == -1 && !tempKeyword.OutputFilePath.Equals("") && !StringAndListOperations.StartsWithVariable(tempKeyword.Name))
            {
                var toAdd = true;
                foreach (var lib in SuggestionsClass.Suggestions)
                    if (lib.ToInclude)
                        foreach (var suggested in lib.LibKeywords)
                        {
                            if (suggested.OutputFilePath.Equals("")) continue;
                            if (!suggested.Name.Equals(tempKeyword.Name) ||
                                !suggested.OutputFilePath.Equals(tempKeyword.OutputFilePath)) continue;
                            toAdd = false;
                            break;
                        }
                        if (toAdd)
                        {
                            tempKeyword.SuggestionIndex = SuggestionsClass.GetCustomLibKeywords().Count;
                            SuggestionsClass.GetCustomLibKeywords().Add(tempKeyword);
                        }
            }
            if (tempKeyword.Keywords != null)
                foreach (var nestedKeyword in tempKeyword.Keywords)
                {
                    KeywordToSuggestions(nestedKeyword);
                }

            if (tempKeyword.ForLoopKeywords == null) return;
            {
                foreach (var nestedKeyword in tempKeyword.ForLoopKeywords)
                {
                    KeywordToSuggestions(nestedKeyword);
                }
            }
        }

        //Clear dynamic elements when new file is opened
        private void ClearDynamicElements()
        {
            var cleared = false;
            while (!cleared)
            {
                foreach (Control tempControl in Controls)
                    if (tempControl.Name.ToLower().StartsWith("dynamictest"))
                        FormControls.RemoveControlByKey(tempControl.Name, Controls);

                cleared = true;
                foreach (Control tempControl in Controls)
                {
                    if (!tempControl.Name.ToLower().StartsWith("dynamictest")) continue;
                    cleared = false;
                    break;
                }
            }
        }

        private void AddTestCasesToMainForm()
        {
            _numberOfTestCases = TestCases.Count;
            var testCasesCounter = 1;
            if (TestCases != null && TestCases.Count != 0)
                foreach (var testCase in TestCases)
                {
                    AddTestCaseField(testCase, testCasesCounter);
                    testCasesCounter++;
                }
            else
            {
                TestCases.Add(new TestCase("New Test Case", FilesAndFolderStructure.GetFolder(FolderType.Tests) + "Auto.robot"));
                AddTestCaseField(TestCases[0], testCasesCounter);
                _numberOfTestCases = 1;
                FilesAndFolderStructure.AddFileToSavedFiles(TestCases[0].OutputFilePath);
            }
        }

        private void AddTestCaseField(TestCase testCase, int testCasesCounter)
        {
            FormControls.AddControl("TextBox", "DynamicTest" + testCasesCounter + "Name",
                testCasesCounter,
                new Point(30 - HorizontalScroll.Value, 50 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(280, 20),
                testCase.Name.Trim(),
                Color.Black,
                null,
                this);
            FormControls.AddControl("Label", "DynamicTest" + testCasesCounter + "Label",
                testCasesCounter,
                new Point(10 - HorizontalScroll.Value, 53 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(20, 20),
                testCasesCounter + ".",
                Color.Black,
                null,
                this);
            FormControls.AddControl("CheckBox", "DynamicTest" + testCasesCounter + "CheckBox",
                testCasesCounter,
                new Point(325 - HorizontalScroll.Value, 50 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(20, 20),
                "Add",
                Color.Black,
                null,
                this);

            var implementationText = TestCases[testCasesCounter - 1].Implemented? "Edit Implementation" : "Add Implementation";
            FormControls.AddControl("Button", "DynamicTest" + testCasesCounter + "AddImplementation",
                testCasesCounter,
                new Point(345 - HorizontalScroll.Value, 50 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(120, 20),
                implementationText,
                Color.Black,
                InstantiateAddTestCaseForm,
                this);

            FormControls.AddControl("Button", "DynamicTest" + testCasesCounter + "AddTestCase",
                testCasesCounter,
                new Point(470 - HorizontalScroll.Value, 50 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(20, 20),
                "+",
                Color.Black,
                InstantiateNameAndOutputForm,
                this);
            FormControls.AddControl("Button", "DynamicTest" + testCasesCounter + "RemoveTestCase",
                testCasesCounter,
                new Point(490 - HorizontalScroll.Value, 50 + (testCasesCounter - 1) * 25 - VerticalScroll.Value),
                new Size(20, 20),
                "-",
                Color.Black,
                RemoveTestCaseFromProject,
                this);
        }

        private static void SetStructureFolder(string outputFolder)
        {
            if (!outputFolder.EndsWith("\\"))
                outputFolder = outputFolder + "\\";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            FilesAndFolderStructure.SetFolder(outputFolder);

            FilesAndFolderStructure.FindAllRobotFilesAndAddToStructure();
        }

        protected void InstantiateAddTestCaseForm(object sender, EventArgs e)
        {
            var testIndex = int.Parse(((Button)sender).Name.Replace("AddImplementation", "").Replace("DynamicTest", ""));
            _indexOfTheTestCaseToBeImplemented = testIndex;
            var testCase = TestCases[testIndex - 1];
            testCase.Name = Controls["DynamicTest" + testIndex + "Name"].Text;
            var testCaseAddForm = new TestCaseAddForm(this);
            testCaseAddForm.FormClosing += UpdateThisFormTestCaseAddFormClosing;
            testCaseAddForm.ShowTestCaseContent(testCase, testIndex - 1);
        }

        private void UpdateThisFormTestCaseAddFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!((TestCaseAddForm) sender).SkipForm)
            {
                Controls["DynamicTest" + _indexOfTheTestCaseToBeImplemented + "Name"].Text = TestCases[_indexOfTheTestCaseToBeImplemented - 1].Name;
                Controls["DynamicTest" + _indexOfTheTestCaseToBeImplemented + "AddImplementation"].Text = TestCases[_indexOfTheTestCaseToBeImplemented - 1].Implemented ? "Edit implementation" : "Add implementation";
            }

            //Adds file path + name to the Files And Folder structure for use in the drop down lists when choosing output file
            FilesAndFolderStructure.AddImplementedTestCasesFilesToSavedFiles(TestCases, _indexOfTheTestCaseToBeImplemented);
        }

        internal void ShowTestCasePanels()
        {
            IndexLabel.Visible = true;
            TestCaseNameLabel.Visible = true;
            AddLabel.Visible = true;
        }

        private void SaveToRobotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteToRobot.Includes = new List<Includes>();
            //Cleanup
            FilesAndFolderStructure.DeleteAllFiles();

            TestCases.Sort();
            foreach (var testCase in TestCases)
                WriteToRobot.AddTestCaseToRobot(testCase);

            Console.WriteLine(@"WriteSuiteSettingsListToRobot ===============================");
            WriteToRobot.WriteSuiteSettingsListToRobot();
            Console.WriteLine(@"WriteIncludesToRobotFiles ===================================");
            WriteToRobot.WriteIncludesToRobotFiles();

            foreach (var fileName in FilesAndFolderStructure.GetShortSavedFiles(FolderType.Root))
                RobotFileHandler.TrimFile(FilesAndFolderStructure.ConcatFileNameToFolder(fileName, FolderType.Root));
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FilesAndFolderStructure.GetShortSavedFiles(FolderType.Root) != null && FilesAndFolderStructure.GetShortSavedFiles(FolderType.Root).Count > 0)
                InstantiateSettingsAddForm(sender, e);
            else
            {
                MessageBox.Show(@"You haven't saved any keywords or test cases to files yet.",
                    @"Alert",
                    MessageBoxButtons.OK);
            }
        }

        private void SuggestionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SuggestionsClass.Suggestions != null && SuggestionsClass.Suggestions.Count > 0)
                InstantiateLibrariesAddForm(sender, e);
            else
            {
                MessageBox.Show(@"No libraries loaded.",
                    @"Alert",
                    MessageBoxButtons.OK);
            }
        }

        private static void RunCom(string command)
        {
            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/c" + command,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false,
                    UseShellExecute = false
                }
            };
            cmd.Start();
        }

        private void SaveToRobotAndRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToRobotToolStripMenuItem_Click(sender, e);
            RunCom("cd " + FilesAndFolderStructure.GetFolder(FolderType.Root) + "&robot tests");
        }

        internal new void InstantiateNameAndOutputForm(object sender, EventArgs e)
        {
            _realSender = sender;
            if (Log) Console.WriteLine(@"InstantiateParamsAddForm " + ((Button)sender).Name);
            var formType = Name.Contains("RobotAutomationHelper") ? FormType.Test : FormType.Keyword;

            var nameAndOutputForm = new NameAndOutputForm(formType, this, null);
            nameAndOutputForm.FormClosing += UpdateAfterClosingNameAndOutputForm;
            nameAndOutputForm.ShowTestCaseContent();
        }

        private void UpdateAfterClosingNameAndOutputForm(object sender, EventArgs e)
        {
            if (NameAndOutputToTestCaseFormCommunication.Save)
                AddTestCaseToProject(_realSender, e);
        }

        internal void AddTestCaseToProject(object sender, EventArgs e)
        {
            var testCaseIndex = int.Parse(((Button)sender).Name.Replace("DynamicTest", "").Replace("AddTestCase", ""));

            AssignThisTestCasesNamesFromTextFields();

            TestCases.Add(new TestCase("New Test Case", FilesAndFolderStructure.GetFolder(FolderType.Tests) + "Auto.robot"));

            for (var i = _numberOfTestCases; i > testCaseIndex; i--)
                TestCases[i] = TestCases[i - 1];

            TestCases[testCaseIndex] = new TestCase(NameAndOutputToTestCaseFormCommunication.Name, NameAndOutputToTestCaseFormCommunication.OutputFile);
            _numberOfTestCases++;
            AddTestCaseField(TestCases[_numberOfTestCases - 1], _numberOfTestCases);

            for (var i = 1; i < _numberOfTestCases; i++)
                Controls["DynamicTest" + i + "Name"].Text = TestCases[i - 1].Name.Trim();
        }

        internal void RemoveTestCaseFromProject(object sender, EventArgs e)
        {
            AssignThisTestCasesNamesFromTextFields();

            if (_numberOfTestCases <= 1) return;
            var testCaseIndex = int.Parse(((Button)sender).Name.Replace("DynamicTest", "").Replace("RemoveTestCase", ""));
            RemoveTestCaseField(_numberOfTestCases, false);
            TestCases.RemoveAt(testCaseIndex - 1);
            _numberOfTestCases--;
            for (var i = 1; i <= _numberOfTestCases; i++)
                Controls["DynamicTest" + i + "Name"].Text = TestCases[i - 1].Name.Trim();
        }

        private void AssignThisTestCasesNamesFromTextFields()
        {
            for (var i = 1; i <= _numberOfTestCases; i++)
                if (Controls.Find("DynamicTest" + i + "Name", false).Length != 0)
                    TestCases[i - 1].Name = Controls["DynamicTest" + i + "Name"].Text;
        }

        // Removes TextBox / Label / Add implementation / Add and remove keyword / Params
        private void RemoveTestCaseField(int testCaseIndex, bool removeFromList)
        {
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "Name", Controls);
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "Label", Controls);
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "AddImplementation", Controls);
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "CheckBox", Controls);
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "AddTestCase", Controls);
            FormControls.RemoveControlByKey("DynamicTest" + testCaseIndex + "RemoveTestCase", Controls);
            if (removeFromList)
                TestCases.RemoveAt(testCaseIndex - 1);
        }
    }
}