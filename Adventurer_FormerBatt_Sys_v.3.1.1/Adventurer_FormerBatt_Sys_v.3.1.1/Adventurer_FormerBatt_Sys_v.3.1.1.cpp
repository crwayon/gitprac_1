#include <iostream>
#include <string>
#include <thread>
#include <chrono>
#include <limits>
#include <algorithm>
using namespace std;

// ===== 입력 유틸 =====
bool readInt(int& out) {
    if (!(cin >> out)) {
        cin.clear();
        cin.ignore(numeric_limits<streamsize>::max(), '\n');
        return false;
    }
    return true;
}

int readIntInRange(const string& prompt, int lo, int hi) {
    int v;
    while (true) {
        cout << prompt;
        if (!readInt(v)) {
            cout << "숫자를 입력하세요.\n";
            continue;
        }
        if (v < lo || v > hi) {
            cout << lo << "~" << hi << " 범위만 입력하세요.\n";
            continue;
        }
        return v;
    }
}

// ===== 모험가 클래스 =====
class Adventurer {
protected:
    int status[4];      // [HP, MP, ATK, DEF]
    int maxHP;
    int maxMP;
    int hpPotion;
    int mpPotion;
    string className;
public:
    Adventurer() {
        for (int i = 0; i < 4; i++) status[i] = 0;
        maxHP = 0; maxMP = 0; hpPotion = 0; mpPotion = 0;
        className = "모험가";
    }
    virtual ~Adventurer() {}

    virtual void attack() {
        if (status[1] >= 50) {
            status[1] -= 50;
            cout << "\n[모험가 기본공격] MP 50 소모 → 남은 MP: " << status[1] << "/" << maxMP << "\n";
        }
        else {
            cout << "[MP 부족: 50 필요, 현재 " << status[1] << "]\n";
        }
    }
    virtual void jobSkill() {
        cout << "[아직 전직하지 않아 전직 스킬 사용 불가]\n";
    }

    void setPotion(int count) { hpPotion = count; mpPotion = count; }

    void initializeStats() {
        cout << "\n=== 캐릭터 생성 ===\n";
        status[0] = readIntInRange("HP (30~50): ", 30, 50); maxHP = status[0];
        status[1] = readIntInRange("MP (60~70): ", 60, 70); maxMP = status[1];
        status[2] = readIntInRange("ATK (1~50): ", 1, 50);
        status[3] = readIntInRange("DEF (1~50): ", 1, 50);
        setPotion(5);
    }

    void copyStats(Adventurer* src) {
        for (int i = 0; i < 4; i++) status[i] = src->status[i];
        maxHP = src->maxHP; maxMP = src->maxMP;
        hpPotion = src->hpPotion; mpPotion = src->mpPotion;
    }

    void showStatus() {
        cout << "\n--- 현재 스탯 ---\n";
        cout << "직업: " << className << "\n";
        cout << "HP: " << status[0] << "/" << maxHP << "\n";
        cout << "MP: " << status[1] << "/" << maxMP << "\n";
        cout << "ATK: " << status[2] << "  DEF: " << status[3] << "\n";
        cout << "HP포션: " << hpPotion << "  MP포션: " << mpPotion << "\n";
    }

    void statManagementSystem() {
        bool running = true;
        while (running) {
            cout << "\n[스탯 관리 시스템]\n";
            cout << "1. HP 회복\n";
            cout << "2. MP 회복\n";
            cout << "3. HP 강화\n";
            cout << "4. MP 강화\n";
            cout << "5. 공격 (전직 전: 기본 / 전직 후: 전직공격)\n";
            cout << "6. 전직 스킬 사용\n";
            cout << "7. 나가기\n";

            int ch = readIntInRange("선택: ", 1, 7);
            switch (ch) {
            case 1: {
                if (hpPotion > 0) {
                    int heal = min(20, maxHP - status[0]);
                    status[0] += heal; hpPotion--;
                    cout << "[HP +" << heal << ", 현재 HP: " << status[0] << "/" << maxHP << "]\n";
                    cout << "남은 HP포션: " << hpPotion << "\n";
                }
                else cout << "[HP포션 없음]\n";
                break;
            }
            case 2: {
                if (mpPotion > 0) {
                    int heal = min(20, maxMP - status[1]);
                    status[1] += heal; mpPotion--;
                    cout << "[MP +" << heal << ", 현재 MP: " << status[1] << "/" << maxMP << "]\n";
                    cout << "남은 MP포션: " << mpPotion << "\n";
                }
                else cout << "[MP포션 없음]\n";
                break;
            }
            case 3: {
                int beforeMax = maxHP;
                int beforeCur = status[0];
                status[0] *= 2; maxHP *= 2;
                cout << "[HP 강화] 최대HP " << beforeMax << " → " << maxHP << " (2배), 현재HP " << beforeCur << " → " << status[0] << "\n";
                break;
            }
            case 4: {
                int beforeMax = maxMP;
                int beforeCur = status[1];
                status[1] *= 2; maxMP *= 2;
                cout << "[MP 강화] 최대MP " << beforeMax << " → " << maxMP << " (2배), 현재MP " << beforeCur << " → " << status[1] << "\n";
                break;
            }
            case 5:
                attack(); break;
            case 6:
                jobSkill(); break;
            case 7:
                running = false; break;
            }
        }
    }
};

// ===== 전직 클래스 =====
class Warrior : public Adventurer {
public:
    Warrior() { className = "용사"; }
    void applyJobBonus() { status[0] += 10; maxHP += 10; status[2] += 5; }
    void attack() override {
        double dmg = status[2] * 1.5;
        cout << "[용사 - 슬래시] 피해: " << dmg << ", 현재 MP: " << status[1] << "/" << maxMP << "\n";
    }
    void jobSkill() override {
        if (status[1] >= 30) {
            status[1] -= 30;
            cout << "[용사 스킬 - 대검술] MP 30 소모 → 남은 MP: " << status[1] << "/" << maxMP << "\n";
        }
        else cout << "[MP 부족: 30 필요, 현재 " << status[1] << "]\n";
    }
};

class Dwarf : public Adventurer {
public:
    Dwarf() { className = "드워프"; }
    void applyJobBonus() { status[0] += 5; maxHP += 5; status[3] += 10; }
    void attack() override {
        double dmg = status[2] * 1.3;
        cout << "[드워프 - 크러시] 피해: " << dmg << ", 현재 MP: " << status[1] << "/" << maxMP << "\n";
    }
    void jobSkill() override {
        if (status[1] >= 25) {
            status[1] -= 25;
            cout << "[드워프 스킬 - 대지의 일격] MP 25 소모 → 남은 MP: " << status[1] << "/" << maxMP << "\n";
        }
        else cout << "[MP 부족: 25 필요, 현재 " << status[1] << "]\n";
    }
};

class Sage : public Adventurer {
public:
    Sage() { className = "현자"; }
    void applyJobBonus() { status[1] += 15; maxMP += 15; status[2] += 3; }
    void attack() override {
        if (status[1] >= 10) {
            status[1] -= 10;
            double dmg = status[2] * 2.0;
            cout << "[현자 - 파이어볼] 피해: " << dmg << ", MP 10 소모 → 남은 MP: " << status[1] << "/" << maxMP << "\n";
        }
        else cout << "[MP 부족: 10 필요, 현재 " << status[1] << "]\n";
    }
    void jobSkill() override {
        if (status[1] >= 40) {
            status[1] -= 40;
            cout << "[현자 스킬 - 메테오] MP 40 소모 → 남은 MP: " << status[1] << "/" << maxMP << "\n";
        }
        else cout << "[MP 부족: 40 필요, 현재 " << status[1] << "]\n";
    }
};

// ===== 메인 =====
int main() {
    Adventurer* player = new Adventurer();
    player->initializeStats();

    // 전직
    while (true) {
        cout << "\n[전직 시스템]\n";
        cout << "1. 용사\n2. 드워프\n3. 현자\n";
        int sel = readIntInRange("선택: ", 1, 3);
        Adventurer* newJob = nullptr;
        if (sel == 1) newJob = new Warrior();
        else if (sel == 2) newJob = new Dwarf();
        else if (sel == 3) newJob = new Sage();
        newJob->copyStats(player);
        if (auto* w = dynamic_cast<Warrior*>(newJob)) w->applyJobBonus();
        if (auto* d = dynamic_cast<Dwarf*>(newJob)) d->applyJobBonus();
        if (auto* s = dynamic_cast<Sage*>(newJob)) s->applyJobBonus();
        delete player;
        player = newJob;
        break;
    }

    player->showStatus();
    player->statManagementSystem();
    delete player;
    cout << "\n게임 종료\n";
    return 0;
}