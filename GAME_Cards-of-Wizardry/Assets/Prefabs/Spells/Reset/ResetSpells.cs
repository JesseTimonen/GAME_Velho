using UnityEngine;

public class ResetSpells : MonoBehaviour
{
    private SpellCard card1;
    private SpellCard card2;
    private SpellCard card3;
    private SpellCard card4;
    private SpellCard card5;
    private SpellCard card6;
    private SpellCard card7;

    private void Start()
    {
        card1 = GameObject.Find("UI/Cards/Card 1").GetComponent<SpellCard>();
        card2 = GameObject.Find("UI/Cards/Card 2").GetComponent<SpellCard>();
        card3 = GameObject.Find("UI/Cards/Card 3").GetComponent<SpellCard>();
        card4 = GameObject.Find("UI/Cards/Card 4").GetComponent<SpellCard>();
        card5 = GameObject.Find("UI/Cards/Card 5").GetComponent<SpellCard>();
        card6 = GameObject.Find("UI/Cards/Wis 10 Card").GetComponent<SpellCard>();
        card7 = GameObject.Find("UI/Cards/Wis 20 Card").GetComponent<SpellCard>();

        SpellCard[] cards = new SpellCard[] { card1, card2, card3, card4, card5, card6, card7 };

        foreach (SpellCard card in cards)
        {
            if (card.cooldownTimeRemaining > 0)
            {
                card.cooldownTimeRemaining = 0;
            }
        }

        // Give audio source time to play spell audio
        Invoke(nameof(DestroyGameObject), 3f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
