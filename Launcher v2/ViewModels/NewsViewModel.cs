using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Launcher.ViewModels
{
    class NewsViewModel : NotifyPropertyChanged
    {
        public List<TextBlock> Content { get; set; }

        public NewsViewModel()
        {
            //this is ugly and i do not like it, will refactor at some point.
            Content = new List<TextBlock>();
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString("http://www.hybrasyl.com/launcher");

                //let's have some fun parsing out html.. yay
                htmlCode = htmlCode.Substring(htmlCode.IndexOf("<li>"));
                htmlCode = htmlCode.Substring(0, htmlCode.LastIndexOf("</li>"));
                htmlCode = htmlCode.Replace("\n", "");
                htmlCode = htmlCode.Replace("</li>", "");
                string[] splits = { "<li>" };
                var items = htmlCode.Split(splits, StringSplitOptions.RemoveEmptyEntries);
                List<string> cleaned = new List<string>();
                foreach(string s in items)
                {
                    if(s.Contains("<a href") || s.Contains("<a target"))
                    {
                        string news = s.Substring(0, s.IndexOf("<a"));
                        string nourl = s.Substring(s.IndexOf("\">") + 2, s.IndexOf("</a>") - (s.IndexOf("\">") + 2));

                       cleaned.Add(news + nourl);
                    }
                    else
                    {
                        cleaned.Add(s);
                    }
                }

                foreach(var s in cleaned)
                {
                    Content.Add(new TextBlock() { Text = s, Foreground = Brushes.WhiteSmoke, TextWrapping = System.Windows.TextWrapping.Wrap, Margin = new System.Windows.Thickness(25, 5, 5, 5) });
                }
                OnPropertyChanged("Content");
            }

        }
    }
}
