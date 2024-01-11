using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace Raycast_Game
{
    class Content
    {
        public const string CONTENT_DIR = "Resources\\";

        public static Font font;
        public static Texture texLoadScreen;
        public static Texture texHand;

        public static Texture texWalls;

        public static Texture texCrab;
        public static Texture texBee;
        public static Texture texCat;

        public static Texture texBut0;

        public static void Load()
        {
            font = new Font(CONTENT_DIR + "font.ttf");
            texLoadScreen = new Texture(CONTENT_DIR + "loading_screen.png");
            texHand = new Texture(CONTENT_DIR + "hand.png");

            texWalls = new Texture(CONTENT_DIR + "walls.png");

            texCrab = new Texture(CONTENT_DIR + "crab.png");
            texBee  = new Texture(CONTENT_DIR + "bee.png");
            texCat  = new Texture(CONTENT_DIR + "cat.png");

            texBut0 = new Texture(CONTENT_DIR + "butterfly0.png");
        }
    }
}
