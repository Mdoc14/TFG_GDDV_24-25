using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Netcode;
using System;
using Unity.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using System.Collections;

public class ImageToTextureScript : MonoBehaviour
{
    private string protoFile = "/models/face/deploy.prototxt";
    private string modelFile = "/models/face/res10_300x300_ssd_iter_140000.caffemodel";
    public float confidenceThreshold = 0.6f;
    public int OUTPUT_SIZE = 1024;

    public void LoadAndProcessImage()
    {
        string imagePath;
        try
        {
            imagePath = LoadFileScript.PickImagePath();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar la imagen: " + e.Message);
            return;
        }
        if (!File.Exists(imagePath))
        {
            Debug.LogError("No se encontró la imagen en el path: " + imagePath);
            return;
        }

        byte[] bytes = File.ReadAllBytes(imagePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Mat img = new Mat(tex.height, tex.width, CvType.CV_8UC4);
        Utils.texture2DToMat(tex, img);
        Imgproc.cvtColor(img, img, Imgproc.COLOR_RGBA2RGB); // DNN solo acepta 3 canales

        string protoPath = Application.streamingAssetsPath + protoFile;
        string modelPath = Application.streamingAssetsPath + modelFile;

        if (!File.Exists(protoPath) || !File.Exists(modelPath))
        {
            Debug.LogError("No se encontraron los archivos del modelo. Asegúrate de colocarlos en StreamingAssets.");
            Debug.LogError("Ruta del prototxt: " + protoPath);
            Debug.LogError("Ruta del modelo: " + modelPath);
            return;
        }

        Net faceNet = Dnn.readNetFromCaffe(protoPath, modelPath);
        if (faceNet == null)
        {
            Debug.LogError("No se pudo cargar el modelo DNN.");
            return;
        }

        Mat inputBlob = Dnn.blobFromImage(img, 1.0, new Size(300, 300), new Scalar(104, 177, 123), false, false);
        faceNet.setInput(inputBlob);

        Mat detections = faceNet.forward();

        int detected = 0;
        for (int i = 0; i < detections.size(2); i++)
        {
            Mat detectionMat = detections.reshape(1, (int)detections.total() / 7);

            float confidence = (float)detectionMat.get(i, 2)[0];
            if (confidence > confidenceThreshold)
            {
                float x1 = (float)detectionMat.get(i, 3)[0] * img.cols();
                float y1 = (float)detectionMat.get(i, 4)[0] * img.rows();
                float x2 = (float)detectionMat.get(i, 5)[0] * img.cols();
                float y2 = (float)detectionMat.get(i, 6)[0] * img.rows();

                float faceWidth = x2 - x1;
                float faceHeight = y2 - y1;

                // Tamaño del cuadrado: el mayor entre ancho y alto del bounding box
                float squareSize = Mathf.Max(faceWidth, faceHeight);

                // Centro del rostro
                float centerX = x1 + faceWidth / 2f;
                float centerY = y1 + faceHeight / 2f;

                // Coordenadas del cuadrado centrado
                float cropX = centerX - squareSize / 2f;
                float cropY = centerY - squareSize / 2f;

                // Asegurarse de que no se sale de la imagen
                cropX = Mathf.Clamp(cropX, 0, img.cols() - squareSize);
                cropY = Mathf.Clamp(cropY, 0, img.rows() - squareSize);

                // Convertir a enteros para el rectángulo
                int intCropX = Mathf.RoundToInt(cropX);
                int intCropY = Mathf.RoundToInt(cropY);
                int intSize = Mathf.RoundToInt(squareSize);

                // Ajustar tamaño si está en el borde
                intSize = Mathf.Min(intSize, img.cols() - intCropX);
                intSize = Mathf.Min(intSize, img.rows() - intCropY);

                // Recorte cuadrado
                OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(intCropX, intCropY, intSize, intSize);
                Mat faceRegion = new Mat(img, cropRect);

                Mat squareFace = MakeSquare(faceRegion, OUTPUT_SIZE);

                Texture2D resultTexture = new Texture2D(squareFace.cols(), squareFace.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(squareFace, resultTexture);

                SaveTextureToStreamingAssets(resultTexture, "/recorteCara.png");

                Image imgUI = GetComponent<Image>();
                if (imgUI != null)
                {
                    imgUI.sprite = Sprite.Create(resultTexture, new UnityEngine.Rect(0, 0, resultTexture.width, resultTexture.height), new Vector2(0.5f, 0.5f));
                }
                detected++;
                break;
            }
        }

        if (detected == 0)
            Debug.LogWarning("No se detectaron caras con suficiente confianza.");
    }
    void SaveTextureToStreamingAssets(Texture2D texture, string fileName)
    {
        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            string savePath = Application.streamingAssetsPath + fileName;

            // Guardar el archivo PNG
            File.WriteAllBytes(savePath, pngData);

            Debug.Log("Imagen guardada en: " + savePath);
        }
        else
        {
            Debug.LogError("No se pudo codificar la textura a PNG.");
        }
    }

    Mat MakeSquare(Mat src, int size)
    {
        int maxDim = Mathf.Max(src.cols(), src.rows());
        Mat square = new Mat(new Size(maxDim, maxDim), src.type(), new Scalar(0, 0, 0));
        int xOffset = (maxDim - src.cols()) / 2;
        int yOffset = (maxDim - src.rows()) / 2;

        src.copyTo(new Mat(square, new OpenCVForUnity.CoreModule.Rect(xOffset, yOffset, src.cols(), src.rows())));

        Mat resized = new Mat();
        Imgproc.resize(square, resized, new Size(size, size));
        return resized;
    }

}
