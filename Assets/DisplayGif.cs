using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.UI;

public class DisplayGif : MonoBehaviour
{
    public TextAsset gifImg;

    private List<Texture> frames = new List<Texture>();
    private List<float> frameDelays = new List<float>();

    private float time = 0;
    private int index = 0;
    
    private  RawImage img;
    
    // Start is called before the first frame update
    private void Start()
    {
        img = GetComponent<RawImage>();
        
        Decode();
    }

    // Update is called once per frame
    private void Update()
    {
        if (frames.Count > 0)
        {
            time += Time.deltaTime;
            if (time > frameDelays[index])
            {
                time = 0;
                index = index + 1 >= frameDelays.Count ? 0 : index + 1;
            }

            img.texture = frames[index];

            // switch (renderMode)
            // {
            //     case RenderMode.MaterialOverride:
            //         targetRenderer.material.mainTexture = texture2DList[index];
            //         break;
            //     case RenderMode.UGUIImage:
            //         targetImage.sprite = sprites[index];
            //         break;
            // }
        }
    }

    private void Decode()
    {
        var ms = new MemoryStream(gifImg.bytes);
        
        frames = new List<Texture>();
        frameDelays = new List<float>();

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
}
