using Enlishing.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System.Diagnostics;
using static Enlishing.Dungeon;

namespace Enlishing
{
    public class Program
    {
        static void Main(string[] args)
        {
            ConsoleEx.InitConsole();
            ConsoleEx.RemoveMenus(true, true, true, false);
            ConsoleEx.SetFont("굴림체", new ConsoleEx.COORD() { X=8, Y=16 });
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "battle":
                        string dungeonPath = args[1];
                        string json = File.ReadAllText(dungeonPath);
                        Dungeon dungeon = JsonConvert.DeserializeObject<Dungeon>(json);
                        if (dungeon.hp <= 0)
                        {
                            GameOver(dungeon);
                            return;
                        }
                        int floorIndex = int.Parse(args[2]);
                        int roomIndex = int.Parse(args[3]);
                        var room = dungeon.floors[floorIndex].rooms[roomIndex];
                        if (room.cleared)
                        {
                            if (dungeon.floors.Length <= floorIndex + 1)
                            {
                                Clear(dungeon);
                                return;
                            }
                            Console.Title = "전투 종료";
                            Console.WriteLine("이 전투는 이미 클리어했습니다.");
                            Console.ReadLine();
                            return;
                        }
                        string batPath = args[4];
                        Console.Title = "전투 시작";
                        ConsoleEx.ShowConsole(false);
                        var form = new BattleForm(dungeon, floorIndex, room.isBoss ? Boss.Type.MSPaint : null);
                        form.ShowDialog();
                        dungeon.kills += form.kills;

                        ConsoleEx.ShowConsole(true);
                        ConsoleEx.FocusConsole();

                        if (form.win)
                        {
                            room.cleared = true;
                            File.WriteAllText(dungeonPath, JsonConvert.SerializeObject(dungeon));
                            File.Delete(batPath);
                            if (dungeon.floors.Length <= floorIndex + 1)
                            {
                                Clear(dungeon);
                                return;
                            }
                            if (room.hasKey && dungeon.floors.Length > floorIndex + 1)
                                File.WriteAllText(Path.Combine(new FileInfo(batPath).DirectoryName, dungeon.floors[floorIndex + 1].key), "asdf");
                            Console.Title = "전투 종료";
                            Console.WriteLine($"이번 전투에서의 처치 수: {form.kills}");
                            Console.WriteLine($"전체 처치 수: {dungeon.kills}");
                            if (room.hasKey)
                                Console.WriteLine(ConsoleEx.ColorPrefix(Color.Yellow, true) + "암호를 발견했습니다!" + ConsoleEx.ColorPostfix());
                            Console.ReadLine();
                        }
                        else
                        {
                            dungeon.hp = 0;
                            File.WriteAllText(dungeonPath, JsonConvert.SerializeObject(dungeon));
                            GameOver(dungeon);
                        }
                        return;
                    case "shop":
                        // 언젠간.... 만들지 ㅇ낳을가....
                        break;
                }
                return;
            }
            Lobby();
        }

        public static void Clear(Dungeon dungeon)
        {
            Console.Title = "게임 클리어";
            Console.SetWindowSize(120, 40);

            Console.Clear();
            Bitmap logo = Resources.clear;
            for (int i = 0; i < logo.Height; i++)
            {
                for (int j = 0; j < logo.Width; j++)
                {
                    if (logo.GetPixel(j, i).R == 0)
                        Console.Write("　");
                    else
                        Console.Write(ConsoleEx.ColorPrefix(logo.GetPixel(j, i), true) + "■");
                }
                Console.WriteLine();
            }
            Console.Write(ConsoleEx.ColorPostfix());

            Console.SetCursorPosition(0, 25);
            Console.WriteLine($"전체 처치 수: {dungeon.kills}");

            Console.ReadLine();
        }

        public static void GameOver(Dungeon dungeon)
        {
            Console.Title = "게임 오버";
            Console.SetWindowSize(120, 40);

            Console.Clear();
            Bitmap logo = Resources.gameover;
            for (int i = 0; i < logo.Height; i++)
            {
                for (int j = 0; j < logo.Width; j++)
                {
                    if (logo.GetPixel(j, i).R == 0)
                        Console.Write("　");
                    else
                        Console.Write(ConsoleEx.ColorPrefix(logo.GetPixel(j, i), true) + "■");
                }
                Console.WriteLine();
            }
            Console.Write(ConsoleEx.ColorPostfix());

            Console.SetCursorPosition(0, 25);
            Console.WriteLine($"전체 처치 수: {dungeon.kills}");

            Console.ReadLine();
        }

        public static void Lobby()
        {
            Console.SetWindowSize(160, 40);
            PrintLobby(true, false);
            PrintLobby(false, false);
            var selector = new CommonOpenFileDialog { IsFolderPicker = true, EnsurePathExists = true, Title = "던전을 생성할 위치를 정해주세요." };
            bool hover = false;
            ConsoleEx.WaitFor((x, y, btn, ctrl, flags, _) =>
            {
                bool contains = x > 61 && x < 98 && y > 25 && y < 33;
                if (btn == ConsoleEx.Button.Left && flags == ConsoleEx.EventFlags.None)
                {
                    if (contains)
                    {
                        if (selector.ShowDialog(ConsoleEx.GetConsoleWindow()) == CommonFileDialogResult.Ok)
                        {
                            Dungeon.Create(selector.FileName);
                            Console.Clear();
                            Console.WriteLine("다음 폴더에 던전이 생성되었습니다.");
                            Console.WriteLine(selector.FileName);
                            Process.Start("explorer.exe", selector.FileName);
                            return true;
                        }
                        contains = x > 61 && x < 98 && y > 25 && y < 33;
                    }
                }
                if (contains != hover)
                {
                    PrintLobby(false, contains);
                    hover = contains;
                }
                return false;
            }, false);
        }

        public static void PrintLobby(bool upperOrLower, bool hover = false)
        {
            if (upperOrLower)
            {
                Console.Clear();
                Bitmap logo = Resources.logo;
                for (int i = 0; i < logo.Height; i++)
                {
                    for (int j = 0; j < logo.Width; j++)
                    {
                        if (logo.GetPixel(j, i).R == 0)
                            Console.Write("　");
                        else
                            Console.Write(ConsoleEx.ColorPrefix(logo.GetPixel(j, i), true) + "■");
                        //Console.Write("　▨▩■"[(int)(4 * logo.GetPixel(j, i).R / 256d)]);
                    }
                    Console.WriteLine();
                }
                Console.Write(ConsoleEx.ColorPostfix());

                for (int i = 0; i < 5; i++)
                    Console.WriteLine();
            }
            else
            {
                Console.SetCursorPosition(60, 25);
                Console.Write("┌ ");
                for (int i = 0; i < 18; i++)
                    Console.Write("─ ");
                Console.Write("┐ ");

                for (int i = 0; i < 7; i++)
                {
                    Console.SetCursorPosition(60, 26 + i);
                    Console.Write("│ ");
                    if (hover)
                    {
                        Console.Write(ConsoleEx.ColorPrefix(Color.Black, true));
                        Console.Write(ConsoleEx.ColorPrefix(Color.LightGray, false));
                    }
                    if (i == 3)
                    {
                        for (int j = 0; j < 7; j++)
                            Console.Write("　");
                        Console.Write("게임시작");
                        for (int j = 0; j < 7; j++)
                            Console.Write("　");
                    }
                    else
                        for (int j = 0; j < 18; j++)
                            Console.Write("　");
                    if (hover)
                        Console.Write(ConsoleEx.ColorPostfix());
                    Console.Write("│ ");
                    Console.WriteLine();
                }

                Console.SetCursorPosition(60, 33);
                Console.Write("└ ");
                for (int i = 0; i < 18; i++)
                    Console.Write("─ ");
                Console.Write("┘ ");
                Console.WriteLine();
            }
        }
    }
}
