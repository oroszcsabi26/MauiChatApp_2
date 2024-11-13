using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSharedContent.Models
{
    public class MessageModel
    {
        public string? Text { get; set; }

        public ImageSource? ImageSource { get; set; }

        public bool IsImage { get; set; } = false;

        public bool IsOwnMessage { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
