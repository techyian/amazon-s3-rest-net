using AmazonS3RESTNet5.Classes;
using AmazonS3RESTNet5.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AmazonS3RESTNet5.Services
{
    public class AmazonS3Service : IAmazonS3Service
    {
        //Options and settings
        public static string _Date = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture);

        public static string _ShortDate = DateTime.UtcNow.Date.ToString("yyyyMMdd");

        public const string REQUESTTYPE = "aws4_request";

        public const string FULLALGORITHM = "AWS4-HMAC-SHA256";

        public const string SERVICE = "s3";

        public const string EXPIRES = "86400";

        public const string SUCCESSSTATUS = "201";

        public const string SCHEME = "AWS4";

        public const string TERMINATOR = "aws4_request";

        private IOptions<Amazon> _Settings { get; set; }

        public AmazonS3Service(IOptions<Amazon> settings)
        {
            _Settings = settings;
        }

        /// <summary>
        /// Responsible for generating the Amazon S3 policy json string and also signing the signature request.
        /// </summary>
        /// <param name="s3Bucket"></param>
        /// <param name="region"></param>
        /// <param name="folder"></param>
        /// <param name="contentType"></param>
        /// <param name="aclType"></param>
        /// <returns>A List of KeyValuePairs containing the HTML form inputs</returns>
        public List<KeyValuePair<string, string>> GetS3Details(string s3Bucket, string region, string folder = "", string contentType = "", string aclType = "private")
        {
            //Migrated from http://www.designedbyaturtle.co.uk/2015/direct-upload-to-s3-using-aws-signature-v4-php/
            string url = "//" + s3Bucket + "." + SERVICE + '-' + region + ".amazonaws.com";
            string awsSecret = _Settings.Value.AWSSecret;
            string awsKey = _Settings.Value.AWSKey;

            // Step 1: Generate the Scope
            string[] scope = new string[] { awsKey, _ShortDate, region, SERVICE, REQUESTTYPE };
            string credentials = string.Join("/", scope);

            // Step 2: Making a Base64 Policy
            AmazonPolicy policy = new AmazonPolicy();
            policy.expiration = DateTime.UtcNow.AddHours(6).ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            policy.conditions = GeneratePolicy(s3Bucket, aclType, credentials);

            string jsonEncoded = JsonConvert.SerializeObject(policy);

            jsonEncoded = jsonEncoded.Replace("x_amz_credential", "x-amz-credential")
                                                                     .Replace("x_amz_algorithm", "x-amz-algorithm")
                                                                     .Replace("x_amz_date", "x-amz-date")
                                                                     .Replace("x_amz_expires", "x-amz-expires");

            string base64Policy = Base64Encode(jsonEncoded);

            // Step 3: Signing your Request (Making a Signature)
            var hmacsha256 = new HMACSHA256(DeriveSigningKey(awsSecret, region, _ShortDate, SERVICE));
            var signature = ToHexString(hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(base64Policy)), true);

            // Step 4: Build form inputs
            // This is the data that will get sent with the form to S3
            List<KeyValuePair<string, string>> inputs = GenerateAmazonInputs(contentType, aclType, base64Policy, credentials, signature, folder, url);

            return inputs;
        }

        /// <summary>
        /// Generating the Amazon policy json string which is used in order to sign the signature request.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="aclType"></param>
        /// <param name="credentials"></param>
        /// <returns>List of objects which are mostly interpreted as strings in order to generate the correct json markup when converted.</returns>
        private List<object> GeneratePolicy(string bucketName, string aclType, string credentials)
        {
            List<object> conditions = new List<object>();
            conditions.Add(new { bucket = bucketName });
            conditions.Add(new { acl = aclType });
            conditions.Add(new string[] { "starts-with", "$key", "" });
            conditions.Add(new string[] { "starts-with", "$Content-Type", "" });
            conditions.Add(new { success_action_status = SUCCESSSTATUS });
            conditions.Add(new { x_amz_credential = credentials });
            conditions.Add(new { x_amz_algorithm = FULLALGORITHM });
            conditions.Add(new { x_amz_date = _Date });
            conditions.Add(new { x_amz_expires = EXPIRES });

            return conditions;
        }

        /// <summary>
        /// Generating the Amazon HTML form inputs. 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="aclType"></param>
        /// <param name="policy"></param>
        /// <param name="credentials"></param>
        /// <param name="signature"></param>
        /// <param name="folder"></param>
        /// <param name="url"></param>
        /// <returns>List of KeyValuePairs with string keys and values with the HTML input names and values respectively. </returns>
        private List<KeyValuePair<string, string>> GenerateAmazonInputs(string contentType, string aclType, string policy, string credentials, string signature, string folder, string url)
        {
            List<KeyValuePair<string, string>> inputs = new List<KeyValuePair<string, string>>();

            inputs.Add(new KeyValuePair<string, string>("Content-Type", contentType));
            inputs.Add(new KeyValuePair<string, string>("acl", aclType));
            inputs.Add(new KeyValuePair<string, string>("success_action_status", SUCCESSSTATUS));
            inputs.Add(new KeyValuePair<string, string>("policy", policy));
            inputs.Add(new KeyValuePair<string, string>("X-amz-credential", credentials));
            inputs.Add(new KeyValuePair<string, string>("X-amz-algorithm", FULLALGORITHM));
            inputs.Add(new KeyValuePair<string, string>("X-amz-date", _Date));
            inputs.Add(new KeyValuePair<string, string>("X-amz-expires", EXPIRES));
            inputs.Add(new KeyValuePair<string, string>("X-amz-signature", signature));
            inputs.Add(new KeyValuePair<string, string>("key", folder));
            inputs.Add(new KeyValuePair<string, string>("url", url));

            return inputs;
        }

        /// <summary>
        /// Convert string text into UTF8 Base 64 encoded string.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>Base64 encoded string</returns>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Compute and return the multi-stage signing key for the request.
        /// </summary>
        /// <param name="algorithm">Hashing algorithm to use</param>
        /// <param name="awsSecretAccessKey">The clear-text AWS secret key</param>
        /// <param name="region">The region in which the service request will be processed</param>
        /// <param name="date">Date of the request, in yyyyMMdd format</param>
        /// <param name="service">The name of the service being called by the request</param>
        /// <returns>Computed signing key</returns>
        protected byte[] DeriveSigningKey(string awsSecretAccessKey, string region, string date, string service)
        {
            const string ksecretPrefix = SCHEME;

            byte[] ksecret = Encoding.UTF8.GetBytes((ksecretPrefix + awsSecretAccessKey).ToCharArray());
            byte[] hashDate = ComputeKeyedHash(ksecret, Encoding.UTF8.GetBytes(date));
            byte[] hashRegion = ComputeKeyedHash(hashDate, Encoding.UTF8.GetBytes(region));
            byte[] hashService = ComputeKeyedHash(hashRegion, Encoding.UTF8.GetBytes(service));
            return ComputeKeyedHash(hashService, Encoding.UTF8.GetBytes(TERMINATOR));
        }

        /// <summary>
        /// Compute and return the hash of a data blob using the specified algorithm
        /// and key
        /// </summary>
        /// <param name="algorithm">Algorithm to use for hashing</param>
        /// <param name="key">Hash key</param>
        /// <param name="data">Data blob</param>
        /// <returns>Hash of the data</returns>
        protected byte[] ComputeKeyedHash(byte[] key, byte[] data)
        {
            using (var hmacsha256 = new HMACSHA256(key))
            {
                return hmacsha256.ComputeHash(data);
            }
        }

        /// <summary>
        /// Helper to format a byte array into string
        /// </summary>
        /// <param name="data">The data blob to process</param>
        /// <param name="lowercase">If true, returns hex digits in lower case form</param>
        /// <returns>String version of the data</returns>
        public static string ToHexString(byte[] data, bool lowercase)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
            }
            return sb.ToString();
        }

    }
}
