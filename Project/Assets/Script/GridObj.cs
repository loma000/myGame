using System;
using UnityEngine;

public class GridObj : MonoBehaviour
{
    [SerializeField]
    Texture2D NormalGrid;

    [SerializeField]
    Texture2D SelectGrid;
    public int row;
    public int col;
    private Renderer render;
    private MaterialPropertyBlock block;

    void Awake()
    {
        render = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    public void SetGridTex(string action)
    {
        render.GetPropertyBlock(block);
        if (action.Equals("ShowMove"))
        {
            block.SetTexture("_BaseMap", SelectGrid);
        }
        else
        {
            block.SetTexture("_BaseMap", NormalGrid);
        }
        render.SetPropertyBlock(block);
    }

    // Update is called once per frame
    void Update() { }
}
