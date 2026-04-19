using UnityEngine;

public class GridTool : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    public static Vector3 HexToWorld(int col, int row, float hexSize = 1f)
    {
        float x = hexSize * 3f / 2f * col;
        // col คู่ → ต่ำกว่า, col คี่ → สูงกว่า
        float z = hexSize * Mathf.Sqrt(3f) * (row - (col % 2) * 0.5f);
        return new Vector3(x, 0, z);
    }

    // Update is called once per frame
    void Update() { }
}
