# amazon-s3-rest-net
A port of Edd Turtles Direct Upload to Amazon S3 for ASP.NET

Thank you to Edd Turtle for the inspiration, you can find his works at: http://www.designedbyaturtle.co.uk/2015/direct-upload-to-s3-using-aws-signature-v4-php/

This project has been developed using the new ASP.NET 5 framework, currently on beta8 (will update periodically for new version releases).

It is intended that the Kestrel web server will be used with this project, and therefore I haven't included any references for IIS (you can add these in yourself into the project.json file if you wish). To use, change the AWSKey and AWSSecret values in config.json. You will also need to open HomeController.cs and change the Bucket Name and the Region your bucket is running in.

Other than that you should be set to go - Edd's original work helped me massively when trying to implement the same functionality for my ASP.NET project.
