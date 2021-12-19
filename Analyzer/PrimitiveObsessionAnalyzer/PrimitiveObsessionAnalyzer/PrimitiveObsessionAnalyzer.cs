using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace PrimitiveObsessionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveObsessionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PrimitiveObsessionAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Declaration";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeDeclaration, SyntaxKind.LocalDeclarationStatement);

          //  context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);

            context.RegisterSyntaxNodeAction(AnlyzeReturnType, SyntaxKind.PredefinedType);
        }
        private void AnlyzeReturnType(SyntaxNodeAnalysisContext context)
        {
            var predefinedTypeDeclaration = (PredefinedTypeSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(predefinedTypeDeclaration);
            AnalyzeSymbol(symbolInfo, context, predefinedTypeDeclaration.GetLocation(), 
                context.Node.Parent.DescendantTokens().OfType<SyntaxToken>().FirstOrDefault( x=> x.Kind() == SyntaxKind.IdentifierToken).Text);
        }

        private void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(localDeclaration.Declaration.Type);
            AnalyzeSymbol(symbolInfo, context, localDeclaration.GetLocation(), context.Node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First().Identifier.Text);
        }

        private void AnalyzeSymbol(SymbolInfo symbolInfo, SyntaxNodeAnalysisContext context, Location location, string declarator)
        {
            var typeSymbol = ((ITypeSymbol)symbolInfo.Symbol);
            if (symbolInfo.Symbol != null
                && symbolInfo.Symbol.Name != "T"
                && typeSymbol  != null 
                && typeSymbol.SpecialType != SpecialType.System_Void
                && typeSymbol.SpecialType != SpecialType.None)
            {
                if (symbolInfo.Symbol.Name == "String" || typeSymbol.BaseType.Name == "ValueType" )
                {
                    var diagnostic = Diagnostic.Create(Rule, location, declarator);

                    context.ReportDiagnostic(diagnostic);

                }
            }
        }
    }
}
