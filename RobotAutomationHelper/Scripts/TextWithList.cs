﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RobotAutomationHelper.Scripts
{
    internal class TextWithList : TextBox
    {

        private SuggestionsList SuggestionsList;
        private Control ParentControl;
        private bool ForcedFocusToList { get; set; }

        internal TextWithList(Control Parent)
        {
            ParentControl = Parent;
            SuggestionsList = new SuggestionsList(this);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            string txt = Text;

            // find all the items in suggestion that match the current text
            List<SuggestionsListObjects> foundItems = ReturnSuggestionsMatches(txt);

            if (foundItems.Count > 0)
            {
                //show suggestions list
                SuggestionsList.Items.Clear();
                SuggestionsList.Items.AddRange(foundItems.ToArray());
                SuggestionsList.Visible = true;
                SuggestionsList.Location = new Point(Location.X, Location.Y + 20);
                SuggestionsList.Size = new Size(Size.Width, foundItems.Count * SuggestionsList.ItemHeight > 200 ? 200 : (foundItems.Count + 1) * SuggestionsList.ItemHeight);
                SuggestionsList.IntegralHeight = true;
                ParentControl.Controls.Add(SuggestionsList);
                SuggestionsList.BringToFront();
                return;
            }
            else
            {
                //hide suggestions list
                HideSuggestionsList();
            }

            // if an item is selected from the list, then TriggerUpdate
            if (SuggestionsList.SelectionPerformed)
            {
                SuggestionsList.SelectionPerformed = false;
                TriggerUpdate();
            }
        }

        //on Enter triggers update and hides suggestions
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter && e.KeyCode == Keys.Return)
            {
                HideSuggestionsList();
                TriggerUpdate();
            }
            else
            {
                if (e.KeyCode == Keys.Down && Parent.Controls.Find("SuggestionsList", false).Length > 0)
                {
                    ForcedFocusToList = true;
                    SuggestionsList.SelectedIndex = 0;
                    Parent.Controls["SuggestionsList"].Focus();
                }
                else
                {
                    if (e.KeyCode == Keys.Up && Parent.Controls.Find("SuggestionsList", false).Length > 0)
                    {
                        ForcedFocusToList = true;
                        SuggestionsList.SelectedIndex = SuggestionsList.Items.Count - 1;
                        Parent.Controls["SuggestionsList"].Focus();
                    }
                }
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (!ForcedFocusToList)
                HideSuggestionsList();
            ForcedFocusToList = false;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            OnTextChanged(e);
        }

        private List<SuggestionsListObjects> ReturnSuggestionsMatches(string txt)
        {
            List<SuggestionsListObjects> foundItems = new List<SuggestionsListObjects>();
            foreach (Keyword keyword in FormControls.Suggestions)
                if (!keyword.GetKeywordName().ToLower().Trim().Equals(txt.ToLower().Trim()))
                {
                    bool containsAll = true;
                    foreach (string temp in txt.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (RobotAutomationHelper.Log) Console.WriteLine(keyword.ToString());
                        if (!keyword.GetKeywordName().ToLower().Contains(temp))
                        {
                            containsAll = false;
                            break;
                        }
                    }
                    if (containsAll)
                    {
                        if (keyword.Type != KeywordType.CUSTOM)
                            foundItems.Add(new SuggestionsListObjects { Text = keyword.ToString(), ValueMember = keyword.GetKeywordName(), Documentation = keyword.GetKeywordDocumentation() });
                        else
                            foundItems.Add(new SuggestionsListObjects { Text = keyword.ToString().Trim(), ValueMember = keyword.GetKeywordName().Trim(), Documentation = keyword.GetOutputFilePath() + "\n" + keyword.GetKeywordDocumentation().Trim() });
                    }
                }
            return foundItems;
        }

        private void TriggerUpdate()
        {
            bool isKeywordAddForm = (Parent as Form).Name.Contains("Keyword");

            List<Keyword> Keywords = new List<Keyword>();
            if (isKeywordAddForm)
                Keywords = (Parent as KeywordAddForm).ThisFormKeywords;
            else
                Keywords = (Parent as TestCaseAddForm).Keywords;

            BaseKeywordAddForm.ChangeTheKeywordFieldOnUpdate(this, (Parent as Form), isKeywordAddForm, Keywords);
        }

        internal void HideSuggestionsList()
        {
            SuggestionsList.Visible = false;
            SuggestionsList.HideToolTip();
            ParentControl.Controls.Remove(SuggestionsList);
        }
    }
}
