using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text Name;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color selectColor = Color.cyan;

    void Start()
    {
        GetComponent<Button>()
            .onClick.AddListener(() =>
            {
                // ทำอะไรก็ได้เมื่อ click
                Debug.Log("Clicked: " + Name.text);
                SpawnSelector.SpawnCharacter?.Invoke(Name.text);
                CharacterCardList.Instance.Select(this);
            });
    }

    public void Setup(string name)
    {
        Name.text = name;
    }

    public void setSelect(bool select)
    {
        background.color = select ? selectColor : normalColor;
    }
}
