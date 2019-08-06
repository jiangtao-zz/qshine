using System;
using System.IO;

namespace Archive
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
@"
    ********************************************************************************
    **                                                                            **
    **  1. Archive a folder to a zip file with password and an image encryption.  **
    **  2. Extract encrypted file to folder using same password and image file    **
    **  3. Encrypt a file with password and image file                            ** 
    **  4. Decrypt a file with password and image file                            **
    **                                                                            **
    ** Usage:                                                                     **
    **                                                                            **
    **                                                                            **
    ** Archive a folder to a zip file with password and an image encryption.      **
    **                                                                            **
    **       C:\> Archive folder zipFile password, imageFile                      **
    **                                                                            **
    ** Extract an encrypted zip file to folder using same password and image file.**
    **                                                                            **
    **       C:\> Archive -r folder zipFile password, imageFile                   **
    **                                                                            **
    ** Encrypt a file with password and image file                                **
    **                                                                            **
    **       C:\> Archive -e plainFile outputFile password, imageFile             **
    **                                                                            **
    ** Decrypt a file with password and image file                                **
    **                                                                            **
    **       C:\> Archive -d encryptedFile plainFile password, imageFile          **  
    **                                                                            **
    ********************************************************************************

");
            if (args.Length >= 4)
            {
                int argIndex = 0;
                var arg1 = args[argIndex].ToLower();

                bool restoreFlag = (arg1 == "-r" || arg1 == "/r");
                bool encryptFlag = (arg1 == "-e" || arg1 == "/e");
                bool decryptFlag = (arg1 == "-d" || arg1 == "/d");
                if (restoreFlag || encryptFlag || decryptFlag)
                {
                    argIndex++;
                }

                string folder = args[argIndex++];
                string zipFile = args[argIndex++];
                string password = args[argIndex++];
                string imageFile = args[argIndex++];
                int errorCode = 0;

                if (encryptFlag || decryptFlag)
                {
                    if (!File.Exists(folder))
                    {
                        errorCode = -10;
                        Console.WriteLine($"The file {folder} is not found.");
                    }
                }
                else
                {
                    if (!Directory.Exists(folder))
                    {
                        if (!restoreFlag)
                        {
                            errorCode = -1;
                            Console.WriteLine($"Source folder {folder} is not found.");
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(folder);
                            }
                            catch (Exception ex)
                            {
                                errorCode = -2;
                                Console.WriteLine($"Failed to create folder {folder}. {ex}");
                            }
                        }
                    }
                }

                if (!File.Exists(zipFile) && restoreFlag)
                {
                    errorCode = -3;
                    Console.WriteLine($"The file {zipFile} is not found.");
                }

                if (File.Exists(zipFile) && !restoreFlag)
                {
                    errorCode = -4;
                    Console.WriteLine($"The file {zipFile} exists already. Continue (Y/N)?");
                    var x = Console.ReadKey();
                    if(x.KeyChar=='Y'|| x.KeyChar == 'y')
                    {
                        File.Delete(zipFile);
                        errorCode = 0;
                        Console.WriteLine();
                    }
                    else
                    {
                        Environment.Exit(errorCode);
                        return;
                    }
                }

                if (!File.Exists(imageFile))
                {
                    errorCode = -5;
                    Console.WriteLine($"The image file {imageFile} is not found.");
                }

                if (errorCode == 0)
                {
                    var archive = new FileArchive(folder, zipFile, password, imageFile);
                    if(encryptFlag)
                    {
                        archive.EncryptFile();
                    }else if(decryptFlag)
                    {
                        archive.DecryptFile();
                    }
                    else if (restoreFlag)
                    {
                        archive.Restore();
                    }
                    else
                    {
                        archive.Archive();
                    }
                }

/*                Console.WriteLine(
    $@"
restoreFlag = {restoreFlag}
folder={folder}
zipFile={zipFile}
password={password}
imageFile={imageFile}
"
                    );
*/
            }
            else
            {
                Console.WriteLine("Missing argument.");
            }
            Console.WriteLine("Press any key to exist.");
            Console.ReadKey();
        }
    }
}
