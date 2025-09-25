using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScale : MonoBehaviour
{
      public void SetScale(float scaleFactor)
    {
        // Convert the integer scaleFactor to a float and apply it
        float scale = scaleFactor; // Assuming scaleFactor is a float, if it's an int, you can cast it to float directly.
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
