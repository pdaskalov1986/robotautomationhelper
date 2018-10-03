﻿using RobotAutomationHelper.Scripts;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RobotAutomationHelper.Forms
{
    public partial class ParamAddForm : Form
    {
        private bool skip = false;
        private Keyword keyword;
        private int paramsCount = 0;

        internal bool SkipValue()
        {
            return skip;
        }

        public ParamAddForm()
        {
            InitializeComponent();
        }

        internal void ShowParamContent(Keyword keyword)
        {
            this.keyword = keyword;

            List<string> args = new List<string>();
            if (keyword.GetKeywordArguments() != null)
                args.AddRange(keyword.GetKeywordArguments().Replace("[Arguments]", "").Trim().Split(' '));

            List<Param> paramsList = new List<Param>();
            if (keyword.GetKeywordParams() != null)
                paramsList.AddRange(keyword.GetKeywordParams());

            if (keyword.GetKeywordName() != null)
                KeywordName.Text = keyword.GetKeywordName().Trim();
            if (keyword.GetKeywordDocumentation() != null)
                KeywordDocumentation.Text = keyword.GetKeywordDocumentation().Replace("[Documentation]", "").Trim();

            if (args != null)
                for (int i = 0; i < args.Count; i++)
                    if (args[i].Equals(""))
                        args.RemoveAt(i);

            if (args != null && args.Count != 0)
                foreach (string arg in args)
                {
                    paramsCount++;
                    FormControls.AddControl("Label", "DynamicTestStep" + paramsCount + "Name",
                        new System.Drawing.Point(10 - this.HorizontalScroll.Value, 123 + (paramsCount - 1) * 30 - this.VerticalScroll.Value),
                        new System.Drawing.Size(80, 20),
                        arg,
                        System.Drawing.Color.Black,
                        null,
                        this);
                    FormControls.AddControl("TextBox", "DynamicTestStep" + paramsCount + "Value",
                        new System.Drawing.Point(100 - this.HorizontalScroll.Value, 120 + (paramsCount - 1) * 30 - this.VerticalScroll.Value),
                        new System.Drawing.Size(280, 20),
                        paramsList[paramsCount - 1].GetParamValue(),
                        System.Drawing.Color.Black,
                        null,
                        this);
                }

            var dialogResult = this.ShowDialog();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            List<Param> formParams = new List<Param>();
            for (int i = 1; i <= paramsCount; i++)
                formParams.Add(new Param(this.Controls["DynamicTestStep" + i + "Name"].Text, this.Controls["DynamicTestStep" + i + "Value"].Text));

            keyword.SetKeywordParams(formParams);
            this.Close();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            skip = true;
            this.Close();
        }
    }
}
