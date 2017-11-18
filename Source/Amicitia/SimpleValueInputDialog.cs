using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
