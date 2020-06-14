using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace ClassificationDemo
{
    class Program
    {
        async static Task Main(string[] args)
        {
            // default assemblies are
            //    "Microsoft.CodeAnalysis.Workspaces",
            //    "Microsoft.CodeAnalysis.CSharp.Workspaces",
            //    "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            //    "Microsoft.CodeAnalysis.Features",
            //    "Microsoft.CodeAnalysis.CSharp.Features",
            //    "Microsoft.CodeAnalysis.VisualBasic.Features"
            // http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Workspace/Host/Mef/MefHostServices.cs,126
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var workspace = new AdhocWorkspace(host);

            var code = @"using System;

            public class MyClass
            {
                public static void MyMethod(int value)
                {
                }
            }";

            var souceText = SourceText.From(code);
            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp).
                WithMetadataReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
            var project = workspace.AddProject(projectInfo);
            var document = workspace.AddDocument(project.Id, "MyFile.cs", souceText);

            var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, new TextSpan(0, code.Length));

            foreach (var classifiedSpan in classifiedSpans)
            {
                var position = souceText.Lines.GetLinePositionSpan(classifiedSpan.TextSpan);
                Console.WriteLine($"{souceText.ToString(classifiedSpan.TextSpan)} - {classifiedSpan.ClassificationType} - {position.Start}:{position.End}");
            }
            Console.ReadLine();
        }
    }
}
