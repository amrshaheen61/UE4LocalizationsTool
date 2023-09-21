using System;
using System.Drawing;
using System.Windows.Forms;
using UE4localizationsTool.Controls;

namespace UE4localizationsTool
{
    public partial class FrmState : NForm
    {
        private readonly DateTime dateTime;
        private readonly Timer timer1;

        public FrmState()
        {
            InitializeComponent();
            dateTime = DateTime.Now;
            timer1 = new Timer { Interval = 1000 };
            timer1_Tick(null, null);
            timer1.Tick += timer1_Tick; 
            timer1.Start();
        }

        public FrmState(Form form, string title, string state) : this()
        {
            label1.Text = state;
            Text = title;
            Location = new Point(form.Location.X + (form.Width - Width) / 2, form.Location.Y + (form.Height - Height) / 2);
        }

        public FrmState(string title, string state) : this()
        {
            label1.Text = state;
            Text = title;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan dateElapsed = DateTime.Now - dateTime;
            label2.Text = "Elapsed time: " + dateElapsed.ToString("hh':'mm':'ss");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
    
            if (disposing)
            {
                timer1.Stop();
                timer1.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}
