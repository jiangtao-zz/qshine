The ScriptCompiler component is used to parse input text which mix with both script text and Script Control Code to pure script text based on Script Control Code syntax.

The Script Control Code is a markup syntax to control which part of script text to be render out based on input variables and control logic embbeding in input text. 

Script Control Code Syntax:

The Script Control Code always start with "#@" symbol.
It also can be block within brackets symbol "#@{" and "#@}".

	Example:

		#@{
			var Oracle = true;
			var value = 1 + 2;
		#@}

		#@if(value==2)



Variable:
	There are two type variables:
		1. Implicit variable:

		The implicit variable is an accessable C# variable passed from running program into ScriptCompiler instance when calling compiler parse().

		2. Defined variable:

		The defined variable should be declare before use it. The value will be empty if the value is not defined before use.

	Embeded a variable in the script:
		Embeded variable is a variable boxed in #@{}

		Example:

			 delete from table1 where id=#@{personId}

	
Control structures:
	The script compiler only support conditional control keywork (can be extend later)

	#@if(c# condition) {
	<<true block>>
	#@}
	#@else if(C# condition) {
	<<else if block>>
	#@}
	#@else {
	<<false block>>
	#@}

Comments:

	inline comments:
		#@//

	comments block
		#@/*
			<block>
		#@*/

		or
		#@{
		/*
		<block>
		*/
		#@}


Example:

<any non script control code block 1>

#@/* add comments here
 any
 any
#@*/

<any non script control code block 2>

#@ var IsOracle = 1;
#@{
	var HasTableExists=true;
	var placeHolder = "XYZ";

	print(placeHolder);
#@}
<any non script control code block 3>
#@if(HasTableExists == true && placeHolder=="XYZ")
<any non script control code block 4>
#@else
<any non script control code block may contain #@{variable} 5>
#@

Output function:

	print(text);

	it equavalent to #@{text}












