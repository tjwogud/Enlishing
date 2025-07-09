using Enlishing.Properties;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Text;

namespace Enlishing
{
    public class Dungeon
    {
        public class Floor
        {
            public int level;
            public bool isBoss;
            public Room[] rooms;
            public string key;
        }

        public class Room
        {
            public bool hasKey;
            public bool isBoss;
            public bool cleared;
        }

        public Floor[] floors;

        public double hp = 100;
        public double maxHp = 100;

        public int kills = 0;

        public static string CreateKey()
        {
            char[] chars = new char[4];
            for (int i = 0; i < 4; i++)
            {
                char code;
                int index = Random.Shared.Next(10 + 26);
                if (index < 10)
                    code = (char)('0' + index);
                else
                    code = (char)('a' + index - 10);
                chars[i] = code;
            }
            return new string(chars);
        }

        public static void Create(string dir)
        {
            const int floors = 3;
            const int rooms = 3;

            Dungeon dungeon = new();

            dungeon.floors = new Floor[floors];

            for (int i = 0; i < floors; i++)
            {
                Floor floor = dungeon.floors[i] = new Floor();
                floor.level = i + 1;
                if ((i + 1) % 3 == 0)
                {
                    //보스
                    floor.isBoss = true;
                    floor.rooms = new Room[1];
                    Room boss = floor.rooms[0] = new Room();
                    boss.hasKey = true;
                    boss.isBoss = true;
                }
                else
                {
                    floor.rooms = new Room[rooms];
                    int keyRoom = Random.Shared.Next(rooms);
                    for (int j = 0; j < rooms; j++)
                    {
                        Room room = floor.rooms[j] = new Room();
                        if (j == keyRoom)
                            room.hasKey = true;
                    }
                }
                if (i != 0)
                    floor.key = CreateKey();
            }

            string savePath = Path.Combine(dir, "dungeonSave.sav");
            File.WriteAllText(savePath, JsonConvert.SerializeObject(dungeon));

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            byte[] prev = null;
            for (int i = 0; i < floors; i++)
            {
                int level = floors - i - 1;
                var floor = dungeon.floors[level];
                MemoryStream memory = new();
                using ZipOutputStream zos = new(memory);
                zos.SetLevel(0);
                if (floor.key != null)
                    zos.Password = floor.key;
                if (floor.isBoss)
                {
                    // 보스
                    zos.PutNextEntry(new ZipEntry($"제 {level + 1}계층/보스 전투.bat"));
                    string launcher = $"start \"\" conhost \"{Environment.ProcessPath}\" battle \"{savePath}\" {level} {0} \"%~dpnx0\"";
                    var bytes = Encoding.GetEncoding(949).GetBytes(launcher);
                    zos.Write(bytes, 0, bytes.Length);
                    zos.CloseEntry();
                }
                else
                {
                    for (int j = 0; j < rooms; j++)
                    {
                        zos.PutNextEntry(new ZipEntry($"제 {level + 1}계층/{j + 1}번 방/전투.bat"));
                        string launcher = $"start \"\" conhost \"{Environment.ProcessPath}\" battle \"{savePath}\" {level} {j} \"%~dpnx0\"";
                        var bytes = Encoding.GetEncoding(949).GetBytes(launcher);
                        zos.Write(bytes, 0, bytes.Length);
                        zos.CloseEntry();
                    }
                }
                if (i != 0)
                {
                    zos.PutNextEntry(new ZipEntry($"제 {level + 1}계층/제 {level + 2}계층.zip"));
                    zos.Write(prev, 0, prev.Length);
                    zos.CloseEntry();
                }
                zos.Finish();
                zos.Close();
                prev = memory.ToArray();
            }
            File.WriteAllBytes(Path.Combine(dir, "제 1계층.zip"), prev);
            File.WriteAllText(Path.Combine(dir, "읽어주세요.txt"), Resources.help);
        }
    }
}
