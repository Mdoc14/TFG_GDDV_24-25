using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageToTextureScript : MonoBehaviour
{
    /*[SerializeField] string imagePath;
    public MeshRenderer modelRenderer;
    public void GetImagePathFromLoad() // Función que guarda el path de la imagen cargada en una variable
    {
        imagePath = LoadPhotoScript.PickImagePath();
        ConvertImageToTexture();
    }

    public void ConvertImageToTexture()
    {
        

        Debug.Log("Convirtiendo textura: " + imagePath);

        if (!File.Exists(imagePath)){ Debug.LogError("No se encontró la imagen: " + imagePath); return;}

        // Cargar la imagen en una Texture2D
        byte[] imageBytes = File.ReadAllBytes(imagePath); // Convierte la imagen en un array de bytes mediante su path
        Texture2D initialTexture = new Texture2D(2, 2);
        if (!initialTexture.LoadImage(imageBytes)) { Debug.LogError("No se pudo cargar la imagen en Texture2D."); return; }

        // Se convierte la Texture2D a Mat
        Mat img = Texture2DToMat(initialTexture);

        if (img.Empty()) { Debug.LogError("No se pudo convertir la imagen en Mat."); }
        else { Debug.Log("Imagen convertida a Mat correctamente: " + img.Width + "x" + img.Height); }

        // Se convierte a escala de grises para mejorar la detección del rostro
        //Mat gray = new Mat();
        //Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        
        // Se carga el clasificador de caras
        CascadeClassifier faceCascade = new CascadeClassifier(Application.dataPath + "/OpenCV+Unity/Assets/haar_cascade.xml");
        if (faceCascade.Empty())
        {
            Debug.LogError("No se pudo cargar el clasificador de caras.");
            return;
        }

        // Detectar caras
        OpenCvSharp.Rect[] faces = faceCascade.DetectMultiScale(img, 1.1, 5, HaarDetectionType.ScaleImage, new Size(100, 100));
        if (faces.Length == 0)
        {
            Debug.LogError("No se detectaron caras en la imagen.");
            return;
        }

        // Extrae la cara de la imagen original
        OpenCvSharp.Rect faceRect = faces[0];

        // Ajusta el rectángulo de la cara 
        int xValue = 40; // Offset de la coordenada X
        int yValue = -20; // Offset de la coordenada Y
        int sizeValue = 40;
        int xPadding = Mathf.Min(xValue, faceRect.X); // Para evitar valores negativos
        int yPadding = Mathf.Min(yValue, faceRect.Y);

        OpenCvSharp.Rect adjustedFaceRect = new OpenCvSharp.Rect(
            faceRect.X - xPadding,
            faceRect.Y - yPadding,
            faceRect.Width + sizeValue * 2,  // Expandir el tamaño del ancho
            faceRect.Height + sizeValue * 2  // Expandir el tamaño de la altura
        );

        // Extraer la cara ajustada
        Mat faceImg = new Mat(img, adjustedFaceRect);

        Cv2.Rectangle(img, adjustedFaceRect, new Scalar(0, 255, 0), 2);

        // Hacer la imagen cuadrada
        Mat squareFace = ResizeToSquare(faceImg, 1024);

        // Convertirla a Texture2D para usarla en Unity
        Texture2D finalTexture = MatToTexture2D(squareFace);

        // Aplicar la textura en un material de un objeto
        ApplyTexture(finalTexture);
    }

    Mat ResizeToSquare(Mat src, int size)
    {
        // Determina las dimensiones necesarias para ajustar la imagen
        int maxDim = Mathf.Max(src.Width, src.Height);

        // Crear una nueva imagen cuadrada vacía (rellenada con un color de fondo, como el negro)
        Mat squareImg = new Mat(new OpenCvSharp.Size(maxDim, maxDim), src.Type(), new Scalar(0, 0, 0, 255));

        // Calcular los desplazamientos
        int xOffset = (maxDim - src.Width) / 2;
        int yOffset = (maxDim - src.Height) / 2;

        // Copiar la imagen original en la nueva imagen cuadrada
        OpenCvSharp.Rect roi = new OpenCvSharp.Rect(xOffset, yOffset, src.Width, src.Height);
        src.CopyTo(new Mat(squareImg, roi));

        // Asegurarse de que la imagen sea cuadrada
        Mat resizedMat = new Mat();
        Cv2.Resize(squareImg, resizedMat, new OpenCvSharp.Size(size, size), 0, 0, InterpolationFlags.Linear);

        return resizedMat;
    }

    void ApplyTexture(Texture2D texture)
    {
        if (modelRenderer != null && texture != null)
        {
            modelRenderer.material.mainTexture = texture;
            Debug.Log("Textura aplicada correctamente.");
            this.GetComponent<Image>().sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("No se pudo aplicar la textura.");
        }
    }

    Mat Texture2DToMat(Texture2D texture) // Este método convierte una textura en Mat para que OpenCV pueda leer la imagen
    {
        Color32[] pixels = texture.GetPixels32();

        byte[] data = new byte[pixels.Length * 4]; // Al estar en RGBA necesita 4 canales por pixel
        for (int i = 0; i < pixels.Length; i++)
        {
            data[i * 4] = pixels[i].b;     // Blue
            data[i * 4 + 1] = pixels[i].g; // Green
            data[i * 4 + 2] = pixels[i].r; // Red
            data[i * 4 + 3] = pixels[i].a; // Alpha
        }

        Mat mat = new Mat(texture.height, texture.width, MatType.CV_8UC4, data);

        return mat;
    }
    Texture2D MatToTexture2D(Mat mat)
    {
        if (mat.Empty())
        {
            Debug.LogError("El Mat está vacío al intentar convertirlo a Texture2D.");
            return null;
        }

        // Crear una textura del tamaño de la imagen
        Texture2D texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGBA32, false);
        Mat rotatedMat = new Mat();
        Cv2.Flip(mat, rotatedMat, FlipMode.X);

        // Convertir cada píxel del Mat a un Color32 en la textura
        for (int y = 0; y < rotatedMat.Height; y++)
        {
            for (int x = 0; x < rotatedMat.Width; x++)
            {
                Vec3b color = rotatedMat.At<Vec3b>(y, x);
                texture.SetPixel(x, rotatedMat.Height - 1 - y, new Color32(color[2], color[1], color[0], 255));
            }
        }

        texture.Apply();
        return texture;
    }

    */
}
