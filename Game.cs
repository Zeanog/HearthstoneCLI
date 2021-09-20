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

public class Game {
    protected static Game m_Instance = null;
    public static Game Instance {
        get {
            return m_Instance;
        }
    }

    public static void  DestroyInstance()
    {
        m_Instance = null;
    }

    protected Random m_RNG = new Random( DateTime.UtcNow.Millisecond );

    protected List<Player> m_Players = new List<Player>();
    protected int m_InitialPlayerIndex = 0;

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

    public static void  Create(string configPath, params string[] playerDefs)
    {
        m_Instance = new Game(configPath, playerDefs);
    }

    public static void Create( string gameDefPath )
    {
        string encodedData = File.ReadAllText(gameDefPath);
        var gameDef = JsonConvert.DeserializeObject<Dictionary<string, object>>(encodedData, DataLoader.Settings);

        List<string> playerDefs = JsonConvert.DeserializeObject<List<string>>(gameDef["players"].ToString());
        m_Instance = new Game(gameDef["config"].ToString(), playerDefs.ToArray());
    }

    protected Game(string configPath, params string[] playerDefs)
    {
        string encoded = File.ReadAllText(configPath);
        CurrentConfig = JsonConvert.DeserializeObject<Game.Config>(encoded, DataLoader.Settings);

        CreateNew(playerDefs);
    }

    protected Game( Config config, params string[] playerDefs )
    {
        CurrentConfig = config;
        CreateNew(playerDefs);
    }

    public void CreateNew(params string[] playerDefs)
    {
        RoundCount = 0;

        try
        {
            for (int ix = 0; ix < playerDefs.Length; ++ix)
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

            m_InitialPlayerIndex = m_RNG.Next(0, playerDefs.Length);
        }
        catch( Exception ex )
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected void ResetGame()
    {
        
    }

    public bool ExecuteNextTurn()
    {
        Console.WriteLine("***************** Round {0} *****************", ++RoundCount);
        try
        {
            for (int ix = m_InitialPlayerIndex; ix < m_Players.Count; ix = (ix + 1) % m_Players.Count)
            {
                var player = m_Players[ix];
                var otherPlayer = m_Players[(ix + 1) % m_Players.Count];

                if (player.CurrentHealth <= 0)
                {
                    Console.Clear();
                    Console.WriteLine("***************** Congratulations *****************\n");

                    Console.WriteLine(string.Format("{0} has one the game!!\n", otherPlayer.Name));

                    //TODO: Would like to have a way to play again.
                    Console.WriteLine("Press any key to quit...");
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

                    //TODO:  Feel like this should be in a better place
                    string line = Console.ReadLine();

                    if (line[0] >= '0' && line[0] <= '9')
                    {
                        player.PlayCard((line[0] - '0') - 1, otherPlayer);
                    }
                    else if (line[0] == 'd')
                    {
                        break;
                    }
                    else if (line[0] == 'q')
                    {
                        return false;
                    } else
                    {
                        Console.WriteLine("Unknown command!  Please make a valid selection...");
                        System.Threading.Thread.Sleep(1000);
                    }

                    Console.WriteLine();
                };
            }

            return true;
        }
        catch( Exception ex )
        {
            Console.WriteLine(ex.Message);
            return false;
        }
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