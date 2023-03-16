using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UE4localizationsTool.Controls;

namespace UE4localizationsTool
{
    public partial class FrmFilter : NForm
    {
        public bool UseMatching;
        public bool RegularExpression;
        public bool ReverseMode;
        public List<string> ArrayValues;
        public FrmFilter(Form form)
        {
            InitializeComponent();
            this.Location = new Point(form.Location.X + (form.Width - this.Width) / 2, form.Location.Y + (form.Height - this.Height) / 2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ArrayValues = new List<string>();
            ArrayValues.Add(matchcase.Checked + "|" + regularexpression.Checked + "|" + reversemode.Checked);
            foreach (string val in listBox1.Items)
            {
                ArrayValues.Add(val);
            }

            File.WriteAllLines("FilterValues.txt", ArrayValues.ToArray());
            ArrayValues.RemoveAt(0);
            UseMatching = matchcase.Checked;
            RegularExpression = regularexpression.Checked;
            ReverseMode = reversemode.Checked;
            this.Close();
        }

        private void ClearList_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void RemoveSelected_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            else
            {
                MessageBox.Show("Select value from list", "no selected value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Can't input null value", "Null value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (!listBox1.Items.Contains(textBox1.Text))
                listBox1.Items.Add(textBox1.Text);
            else
            {
                MessageBox.Show($"The Value '{textBox1.Text}' is already in list", "Existed value", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

        }

        private void FrmFilter_Load(object sender, EventArgs e)
        {
            if (File.Exists("FilterValues.txt"))
            {
                listBox1.Items.Clear();
                List<string> FV = new List<string>();
                FV.AddRange(File.ReadAllLines("FilterValues.txt"));
                string[] Controls = FV[0].Split(new char[] { '|' });

                if (Controls.Length == 3)
                {
                    matchcase.Checked = Convert.ToBoolean(Controls[0]);
                    regularexpression.Checked = Convert.ToBoolean(Controls[1]);
                    reversemode.Checked = Convert.ToBoolean(Controls[2]);
                    FV.RemoveAt(0);
                }
                listBox1.Items.AddRange(FV.ToArray());
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void regularexpression_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
