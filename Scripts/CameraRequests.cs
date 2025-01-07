using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScintillaNET;
using Streamer.bot.Plugin.Interface;

namespace Scripts
{
    public class CameraRequests : CPHInlineBase
    {
        bool SetCameraPropertyExecute()
        {
            if (!CPH.TryGetArg<string>("rawInput", out var rawInput))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                return false;
            }

            var parts = rawInput.Split(' ');

            if (parts.Length < 3)
            {
                return false;
            }

            var cameraName = parts[0];
            var property = parts[1];
            if (!int.TryParse(parts[2], out int value))
            {
                return false;
            }


            var jObject = new JObject();

            jObject.Add("Name", cameraName);
            jObject.Add(property, value);


            var url = "http://localhost:5555/GetCamera"; // Replace with your API URL

            var json = JsonConvert.SerializeObject(jObject);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        bool SaveCameraPreset()
        {
            //CamPreset_{camName}_{presetName}
            //!command {CamName} {PresetName}
            // !savecam Cam3 day
            if (!CPH.TryGetArg<string>("rawInput", out var raw))
            {
                return false;
            }

            var parts = raw.Split(' ');

            if (parts.Length != 2)
            {
                return false;
            }

            var camName = parts[0].Trim();
            var presetName = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(camName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(presetName))
            {
                return false;
            }

            var url = $"http://localhost:5555/GetCamera/{camName}"; // Replace with your API URL

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";


            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                JObject json = null;
                try
                {
                    json = JObject.Parse(result);
                }
                catch (Exception ex)
                {
                    return false;
                }

                CPH.SetGlobalVar($"CamPreset_{camName}_{presetName}", JsonConvert.SerializeObject(json));
            }

            return false;
        }


        bool SetCameraPreset()
        {
            if (!CPH.TryGetArg<string>("rawInput", out var raw))
            {
                return false;
            }

            var parts = raw.Split(' ');

            if (parts.Length != 2)
            {
                return false;
            }

            var camName = parts[0].Trim();
            var presetName = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(camName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(presetName))
            {
                return false;
            }


            //CamPreset_{camName}_{presetName}
            var json = CPH.GetGlobalVar<string>($"CamPreset_{camName}_{presetName}", true);


            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var url = "http://localhost:5555/GetCamera"; // Replace with your API URL

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }


            return true;
        }
    }
}