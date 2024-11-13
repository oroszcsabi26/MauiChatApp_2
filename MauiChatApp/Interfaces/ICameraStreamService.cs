using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Interfaces
{
    public interface ICameraStreamService
    {
        event Action<ImageSource> NewImageReceived; 
        Task StartCameraStream(); 
        Task StopCameraStream(); 
    }
}
