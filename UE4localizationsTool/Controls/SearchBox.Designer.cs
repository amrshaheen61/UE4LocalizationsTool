using System;
using System.Windows.Forms;

namespace UE4localizationsTool.Controls
{
    partial class SearchBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.searchcount = new System.Windows.Forms.Label();
            this.SearchHide = new System.Windows.Forms.Label();
            this.FindPrevious = new System.Windows.Forms.Button();
            this.FindNext = new System.Windows.Forms.Button();
            this.InputSearch = new UE4localizationsTool.Controls.NTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // searchcount
            // 
            this.searchcount.Location = new System.Drawing.Point(423, 7);
            this.searchcount.Name = "searchcount";
            this.searchcount.Size = new System.Drawing.Size(211, 18);
            this.searchcount.TabIndex = 6;
            // 
            // SearchHide
            // 
            this.SearchHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchHide.AutoSize = true;
            this.SearchHide.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SearchHide.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.SearchHide.Location = new System.Drawing.Point(665, 7);
            this.SearchHide.Name = "SearchHide";
            this.SearchHide.Size = new System.Drawing.Size(18, 17);
            this.SearchHide.TabIndex = 5;
            this.SearchHide.Text = "X";
            this.SearchHide.Click += new System.EventHandler(this.SearchHide_Click);
            // 
            // FindPrevious
            // 
            this.FindPrevious.Location = new System.Drawing.Point(331, 4);
            this.FindPrevious.Name = "FindPrevious";
            this.FindPrevious.Size = new System.Drawing.Size(86, 23);
            this.FindPrevious.TabIndex = 4;
            this.FindPrevious.Text = "Find Previous";
            this.FindPrevious.UseVisualStyleBackColor = true;
            this.FindPrevious.Click += new System.EventHandler(this.FindPrevious_Click);
            // 
            // FindNext
            // 
            this.FindNext.Location = new System.Drawing.Point(250, 4);
            this.FindNext.Name = "FindNext";
            this.FindNext.Size = new System.Drawing.Size(75, 23);
            this.FindNext.TabIndex = 3;
            this.FindNext.Text = "Find Next";
            this.FindNext.UseVisualStyleBackColor = true;
            this.FindNext.Click += new System.EventHandler(this.FindNext_Click);
            // 
            // InputSearch
            // 
            this.InputSearch.Location = new System.Drawing.Point(79, 5);
            this.InputSearch.Name = "InputSearch";
            this.InputSearch.PlaceholderText = "Type your search here...";
            this.InputSearch.Size = new System.Drawing.Size(162, 20);
            this.InputSearch.TabIndex = 0;
            this.InputSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputSearch_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Find what:";
            // 
            // SearchBox
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.searchcount);
            this.Controls.Add(this.SearchHide);
            this.Controls.Add(this.FindPrevious);
            this.Controls.Add(this.FindNext);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InputSearch);
            this.Location = new System.Drawing.Point(155, 23);
            this.Name = "SearchBox";
            this.Size = new System.Drawing.Size(689, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label searchcount;
        private System.Windows.Forms.Label SearchHide;
        private System.Windows.Forms.Button FindPrevious;
        private System.Windows.Forms.Button FindNext;

        private System.Windows.Forms.Label label1;
        public NTextBox InputSearch;
    }
}
