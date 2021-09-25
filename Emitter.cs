using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public ConsoleColor Color;
    public char         Image;

    public float        Lifetime = 2.0f;
    public float        GravityScale = 1.0f;

    public Particle( char image, ConsoleColor color )
    {
        Image = image;
        Color = color;
        PrevOrigin = Origin;
    }

    public void Dispose()
    {
        Console.CursorLeft = (int)PrevOrigin.X;
        Console.CursorTop = Console.WindowHeight - (int)PrevOrigin.Y;
        Console.Write(' ');
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

    protected LinkedList<Particle> m_Particles = new LinkedList<Particle>();

    public Vector2i Origin {
        get;
        set;
    }

    public void Start()
    {
        for (int ix = 0; ix < 10; ++ix)
        {
            Particle p = new Particle('#', ConsoleColor.DarkRed);
            p.Origin = (Vector2f)Origin;
            p.PrevOrigin = p.Origin;
            p.Velocity = Vector2f.Random(20.0f, 160.0f) * 15.0f;
            p.Lifetime = 1.0f + (float)MathExtensions.RNG.NextDouble() * 2.0f;
            m_Particles.AddLast(p);
        }

        //for (int ix = 0; ix < 15; ++ix)
        //{
        //    Particle p = new Particle('@', ConsoleColor.Gray);
        //    p.Origin = (Vector2f)Origin;
        //    p.PrevOrigin = p.Origin;
        //    p.Velocity = Vector2f.Random(20.0f, 160.0f) * 1f;
        //    p.GravityScale = 0.01f;
        //    p.Lifetime = 5.0f;
        //    m_Particles.AddLast(p);
        //}

        for (int ix = 0; ix < 5; ++ix)
        {
            Particle p = new Particle('^', ConsoleColor.Red);
            p.Origin = (Vector2f)Origin;
            p.PrevOrigin = p.Origin;
            p.Lifetime = 0.5f;
            p.Velocity = Vector2f.Random(80.0f, 100.0f) * 15.0f;
            p.GravityScale = 1f;
            p.Lifetime = 1.0f + (float)MathExtensions.RNG.NextDouble() * 1f;
            m_Particles.AddLast(p);
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
        foreach (var particle in m_Particles)
        {
            particle.Velocity += deltaGravity * particle.GravityScale;
            particle.PrevOrigin = particle.Origin;
            particle.Origin += (particle.Velocity * deltaTime);
            particle.Lifetime -= deltaTime;
        }

        LinkedListNode<Particle> p = m_Particles.First;
        while( p != null )
        {
            LinkedListNode<Particle> next = p.Next;
            if (IsParticleOutofBounds(p.Value) || p.Value.Lifetime <= 0)
            {
                p.Value.Dispose();
                m_Particles.Remove(p);
            }

            p = next;
        }
    }

    //TODO: Need a way to removed char from last frame
    public void     RenderFrame()
    {
        foreach( var particle in m_Particles )
        {
            Console.CursorLeft = (int)particle.PrevOrigin.X;
            Console.CursorTop = Console.WindowHeight - (int)particle.PrevOrigin.Y;
            Console.Write(' ');

            Console.ForegroundColor = particle.Color;
            Console.CursorLeft = (int)particle.Origin.X;
            Console.CursorTop = Console.WindowHeight - (int)particle.Origin.Y;
            Console.Write(particle.Image);
        }
    }
}