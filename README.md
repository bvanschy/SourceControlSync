# SourceControlSync
An ASP.NET Web API service that accepts Web Hooks from Visual Studio Online source control and pushes changes to Amazon S3

 - Copyright 2015: Brian VanSchyndel, bvan.ca

Setup Instructions
======
1. Deploy the web service with HTTPS security (e.g., in Azure Web Apps)

#### In AWS web console
1. Add a user with AmazonS3FullAccess permission and write down the access key ID and secret

#### In Visual Studio Online web console
1. Go to a user's profile and select the Security tab

1. Add a Personal access token with Code (read) permission and write down the access token

1. Select a Git project and go to Settings

1. Under the Service Hooks tab add a Web Hooks service

1. Select the "Code pushed" trigger and add any filters

1. Click Next

1. Enter the web service URL

1. Add the following headers, substitute appropriate values and credentials:

  ```
  Sync-Root:/
  Sync-SourceConnectionString:BaseUrl=https://<<user>>.visualstudio.com/DefaultCollection/;UserName=<<VSO username>>;AccessToken=<<VSO access token>>
  Sync-DestConnectionString:BucketName=<<S3 bucket name>>;RegionSystemName=<<us-east-1>>;AccessKeyId=<<AWS access key ID>>;SecretAccessKey=<<AWS secret key>>
  ```

1. Set "Messages to send" and "Detailed messages to send" to None

1. Click Finish

HTTP Headers
======
#### Sync-Root
This header indicates the source control folder to sync.

For example, setting `Sync-Root:/wwwroot/` with the following source will sync only the files in `/wwwroot/`.

From Git:
```
/Readme.txt
/wwwroot/index.html
/wwwroot/images/logo.jpg
```

To AWS S3:
```
index.html
images/logo.jpg
```

#### RegionSystemName
This connection string property indicates the region of the AWS S3 bucket.

The system name can be found in the endpoint address.

For example, with Endpoint: bvan-test.s3-website-us-east-1.amazonaws.com, the region system name is `us-east-1`.
