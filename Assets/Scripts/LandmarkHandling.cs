using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR.WSA.WebCam; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI; 

 
public class LandmarkHandling : MonoBehaviour 
{
    Text text;
    GameObject menuPanels;
    string mon;
    Resolution cameraResolution;
    PhotoCapture photoCapt; 
    public string m_apiKey; 
    public GameObject panel;
    public GameObject PrimaryInterface;
    AudioSource audiop;

    public void Start()
    {
        panel.SetActive(false);
        text = GameObject.Find("LandMarkName").GetComponent<Text>(); 
        text.text = ""; 
        PrimaryInterface.SetActive(true);
        audiop = GetComponent<AudioSource>();
    } 
    public void StartTheFunction()
    {
        
        audiop.Play();
        PrimaryInterface.SetActive(false); 
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCapt = captureObject;
             
            CameraParameters cameraPar = new CameraParameters();
            cameraPar.hologramOpacity = 0.0f;
            cameraPar.cameraResolutionWidth = cameraResolution.width;
            cameraPar.cameraResolutionHeight = cameraResolution.height;
            cameraPar.pixelFormat = CapturePixelFormat.JPEG;

            photoCapt.StartPhotoModeAsync(cameraPar, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                PictureShoot();
            });
        });
    } 

    void OnDestroy()
    {
        if(photoCapt != null) {
            photoCapt.StopPhotoModeAsync(
          delegate (PhotoCapture.PhotoCaptureResult res)
          {
              photoCapt.Dispose();
              photoCapt = null; 
          }
        );
        }
    }

     
    void PictureShoot()
    {

        if (photoCapt != null)
        {
            photoCapt.TakePhotoAsync(delegate (PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
            {
                List<byte> buffer = new List<byte>();
                Matrix4x4 cameraToWorldMatrix;
                photoCaptureFrame.CopyRawImageDataIntoBuffer(buffer);


                if (!photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix))
                {
                    return;
                }  
                StartCoroutine(PhotoUploading(buffer.ToArray()));
            });
        }
    }
     
    UnityWebRequest RequestCreation(byte[] photo)
    {

        DownloadHandler download = new DownloadHandlerBuffer();

        string base64image = Convert.ToBase64String(photo);
        string json = "{\"requests\": [{\"image\": {\"content\": \"" + base64image + "\"},\"features\": [{\"type\": \"LANDMARK_DETECTION\",\"maxResults\": 5}]}]}";
        byte[] content = Encoding.UTF8.GetBytes(json);
        UploadHandler upload = new UploadHandlerRaw(content);
        string url = "https://vision.googleapis.com/v1/images:annotate?key=" + m_apiKey;
        UnityWebRequest www = new UnityWebRequest(url, "POST", download, upload);
        www.SetRequestHeader("Content-Type", "application/json");

        return www;
         
    }

    IEnumerator PhotoUploading(byte[] photo)
    {
        using (UnityWebRequest www = RequestCreation(photo))
        {
             
            yield return www.Send();

            if (www.isError)
            {
                Debug.Log("errore" + www.error);
            }
            else
            { 
                string jsonString = www.downloadHandler.text;
              
                JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));

                int count = 0;
                mon = null;
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        count++;
                        if (count == 6)
                            mon = reader.Value.ToString(); 
                    }
                } 

                if (mon != null) { 
                               text.text = mon;
                            }else
                                {
                                text.text = "Not found, Sorry. Retry.";
                                 }
                if (!text.text.Equals("Not found, Sorry. Retry.")) { 
                    OnDestroy();
                    StartCoroutine(LateCall());
                }
            }
        }
    }
    IEnumerator LateCall()
    {
        yield return new WaitForSeconds(2);
        panel.SetActive(true); 
    } 
      
}
 