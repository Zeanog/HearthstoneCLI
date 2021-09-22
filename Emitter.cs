using System;
using System.Collections.Generic;

public struct Vector2 {
    public int X;
    public int Y;
}

public class Particle {
    public Vector2 Origin;
    public Vector2 Velocity;

    public ConsoleColor Color;
    public char         Image;

    public Particle( char image, ConsoleColor color )
    {
        Image = image;
        Color = color;
    }
}

public class Emitter {
    protected LinkedList<Particle> m_Particles = new LinkedList<Particle>();

    protected Vector2 m_Origin;

    public void UpdateFrame(float deltaTime)
    {

    }

    public void     RenderFrame( float deltaTime )
    {
        //Vector2 prevCursor = new Vector2() { X = Console.CursorLeft, Y = Console.CursorTop };
        foreach( var particle in m_Particles )
        {
            Console.CursorLeft = particle.Origin.X;
            Console.CursorTop = particle.Origin.Y;
            Console.Write(particle.Image);
        }
        //Console.CursorLeft = prevCursor.X;
        //Console.CursorTop = prevCursor.Y;
    }
}