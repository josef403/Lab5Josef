using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab5Josef
{
    public partial class Form1 : Form
    {
        private string _folderpath;
        private string _htmlcode;
        private string[] _imageArray;
        public Form1()
        {
            InitializeComponent();
        }

        //Gör så att du kan trycka enter istället för att klicka på knappen
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //Kollar så att textboxen inte är tom när man extractar
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    MessageBox.Show("Enter a url.");
                    return;
                }
                buttonExtract_Click(this, new EventArgs());
            }
        }
        private async void buttonExtract_Click(object sender, EventArgs e)
        {
            //Kollar så att textboxen inte är tom när man extractar
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Enter a url.");
                return;
            }
            textBox2.Clear();

            await ImageChecker(textBox1.Text);
            string pattern = @"(?<=src=""\/image)(.*?)(?="")";
            Regex rgx = new Regex(pattern);
            var results = rgx.Matches(_htmlcode);
            //Skapar en bild array med antalet matchningar den hittar
            _imageArray = new string[results.Count];
            int countedImages = 0;

            //Lägger till alla urls för bilderna i textboxen och spara de i en array
            foreach (Match match in results)
            {
                textBox2.Text += "https://gp.se/image" + match + Environment.NewLine;
                _imageArray[countedImages] = "https://gp.se/image" + match.Value;
                countedImages++;
            }

            //Räknar hur många bilder den hittade hos gp.se
            label1.Text = $"Found {countedImages.ToString()} images.";
            //Gör Save knappen bara synlig ifall man klickat på extract
            buttonSave.Visible = true;
        }

        public async Task ImageChecker(string url)
        {
            /*gör det simplare att skriva in en url
            genom att lägga till http:// om det inte finns */

            string urlStart = @"https://";
            Regex rgx = new Regex(urlStart);
            var urlcheck = rgx.Matches(url);

            if (urlcheck.Count == 0)
            {
                url = @"https://" + url;
            }

            var client = new HttpClient();

            Task<string> website = client.GetStringAsync(url);
            await website;
            _htmlcode = website.Result;
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _folderpath = fbd.SelectedPath;
                await DownloadImages();
            }
        }

        public async Task DownloadImages()
        {
            var httpClient = new HttpClient();

            for (int i = 0; i < _imageArray.Length; i++)
            {
                byte[] imageBytes = await httpClient.GetByteArrayAsync(_imageArray[i]);
                string format = string.Empty;
                switch (_imageArray[i])
                {
                    case string a when a.Contains(".jpg"):
                        format = ".jpg";
                        break;
                    case string b when b.Contains(".png"):
                        format = ".png";
                        break;
                    case string c when c.Contains(".gif"):
                        format = ".gif";
                        break;
                    case string d when d.Contains(".bmp"):
                        format = ".bmp";
                        break;
                    case string e when e.Contains(".jpeg"):
                        format = ".jpeg";
                        break;
                }

                //Skapar bilden i  din valda map.
                File.WriteAllBytes(_folderpath + $"/image{i + 1}{format}", imageBytes);
                //Räknar antalet nedladdningar asynchronous.
                label1.Text = $"Downloaded {i + 1} out of {_imageArray.Length} images.";
            }
        }
    }
}
