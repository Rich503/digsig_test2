//#define DEBUG

// Couldn't use this using statement - gave build errors. Needed to explicitely list path to class in program below
//using System.​Security.​Cryptography.​X509Certificates;

namespace DigitalSignatureVerify
{
    


    class GlobalServiceClass             //RGD??? should this class be in it's own file?
    {
        public  bool debug;
        public  string extensions;
        public string[] extensionsArray = {  ".*"};

        //count of valid and invalid signatures
        //public int validSignatureCount;
        //public int invalidSignatureCount;
        //public int notSignedCount;
        //public int otherErrorCount;
        //public int illegalFileTypeCount;
        //public int fileNotExistCount;
    }
}
