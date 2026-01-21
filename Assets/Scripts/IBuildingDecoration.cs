using UnityEngine;

public interface IBuildingDecoration
{
    void Generate(Vector3 position, Transform parent, Material[] materials);
}
