using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonS3RESTNet5.Classes
{

        public class Amazon
        {
            public string AWSKey { get; set; }

            public string AWSSecret { get; set; }

        }

        public class AmazonPolicy
        {
            public string expiration { get; set; }

            public List<object> conditions { get; set; }
        }

        public class AmazonInputs
        {
            public string Url { get; set; }

            public KeyValuePair<string, string> Acl { get; set; }

            public KeyValuePair<string, string> Policy { get; set; }

            public KeyValuePair<string, string> ContentType { get; set; }

            public KeyValuePair<string, string> SuccessActionStatus { get; set; }

            public KeyValuePair<string, string> XAmzCredential { get; set; }

            public KeyValuePair<string, string> XAmzAlgorithm { get; set; }

            public KeyValuePair<string, string> XAmzDate { get; set; }

            public KeyValuePair<string, string> XAmzExpires { get; set; }

            public KeyValuePair<string, string> XAmzSignature { get; set; }
        }


}
