using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SilverStarSeaGame
{
    class Program
    {
        // 게임 상태 관리
        private static int health = 0;
        private static string userName = string.Empty;
        private static int freeSpins = 10;
        private static readonly Random rnd = new Random();
        private static readonly List<MemoryRecord> memories = new List<MemoryRecord>();

        // 심볼 정의 - readonly로 변경하여 불변성 보장
        private static readonly string[] symbols = { "가리비", "돌문어", "광어", "고동", "은별", "우럭", "소라", "낙지", "붕장어" };
        private static readonly string[] storyCards = { "은별", "추억", "바람", "파도", "모래", "조개", "등대", "갈매기",
                                       "노을", "달빛", "별빛", "약속", "시간", "마음", "여행", "꿈" };

        // 페이라인 정의 (이미지에 표시된 패턴)
        private static readonly int[][,] paylines = new int[][,]
        {
            new int[,] {{0,0},{0,1},{0,2}}, // 라인 1
            new int[,] {{1,0},{1,1},{1,2}}, // 라인 2
            new int[,] {{2,0},{2,1},{2,2}}, // 라인 3
            new int[,] {{0,0},{1,1},{2,2}}, // 라인 4 (대각선)
            new int[,] {{2,0},{1,1},{0,2}}  // 라인 5 (역대각선)
        };

        // 메모리 기록 클래스
        private class MemoryRecord
        {
            public DateTime Date { get; set; }
            public string Text { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public List<string> SelectedCards { get; set; } = new List<string>();
        }

        static int Main(string[] args)
        {
            // UTF-8 인코딩 설정으로 한글 깨짐 방지
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                ShowIntro();
                GetBasicInfo();
                ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류가 발생했습니다: {ex.Message}");
                Console.WriteLine("프로그램을 종료합니다.");
                Thread.Sleep(2000);
            }

            // 안전한 종료 (return 0)
            return 0;
        }

        private static void ShowIntro()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("┌──────────────────────────────────────────────────────┐");
            Console.WriteLine("│                    은별의 바다                       │");
            Console.WriteLine("└──────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine("\n깊은 바다 속 은빛 별들이 춤추는 곳,");
            Console.WriteLine("당신의 행운이 파도처럼 밀려옵니다.");
            Console.WriteLine("해질녘 바닷가의 영원한 약속을 찾아서...\n");
            Thread.Sleep(2000);
        }

        private static void GetBasicInfo()
        {
            // 체력 입력
            bool validInput = false;
            while (!validInput)
            {
                Console.Write("체력을 입력하세요 (1-100): ");
                string? input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int tempHealth))
                {
                    if (tempHealth >= 1 && tempHealth <= 100)
                    {
                        health = tempHealth;
                        validInput = true;
                    }
                    else
                    {
                        Console.WriteLine("1에서 100 사이의 숫자를 입력하세요.");
                    }
                }
                else
                {
                    Console.WriteLine("올바른 숫자를 입력하세요.");
                }
            }

            // 이름 입력
            Console.Write("사용자 이름을 입력하세요: ");
            string? nameInput = Console.ReadLine();
            userName = string.IsNullOrWhiteSpace(nameInput) ? "플레이어" : nameInput;

            Console.WriteLine($"\n환영합니다, {userName}님! 체력: {health}");
            Thread.Sleep(1500);
        }

        private static void ShowMainMenu()
        {
            bool exitGame = false;
            while (!exitGame)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("═══════════ 게임 선택 화면 ═══════════");
                Console.ResetColor();
                Console.WriteLine($"현재 체력: {health} | 사용자: {userName}");
                Console.WriteLine("\n1. 메인 게임 - 은별의 바다");
                Console.WriteLine("2. 미니 게임 목록");
                Console.WriteLine("3. 게임 방법");
                Console.WriteLine("4. 게임 종료");
                Console.Write("\n선택: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        PlayMainSlotGame();
                        break;
                    case "2":
                        ShowMiniGameMenu();
                        break;
                    case "3":
                        ShowGameRules();
                        break;
                    case "4":
                        Console.WriteLine("\n게임을 종료합니다. 안녕히 가세요!");
                        exitGame = true;
                        break;
                    default:
                        Console.WriteLine("잘못된 선택입니다.");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private static void PlayMainSlotGame()
        {
            bool exitSlot = false;
            while (!exitSlot)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("┌────────────────────────────────────┐");
                Console.WriteLine("│      은별의 바다 - PHASE 1         │");
                Console.WriteLine("└────────────────────────────────────┘");
                Console.ResetColor();

                if (freeSpins > 0)
                {
                    Console.WriteLine($"무료 스핀 남은 횟수: {freeSpins}");
                }
                else if (health <= 0)
                {
                    Console.WriteLine("\n체력이 부족합니다!");
                    Console.WriteLine("1. 체력 재충전");
                    Console.WriteLine("2. 게임 선택화면으로");
                    string? choice = Console.ReadLine();
                    if (choice == "1")
                    {
                        GetBasicInfo();
                        freeSpins = 10;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"체력: {health}");
                }

                Console.WriteLine("\n1. 스핀 시작 (해루질)");
                Console.WriteLine("2. 스핀 멈춤 (조금 쉬자)");
                Console.WriteLine("3. 게임 선택화면으로");
                Console.Write("\n선택: ");

                string? input = Console.ReadLine();

                if (input == "1")
                {
                    if (freeSpins > 0)
                    {
                        freeSpins--;
                    }
                    else if (health > 0)
                    {
                        health--;
                    }
                    else
                    {
                        Console.WriteLine("체력이 부족합니다!");
                        Thread.Sleep(1500);
                        continue;
                    }

                    string[,] grid = SpinSlot();
                    DisplaySlot(grid);
                    CheckWinConditions(grid);

                    Console.WriteLine("\n계속하려면 Enter를 누르세요...");
                    Console.ReadLine();
                }
                else if (input == "2")
                {
                    Console.WriteLine("\n잠시 쉬어갑니다...");
                    Thread.Sleep(2000);
                }
                else if (input == "3")
                {
                    exitSlot = true;
                }
            }
        }

        private static string[,] SpinSlot()
        {
            string[,] grid = new string[9, 9];

            Console.WriteLine("\n릴이 돌아갑니다...");
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                Thread.Sleep(500);
            }
            Console.WriteLine();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    grid[i, j] = symbols[rnd.Next(symbols.Length)];
                }
            }

            return grid;
        }

        private static void DisplaySlot(string[,] grid)
        {
            Console.WriteLine("\n┌──────────────────────────────────────────────────────────────────────┐");
            for (int i = 0; i < 9; i++)
            {
                Console.Write("│ ");
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i, j] == "은별")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{grid[i, j],-6} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write($"{grid[i, j],-6} ");
                    }
                }
                Console.WriteLine("│");
            }
            Console.WriteLine("└──────────────────────────────────────────────────────────────────────┘");
        }

        private static void CheckWinConditions(string[,] grid)
        {
            List<string> winLines = new List<string>();

            // 페이라인 체크
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col <= 6; col++)
                {
                    if (grid[row, col] == grid[row, col + 1] &&
                        grid[row, col + 1] == grid[row, col + 2])
                    {
                        winLines.Add($"라인 {row + 1}: {grid[row, col]} x3");

                        // 은별 3개 체크 (하이퍼 잭팟)
                        if (grid[row, col] == "은별")
                        {
                            ShowHyperJackpot();
                            return;
                        }
                    }
                }
            }

            // 잭팟 체크
            CheckJackpots(grid, winLines);

            if (winLines.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n★ 승리! ★");
                foreach (string line in winLines)
                {
                    Console.WriteLine(line);
                }
                Console.ResetColor();
            }
        }

        private static void CheckJackpots(string[,] grid, List<string> winLines)
        {
            Dictionary<string, int> symbolCount = new Dictionary<string, int>();
            foreach (string symbol in symbols)
            {
                symbolCount[symbol] = 0;
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    symbolCount[grid[i, j]]++;
                }
            }

            // 매운탕 체크
            if (symbolCount["낙지"] >= 3 && symbolCount["우럭"] >= 3 && symbolCount["붕장어"] >= 3)
            {
                winLines.Add("★ JACKPOT! 매운탕 완성! ★");
            }
            // 해산물찜 체크
            if (symbolCount["우럭"] >= 3 && symbolCount["가리비"] >= 3 && symbolCount["은별"] >= 3)
            {
                winLines.Add("★ JACKPOT! 해산물찜 완성! ★");
            }
            // 모둠횟감 체크
            if (symbolCount["광어"] >= 3 && symbolCount["우럭"] >= 3 && symbolCount["돌문어"] >= 3)
            {
                winLines.Add("★ JACKPOT! 모둠횟감 완성! ★");
            }
        }

        private static void ShowHyperJackpot()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("┌──────────────────────────────────────────┐");
            Console.WriteLine("│     ★ PHASE 2: 은별의 마음 ★          │");
            Console.WriteLine("└──────────────────────────────────────────┘");
            Console.ResetColor();

            Console.WriteLine("\n은별 3개가 연결되었습니다!");
            Console.WriteLine("특별한 보너스 스테이지에 진입합니다!");
            Thread.Sleep(2000);

            // Phase 2 미니 슬롯
            Console.WriteLine("\n2행만 작동하는 특별 릴...");
            string[] miniGrid = new string[3];
            for (int i = 0; i < 3; i++)
            {
                miniGrid[i] = symbols[rnd.Next(symbols.Length)];
            }

            Console.WriteLine($"\n[ {miniGrid[0]} ] [ {miniGrid[1]} ] [ {miniGrid[2]} ]");

            if (miniGrid[0] == "은별" && miniGrid[1] == "은별" && miniGrid[2] == "은별")
            {
                ShowEnding();
            }
            else
            {
                Console.WriteLine("\n아쉽게도 은별을 모으지 못했습니다.");
            }

            Thread.Sleep(3000);
        }

        private static void ShowEnding()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══════════════════════════════════════════");
            Console.WriteLine("          해질녘 어딘가 공허한 바닷가");
            Console.WriteLine("═══════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine("\n파도 소리만이 고요히 들려오는 이곳에서");
            Console.WriteLine("당신은 은빛 별을 손에 쥐었습니다.");
            Console.WriteLine("\n그것은 누군가와의 영원한 약속처럼");
            Console.WriteLine("당신의 마음속에 빛나고 있습니다.");
            Console.WriteLine("\n바람이 불어옵니다.");
            Console.WriteLine("추억이 파도처럼 밀려옵니다.");
            Console.WriteLine("\n이것이 바로 은별의 바다가 전하는");
            Console.WriteLine("영원한 약속입니다.");

            Console.WriteLine("\n\n계속하려면 Enter를 누르세요...");
            Console.ReadLine();
        }

        private static void ShowMiniGameMenu()
        {
            bool exitMini = false;
            while (!exitMini)
            {
                Console.Clear();
                Console.WriteLine("═══════════ 미니 게임 목록 ═══════════");
                Console.WriteLine("\n1. 해루질 뽑기 게임");
                Console.WriteLine("2. 카드 스토리텔링");
                Console.WriteLine("3. 우리의 별빛 기억들");
                Console.WriteLine("4. 게임 선택화면으로");
                Console.Write("\n선택: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        PlayHaeruGame();
                        break;
                    case "2":
                        PlayCardStoryGame();
                        break;
                    case "3":
                        ShowMemories();
                        break;
                    case "4":
                        exitMini = true;
                        break;
                    default:
                        Console.WriteLine("잘못된 선택입니다.");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private static void PlayHaeruGame()
        {
            bool exitHaeru = false;
            while (!exitHaeru)
            {
                Console.Clear();
                Console.WriteLine("┌────────────────────────────┐");
                Console.WriteLine("│     해루질 뽑기 게임       │");
                Console.WriteLine("└────────────────────────────┘");
                Console.WriteLine("\n9개의 심볼 중 하나를 뽑습니다!");
                Console.WriteLine("은별을 뽑으면 당첨!");
                Console.WriteLine("\n1. 뽑기");
                Console.WriteLine("2. 나가기");
                Console.Write("\n선택: ");

                string? choice = Console.ReadLine();
                if (choice == "1")
                {
                    string result = symbols[rnd.Next(symbols.Length)];
                    Console.WriteLine($"\n뽑기 결과: [{result}]");

                    if (result == "은별")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("★ 축하합니다! 은별 당첨! ★");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("아쉽네요. 다시 도전하세요!");
                    }

                    Console.WriteLine("\n계속하려면 Enter를 누르세요...");
                    Console.ReadLine();
                }
                else if (choice == "2")
                {
                    exitHaeru = true;
                }
            }
        }

        private static void PlayCardStoryGame()
        {
            Console.Clear();
            Console.WriteLine("┌────────────────────────────────────┐");
            Console.WriteLine("│      카드 스토리텔링 게임          │");
            Console.WriteLine("└────────────────────────────────────┘");
            Console.WriteLine("\n16장의 카드 중 3장을 선택하세요:");

            // 카드 표시
            for (int i = 0; i < storyCards.Length; i++)
            {
                if (i % 4 == 0) Console.WriteLine();
                Console.Write($"{i + 1,2}. {storyCards[i],-6}  ");
            }

            List<string> selectedCards = new List<string>();
            Console.WriteLine("\n\n3장의 카드를 선택하세요 (번호 입력):");

            for (int i = 0; i < 3; i++)
            {
                bool validCard = false;
                while (!validCard)
                {
                    Console.Write($"카드 {i + 1}: ");
                    string? input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int cardNum))
                    {
                        if (cardNum >= 1 && cardNum <= 16)
                        {
                            selectedCards.Add(storyCards[cardNum - 1]);
                            validCard = true;
                        }
                        else
                        {
                            Console.WriteLine("1-16 사이의 번호를 입력하세요.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("올바른 번호를 입력하세요.");
                    }
                }
            }

            Console.WriteLine($"\n선택한 카드: {string.Join(", ", selectedCards)}");
            Console.WriteLine("\n이 카드들을 보고 떠오르는 생각을 적어주세요:");
            string? thought = Console.ReadLine();

            // 메모리에 저장
            memories.Add(new MemoryRecord
            {
                Date = DateTime.Now,
                Text = thought ?? string.Empty,
                UserName = userName,
                SelectedCards = selectedCards
            });

            Console.WriteLine("\n당신의 이야기가 '우리의 별빛 기억들'에 저장되었습니다.");
            Console.WriteLine("계속하려면 Enter를 누르세요...");
            Console.ReadLine();
        }

        private static void ShowMemories()
        {
            Console.Clear();
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│              우리의 별빛 기억들                          │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘");

            if (memories.Count == 0)
            {
                Console.WriteLine("\n아직 기록된 기억이 없습니다.");
            }
            else
            {
                Console.WriteLine("\n┌─────────────┬──────────────────────────────┬────────────┐");
                Console.WriteLine("│    날짜     │         기록된 생각          │   이름     │");
                Console.WriteLine("├─────────────┼──────────────────────────────┼────────────┤");

                foreach (MemoryRecord memory in memories)
                {
                    string dateStr = memory.Date.ToString("MM/dd HH:mm");
                    string textStr = memory.Text.Length > 25 ?
                        memory.Text.Substring(0, 22) + "..." : memory.Text;
                    Console.WriteLine($"│ {dateStr,-11} │ {textStr,-28} │ {memory.UserName,-10} │");
                }
                Console.WriteLine("└─────────────┴──────────────────────────────┴────────────┘");
            }

            Console.WriteLine("\n계속하려면 Enter를 누르세요...");
            Console.ReadLine();
        }

        private static void ShowGameRules()
        {
            Console.Clear();
            Console.WriteLine("═══════════════ 게임 방법 ═══════════════\n");

            Console.WriteLine("【 메인 게임 - 은별의 바다 】");
            Console.WriteLine("• 9x9 슬롯에서 9가지 심볼이 무작위로 나타납니다");
            Console.WriteLine("• 체력 1당 1회 스핀 가능 (처음 10회 무료)");
            Console.WriteLine("• 가로줄에 같은 심볼 3개가 연속되면 승리");
            Console.WriteLine("• 은별 3개 연속 시 특별 보너스 진입");
            Console.WriteLine("• 잭팟: 특정 심볼 조합 달성 시 보너스");
            Console.WriteLine("  - 매운탕: 낙지, 우럭, 붕장어 각 3개 이상");
            Console.WriteLine("  - 해산물찜: 우럭, 가리비, 은별 각 3개 이상");
            Console.WriteLine("  - 모둠횟감: 광어, 우럭, 돌문어 각 3개 이상");

            Console.WriteLine("\n【 해루질 뽑기 게임 】");
            Console.WriteLine("• 9개 심볼 중 하나를 무작위로 뽑습니다");
            Console.WriteLine("• 은별을 뽑으면 당첨!");
            Console.WriteLine("• 횟수 제한 없이 플레이 가능");

            Console.WriteLine("\n【 카드 스토리텔링 】");
            Console.WriteLine("• 16장의 카드 중 3장을 선택");
            Console.WriteLine("• 선택한 카드로 이야기를 작성");
            Console.WriteLine("• 작성한 이야기는 '우리의 별빛 기억들'에 저장");

            Console.WriteLine("\n【 조작 방법 】");
            Console.WriteLine("• 숫자 키: 메뉴 선택");
            Console.WriteLine("• Enter: 확인/계속");
            Console.WriteLine("• 각 화면에 표시된 번호를 입력하여 진행");

            Console.WriteLine("\n계속하려면 Enter를 누르세요...");
            Console.ReadLine();
        }
    }
}