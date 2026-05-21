using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardList : MonoBehaviour
{
    List<CharacterCardData> Characters = new();

    [SerializeField]
    private GameObject Card;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private SpawnSelector spawn;
    public static CharacterCardList Instance;
    private CharacterCard selectedCard;
 
    // Start is calListled once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateCard();
    }

    // Update is called once per frame
    void Update()
    {
         

    }

    public void CreateCard()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        foreach (var card in spawn.CharacterName)
        {
            var CharCard = Instantiate(Card, content).GetComponent<CharacterCard>();
            CharCard.Setup(card);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
    }

    public void Select(CharacterCard card)
    {
        if (selectedCard != null)
            selectedCard.setSelect(false);

        selectedCard = card;
        selectedCard.setSelect(true);
    }
}

[System.Serializable]
public class CharacterCardData
{
    public string name;
}
