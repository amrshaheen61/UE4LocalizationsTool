using System;
using System.Drawing;
using System.Windows.Forms;
using UE4localizationsTool.Controls;

namespace UE4localizationsTool
{
    public partial class FrmState : NForm
    {

        DateTime dateTime;

        public FrmState()
        {
            InitializeComponent();
        }
        public FrmState(Form form, string Title, string state)
        {
            dateTime = DateTime.Now;
            InitializeComponent();
            label1.Text = state;
            this.Text = Title;
            this.Location = new Point(form.Location.X + (form.Width - this.Width) / 2, form.Location.Y + (form.Height - this.Height) / 2);
            timer1.Start();

        }

        public FrmState(string Title, string state)
        {
            InitializeComponent();
            label1.Text = state;
            this.Text = Title;
            timer1.Start();
            timer1_Tick(null, null);
        }


        private void State_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            TimeSpan DateElapsed = DateTime.Now - dateTime;

            label2.Text = "Elapsed time:" + DateElapsed.ToString("hh':'mm':'ss");
        }
    }
}
