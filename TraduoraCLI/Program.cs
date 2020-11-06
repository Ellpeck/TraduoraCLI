using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TraduoraCLI.Misc;

namespace TraduoraCLI {
    public static class Program {

        private static readonly HttpClient Client = new HttpClient();

        public static async Task<int> Main(string[] args) {
            var verbs = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
            try {
                return await Parser.Default.ParseArguments(args, verbs).MapResult(
                    v => (Task<int>) v.GetType().GetMethod("Parse").Invoke(v, null),
                    e => Task.FromResult(-1));
            } catch (Exception e) {
                switch (e) {
                    case ProjectDoesNotExistException _:
                    case NotAuthenticatedException _:
                    case ResultException _:
                        Console.WriteLine(e.Message);
                        return 1;
                    default:
                        throw;
                }
            }
        }

        public static Task<JObject> PostJson(string location, JObject content, bool auth = true, Action<HttpStatusCode> errorHandler = null) {
            return Post(location, new StringContent(content.ToString(), Encoding.UTF8, "application/json"), auth, errorHandler);
        }

        public static async Task<JObject> Post(string location, HttpContent content, bool auth = true, Action<HttpStatusCode> errorHandler = null) {
            if (auth)
                location = await Authenticate(location);
            var response = await Client.PostAsync(location, content);
            CheckError(response, errorHandler);
            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public static async Task<JObject> GetJson(string location, Action<HttpStatusCode> errorHandler = null) {
            var response = await Client.GetAsync(await Authenticate(location));
            CheckError(response, errorHandler);
            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public static async Task<string> GetString(string location, Action<HttpStatusCode> errorHandler = null) {
            var response = await Client.GetAsync(await Authenticate(location));
            CheckError(response, errorHandler);
            return await response.Content.ReadAsStringAsync();
        }

        public static FileInfo GetAuthFile() {
            return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".traduoracli", ".auth"));
        }

        public static async Task<string> GetProjectId(string nameOrId) {
            var projects = await GetJson("/api/v1/projects");
            foreach (var project in (JArray) projects["data"]) {
                if (project["id"].ToString() == nameOrId)
                    return nameOrId;
                if (project["name"].ToString() == nameOrId)
                    return project["id"].ToString();
            }
            throw new ProjectDoesNotExistException(nameOrId);
        }

        private static async Task<string> Authenticate(string location) {
            var file = GetAuthFile();
            if (!file.Exists)
                throw new NotAuthenticatedException();
            using var stream = file.OpenText();
            var ret = await stream.ReadLineAsync() + location;
            var token = await stream.ReadLineAsync();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return ret;
        }

        private static void CheckError(HttpResponseMessage message, Action<HttpStatusCode> errorHandler) {
            if (message.StatusCode == HttpStatusCode.OK)
                return;
            errorHandler?.Invoke(message.StatusCode);
            if (message.StatusCode == HttpStatusCode.Unauthorized)
                throw new NotAuthenticatedException();
            throw new ResultException($"Error {(int) message.StatusCode}: {message.StatusCode}");
        }

    }
}