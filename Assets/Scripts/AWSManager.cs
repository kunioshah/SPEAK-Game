using System.IO;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//https://bergstrand-niklas.medium.com/how-to-get-started-with-aws-s3-cloud-storage-for-unity-160912c16f65
//https://medium.com/nerd-for-tech/using-aws-s3-with-unity-60d5eff26bec

public class AWSManager : MonoBehaviour
{
    private static AWSManager _instance;

    private readonly string awsKey = File.ReadAllText(Application.streamingAssetsPath + "/APIKeys/AWSCongnito.apikey");

    private AWSManager(){}
    public static AWSManager Instance
    {
        get{
            if (_instance == null) {
                Debug.Log("AWSManager is null");
            }

            return _instance;
        }
    }

    public string S3Region = Amazon.RegionEndpoint.USEast1.SystemName;
    private RegionEndpoint _S3RegionEndpoint
    {
        get{
            return RegionEndpoint.GetBySystemName(S3Region);
        }
    }

    private AmazonS3Client _S3Client;
    public AmazonS3Client S3Client{
        get{
            if (_S3Client == null){
                _S3Client = new AmazonS3Client(new CognitoAWSCredentials(awsKey, RegionEndpoint.USEast1), _S3RegionEndpoint);
            }

            return _S3Client;
        }
    }

    private void Awake(){
        _instance = this;
        awsKey = ((TextAsset)Resources.Load("AWSCongnito.apikey", typeof(TextAsset))).text;
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        S3Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                responseObject.Response.Buckets.ForEach((s3b) =>
                {
                    Debug.Log("Bucketname=" + s3b.BucketName);
                });
            }
            else
            {
                Debug.Log("AWS Error:" + responseObject.Exception);
            }
        });


    }

    public void UpLoadToS3(string filePath, string fileName){
        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        var request = new PostObjectRequest()
        {
            Bucket = "speakgamebucket",
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private
        };

        S3Client.PostObjectAsync(request, (response) =>
        {
            if (response.Exception == null)
            {
                Debug.Log("S3 file uploaded successfully " + fileName);
            }
            else
            {
                Debug.Log("Upload Exception: Status Code" + response.Response.HttpStatusCode + " Error:" + response.Exception);
            }
        }); 
    }
}