using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
using SFML.System;
using System.Threading;

namespace Raycast_Game
{
    public class Animation
    {
        //-------------------------------------------
        public Texture texture;
        public Vector2i size;
        public int[] frame_order;
        public float speed;

        public int current_frame;
        public List<Sprite> sprites = new List<Sprite>();
        public volatile bool isAnimating = false;
        //-------------------------------------------

        volatile Thread thread;

        public Animation(Texture texture, Vector2i size, int[] frame_order, float speed)
        {
            this.texture = texture;
            this.size = size;
            this.frame_order = frame_order;
            this.speed = speed;
            current_frame = 0;
            for (int i = 0; i < sprites.Count; i++)
            {
                Sprite spr = new Sprite();
                spr.Texture = texture;
                spr.TextureRect = new IntRect(i * size.X, 0, size.X, size.Y);
                sprites.Add(spr);
            }
        }
        public Animation(Animation animation)
        {
            texture = new Texture(animation.texture);
            size = animation.size;
            frame_order = (int[])animation.frame_order.Clone();
            speed = animation.speed;
            current_frame = 0;
            foreach(Sprite spr in animation.sprites)
                sprites.Add(new Sprite(spr));
        }

        public void Play()
        {
            if (!isAnimating)
            {
                isAnimating = true;
                thread = new Thread(play_thread);
                thread.Start();
            }
        }

        private void play_thread()
        {
            do
            {
                Thread.Sleep((int)(speed * 1000));
                current_frame++;
            }
            while (current_frame < frame_order.Length);
            current_frame = 0;
            isAnimating = false;
        }
        public Texture GetCurrentFrame()
        {
            Texture tex = new Texture(texture.CopyToImage(), new IntRect(frame_order[current_frame] * size.X,
                                                                         0, size.X, size.Y));
            return tex;
        }
    }
}
