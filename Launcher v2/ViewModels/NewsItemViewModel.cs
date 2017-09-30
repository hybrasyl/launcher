using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.ViewModels
{
    public class NewsItemViewModel : NotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string Created { get; set; }

        public string CombinedTitle { get; set; }
        public string PostedBy { get; set; }
        

        public NewsItemViewModel(string title, string body, string author, DateTime created)
        {
            Title = title;
            Body = body;
            Author = author;
            Created = created.ToShortDateString();
            CombinedTitle = $"{created.ToShortDateString()} - {title}";
            PostedBy = $"Posted by {author}";

            OnPropertyChanged("Title");
            OnPropertyChanged("Body");
            OnPropertyChanged("Author");
            OnPropertyChanged("Created");
            OnPropertyChanged("CombinedTitle");
            OnPropertyChanged("PostedBy");
        }
    }
}
