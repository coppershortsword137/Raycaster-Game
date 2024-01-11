using SFML.Graphics;
using SFML.System;
using static System.Math;

namespace Raycast_Game
{
    public static class Hand
    {
        public static Animation attack_anim = new Animation(Content.texHand, new Vector2i(320, 180),
                                                            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 0.025f);
        public static Texture idle_tex = new Texture(attack_anim.texture.CopyToImage(), 
                                                     new IntRect(0, 0, 320, 180));
        public static float attackRange = 30;
        public static int counter = 0;
        public static Text counterText = new Text(counter.ToString(), Content.font, 80);

        private readonly static Clock clock = new Clock();
        public static void Attack()
        {
            attack_anim.Play();
            if (clock.ElapsedTime.AsSeconds() >= attack_anim.speed)
            {
                clock.Restart();
                for (int i = 0; i < Program.sprs.Count; i++)
                {
                    if (Program.sprs[i].GetType() == new Mob().GetType()
                        && Program.Dist(Program.sprs[i].Position, Program.player.Position) < attackRange)
                    {
                        Vector2f pos = Program.player.Position - Program.sprs[i].Position;
                        float angle = (float)Atan2(pos.Y, pos.X) - Program.Deg2Rad(Program.player.Rotation);
                        if (-Program.PI >= angle)
                            angle += Program.PI * 2;
                        else if (Program.PI < angle)
                            angle -= Program.PI * 2;

                        if (-Program.Deg2Rad(150) > angle || angle > Program.Deg2Rad(150))
                        {
                            Mob mob = (Mob)Program.sprs[i];
                            if (mob.type == Mob.AIType.Butterfly)
                            {
                                counter++;
                                mob.isDrawn = false;
                                counterText.DisplayedString = counter.ToString();
                            }
                        }
                    }
                }
            }
        }
    }
}
