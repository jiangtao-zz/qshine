set OutDir=bin\Debug
@echo ====copy native database drivers and providers to database config folder
@echo =========Sqlite======
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\System.Data.SQLite.dll" ".\database\config\sqlite\bin\net45\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\System.Data.SQLite.xml" ".\database\config\sqlite\bin\net45\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\System.Data.SQLite.EF6.dll" ".\database\config\sqlite\bin\net45\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\System.Data.SQLite.Linq.dll" ".\database\config\sqlite\bin\net45\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\x64\SQLite.Interop.dll" ".\database\config\sqlite\bin\net45\x64\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\x86\SQLite.Interop.dll" ".\database\config\sqlite\bin\net45\x86\"
xcopy  /F /R /Y /D ".\database\sqlite\qshine.database.sqlite\%OutDir%\qshine.database.sqlite.dll" ".\database\config\sqlite\bin\"
@echo =========postgresql======
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.NetworkInformation.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Threading.Tasks.Extensions.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.Security.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.Sockets.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Runtime.CompilerServices.Unsafe.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.Requests.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\Npgsql.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\Npgsql.pdb" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\Npgsql.xml" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.Ping.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.Primitives.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\System.Net.NameResolution.dll" ".\database\config\postgresql\bin\net45\"
xcopy  /F /R /Y /D ".\database\postgresql\qshine.database.postgresql\%OutDir%\qshine.database.postgresql.dll" ".\database\config\postgresql\bin\"
echo =========mysql======
xcopy  /F /R /Y /D ".\database\mysql\qshine.database.mysql\%OutDir%\qshine.database.mysql.dll" ".\database\config\mysql\bin\"
@echo =========sqlserver======
xcopy  /F /R /Y /D ".\database\sqlserver\qshine.database.sqlserver\%OutDir%\qshine.database.sqlserver.dll" ".\database\config\sqlserver\bin\"
@echo =========oracle======
xcopy  /F /R /Y /D ".\database\oracle\qshine.database.oracle\%OutDir%\Oracle.ManagedDataAccess.dll" ".\database\config\oracle\bin\"
xcopy  /F /R /Y /D ".\database\oracle\qshine.database.oracle\%OutDir%\qshine.database.oracle.dll" ".\database\config\oracle\bin\"
@echo =========debug log======
xcopy  /F /R /Y /D ".\qshine.extension\qshine.LogInspector\%OutDir%\qshine.LogInspector.dll" ".\database\config\bin\"


