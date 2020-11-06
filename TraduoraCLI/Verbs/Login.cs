using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;
using TraduoraCLI.Misc;

namespace TraduoraCLI.Verbs {
    [Verb("login", HelpText = "Log in using your traduora instance and save an auth token")]
    public class Login {

        [Option('s', "site", Required = true, HelpText = "The url of your traduora instance. Should be https for security")]
        public string Site { get; set; }

        [Option('u', "username", Required = true)]
        public string Username { get; set; }

        [Option('p', "password", Required = true)]
        public string Password { get; set; }

        [Option('c', "client-mode", HelpText = "Use a client id and secret instead of username and password")]
        public bool ClientMode { get; set; }

        public async Task<int> Parse() {
            var result = await Program.PostJson($"{this.Site}/api/v1/auth/token", new JObject {
                {"grant_type", this.ClientMode ? "client_credentials" : "password"},
                {this.ClientMode ? "client_id" : "username", this.Username},
                {this.ClientMode ? "client_secret" : "password", this.Password}
            }, false, e => {
                if (e == HttpStatusCode.NotFound || e == HttpStatusCode.BadRequest)
                    throw new ResultException("Failed to log in. Remember to set the client-mode option if you're using a client instead of a user");
            });

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