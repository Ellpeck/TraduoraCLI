using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
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
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        public static async Task<JObject> Post(string location, JObject content, bool auth = true) {
            if (auth)
                location = await Authenticate(location);
            var response = await Client.PostAsync(location, new StringContent(content.ToString(), Encoding.UTF8, "application/json"));
            return await ParseResponse(response);
        }

        public static async Task<JObject> Get(string location) {
            var response = await Client.GetAsync(await Authenticate(location));
            return await ParseResponse(response);
        }

        public static FileInfo GetAuthFile() {
            return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".traduoracli", ".auth"));
        }

        public static async Task<string> GetProjectId(string nameOrId) {
            var projects = await Get("/api/v1/projects");
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

        private static async Task<JObject> ParseResponse(HttpResponseMessage message) {
            var result = JObject.Parse(await message.Content.ReadAsStringAsync());
            var error = result["error"];
            if (error != null)
                throw new ResultException($"{error["code"]}: {error["message"]}");
            return result;
        }

    }
}