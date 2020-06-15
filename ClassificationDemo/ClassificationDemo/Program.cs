using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClassificationDemo
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var code = @"using System;

            public class MyClass
            {
                public static void MyMethod(int value)
                {
                }
            }";

            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            var workspace = new AdhocWorkspace(host);

            var souceText = SourceText.From(code);

            // with a project
            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp).
                WithMetadataReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
            var project = workspace.AddProject(projectInfo);
            var document = workspace.AddDocument(project.Id, "MyFile.cs", souceText);

            var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, new TextSpan(0, code.Length));

            foreach (var classifiedSpan in classifiedSpans.Where(s => !ClassificationTypeNames.AdditiveTypeNames.Contains(s.ClassificationType)))
            {
                var position = souceText.Lines.GetLinePositionSpan(classifiedSpan.TextSpan);
                Console.WriteLine($"{souceText.ToString(classifiedSpan.TextSpan)} - {classifiedSpan.ClassificationType} - {position.Start}:{position.End}");
            }

            // with semantic model
            var syntaxTree = CSharpSyntaxTree.ParseText(souceText);
            var compilation = CSharpCompilation.Create("Dummy").AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)).AddSyntaxTrees(syntaxTree);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            classifiedSpans = Classifier.GetClassifiedSpans(semanticModel, new TextSpan(0, code.Length), workspace);

            foreach (var classifiedSpan in classifiedSpans.Where(s => !ClassificationTypeNames.AdditiveTypeNames.Contains(s.ClassificationType)))
                {
                var position = souceText.Lines.GetLinePositionSpan(classifiedSpan.TextSpan);
                Console.WriteLine($"{souceText.ToString(classifiedSpan.TextSpan)} - {classifiedSpan.ClassificationType} - {position.Start}:{position.End}");
            }

            Console.ReadLine();
        }
    }
}
