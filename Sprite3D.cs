using SFML.Graphics;
using SFML.System;
using System;
using System.Threading;

namespace Raycast_Game
{
    public abstract class Sprite3D : IComparable<Sprite3D>, ICloneable
    {
        public bool isDrawn { get; set; }
        public Texture Texture { get; set; }
        public Vector2f Position { get; set; }
        public float Scale { get; set; }

        public bool isOn = false;

        public int CompareTo(Sprite3D other)
        {
            return Program.Dist(Program.player.Position, other.Position).CompareTo(Program.Dist(Program.player.Position, Position));
        }
        public virtual dynamic Clone()
        {
            return new object();
        }
    }
    class Mob : Sprite3D
    {
        #region Base properties
        public float speed;
        public Animation idle_anim;
        public AIType type;
        public enum AIType
        {
            Butterfly,
            Crab,
            Bee,
            Cat
        }
        public Mob(Texture texture, Vector2f position,
                   float speed, Animation idle_anim, AIType type, float scale = 1, bool isDrawn = true)
        {
            Texture = texture;
            Position = position;
            this.speed = speed;
            this.idle_anim = idle_anim;
            this.type = type;
            Scale = scale;
            this.isDrawn = isDrawn;
            thread_i = new Thread(idle_thread);
        }
        public Mob() { }

        public override dynamic Clone()
        {
            Animation i_a = new Animation(idle_anim);
            return new Mob(Texture, Position, speed, i_a, type, Scale, isDrawn);
        }
        #endregion

        Thread thread_i;
        volatile bool isIdling = false;

        public void Idle()
        {
            if (!isIdling)
            {
                if (thread_i.ThreadState != ThreadState.Running)
                {
                    thread_i = new Thread(idle_thread);
                    isIdling = true;
                    thread_i.Start();
                }
            }
        }
        private void idle_thread()
        {
            idle_anim.Play();
            Texture = idle_anim.GetCurrentFrame();
            do
            {
                Thread.Sleep((int)(idle_anim.speed * 1000f / 2f));
                Texture = idle_anim.GetCurrentFrame();
            }
            while (idle_anim.isAnimating);
            isIdling = false;
        }
        public void Move(float dt)
        {
            switch (type)
            {
                case AIType.Butterfly:
                    AIButterfly(dt);
                    break;
                case AIType.Crab:
                    AIMobs(dt, 2);
                    break;
                case AIType.Bee:
                    AIMobs(dt, 1);
                    break;
                case AIType.Cat:
                    AIMobs(dt, 3);
                    break;
            }
        }

        #region Enemy AIs
        Clock clock = new Clock();
        
        float x = 0, y = 0;
        private void AIMobs(float dt, float time)
        {
            if (clock.ElapsedTime.AsSeconds() >= time)
            {
                clock.Restart();
                do
                {
                    x = new Random().Next(-1, 2);
                    y = new Random().Next(-1, 2);
                }
                while (x == 0 && y == 0);
            }

            Vector2f dir = new Vector2f(x, y);
            Vector2i offset = new Vector2i(12 * Math.Sign(dir.X), 12 * Math.Sign(dir.Y));
            Vector2i mPos = new Vector2i(Program.RoundUp(Position.X) - 1,
                                         Program.RoundUp(Position.Y) - 1);
            Vector2i a_mPos = new Vector2i(Program.RoundUp(Position.X + offset.X) - 1,
                                           Program.RoundUp(Position.Y + offset.Y) - 1);
            if (Program.map[mPos.Y * Program.mSize.X + a_mPos.X] == '0')
                Position += new Vector2f(dir.X * speed * dt, 0);
            if (Program.map[a_mPos.Y * Program.mSize.X + mPos.X] == '0')
                Position += new Vector2f(0, dir.Y * speed * dt);

        }
        private void AIButterfly(float dt)
        {
            if (Program.Dist(Program.player.Position, Position) < 70)
            {
                x = (float)Math.Sin(Math.Atan2(Position.X - Program.player.Position.X,
                                               Position.Y - Program.player.Position.Y)) * 2;
                y = (float)Math.Cos(Math.Atan2(Position.X - Program.player.Position.X,
                                               Position.Y - Program.player.Position.Y)) * 2;
            }

            else if (clock.ElapsedTime.AsSeconds() >= 2)
            {
                clock.Restart();
                do
                {
                    x = new Random().Next(-1, 2) * (float)new Random().NextDouble();
                    y = new Random().Next(-1, 2) * (float)new Random().NextDouble();
                }
                while (x == 0 && y == 0);
                x /= Program.Dist(new Vector2f(0, 0), new Vector2f(x, y));
                y /= Program.Dist(new Vector2f(0, 0), new Vector2f(x, y));
            }

            Vector2f dir = new Vector2f(x, y);
            Vector2i offset = new Vector2i(12 * Math.Sign(dir.X), 12 * Math.Sign(dir.Y));
            Vector2i mPos = new Vector2i(Program.RoundUp(Position.X) - 1,
                                         Program.RoundUp(Position.Y) - 1);
            Vector2i a_mPos = new Vector2i(Program.RoundUp(Position.X + offset.X) - 1,
                                           Program.RoundUp(Position.Y + offset.Y) - 1);
            if (mPos.Y * Program.mSize.X + a_mPos.X > 0
                && mPos.Y * Program.mSize.X + a_mPos.X < Program.map.Length)
            {
                if (Program.map[mPos.Y * Program.mSize.X + a_mPos.X] == '0')
                    Position += new Vector2f(dir.X * speed * dt, 0);
                else
                {
                    x = -x;
                    offset = new Vector2i(12 * Math.Sign(dir.X), 12 * Math.Sign(dir.Y));
                    a_mPos = new Vector2i(Program.RoundUp(Position.X + offset.X) - 1,
                                          Program.RoundUp(Position.Y + offset.Y) - 1);
                    if (Program.map[mPos.Y * Program.mSize.X + a_mPos.X] == '0')
                        Position += new Vector2f(dir.X * speed * dt, 0);
                }
            }
            if (a_mPos.Y * Program.mSize.X + mPos.X > 0
                && a_mPos.Y * Program.mSize.X + mPos.X < Program.map.Length)
            {
                if (Program.map[a_mPos.Y * Program.mSize.X + mPos.X] == '0')
                    Position += new Vector2f(0, dir.Y * speed * dt);
                else
                {
                    y = -y;
                    offset = new Vector2i(12 * Math.Sign(dir.X), 12 * Math.Sign(dir.Y));
                    a_mPos = new Vector2i(Program.RoundUp(Position.X + offset.X) - 1,
                                          Program.RoundUp(Position.Y + offset.Y) - 1);
                    if (Program.map[a_mPos.Y * Program.mSize.X + mPos.X] == '0')
                        Position += new Vector2f(0, dir.Y * speed * dt);
                }
            }
        }
        #endregion
    }

}
