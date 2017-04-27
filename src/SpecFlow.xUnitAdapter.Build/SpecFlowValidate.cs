using System;
using System.IO;
using System.Text.RegularExpressions;
using Gherkin;
using Gherkin.Ast;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SpecFlow.xUnitAdapter.SpecFlowPlugin;

namespace SpecFlow.xUnitAdapter.Build
{
    public class SpecFlowValidate : AppDomainIsolatedTask
    {
        private static Regex LineAndColumnRegex = new Regex(@"\(([^)]*)\)");

        /// <summary>
        /// Gets or sets the collection of feature files being validated.
        /// </summary>
        [Required]
        public ITaskItem[] FeatureFiles { get; set; }

        public override Boolean Execute()
        {
            if (this.FeatureFiles.Length <= 0)
            {
                return true;
            }

            //Debugger.Launch();

            this.Log.LogMessage(MessageImportance.Normal, "Validating SpecFlow Features:");

            for (var i = 0; i < this.FeatureFiles.Length; i++)
            {
                var item = this.FeatureFiles[i];

                this.Validate(item);
            }

            return !this.Log.HasLoggedErrors;
        }

        private void Validate(ITaskItem item)
        {
            this.Log.LogMessage(MessageImportance.Normal, $"    {item}");

            var path = item.GetMetadata("FullPath");

            var parser = SpecFlowParserHelper.CreateParser();

            using (var stream = File.OpenText(path))
            {
                try
                {
                    var document = parser.Parse(stream, path);
                }
                catch (ParserException ex)
                {
                    var location = ex.Location ?? GetLocation(ex.Message);

                    this.Log.LogError("SubCategory", "ErrorCode", "HelpKeyword", path, location.Line, location.Column, location.Line, location.Line, ex.Message);
                }
            }
        }

        // Error message will have the location (17:2) for line 17, column 2.
        private Location GetLocation(string message)
        {
            var match = LineAndColumnRegex.Match(message);

            var lineAndColumn = match.Groups[1].Value;

            if (String.IsNullOrWhiteSpace(lineAndColumn))
            {
                return new Location();
            }

            var split = lineAndColumn.Split(':');
            if (split.Length != 2)
            {
                return new Location();
            }

            Int32.TryParse(split[0], out var line);
            Int32.TryParse(split[1], out var column);

            return new Location(line, column);
        }
    }
}
