using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileEncoding
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = @"<div class='adam-accordion__component'>
  <md-toolbar class='adam-accordion__toolbar'
              tabindex='-1'
              ng-click='adamAccordion.changeState()'>
    <div class='md-toolbar-tools'>
      <span ng-bind='adamAccordion.title'></span>
      <span flex></span>";
            MessageBox.Show(Regex.IsMatch(richTextBox1.Text, @"\r\n").ToString());
        }
    }
}
