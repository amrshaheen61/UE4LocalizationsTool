using System;
using System.Drawing;
using System.Windows.Forms;

namespace UE4localizationsTool
{
    public partial class FrmState : Form
    {

        private int Timer = 0;

        public FrmState()
        {
            InitializeComponent();
        }
        public FrmState(Form form, string Title, string state)
        {
            InitializeComponent();
            label1.Text = state;
            this.Text = Title;
            this.Location = new Point(form.Location.X + (form.Width - this.Width) / 2, form.Location.Y + (form.Height - this.Height) / 2);
            timer1.Start();
        }




        private void State_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = "Elapsed time:" + TimeSpan.FromSeconds(Timer).ToString("hh':'mm':'ss");
            Timer++;
        }
    }
}
