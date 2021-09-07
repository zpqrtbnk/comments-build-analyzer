// Copyright (c) 2008-2021, ZpqrtBnk. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ZpqrtBnk.CommentsBuildAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer : DiagnosticAnalyzer
    {
        private const string Category = "ZpqrtBnk";
        private const string HelpLinkUri = "https://github.com/zpqrtbnk/Zpqrtbnk.CommentsBuildAnalyzer";

        private static readonly LocalizableString Title = "Comments Analyzer.";
        //private static readonly LocalizableString MessageFormat = "Found a FIX*ME comment.";
        //private static readonly LocalizableString Description = "Found a FIX*ME comment.";

        private static readonly DiagnosticDescriptor WarnOnFixmeRule = new DiagnosticDescriptor(
            "ZB1001",
            Title,
            "FIXME comment in code.",
            Category,
            DiagnosticSeverity.Warning, // report as a warning
            true,
            //description: Description,
            helpLinkUri: HelpLinkUri);

        private static readonly DiagnosticDescriptor ErrOnFixmeRule = new DiagnosticDescriptor(
            "ZB1002",
            Title,
            "FIXME! comment in code.",
            Category,
            DiagnosticSeverity.Warning, // report as a warning
            true,
            //description: Description,
            helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(WarnOnFixmeRule, ErrOnFixmeRule);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze|GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(HandleSyntaxTree);
        }

        private static void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            try
            {
                TryHandleSyntaxTree(context);
            }
            catch
            {
                // Analyzers should not throw
                if (Debugger.IsAttached) throw;
            }
        }

        private static void TryHandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);

            foreach (var node in root.DescendantTrivia())
            {
                string comment;

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (node.Kind())
                {
                    case SyntaxKind.SingleLineCommentTrivia:

                        comment = node.ToString();
                        AnalyzeComment(comment, node.GetLocation(), context);
                        break;

                    case SyntaxKind.SingleLineDocumentationCommentTrivia:
                    case SyntaxKind.MultiLineCommentTrivia:

                        comment = node.ToString();

                        var offset = node.SpanStart;

                        foreach (var commentLine in comment.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            AnalyzeComment(commentLine, node.GetLocation(), context, offset);
                            offset = offset + commentLine.Length + Environment.NewLine.Length;
                        }

                        break;
                }
            }
        }

        private static void AnalyzeComment(string comment, Location location, SyntaxTreeAnalysisContext context, int startOffset = -1)
        {
            // order is important else the FIX*ME rule triggers for FIX*ME!
            var _ =
                LookupTerm(comment, location, context, "FIXME!", ErrOnFixmeRule, startOffset) ||
                LookupTerm(comment, location, context, "FIXME", WarnOnFixmeRule, startOffset);
        }

        private static bool LookupTerm(string comment, Location location, SyntaxTreeAnalysisContext context, string term, DiagnosticDescriptor descriptor, int startOffset = -1)
        {
            if (!comment.Contains(term))
                return false;

            var termOffset = comment.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (startOffset < 0) startOffset = location.SourceSpan.Start;
            var diagnosticLocation = Location.Create(location.SourceTree, new TextSpan(startOffset + termOffset, term.Length));
            context.ReportDiagnostic(Diagnostic.Create(descriptor, diagnosticLocation));
            return true;
        }
    }
}
