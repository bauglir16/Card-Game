using UnityEngine;

public class RenderTextureScreenshot : MonoBehaviour
{
    public Camera targetCamera; // Η κάμερα από την οποία θέλεις να τραβήξεις screenshot.
    public int resolutionWidth = 1920; // Πλάτος του screenshot.
    public int resolutionHeight = 1080; // Ύψος του screenshot.

    public void TakeScreenshot()
    {
        // Δημιουργία RenderTexture για την κάμερα.
        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        targetCamera.targetTexture = renderTexture;

        // Δημιουργία Texture2D για να αποθηκεύσεις την εικόνα.
        Texture2D screenshot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);

        // Απόδοση (render) της σκηνής στην RenderTexture.
        targetCamera.Render();

        // Ρύθμιση της RenderTexture ως ενεργή.
        RenderTexture.active = renderTexture;

        // Ανάγνωση της εικόνας από την RenderTexture και αποθήκευση στο Texture2D.
        screenshot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        screenshot.Apply();

        // Καθαρισμός.
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Αποθήκευση του screenshot ως PNG.
        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = $"{Application.persistentDataPath}/Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Screenshot saved to {filePath}");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeScreenshot();
        }
    }

}
