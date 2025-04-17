using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Figgle;
using Newtonsoft.Json;
using System.IO;
using System.Media;

namespace TextRPG
{
    public class Player // 플레이어 스테이터스 및 프로퍼티 구조화
    {
        public string Name;
        public string Job;
        public int HP;
        public int maxHP;
        public int Attack;
        public int Defense;
        public int Gold;
        public int Level;
        public int EXP;

        // 이벤트 프로퍼티
        public int CaveEvent;
        public int CaveCheckPoint;
        public int CaveBoss;

        public void Setstate(int hp, int maxHp, int attack, int defense)
        {
            HP = hp;
            maxHP = maxHp;
            Attack = attack;
            Defense = defense;
            Gold = 400;
            Level = 1;
            EXP = 0;
            CaveEvent = 0;

            if (HP > maxHP)
            {
                HP = maxHP;
            }
            else if (HP < 0)
            {
                HP = 0;
            }
            else if (Gold < 0)
            {
                Gold = 0;
            }
        }
    }
    class Equipment // 장비 데이터 구조화
    {
        public string Name;
        public string Description;
        public int AttackBonus;
        public int DefenseBonus;
        public string Slot;
        public int Gold;
    }
    public static class TextRPG
    {
        static Player player = new Player(); // 구조화한 player 내의 정보들을 불러오기 위해 인스턴스화
        static Random rand = new Random(); // 랜덤 구조 인스턴스화
        //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asset", ".wav");

        static Dictionary<string, Player> jobData = new Dictionary<string, Player>();
        static void SetjobData() // 직업별 스탯 Dictionary화
        {
            jobData["전사"] = new Player();
            jobData["전사"].Setstate(150, 150, 10, 5);

            jobData["기사"] = new Player();
            jobData["기사"].Setstate(100, 100, 10, 10);

            jobData["도적"] = new Player();
            jobData["도적"].Setstate(100, 100, 15, 5);
        }
        static void CopyStatsFromJob(Player target, Player source) // 스탯을 붙여넣을 기능 추가
        {
            target.Setstate(source.HP, source.maxHP, source.Attack, source.Defense);
        }

        static Dictionary<string, Equipment> equipData = new Dictionary<string, Equipment>() // 장비 데이터 (아이템 이름이 Dictionary에서 불러오는 키값)
        {
            { "검은 얼룩의 검", new Equipment { Name = "검은 얼룩의 검", Description = "깨어나보니 쥐고있던 검", AttackBonus = 0, DefenseBonus = 0, Slot = "무기" } },
            { "검게 얼룩진 옷", new Equipment { Name = "검은 얼룩진 옷", Description = "깨어나보니 입고있던 옷", AttackBonus = 0, DefenseBonus = 0, Slot = "몸통" } },
            { "일반적인 검", new Equipment { Name = "일반적인 검", Description = "특별한 점이 없는 일반적인 장검", AttackBonus = 5, DefenseBonus = 0, Slot = "무기", Gold = 1500 } },
            { "패링 대거", new Equipment { Name = "패링 대거", Description = "공격과 방어를 적절하게 할 수 있는 단검", AttackBonus = 2, DefenseBonus = 2, Slot = "무기", Gold = 1500 } },
            { "목제 방패", new Equipment { Name = "목제 방패", Description = "나무로 만들어진 흔한 방패", AttackBonus = 0, DefenseBonus = 1, Slot = "방패", Gold = 500 } },
            { "철 방패", new Equipment { Name = "철 방패", Description = "철로 만들어진 조금 무거운 방패", AttackBonus = 0, DefenseBonus = 2, Slot = "방패", Gold = 1000 } },
            { "레더 아머", new Equipment { Name = "레더 아머", Description = "단단한 동물의 가죽으로 만들어진 방어구", AttackBonus = 0, DefenseBonus = 2, Slot = "몸통", Gold = 1000 } },
            { "아이언 아머", new Equipment { Name = "아이언 아머", Description = "치명상도 막아주는 튼튼한 방어구", AttackBonus = 0, DefenseBonus = 3, Slot = "몸통", Gold = 1500 } },
            { "불온한 느낌의 검은 구슬", new Equipment { Name = "불온한 느낌의 검은 구슬", Description = "용도를 모르겠는 구슬", AttackBonus = 0, DefenseBonus = 0, Slot = "아이템", Gold = 10000 } },
            { "음울한 발톱", new Equipment { Name = "음울한 발톱", Description = "검은 짐승의 거대한 발톱에서 떨어져나온 불길한 검", AttackBonus = 10, DefenseBonus = 0, Slot = "무기", Gold = 0 } }
        };
        static List<string> equippedItems = new List<string>(); // 장착 데이터
        static List<string> inventory = new List<string>() // 인벤토리 데이터
        {
            "검은 얼룩의 검",
            "검게 얼룩진 옷"
        };
        class Monster // 몬스터 구조화
        {
            public string MonName;
            public int HP;
            public int maxHP;
            public int Attack;
            public int Defense;
            public int GoldReward;
            public int EXPReward;

            public Monster(string name, int hp, int attack, int defense, int gold, int exp)
            {
                MonName = name;
                HP = hp;
                maxHP = hp;
                Attack = attack;
                Defense = defense;
                GoldReward = gold;
                EXPReward = exp;

                if (HP > maxHP)
                {
                    HP = maxHP;
                }
                else if (HP < 0)
                {
                    HP = 0;
                }
            }
        }
        static Dictionary<string, Monster> monsterData = new Dictionary<string, Monster>() // 몬스터 데이터 (몬스터 이름이 Dictionary에서 불러오는 키값)
        {
            { "고블린", new Monster("고블린", 35, 5, 3, 75, 15) },
            { "홉고블린", new Monster("홉고블린", 50, 7, 1, 90, 20) },
            { "트롤", new Monster("트롤", 60, 5, 0, 150, 25) },
            { "슬라임", new Monster("슬라임", 15, 4, 9, 75, 20) },
            { "검은 짐승", new Monster("검은 짐승", 100, 50, 15, 3000, 100) }
        };

        static void ShowText(string text, int delay = 0) // 대사 제어
        {
            Console.WriteLine(text);
            if (delay > 0)
            {
                Thread.Sleep(delay);
            }
        }
        static void DelKeyBuffer() // 키 입력 버퍼 지우기
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        static void Main()
        {
            SetjobData();
            StartEquip();
            Title();
        }

        static void Title() // 타이틀 화면
        {
            PlayBGM("Asset/Title.wav");
            string asciiTitle = FiggleFonts.Standard.Render("RUINED DOLL");
            string[] asciiLines = asciiTitle.Split('\n');

            // 아스키 아트의 최대 너비 구하기
            int maxLineLength = 0;
            foreach (string line in asciiLines)
            {
                if (line.Length > maxLineLength)
                    maxLineLength = line.Length;
            }

            int consoleWidth = Console.WindowWidth;
            int titleStartX = (consoleWidth - maxLineLength) / 2;

            for (int i = 0; i < asciiLines.Length; i++)
            {
                Console.SetCursorPosition(titleStartX, i);
                ShowText(asciiLines[i]);
            }

            string startText = "START";
            int startX = titleStartX + (maxLineLength - startText.Length) / 2;
            int startY = asciiLines.Length + 1;

            while (true)
            {
                // START 출력
                Console.SetCursorPosition(startX, startY);
                ShowText(startText, 500);

                // START 지우기
                Console.SetCursorPosition(startX, startY);
                ShowText(new string(' ', startText.Length), 500);

                // 종료 조건 (아무 키 입력)
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);

                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift)) // 저장파일 불러오기
                    {
                        if (!LoadGame())
                        {
                            ShowText("Fail", 1000);
                            Console.ReadKey(true);
                            Title();
                        }
                    }
                    else
                    {
                        FirstScene(); // 테스트용 메서드 호출 위치
                    }
                    return;
                }
            }
        }
        static void FirstScene() // 첫 씬
        {
            Console.Clear();
            ShowText("*이름없는 동굴*\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            ShowText("너는 눈을 떴다.", 1000);
            ShowText("동굴 속 어딘가인 것 같다.", 1000);
            ShowText("\n(......)");
            DelKeyBuffer();
            Console.ReadKey(true);

            SelectJob();
        }
        static void SelectJob() // 직업 선택
        {
            while (true)
            {
                Console.Clear();
                ShowText("너는 어떠한 자였는가?\n", 500);
                ShowText("1. 전사\n2. 기사\n3. 도적\n");
                DelKeyBuffer();
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        player.Job = "전사";
                        CopyStatsFromJob(player, jobData["전사"]);
                        break;
                    case "2":
                        player.Job = "기사";
                        CopyStatsFromJob(player, jobData["기사"]);
                        break;
                    case "3":
                        player.Job = "도적";
                        CopyStatsFromJob(player, jobData["도적"]);
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
            SelectItem();
        }
        static void SelectItem() // 아이템 선택
        {
            while (true)
            {
                Console.Clear();
                ShowText("너는 지금 어떤 행동을 하려는가?\n", 500);
                ShowText("1. 경계하며 이 곳을 빠져나가야겠다.\n2. 주변의 위협 요소를 찾는다.\n3. 몸 상태를 확인한다.\n4. 내가 현재 무엇을 가지고 있는지 확인한다.\n");
                DelKeyBuffer();
                string addStatus = Console.ReadLine();

                switch (addStatus)
                {
                    case "1":
                        player.Defense += 2;
                        break;
                    case "2":
                        player.Attack += 3;
                        break;
                    case "3":
                        player.maxHP += 25;
                        player.HP += 25;
                        break;
                    case "4":
                        player.Gold += 500;
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
            SelectName();
        }

        static void SelectName() // 이름 결정
        {
            Console.Clear();
            ShowText("너의 이름은 무엇인가?\n");
            DelKeyBuffer();
            player.Name = Console.ReadLine();

            Console.Clear();
            ShowText($"너는 전직 {player.Job}, 이름은 {player.Name}.\n", 1000);
            ShowText("1. 그렇다.\n2. 아니다.\n");
            DelKeyBuffer();
            string next = Console.ReadLine();
            if (next != "1")
            {
                ShowText("너는 다시 생각했다.");
                DelKeyBuffer();
                Console.ReadKey(true);
                SelectJob();
            }

            Console.Clear();
            player.HP = 1;
            ShowText("*이름없는 동굴*\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            ShowText("저 멀리 빛이 보인다.", 1000);
            ShowText("너는 그 곳을 향해 걸어나간다.", 1000);
            ShowText("\n(......)");
            DelKeyBuffer();
            Console.ReadKey(true);

            StartVillage();
        }
        static void StartVillage()
        {
            PlayBGM("Asset/village.wav");
            Console.Clear();
            ShowText("*작은 마을*\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            ShowText("너는 동굴에서 나와 멀리 보이던 작은 마을에 도착했다.", 1000);
            ShowText("한 노인이 너를 발견하고 다가온다.\n", 1000);
            ShowText("\"자네 꼴이 왜 그런가?\"\n", 1000);
            ShowText("너는 상황을 설명했다.\n", 1000);
            ShowText("\"여기서 북쪽의 언덕을 넘으면 큰 도시가 있다네.\"", 1000);
            ShowText("\"그곳에서 필요한 걸 구할 수 있을걸세.\"", 1000);
            ShowText("\n(......)");
            DelKeyBuffer();
            Console.ReadKey(true);

            Console.Clear();
            ShowText("*작은 마을*\n");
            DelKeyBuffer();
            ShowText("\"자네... 보아하니 평범한 사람으로는 안보이는구먼...\"", 1000);
            ShowText("\"요즘 흉흉한 소문도 돌고있으니...\"", 1000);
            ShowText("\"되도록이면 혼자 폐허나 동굴을 가는 것은 피하게나.\"\n", 1000);
            ShowText("너는 가볍게 감사를 표시하고 북쪽으로 향한다.", 1000);
            ShowText("\n(......)");
            DelKeyBuffer();
            Console.ReadKey(true);

            FirstMendacium();
        }
        static void FirstMendacium()
        {
            PlayBGM("Asset/Mendacium.wav");
            Console.Clear();
            ShowText("*도시 멘더시움*\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            ShowText("너는 도시에 도착했다.", 1000);
            ShowText("꽤 많은 인파가 눈에 띄고있다.", 1000);
            ShowText("앞으로 너는 이곳을 거점삼아 움직이려고 한다.", 1000);
            DelKeyBuffer();
            Console.ReadKey(true);

            Mendacium();
        }
        static void Mendacium() // 멘더시움
        {
            PlayBGM("Asset/Mendacium.wav");
            while (true)
            {
                Console.Clear();
                ShowText("*도시 멘더시움*\n", 1000);
                ShowText("무엇을 할 것인가?\n");
                ShowText("1. 상태 보기\n2. 인벤토리 확인\n3. 상점\n4. 여관\n5. 이름없는 동굴\n6. 데이터 저장", 1000);
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        Status();
                        break;
                    case "2":
                        Inventory();
                        break;
                    case "3":
                        Store();
                        break;
                    case "4":
                        Inn();
                        break;
                    case "5":
                        NamelessCave();
                        break;
                    case "6":
                        SaveGame();
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
        }
        static void Status() // 스테이터스
        {
            Console.Clear();
            ShowText("<상태>\n");
            ShowText($"이름 : {player.Name}");
            ShowText($"레벨 : {player.Level}");
            ShowText($"직업 : {player.Job}");
            ShowText($"공격력 : {player.Attack}");
            ShowText($"방어력 : {player.Defense}");
            ShowText($"체력 : {player.HP} / {player.maxHP}");
            ShowText($"보유 골드 : {player.Gold}");
            ShowText($"현재 경험치 : {player.EXP}");
            ShowText("\n돌아간다.\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            Console.ReadLine();

            Mendacium();
        }
        static void EXPSystem(int gainedEXP) // 경험치 시스템
        {
            player.EXP += gainedEXP;

            while (player.EXP >= 100)
            {
                int attackUp = 2;
                int defenseUp = 1;
                int maxhpUp = 10;

                player.Level++;
                player.EXP -= 100;

                Console.Clear();
                ShowText("너는 강해진 것을 느낀다.", 500);
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText($"레벨 : {player.Level}");
                ShowText($"공격력 : {player.Attack} + {attackUp}");
                ShowText($"방어력 : {player.Defense} + {defenseUp}");
                ShowText($"체력 : {player.HP} / {player.maxHP} + {maxhpUp}");

                player.Attack += attackUp;
                player.Defense += defenseUp;
                player.maxHP += maxhpUp;
                player.HP = player.maxHP;
            }
            Console.ReadKey(true);
        }
        static void Inventory() // 인벤토리
        {
            while (true)
            {
                Console.Clear();
                ShowText("<인벤토리>\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText("무엇을 할 것인가?\n", 500);
                ShowText("1. 아이템 관리\n2. 그만둔다.\n");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        EquipManager();
                        break;
                    case "2":
                        Mendacium();
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
        }
        static void EquipManager() // 아이템 관리
        {
            while (true)
            {
                Console.Clear();
                ShowText("<아이템 관리>\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText("너는 현재 가지고 있는 물건들을 확인했다.\n", 1000);
                ShowEquipment();
                DelKeyBuffer();
                ShowText("\n어떻게 할 것인가?", 1000);
                ShowText("\n1. 장착하기\n2. 그만둔다.\n");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        EquipMenu();
                        break;
                    case "2":
                        Mendacium();
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
        }
        static void ShowEquipment() // 내 인벤토리 아이템 보여주는 기능
        {
            ShowText("<내 인벤토리>");

            List<string> equippable = inventory // 인벤토리에서
                .Where(item => equipData.ContainsKey(item)) // equipData 딕셔너리에 등록된 아이템 리스트화해서 가져옴
                .ToList();

            if (equippable.Count == 0)
            {
                ShowText("아이템 없음");
            }
            else
            {
                for (int i = 0; i < equippable.Count; i++)
                {
                    string ItemName = equippable[i]; // 리스트화 한 equipData아이템을 첫번째부터 출력
                    Equipment eq = equipData[ItemName];

                    ShowText($"{ItemName} : {eq.Description} / 공격력 + {eq.AttackBonus} / 방어력 + {eq.DefenseBonus}");
                }
            }
        }
        static void StartEquip()
        {
            inventory.Clear();
            inventory.Add("검은 얼룩의 검");
            inventory.Add("검게 얼룩진 옷");
            foreach (string itemName in inventory)
            {
                Equipment item = equipData[itemName];
                equippedItems.Add(itemName);
                player.Attack += item.AttackBonus;
                player.Defense += item.DefenseBonus;
            }
        }
        static void EquipMenu()
        {
            while (true)
            {
                Console.Clear();
                ShowText("<장비 확인>\n");

                List<string> equippable = inventory
                    .Where(item => equipData.ContainsKey(item))
                    .ToList();

                if (equippable.Count == 0)
                {
                    ShowText("아이템 없음");
                    return;
                }
                for (int i = 0; i < equippable.Count; i++)
                {
                    string ItemName = equippable[i];
                    Equipment item = equipData[ItemName];
                    bool isEquipped = equippedItems.Contains(ItemName);
                    string mark = isEquipped ? "[E]" : "";

                    ShowText($"{i + 1}. {mark} {ItemName} : {item.Description} / 공격력 + {item.AttackBonus} / 방어력 + {item.DefenseBonus}");
                }

                ShowText("\n너는 어떤 장비를 선택하려는가?\n", 1000);
                ShowText("0. 그만둔다.");
                DelKeyBuffer();
                string input = Console.ReadLine();

                if (input == "0")
                {
                    EquipManager();
                }

                if (int.TryParse(input, out int select) && select >= 1 && select <= equippable.Count)
                {
                    string selectedItem = equippable[select - 1];
                    Equipment item = equipData[selectedItem];

                    if (equippedItems.Contains(selectedItem))
                    {
                        equippedItems.Remove(selectedItem);
                        player.Attack -= item.AttackBonus;
                        player.Defense -= item.DefenseBonus;
                        ShowText("너는 선택한 장비를 해제했다.", 1000);
                        DelKeyBuffer();
                    }
                    else
                    {
                        equippedItems.Add(selectedItem);
                        player.Attack += item.AttackBonus;
                        player.Defense += item.DefenseBonus;
                        ShowText("너는 선택한 장비를 장착했다.", 1000);
                        DelKeyBuffer();
                    }
                }
                else
                {
                    ShowText("너는 다시 생각했다.", 1000);
                }
            }
        }

        static void Store() // 상점
        {
            while (true)
            {
                Console.Clear();
                ShowText("*멘더시움 상점*\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText("필요한 아이템이 있을지도 모른다.", 1000);
                ShowText("무엇을 할 것인가?\n", 1000);
                ShowText($"<현재 골드 : {player.Gold}G>");

                for (int i = 0; i < shopItems.Count; i++)
                {
                    string itemName = shopItems[i];
                    Equipment item = equipData[itemName];
                    bool isAlreadyBought = purchasedItems.Contains(itemName);

                    string status = isAlreadyBought ? "[재고 없음]" : $"[{item.Gold}G]";
                    ShowText($"{i + 1}. {itemName} {status} : {item.Description} / 공격력 + {item.AttackBonus} / 방어력 + {item.DefenseBonus}");
                }
                ShowText("\n아이템 구매 (번호 선택)", 1000);
                ShowText("0. 그만둔다.");
                DelKeyBuffer();
                string input = Console.ReadLine();

                if (input == "0")
                {
                    ShowText("너는 인사를 하고 상점을 뒤로했다.", 1000);
                    DelKeyBuffer();
                    Console.ReadKey(true);
                    Mendacium();
                }

                if (int.TryParse(input, out int select) && select >= 1 && select <= shopItems.Count)
                {
                    string selectedItem = shopItems[select - 1];

                    if (purchasedItems.Contains(selectedItem))
                    {
                        ShowText("이미 구매한 아이템이다.", 1000);
                        DelKeyBuffer();
                        continue;
                    }

                    Equipment item = equipData[selectedItem];

                    if (player.Gold < item.Gold)
                    {
                        ShowText("가진 골드가 부족하다.", 1000);
                        DelKeyBuffer();
                        continue;
                    }

                    player.Gold -= item.Gold;
                    inventory.Add(selectedItem);
                    purchasedItems.Add(selectedItem);

                    if (selectedItem == "불온한 느낌의 검은 구슬")
                    {
                        ShowText("......", 1000);
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        Console.Clear();
                        ShowText("*멘더시움 상점*\n");
                        ShowText("\"자네...\"", 1500);
                        ShowText("상점 주인이 어두운 얼굴로 너에게 말을 건다.", 2000);
                        ShowText("\"정말로 그 가격으로 그걸 살 생각이오?\"", 1500);
                        ShowText("\"나야 상관없소만...\"", 1500);
                        ShowText("\"그 구슬 아름다우면서 뭔가 꺼림찍하단 말이지.\"", 1500);
                        ShowText("\"나라면 가까이하고 싶지 않소만, 보석으로써 가치는 있을거요.\"", 1500);
                        ShowText("\"애물단지를 사줘서 고맘소.\"\n", 1500);
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        ShowText("너는 묘한 느낌을 받으며 구슬을 바라본다.", 1000);
                        ShowText("(......)", 1000);
                    }

                    ShowText($"너는 {item.Gold}G를 지불하여 {selectedItem}을 구매했다.", 1000);
                    DelKeyBuffer();
                    Console.ReadKey(true);
                }
                else
                {
                    ShowText("너는 다시 생각했다.", 1000);
                }
            }
        }
        static List<string> shopItems = new List<string> // 상점 판매 아이템 리스트 (여기에 써야 보임)
        {
            "일반적인 검",
            "패링 대거",
            "목제 방패",
            "철 방패",
            "레더 아머",
            "아이언 아머",
            "불온한 느낌의 검은 구슬"
        };
        static List<string> purchasedItems = new List<string>(); // 구매된 아이템 리스트

        static void Inn() // 여관 휴식 기능
        {
            int cost = 100;

            while (true)
            {
                Console.Clear();
                ShowText("*멘더시움 여관*\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText($"현재 체력 : {player.HP} / {player.maxHP}");
                ShowText($"현재 골드 : {player.Gold}G");
                ShowText("너는 여관에 들어갔다.\n", 1000);
                ShowText("너는 무엇을 할 것인가?\n", 1000);
                ShowText("1. 휴식한다.[100G]\n2. 그만둔다.\n");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        if (player.CaveEvent == 0 && player.HP <= 100 && player.Gold < 100) // 첫 방문인데 골드를 다 써버려서 HP회복을 못하는 경우
                        {
                            Console.Clear();
                            ShowText("*멘더시움 여관*\n");
                            ShowText("여관 주인이 너를 바라본다.", 1000);
                            ShowText("\"당신, 어디서 무엇을 했길래 꼴이 그래?\"", 1000);
                            ShowText("......", 1000);
                            ShowText("\"무일푼으로 보이는데, 곤란해 보이니 하루 정도는 방을 빌려줄게.\"", 1000);
                            ShowText("\"대신 다음부터 자주 이용해달라고.\"", 1000);
                            player.HP = player.maxHP;
                            DelKeyBuffer();
                            Console.ReadKey(true);
                            Inn();
                        }
                        else
                        {
                            if (player.Gold >= cost)
                            {
                                player.Gold -= cost;
                                player.HP = player.maxHP;
                                ShowText("너는 여관에 들어가 잠시 쉴 방을 대여했다.", 1000);
                                ShowText("......", 2000);
                                ShowText("너는 몸이 완전히 회복된 것을 느낀다.", 1000);
                                DelKeyBuffer();
                                Console.ReadKey(true);
                                Inn();
                            }
                            else
                            {
                                ShowText("너는 휴식을 위한 방을 빌리려고 했으나, 가진 골드가 부족했다.", 1000);
                                ShowText("너는 겸연쩍은 미소를 지으며 여관을 뒤로하고 돌아갔다.", 1000);
                                ShowText("(......)", 1000);
                                DelKeyBuffer();
                                Console.ReadKey(true);
                                Mendacium();
                            }
                        }
                        break;
                    case "2":
                        ShowText("너는 인사를 하고 여관을 뒤로했다.", 1000);
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        Mendacium();
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
                break;
            }
        }
        static void NamelessCaveEvent()
        {
            PlayBGM("Asset/Dungeon.wav");
            Console.Clear();
            ShowText("*이름없는 동굴*\n");
            DelKeyBuffer();
            Console.ReadKey(true);
            ShowText("너는 너가 깨어났던 그 동굴 속으로 다시 도착했다.", 1000);
            ShowText("이전과 다르게 동굴 안이 조금 밝아보인다.", 1000);
            ShowText("너는 마음을 다잡고 동굴 깊숙한 곳으로 발을 내딛였다.", 1000);
            DelKeyBuffer();
            Console.ReadKey(true);
            player.CaveEvent = 1; // 첫회용 제어 프로퍼티
            NamelessCave();
        }
        static void NamelessCave()
        {
            PlayBGM("Asset/Dungeon.wav");
            if (player.CaveEvent == 0 && player.HP == player.maxHP) // 이벤트를 본 적 없고 풀피일때 최초 이벤트
            {
                NamelessCaveEvent();
            }
            else if (player.CaveEvent == 0 && player.HP == 1)
            {
                Console.Clear();
                ShowText("*이름없는 동굴*\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText("너는 이곳을 탐색하기에 지금 상태가 좋지 않다.", 1000);
                ShowText("몸을 회복시키고 다시 오기로 했다.", 1000);
                DelKeyBuffer();
                Console.ReadKey(true);
                Mendacium();
            }
            Console.Clear();
            ShowText("*이름없는 동굴*\n");
            Dungeon1();
        }
        static void Dungeon1()
        {
            PlayBGM("Asset/Dungeon.wav");
            int floor = 1;
            string[] randomTexts = {
                "어두운 공간이 계속된다.",
                "뭔가 바람소리 같은 것이 들리는듯 하다.",
                "어둠에 점점 익숙해지는 느낌이다.",
                "작은 기척이 느껴진다.",
                "긴장감이 흐른다."
            }; // 랜덤 대사 배열로 세팅
            string randomText = randomTexts[rand.Next(randomTexts.Length)];

            if (player.CaveCheckPoint == 2)
            {
                ShowText("너는 워프 장치를 이용할 수 있다.\n", 1000);
                ShowText("1. 19 구역으로 간다.\n2. 10 구역으로 간다.\n3. 1 구역으로 간다.");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        floor = 19;
                        break;
                    case "2":
                        floor = 10;
                        break;
                    case "3":
                        floor = 1;
                        break;
                }
            }
            else if (player.CaveCheckPoint == 1)
            {
                ShowText("너는 워프 장치를 이용할 수 있다.\n", 1000);
                ShowText("1. 10 구역으로 간다.\n2. 1 구역으로 간다.");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch (select)
                {
                    case "1":
                        floor = 10;
                        break;
                    case "2":
                        floor = 1;
                        break;
                }
            }
            while (true)
            {
                Console.Clear();
                ShowText("*이름없는 동굴*\n");
                ShowText($"구역 : {floor} / 20\n");
                DelKeyBuffer();
                Console.ReadKey(true);
                ShowText(randomText, 1000);
                ShowText("\n1. 앞으로 나아간다.\n2. 돌아간다.\n");
                DelKeyBuffer();
                string select = Console.ReadLine();

                switch(select)
                {
                    case "1":
                        if (floor < 20)
                        {
                            if (floor == 10)
                            {
                                ShowText("너는 이상한 워프 장치를 발견했다.", 1000);
                                ShowText("다음에는 이 구역에서부터 시작할 수 있을 것 같다.", 1000);
                                player.CaveCheckPoint = 1;
                            }
                            if (floor == 19)
                            {
                                ShowText("너는 이상한 워프 장치를 발견했다.", 1000);
                                ShowText("다음에는 이 구역에서부터 시작할 수 있을 것 같다.", 1000);
                                player.CaveCheckPoint = 2;
                            }
                            floor++;
                            RandomEvent(floor);
                        }
                        else
                        {
                            if (player.CaveBoss == 0)
                            {
                                Monster boss = new Monster(
                                    monsterData["검은 짐승"].MonName,
                                    monsterData["검은 짐승"].HP,
                                    monsterData["검은 짐승"].Attack,
                                    monsterData["검은 짐승"].Defense,
                                    monsterData["검은 짐승"].GoldReward,
                                    monsterData["검은 짐승"].EXPReward
                                );
                                // 최종 보스
                                ShowText("동굴의 최심부에 도착한 너는 심상치 않은 기운을 느낀다.", 1000);
                                ShowText("너는 무언가의 기척을 느낀다.", 1000);
                                ShowText("...", 1500);
                                ShowText("어둠 속에서 붉게 빛나는 2개의 점이 보인다.", 1000);
                                ShowText("......", 2000);
                                ShowText("무언가가 너를 향해 달려들었다.", 1000);
                                DelKeyBuffer();
                                Console.ReadKey(true);
                                Battle(boss);
                                PlayBGM("Asset/Boss1.wav");
                            }
                            else
                            {
                                ShowText("......", 1500);
                                ShowText("동굴의 내부에는 더이상 길이 없다.", 1000);
                                ShowText("너는 이 동굴을 훌륭하게 탐색해냈다.", 1000);
                                ShowText("너는 마을로 돌아가기로 했다.", 1000);
                                DelKeyBuffer();
                                Console.ReadKey(true);

                                Mendacium();
                            }
                        }
                        break;
                    case "2":
                        ShowText("......", 2000);
                        ShowText("더 안쪽으로 들어가기에 위험하다고 판단한 너는 길을 되돌아갔다.", 1000);
                        DelKeyBuffer();
                        Console.ReadKey(true);

                        Mendacium();
                        break;
                    default:
                        ShowText("너는 다시 생각했다.");
                        DelKeyBuffer();
                        Console.ReadKey(true);
                        continue;
                }
            }
        }
        static void RandomEvent(int floor) // 던전 랜덤 이벤트 관리
        {
            int randEvent = rand.Next(1, 101);

            if (randEvent <= 60)
            {
                ShowText("\n너는 앞으로 걸어나간다.", 2000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                Monster monster = GetRandomMonster();
                ShowText($"너는 몬스터의 습격을 받았다.", 1000);
                DelKeyBuffer();
                Console.ReadKey(true);
                Battle(GetRandomMonster());
            }
            else if (randEvent > 60 && randEvent <= 75)
            {
                int gold = rand.Next(25, 51);
                player.Gold += gold;
                ShowText("\n너는 앞으로 걸어나간다.", 2000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                ShowText($"다음 구역으로 향하기 전, 너는 바닥에서 {gold}G를 발견했다.", 1000);
                DelKeyBuffer();
                Console.ReadKey(true);
            }
            else
            {
                ShowText("\n너는 앞으로 걸어나간다.", 2000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                ShowText("......", 1000);
                ShowText("안쪽으로 더 들어갈 수 있는 공간이 보인다.", 1000);
                DelKeyBuffer();
                Console.ReadKey(true);
            }
        }
        static void Battle(Monster monster) // 전투 시스템
        {
            bool isPlayerDoubleAttack = false;
            bool isMonsterDoubleAttack = false;

            while (player.HP > 0 && monster.HP > 0) // 어느 한쪽의 HP가 0이 될 때까지 지속
            {
                Console.Clear();
                ShowText("*이름없는 동굴*\n");
                ShowText($"{monster.MonName}은(는) 너에게 강한 적의를 품고있다.\n", 1000);
                ShowText($"[{monster.MonName}] HP : {monster.HP} / {monster.maxHP}");
                ShowText($"[{player.Name}] HP : {player.HP} / {player.maxHP}");
                ShowText("\n너는 어떤 행동을 할 지 선택해야한다.");
                ShowText("1. 공격한다.\n2. 방어한다\n3. 도망친다.\n", 1000);
                DelKeyBuffer();
                string playerChoice = Console.ReadLine();

                string monsterChoice = GetMonsterAction();

                int plDie1 = rand.Next(1, 7); // 1번 주사위  1 ~ 6
                int plDie2 = rand.Next(1, 7); // 2번 주사위  1 ~ 6
                int msDie1 = rand.Next(1, 7); // 1번 주사위  1 ~ 6
                int msDie2 = rand.Next(1, 7); // 2번 주사위  1 ~ 6

                int plSum = plDie1 + plDie2;
                int msSum = msDie1 + msDie2;

                bool plDouble = (plDie1 == plDie2) && rand.Next(1, 101) <= 30; // 주사위가 같으면 30% 확률로 크리티컬
                bool msDouble = (msDie1 == msDie2) && rand.Next(1, 101) <= 30;

                switch(playerChoice)
                {
                    case "1":
                        playerChoice = "공격";
                        break;
                    case "2":
                        playerChoice = "방어";
                        break;
                    case "3":
                        playerChoice = "도주";
                        break;
                }

                Console.Clear();
                ShowText("*이름없는 동굴*\n");
                ShowText($"너는 {playerChoice}을(를) 취했다.", 1500);
                ShowText($"값 : {plDie1} + {plDie2} = {plSum}", 500);
                ShowText($"{monster.MonName}은(는) {monsterChoice}을(를) 취했다.", 1500);
                ShowText($"값 : {msDie1} + {msDie2} = {msSum}", 500);

                // 전투 계산
                if (playerChoice == "공격" && monsterChoice == "공격")
                {
                    if (plSum > msSum)
                    {
                        int damage = player.Attack + plSum - monster.Defense;
                        if (isPlayerDoubleAttack || plDouble)
                        {
                            damage *= 2;
                            ShowText($"너는 위협적인 공격을 준비한다.", 1000);
                        }
                        monster.HP -= damage;
                        ShowText($"{monster.MonName}은(는) {damage}의 대미지를 입었다.", 1500);
                    }
                    else if (plSum < msSum)
                    {
                        int damage = monster.Attack + msSum - player.Defense;
                        if (isMonsterDoubleAttack || msDouble)
                        {
                            damage *= 2;
                            ShowText($"{monster.MonName}은(는) 위협적인 공격을 준비한다.", 1000);
                        }
                        player.HP -= damage;
                        ShowText($"너는 {damage}의 대미지를 입었다.", 1500);
                    }
                    else
                    {
                        ShowText($"너는 {monster.MonName}과(와) 공격을 서로 주고받았다.", 1500);
                    }
                    isPlayerDoubleAttack = false; // 초기화
                    isMonsterDoubleAttack = false;
                }
                else if (playerChoice == "공격" && monsterChoice == "방어")
                {
                    if (plSum > msSum)
                    {
                        int damage = plSum - msSum;
                        if (isPlayerDoubleAttack || plDouble)
                        {
                            damage *= 2;
                            ShowText("너는 위협적인 공격을 준비한다.", 1000);
                        }
                        monster.HP -= damage;
                        ShowText($"{monster.MonName}은(는) 방어했지만 {damage}의 대미지를 입었다.", 1500);
                    }
                    else
                    {
                        ShowText($"{monster.MonName}은(는) 너의 공격을 완벽하게 방어했다.", 1500);
                        isMonsterDoubleAttack = true; // 완벽 방어 보너스
                    }
                    isPlayerDoubleAttack = false;
                }
                else if (playerChoice == "방어" && monsterChoice == "공격")
                {
                    if (plSum < msSum)
                    {
                        int damage = msSum - plSum;
                        if (isMonsterDoubleAttack || msDouble)
                        {
                            damage *= 2;
                            ShowText($"{monster.MonName}은(는) 위협적인 공격을 준비한다.", 1000);
                        }
                        monster.HP -= damage;
                        ShowText($"너는 방어했지만 {damage}의 대미지를 입었다.", 1500);
                    }
                    else
                    {
                        ShowText($"너는 {monster.MonName}의 공격을 완벽하게 방어했다.", 1500);
                        isPlayerDoubleAttack = true; // 완벽 방어 보너스
                    }
                    isMonsterDoubleAttack = false;
                }
                else if (playerChoice == "방어" && monsterChoice == "방어")
                {
                    ShowText($"너와 {monster.MonName}은(는) 서로를 경계중이다.", 1500);
                    isPlayerDoubleAttack = false; // 초기화
                    isMonsterDoubleAttack = false;
                }
                else if (playerChoice == "도주")
                {
                    int goldloss = rand.Next(1, 101);
                    int gold = rand.Next(25, 101);
                    if (plSum >= 6 && plSum <= 8) // 약 44.4% 확률로 도망 가능
                    {
                        Console.Clear();
                        ShowText("...", 1000);
                        ShowText("......", 1000);
                        ShowText(".........", 1000);
                        ShowText("너는 생존을 위해 무작정 되돌아간다.", 1000);
                        if (goldloss <= 40) // 40% 확률로 Gold 손실
                        {
                            ShowText($"너는 도망치며 {gold}G를 떨어뜨렸지만, 멈추지 않았다.", 1500);
                            player.Gold -= gold;
                        }
                        Mendacium();
                    }
                    else
                    {
                        ShowText("너는 전투를 포기하려 했지만 발이 떨어지지 않았다.", 1500);
                        if (monsterChoice == "공격")
                        {
                            int damage = monster.Attack + msSum - player.Defense;
                            if (isMonsterDoubleAttack || msDouble)
                            {
                                damage *= 2;
                                ShowText($"{monster.MonName}은(는) 위협적인 공격을 준비한다.", 1000);
                            }
                            player.HP -= damage;
                            ShowText($"너는 {damage}의 대미지를 입었다.", 1500);
                        }
                    }
                    isPlayerDoubleAttack = false; // 초기화
                    isMonsterDoubleAttack = false;
                }
                ShowText("너는 숨이 가빠지는 것을 느낀다.");
                DelKeyBuffer();
                Console.ReadKey(true);
            }
            // 전투 종료
            if (player.HP <= 0)
            {
                ShowText("너는 서서히 의식을 잃어간다.", 1500);
                ShowText("...", 1500);
                ShowText("......", 2000);
                ShowText(".........", 3000);
                player.Gold = 0;
                player.HP = player.maxHP;
                DelKeyBuffer();
                Console.ReadKey(true);

                Mendacium();
            }
            else if (monster.HP <= 0)
            {
                ShowText($"{monster.MonName}을(를) 처치했다.", 1500);
                ShowText($"너는 {monster.GoldReward}G와 {monster.EXPReward} 만큼의 경험치를 얻었다.", 1000);
                player.Gold += monster.GoldReward;
                EXPSystem(monster.EXPReward); // 경험치 계산
                if (monster.MonName == "검은 짐승")
                {
                    ShowText($"너는 {monster.MonName}의 사체에서 무언가를 발견했다.", 1500);
                    inventory.Add("음울한 발톱");
                    ShowText("너는 전리품을 챙겨 마을로 돌아왔다.", 1000);
                    DelKeyBuffer();
                    Console.ReadKey(true);

                    Mendacium();
                }
                DelKeyBuffer();
                Console.ReadKey(true);
            }
        }
        static Monster GetRandomMonster() // 랜덤 몬스터 조우 기능 (Dictionary에서 랜덤 불러옴)
        {
            string key;
            do
            {
                var keys = monsterData.Keys.ToList();
                key = keys[rand.Next(keys.Count)];
            }
            while (key.Contains("검은 짐승")); // 보스가 걸리면 다시 돌림
            
            Monster baseMonster = monsterData[key];

            return new Monster(
                baseMonster.MonName,
                baseMonster.HP,
                baseMonster.Attack,
                baseMonster.Defense,
                baseMonster.GoldReward,
                baseMonster.EXPReward
                
            );
        }
        static string GetMonsterAction() // 랜덤 몬스터 행동 기능
        {
            int choice = rand.Next(1, 101);
            return (choice <= 66) ? "공격" : "방어"; // 66% 확률로 공격
        }
        static void SaveGame() // 저장 기능
        {
            GameData data = new GameData
            {
                Player = player,
                Inventory = inventory,
                EquippedItems = equippedItems,
                PurchasedItems = purchasedItems
            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("save.json", json);
            ShowText("게임이 저장되었습니다.", 1000);
            Mendacium();
        }
        static bool LoadGame() // 불러오기 기능
        {
            if (File.Exists("save.json"))
            {
                string json = File.ReadAllText("save.json");
                GameData data = JsonConvert.DeserializeObject<GameData>(json);

                player = data.Player;
                inventory = data.Inventory;
                equippedItems = data.EquippedItems;
                purchasedItems = data.PurchasedItems ?? new List<string>();

                ShowText("\n\n\n\n저장된 데이터를 불러왔습니다.", 2000);
                Console.Clear();
                Mendacium();
                return true;
            }
            else
            {
                ShowText("\n\n\n\n저장된 데이터가 없습니다.", 1000);
                Console.Clear();
                return false;
            }
        }
        static void PlayBGM(string path) // 브금 기능
        {
            try
            {
                SoundPlayer player = new SoundPlayer(path);
                player.PlayLooping(); // 무한 반복
            }
            catch (Exception ex)
            {
                ShowText("\n\n\n\n\n\n\n\n\n\n\n\n\n\nBGM 오류 : " + ex.Message);
            }
        }
    }
    class GameData
    {
        public Player Player { get; set; }
        public List<string> Inventory { get; set; }
        public List<string> EquippedItems {  get; set; }
        public List<string> PurchasedItems { get; set; }
    }
}
