using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine;
using System;
using qshine.Compiler;
namespace qshine.UnitTests
{
	[TestClass]
	public class ScriptCompilerTest
	{
		[TestMethod()]
		public void CompilerTest()
		{
			var compiler = new ScriptCompiler();

			var input=
@"<any non script control code block 1>

#@/* 
 add comments here
 any
 any
#@*/


<any non script control code block 2>

#@ var IsOracle = 1;
#@{
	var HasTableExists=true;
	var placeHolder = ""XYZ"";

	print(placeHolder);
#@}
<any non script control code block 3>
#@if(HasTableExists == true && placeHolder==""XYZ""){
<any non script control code block 4>
#@}else{
<any non script control code block may contain #@{variable} 5>
#@}";

			var outputText = compiler.Parse(input);
		}

	}
}
