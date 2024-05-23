using UnityEngine;


[CreateAssetMenu(fileName = "New SpellBook", menuName = "Spell System/SpellBook")]
public class SpellBook : ScriptableObject
{
    [System.Serializable]
    public struct Spell
    {
        public string name;
        public int amountInDeck;
        public bool isUnlocked;
        public int minLevel;

        [Header("Basic")]
        public GameObject basicSpellPrefab;
        public GameObject basicSpellPreview;
        public Texture basicCardSprite;
        public int basicManaCost;
        public float basicCooldownDelay;

        [Header("Flawless")]
        public int flawlessMasteryRequirement;
        public GameObject flawlessSpellPrefab;
        public GameObject flawlessSpellPreview;
        public Texture flawlessCardSprite;
        public int flawlessManaCost;
        public float flawlessCooldownDelay;

        [Header("Masterful")]
        public int masterfulMasteryRequirement;
        public GameObject masterfulSpellPrefab;
        public GameObject masterfulSpellPreview;
        public Texture masterfulCardSprite;
        public int masterfulManaCost;
        public float masterfulCooldownDelay;
    }


    public Spell[] spells;


    public void Initialize()
    {
        for (int i = 0; i < spells.Length; i++)
        {
            Spell spell = spells[i];
            spell.isUnlocked = false;
            spell.amountInDeck = 0;
            spells[i] = spell;
        }

        AddSpell("Fireball", 3);
        AddSpell("Fire Storm", 1);

        // Testing
        //AddSpell("Fireball", 1);
        //AddSpell("Fire Storm", 1);
        //AddSpell("Shield", 1);
        //AddSpell("Heal", 1);
        //AddSpell("Meteorite", 1);
        //AddSpell("Fire Burst", 1);
        //AddSpell("Freeze", 1);
        //AddSpell("Empower", 1);
        //AddSpell("Refresh", 1);
        //AddSpell("Fire Volley", 1);
        //AddSpell("Reset", 1);
        //AddSpell("Mana Flow", 1);
        //AddSpell("Teleport", 1);
        //AddSpell("Arcane Fury", 1);
    }


    public void AddSpell(string spellName, int amount = 1)
    {
        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i].name == spellName)
            {
                Spell spell = spells[i];
                spell.isUnlocked = true;
                spell.amountInDeck += amount;
                spells[i] = spell;
                break;
            }
        }

        PlayerPrefs.SetInt("BasicMastery_" + spellName, 1);
        PlayerPrefs.Save();
    }
}
