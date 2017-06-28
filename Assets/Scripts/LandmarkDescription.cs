using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class LandmarkDescription : MonoBehaviour
{
    private Text txtRef;
    Text text;

    void OnEnable()
    {
        text = GameObject.Find("LandMarkName").GetComponent<Text>();
        txtRef = GetComponent<Text>();
        txtRef.text = ""; 
        StartCoroutine(GetText());  
    }

     
    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&explaintext=1&titles="+ text.text );
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        { 
            JObject o = JObject.Parse(www.downloadHandler.text);   
           
            txtRef.text = "\n" + o["query"]["pages"].First.Last.Last.Last;

        }
    }
} 