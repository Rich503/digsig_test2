//#define DEBUG

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
// Couldn't use this using statement - gave build errors. Needed to explicitely list path to class in program below
//using System.​Security.​Cryptography.​X509Certificates;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignatureVerify
{

    //******************************************************************************************************************
    // DigitalSignatureVerify
    // Purpose: Read contents of a text file, (filtering out blank lines and comments) and output lines to stdout
    // Input: cmd line - Source File
    //        i.e. DigitalSignatureVerify foo.txt
    // Output: 
    //      "Certificate is VALID"
    //      "File is not signed"
    //
    // Cmd Line Arguments
    //      Usage: DigitalSignatureVerify <file> -criteriaFile <file> -debug
    //      MANDATORY (one of the following is needed):
    //      <file>                  single file to check
    //      <directory>             folder of files to check
    //      -fileList <file)        file containing list of files to check
    //
    //      OPTIONAL Arguments:
    //      -criteriaFile <file>    file holding expected criteria of digital signature
    //                              this file must have all fields and be in specific order
    //                              i.e. digsig_criteria.txt
    //                                  SUBJECT = "Sage Software Inc."
    //                                  ISSUER = "DigiCert Trusted G4 Code Signing RSA4096 SHA384 2021 CA1"
    //                                  VALID_DATE = "7/16/2018 5:00:00 PM"
    //                                  EXPIRY_DATE = "7/21/2021 5:00:00 AM"
    //                                  SIGNATURE_ALGORITHM = "sha256RSA"
    //                                  VERSION = 3
    //                                Note: if not present, defaults to use file .\digsig_criteria.txt
    //                                Note: to ignore a SUBJECT, ISSUER, ALGORITHM field, leave the field, but remove the value after the "="
    //                                      to ignore a data, change the date to a very early/late date.
    //                                      I haven't found a good way to ignore VERSION
    //      -extensions             type of files to check (i.e. .exe .dll .cab - TBD
    //      -debug                  display debug info
    //
    //  
    // TODO:
    //      check illegal or non-existing certificate. Currently it gives an unhandled exception. (i.e. if read .txt file) -DONE
    //      read all files from a folder (recursively) or list of files in -listFile -DONE
    //      allow user to ignore specific criteria to check -DONE
    //      determine why "Valid to" date is failing (when check 20.5.1 file AB.exe)
    //      add cmd line option on which file extensions to check (i.e. *.exe *.dll *.cab) -DONE
    //      send info into logging file
    //      put file(s) to verify in a List and call one method to verify each file in list  --RGD??? is this ia good idea?
    //      put code into GitHub
    //      change extensions field default to allow all file types
    //      change extensions to all allow all file types
    //      move xxxCnt fields to globalService class
    //      add return type to screen for file "File is unsigned"
    //      refactor returnType to use enums instead of strings
    //



    enum returnValues { TRUE, FALSE, NOT_SIGNED, FILE_DOESNT_EXIST, ILLEGAL_FILE_TYPE, FILE_NOT_EXIST, UNKNOWN_ERROR_WITH_FILE }
    

    class Program
    {
        static void Main(string[] args)
        {
            //VARIABLES
            //RGD??? should I change the return types from string to enum?
            //enum returnValues {TRUE, FALSE, NOT_SIGNED, FILE_DOESNT_EXIST, ILLEGAL_FILE_TYPE, FILE_NOT_EXIST, UNKNOWN_ERROR_WITH_FILE }
            //flag - source file has valid digital signature
            string signatureStatus = "FALSE";
            //returnValues signatureStatus = returnValues.FALSE; ;  //RGD???enum -test of enum
            GlobalServiceClass globalServiceInMainMethod = new GlobalServiceClass();

            //List of files to verify
            string[] filesToVerify;

            // Parse command line args
            CmdLine cmdLineCheck = new CmdLine(globalServiceInMainMethod);
            if (cmdLineCheck.ParseArgs(args) == false)
            {
                return;
            }

            // Set global debug variable
            if (cmdLineCheck.Debug == true)
                globalServiceInMainMethod.debug = true;

            // Set global extension variable
            //GlobalService.extensions = cmdLineCheck.extensions;  //RGD???extension add later  //RGD??? should I put field "extensions" in the GlobalService class?

            if (globalServiceInMainMethod.debug)
            {
                //single file to check
                Console.WriteLine("");
                Console.WriteLine($"singleFileVerify: {cmdLineCheck.singleFileVerify}");
                Console.WriteLine($"direcotyrVerify: {cmdLineCheck.directoryVerify}");
                Console.WriteLine($"debug: {cmdLineCheck.Debug}");
                Console.WriteLine($"help: {cmdLineCheck.Help}");
                Console.WriteLine($"criteriaFile: {cmdLineCheck.criteriaFile}");
                Console.WriteLine($"fileListVerify: {cmdLineCheck.fileListVerify}");
                Console.WriteLine("");
            }


            // Read criteria file to get Digital Signature criteria for comparison
            DigSigCriteria digSigCriteria = new DigSigCriteria(cmdLineCheck.criteriaFile);

            // Create object to inspect and verify file(s)
            FileToVerify sourceFile = new FileToVerify(globalServiceInMainMethod);

            //RGD???List - should I load all of the files into a List and then run iterate through the list, instead of calling verifyFile() 3 times?
            // Verify file(s) - single file, directory of files, list of files from file
            if (cmdLineCheck.singleFileVerify != null)
            {
                // Check Single File
                string file = cmdLineCheck.singleFileVerify;
                signatureStatus = sourceFile.verifyFile(file, digSigCriteria);
                printResults(file, signatureStatus, sourceFile, globalServiceInMainMethod.debug);
            }
            else if (cmdLineCheck.directoryVerify != null)
            {
                // User input was a directory of files to verify
                // examine each file in directory. Throw out if a sub-directory. Verify signature of file
                // RGD???directory - should I recursively search the folder or use TopDirectory Only? (or should I make this a cmd line option?
                filesToVerify = Directory.GetFiles(cmdLineCheck.directoryVerify, "*.*", SearchOption.AllDirectories);
                foreach (string file in filesToVerify)
                {
                    signatureStatus = sourceFile.verifyFile(file, digSigCriteria);
                    printResults(file, signatureStatus, sourceFile, globalServiceInMainMethod.debug);
                }
            }
            else if (cmdLineCheck.fileListVerify != null)
            {
                // Examine each file in file listing files to verify.
                // Verify signature of file
                // Read file and put lines into array of strings
                string[] files = System.IO.File.ReadAllLines(cmdLineCheck.fileListVerify);
                foreach (string file in files)
                {
                    signatureStatus = sourceFile.verifyFile(file, digSigCriteria);
                    printResults(file, signatureStatus, sourceFile, globalServiceInMainMethod.debug);
                }
            }


            Console.WriteLine("");
            Console.WriteLine($"Valid Signatures: {sourceFile.validSignatureCount}");
            Console.WriteLine($"Invalid Signatures: {sourceFile.invalidSignatureCount}");
            Console.WriteLine($"File Not Signed: {sourceFile.notSignedCount}");
            Console.WriteLine($"Illegal File Types: {sourceFile.illegalFileTypeCount}");
            Console.WriteLine($"File Not Exist: {sourceFile.fileNotExistCount}");
            Console.WriteLine($"Other Errors: {sourceFile.otherErrorCount}");
            Console.WriteLine($"Total Files Checked: {sourceFile.validSignatureCount + sourceFile.invalidSignatureCount + sourceFile.notSignedCount +sourceFile.illegalFileTypeCount + sourceFile.fileNotExistCount + sourceFile.otherErrorCount}");

        }


        //****************************************************************************************************
        // printResults()
        // Purpose:
        //
        // Parameters:
        //
        // TODO:
        //      change which classess are sent in. FileToVerify needed? better way to use debug than as a parameter?
        //
        static void printResults(string file, string returnType, FileToVerify sourceFile, bool debug)
        {                        
            //RGD??? - should I be using these public fields since they don't belong to the file being searched, but to the entire list of files searched
            if (returnType == "TRUE")
            {
                sourceFile.validSignatureCount++;
                Console.WriteLine($"{file}: Signature is Valid"); 
            }

            else if (returnType == "FALSE")
            {
                sourceFile.invalidSignatureCount++;
                Console.WriteLine($"{file}: Signature is Invalid");
            }

            else if (returnType == "NOT SIGNED")
            {
                sourceFile.notSignedCount++;
                Console.WriteLine($"{file}: File is not signed");
            }
            else if (returnType == "ILLEGAL FILE TYPE")
            {
                sourceFile.illegalFileTypeCount++; 
                if (debug)
                {
                    Console.WriteLine($"[Error 4]: {file} is an illegal file type");
                }
            }

            else if (returnType == "FILE NOT EXIST")
            {
                sourceFile.fileNotExistCount++;
                if (debug)
                {
                    Console.WriteLine($"[Error 5] doesn't exist: {file}");
                }
            }
            else
            {
                sourceFile.otherErrorCount++;
                if (debug)
                {
                    Console.WriteLine($"6Error reading file {file}");
                }
            }
        }


    }
}

#if QUESTIONS
//QUESTIONS:

//File: Program.cs
line 86
string signatureStatus = "FALSE";                  //RGD???enum should I return an enumeration type instead of a string?
	if so, should I declare them in the namespace to make them global?

line 105
RGD???extension Should I put property "extensions" in the GlobalService class?

line 127 (priority 4)
//RGD???List - should I load all of the files into a List and then run iterate through the list, instead of calling verifyFile() 3 times?

line 139
RGD???directory - should I recursively search the folder or use TopDirectory Only? (or should I make this a cmd line option?



General (priority 5)
What's the best way to add logging?

What should the output of the utility be?
    currently it's:
        <file>: Signature is Valid /Invalid
        <file>- [Error x]  is an illegal file type


//File: CmdLine.cs:
line 151 (priority 3)
RGD??? - how do I input several file extensions i.e. .exe.dll.cab ?


//File: FileToVerify.cs:
line 15 (priority 2)
//RGD??? - should I be using these public fields  (xxxCount) since they don't belong to the file being searched, but to the entire list of files searched?
//RGD??? - should I make these private and create properties for each field?
//RGD??? - should I put these in the GlobalService class?

line 15
should I create properties for the count fields?

line 26 (priority 1)
line 42
why do I need to add globalService to the FileToVerify class?
	since I'm passing globalService into verifyFile, it doesn't seem that I need it in this class property. 


line 66
RGD??? this is ugly to use "Global.extensions"; what is a better way? - put into GlobalService?

line 68 (priority 3)
RGD???  How do I get file.EndsWith(extensions) to work with multiple file types(i.e. .exe.dll.cab)

line 101
//returnValues rtnVal = returnValues.NOT_SIGNED;  //RGD??? test of enum - should I use enum instead of strings for return values?


#endif
