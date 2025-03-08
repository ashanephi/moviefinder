using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OpenCvSharp;

namespace MovieFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly string _pythonPath = @"C:\Users\nephi\AppData\Local\Programs\Python\Python313\python.exe"; // Update with your Python path
        private readonly string _scriptPath = @"C:\Users\nephi\OneDrive\Desktop\C#\projects\MovieFinder\PythonScripts\google_lens.py"; // Path to Python script

        // API to handle video uploads
        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded." });
            }

            try
            {
                // Save the uploaded file
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract frames from video
                string framesFolder = Path.Combine(uploadsFolder, "frames");
                Directory.CreateDirectory(framesFolder);
                ExtractFrames(filePath, framesFolder);

                // Process extracted frames using Google Lens
                string[] frameFiles = Directory.GetFiles(framesFolder, "*.jpg");
                if (frameFiles.Length == 0)
                {
                    return BadRequest(new { error = "No frames extracted from video." });
                }

                string movieTitle = RunGoogleLens(frameFiles[0]); // Process first frame

                return Ok(new { movie = movieTitle });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Function to extract frames from the video
        private void ExtractFrames(string videoPath, string outputFolder)
        {
            using (var capture = new VideoCapture(videoPath))
            {
                if (!capture.IsOpened())
                {
                    throw new Exception("Could not open video file.");
                }

                double fps = capture.Fps;
                int frameRate = (int)fps;
                int interval = frameRate * 2; // Extract frame every 2 seconds

                Mat frame = new Mat();
                int frameIndex = 0;
                int savedFrames = 0;

                while (capture.Read(frame))
                {
                    if (frameIndex % interval == 0)
                    {
                        string framePath = Path.Combine(outputFolder, $"frame{savedFrames}.jpg");
                        Cv2.ImWrite(framePath, frame);
                        savedFrames++;
                    }
                    frameIndex++;
                }
            }
        }

        // Function to run the Python script for Google Lens processing
        private string RunGoogleLens(string imagePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"\"{_scriptPath}\" \"{imagePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result.Trim(); // Return the detected movie name
            }
        }
    }
}
