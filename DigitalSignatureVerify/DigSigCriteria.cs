using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureVerify
{
    public class DigSigCriteria
    {


        /* Example of fields in file
        public string SUBJECT = "CN=Sage Software Inc.";
        public string ISSUER = "CN=DigiCert Trusted G4 Code Signing RSA4096 SHA384 2021 CA1";
        public string VALID_DATE = "7/16/2018 5:00:00 PM";
        public string EXPIRY_DATE = "7/21/2021 5:00:00 AM";
        public string SIGNATURE_ALGORITHM = "sha256RSA";
        public int VERSION = 3;
        */

        //FIELDS

        //PROPERTIES
        public string SUBJECT { get; set; }
        public string ISSUER { get; set; }
        public string VALID_DATE { get; set; }
        public string EXPIRY_DATE { get; set; }
        public string SIGNATURE_ALGORITHM { get; set; }
        public int VERSION { get; set; }

        //CONSTRUCTORS

        //**********************************************************************
        // DigSigCriteria
        // Purpose:
        //      List which fields in a certificate to check
        //
        // Input: 
        //      string      file pointing to list of criteria
        //
        // Notes:
        //      blank lines will be ignored
        //      fields can be in any order
        //      The user can have the utility ignore specific fields (if line starts with "#")
        //
        public DigSigCriteria(string digSigCriteriaFile)
        {

            string[] lines = System.IO.File.ReadAllLines(digSigCriteriaFile);

            //set default values
            SUBJECT = "";
            ISSUER = "";
            VALID_DATE = "1/1/2000 5:00:00 PM";
            EXPIRY_DATE = "12/31/2099 5:00:00 AM";
            SIGNATURE_ALGORITHM = "";
            VERSION = 3;


            string[] criteriaToken;
            string expectedValue;
            string criteria;
            foreach (string line in lines)
            {
                if (String.IsNullOrEmpty(line))
                {
                    //empty string
                }
                else if (line[0] == '#')
                {
                    // line is a comment; ignore line
                }
                else
                {
                    criteriaToken = line.Split("=");
                    criteria = criteriaToken[0].Trim('"', '/', ' ');
                    expectedValue = criteriaToken[1].Trim('"', '/', ' ');

                    //switch (criteriaToken[0])
                    switch (criteria)
                    {
                        case "SUBJECT":
                            SUBJECT = expectedValue;
                            break;
                        case "ISSUER":
                            ISSUER = expectedValue;
                            break;
                        case "VALID_DATE":
                            VALID_DATE = expectedValue;
                            break;
                        case "EXPIRY_DATE":
                            EXPIRY_DATE = expectedValue;
                            break;
                        case "SIGNATURE_ALGORITHM":
                            SIGNATURE_ALGORITHM = expectedValue;
                            break;
                        case "VERSION":
                            VERSION = Int32.Parse(expectedValue);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        //METHODS


    }
}
