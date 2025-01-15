using UnityEngine;

public class RenderTextureScreenshot : MonoBehaviour
{
    public Camera targetCamera; // � ������ ��� ��� ����� ������ �� ��������� screenshot.
    public int resolutionWidth = 1920; // ������ ��� screenshot.
    public int resolutionHeight = 1080; // ���� ��� screenshot.

    public void TakeScreenshot()
    {
        // ���������� RenderTexture ��� ��� ������.
        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        targetCamera.targetTexture = renderTexture;

        // ���������� Texture2D ��� �� ������������ ��� ������.
        Texture2D screenshot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);

        // ������� (render) ��� ������ ���� RenderTexture.
        targetCamera.Render();

        // ������� ��� RenderTexture �� ������.
        RenderTexture.active = renderTexture;

        // �������� ��� ������� ��� ��� RenderTexture ��� ���������� ��� Texture2D.
        screenshot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        screenshot.Apply();

        // ����������.
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // ���������� ��� screenshot �� PNG.
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
