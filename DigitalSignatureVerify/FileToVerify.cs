using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;


namespace DigitalSignatureVerify
{
    class FileToVerify
    {
        // FIELDS
        //RGD??? - should I be using these public fields since they don't belong to the file being searched, but to the entire list of files searched
        //RGD??? - should I make these private and create properties for each field?
        //RGD??? - should I put these in the GlobalService class?

        //count of valid and invalid signatures
        public int validSignatureCount;
        public int invalidSignatureCount;
        public int notSignedCount;
        public int illegalFileTypeCount;
        public int fileNotExistCount;
        public int otherErrorCount;
        private GlobalServiceClass globalServiceVariableInFileToVerify;         //why is this globalService here since it's in GlobalService in main?

        // PROPERTIES


        // CONSTRUCTORS

        public FileToVerify (GlobalServiceClass gs)
        {
            //initialize counts
            validSignatureCount = 0;
            invalidSignatureCount = 0;
            notSignedCount = 0;
            illegalFileTypeCount = 0;
            fileNotExistCount = 0;
            otherErrorCount = 0;
            globalServiceVariableInFileToVerify = gs;
        }

        //METHODS

        //********************************************************************************************************************
        // VerifyFile
        //  Purpose:
        //      check each file to see if valid file (file exists, filer out non .exe .dll .cab files, has a signature
        //
        // Input:
        //  file        file to check
        // Return:
        //  string      result of file check
        //      "FILE NOT EXIST"    -file doesn't exist
        //      "ILLEGAL FILE TYPE" -not a type of file we're accepting (i.e. .exe, .dll, .cab)
        //      "OTHER ERROR        -some other unspecified error occured
        //      "FALSE"             -signature is invalid
        //      "TRUE"              -signature is valid
        //
        public string verifyFile(string file, DigSigCriteria digSigCriteria)
        {
            string returnType = "FALSE";


            if (validFileExtension(file) == false)
            //if (!((file.EndsWith(".exe")) || (file.EndsWith(".dll")) || (file.EndsWith(".cab"))))   //original - works OK
            {
                returnType = "ILLEGAL FILE TYPE";
            }
            else if (!File.Exists(file))
            {
                returnType = "FILE NOT EXIST";
                //Console.WriteLine($"{file} - [Error 2] FILE DOESN'T EXIST");
            }
            else if (!String.IsNullOrEmpty(file))
            {
                returnType = VerifySignature(file, digSigCriteria, globalServiceVariableInFileToVerify);
                //Console.WriteLine($"{file}: Signature is {(returnType == "TRUE" ? "Valid" : "Invalid")}");  //RGD string vs bool mod - change to use other statuses 
            }
            else
            {
                returnType = "OTHER ERROR";
                //Console.WriteLine($"{file} - [Error 3] UNKNOWN ERROR WITH FILE");
            }

            return returnType;
        }



        private bool validFileExtension(string file)
        {
            foreach (string extension in globalServiceVariableInFileToVerify.extensionsArray)
            {
                //RGD??? why doesn't EndsWith() see ".*"? - workaround is to explicitely check for ".*"
                //Console.WriteLine($"extensionArray[]: {extension}");
                if ((file.EndsWith(extension)) || (extension == "*"))
                {
                    return true;
                }
            }
            return false;
        }



//===================================================================================================
// VerifySignature
//
// Purpose:
// Read criteria from file of what criteria to compare digital signature (i.e. SUBJECT, ISSUER, etc
// Compare against a file's digital signature
// Format output for debugging
// Send success/fail status
//
// Input:
//      file                file to verify
//      digSigCriteria      object holding criteria to check against
// Output:
//      true        -file's digital signature is valid
//      false       -file's digital signature is invalid
//
static string VerifySignature(string file, DigSigCriteria digSigCriteria, GlobalServiceClass globalService)
        {
            string validSignature = "TRUE";
            bool validCheck = false;


            // For info on digital signature classes, see MS site: 
            //     https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate?view=net-5.0
            //     https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate?view=net-5.0
            //
            //X509Certificate cert2 = new X509Certificate();
            //X509Certificate cert = X509Certificate.​Create​From​Signed​File(file);


            //Gets the x.509v3 certificate from the specified signed file
            //Q: How do I determine if this is a signed file to begin with? if it's not, an Unhandled exception occurs

            //System.Security.Cryptography.X509Certificates.X509Certificate cert = System.Security.Cryptography.X509Certificates.X509Certificate.Create​From​Signed​File(file);

            //X509Certificate cert = X509Certificate.Create​From​Signed​File(file);
            // ???
            try
            {
                X509Certificate cert = X509Certificate.Create​From​Signed​File(file);
                //Convert certificate from file into x509Certificate2 object
                //System.Security.Cryptography.X509Certificates.X509Certificate2 x509 =  new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);
                X509Certificate2 x509 = new X509Certificate2(cert);


#if DEBUG
                if (globalService.debug)
                {
                    // Print to console - information contained in the certificate.

                    byte[] rawdata = x509.RawData;
                    Console.WriteLine("\n**************************** x509 Certificate Fields ***************************");
                    Console.WriteLine("*** Subject: {1}", Environment.NewLine, x509.Subject);
                    Console.WriteLine("*** Issuer: {1}", Environment.NewLine, x509.Issuer);
                    Console.WriteLine("*** Version: {1}", Environment.NewLine, x509.Version);
                    Console.WriteLine("*** Valid Date: {1}", Environment.NewLine, x509.NotBefore);
                    Console.WriteLine("*** Expiry Date: {1}", Environment.NewLine, x509.NotAfter);
                    Console.WriteLine("*** Thumbprint: {1}", Environment.NewLine, x509.Thumbprint);
                    Console.WriteLine("*** Serial Number: {1}", Environment.NewLine, x509.SerialNumber);
                    Console.WriteLine("*** Friendly Name: {1}", Environment.NewLine, x509.PublicKey.Oid.FriendlyName);
                    //Console.WriteLine("*** Public Key Format: {1}", Environment.NewLine, x509.PublicKey.EncodedKeyValue.Format(true));
                    Console.WriteLine("*** Raw Data Length: {1}", Environment.NewLine, x509.RawData.Length);
                    //Console.WriteLine("*** Certificate to string: {1}", Environment.NewLine, x509.ToString(true));
                    Console.WriteLine("*** Certificate to XML String: {1}", Environment.NewLine, x509.PublicKey.Key.ToXmlString(false));
                    Console.WriteLine("### Signature Algorithm.FriendlyName: {1}", Environment.NewLine, x509.SignatureAlgorithm.FriendlyName);
                    Console.WriteLine("### Signature Algorithm.Value: {1}", Environment.NewLine, x509.SignatureAlgorithm.Value);
                    Console.WriteLine("### FriendlyName: {1}", Environment.NewLine, x509.FriendlyName);
                    Console.WriteLine("### Issuer Name: {1}", Environment.NewLine, x509.IssuerName);
                    Console.WriteLine("### GetKeyAlgorithm: {1}", Environment.NewLine, x509.GetKeyAlgorithmParametersString());
                    //Console.WriteLine("### Content Type: {1}", x509.GetCertContentType(rawdata));
                    Console.WriteLine("########################   End of x503Certificate2 output  #################################\n\n");
                }
#endif

#if DEBUG
                if (globalService.debug)
                {
                    // Display the value to the console.
                    string resultsTrue = cert.ToString(true);
                    Console.WriteLine("resultsTrue: {0}", resultsTrue);

                    string resultsFalse = cert.ToString(false);
                    Console.WriteLine("resultsFalse: {0}", resultsFalse);
                }
#endif




                // Mike's suggestion: 2022_03_10
                //Assert.Equals(x509.Subject, SUBJECT);


                if (globalService.debug)
                {
                    Console.WriteLine("\n*** Signature Results ************");
                    Console.WriteLine("Filename: {0}", file);
                }


                // Check SUBJECT
                if (x509.Subject.Contains(digSigCriteria.SUBJECT))
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Subject: \t{0} \n\tFound:\t\t{1}  \n\tExpected:\t{2}", validCheck, x509.Subject, digSigCriteria.SUBJECT);
                }


                // Check ISSUER
                if (x509.Issuer.Contains(digSigCriteria.ISSUER))
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Issuer: \t{0}\n\tFound:\t\t{1}\n\tExpected:\t{2}", validCheck, x509.Issuer, digSigCriteria.ISSUER);
                }



                // check ALGORITHM
                if (x509.SignatureAlgorithm.FriendlyName.Contains(digSigCriteria.SIGNATURE_ALGORITHM))
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Algorithm: \t{0}\n\tFound:\t\t{1}\n\tExpected:\t{2}", validCheck, x509.SignatureAlgorithm.FriendlyName, digSigCriteria.SIGNATURE_ALGORITHM);
                }
                //Assert.AreEqual(x509.Issuer, digSigCriteria.ISSUER);
                string temp1 = (x509.SignatureAlgorithm.FriendlyName);

                // Check VERSION
                if (x509.Version == digSigCriteria.VERSION)
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Version: \t{0}\n\tFound:\t\t{1}\n\tExpected:\t{2}", validCheck, x509.Version, digSigCriteria.VERSION);

                }
                //Assert.AreEqual(x509.Version, digSigCriteria.VERSION);


                // Check START DATE
                if (x509.NotBefore >= (DateTime.Parse(digSigCriteria.VALID_DATE)))
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Valid From: {0}\n\tFound:\t\t{1}\n\tExpected:\t{2}", validCheck, x509.NotBefore, digSigCriteria.VALID_DATE);
                }

                // Check END DATE
                if (x509.NotAfter <= (DateTime.Parse(digSigCriteria.EXPIRY_DATE)))
                    validCheck = true;
                else
                {
                    validCheck = false;
                    validSignature = "FALSE";
                }

                if (globalService.debug)
                {
                    Console.WriteLine("*** Valid To: \t{0}\n\tFound:\t\t{1}\n\tExpected:\t{2}", validCheck, x509.NotAfter, digSigCriteria.EXPIRY_DATE);
                }

                return validSignature;

            }
            catch
            {
                if (globalService.debug)
                {
                    Console.WriteLine("[Error 100] Filename: {0} - NOT SIGNED", file);
                }
                return "NOT SIGNED";
            }

        }

    }
}

