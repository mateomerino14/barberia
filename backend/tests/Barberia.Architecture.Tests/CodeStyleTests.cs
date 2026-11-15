using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Barberia.Architecture.Tests;

/// <summary>Revisa en el código fuente las reglas de estilo que el compilador no puede exigir.</summary>
public sealed class CodeStyleTests
{
    private const string SolutionFile = "Barberia.slnx";
    private const string SummaryOpen = "<summary>";
    private const string SummaryClose = "</summary>";
    private static readonly string[] SourceFolders = ["src", "tests"];
    private static readonly string[] IgnoredFolders = ["bin", "obj"];

    /// <summary>No hay operadores ternarios: las decisiones se escriben con if y else.</summary>
    [Fact]
    public void Sources_HaveNoTernaryOperators()
    {
        List<string> offenders = [];
        foreach ((string path, SyntaxNode root) in ParseSources())
        {
            foreach (ConditionalExpressionSyntax node in root.DescendantNodes().OfType<ConditionalExpressionSyntax>())
            {
                offenders.Add(Describe(path, node));
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>Cada clase, record, interfaz y método lleva una documentación de una sola línea.</summary>
    [Fact]
    public void Declarations_HaveOneLineSummary()
    {
        List<string> offenders = [];
        foreach ((string path, SyntaxNode root) in ParseSources())
        {
            IEnumerable<MemberDeclarationSyntax> declarations = root.DescendantNodes().OfType<MemberDeclarationSyntax>()
                .Where(IsDocumentedKind);
            foreach (MemberDeclarationSyntax declaration in declarations)
            {
                if (!HasOneLineSummary(declaration))
                {
                    offenders.Add(Describe(path, declaration));
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>Indica si el tipo de declaración debe llevar documentación.</summary>
    private static bool IsDocumentedKind(MemberDeclarationSyntax declaration)
    {
        return declaration is BaseTypeDeclarationSyntax or MethodDeclarationSyntax or DelegateDeclarationSyntax;
    }

    /// <summary>Comprueba que la declaración tenga un summary escrito en una sola línea.</summary>
    private static bool HasOneLineSummary(MemberDeclarationSyntax declaration)
    {
        string[] docLines = declaration.GetLeadingTrivia()
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .SelectMany(trivia => trivia.ToFullString().Split('\n'))
            .Where(line => line.Contains(SummaryOpen, StringComparison.Ordinal)
                || line.Contains(SummaryClose, StringComparison.Ordinal))
            .ToArray();
        if (docLines.Length != 1)
        {
            return false;
        }

        return docLines[0].Contains(SummaryOpen, StringComparison.Ordinal)
            && docLines[0].Contains(SummaryClose, StringComparison.Ordinal);
    }

    /// <summary>Arma el texto de error con el archivo y la línea de la declaración.</summary>
    private static string Describe(string path, SyntaxNode node)
    {
        int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return $"{path}:{line}";
    }

    /// <summary>Lee y analiza todos los archivos C# del backend.</summary>
    private static IEnumerable<(string Path, SyntaxNode Root)> ParseSources()
    {
        string backend = FindBackendFolder();
        foreach (string folder in SourceFolders)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Path.Combine(backend, folder), "*.cs",
                SearchOption.AllDirectories).Where(file => !IsGenerated(file));
            foreach (string file in files)
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                yield return (Path.GetRelativePath(backend, file), tree.GetRoot());
            }
        }
    }

    /// <summary>Indica si el archivo está dentro de una carpeta de compilación.</summary>
    private static bool IsGenerated(string file)
    {
        string[] parts = file.Split(Path.DirectorySeparatorChar);
        return parts.Any(part => IgnoredFolders.Contains(part));
    }

    /// <summary>Sube desde la carpeta de la prueba hasta la que contiene la solución.</summary>
    private static string FindBackendFolder()
    {
        DirectoryInfo? folder = new(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, SolutionFile)))
        {
            folder = folder.Parent;
        }

        if (folder is null)
        {
            throw new DirectoryNotFoundException(SolutionFile);
        }

        return folder.FullName;
    }
}
