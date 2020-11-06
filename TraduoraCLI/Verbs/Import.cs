using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using TraduoraCLI.Misc;

namespace TraduoraCLI.Verbs {
    [Verb("import", HelpText = "Import one or more translations from a file")]
    public class Import {

        [Option('p', "project", Required = true)]
        public string Project { get; set; }

        [Option('l', "locales", Required = true, HelpText = "One or more language codes of the languages that should be imported")]
        public IEnumerable<string> Locales { get; set; }

        [Option('f', "format", Default = "jsonflat", HelpText = "The format that the files to import are in. One of androidxml, csv, xliff12, jsonflat, jsonnested, yamlflat, yamlnested, properties, po, strings, php")]
        public string Format { get; set; }

        [Option('d', "directory", Default = ".", HelpText = "The directory that the translation files can be found in, relative to the current location")]
        public string Directory { get; set; }

        public async Task<int> Parse() {
            var dir = new DirectoryInfo(this.Directory);
            if (!dir.Exists) {
                Console.WriteLine($"Couldn't find any translation files in {dir.FullName}");
                return -1;
            }
            var files = dir.GetFiles();

            var project = await Program.GetProjectId(this.Project);
            foreach (var locale in this.Locales) {
                var file = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).Equals(locale, StringComparison.OrdinalIgnoreCase));
                if (file == null) {
                    Console.WriteLine($"Skipping locale {locale} as there is no matching file");
                    continue;
                }

                var multipartContent = new MultipartFormDataContent();
                using (var stream = file.OpenText()) {
                    var content = new StringContent(await stream.ReadToEndAsync(), Encoding.UTF8, "text/plain");
                    content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse($"form-data; name=\"file\"; filename=\"{file.Name}\"");
                    multipartContent.Add(content, "file");
                }

                var result = await Program.Post($"/api/v1/projects/{project}/imports?locale={locale}&format={this.Format}", multipartContent, errorHandler: c => {
                    if (c == HttpStatusCode.BadRequest)
                        throw new ResultException($"There was an error importing {locale}. Are you sure you have the right format selected?");
                });
                var data = result["data"];
                Console.WriteLine($"Imported {locale} with {data["terms"]["added"]} terms added and {data["translations"]["upserted"]} terms upserted");
            }
            return 0;
        }

    }
}