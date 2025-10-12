using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SlotMachineReel
{
    public class ReelSystem
    {
        // 9개의 그림을 나타내는 열거형
        public enum Picture
        {
            Cherry = 1,
            Lemon = 2,
            Orange = 3,
            Plum = 4,
            Bell = 5,
            Bar = 6,
            Seven = 7,
            Diamond = 8,
            Star = 9
        }

        private readonly Random _random;
        private volatile bool _isSpinning;
        private Picture _currentPicture;
        private CancellationTokenSource _cancellationTokenSource;

        public ReelSystem()
        {
            _random = new Random();
            _isSpinning = false;
            _currentPicture = Picture.Cherry;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // 릴 시작 (돌리기)
        public async Task StartSpinAsync()
        {
            if (_isSpinning)
            {
                Console.WriteLine("릴이 이미 돌아가고 있습니다.");
                return;
            }

            _isSpinning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("릴이 돌아가기 시작했습니다!");

            // 비동기적으로 애니메이션 실행
            _ = Task.Run(async () => await SpinAnimationAsync(_cancellationTokenSource.Token));
        }

        // 릴 정지 (멈춤 버튼)
        public Picture StopSpin()
        {
            if (!_isSpinning)
            {
                Console.WriteLine("릴이 돌아가고 있지 않습니다. 먼저 릴을 돌려주세요.");
                return _currentPicture;
            }

            _isSpinning = false;
            _cancellationTokenSource?.Cancel();

            // 1부터 9까지의 랜덤한 숫자 생성
            int randomNumber = _random.Next(1, 10);
            _currentPicture = (Picture)randomNumber;

            Console.WriteLine($"릴이 멈췄습니다! 결과: {GetPictureName(_currentPicture)}");
            return _currentPicture;
        }

        // 현재 그림 가져오기
        public Picture GetCurrentPicture() => _currentPicture;

        // 릴이 돌아가고 있는지 확인
        public bool IsSpinning() => _isSpinning;

        // 그림 이름을 문자열로 변환
        private static string GetPictureName(Picture picture)
        {
            return picture switch
            {
                Picture.Cherry => "🍒 체리",
                Picture.Lemon => "🍋 레몬",
                Picture.Orange => "🍊 오렌지",
                Picture.Plum => "🟣 자두",
                Picture.Bell => "🔔 벨",
                Picture.Bar => "📶 바",
                Picture.Seven => "7️⃣ 세븐",
                Picture.Diamond => "💎 다이아몬드",
                Picture.Star => "⭐ 별",
                _ => "알 수 없음"
            };
        }

        // 릴이 돌아가는 애니메이션 효과 (비동기)
        private async Task SpinAnimationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isSpinning)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소, 무시
            }
        }

        // 리소스 정리
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }

    // UI 관련 정적 클래스
    public static class ReelUI
    {
        // 그림 목록을 보여주는 UI
        public static void DisplayPictureList()
        {
            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║            그림 목록 (9가지)           ║");
            Console.WriteLine("╠═══════════════════════════════════════╣");
            Console.WriteLine("║ 1. 🍒 체리 (Cherry)                  ║");
            Console.WriteLine("║ 2. 🍋 레몬 (Lemon)                   ║");
            Console.WriteLine("║ 3. 🍊 오렌지 (Orange)                ║");
            Console.WriteLine("║ 4. 🟣 자두 (Plum)                    ║");
            Console.WriteLine("║ 5. 🔔 벨 (Bell)                      ║");
            Console.WriteLine("║ 6. 📶 바 (Bar)                       ║");
            Console.WriteLine("║ 7. 7️⃣ 세븐 (Seven)                   ║");
            Console.WriteLine("║ 8. 💎 다이아몬드 (Diamond)            ║");
            Console.WriteLine("║ 9. ⭐ 별 (Star)                      ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
        }

        // 현재 릴 상태를 보여주는 UI
        public static void DisplayReelStatus(ReelSystem reel)
        {
            if (reel == null)
            {
                Console.WriteLine("릴 시스템이 초기화되지 않았습니다.");
                return;
            }

            string status = reel.IsSpinning() ? "🎰 돌아가는 중..." : "⏹️ 정지됨";
            string currentPicture = reel.IsSpinning() ? "???" : GetPictureDisplay(reel.GetCurrentPicture());

            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║              릴 상태                  ║");
            Console.WriteLine("╠═══════════════════════════════════════╣");
            Console.WriteLine($"║ 상태: {status,-25} ║");
            Console.WriteLine($"║ 현재 그림: {currentPicture,-21} ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
        }

        // 그림 표시용 헬퍼 메서드
        private static string GetPictureDisplay(ReelSystem.Picture picture)
        {
            return picture switch
            {
                ReelSystem.Picture.Cherry => "🍒 체리",
                ReelSystem.Picture.Lemon => "🍋 레몬",
                ReelSystem.Picture.Orange => "🍊 오렌지",
                ReelSystem.Picture.Plum => "🟣 자두",
                ReelSystem.Picture.Bell => "🔔 벨",
                ReelSystem.Picture.Bar => "📶 바",
                ReelSystem.Picture.Seven => "7️⃣ 세븐",
                ReelSystem.Picture.Diamond => "💎 다이아몬드",
                ReelSystem.Picture.Star => "⭐ 별",
                _ => "알 수 없음"
            };
        }

        // 전체 UI를 그리는 메서드
        public static void DrawCompleteUI(ReelSystem reel)
        {
            Console.Clear();

            Console.WriteLine("🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰");
            Console.WriteLine("          슬롯머신 릴 시스템");
            Console.WriteLine("🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰🎰");
            Console.WriteLine();

            // 그림 목록 (항상 표시됨)
            DisplayPictureList();
            Console.WriteLine();

            // 릴 상태
            DisplayReelStatus(reel);
            Console.WriteLine();

            // 명령어 안내
            DisplayCommands();
            Console.WriteLine();
        }

        // 명령어 안내
        private static void DisplayCommands()
        {
            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║              명령어                   ║");
            Console.WriteLine("╠═══════════════════════════════════════╣");
            Console.WriteLine("║ 'spin' - 릴 돌리기                   ║");
            Console.WriteLine("║ 'stop' - 릴 멈추기                   ║");
            Console.WriteLine("║ 'refresh' - 화면 새로고침             ║");
            Console.WriteLine("║ 'quit' - 종료                        ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
        }

        // 결과 표시
        public static void DisplayResult(ReelSystem.Picture result)
        {
            Console.WriteLine($"🎉 결과: {GetPictureDisplay(result)}");
            Console.WriteLine("Enter를 눌러서 계속...");
        }
    }

    // 프로그램 진입점
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ReelSystem reel = new ReelSystem();

            try
            {
                // 초기 UI 표시
                ReelUI.DrawCompleteUI(reel);

                while (true)
                {
                    Console.Write("명령어를 입력하세요: ");
                    string? input = Console.ReadLine()?.ToLower().Trim();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    switch (input)
                    {
                        case "spin":
                            await reel.StartSpinAsync();
                            ReelUI.DrawCompleteUI(reel);
                            break;

                        case "stop":
                            var result = reel.StopSpin();
                            ReelUI.DrawCompleteUI(reel);
                            ReelUI.DisplayResult(result);
                            Console.ReadLine();
                            ReelUI.DrawCompleteUI(reel);
                            break;

                        case "refresh":
                            ReelUI.DrawCompleteUI(reel);
                            break;

                        case "quit":
                            Console.Clear();
                            Console.WriteLine("🎰 슬롯머신을 종료합니다. 안녕히 가세요! 🎰");
                            return;

                        default:
                            Console.WriteLine("❌ 올바른 명령어를 입력해주세요.");
                            await Task.Delay(1500);
                            ReelUI.DrawCompleteUI(reel);
                            break;
                    }
                }
            }
            finally
            {
                reel.Dispose();
            }
        }
    }

    // 확률 가중치 시스템 (선택사항)
    public class WeightedReelSystem : ReelSystem
    {
        private readonly Dictionary<Picture, int> _weights;
        private readonly int _totalWeight;
        private readonly Random _weightedRandom;

        public WeightedReelSystem()
        {
            // 각 그림의 가중치 설정 (높을수록 나올 확률이 높음)
            _weights = new Dictionary<Picture, int>
            {
                { Picture.Cherry, 20 },      // 가장 흔함
                { Picture.Lemon, 20 },
                { Picture.Orange, 15 },
                { Picture.Plum, 15 },
                { Picture.Bell, 10 },
                { Picture.Bar, 8 },
                { Picture.Seven, 5 },        // 드문
                { Picture.Diamond, 4 },      // 매우 드문
                { Picture.Star, 3 }          // 가장 드문
            };

            _totalWeight = 0;
            foreach (var weight in _weights.Values)
            {
                _totalWeight += weight;
            }

            _weightedRandom = new Random();
        }

        public new Picture StopSpin()
        {
            if (!IsSpinning())
            {
                Console.WriteLine("릴이 돌아가고 있지 않습니다. 먼저 릴을 돌려주세요.");
                return GetCurrentPicture();
            }

            int randomValue = _weightedRandom.Next(0, _totalWeight);
            int currentWeight = 0;

            foreach (var kvp in _weights)
            {
                currentWeight += kvp.Value;
                if (randomValue < currentWeight)
                {
                    Console.WriteLine($"가중치 기반 결과: {kvp.Key}");
                    return kvp.Key;
                }
            }

            // 폴백으로 기본 동작 수행
            return base.StopSpin();
        }
    }
}