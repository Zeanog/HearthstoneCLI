using System.Collections.Generic;
using Newtonsoft.Json;

public class Card {
    public enum ERarity {
        Common,
        Legendary
    }

    public string Title;

    [JsonProperty("cost")]
    public int Cost {
        get;
        protected set;
    }

    [JsonProperty("rarity")]
    public ERarity Rarity = ERarity.Common;

    [JsonProperty("actions")]
    public List<AAction> m_Actions = new List<AAction>();

    public Card(string title, int cost, ERarity rarity)
    {
        Title = title;
        Cost = cost;
        Rarity = rarity;
    }

    public void AddAction(AAction action)
    {
        m_Actions.Add(action);
    }

    public void Play(Player self, Player otherPlayer)
    {
        System.Console.WriteLine();
        foreach (var action in m_Actions)
        {
            action.Execute(self, otherPlayer);
            System.Threading.Thread.Sleep(1000);
        }
    }

    public override string ToString()
    {
        using( var builderSlip = Neo.Utility.DataStructureLibrary<System.Text.StringBuilder>.Instance.CheckOut() )
        {
            builderSlip.Value.Clear();

            builderSlip.Value.AppendFormat("Card: {0}  ManaCost: {1}\n", Title, Cost);
            foreach ( var action in m_Actions)
            {
                builderSlip.Value.AppendLine(action.ToString());
            }

            return builderSlip.Value.ToString();
        }
    }
}