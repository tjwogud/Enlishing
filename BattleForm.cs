using Enlishing.Properties;
using System;

namespace Enlishing
{
    public class Bullet
    {
        public Vector position;
        public Vector direction;
    }

    public class Enemy
    {
        public enum Type
        {
            Normal, Fast
        }

        public int hp;
        public Vector position;
        public Size size;
        public int speed;
        public int damage;
        public Type type;
    }

    public class BattleForm : Form
    {
        public readonly System.Windows.Forms.Timer timer;
        public readonly HashSet<Keys> presseds = [];

        public Vector playerPosition;

        public readonly List<Bullet> bullets = [];

        public readonly List<Enemy> enemies = [];

        public readonly Dungeon dungeon;

        public readonly int floor;

        public Boss boss;

        public BattleForm(Dungeon dungeon, int floor, Boss.Type? bossType = null)
        {
            this.dungeon = dungeon;
            this.floor = floor;

            AllowTransparency = true;
            BackColor = Color.Red;
            TransparencyKey = Color.Red;
            SizeGripStyle = SizeGripStyle.Hide;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            TopLevel = true;
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;

            timer = new();
            timer.Interval = 10;
            timer.Tick += Update;
            timer.Start();

            Paint += Draw;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseDown += OnMouseDown;

            startTime = lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (bossType != null)
            {
                boss = new(this, bossType.Value);
                Activated += (s, e) => boss.OnActivated();
                lastEnemy = startTime;
            }

            Invalidate();
        }

        public Point Transform(Vector vector) => new(vector.intX + Width / 2, vector.intY + Height / 2);

        public Vector InverseTransform(Point point) => new(point.X - Width / 2, point.Y - Height / 2);

        public static Rectangle CenteredRect(Point p, Size s) => new(p.X - s.Width / 2, p.Y - s.Height / 2, s.Width, s.Height);

        public long startTime;

        public long lastTime;

        public long curTime;

        public long lastBullet = -1000;

        public long lastEnemy = -100000;

        public bool clicked;
        public Point mousePosition;

        public int kills;

        public bool win = false;

        private void Update(object sender, EventArgs e)
        {
            curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int deltaTime = (int)(curTime - lastTime);
            lastTime = curTime;

            const int playerSpeed = 100;
            int v = playerSpeed * deltaTime / 1000;
            if (IsPressed(Keys.Up) || IsPressed(Keys.W)) playerPosition += v * Vector.up;
            if (IsPressed(Keys.Down) || IsPressed(Keys.S)) playerPosition += v * Vector.down;
            if (IsPressed(Keys.Left) || IsPressed(Keys.A)) playerPosition += v * Vector.left;
            if (IsPressed(Keys.Right) || IsPressed(Keys.D)) playerPosition += v * Vector.right;
            
            const int bulletSpeed = 500;
            const int bulletDamage = 100;
            bullets.RemoveAll(bullet =>
            {
                var transformed = Transform(bullet.position);
                foreach (Enemy enemy in enemies)
                {
                    if (CenteredRect(Transform(enemy.position), enemy.size).Contains(transformed))
                    {
                        enemy.hp -= bulletDamage;
                        return true;
                    }
                }

                bullet.position += bullet.direction * bulletSpeed * deltaTime / 1000;

                return !ClientRectangle.Contains(Transform(bullet.position));
            });

            const int enemyCooltime = 20000;
            int enemyCount = boss == null ? (floor + 1) * 5 : 5;
            if (curTime - lastEnemy >= enemyCooltime)
            {
                lastEnemy = curTime;
                for (int i = 0; i < enemyCount; i++)
                {
                    var type = RandomUtils.SelectOne([Enemy.Type.Normal, Enemy.Type.Fast], [100, boss == null ? (floor + 1) * 20 : 0]);
                    var sum = (Width + Height) * 2;
                    var pos = Random.Shared.Next(sum);
                    var enemy = new Enemy()
                    {
                        type = type,
                        hp = type == Enemy.Type.Normal ? 200 : 100,
                        speed = type == Enemy.Type.Normal ? 80 : 200,
                        damage = 10,
                        size = new Size(50, 50),
                        position = pos < Width
                        ? InverseTransform(new(pos, 0))
                        : (pos < (Width + Height)
                            ? InverseTransform(new(Width, pos - Width))
                            : (pos < (Width * 2 + Height)
                                ? InverseTransform(new(pos - Width - Height, Height))
                                : InverseTransform(new(0, pos - Width * 2 - Height))))
                    };
                    enemies.Add(enemy);
                }
            }

            var playerRect = CenteredRect(Transform(playerPosition), new(30, 30));
            enemies.RemoveAll(enemy =>
            {
                if (enemy.hp <= 0)
                {
                    kills++;
                    return true;
                }
                var rect = CenteredRect(Transform(enemy.position), enemy.size);
                if (playerRect.IntersectsWith(rect))
                {
                    dungeon.hp -= enemy.damage;
                    if (dungeon.hp <= 0)
                        Close();
                    return true;
                }
                enemy.position += (playerPosition - enemy.position).Normalize() * enemy.speed * deltaTime / 1000;
                return false;
            });

            const int bulletCooltime = 500;
            if (clicked)
            {
                clicked = false;
                if (curTime - lastBullet >= bulletCooltime)
                {
                    lastBullet = curTime;
                    var bullet = new Bullet()
                    {
                        position = playerPosition,
                        direction = (InverseTransform(mousePosition) - playerPosition).Normalize()
                    };
                    bullets.Add(bullet);
                }
            }

            if (boss != null)
                boss.OnUpdate(deltaTime);
            else if (curTime - startTime >= 60000)
            {
                win = true;
                Close();
            }

            Invalidate();
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            const int size = 50;
            e.Graphics.DrawImage(Resources.asdf, CenteredRect(Transform(playerPosition), new Size(size, size)));
            e.Graphics.FillRectangle(Brushes.Black, CenteredRect(Transform(playerPosition + new Vector(0, -40)), new(85, 25)));
            e.Graphics.FillRectangle(Brushes.Green, new Rectangle(Transform(playerPosition + new Vector(-40, -50)), new((int)(80 * dungeon.hp / dungeon.maxHp), 20)));
            
            foreach (Bullet bullet in bullets)
                e.Graphics.DrawImage(Resources.bullet, CenteredRect(Transform(bullet.position), new(20, 20)));

            foreach (Enemy enemy in enemies)
                e.Graphics.DrawImage(
                    enemy.type switch
                    {
                        Enemy.Type.Normal => Resources.enemy,
                        Enemy.Type.Fast => Resources.fastEnemy,
                        _ => throw new NotImplementedException()
                    }, CenteredRect(Transform(enemy.position), enemy.size));

            if (boss != null)
                boss.OnDraw(e.Graphics);
            else
            {
                e.Graphics.FillRectangle(Brushes.Black, CenteredRect(Transform(new(0, -Height / 2 + 50)), new(310, 60)));
                e.Graphics.FillRectangle(Brushes.Green, new Rectangle(Transform(new Vector(0, -Height / 2 + 25) + new Vector(-150, 0)), new((int)(300 * (curTime - startTime) / 60000), 50)));
            }
        }

        private bool IsPressed(Keys keyCode)
        {
            return presseds.Contains(keyCode);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            presseds.Add(e.KeyCode);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            presseds.Remove(e.KeyCode);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            clicked = true;
            mousePosition = e.Location;
        }
    }
}
