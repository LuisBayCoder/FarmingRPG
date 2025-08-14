using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeColliderOfObject : MonoBehaviour
{
    [SerializeField] GameObject objectToResizeCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalPosition;

    void Start()
    {
        // Store the original collider size
        StoreOriginalColliderData();
        
        // Wait a frame then find and resize new object
        StartCoroutine(WaitAndResizeNewObject());
    }
    
    void StoreOriginalColliderData()
    {
        if (objectToResizeCollider != null)
        {
            BoxCollider2D boxCollider = objectToResizeCollider.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                originalColliderSize = boxCollider.size;
                originalColliderOffset = boxCollider.offset;
                originalPosition = objectToResizeCollider.transform.position;
                Debug.Log($"Stored original collider size: {originalColliderSize}, offset: {originalColliderOffset}");
            }
        }
    }
    
    IEnumerator WaitAndResizeNewObject()
    {
        // Wait 0.1 seconds for the new object to appear
        yield return new WaitForSeconds(0.6f);
        
        // Find new object at the same location
        GameObject newObject = FindObjectAtPosition(originalPosition);
        Debug.Log($"Found new object: {newObject?.name} at position: {originalPosition}");
        if (newObject != null && newObject != objectToResizeCollider)
        {
            ApplyOriginalSizeToNewObject(newObject);
        }
    }
    
    GameObject FindObjectAtPosition(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (Collider2D col in colliders)
        {
            // Check if it's not the original object and has an Item script
            if (col.gameObject != objectToResizeCollider && col.gameObject.GetComponent<Item>() != null)
            {
                return col.gameObject;
            }
        }
        return null;
    }
    
    void ApplyOriginalSizeToNewObject(GameObject newObject)
    {
        BoxCollider2D boxCollider = newObject.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
            Debug.Log($"Applied original size {originalColliderSize} and offset {originalColliderOffset} to new object: {newObject.name}");
        }
    }
}
