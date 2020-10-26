using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public Text textOne, textTwo, textThree; 
    
    public Button btnOne;
    public Button btnTwo;
    public Button btnThree;

    private List<Texture> frames = new List<Texture>();
    private List<float> frameDelays = new List<float>();
    
    private float time = 0;
    private int index = 0;
    
    public  RawImage img;
    
    // Start is called before the first frame update
    private void Start()
    {
        textOne.text = "Thigmotropism";
        textTwo.text = "Geotropism";
        textThree.text = "Chomotropism";
        
        // Get the download size for each button.
        // If the download size is 0, file is already downloaded. 
        GetDownloadSize("Thigmotropism", handle =>
        {
            if (handle.Result == 0)
            {
                btnOne.GetComponentInChildren<Text>().text = "Play";
            }
            else
            {
                btnOne.GetComponentInChildren<Text>().text = "Download";
            }
        });
        
        GetDownloadSize("Geotropism", handle =>
        {
            if (handle.Result == 0)
            {
                btnTwo.GetComponentInChildren<Text>().text = "Play";
            }
            else
            {
                btnTwo.GetComponentInChildren<Text>().text = "Download";
            }
        });
        
        GetDownloadSize("Chomotropism", handle =>
        {
            if (handle.Result == 0)
            {
                btnThree.GetComponentInChildren<Text>().text = "Play";
            }
            else
            {
                btnThree.GetComponentInChildren<Text>().text = "Download"; 
            }
        });
        
        
        // When click the button, 
        btnOne.onClick.AddListener(() =>
        {
            if (btnOne.GetComponentInChildren<Text>().text == "Download")
            {
                StartCoroutine(DownloadFile("Thigmotropism", btnOne.GetComponentInChildren<Text>(), handler =>
                {
                    handler.Completed += handle =>
                    {
                        btnOne.GetComponentInChildren<Text>().text = "Play";
                    };
                }));
            }
            else
            {
                LoadGifImage("Thigmotropism");
            }
        });
        
        
        btnTwo.onClick.AddListener((() =>
        {
            if (btnTwo.GetComponentInChildren<Text>().text == "Download")
            {
                StartCoroutine(DownloadFile("Geotropism", btnTwo.GetComponentInChildren<Text>(), handler =>
                {
                    handler.Completed += handle =>  btnTwo.GetComponentInChildren<Text>().text = "Play";
                }));
            }
            else
            {
                LoadGifImage("Geotropism");
            }
        }));
        
        btnThree.onClick.AddListener((() =>
        {
            if (btnThree.GetComponentInChildren<Text>().text == "Download")
            {
                StartCoroutine(DownloadFile("Chomotropism", btnThree.GetComponentInChildren<Text>(), handler =>
                {
                    handler.Completed += handle => btnThree.GetComponentInChildren<Text>().text = "Play";
                }));
            }
            else
            {
                LoadGifImage("Chomotropism");
            }
        }));
        
    }

    // Get the download size for the given key 
    private void GetDownloadSize(string key, Action<AsyncOperationHandle<long>> handler)
    {
        Addressables.GetDownloadSizeAsync(key).Completed += handler;
    }

    // Start download the file and display the progress 
    private IEnumerator DownloadFile(string key, Text displayText,  Action<AsyncOperationHandle> handle)
    {
        var handler = Addressables.DownloadDependenciesAsync(key);

        while (!handler.IsDone)
        {
            displayText.text = "Downloading: " + (handler.GetDownloadStatus().Percent * 100).ToString("00") + "%";

            yield return null; 
        }
        
        handle(handler);
    }

    // Load gif image from the local addressable storage location 
    private void LoadGifImage(string key)
    {
        Addressables.LoadAssetAsync<TextAsset>(key).Completed += handle =>
        {
            DecodeGif(handle.Result.bytes);
        };
    }

    // Decode gif file to textures frame list
    private void DecodeGif(byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        
        frames = new List<Texture>();
        frameDelays = new List<float>();
        
        // To stop playing previous gif 
        index = 0;

        using (var gifStream = new GifStream(ms))
        {
            while (gifStream.HasMoreData)
            {
                switch (gifStream.CurrentToken)
                {
                    case GifStream.Token.Image:
                        // var image = gifStream.ReadImage();
                        // do something with image
                        // This code is copy from Unitylist
                        var image = gifStream.ReadImage();
                        var frame = new Texture2D(
                            gifStream.Header.width, 
                            gifStream.Header.height, 
                            TextureFormat.ARGB32, false); 

                        frame.SetPixels32(image.colors);
                        frame.Apply();

                        frames.Add(frame);
                        frameDelays.Add(image.SafeDelaySeconds); // More about SafeDelay below
                        
                        break;
                            
                    case GifStream.Token.Comment:
                        var comment = gifStream.ReadComment();
                        // log this comment
                        break;
        
                    default:
                        gifStream.SkipToken();
                        // this token has no use for you, skip it
                        break;
                }
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Play gif image 
        if (frames.Count > 0)
        {
            time += Time.deltaTime;
            if (time > frameDelays[index])
            {
                time = 0;
                index = index + 1 >= frameDelays.Count ? 0 : index + 1;
            }

            img.texture = frames[index];
        }
    }
}
