# Unity UI Binding System

Unity UI 컴포넌트와 UI 로직을 문자열 Key로 연결하는 경량 Binding System입니다.

`Button`, `Toggle`, `Slider`, `TMP_Text`, `TMP_Dropdown` 등의 Unity UI 컴포넌트를 코드에서 직접 참조하지 않고, Property / Command / Event Binding을 통해 UI와 로직을 분리할 수 있습니다.

## Features

- String-Key 기반 UI Binding
- Property Binding
- Command Binding
- UI Event Binding
- 하나의 Key를 여러 UI 컴포넌트에 Binding
- 동적으로 생성되는 Binder 자동 등록
- 중첩 BaseUI Scope 지원
- Inspector Binding Validation
- BaseUI Inspector에서 Binding 관계 확인
- Reflection metadata cache
- IL2CPP 지원
- UniTask 등 외부 Runtime dependency 없음

## Concept

ViewModel을 자동 생성하거나 Property 변경을 감시하는 대신, UI 로직과 Unity UI 컴포넌트 사이를 Key로 연결합니다.

Binding의 방향은 다음과 같습니다.

Property

    BaseUI
      ↓
    Binder
      ↓
    UI Component

Command

    BaseUI
      ↓
    Binder
      ↓
    UI Component Method

Event

    UI Component
      ↓
    Binder
      ↓
    BaseUI Event Handler

예를 들어 다음 Key가 있다고 가정합니다.

    CanSubmit
    SelectedIndex
    Submit

Inspector에서는 다음과 같이 연결할 수 있습니다.

    CanSubmit
        ButtonBinder → Interactable

    SelectedIndex
        TMP_DropdownBinder → SetValueWithoutNotify

    Submit
        ButtonBinder → OnClick
        BaseUI → Submit(Button)

코드에서는 실제 Button이나 TMP_Dropdown을 알 필요가 없습니다.

## Installation

Unity Package Manager에서 다음 Git URL을 추가합니다.

    https://github.com/causeless8t/Unity-UI-Binding-System.git

Unity에서:

    Window
    → Package Manager
    → +
    → Add package from git URL...

## Basic Usage

### Property Binding

Inspector에서 ButtonBinder를 다음과 같이 설정합니다.

    Key       : CanSubmit
    Type      : Interactable

코드에서는:

    BroadcastSetProperty(
        nameof(CanSubmit),
        CanSubmit);

동일한 Key는 여러 Binder에서 사용할 수 있습니다.

예를 들어:

    CanSubmit
        ButtonBinder → Interactable
        TMP_TextBinder → Color

처럼 하나의 상태를 여러 UI 요소에 전달할 수 있습니다.

### Property Getter

Binder의 Getter Key를 설정하면 UI Component 자체를 가져올 수도 있습니다.

    Button SubmitButton

코드:

    var button =
        BroadcastGetProperty<Button>(
            nameof(SubmitButton));

Getter는 하나의 결과를 반환하므로 동일 Scope에서 Getter Key가 중복되지 않도록 구성하는 것을 권장합니다.

### Command Binding

Property 변경이 아니라 UI Component의 특정 동작을 실행해야 할 경우 Command를 사용할 수 있습니다.

예를 들어 TMP_Dropdown의:

    SetValueWithoutNotify(int)

를 호출하려면 Inspector에서:

    Key       : SelectedIndex
    Type      : SetValueWithoutNotify

를 설정합니다.

코드:

    BroadcastCommand(
        nameof(SelectedIndex),
        selectedIndex);

UI 로직은 `TMP_Dropdown.SetValueWithoutNotify()`를 직접 호출할 필요가 없습니다.

### Event Binding

UI Event는 `[UIEvent]` Attribute를 사용해 BaseUI의 메서드와 연결합니다.

    [ButtonClickEventRegister(nameof(Submit))]
    private void Submit(Button button)
    {
        // Submit
    }

Key를 생략하면 Method 이름이 자동으로 Key가 됩니다.

위 코드의 Key는:

    Submit

입니다.

Inspector의 ButtonBinder에서:

    Key       : Submit
    Type      : OnClick

을 설정하면 연결됩니다.

다른 이름을 사용하려면 Key를 명시할 수 있습니다.

    [ButtonClickEventRegister("Submit")]
    private void OnSubmitButtonClicked(Button button)
    {
    }

## Key Convention

Key는 개별 UI GameObject의 이름이 아니라 코드의 의미를 표현하는 것을 권장합니다.

Property Key는 일반적으로 Property 또는 Variable 이름을 사용합니다.

    CanSubmit
    CharacterName
    SelectedIndex

Command Key는 해당 UI 동작을 발생시키는 코드상의 의미를 사용합니다.

    RefreshSelection
    ResetScrollPosition

Event Key는 Event Handler의 이름을 사용하는 것을 권장합니다.

    Submit
    Cancel
    SelectCharacter

가능하면 문자열 literal 대신 `nameof()`를 사용하세요.

    BroadcastSetProperty(nameof(CanSubmit), CanSubmit);

## Multiple Bindings

동일한 Key는 여러 Binder에서 사용할 수 있습니다.

예를 들어:

    PlayerName
        TMP_TextBinder → Text
        TMP_InputFieldBinder → Text

`BroadcastSetProperty()`는 해당 Key를 지원하는 모든 Binder에 값을 전달합니다.

    BroadcastSetProperty(nameof(PlayerName), playerName);

단, 하나의 Binder 내부에서는 하나의 Key가 하나의 Binding Type만 가져야 합니다.

잘못된 예:

    Submit → Interactable
    Submit → OnClick

하나의 Binder 내부에서 중복 Key가 발견되면 Validation Error가 표시됩니다.

## Dynamic Binder Registration

Binder는 자신이 속한 가장 가까운 `IBinderManager`에 자동으로 등록됩니다.

따라서 Runtime에 생성되는 UI도 별도의 수동 등록 과정이 필요하지 않습니다.

    BaseUI
    └── Runtime Created Object
        └── ButtonBinder

Binder가 생성되면 현재 Hierarchy의 가장 가까운 Manager를 찾아 등록합니다.

부모가 변경되면 기존 Manager에서 제거하고 새로운 Manager에 등록됩니다.

## Nested UI Scope

`BaseUI`를 중첩해서 사용할 수 있습니다.

    InventoryUI
    ├── ItemButtonBinder
    │
    └── ConfirmPopupUI
        └── ConfirmButtonBinder

`ItemButtonBinder`는 `InventoryUI`에 속하고,

`ConfirmButtonBinder`는 가장 가까운 Manager인 `ConfirmPopupUI`에 속합니다.

따라서 각 UI의 Binding Scope를 독립적으로 유지할 수 있습니다.

## Binding Inspector

BaseUI Inspector에서는 현재 UI Scope에 포함된 Binding 관계를 확인할 수 있습니다.

예:

    UI Bindings

    CanSubmit
        Binding    ButtonBinder
                   Interactable

    SelectedIndex
        Binding    TMP_DropdownBinder
                   SetValueWithoutNotify

    Submit
        Binding    ButtonBinder
                   OnClick

        Handler    CharacterUI
                   Submit(Button)

`Select` 버튼을 사용하면 해당 Binder를 Hierarchy에서 바로 찾을 수 있습니다.

Nested BaseUI 내부의 Binder는 각각 자신의 Scope에만 표시됩니다.

## Validation

Binder Inspector는 잘못된 Binding 설정을 검사합니다.

현재 다음 항목을 검사합니다.

- Empty Key
- Duplicate Key
- Getter Key와 Binding Key 충돌

Runtime에서도 중복 Key 등록을 방지하므로 Editor Validation을 통과하지 못한 설정이 실행되는 경우를 추가로 방어합니다.

## Architecture

주요 Runtime 구조는 다음과 같습니다.

    BaseUI
      │
      ├── BinderRegistry
      │
      ├── Property Binding
      ├── Command Binding
      └── Event Binding
              │
              ▼
        ComponentBinder<T>
              │
              ├── ButtonBinder
              ├── ToggleBinder
              ├── SliderBinder
              ├── ImageBinder
              ├── TMP_TextBinder
              └── TMP_DropdownBinder

### IBinder

모든 Binder가 구현하는 기본 인터페이스입니다.

    public interface IBinder
    {
        void Bind();
        bool HasKey(string key);
    }

### IPropertyBinder<T>

값을 UI Component에 전달하거나 가져옵니다.

    public interface IPropertyBinder<T>
    {
        void SetProperty(string key, T value);

        T GetProperty(string key);
    }

### ICommandBinder<T>

UI Component의 동작을 실행합니다.

    public interface ICommandBinder<T>
    {
        void ExecuteCommand(string key, T parameter);
    }

### IEventBinder

Unity UI Event와 BaseUI Event Handler를 연결합니다.

    public interface IEventBinder
    {
        void AddListener(string key, Delegate action);

        void RemoveListener(string key, Delegate action);
    }

## IL2CPP

UI Event Handler는 Reflection을 사용해 검색됩니다.

`UIRegisterEventAttribute`는 Unity의 `PreserveAttribute`를 기반으로 구현되어 있어 IL2CPP Managed Code Stripping 환경에서도 Event Handler가 제거되지 않도록 구성되어 있습니다.

Runtime에서 Reflection Emit이나 동적 코드 생성은 사용하지 않습니다.

Reflection 결과는 UI Type 단위로 캐싱됩니다.

## Dependencies

Runtime에서 별도의 외부 라이브러리를 요구하지 않습니다.

Unity 기본 UI와 TextMeshPro를 사용합니다.

## Design Goals

이 프로젝트는 다음 문제를 가볍게 해결하는 것을 목표로 합니다.

- UI 코드에서 Unity Component 직접 참조 감소
- UI Hierarchy와 로직의 결합도 감소
- 동일 상태를 여러 UI 요소에 쉽게 전달
- 동적 UI Binding 지원
- String-Key Binding의 추적 어려움을 Editor Tool로 보완
- 최소한의 Runtime Reflection 및 외부 Dependency 유지

복잡한 Reactive State Management나 자동 Property Change Tracking이 필요한 프로젝트라면 별도의 MVVM/Reactive Framework가 더 적합할 수 있습니다.

이 패키지는 명시적이고 단순한 UI Binding을 원하는 프로젝트를 대상으로 합니다.

## License

See `LICENSE.md`.