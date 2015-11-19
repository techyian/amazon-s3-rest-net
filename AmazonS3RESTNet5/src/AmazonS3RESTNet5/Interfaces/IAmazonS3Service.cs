using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonS3RESTNet5.Interfaces
{
    public interface IAmazonS3Service
    {
        List<KeyValuePair<string, string>> GetS3Details(string s3Bucket, string region, string folder = "", string contentType = "", string acl = "private");
    }
}
