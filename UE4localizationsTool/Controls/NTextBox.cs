using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UE4localizationsTool.Controls
{
    public partial class NTextBox : TextBox
    {

        private string _placeholderText = string.Empty;
        [Browsable(true)]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                UpdatePlaceholderText();
            }
        }
        [Browsable(true)]
        public bool StopEnterKey { get; set; } = false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdatePlaceholderText();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdatePlaceholderText();
        }

        private void UpdatePlaceholderText()
        {
            if (IsHandleCreated && string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(_placeholderText))
            {
                SendMessage(this.Handle, EM_SETCUEBANNER, 0, _placeholderText);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (Multiline && StopEnterKey && e.KeyChar == '\r' || StopEnterKey && e.KeyChar == '\n')
            {
                e.Handled = true;
            }


            base.OnKeyPress(e);
        }


        private const int EM_SETCUEBANNER = 0x1501;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);
    }
}
