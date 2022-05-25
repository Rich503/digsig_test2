using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DigitalSignatureVerify
{
    class CmdLine
    {
        //FIELDS
        //public string[] extensionArray;
        private GlobalServiceClass globalServiceVariableInCmdLine;         //why is this globalService here since it's in GlobalService in main?

        //PROPERTIES

        //single file to check
        public string singleFileVerify { get; set; }
        //Directory to check files
        public string directoryVerify { get; set; }
        //file holding files to check
        public string fileListVerify { get; set; }
        //debug flag
        public bool Debug { get; set; }
        //Help flag
        public bool Help { get; set; }
        //file holding criteria to check
        public string criteriaFile { get; set; }

        //type of files to check
        //public string extensions { get; set; }




        //CONSTRUCTORS
        public CmdLine(GlobalServiceClass gs)
        {
            //Set defaults
            Debug = false;
            criteriaFile = ".\\digsig_criteria.txt";
            //extensions = "*";
            globalServiceVariableInCmdLine = gs;
            gs.extensions = "*";

        }


    //METHODS

    //********************************************************************************************
    // ParseArgs
    // 
    // Purpose: Parse command line arguments
    // Parameters:
    //      args        string of cmd line args
    // Return:
    //      true        valid cmd line args
    //      false       invalid cmd line args
    //  
    // Algorithm:
    //      1st arg needs to be present and consists of either:
    //          <file>                  -a single file to check the digital signature
    //          <directory>             -directory of files to compare (NOT IMPLEMENTED YET)
    //          -fileList               -list of files to compare (NOT IMPLEMENTED YET)
    //          
    //      all other args are optional
    //          -criteralFile           -holds list of digital signature values exptected
    //          -debug                  -send debug info to screen
    //
    public bool ParseArgs(string[] args)
        {
            bool returnVal = false;
            string usageString = "\nError: Usage: DigitalSignatureVerify <file> -criteriaFile <file> -debug\n";
            int cmdLineArg = 0;


            // Check 1st argument
            // Need to have at least one argument
            // if at least one arg exists assume the first arg is either:
            //      a single file to check,
            //      a directory of files to check or
            //      a file listing files to check
            if (args.Length < 1)
            {
                //Console.WriteLine("\nError: Usage: DigitalSignatureVerify <file> -debug\n");
                Console.WriteLine(usageString);
                return (false);
            }
            else if (File.Exists(args[0]))
            {
                singleFileVerify = args[0];
                returnVal = true;
            }
            else if (Directory.Exists(args[0]))
            {
                directoryVerify = args[0];
                returnVal = true;
            }
            else if (String.Equals(args[0], "-fileList"))
            {
                fileListVerify = args[0];

                //next argument should be the list of file; check if on cmd line; check if file exists
                cmdLineArg++;
                if ((cmdLineArg < args.Length) && (File.Exists(args[cmdLineArg])))
                {
                    fileListVerify = args[cmdLineArg];
                }
                else
                {
                    Console.WriteLine(usageString);
                    return false;
                }
                returnVal = true;
            }
            else
            { 
                Console.WriteLine(usageString);
                return false;
            }
            cmdLineArg++;

            //Check optional arguments
            while (cmdLineArg < args.Length)
            {
                if (String.Equals(args[cmdLineArg], "-criteriaFile"))
                {
                    //next argument should be the criteria file; check if on cmd line; check if file exists
                    cmdLineArg++;
                    if ((cmdLineArg < args.Length) && (File.Exists(args[cmdLineArg])))
                    {
                        criteriaFile = args[cmdLineArg];
                    }
                    else
                    {
                        Console.WriteLine(usageString);
                        return false;
                    }
                    cmdLineArg++;
                    returnVal = true;
                }

                else if (String.Equals(args[cmdLineArg], "-debug"))
                {
                    Debug = true;
                    cmdLineArg++;
                    returnVal = true;
                }
                else if (String.Equals(args[cmdLineArg], "-extensions"))
                {
                    //next argument should be the list of file types to allow; check if on cmd line
                    cmdLineArg++;
                    //Check for list of extensions. Input is assumed to be comma separated (i.e. .exe,.dll,.cab) - no spaces
                    //RGD??? - need to check for extensions
                    //if ((cmdLineArg < args.Length) && (args[cmdLineArg]).Contains("."))
                    if ((cmdLineArg < args.Length))
                        {
                        globalServiceVariableInCmdLine.extensions = args[cmdLineArg];
                        globalServiceVariableInCmdLine.extensionsArray = globalServiceVariableInCmdLine.extensions.Split(',');
                    }
                    else
                    {
                        Console.WriteLine(usageString);
                        return false;
                    }
                    cmdLineArg++;
                    returnVal = true;
                }
                else if (String.Equals(args[0], "-help"))
                {
                    Console.WriteLine("In ParseArgs - help= true; TBD");
                    Help = true;
                    returnVal = true;
                }
                else
                {
                    Console.WriteLine(usageString);
                    return false;
                }
            }
            return returnVal;
  
        }

    }
}
