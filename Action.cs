using System;
using System.Collections.Generic;

public abstract class AAction {
    public string Name;
    public abstract void Execute(Player self, Player otherPlayer);

    public AAction( string name )
    {
        Name = name;
    }
}

public class Action_PrintMessage : AAction {
    public string Message;

    public Action_PrintMessage(string name, string msg) : base(name)
    {
        Message = msg;
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        Console.WriteLine(Message);
    }

    public override string ToString()
    {
        using (var builderSlip = Neo.Utility.DataStructureLibrary<System.Text.StringBuilder>.Instance.CheckOut())
        {
            builderSlip.Value.Clear();
            builderSlip.Value.AppendFormat("        {0} \"{1}\"", Name, Message);
            return builderSlip.Value.ToString();
        }
    }
}

public abstract class Action_Discrete : AAction {
    public int Amount;

    public Action_Discrete( string name, int amount ) : base(name)
    {
        Amount = amount;
    }

    public override string ToString()
    {
        using (var builderSlip = Neo.Utility.DataStructureLibrary<System.Text.StringBuilder>.Instance.CheckOut())
        {
            builderSlip.Value.Clear();
            builderSlip.Value.AppendFormat("        {0} {1}", Name, Amount);
            return builderSlip.Value.ToString();
        }
    }
}

public class Action_CauseDamage : Action_Discrete {
    public Action_CauseDamage(string name, int amount) : base(name, amount)
    {
        Name = "deal";
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        otherPlayer.CurrentHealth -= Amount;
        Console.WriteLine(string.Format("Dealing {0} damage to {1}", Amount, otherPlayer.Name));
    }
}

public class Action_HealDamage : Action_Discrete {
    public Action_HealDamage(string name, int amount) : base(name, amount)
    {
        Name = "heal";
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        self.CurrentHealth += Amount;
        Console.WriteLine(string.Format("Healing {0} damage to {1}", Amount, self.Name));
    }
}

public class Action_DrawCards : Action_Discrete {
    public Action_DrawCards(string name, int amount) : base(name, amount)
    {
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        self.DrawCards(Amount);
        Console.WriteLine(string.Format("Player {0} is drawing {1} card(s)", self.Name, Amount));
    }
}

public class Action_OtherDrawCards : Action_Discrete {
    public Action_OtherDrawCards(string name, int amount) : base(name, amount)
    {
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        otherPlayer.DrawCards(Amount);
        Console.WriteLine(string.Format("Player {0} is drawing {1} card(s)", otherPlayer.Name, Amount));
    }
}

public class Action_AdjustManaCrystalCount : Action_Discrete {
    public Action_AdjustManaCrystalCount(string name, int amount) : base(name, amount)
    {
    }

    public override void Execute(Player self, Player otherPlayer)
    {
        self.ManaCrystals += Amount;
        Console.WriteLine(string.Format("Player {0} gets {1} mana crystal(s)", self.Name, Amount));
    }
}

public class ActionFactory {
    public static readonly ActionFactory Instance = new ActionFactory();

    protected Dictionary<string, Func<object, AAction>> m_Allocators = new Dictionary<string, Func<object, AAction>>();

    protected ActionFactory()
    {
        m_Allocators.Add("deal", delegate ( object arg ) {
            return new Action_CauseDamage( "deal", Convert.ToInt32(arg) );
        });
        m_Allocators.Add("heal", delegate (object arg) {
            return new Action_HealDamage("heal", Convert.ToInt32(arg));
        });
        m_Allocators.Add("msg", delegate (object arg) {
            return new Action_PrintMessage("msg", arg.ToString());
        });
        m_Allocators.Add("drawOther", delegate (object arg) {
            return new Action_OtherDrawCards("drawOther", Convert.ToInt32(arg));
        });
        m_Allocators.Add("drawSelf", delegate (object arg) {
            return new Action_DrawCards("drawSelf", Convert.ToInt32(arg));
        });
        m_Allocators.Add("addManaTotal", delegate (object arg) {
            return new Action_AdjustManaCrystalCount("addManaTotal", Convert.ToInt32(arg));
        });
    }

    public AAction  CreateAction( string name, object arg )
    {
        try
        {
            return m_Allocators[name].Invoke(arg);
        }
        catch( Exception ex )
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}