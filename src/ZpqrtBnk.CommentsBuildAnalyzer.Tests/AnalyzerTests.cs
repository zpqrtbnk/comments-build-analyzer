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

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = ZpqrtBnk.CommentsBuildAnalyzer.Testing.Verifiers.CSharpCodeFixVerifier<
    ZpqrtBnk.CommentsBuildAnalyzer.Analyzer,
    ZpqrtBnk.CommentsBuildAnalyzer.CodeFixProvider>;

namespace ZpqrtBnk.CommentsBuildAnalyzer.Tests
{
    [TestClass]
    public class AnalyzerTests
    {
        [TestMethod]
        public async Task Test()
        {
            const string code = @"
using System;

// define namespace
namespace NameSpace
{
    // define class
    // FIXME: use an explicit name
    public class Foo
    {
        // FIXME! this is really bad
        public void Meh() { }
    }
}";

            var expected1 = new DiagnosticResult("ZB1001", DiagnosticSeverity.Warning)
                .WithMessage("FIXME comment in code.")
                .WithLocation(8, 8);

            var expected2 = new DiagnosticResult("ZB1002", DiagnosticSeverity.Warning)
                .WithMessage("FIXME! comment in code.")
                .WithLocation(11, 12);

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }
    }
}
