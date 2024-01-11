using System;
using System.Collections.Generic;
using static System.Math;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace Raycast_Game
{
    class Program
    {
        #region -------VARIABLES-------
        public const int BLOCK_SIZE = 20;
        public const float PI = 3.141592653589f;
        public const int FOV = 90;
        public const int TILE_SIZE = 32;
        public const int numberOfRays = 500;

        public enum GameState
        {
            Loading,
            MainMenu,
            Game
        }
        public static GameState state = GameState.Game;

        public static Vector2i mSize = new Vector2i(20, 20);
        static float p_Speed = 1f;
        static Vector2f dir = new Vector2f(0, 1);

        public static string map = "";

        static float dt;
        // for easier calculation - dt = dt * mult (mult = 60)

        static RenderWindow win = new RenderWindow(new VideoMode(1024, 512), "Raycast Game", Styles.Fullscreen);

        public static RectangleShape player = new RectangleShape(new Vector2f(10, 10));

        public class Stripe : IComparable<Stripe>
        {
            public RectangleShape line;
            public float dist;
            public Stripe(RectangleShape line, float dist)
            {
                this.line = line;
                this.dist = dist;
            }
            public int CompareTo(Stripe other)
            {
                return other.dist.CompareTo(dist);
            }
        }
        public static Stripe[] stripes = new Stripe[numberOfRays];
        public volatile static List<Sprite3D> sprs = new List<Sprite3D>();

        static Mob but0 = new Mob();
        #endregion -----------------------

        static void Main(string[] args)
        {
            win.SetVerticalSyncEnabled(true);
            win.Closed += Win_Closed;
            win.Resized += Win_Resized;
            Content.Load();
            Clock clock = new Clock();

            #region -------------CREATING CONTENT--------------
            #region Animations
            Animation crabAnim = new Animation(Content.texCrab, new Vector2i(32, 32),
                                                   new int[] { 0, 1 }, 0.3f);
            Animation beeAnim = new Animation(Content.texBee, new Vector2i(32, 32),
                                                   new int[] { 0, 1, 2, 3 }, 0.1f);
            Animation catAnim = new Animation(Content.texCat, new Vector2i(48, 48),
                                                   new int[] { 0, 1, 2 }, 0.5f);

            Animation butAnim0 = new Animation(Content.texBut0, new Vector2i(64, 64),
                                                   new int[] { 0, 1, 2, 3, 2, 1 }, 0.1f);
            #endregion

            #region Buts presets
            but0 = new Mob(Content.texBut0, new Vector2f(), 1, butAnim0, Mob.AIType.Butterfly, 0.25f);
            #endregion

            #region Levels
            const int K = (BLOCK_SIZE / 2) - 1;
            float crabSpeed = 0.5f;
            float beeSpeed = 1f;
            float catSpeed = 0.2f;

            Level lvl0 = new Level(0, new Vector2i(20, 20), new Vector2f(100, 130), 270,
                new List<Sprite3D>()
                {
                    new Mob(Content.texCrab, new Vector2f(4 * BLOCK_SIZE - K, 3 * BLOCK_SIZE - K), crabSpeed, 
                            new Animation(crabAnim), Mob.AIType.Crab),
                    new Mob(Content.texBee, new Vector2f(7 * BLOCK_SIZE - K, 7 * BLOCK_SIZE - K), beeSpeed, 
                            new Animation(beeAnim), Mob.AIType.Bee),
                    new Mob(Content.texCat, new Vector2f(14 * BLOCK_SIZE - K, 11 * BLOCK_SIZE - K), catSpeed, 
                            new Animation(catAnim), Mob.AIType.Cat)
                },
                "11111111111111111111" +
                "10000000000000000001" +
                "10000001000000000001" +
                "10000000000000000001" +
                "10000000000000000001" +
                "10010000000111000001" +
                "10010000001000000001" +
                "10010000001000000001" +
                "10010000001000000001" +
                "10000000000000000001" +
                "10000000000000000001" +
                "10000110000000000001" +
                "10000000000000000001" +
                "10000000001000000001" +
                "10111000010000000101" +
                "10000000100000000101" +
                "10000001000000000101" +
                "10000001000000000001" +
                "10000000000000000001" +
                "11111111111111111111");
            #endregion
            #endregion -------------------------------------------

            while (win.IsOpen)
            {
                switch (state)
                {
                    case GameState.MainMenu:
                        MainMenuState();
                        break;
                    case GameState.Game:
                        LevelState(lvl0);
                        break;
                }
            }
        }

        #region MainMenu State
        static void MainMenuState()
        {
            win.SetMouseCursorVisible(true);
            Mouse.SetPosition((Vector2i)win.Size / 2);

            RectangleShape backgr = new RectangleShape((Vector2f)win.Size * 2);
            backgr.Origin = backgr.Size / 2;
            backgr.Texture = Content.texWalls;
            backgr.Position = (Vector2f)win.Size / 2;
            Button startButton = new Button(new RectangleShape(new Vector2f(500, 250)),
                                            Content.texCrab);
            startButton.rect.Position = new Vector2f((win.Size.X - startButton.rect.Size.X) / 2,
                                                     (win.Size.Y - startButton.rect.Size.Y) / 2);

            float bg_Rotation = Deg2Rad(backgr.Rotation);
            const float rotClamp = PI;
            float i = -rotClamp;

            RectangleShape fadeRect = new RectangleShape((Vector2f)win.Size);
            fadeRect.FillColor = Color.Black;

            while (win.IsOpen
                   && state == GameState.MainMenu)
            {
                //----------Rotate BG-----------
                i += 0.01f;
                if (i > rotClamp)
                    i -= 2 * rotClamp;
                backgr.Rotation = (float)Rad2Deg(Sin(bg_Rotation + i));
                //------------------------------


                //if (Keyboard.IsKeyPressed(Keyboard.Key.P))
                //{
                //    win.Close();
                //    break;
                //}
                Clock clock = new Clock();

                startButton.Update();
                if (startButton.isPressed())
                {
                    LoadState(5, GameState.Game);
                    break;
                }

                #region Drawing
                win.DispatchEvents();
                win.Clear();
                win.Draw(backgr);
                win.Draw(startButton);
                win.Display();
                #endregion
            }
        }
        static void LoadState(float seconds, GameState gameState)
        {
            state = GameState.Loading;
            win.Clear();
            Clock clock = new Clock();
            clock.Restart();
            Animation anim = new Animation(Content.texLoadScreen, new Vector2i(320, 180),
                                           new int[] { 0, 1, 2 }, 0.25f);
            RectangleShape fadeRect = new RectangleShape((Vector2f)win.Size);
            while(clock.ElapsedTime.AsSeconds() <= seconds)
            {
                anim.Play();
                fadeRect.Texture = anim.GetCurrentFrame();
                win.Draw(fadeRect);
                win.Display();
            }
            state = gameState;
        }
        #endregion

        #region Levels State
        static void LevelState(Level level)
        {
            #region Start
            Clock clock = new Clock();

            player.Rotation = level.p_Rotation;
            player.Position = level.p_Position;
            player.FillColor = Color.Red;
            player.Origin = new Vector2f(5, 5);
            mSize = level.mSize;
            sprs.Clear();
            foreach(Sprite3D spr in level.sprs)
            {
                if (spr.GetType() == new Mob().GetType())
                {
                    Mob espr = (Mob)spr;
                    sprs.Add(espr.Clone());
                }
            }
            map = level.map;

            dir = new Vector2f((float)Cos(Deg2Rad(player.Rotation)), (float)Sin(Deg2Rad(player.Rotation)));
            Hand.counterText.OutlineThickness = 8;
            float _sp = p_Speed;
            #endregion

            while (win.IsOpen
                   && state == GameState.Game)
            {
                win.SetMouseCursorVisible(false);

                dt = clock.Restart().AsSeconds();
                dt *= 60;

                float center = (int)win.Size.X / 2;
                float mousePos = Mouse.GetPosition().X;
                mousePos -= center;
                mousePos /= (int)win.Size.X;
                mousePos *= FOV;
                mousePos = ClampAng(mousePos, false);
                Mouse.SetPosition(new Vector2i((int)win.Size.X / 2, (int)win.Size.Y / 2));

                #region Movement
                player.Rotation += mousePos;
                player.Rotation = ClampAng(player.Rotation, false);
                if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                    LoadState(2, GameState.MainMenu);

                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
                    p_Speed = _sp * 1.5f;
                else if (Keyboard.IsKeyPressed(Keyboard.Key.LControl))
                    p_Speed = _sp * 0.5f;
                else
                    p_Speed = _sp;
                if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    Hand.Attack();

                bool isMoveRight = Keyboard.IsKeyPressed(Keyboard.Key.D);
                bool isMoveLeft = Keyboard.IsKeyPressed(Keyboard.Key.A);
                bool isMoveUp = Keyboard.IsKeyPressed(Keyboard.Key.W);
                bool isMoveDown = Keyboard.IsKeyPressed(Keyboard.Key.S);

                dir = new Vector2f((float)Cos(Deg2Rad(player.Rotation)),
                                   (float)Sin(Deg2Rad(player.Rotation)));

                if (isMoveRight)
                {
                    Move('d', dt);
                }
                else if (isMoveLeft)
                {
                    Move('a', dt);
                }
                if (isMoveUp)
                {
                    Move('w', dt);
                }
                else if (isMoveDown)
                {
                    Move('s', dt);
                }
                #endregion

                AddButs(5);
                UpdateSprites();

                win.DispatchEvents();
                win.Clear(new Color(40, 40, 40, 255));
                #region Floor and ceiling
                RectangleShape ceiling = new RectangleShape(new Vector2f(win.Size.X, win.Size.Y / 2));
                RectangleShape floor = new RectangleShape(new Vector2f(win.Size.X, win.Size.Y / 2));
                ceiling.FillColor = new Color(0, 255, 255);
                floor.FillColor = new Color(0, 255, 0);
                floor.Position += new Vector2f(0, win.Size.Y / 2);
                win.Draw(ceiling);
                win.Draw(floor);
                #endregion
                DrawRays(numberOfRays);
                #region Hand
                RectangleShape rect = new RectangleShape(new Vector2f(win.Size.X, win.Size.Y));
                rect.Texture = Hand.attack_anim.GetCurrentFrame();
                win.Draw(rect);
                #endregion
                win.Draw(Hand.counterText);
                if (Keyboard.IsKeyPressed(Keyboard.Key.M))
                    DrawMiniMap();
                win.Display();
            }
        }

        static readonly Clock clock1 = new Clock();
        static void AddButs(float time)
        {
            if (clock1.ElapsedTime.AsSeconds() >= time)
            {
                clock1.Restart();
                Vector2f pos;
                do
                {
                    pos = new Vector2f(new Random().Next(0, mSize.X),
                                       new Random().Next(0, mSize.Y));
                }
                while (map[(int)pos.Y * mSize.Y + (int)pos.X] == '0');
                pos *= BLOCK_SIZE;
                pos -= new Vector2f((BLOCK_SIZE / 2) - 1, (BLOCK_SIZE / 2) - 1);

                int count = new Random().Next(0, 100);
                if (count < 80)
                {
                    Mob mob = but0.Clone();
                    mob.Position = pos;
                    sprs.Add(mob);
                }
            }
        }
        static void UpdateSprites()
        {
            for (int s = 0; s < sprs.Count; s++)
            {
                Sprite3D spr = sprs[s];
                if (sprs[s].isDrawn)
                {
                    if (sprs[s].GetType() == new Mob().GetType())
                    {
                        Mob espr = (Mob)sprs[s];
                        espr.Idle();
                        espr.Move(dt);
                    }
                }
                else
                    sprs.RemoveAt(s);
            }
        }
        static void DrawRays(int n)
        {
            #region Calculating
            float deltaA = Deg2Rad((float)FOV / (n - 1));
            float r_A;
            int DOF;
            const int mDOF = 20;
            float distH = 0;
            float distV = 0;
            float dist = 0;
            Vector2f pPos = player.Position;
            Vector2f rPos;
            Vector2f hPos;
            Vector2f vPos;
            Vector2i mPos = new Vector2i(0, 0);
            Vector2f offset = new Vector2f(0, 0);
            for (int i = 0; i < n; i++)
            {
                DOF = 0;
                r_A = Deg2Rad(player.Rotation - ((float)FOV / 2));
                rPos = pPos;
                r_A += deltaA * i;
                r_A = ClampAng(r_A, true);
                #region Check Horizontal Lines
                #region Check Angle
                if (PI < r_A && r_A < 2 * PI)
                {
                    rPos.Y = pPos.Y - (pPos.Y % BLOCK_SIZE);
                    rPos.X = pPos.X - (float)((pPos.Y - rPos.Y) / Tan(r_A));
                    offset.Y = -BLOCK_SIZE;
                    offset.X = -(float)(BLOCK_SIZE / Tan(r_A));
                }
                else if (0 < r_A && r_A < PI)
                {
                    rPos.Y = pPos.Y + (BLOCK_SIZE - (pPos.Y % BLOCK_SIZE));
                    rPos.X = pPos.X + (float)((rPos.Y - pPos.Y) / Tan(r_A));
                    offset.Y = BLOCK_SIZE;
                    offset.X = (float)(BLOCK_SIZE / Tan(r_A));
                }
                else if (r_A == PI)
                {
                    rPos = pPos;
                    offset.Y = 0;
                    offset.X = -BLOCK_SIZE;
                    DOF = mDOF;
                }
                #endregion
                while (DOF < mDOF)
                {
                    mPos.Y = (int)(rPos.Y / BLOCK_SIZE);
                    mPos.X = (int)(rPos.X + (BLOCK_SIZE - (rPos.X % BLOCK_SIZE))) / BLOCK_SIZE;
                    mPos -= new Vector2i(1, 1);
                    int mp = mPos.Y * mSize.X + mPos.X;
                    int mp1 = (mPos.Y + 1) * mSize.X + mPos.X;
                    if ((mp < map.Length && mp >= 0 && map[(int)mp] == '1')
                        || (mp1 < map.Length && mp1 >= 0 && map[(int)mp1] == '1'))
                    {
                        DOF = mDOF;
                    }
                    else
                    {
                        rPos += offset;
                        DOF++;
                    }
                }
                hPos = rPos;
                distH = Dist(pPos, rPos);
                #endregion

                DOF = 0;
                rPos = pPos;

                #region Check Vertical Lines
                #region Check Angle
                if (PI / 2 < r_A && r_A < 3 * PI / 2)
                {
                    rPos.X = pPos.X - (pPos.X % BLOCK_SIZE);
                    rPos.Y = pPos.Y - (float)((pPos.X - rPos.X) * Tan(r_A));
                    offset.X = -BLOCK_SIZE;
                    offset.Y = -(float)(BLOCK_SIZE * Tan(r_A));
                }
                else if (3 * PI / 2 < r_A || r_A < PI / 2)
                {
                    rPos.X = pPos.X + (BLOCK_SIZE - (pPos.X % BLOCK_SIZE));
                    rPos.Y = pPos.Y + (float)((rPos.X - pPos.X) * Tan(r_A));
                    offset.X = BLOCK_SIZE;
                    offset.Y = (float)(BLOCK_SIZE * Tan(r_A));
                }
                else if (r_A == PI / 2)
                {
                    rPos = pPos;
                    offset.Y = BLOCK_SIZE;
                    offset.X = 0;
                    DOF = mDOF;
                }
                else if (r_A == PI)
                {
                    rPos.X = pPos.X + (BLOCK_SIZE - (pPos.X % BLOCK_SIZE));
                    rPos.Y = pPos.Y;
                    offset.Y = 0;
                    offset.X = -BLOCK_SIZE;
                }
                #endregion
                while (DOF < mDOF)
                {
                    mPos.X = (int)(rPos.X / BLOCK_SIZE);
                    mPos.Y = (int)(rPos.Y + (BLOCK_SIZE - (rPos.Y % BLOCK_SIZE))) / BLOCK_SIZE;
                    mPos -= new Vector2i(1, 1);
                    int mp = mPos.Y * mSize.X + mPos.X;
                    int mp1 = mPos.Y * mSize.X + mPos.X + 1;
                    if ((mp < map.Length && mp >= 0 && map[(int)mp] == '1')
                        || (mp1 < map.Length && mp1 >= 0 && map[(int)mp1] == '1'))
                    {
                        DOF = mDOF;
                    }
                    else
                    {
                        rPos += offset;
                        DOF++;
                    }
                }
                vPos = rPos;
                distV = Dist(pPos, rPos);
                #endregion

                float shade;
                int column;
                if (distH < distV)
                {
                    dist = distH;
                    column = (int)(hPos.X % BLOCK_SIZE * ((float)BLOCK_SIZE / TILE_SIZE * 2.48));
                }
                else
                {
                    dist = distV;
                    column = (int)(vPos.Y % BLOCK_SIZE * ((float)BLOCK_SIZE / TILE_SIZE * 2.48));
                }

                shade = 1 / dist;

                float rd = FOV * (float)(Floor((double)n / 2) - i) / (n - 1);
                float rpp = 0.5f * (float)(Tan(Deg2Rad(rd)) / Tan(Deg2Rad(FOV / 2)));
                float cc = (float)Round(n * (0.5f - rpp));
                float nc = 1;
                float nrd;
                float nrpp = 1;
                if (i < n - 1)
                {
                    nrd = FOV * (float)(Floor((double)n / 2) - 1 - i) / (n - 1);
                    nrpp = 0.5f * (float)(Tan(Deg2Rad(nrd)) / Tan(Deg2Rad(FOV / 2)));
                    nc = (float)Round(n * (0.5f - nrpp));
                }

                float true_dist = dist;

                float prA = ClampAng(Deg2Rad(player.Rotation) - r_A, true);
                dist *= (float)Cos(prA);
                float lineW = (float)win.Size.X / n;
                int lineH = (int)(BLOCK_SIZE * win.Size.Y / dist / FOV * 90);
                float texOffset = 0;
                if (lineH > win.Size.Y)
                {
                    texOffset = (lineH - win.Size.Y) / 2;
                }
                lineH = (int)Clamp(lineH, 0, win.Size.Y);
                float lineOffset = win.Size.Y / 2 - lineH / 2;

                RectangleShape line = new RectangleShape(new Vector2f(lineW * (float)Max(1, nc - cc), lineH + texOffset * 2));
                line.Texture = Content.texWalls;
                line.TextureRect = new IntRect(column, 0, 1, TILE_SIZE - 1);
                line.Position = new Vector2f(cc * lineW, lineOffset - texOffset);
                line.FillColor *= new Color((byte)Clamp(shade * 50000, 1, 255),
                                            (byte)Clamp(shade * 50000, 1, 255),
                                            (byte)Clamp(shade * 50000, 1, 255));
                stripes[i] = new Stripe(line, true_dist);
            }
            #endregion

            #region Drawing
            Array.Sort(stripes);
            sprs.Sort();
            foreach (Stripe stripe in stripes)
            {
                for (int s = 0; s < sprs.Count; s++)
                {
                    if (Dist(sprs[s].Position, player.Position) > stripe.dist && !sprs[s].isOn)
                    {
                        sprs[s].isOn = true;
                        DrawSprite(sprs[s]);
                    }
                }
                win.Draw(stripe.line);
            }
            for (int s = 0; s < sprs.Count; s++)
            {
                if (!sprs[s].isOn)
                    DrawSprite(sprs[s]);
                sprs[s].isOn = false;
            }
            #endregion
        }
        static void DrawSprite(Sprite3D spr)
        {
            Vector2f pos = player.Position - spr.Position;
            float angle = (float)Atan2(pos.Y, pos.X) - Deg2Rad(player.Rotation);
            if (-PI >= angle)
                angle += PI * 2;
            else if (PI < angle)
                angle -= PI * 2;

            if (-Deg2Rad(70) > angle || angle > Deg2Rad(70))
            {
                RectangleShape rect = new RectangleShape(new Vector2f(BLOCK_SIZE * win.Size.X / Dist(player.Position, spr.Position) / 2 / (float)Cos(angle),
                                                                      BLOCK_SIZE * win.Size.Y / Dist(player.Position, spr.Position) / (float)Cos(angle)));
                rect.Size *= spr.Scale;

                float x = (float)(Tan(angle) * win.Size.X / 2 + (win.Size.X / 2 - rect.Size.X / 2) / FOV * 90);
                rect.Position = new Vector2f(x, (win.Size.Y - rect.Size.Y) / 2);
                rect.Texture = spr.Texture;
                rect.FillColor *= new Color((byte)Clamp(1 / Dist(player.Position, spr.Position) * 50000, 1, 255),
                                            (byte)Clamp(1 / Dist(player.Position, spr.Position) * 50000, 1, 255),
                                            (byte)Clamp(1 / Dist(player.Position, spr.Position) * 50000, 1, 255));
                win.Draw(rect);
            }
        }
        static void DrawMiniMap()
        {
            for (int i = 0; i < mSize.Y; i++)
            {
                for (int j = 0; j < mSize.X; j++)
                {
                    if (map[i * mSize.Y + j] == '1')
                    {
                        RectangleShape rect = new RectangleShape(new Vector2f(BLOCK_SIZE, BLOCK_SIZE));
                        rect.Position = new Vector2f(j * BLOCK_SIZE + (win.Size.X / 2 - mSize.X * BLOCK_SIZE / 2),
                                                     i * BLOCK_SIZE + (win.Size.Y / 2 - mSize.Y * BLOCK_SIZE / 2));
                        rect.FillColor = Color.Green;
                        rect.OutlineColor = Color.Black;
                        rect.OutlineThickness = 2;
                        win.Draw(rect);
                    }
                }
            }
            RectangleShape plr = new RectangleShape(player);
            plr.Position += new Vector2f(win.Size.X / 2 - mSize.X * BLOCK_SIZE / 2,
                                         win.Size.Y / 2 - mSize.Y * BLOCK_SIZE / 2);
            plr.OutlineColor = Color.White;
            plr.OutlineThickness = 3;
            win.Draw(plr);
        }
        static void Move(char key, float dt)
        {
            Vector2f dirH = new Vector2f((float)Cos(Deg2Rad(ClampAng(player.Rotation + 90, false))),
                                         (float)Sin(Deg2Rad(ClampAng(player.Rotation + 90, false))));
            Vector2i offsetV = new Vector2i(6 * Sign(dir.X), 6 * Sign(dir.Y));
            Vector2i offsetH = new Vector2i(6 * Sign(dirH.X), 6 * Sign(dirH.Y));
            Vector2i pPos = new Vector2i(RoundUp(player.Position.X) - 1,
                                         RoundUp(player.Position.Y) - 1);
            Vector2i a_pPosV = new Vector2i(RoundUp(player.Position.X + offsetV.X) - 1,
                                            RoundUp(player.Position.Y + offsetV.Y) - 1);
            Vector2i s_pPosV = new Vector2i(RoundUp(player.Position.X - offsetV.X) - 1,
                                            RoundUp(player.Position.Y - offsetV.Y) - 1);
            Vector2i a_pPosH = new Vector2i(RoundUp(player.Position.X + offsetH.X) - 1,
                                            RoundUp(player.Position.Y + offsetH.Y) - 1);
            Vector2i s_pPosH = new Vector2i(RoundUp(player.Position.X - offsetH.X) - 1,
                                            RoundUp(player.Position.Y - offsetH.Y) - 1);
            switch (key)
            {
                case 'w':
                    if (map[pPos.Y * mSize.X + a_pPosV.X] == '0')
                        player.Position += new Vector2f(dir.X * p_Speed * dt, 0);
                    if (map[a_pPosV.Y * mSize.X + pPos.X] == '0')
                        player.Position += new Vector2f(0, dir.Y * p_Speed * dt);
                    break;
                case 's':
                    if (map[pPos.Y * mSize.X + s_pPosV.X] == '0')
                        player.Position -= new Vector2f(dir.X * p_Speed * dt, 0);
                    if (map[s_pPosV.Y * mSize.X + pPos.X] == '0')
                        player.Position -= new Vector2f(0, dir.Y * p_Speed * dt);
                    break;
                case 'd':
                    if (map[pPos.Y * mSize.X + a_pPosH.X] == '0')
                        player.Position += new Vector2f(dirH.X * p_Speed * dt, 0);
                    if (map[a_pPosH.Y * mSize.X + pPos.X] == '0')
                        player.Position += new Vector2f(0, dirH.Y * p_Speed * dt);
                    break;
                case 'a':
                    if (map[pPos.Y * mSize.X + s_pPosH.X] == '0')
                        player.Position -= new Vector2f(dirH.X * p_Speed * dt, 0);
                    if (map[s_pPosH.Y * mSize.X + pPos.X] == '0')
                        player.Position -= new Vector2f(0, dirH.Y * p_Speed * dt);
                    break;
            }
        }
        #endregion

        #region Event Functions
        private static void Win_Closed(object sender, EventArgs e)
        {
            RenderWindow win = (RenderWindow)sender;
            win.Close();
        }
        private static void Win_Resized(object sender, SizeEventArgs e)
        {
            RenderWindow win = (RenderWindow)sender;
            win.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
        }
        #endregion

        #region Utility Functions
        static double Rad2Deg(double n)
        {
            n = n * (180 / PI);
            return n;
        }
        static float Rad2Deg(float n)
        {
            n = n * (180 / PI);
            return n;
        }

        static double Deg2Rad(double n)
        {
            n = n * (PI / 180);
            return n;
        }
        public static float Deg2Rad(float n)
        {
            n = n * (PI / 180);
            return n;
        }

        static float ClampAng(float ray, bool isRadians)
        {
            if (isRadians)
            {
                if (ray >= 2 * PI)
                    ray -= 2 * PI;
                else if (ray <= 0)
                    ray += 2 * PI;
            }
            else
            {
                if (ray >= 360)
                    ray -= 360;
                else if (ray <= 0)
                    ray += 360;
            }
            return ray;
        }

        public static int RoundUp(float value)
        {
            return (int)((value - (value % BLOCK_SIZE)) + BLOCK_SIZE) / BLOCK_SIZE;
        }

        public static float Dist(Vector2f a, Vector2f b)
        {
            return (float)Sqrt(Pow(a.X - b.X, 2) + Pow(a.Y - b.Y, 2));
        }
        #endregion
    }
}
