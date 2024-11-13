using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MauiChatApp.Interfaces;
using CommunityToolkit.Maui.Views;

namespace MauiChatApp.Services
{
    public class CameraStreamService : ICameraStreamService
    {
        public event Action<ImageSource> NewImageReceived;
        private bool m_isStreaming = false;

        private CameraView m_cameraView;

        public CameraStreamService(CameraView p_cameraView)
        {
            // Add camera view to the service
            m_cameraView = p_cameraView; 
        }

        public async Task StartCameraStream()
        {
            m_isStreaming = true;
            m_cameraView.ImageCaptureResolution = new Size(320, 240);

            while (m_isStreaming)
            {
                ImageSource image = await CaptureImageFromCamera();

                NewImageReceived?.Invoke(image);

                await Task.Delay(100);
            }
        }

        public Task StopCameraStream()
        {
            m_isStreaming = false;
            return Task.CompletedTask;
        }

        private async Task<ImageSource> CaptureImageFromCamera()
        {
            var completionSource = new TaskCompletionSource<ImageSource>();

            // Event handler for capturing image
            void OnMediaCaptured(object sender, MediaCapturedEventArgs e)
            {
                m_cameraView.Dispatcher.Dispatch(() =>
                {
                    Console.WriteLine("MediaCaptured event triggered");

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        e.Media.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        ImageSource image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));

                        // Get the captured image and set the result
                        completionSource.SetResult(image);
                    }

                    m_cameraView.MediaCaptured -= OnMediaCaptured;
                });
            }

            m_cameraView.MediaCaptured += OnMediaCaptured;

            try
            {
                Console.WriteLine("Attempting to capture image...");
                m_cameraView.CaptureImage(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing CaptureImage: {ex.Message}");
                completionSource.SetException(ex);
            }

            // We return the task from the completion source with the captured image
            return await completionSource.Task;
        }
    }
}