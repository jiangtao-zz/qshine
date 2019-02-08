Steps to generate design diagram.
1. Download Graphviz software from https://www.graphviz.org/ and install
2. Download "plantuml.jar" from https://sourceforge.net/projects/plantuml/files/plantuml.jar/download and copy into c:\myproject\plantuml folder
3. Add plantunl.jar command in tool:
	>>Command: C:\Program Files (x86)\Java\jre1.8.0_201\bin\javaw.exe
	>>Arguments: -jar c:\myproject\plantuml\plantuml.jar
	>>Work directory: ${FileDir} or ${ItemDir}
4. Locate to "applicationDesigner" folder and execute this "external tool".

