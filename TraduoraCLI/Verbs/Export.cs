using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using TraduoraCLI.Misc;

namespace TraduoraCLI.Verbs {
    [Verb("export", HelpText = "Export one or more translations to a file")]
    public class Export {

        [Option('p', "project", Required = true)]
        public string Project { get; set; }

        [Option('l', "locales", Required = true, HelpText = "One or more language codes of the languages that should be exported")]
        public IEnumerable<string> Locales { get; set; }

        [Option('f', "format", Default = "jsonflat", HelpText = "The format to export in. One of androidxml, csv, xliff12, jsonflat, jsonnested, yamlflat, yamlnested, properties, po, strings, php")]
        public string Format { get; set; }

        [Option('d', "directory", Default = ".", HelpText = "The directory to store the files in, relative to the current location")]
        public string Directory { get; set; }

        [Option('e', "extension", Default = "json", HelpText = "The resulting files' extension")]
        public string Extension { get; set; }

        [Option('c', "capitalize-name", HelpText = "When this is set, the resulting files' names will start with a capital letter")]
        public bool CapitalizeName { get; set; }

        public async Task<int> Parse() {
            var dir = new DirectoryInfo(this.Directory);
            if (!dir.Exists)
                dir.Create();

            var project = await Program.GetProjectId(this.Project);
            foreach (var locale in this.Locales) {
                var export = await Program.GetString($"/api/v1/projects/{project}/exports?locale={locale}&format={this.Format}", c => {
                    if (c == HttpStatusCode.BadRequest)
                        throw new ResultException($"There was an error exporting {locale}. Are you sure you have the right format selected?");
                });

                var name = locale;
                if (this.CapitalizeName)
                    name = char.ToUpper(name[0]) + name.Substring(1);
                var file = Path.Combine(dir.FullName, $"{name}.{this.Extension}");

                await using var stream = File.CreateText(file);
                await stream.WriteAsync(export);

                Console.WriteLine($"Exported {locale} to {file}");
            }
            return 0;
        }

    }
}