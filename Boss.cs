using Enlishing.Properties;
using System.Net;
using System.Windows.Media.Media3D;

namespace Enlishing
{
    public class Boss(BattleForm form, Boss.Type type)
    {
        public class Polygon
        {
            public enum Type
            {
                Rectangle = 0,
                Triangle = 1,
                Circle = 2
            }

            public Vector position;
            public bool show;
            public long spawnTime;
            public Type type;
            public Color color;
        }

        public enum Type
        {
            MSPaint
        }

        public Type type = type;

        public BattleForm form = form;

        public int hp;

        public int maxHp;

        public Vector position;

        public List<Polygon> polygons = [];

        public void OnActivated()
        {
            if (type == Type.MSPaint)
            {
                hp = maxHp = 10000;
                position = new(0, -form.Height / 2 + 300);
                form.playerPosition = -position;
                lastPolygon = form.startTime;
            }
        }

        public long lastPolygon = 0;

        public const int polygonSize = 200;
        
        public static bool PointInTriangle(Vector p, Vector a, Vector b, Vector c)
        {
            double as_x = p.x - a.x;
            double as_y = p.y - a.y;

            bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

            if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab)
                return false;
            if ((c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x) > 0 != s_ab)
                return false;
            return true;
        }

        public void OnUpdate(int deltaTime)
        {
            form.bullets.RemoveAll(bullet =>
            {
                if ((bullet.position - position).magnitude <= 250)
                {
                    hp -= 100;
                    return true;
                }
                return false;
            });
            if (hp <= 0)
            {
                form.kills++;
                form.win = true;
                form.Close();
            }

            const int polygonCooltime = 5000;
            if (form.curTime - lastPolygon >= polygonCooltime)
            {
                lastPolygon = form.curTime;
                var polygon = new Polygon()
                {
                    position = form.playerPosition,
                    spawnTime = lastPolygon,
                    type = (Polygon.Type)Random.Shared.Next(3),
                    color = Color.FromArgb(255, Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255)) // RED는 Transparent 처리되기 때문에 0~254
                };
                polygons.Add(polygon);
            }

            const int polygonShow = 3000;
            const int polygonDie = 30000;
            const int polygonDamage = 40;
            polygons.RemoveAll(polygon =>
            {
                if (!polygon.show && form.curTime - polygon.spawnTime >= polygonShow)
                    polygon.show = true;
                if (polygon.show)
                {
                    bool damage = false;
                    int polygonSize = (int)(Boss.polygonSize * 1.2);
                    switch (polygon.type)
                    {
                        case Polygon.Type.Rectangle:
                            damage = new Rectangle(form.Transform(polygon.position - new Vector(polygonSize, polygonSize) / 2), new(polygonSize, polygonSize)).Contains(form.Transform(form.playerPosition));
                            break;
                        case Polygon.Type.Triangle:
                            int height = (int)(Math.Sqrt(3) / 2 * polygonSize);
                            damage = PointInTriangle(form.playerPosition,
                                polygon.position + new Vector(0, -height * 2 / 3),
                                polygon.position + new Vector(-polygonSize / 2, height / 3),
                                polygon.position + new Vector(polygonSize / 2, height / 3));
                            break;
                        case Polygon.Type.Circle:
                            damage = (form.playerPosition - polygon.position).magnitude <= polygonSize / 2;
                            break;
                    }
                    if (damage)
                    {
                        form.dungeon.hp -= polygonDamage * deltaTime / 1000d;
                        if (form.dungeon.hp <= 0)
                            form.Close();
                    }
                }
                return form.curTime - polygon.spawnTime >= polygonDie;
            });
        }

        public void OnDraw(Graphics g)
        {
            g.DrawImage(Resources.mspaint, BattleForm.CenteredRect(form.Transform(position), new(500, 500)));

            foreach (var polygon in polygons)
            {
                if (!polygon.show)
                {
                    using Pen pen = new(polygon.color);
                    switch (polygon.type)
                    {
                        case Polygon.Type.Rectangle:
                            g.DrawRectangle(pen, new Rectangle(form.Transform(polygon.position - new Vector(polygonSize, polygonSize) / 2), new(polygonSize, polygonSize)));
                            break;
                        case Polygon.Type.Triangle:
                            int height = (int)(Math.Sqrt(3) / 2 * polygonSize);
                            g.DrawPolygon(pen,
                                form.Transform(polygon.position + new Vector(0, -height * 2 / 3)),
                                form.Transform(polygon.position + new Vector(-polygonSize / 2, height / 3)),
                                form.Transform(polygon.position + new Vector(polygonSize / 2, height / 3)));
                            break;
                        case Polygon.Type.Circle:
                            g.DrawEllipse(pen, new Rectangle(form.Transform(polygon.position - new Vector(polygonSize, polygonSize) / 2), new(polygonSize, polygonSize)));
                            break;
                    }
                }
                else
                {
                    using Brush brush = new SolidBrush(polygon.color);
                    switch (polygon.type)
                    {
                        case Polygon.Type.Rectangle:
                            g.FillRectangle(brush, new Rectangle(form.Transform(polygon.position - new Vector(polygonSize, polygonSize) / 2), new(polygonSize, polygonSize)));
                            break;
                        case Polygon.Type.Triangle:
                            int height = (int)(Math.Sqrt(3) / 2 * polygonSize);
                            g.FillPolygon(brush,
                                form.Transform(polygon.position + new Vector(0, -height * 2 / 3)),
                                form.Transform(polygon.position + new Vector(-polygonSize / 2, height / 3)),
                                form.Transform(polygon.position + new Vector(polygonSize / 2, height / 3)));
                            break;
                        case Polygon.Type.Circle:
                            g.FillEllipse(brush, new Rectangle(form.Transform(polygon.position - new Vector(polygonSize, polygonSize) / 2), new(polygonSize, polygonSize)));
                            break;
                    }
                }
            }

            g.FillRectangle(Brushes.Black, BattleForm.CenteredRect(form.Transform(new(0, -form.Height / 2 + 50)), new(610, 60)));
            g.FillRectangle(Brushes.Magenta, new Rectangle(form.Transform(new Vector(0, -form.Height / 2 + 25) + new Vector(-300, 0)), new(600 * hp / maxHp, 50)));
        }
    }
}
