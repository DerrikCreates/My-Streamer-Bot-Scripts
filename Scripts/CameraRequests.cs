using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            request.Method = WebRequestMethods.Http.Get;
            //request.ContentType = "application/json";


            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                CPH.LogError($"failed to get camera data for preset statuscode:{response.StatusCode}");
                return false;
            }

            CPH.LogDebug($"Received response from server statuscode:{response.StatusCode}");

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                CPH.LogDebug($"Camera Raw Json:{result}");


                var globalName = $"CamPreset_{camName}_{presetName}";

                CPH.LogDebug($"Parsed json! saving to global var {globalName}");
                CPH.SetGlobalVar(globalName, result, true);
            }

            return true;
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
                CPH.LogError("not enough arguements");
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

            CPH.LogDebug($"Loading camera preset json:{json}");

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var url = "http://localhost:5555/SetCamera"; // Replace with your API URL

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
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    CPH.LogError(sr.ReadToEnd());
                    sr.Close();
                }

                return false;
            }


            return true;
        }
    }
}