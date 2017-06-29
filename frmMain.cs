using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Ude;

namespace FileEncoding
{
    public partial class FrmgetFileEncoding : Form
    {
        public FrmgetFileEncoding()
        {
            InitializeComponent();
        }

        private void FrmgetFileEncoding_Load(object sender, EventArgs e)
        {
            //Form1 f = new Form1();
            //f.Show();

            comboBox1.SelectedIndex = 1;
        }

        // regular check nếu bằng true thì không phải kiểu đó
        // ví dụ linux = \r\n|\r nếu check file trả về true thì không phải linux
        public static string[][] endlineType = new string[3][] 
            {   new string []{ "Windows", @"\r[^\n]|[^\r]\n", @"\r\n" },
                new string [] { "Linux", @"\r\n|\r", @"\n"},
                new string [] { "Mac", @"\r\n|\n", @"\r" } };


        private void btnOpen_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fo = new FolderBrowserDialog();

            if (fo.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = fo.SelectedPath;
            }
        }

        List<string> getFileNames(string folderPath, string filters)
        {
            if (!Directory.Exists(folderPath))
                return null;

            List<string> rs = new List<string>();

            foreach (string filter in filters.Split('\n'))
            {
                rs.AddRange(Directory.GetFiles(folderPath, filter));
            }

            foreach (string subfolder in Directory.GetDirectories(folderPath))
            {
                rs.AddRange(getFileNames(subfolder, filters));
            }
            return rs;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> listfile = getFileNames(txtPath.Text, txtFilters.Text);

            lvlListFile.Columns[4].Text = endlineType[comboBox1.SelectedIndex][0];

            lvlListFile.Items.Clear();
            bool BOM = false;
            bool regularContain = false;
            string Charset = "";
            //MessageBox.Show(listfile.Count.ToString());
            foreach (string item in listfile)
            {
                GetEncoding(item, ref BOM, ref regularContain, ref Charset);

                ListViewItem it = new ListViewItem();
                it.Text = Path.GetFileName(item);
                it.SubItems.Add(item);
                it.SubItems.Add(Charset);
                it.SubItems.Add(BOM.ToString());
                it.SubItems.Add(regularContain.ToString());

                lvlListFile.Items.Add(it);
            }
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding GetEncoding(string filename, ref bool BOM, ref bool regularContain, ref string Charset)
        {
            // Read the BOM
            var bom = new byte[4];
            Charset = "";
            //Regex reg = new Regex(@"\r\n");
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
                using (BufferedStream bf = new BufferedStream(file))
                {
                    using (StreamReader sr = new StreamReader(bf))
                    {
                        string alltext = sr.ReadToEnd();

                        // check xem có ký tự ngoài quy định không?
                        // nếu endlineType[comboBox1.SelectedIndex][1] == true thì là có ký tự khác kiểu
                        regularContain = !Regex.IsMatch(alltext, endlineType[comboBox1.SelectedIndex][1]);
                        // vì trả về true là không có ký tự khác kiểu nên sẽ đảo ngược lại

                        
                    }
                }
            }

            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                ICharsetDetector cdet = new CharsetDetector();
                cdet.Feed(file);
                cdet.DataEnd();
                if (cdet.Charset != null)
                {
                    //Console.WriteLine("Charset: {0}, confidence: {1}",
                    //     cdet.Charset, cdet.Confidence);
                    Charset = cdet.Charset;
                    Charset = Charset == "ASCII" ? "UTF-8" : Charset;
                }
            }


            BOM = true;

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;

            BOM = false;
            return Encoding.UTF8;
        }

        #region UnUse

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename, ref bool BOM)
        {
            // Read the BOM
            var bom = new byte[4];
            Regex reg = new Regex(@"[^\r]\n");
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }


            BOM = true;

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;

            BOM = false;
            return Encoding.UTF8;
        }

        static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static Encoding GetFileEncoding(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var encodings = Encoding.GetEncodings()
                .Select(e => e.GetEncoding())
                .Select(e => new { Encoding = e, Preamble = e.GetPreamble() })
                .Where(e => e.Preamble.Any())
                .ToArray();

            var maxPrembleLength = encodings.Max(e => e.Preamble.Length);
            byte[] buffer = new byte[maxPrembleLength];

            using (var stream = File.OpenRead(path))
            {
                stream.Read(buffer, 0, (int)Math.Min(maxPrembleLength, stream.Length));
            }

            return encodings
                .Where(enc => enc.Preamble.SequenceEqual(buffer.Take(enc.Preamble.Length)))
                .Select(enc => enc.Encoding)
                .FirstOrDefault() ?? Encoding.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtPath.Text))
            {
                byte[] data = System.IO.File.ReadAllBytes(txtPath.Text);

                using (var file = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string alltext = sr.ReadToEnd();
                        Console.WriteLine(alltext);
                    }
                }

                Console.WriteLine(Encoding.Unicode.GetString(data));
                Console.WriteLine(Encoding.GetEncoding(932).GetString(data));
                Console.WriteLine(Encoding.UTF8.GetString(data));
                Console.WriteLine(Encoding.UTF7.GetString(data));
                Console.WriteLine(Encoding.ASCII.GetString(data));
            }

        }

        #endregion

        private void lvlListFile_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvlListFile.Sorting == SortOrder.Ascending)
            {
                lvlListFile.Sorting = SortOrder.Descending;
            }
            else
            {
                lvlListFile.Sorting = SortOrder.Ascending;
            }

            ListViewColumnSorter lvcs = new ListViewColumnSorter();
            lvcs.Order = lvlListFile.Sorting;
            lvcs.SortColumn = e.Column;

            this.lvlListFile.ListViewItemSorter = lvcs;

            lvlListFile.Sort();
        }

        private void showInFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvlListFile.SelectedItems.Count == 1)
            {
                System.Diagnostics.Process.Start(Path.GetDirectoryName(lvlListFile.SelectedItems[0].SubItems[1].Text));
            }
        }

        private void lvlListFile_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (lvlListFile.Items.Count > 0)
            {
                using (FileStream fs = new FileStream("export" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".tsv", FileMode.CreateNew, FileAccess.Write))
                {
                    using (BufferedStream bf = new BufferedStream(fs))
                    {
                        using (StreamWriter sw = new StreamWriter(bf))
                        {
                            foreach (ListViewItem item in lvlListFile.Items)
                            {
                                sw.WriteLine(
                                    item.Text + "\t" +
                                    item.SubItems[1].Text + "\t" +
                                    item.SubItems[2].Text + "\t" +
                                    item.SubItems[3].Text + "\t" +
                                    item.SubItems[4].Text + "\t");
                            }
                        }
                    }
                }
                MessageBox.Show("OK");
                System.Diagnostics.Process.Start(Application.StartupPath);
            }
        }

        private void btnFixAnsi_Click(object sender, EventArgs e)
        {
            if (lvlListFile.Items.Count > 0)
            {
                foreach (ListViewItem item in lvlListFile.Items)
                {
                    if (item.SubItems[2].Text != "UTF-8")
                    {
                        byte[] ansiBytes = File.ReadAllBytes(item.SubItems[1].Text);
                        var utf8String = Encoding.UTF8.GetString(ansiBytes);
                        File.WriteAllText(item.SubItems[1].Text, utf8String);
                    }
                }

                MessageBox.Show("OK");
            }
        }

        private void btnFixCRLF_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
