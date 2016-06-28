# amazon-s3-rest-net
A port of Edd Turtles Direct Upload to Amazon S3 for ASP.NET

Thank you to Edd Turtle for the inspiration, you can find his works at: http://www.designedbyaturtle.co.uk/2015/direct-upload-to-s3-using-aws-signature-v4-php/

This project is built against ASP.NET Core 1.0

To use, change the AWSKey and AWSSecret values in config.json. You will also need to open HomeController.cs and change the Bucket Name and the Region your bucket is running in.

Other than that you should be set to go - Edd's original work helped me massively when trying to implement the same functionality for my ASP.NET project.
