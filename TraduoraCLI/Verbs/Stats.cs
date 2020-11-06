using System;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;

namespace TraduoraCLI.Verbs {
    [Verb("stats", HelpText = "Show the stats for the given project")]
    public class Stats {

        [Option('p', "project", Required = true)]
        public string Project { get; set; }

        public async Task<int> Parse() {
            var project = await Program.GetProjectId(this.Project);
            var stats = await Program.GetJson($"/api/v1/projects/{project}/stats");

            var projectStats = stats["data"]["projectStats"];
            Console.WriteLine($"{projectStats["terms"]} terms total");
            Console.WriteLine($"{projectStats["translated"]} / {projectStats["total"]} translated in {projectStats["locales"]} locales");
            Console.WriteLine();
            var localeStats = (JObject) stats["data"]["localeStats"];
            foreach (var (key, value) in localeStats)
                Console.WriteLine($"{key}: {value["translated"]} ({(int) (value["progress"]!.Value<float>() * 100)}%) translated");
            return 0;
        }

    }
}