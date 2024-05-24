using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    private List<int> deck = new List<int>();
    private System.Random random = new System.Random();
    [SerializeField] private SpellBook spellBook;

    public void UpdateDeck()
    {
        deck.Clear();

        for (int i = 0; i < spellBook.spells.Length; i++)
        {
            for (int j = 0; j < spellBook.spells[i].amountInDeck; j++)
            {
                deck.Add(i);
            }
        }

        ShuffleDeck();
    }

    public int DrawNextCard()
    {
        if (deck.Count == 0)
        {
            UpdateDeck();
        }

        int cardIndex = deck[0];
        deck.RemoveAt(0);
        return cardIndex;
    }

    public void ShuffleDeck()
    {
        int n = deck.Count;
        for (int i = 0; i < n; i++)
        {
            int rnd = random.Next(i, n);
            int temp = deck[i];
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }
    }
}
