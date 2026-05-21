using System;
using UnityEngine;

public class GridObj : Grid
{
    [SerializeField]
    Texture2D NormalGrid;

    [SerializeField]
    Texture2D AttackGrid;

    [SerializeField]
    Texture2D MoveGrid;
  
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
        if (action.Equals("ShowAttack"))
        {
            block.SetTexture("_BaseMap", AttackGrid);
        }
        else if (action.Equals("ShowMove"))
        {
            block.SetTexture("_BaseMap", MoveGrid);
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
