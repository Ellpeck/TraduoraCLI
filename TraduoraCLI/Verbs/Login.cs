using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;

namespace TraduoraCLI.Verbs {
    [Verb("login", HelpText = "Log in using your traduora instance and save an auth token")]
    public class Login {

        [Option('s', "site", Required = true, HelpText = "The url of your traduora instance. Should be https for security")]
        public string Site { get; set; }

        [Option('u', "username", Required = true)]
        public string Username { get; set; }

        [Option('p', "password", Required = true)]
        public string Password { get; set; }

        public async Task<int> Parse() {
            var result = await Program.PostJson($"{this.Site}/api/v1/auth/token", new JObject {
                {"grant_type", "password"},
                {"username", this.Username},
                {"password", this.Password}
            }, false);

            var authFile = Program.GetAuthFile();
            if (!authFile.Directory.Exists)
                authFile.Directory.Create();
            await using (var stream = authFile.CreateText()) {
                await stream.WriteLineAsync(this.Site);
                await stream.WriteLineAsync(result["access_token"].ToString());
            }
            Console.WriteLine("Login successful");
            return 0;
        }

    }
}