using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{
   void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("2D Hit object: " + hit.collider.name);
        }
        else
        {
            Debug.Log("2D Raycast did not hit anything.");
        }
    }
}

}
