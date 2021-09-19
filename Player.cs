using System.Collections.Generic;

public class Player {
    public string Name;

    public int MaxHealth {
        get;
        protected set;
    }

    protected int m_CurrentHealth = 0;
    public int CurrentHealth {
        get {
            return m_CurrentHealth;
        }

        set {
            m_CurrentHealth = MathExtensions.Clamp(value, 0, MaxHealth);
        }
    }

    public int MaxMana {
        get;
        protected set;
    }

    protected int m_ManaCrystals = 0;
    public int ManaCrystals {
        get {
            return m_ManaCrystals;
        }

        set {
            m_ManaCrystals = MathExtensions.Clamp(value, 0, MaxMana);
        }
    }

    protected int m_ManaCount = 0;
    public int ManaCount {
        get {
            return m_ManaCount;
        }

        set {
            m_ManaCount = MathExtensions.Clamp(value, 0, m_ManaCrystals);
        }
    }

    public LinkedList<Card> Hand {
        get;
    } = new LinkedList<Card>();

    public Deck              Deck {
        get;
        protected set;
    }

    public Player( int maxHealth, int maxMana, Deck newDeck )
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        MaxMana = maxMana;
        Deck = newDeck;
        Hand.Clear();
    }

    public Card FindAt(int cardIndex)
    {
        if( cardIndex >= Hand.Count )
        {
            return null;
        }

        var currentNode = Hand.First;
        for (int ix = 0; ix < cardIndex; ++ix)
        {
            currentNode = currentNode.Next;
        }
        System.Diagnostics.Debug.Assert(currentNode != null);
        return currentNode.Value;
    }

    public bool PlayCard( int cardIndex, Player otherPlayer )
    {
        var currentCard = FindAt(cardIndex);
        if( currentCard.Cost > ManaCount )
        {
            System.Console.WriteLine(string.Format("Not enough mana to play {0}", currentCard.Title));
            System.Threading.Thread.Sleep(1000);
            return false;
        }

        Hand.Remove(currentCard);

        currentCard.Play(this, otherPlayer);
        ManaCount -= currentCard.Cost;
        return true;
    }

    public void DrawCards( int count )
    {
        for( int ix = 0; ix < count; ++ix )
        {
            if( Deck.IsEmpty )
            {
                CurrentHealth -= 1;
                continue;
            }

            Hand.AddLast(Deck.DrawTop());
        }
    }

    public void DiscardCards(int count)
    {
        for (int ix = 0; ix < count; ++ix)
        {
            if (Deck.IsEmpty)
            {
                CurrentHealth -= 1;
                continue;
            }

            Deck.DrawTop();
        }
    }

    public override string ToString()
    {
        using (var builderSlip = Neo.Utility.DataStructureLibrary<System.Text.StringBuilder>.Instance.CheckOut())
        {
            builderSlip.Value.Clear();

            builderSlip.Value.AppendFormat("Name: {0}\n", Name);
            builderSlip.Value.AppendFormat("Health: {0}/{1}\n", CurrentHealth, MaxHealth);
            builderSlip.Value.AppendFormat("Mana: {0}/{1}\n", ManaCount, ManaCrystals);
            builderSlip.Value.AppendFormat("Remaining Cards in Deck: {0}\n", Deck.Cards.Count);
            
            builderSlip.Value.AppendLine("Hand:");
            int cardIndex = 0;
            foreach (var card in Hand)
            {
                builderSlip.Value.AppendFormat("{0})  {1}", ++cardIndex, card.ToString());
            }

            return builderSlip.Value.ToString();
        }
    }
}