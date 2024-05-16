using UnityEngine;
using System.IO;
using System;


public class Screenshot : MonoBehaviour
{
    #if !UNITY_WEBGL
    [Header("REFERENCES")]
    [SerializeField] private InputController inputController;

    [Header("SETTINGS")]
    [SerializeField] private string fileExtension = ".png";
    [SerializeField] private int detailMultiplier = 2;
    private string screenshotsFolder;


    private void Start()
    {
        InitializeScreenshotFolderPath();
        EnsureScreenshotFolderExists();
    }


    private void Update()
    {
        if (inputController.ScreenshotPressed)
        {
            TakeScreenshot();
        }
    }


    private void InitializeScreenshotFolderPath()
    {
        screenshotsFolder = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
            Application.productName,
            "Screenshots"
        );
    }


    private void EnsureScreenshotFolderExists()
    {
        try
        {
            if (!Directory.Exists(screenshotsFolder))
            {
                Directory.CreateDirectory(screenshotsFolder);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to create screenshot directory: " + e.Message);
        }
    }


    public void TakeScreenshot()
    {
        try
        {
            EnsureScreenshotFolderExists();
            string fullPath = GetUniqueFilePath("Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
            ScreenCapture.CaptureScreenshot(fullPath, detailMultiplier);
            Debug.Log("Screenshot saved: " + fullPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to take screenshot: " + e.Message);
        }
    }


    private string GetUniqueFilePath(string baseFileName)
    {
        int count = 1;
        string fileName;

        do
        {
            fileName = baseFileName + (count > 1 ? $"_{count}" : "") + fileExtension;
            count++;
        } while (File.Exists(Path.Combine(screenshotsFolder, fileName)));

        return Path.Combine(screenshotsFolder, fileName);
    }
    #endif
}
