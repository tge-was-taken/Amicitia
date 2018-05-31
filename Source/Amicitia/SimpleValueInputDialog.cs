using System;
using System.Windows.Forms;

namespace Amicitia
{
    public partial class SimpleValueInputDialog : Form
    {
        public string ValueText { get; private set; }

        public SimpleValueInputDialog()
        {
            InitializeComponent();
        }

        public SimpleValueInputDialog( string title, string labelName, string initialValueText ) : this()
        {
            Text = title;
            label1.Text = labelName;
            textBox1.Text = initialValueText;
        }

        private void button1_Click( object sender, EventArgs e )
        {
            ValueText = textBox1.Text;
        }
    }
}
