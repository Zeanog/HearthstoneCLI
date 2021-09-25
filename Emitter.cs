using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

public struct Vector2i {
    public int X;
    public int Y;

    public Vector2i(Vector2i other )
    {
        X = other.X;
        Y = other.Y;
    }

    public static Vector2i operator *(Vector2i lhs, float scale)
    {
        return new Vector2i() { X = (int)(lhs.X * scale), Y = (int)(lhs.Y * scale) };
    }

    public static Vector2i operator +(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i() { X = lhs.X + rhs.X, Y = lhs.Y + rhs.Y };
    }

    public static explicit operator Vector2f(Vector2i rhs)
    {
        return new Vector2f() { X = rhs.X, Y = rhs.Y };
    }
}

public struct Vector2f {
    public float X;
    public float Y;

    public Vector2f(Vector2f other)
    {
        X = other.X;
        Y = other.Y;
    }

    public static Vector2f operator *(Vector2f lhs, float scale)
    {
        return new Vector2f() { X = lhs.X * scale, Y = lhs.Y * scale };
    }

    public static Vector2f operator +(Vector2f lhs, Vector2f rhs)
    {
        return new Vector2f() { X = lhs.X + rhs.X, Y = lhs.Y + rhs.Y };
    }

    public static explicit operator Vector2i(Vector2f rhs)
    {
        return new Vector2i() { X = (int)rhs.X, Y = (int)rhs.Y };
    }

    public static Vector2f Random(float minAngle, float maxAngle)
    {
        float angle = minAngle + (maxAngle - minAngle) * (float)MathExtensions.RNG.NextDouble();
        double radians = (Math.PI / 180f) * angle;
        float radius = 1.0f;

        var vec = new Vector2f() { X = radius * (float)Math.Cos(radians), Y = radius * (float)Math.Sin(radians) };
        return vec;
    }
}

public class Particle : IDisposable {
    public Vector2f     PrevOrigin;
    public Vector2f     Origin;
    public Vector2f     Velocity;
    public float        Lifetime;

    public void Dispose()
    {
        Console.CursorLeft = (int)PrevOrigin.X;
        Console.CursorTop = Console.WindowHeight - (int)PrevOrigin.Y;
        Console.Write(' ');
    }
}

public class EmitterLoader {
    public static Emitter Create(string emitterPath, Vector2i origin)
    {
        var data = File.ReadAllText(emitterPath);
        var particleDefs = JsonConvert.DeserializeObject<List<Emitter.ParticleDef>>(data, DataLoader.Settings);

        Emitter emitter = new Emitter(particleDefs);
        emitter.Origin = origin;
        
        return emitter;
    }
}

public class Emitter {
    private static LinkedList<Emitter> m_Emitters = new LinkedList<Emitter>();
    public static void Register( Emitter emitter )
    {
        m_Emitters.AddLast(emitter);
    }

    public static void Unregister(Emitter emitter)
    {
        m_Emitters.Remove(emitter);
    }

    public static async Task  ProcessEmitters()
    {
        float deltaTime = 1f / 10f;
        Vector2i prevCursor;
        while (true)
        {
            prevCursor = new Vector2i() { X = Console.CursorLeft, Y = Console.CursorTop };

            foreach (var emitter in m_Emitters)
            {
                emitter.UpdateFrame(deltaTime);
            }
            
            foreach (var emitter in m_Emitters)
            {
                emitter.RenderFrame();
            }
            Console.CursorLeft = prevCursor.X;
            Console.CursorTop = prevCursor.Y;
            Console.ResetColor();

            await Task.Delay((int)(deltaTime * 1000f));
        }
    }

    public readonly Vector2f Gravity = new Vector2f() { X = 0.0f, Y = -9.8f };

    public class ParticleDef {
        [JsonProperty("count")]
        public int Count;

        [JsonProperty("launchDirection")]
        public Tuple<int, int> LaunchDirection;

        [JsonProperty("image")]
        public char Image;

        [JsonProperty("color")]
        public ConsoleColor Color;

        [JsonProperty("lifetime")]
        public float Lifetime;

        [JsonProperty("gravityScale")]
        public float GravityScale;

        [JsonProperty("startSpeed")]
        public float StartSpeed;

        [JsonProperty("endSpeed")]
        public float EndSpeed;
    }

    protected Dictionary<ParticleDef, LinkedList<Particle>> m_Particles = new Dictionary<ParticleDef, LinkedList<Particle>>();

    public Vector2i Origin {
        get;
        set;
    }

    public bool DestroyWhenDone;

    public Emitter( List<Emitter.ParticleDef> particleDefs )
    {
        foreach( var def in particleDefs )
        {
            m_Particles.Add(def, new LinkedList<Particle>());
        }
    }

    public void Start()
    {
        foreach( var def in m_Particles.Keys )
        {
            var particleList = m_Particles[def];
            for( int ix = 0; ix < def.Count; ++ix )
            {
                Particle p = new Particle();
                p.Origin = (Vector2f)Origin;
                p.PrevOrigin = p.Origin;
                p.Lifetime = def.Lifetime;
                p.Velocity = Vector2f.Random(def.LaunchDirection.Item1, def.LaunchDirection.Item2) * def.StartSpeed;
                particleList.AddLast(p);
            }
        }

        Register(this);
    }

    public void Stop()
    {
        Unregister(this);
        m_Particles.Clear();
    }

    protected bool  IsParticleOutofBounds(Particle particle)
    {
        if((int)particle.Origin.Y >= Console.WindowHeight )
        {
            return true;
        }

        if ((int)particle.Origin.Y <= 0)
        {
            return true;
        }

        if ((int)particle.Origin.X <= 0)
        {
            return true;
        }

        if ((int)particle.Origin.X >= Console.WindowWidth)
        {
            return true;
        }

        return false;
    }

    public void UpdateFrame(float deltaTime)
    {
        Vector2f deltaGravity = Gravity * deltaTime;

        foreach (var def in m_Particles.Keys)
        {
            var particleList = m_Particles[def];

            LinkedListNode<Particle> node = particleList.First;
            while (node != null)
            {
                node.Value.Velocity += deltaGravity * def.GravityScale;
                node.Value.PrevOrigin = node.Value.Origin;
                node.Value.Origin += (node.Value.Velocity * deltaTime);
                node.Value.Lifetime -= deltaTime;

                LinkedListNode<Particle> next = node.Next;
                if (IsParticleOutofBounds(node.Value) || node.Value.Lifetime <= 0)
                {
                    node.Value.Dispose();
                    particleList.Remove(node);
                }

                node = next;
            }
        }

        //if( m_Particles.Count <= 0 && DestroyWhenDone )
        //{
        //    Unregister(this);//TODO: Find a real way to do this.  Some one could have a reference.  Wrapper struct??
        //}
    }

    public void     RenderFrame()
    {
        foreach (var def in m_Particles.Keys)
        {
            var particleList = m_Particles[def];

            LinkedListNode<Particle> node = particleList.First;
            while (node != null)
            {
                Console.CursorLeft = (int)node.Value.PrevOrigin.X;
                Console.CursorTop = Console.WindowHeight - (int)node.Value.PrevOrigin.Y;
                Console.Write(' ');

                Console.ForegroundColor = def.Color;
                Console.CursorLeft = (int)node.Value.Origin.X;
                Console.CursorTop = Console.WindowHeight - (int)node.Value.Origin.Y;
                Console.Write(def.Image);

                node = node.Next;
            }
        }
    }
}