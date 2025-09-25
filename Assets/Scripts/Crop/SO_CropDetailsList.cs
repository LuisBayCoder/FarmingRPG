using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDetailsList", menuName = "Scriptable Objects/Crop/Crop Details List")]
public class SO_CropDetailsList : ScriptableObject
{
    [SerializeField]
    public List<CropDetails> cropDetails;

    public CropDetails GetCropDetails(int seedItemCode)
    {
        CropDetails crop = cropDetails.Find(x => x.seedItemCode == seedItemCode);
        if (crop != null)
        {
            crop.ValidateArrays();
        }
        return crop;
    }
}
