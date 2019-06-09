// Pixel Perfect Rendering in Unity
// Source: Unity 2014 Unity 2D best practices
// (modified by noobtuts.com | vis2k (zoom factor))
using UnityEngine;

[ExecuteInEditMode]
public class PixelDensityCamera: MonoBehaviour
{
    // The value that all the Sprites use
    public float pixelsToUnits = 16;
    public float pixelsToUnitsMax = 100;
    public float pixelsToUnitsMin = 10;
    public float scaleSpeed = 1;

    // Zoom Factor
    public int zoom = 1;


    private void Update()
    {
        pixelsToUnits += Input.GetAxis("Mouse ScrollWheel") * scaleSpeed;
        if (pixelsToUnits < pixelsToUnitsMin)
        {
            pixelsToUnits = pixelsToUnitsMin;
        }
        else if (pixelsToUnits > pixelsToUnitsMax)
        {
            pixelsToUnits = pixelsToUnitsMax;
        }
        GetComponent<Camera>().orthographicSize = Screen.height / pixelsToUnits / zoom / 2;
    }
}