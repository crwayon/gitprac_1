
"""
Hummingbird UI System - 벌새 인터랙션 시스템
- 페어링 시스템: 마우스 호버 시 정보 표시 (우선)
- 호버링 시스템: Tab 키로 버튼 생성 (페어링 이후)
"""

import tkinter as tk
from tkinter import ttk
from datetime import datetime
from typing import List, Dict, Tuple, Optional, Callable
import threading
from collections import deque
import webbrowser


class ActionHistory:
    """작업 히스토리 관리 클래스"""
    
    def __init__(self, max_size: int = 50):
        self.history: deque = deque(maxlen=max_size)
        self.current_context: Dict = {}
        self.hyperlinks: Dict[str, str] = {}  # 작업별 하이퍼링크 저장
    
    def add_action(self, action_type: str, data: Dict, link: str = None):
        """작업 기록 추가"""
        entry = {
            'type': action_type,
            'data': data,
            'timestamp': datetime.now(),
            'context': self.current_context.copy(),
            'link': link
        }
        self.history.append(entry)
        
        # 링크가 있으면 별도 저장
        if link:
            key = f"{action_type}_{len(self.history)}"
            self.hyperlinks[key] = link
    
    def get_recent_actions(self, count: int = 5) -> List[Dict]:
        """최근 작업 조회"""
        return list(self.history)[-count:]
    
    def set_context(self, key: str, value: any):
        """현재 컨텍스트 설정"""
        self.current_context[key] = value


class PairingSystem:
    """
    페어링 시스템: 마우스 호버 시 정보 표시
    - 1.6초 체류 시 정보 박스 표시
    - 'i' 키: 사전적 정의 표시
    """
    
    def __init__(self, parent: tk.Widget, delay_ms: int = 1600):
        self.parent = parent
        self.delay_ms = delay_ms
        self.pairing_timer: Optional[threading.Timer] = None
        self.info_box: Optional[tk.Toplevel] = None
        self.last_position: Tuple[int, int] = (-1, -1)
        self.is_pairing = False
        self.hover_threshold = 5
        self.current_object = None
        self.dictionary_mode = False
        
        # 객체별 정보 저장소
        self.object_info: Dict[str, Dict] = {
            'canvas_object': {
                'title': '캔버스 객체',
                'brief': '사용자가 생성한 도형',
                'details': '클릭하여 생성된 원형 객체입니다.',
                'definition': '캔버스(Canvas): 그래픽 요소를 그릴 수 있는 영역'
            },
            'button': {
                'title': '액션 버튼',
                'brief': '기능 실행 버튼',
                'details': '특정 작업을 수행하는 인터랙티브 요소',
                'definition': '버튼(Button): 사용자 입력을 받아 동작을 실행하는 UI 컴포넌트'
            },
            'text': {
                'title': '텍스트 요소',
                'brief': '정보 표시 영역',
                'details': '사용자에게 정보를 전달하는 텍스트',
                'definition': '텍스트(Text): 문자 정보를 시각적으로 표현한 것'
            }
        }
    
    def start_pairing_detection(self, x: int, y: int, obj_type: str = 'canvas_object'):
        """페어링 감지 시작"""
        # 이전 타이머 취소
        if self.pairing_timer:
            self.pairing_timer.cancel()
        
        # 위치가 크게 변경되면 정보 박스 제거
        if self.is_pairing:
            dx = abs(x - self.last_position[0])
            dy = abs(y - self.last_position[1])
            if dx > self.hover_threshold or dy > self.hover_threshold:
                self.hide_pairing_info()
        
        self.last_position = (x, y)
        self.current_object = obj_type
        self.dictionary_mode = False
        
        # 새 타이머 시작 (1.6초)
        self.pairing_timer = threading.Timer(
            self.delay_ms / 1000,
            lambda: self.parent.after(0, self._show_pairing_info, x, y, obj_type)
        )
        self.pairing_timer.start()
    
    def show_dictionary_definition(self, x: int, y: int):
        """'i' 키 입력: 사전적 정의 표시"""
        if not self.current_object:
            return
        
        self.dictionary_mode = True
        self.hide_pairing_info()
        self._show_pairing_info(x, y, self.current_object, show_definition=True)
    
    def _show_pairing_info(self, x: int, y: int, obj_type: str, show_definition: bool = False):
        """페어링 정보 박스 표시"""
        if self.is_pairing and not show_definition:
            return
        
        self.is_pairing = True
        self.hide_pairing_info()
        
        info = self.object_info.get(obj_type, self.object_info['canvas_object'])
        
        # 정보 윈도우 생성
        self.info_box = tk.Toplevel(self.parent)
        self.info_box.overrideredirect(True)
        self.info_box.attributes('-topmost', True)
        self.info_box.attributes('-alpha', 0.95)
        
        # 위치 설정 (객체와 겹치도록)
        self.info_box.geometry(f"+{x-10}+{y-10}")
        
        # 메인 프레임
        frame = tk.Frame(
            self.info_box,
            bg='#FFFFFF',
            relief=tk.SOLID,
            borderwidth=2,
            highlightbackground='#00ACC1',
            highlightthickness=2
        )
        frame.pack(fill=tk.BOTH, expand=True)
        
        if show_definition:
            # 사전적 정의 모드
            self._create_definition_view(frame, info)
        else:
            # 기본 정보 모드
            self._create_brief_view(frame, info)
        
        # 힌트 텍스트
        hint_frame = tk.Frame(frame, bg='#F5F5F5')
        hint_frame.pack(fill=tk.X, side=tk.BOTTOM)
        
        hint = tk.Label(
            hint_frame,
            text="💡 'i' 키: 사전적 정의 | Tab 키: 작업 히스토리",
            bg='#F5F5F5',
            fg='#757575',
            font=('Arial', 7, 'italic'),
            pady=4
        )
        hint.pack()
    
    def _create_brief_view(self, parent: tk.Frame, info: Dict):
        """간략 정보 뷰"""
        # 제목
        title = tk.Label(
            parent,
            text=f"📌 {info['title']}",
            bg='#FFFFFF',
            fg='#00ACC1',
            font=('Arial', 11, 'bold'),
            anchor='w',
            padx=12,
            pady=8
        )
        title.pack(fill=tk.X)
        
        # 간략 설명
        brief = tk.Label(
            parent,
            text=info['brief'],
            bg='#FFFFFF',
            fg='#424242',
            font=('Arial', 9),
            anchor='w',
            padx=12,
            pady=4
        )
        brief.pack(fill=tk.X)
        
        # 상세 설명
        details = tk.Label(
            parent,
            text=info['details'],
            bg='#FFFFFF',
            fg='#616161',
            font=('Arial', 8),
            anchor='w',
            wraplength=250,
            justify=tk.LEFT,
            padx=12,
            pady=8
        )
        details.pack(fill=tk.X)
    
    def _create_definition_view(self, parent: tk.Frame, info: Dict):
        """사전적 정의 뷰"""
        # 제목
        title = tk.Label(
            parent,
            text="📖 사전적 정의",
            bg='#FFFFFF',
            fg='#1976D2',
            font=('Arial', 11, 'bold'),
            anchor='w',
            padx=12,
            pady=8
        )
        title.pack(fill=tk.X)
        
        # 구분선
        separator = tk.Frame(parent, height=2, bg='#E0E0E0')
        separator.pack(fill=tk.X, padx=12, pady=4)
        
        # 정의
        definition = tk.Label(
            parent,
            text=info['definition'],
            bg='#FFFFFF',
            fg='#212121',
            font=('Arial', 9),
            anchor='w',
            wraplength=280,
            justify=tk.LEFT,
            padx=12,
            pady=12
        )
        definition.pack(fill=tk.X)
    
    def hide_pairing_info(self):
        """페어링 정보 박스 숨기기"""
        if self.info_box:
            self.info_box.destroy()
            self.info_box = None
        self.is_pairing = False
    
    def stop_pairing_detection(self):
        """페어링 감지 중지"""
        if self.pairing_timer:
            self.pairing_timer.cancel()
            self.pairing_timer = None


class HoveringSystem:
    """
    호버링 시스템: Tab 키로 작업 히스토리 버튼 표시
    - 페어링 상태에서 Tab 키 입력 시 활성화
    - 이전 작업 정보 index 버튼 제공
    - 하이퍼링크 버튼 제공
    """
    
    def __init__(self, parent: tk.Widget, history: ActionHistory):
        self.parent = parent
        self.history = history
        self.hover_buttons: List[tk.Button] = []
        self.is_hovering = False
        self.button_window: Optional[tk.Toplevel] = None
    
    def show_hovering_buttons(self, x: int, y: int):
        """호버링 버튼 표시 (마우스 오른쪽)"""
        if self.is_hovering:
            self.hide_hovering_buttons()
            return
        
        self.is_hovering = True
        
        # 최근 작업 가져오기
        recent_actions = self.history.get_recent_actions(5)
        
        if not recent_actions:
            self._show_empty_message(x, y)
            return
        
        # 버튼 윈도우 생성
        self.button_window = tk.Toplevel(self.parent)
        self.button_window.overrideredirect(True)
        self.button_window.attributes('-topmost', True)
        self.button_window.attributes('-alpha', 0.95)
        
        # 마우스 오른쪽에 위치
        self.button_window.geometry(f"+{x+20}+{y-50}")
        
        # 메인 프레임
        main_frame = tk.Frame(
            self.button_window,
            bg='#263238',
            relief=tk.RAISED,
            borderwidth=2
        )
        main_frame.pack(fill=tk.BOTH, expand=True, padx=2, pady=2)
        
        # 제목
        title = tk.Label(
            main_frame,
            text="🗂️ 작업 히스토리 인덱스",
            bg='#263238',
            fg='#00BCD4',
            font=('Arial', 10, 'bold')
        )
        title.pack(pady=(8, 4), padx=8)
        
        # 구분선
        separator = tk.Frame(main_frame, height=1, bg='#455A64')
        separator.pack(fill=tk.X, padx=8, pady=4)
        
        # 작업 버튼들
        for i, action in enumerate(reversed(recent_actions)):
            self._create_action_button(main_frame, action, i)
        
        # 닫기 안내
        close_hint = tk.Label(
            main_frame,
            text="클릭하여 이동 | Tab 키로 닫기",
            bg='#263238',
            fg='#78909C',
            font=('Arial', 8, 'italic')
        )
        close_hint.pack(pady=(4, 8))
        
        # 자동 닫기 (15초 후)
        self.parent.after(15000, self.hide_hovering_buttons)
    
    def _create_action_button(self, parent: tk.Frame, action: Dict, index: int):
        """작업 버튼 생성"""
        button_frame = tk.Frame(parent, bg='#263238')
        button_frame.pack(fill=tk.X, padx=8, pady=2)
        
        # 작업 정보 버튼
        time_str = action['timestamp'].strftime('%H:%M:%S')
        data_str = str(action['data'])[:25] + ('...' if len(str(action['data'])) > 25 else '')
        
        btn = tk.Button(
            button_frame,
            text=f"#{index+1} [{action['type']}] {data_str}",
            bg='#37474F',
            fg='#ECEFF1',
            font=('Arial', 8),
            relief=tk.FLAT,
            anchor='w',
            padx=8,
            pady=6,
            command=lambda a=action: self._on_action_click(a)
        )
        btn.pack(side=tk.LEFT, fill=tk.X, expand=True)
        
        # 호버 효과
        btn.bind('<Enter>', lambda e, b=btn: b.config(bg='#455A64'))
        btn.bind('<Leave>', lambda e, b=btn: b.config(bg='#37474F'))
        
        # 하이퍼링크 버튼 (링크가 있는 경우)
        if action.get('link'):
            link_btn = tk.Button(
                button_frame,
                text="🔗",
                bg='#00ACC1',
                fg='#FFFFFF',
                font=('Arial', 9),
                relief=tk.FLAT,
                padx=6,
                pady=4,
                command=lambda l=action['link']: self._open_link(l)
            )
            link_btn.pack(side=tk.RIGHT, padx=(4, 0))
            
            # 링크 버튼 호버 효과
            link_btn.bind('<Enter>', lambda e, b=link_btn: b.config(bg='#00BCD4'))
            link_btn.bind('<Leave>', lambda e, b=link_btn: b.config(bg='#00ACC1'))
    
    def _on_action_click(self, action: Dict):
        """작업 버튼 클릭 핸들러"""
        print(f"[작업 인덱스 클릭] {action['type']}: {action['data']}")
        # 여기에 해당 작업으로 이동하는 로직 추가
        self.history.add_action('NAVIGATE', {'target': action['type']})
    
    def _open_link(self, link: str):
        """하이퍼링크 열기"""
        try:
            webbrowser.open(link)
            self.history.add_action('LINK_OPEN', {'url': link})
            print(f"[링크 열림] {link}")
        except Exception as e:
            print(f"[링크 열기 실패] {e}")
    
    def _show_empty_message(self, x: int, y: int):
        """빈 메시지 표시"""
        self.button_window = tk.Toplevel(self.parent)
        self.button_window.overrideredirect(True)
        self.button_window.attributes('-topmost', True)
        self.button_window.geometry(f"+{x+20}+{y}")
        
        label = tk.Label(
            self.button_window,
            text="📭 아직 기록된 작업이 없습니다",
            bg='#263238',
            fg='#B0BEC5',
            font=('Arial', 9),
            padx=20,
            pady=15
        )
        label.pack()
        
        self.is_hovering = True
        self.parent.after(3000, self.hide_hovering_buttons)
    
    def hide_hovering_buttons(self):
        """호버링 버튼 숨기기"""
        if self.button_window:
            self.button_window.destroy()
            self.button_window = None
        self.is_hovering = False


class HummingbirdSystemDemo:
    """Hummingbird UI System 통합 데모"""
    
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("🐦 Hummingbird UI System - 벌새 인터랙션")
        self.root.geometry("900x650")
        self.root.configure(bg='#ECEFF1')
        
        # 컴포넌트 초기화
        self.history = ActionHistory()
        self.pairing = PairingSystem(self.root, delay_ms=1600)
        self.hovering = HoveringSystem(self.root, self.history)
        
        self.current_mouse_pos = (0, 0)
        self.canvas_objects = []  # 캔버스 객체 추적
        
        self._setup_ui()
        self._setup_event_handlers()
        self._add_sample_data()
    
    def _setup_ui(self):
        """UI 설정"""
        # 헤더
        header = tk.Frame(self.root, bg='#1976D2', height=70)
        header.pack(fill=tk.X)
        header.pack_propagate(False)
        
        title = tk.Label(
            header,
            text="🐦 Hummingbird UI System",
            bg='#1976D2',
            fg='white',
            font=('Arial', 18, 'bold')
        )
        title.pack(side=tk.LEFT, padx=20, pady=12)
        
        subtitle = tk.Label(
            header,
            text="페어링 시스템 + 호버링 시스템",
            bg='#1976D2',
            fg='#B3E5FC',
            font=('Arial', 10)
        )
        subtitle.pack(side=tk.LEFT, padx=(0, 20))
        
        # 안내 메시지
        info_frame = tk.Frame(self.root, bg='#FFFFFF')
        info_frame.pack(fill=tk.X, padx=20, pady=10)
        
        info_text = """
        ✨ 사용 방법:
        1️⃣ 마우스를 객체 위에 올리고 1.6초 대기 → 페어링 정보 박스 표시
        2️⃣ 페어링 상태에서 'i' 키 → 사전적 정의 표시
        3️⃣ 페어링 상태에서 'Tab' 키 → 작업 히스토리 버튼 표시 (마우스 오른쪽)
        4️⃣ 버튼 클릭으로 작업 이동, 🔗 버튼으로 링크 열기
        """
        
        info_label = tk.Label(
            info_frame,
            text=info_text,
            bg='#FFFFFF',
            fg='#37474F',
            font=('Arial', 9),
            justify=tk.LEFT
        )
        info_label.pack(padx=10, pady=8)
        
        # 작업 영역 (캔버스)
        canvas_frame = tk.Frame(self.root, bg='#ECEFF1')
        canvas_frame.pack(fill=tk.BOTH, expand=True, padx=20, pady=10)
        
        canvas_label = tk.Label(
            canvas_frame,
            text="📐 작업 영역 - 클릭하여 객체 생성",
            bg='#ECEFF1',
            fg='#616161',
            font=('Arial', 9, 'bold')
        )
        canvas_label.pack(anchor='w', pady=(0, 5))
        
        self.canvas = tk.Canvas(
            canvas_frame,
            bg='#FAFAFA',
            highlightthickness=2,
            highlightbackground='#00ACC1'
        )
        self.canvas.pack(fill=tk.BOTH, expand=True)
        
        # 상태바
        status_frame = tk.Frame(self.root, bg='#37474F', height=35)
        status_frame.pack(fill=tk.X)
        status_frame.pack_propagate(False)
        
        self.status_label = tk.Label(
            status_frame,
            text="🟢 준비됨 | 객체 위에 마우스를 올려보세요",
            bg='#37474F',
            fg='#ECEFF1',
            font=('Arial', 9),
            anchor='w'
        )
        self.status_label.pack(side=tk.LEFT, padx=10, fill=tk.X, expand=True)
        
        # 시스템 상태 표시
        self.system_status = tk.Label(
            status_frame,
            text="페어링: ⚪ | 호버링: ⚪",
            bg='#37474F',
            fg='#B0BEC5',
            font=('Arial', 8)
        )
        self.system_status.pack(side=tk.RIGHT, padx=10)
    
    def _setup_event_handlers(self):
        """이벤트 핸들러 설정"""
        # 마우스 이동
        self.canvas.bind('<Motion>', self._on_mouse_move)
        
        # 마우스 클릭
        self.canvas.bind('<Button-1>', self._on_canvas_click)
        
        # 'i' 키 - 사전적 정의
        self.root.bind('i', self._on_i_key)
        self.root.bind('I', self._on_i_key)
        
        # Tab 키 - 호버링 버튼
        self.root.bind('<Tab>', self._on_tab_key)
        
        # 캔버스 떠날 때
        self.canvas.bind('<Leave>', self._on_canvas_leave)
    
    def _on_mouse_move(self, event):
        """마우스 이동 핸들러"""
        self.current_mouse_pos = (event.x_root, event.y_root)
        
        # 객체 위에 있는지 확인
        obj_type = self._get_object_at_position(event.x, event.y)
        
        if obj_type:
            self.pairing.start_pairing_detection(event.x_root, event.y_root, obj_type)
            self.status_label.config(
                text=f"🔵 페어링 감지 중... | 위치: ({event.x}, {event.y})"
            )
            self._update_system_status(pairing=True)
        else:
            self.pairing.stop_pairing_detection()
            self.pairing.hide_pairing_info()
            self.status_label.config(
                text=f"🟢 준비됨 | 위치: ({event.x}, {event.y})"
            )
            self._update_system_status(pairing=False)
    
    def _on_canvas_click(self, event):
        """캔버스 클릭 핸들러"""
        x, y = event.x, event.y
        
        # 원 그리기
        obj_id = self.canvas.create_oval(
            x-15, y-15, x+15, y+15,
            fill='#2196F3',
            outline='#1976D2',
            width=2,
            tags='clickable'
        )
        self.canvas_objects.append(obj_id)
        
        # 히스토리에 기록 (링크 포함)
        self.history.add_action(
            'CREATE_OBJECT',
            {'position': (x, y), 'type': 'circle', 'id': obj_id},
            link='https://docs.python.org/3/library/tkinter.html'
        )
        
        self.status_label.config(
            text=f"✅ 객체 생성 완료: ({x}, {y}) | 총 {len(self.canvas_objects)}개"
        )
    
    def _on_i_key(self, event):
        """'i' 키 핸들러 - 사전적 정의 표시"""
        if self.pairing.is_pairing or self.pairing.current_object:
            x, y = self.current_mouse_pos
            self.pairing.show_dictionary_definition(x, y)
            self.status_label.config(text="📖 사전적 정의 표시 중")
            self.history.add_action('SHOW_DEFINITION', {'object': self.pairing.current_object})
        return 'break'
    
    def _on_tab_key(self, event):
        """Tab 키 핸들러 - 호버링 버튼 표시"""
        if self.pairing.is_pairing or self.pairing.current_object:
            x, y = self.current_mouse_pos
            self.hovering.show_hovering_buttons(x, y)
            self.status_label.config(text="🗂️ 작업 히스토리 버튼 표시")
            self._update_system_status(hovering=True)
            self.history.add_action('SHOW_HISTORY', {'source': 'tab_key'})
        return 'break'
    
    def _on_canvas_leave(self, event):
        """캔버스 떠날 때"""
        self.pairing.stop_pairing_detection()
        self.pairing.hide_pairing_info()
    
    def _get_object_at_position(self, x: int, y: int) -> Optional[str]:
        """특정 위치의 객체 타입 반환"""
        # 캔버스 객체 확인
        items = self.canvas.find_overlapping(x-2, y-2, x+2, y+2)
        if items:
            return 'canvas_object'
        
        # 위젯 확인
        widget = self.root.winfo_containing(self.root.winfo_pointerx(), 
                                            self.root.winfo_pointery())
        if isinstance(widget, tk.Button):
            return 'button'
        elif isinstance(widget, tk.Label):
            return 'text'
        
        return None
    
    def _update_system_status(self, pairing: bool = None, hovering: bool = None):
        """시스템 상태 표시 업데이트"""
        if pairing is not None:
            p_status = "🟢" if pairing else "⚪"
        else:
            p_status = "🟢" if self.pairing.is_pairing else "⚪"
        
        if hovering is not None:
            h_status = "🟢" if hovering else "⚪"
        else:
            h_status = "🟢" if self.hovering.is_hovering else "⚪"
        
        self.system_status.config(text=f"페어링: {p_status} | 호버링: {h_status}")
    
    def _add_sample_data(self):
        """샘플 데이터 추가"""
        # 초기 작업 히스토리 추가
        self.history.add_action(
            'SYSTEM_START',
            {'message': '시스템 초기화 완료'},
            link='https://github.com/hummingbird-ui'
        )


def main():
    """메인 함수"""
    root = tk.Tk()
    app = HummingbirdSystemDemo(root)
    root.mainloop()


if __name__ == '__main__':
    main()