using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DeckLoader : DataLoader {
    public class CardDef {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("cost")]
        public int Cost;

        [JsonProperty("rarity")]
        public Card.ERarity Rarity = Card.ERarity.Common;

        [JsonProperty("actions")]
        public Dictionary<string, object> ActionDefs;
    }

    public class CardGroup {
        [JsonProperty("count")]
        public int Count;

        [JsonProperty("card")]
        public CardDef Card;
    }

    public static Deck LoadFrom( string absPath )
    {
        try
        {
            string encodedDeck = File.ReadAllText(absPath);
            var pseudoDeck = JsonConvert.DeserializeObject<List<CardGroup>>(encodedDeck, DataLoader.Settings);

            Deck deck = new Deck();
            foreach (var group in pseudoDeck)
            {
                for (int ix = 0; ix < group.Count; ++ix)
                {
                    Card card = new Card(group.Card.Name, group.Card.Cost, group.Card.Rarity);
                    foreach( var actionName in group.Card.ActionDefs.Keys ) {
                        var action = ActionFactory.Instance.CreateAction(actionName, group.Card.ActionDefs[actionName]);
                        card.AddAction(action);
                    }
                    
                    deck.AddCard(card);
                }
            }

            return deck;
        }
        catch( Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}

public class Deck {
    public LinkedList<Card> Cards {
        get;
    } = new LinkedList<Card>();

    public void     AddCard( Card card )
    {
        Cards.AddLast(card);
    }

    public void     Shuffle()
    {
        int cardsToShuffle = Cards.Count / 2;

        using (var rngSlip = Neo.Utility.DataStructureLibrary<Random>.Instance.CheckOut(DateTime.UtcNow.Millisecond))
        {
            for (int ix = 0; ix < cardsToShuffle; ++ix)
            {
                int newPlace = rngSlip.Value.Next(Cards.Count / 2, Cards.Count - 1);

                var cardNode = Cards.First;
                Cards.RemoveFirst();

                LinkedListNode<Card> currentNode = Cards.First;
                for (int iy = 0; iy < newPlace; ++iy)
                {
                    currentNode = currentNode.Next;
                }
                Cards.AddAfter(currentNode, cardNode);
            }
        }
    }

    public Card     DrawTop()
    {
        if(IsEmpty)
        {
            return null;
        }

        Card card = Cards.First.Value;
        Cards.RemoveFirst();
        return card;
    }

    public Card DrawRandom()
    {
        if (IsEmpty)
        {
            return null;
        }

        var rngSlip = Neo.Utility.DataStructureLibrary<Random>.Instance.CheckOut(DateTime.UtcNow.Millisecond);
        int newIndex = rngSlip.Value.Next(Cards.Count - 1);
        rngSlip.Dispose();

        LinkedListNode<Card> currentNode = Cards.First;
        for (int iy = 0; iy < newIndex; ++iy)
        {
            currentNode = currentNode.Next;
        }

        Cards.Remove(currentNode);
        return currentNode.Value;
    }

    public bool     IsEmpty {
        get {
            return Cards.Count <= 0;
        }
    }
}