using Character_Database.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Character_Database
{
    public partial class MainForm : Form
    {
        private List<ListViewItem> hiddenItems = new List<ListViewItem>();
        private Settings settings;

        public MainForm()
        {
            InitializeComponent();

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Character Database");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filename = Path.Combine(folder, "settings.xml");
            this.settings = new Settings(filename);

            this.checkBox1.Checked = this.settings.SearchCaseSensitive;
            this.checkBox2.Checked = this.settings.SearchWhenTyping;
            this.splitContainer1.SplitterDistance = this.settings.MainSidebarWidth;
        }

        private static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.White);
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

        private void neuerCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView2.Items.Add("Neuer Character");
            item.Tag = new Character("Neuer Character");

            listView2.SelectedItems.Clear();
            item.Selected = true;
            listView2_DoubleClick(sender, e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Character Database");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filename = Path.Combine(folder, "database.xml");
            if (!File.Exists(filename)) return;

            XmlLoader loader = new XmlLoader();
            var characters = loader.Read(filename);

            int imageIndex = 0;
            foreach (var character in characters)
            {
                var item = listView2.Items.Add(character.Name);
                item.Tag = character;

                if (File.Exists(character.Picture))
                {
                    imageList1.Images.Add(FixedSize(Image.FromFile(character.Picture), imageList1.ImageSize.Width, imageList1.ImageSize.Height));
                    item.ImageIndex = imageIndex++;
                }
            }
        }

        private void characterLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                if (listView2.SelectedItems.Count == 1 &&
                    MessageBox.Show("Willst du " + listView2.SelectedItems[0].Text + " wirklich löschen?", "Löschen",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                if (listView2.SelectedItems.Count > 1 &&
                    MessageBox.Show("Willst du diese " + listView2.SelectedItems.Count + " Charaktere wirklich löschen?", "Löschen",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                foreach (ListViewItem item in listView2.SelectedItems)
                {
                    listView2.Items.Remove(item);
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            characterLöschenToolStripMenuItem.Enabled = listView2.SelectedItems.Count > 0;
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;

            var item = listView2.SelectedItems[0];
            var cform = new CharacterForm();
            cform.Character = (Character)(item.Tag);
            cform.FormClosed += new FormClosedEventHandler((obj, args) =>
            {
                item.Text = (item.Tag as Character).Name;
                listView2.Sort();
                if (File.Exists((item.Tag as Character).Picture))
                {
                    if (item.ImageIndex >= 0)
                    {
                        imageList1.Images[item.ImageIndex] = FixedSize(Image.FromFile((item.Tag as Character).Picture), imageList1.ImageSize.Width, imageList1.ImageSize.Height);
                    }
                    else
                    {
                        imageList1.Images.Add(FixedSize(Image.FromFile((item.Tag as Character).Picture), imageList1.ImageSize.Width, imageList1.ImageSize.Height));
                        item.ImageIndex = imageList1.Images.Count - 1;
                    }
                }
            });

            cform.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseFilter();
            var characters = new List<Character>();
            foreach (ListViewItem item in listView2.Items)
            {
                characters.Add((Character)item.Tag);
            }

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Character Database");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filename = Path.Combine(folder, "database.xml");
            XmlLoader loader = new XmlLoader();
            loader.Write(filename, characters);
        }

        private void FilterCharacters(string filter)
        {
            ReleaseFilter();
            listView2.BeginUpdate();

            string[] tags = filter.Split(' ');
            foreach (ListViewItem item in listView2.Items)
            {
                Character c = (Character)item.Tag;

                foreach (string tag in tags)
                {
                    if (tag.StartsWith("-"))
                    {
                        var ctag = tag.Substring(1);
                        if (c.Tags.IndexOf(ctag, checkBox1.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            hiddenItems.Add(item);
                            listView2.Items.Remove(item);
                            break;
                        }
                    }
                    else
                    {
                        if (c.Tags.IndexOf(tag, checkBox1.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            this.hiddenItems.Add(item);
                            listView2.Items.Remove(item);
                            break;
                        }
                    }

                }
            }

            listView2.EndUpdate();
        }

        private void ReleaseFilter()
        {
            listView2.BeginUpdate();
            foreach (ListViewItem item in hiddenItems)
            {
                listView2.Items.Add(item);
            }
            listView2.Sort();
            listView2.EndUpdate();
            hiddenItems.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FilterCharacters(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            ReleaseFilter();
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listView2_DoubleClick(sender, e);
            }
            if (e.KeyCode == Keys.Delete)
            {
                characterLöschenToolStripMenuItem_Click(sender, e);
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (textBox1.Text == String.Empty)
                    ReleaseFilter();
                else
                    FilterCharacters(textBox1.Text);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = !checkBox2.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var characters = new List<Character>();
            foreach (ListViewItem item in listView2.Items)
            {
                characters.Add((Character)item.Tag);
            }
            foreach (ListViewItem item in hiddenItems)
            {
                characters.Add((Character)item.Tag);
            }

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Character Database");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filename = Path.Combine(folder, "database.xml");
            XmlLoader loader = new XmlLoader();
            loader.Write(filename, characters);
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            this.settings.SearchWhenTyping = checkBox2.Checked;
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            this.settings.SearchCaseSensitive = checkBox1.Checked;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.settings.MainSidebarWidth = this.splitContainer1.SplitterDistance;
        }
    }
}
