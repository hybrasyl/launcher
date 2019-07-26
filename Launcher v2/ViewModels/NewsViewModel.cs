using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Launcher.Views;
using Hybrasyl.Common;

namespace Launcher.ViewModels
{
    class NewsViewModel : NotifyPropertyChanged
    {
        public List<Views.NewsItem> Content { get; set; }

        public NewsViewModel()
        {
            //this is ugly and i do not like it, will refactor at some point.
            Content = new List<Views.NewsItem>();


            var news = GetNews();
            if (news != null)
            {
                foreach (var item in news)
                {
                    var newsItem = new Views.NewsItem(item.Title, item.Body, item.Author, item.DateCreated);
                    Content.Add(newsItem);
                }
            }
            else
            {
                Content.Add(new Views.NewsItem("News Temporarily Unavailable", "Please bear with us while we work to restore news functionality", "System", DateTime.Now));
            }

            OnPropertyChanged("Content");


        }

        private List<Hybrasyl.Common.NewsItem> GetNews()
        {
            //var client = new HttpClient() {BaseAddress = new Uri("http://build.hybrasyl.com"), DefaultRequestHeaders = { Accept = {new MediaTypeWithQualityHeaderValue("application/json")}}};
            //var response = client.GetAsync("/api/news/getnews").Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    var newsItems = response.Content.ReadAsAsync<BaseResponse<List<Hybrasyl.Common.NewsItem>>>().Result;
            //    return newsItems.Data.ToList();
            //}

            return null;

        }

    }
}
