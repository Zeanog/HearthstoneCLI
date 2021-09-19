using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public abstract class DataLoader {
    public static JsonSerializerSettings Settings = new JsonSerializerSettings();
    
    static DataLoader()
    {
        Settings.Converters.Add(new Converter_CardRarity());
        Settings.DefaultValueHandling = DefaultValueHandling.Ignore;
        Settings.NullValueHandling = NullValueHandling.Ignore;
    }
}

public class GameLoader : DataLoader {
    public static Game LoadFrom(string absPath)
    {
        try
        {
            string encodedData = File.ReadAllText(absPath);
            var gameDef = JsonConvert.DeserializeObject<Dictionary<string, object>>(encodedData, DataLoader.Settings);

            List<string> playerDefs = JsonConvert.DeserializeObject<List<string>>( gameDef["players"].ToString() );
            Game game = new Game(gameDef["config"].ToString(), playerDefs);
            
            return game;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}

public class Game {
    protected static Game m_Instance = null;
    public static Game Instance {
        get {
            if( m_Instance == null )
            {
                m_Instance = GameLoader.LoadFrom("..\\..\\Data\\Game.json");
            }
            return m_Instance;
        }
    }

    public static void  DestroyInstance()
    {
        m_Instance = null;
    }

    protected List<Player> m_Players = new List<Player>();

    public class Config {
        [JsonProperty("maxMana")]
        public int MaxMana;

        [JsonProperty("maxHealth")]
        public int MaxHealth;

        [JsonProperty("initialHandSize")]
        public int InitialHandSize;

        [JsonProperty("manaIncrement")]
        public int ManaIncrement;

        [JsonProperty("cardDrawSize")]
        public int CardDrawSize;
    }

    public int RoundCount {
        get;
        protected set;
    }

    public Config CurrentConfig {
        get;
        protected set;
    }

    public Game(string configPath, List<string> playerDefs)
    {
        string encoded = File.ReadAllText(configPath);
        CurrentConfig = JsonConvert.DeserializeObject<Game.Config>(encoded, DataLoader.Settings);

        CreateNew(playerDefs);
    }

    public Game( Config config, List<string> playerDefs )
    {
        CurrentConfig = config;
        CreateNew(playerDefs);
    }

    public void CreateNew(List<string> playerDefs)
    {
        RoundCount = 0;

        try
        {
            for (int ix = 0; ix < playerDefs.Count; ++ix)
            {
                string encodedPlayer = File.ReadAllText(playerDefs[ix]);
                var pseudoPlayer = JsonConvert.DeserializeObject<Dictionary<string, object>>(encodedPlayer, DataLoader.Settings);

                Deck deck = DeckLoader.LoadFrom(pseudoPlayer["deck"].ToString());
                deck.Shuffle();
                deck.Shuffle();

                Player player = new Player(CurrentConfig.MaxHealth, CurrentConfig.MaxMana, deck);
                player.Name = pseudoPlayer["name"].ToString();

                m_Players.Add(player);

                player.DrawCards(CurrentConfig.InitialHandSize);
            }
        }
        catch( Exception ex )
        {
            Console.WriteLine(ex.Message);
        }
    }

    public bool ExecuteNextTurn()
    {
        Console.WriteLine("***************** Round {0} *****************", ++RoundCount);

        for (int ix = 0; ix < m_Players.Count; ++ix) 
        {
            var player = m_Players[ix];
            var otherPlayer = m_Players[(ix + 1) % m_Players.Count];

            if ( player.CurrentHealth <= 0 )
            {
                Console.WriteLine(string.Format("***************** Congratulations to player {0}!!!\n", otherPlayer.Name));
                break;
            }

            Console.WriteLine(string.Format("\n\n***************** {0}'s turn ****************", player.Name));

            player.ManaCrystals += CurrentConfig.ManaIncrement;
            player.ManaCount = player.ManaCrystals;
            player.DrawCards(CurrentConfig.CardDrawSize);

            while (true)
            {
                Console.Write(player.ToString());
                var foreColor = Console.ForegroundColor;

                Console.ForegroundColor = player.ManaCount <= 0 ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.WriteLine("d) Done with turn");
                Console.ResetColor();

                Console.WriteLine("q) Quit game");
                Console.Write("Enter Selection:  ");

                string line = Console.ReadLine();
                
                if(line[0] >= '0' && line[0] <= '9')
                {
                    player.PlayCard((line[0] - '0') - 1, otherPlayer);
                }
                else if (line[0] == 'd')
                {
                    break;
                }
                else if(line[0] == 'q' )
                {
                    return false;
                }

                Console.WriteLine();
            };
        }

        return true;
    }

    public bool IsOver {
        get {
            if( m_Players.Count <= 0 )
            {
                return true;
            }

            foreach( var player in m_Players )
            {
                if(player.CurrentHealth <= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}