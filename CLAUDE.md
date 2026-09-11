# CLAUDE.md

이 문서는 Claude Code가 이 저장소에서 코드를 수정하거나 새 기능을 구현할 때 따라야 할 프로젝트 구조와 개발 원칙을 정의합니다.

## 프로젝트 개요

Unity UI Binding System은 Unity UI 컴포넌트와 UI 로직을 **String Key 기반의 Property / Command / Event Binding**으로 연결하는 경량 UI 프레임워크입니다.

이 프로젝트는 MVVM 프레임워크가 아닙니다.

ViewModel 생성, Reactive Property, 자동 상태 추적 등을 제공하는 것이 목적이 아니며, Unity UI 컴포넌트에 대한 직접 참조를 줄이고 UI 로직과 View 사이의 결합도를 낮추는 것이 핵심 목적입니다.

핵심 구조는 다음과 같습니다.

```text
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
```

---

# 핵심 설계 원칙

## 1. String Key는 의도된 설계다

Binding은 String Key를 사용합니다.

String Key를 enum, expression tree, generated ID, source-generated property reference 등으로 임의 변경하지 마세요.

예:

```csharp
BroadcastSetProperty(
    nameof(CanSubmit),
    CanSubmit);
```

가능하면 문자열 literal보다 `nameof()`를 사용합니다.

```csharp
// Good
BroadcastCommand(
    nameof(SelectedIndex),
    selectedIndex);

// Avoid
BroadcastCommand(
    "SelectedIndex",
    selectedIndex);
```

String Key의 추적 어려움은 Runtime 구조를 복잡하게 만드는 대신 Editor Tool과 Validation으로 보완합니다.

---

## 2. Key는 UI Object가 아니라 코드의 의미를 나타낸다

Key는 GameObject 이름이나 특정 UI Element를 식별하기 위한 ID가 아닙니다.

Property Key는 일반적으로 변수 또는 Property 이름을 사용합니다.

```text
CanSubmit
PlayerName
SelectedIndex
```

Command Key는 UI에 요청하는 동작의 의미를 나타냅니다.

```text
RefreshSelection
ResetScrollPosition
```

Event Key는 Event Handler 이름을 사용하는 것을 권장합니다.

```text
Submit
Cancel
SelectCharacter
```

---

## 3. 하나의 Key는 여러 Binder에 존재할 수 있다

동일한 Key가 여러 UI Component에 Binding되는 것은 정상적인 사용 방법입니다.

예:

```text
CanSubmit
├── ButtonBinder → Interactable
└── TMP_TextBinder → Color
```

따라서 다음 동작은 모든 matching Binder를 대상으로 해야 합니다.

```csharp
BroadcastSetProperty(...)
BroadcastCommand(...)
AddListener(...)
RemoveListener(...)
```

Registry에서 이러한 동작을 `FindFirst()` 방식으로 변경하지 마세요.

---

## 4. 하나의 Binder 내부에서는 Key가 중복되면 안 된다

여러 Binder가 동일 Key를 가지는 것은 허용하지만, 하나의 Binder 안에서는 하나의 Key가 하나의 Binding 동작만 의미해야 합니다.

잘못된 예:

```text
ButtonBinder

Submit → Interactable
Submit → OnClick
```

Binder 내부의 Binding lookup은 기본적으로 다음과 같은 구조를 사용합니다.

```csharp
Dictionary<string, BindingType>
```

중복 Key는 Editor Validation과 Runtime BindingMap 양쪽에서 차단합니다.

중복 Key를 허용하기 위해 다음과 같은 구조로 변경하지 마세요.

```csharp
Dictionary<string, List<BindingType>>
```

---

# Binder 구조

## IBinder

모든 Binder의 기본 인터페이스입니다.

```csharp
public interface IBinder
{
    void Bind();
    bool HasKey(string key);
}
```

## Capability Interfaces

Binder 기능은 capability interface로 분리합니다.

```csharp
public interface IPropertyBinder<T>
{
    void SetProperty(string key, T value);
    T GetProperty(string key);
}

public interface ICommandBinder<T>
{
    void ExecuteCommand(string key, T parameter);
}

public interface IEventBinder
{
    void AddListener(string key, Delegate action);
    void RemoveListener(string key, Delegate action);
}
```

Capability interface가 `IBinder`를 상속하도록 변경하지 마세요.

Concrete Binder가 필요한 capability를 선택적으로 구현합니다.

따라서 Binder Registry에서 capability interface를 검색하는 Generic API는 반드시 `IBinder` 제약을 요구할 필요가 없습니다.

예:

```csharp
FindAll<IPropertyBinder<bool>>(key);
```

---

# ComponentBinder

Unity Component를 대상으로 하는 Binder는 공통적으로 `ComponentBinder<T>`를 기반으로 구현합니다.

Binder는 자신의 Target Component를 관리하며 Binding registration을 담당합니다.

새 Binder를 구현할 때 기존 Binder와 동일한 lifecycle과 naming convention을 따르세요.

가능하면 다음 serialized 구조를 사용합니다.

```csharp
[SerializeField]
private List<BindInfo> _bindings = new();

[Serializable]
private struct BindInfo
{
    public string Key;
    public BindingType Type;
}
```

Editor Tool이 `_bindings`, `Key`, `Type` 구조를 사용하므로 특별한 이유 없이 필드명을 변경하지 마세요.

Binding 데이터는 Awake 단계에서 lookup 구조로 변환하는 것을 우선합니다.

Binding 호출 시마다 serialized list를 순회하지 마세요.

예:

```csharp
private readonly BindingMap<BindingType> _bindingMap = new();

protected override void BuildBindings()
{
    _bindingMap.Clear();

    foreach (var binding in _bindings)
    {
        _bindingMap.Add(
            binding.Key,
            binding.Type,
            this);
    }
}
```

---

# Getter Key

`ComponentBinder<T>`는 필요에 따라 Component 자체를 반환하기 위한 Getter Key를 가질 수 있습니다.

예:

```csharp
[SerializeField]
protected string getterKey;
```

Getter Key 역시 Binding Key와 동일한 String-Key 정책을 따릅니다.

`HasKey()`를 override할 때 base Getter Key 검사를 누락하지 마세요.

```csharp
public override bool HasKey(string key)
{
    return base.HasKey(key) ||
           _bindingMap.Contains(key);
}
```

잘못된 예:

```csharp
public override bool HasKey(string key)
{
    return _bindingMap.Contains(key);
}
```

위와 같이 구현하면 `getterKey`가 Registry 검색에서 발견되지 않습니다.

---

# Binder Registration

Binder는 동적으로 생성될 수 있습니다.

따라서 Binder가 자신이 속한 `IBinderManager`를 찾아 자동 등록하는 구조는 의도된 설계입니다.

Binder는 현재 Hierarchy에서 가장 가까운 `IBinderManager`에 등록됩니다.

```text
InventoryUI
├── ButtonBinder
└── PopupUI
    └── ButtonBinder
```

위 구조에서:

```text
InventoryUI/ButtonBinder
→ InventoryUI

PopupUI/ButtonBinder
→ PopupUI
```

에 등록됩니다.

Nested BaseUI의 Binding Scope를 침범하지 마세요.

## Bind()는 registration 보장 연산이다

`Bind()`는 단순히 Manager가 변경되었는지 검사하는 함수가 아닙니다.

현재 Manager의 Registry에 Binder가 등록되어 있음을 보장해야 합니다.

예:

```csharp
public override void Bind()
{
    var manager =
        GetComponentInParent<IBinderManager>(true);

    if (!ReferenceEquals(
            _binderManager,
            manager))
    {
        _binderManager?.UnregisterBinder(this);
        _binderManager = manager;
    }

    _binderManager?.RegisterBinder(this);
}
```

다음 최적화를 하지 마세요.

```csharp
if (ReferenceEquals(
        _binderManager,
        manager))
{
    return;
}
```

`BaseUI.SearchBinders()`가 Registry를 Clear한 뒤 동일 Binder를 다시 등록해야 할 수 있기 때문입니다.

Manager reference가 같다는 것은 Registry membership을 의미하지 않습니다.

## OnDisable에서 Binder를 제거하지 않는다

UI GameObject가 비활성화되었다고 해서 Binding 구조 자체에서 Binder가 제거되는 것은 아닙니다.

따라서 기본적으로 `OnDisable()`에서 Binder Registry 등록을 해제하지 마세요.

Binder unregister는 Binder가 실제로 제거되는 lifecycle에서 수행합니다.

```text
OnDestroy
→ UnregisterBinder
```

UI Event listener의 활성화/비활성화 lifecycle은 Binder Registry membership과 별개의 문제입니다.

---

# BinderRegistry

BinderRegistry는 다음 특성을 유지해야 합니다.

- Binder 등록 순서 보존
- Binder instance 중복 등록 방지
- 여러 Binder가 동일 Key를 가지는 것 허용
- `FindFirst<T>()` 지원
- `FindAll<T>()` 지원

일반적으로 다음 기준을 사용합니다.

```text
GetProperty → FindFirst
SetProperty → FindAll
Command     → FindAll
Event       → FindAll
```

`BroadcastGetProperty<T>()`는 하나의 값을 반환해야 하므로 `FindFirst()`를 사용합니다.

동일 Scope에서 Getter Key가 중복되지 않도록 구성하는 것을 권장합니다.

---

# BaseUI

`BaseUI`는 현재 UI Scope의 Binder들을 관리합니다.

주요 역할은 다음과 같습니다.

```text
Binder Registry 관리
Property 전달
Command 전달
UI Event listener 관리
Binding 재검색
```

Property 설정은 해당 Key를 가진 모든 Binder를 대상으로 합니다.

```csharp
public void BroadcastSetProperty<T>(
    string key,
    T value)
{
    foreach (var binder in
             _binderRegistry.FindAll<IPropertyBinder<T>>(key))
    {
        binder.SetProperty(key, value);
    }
}
```

Command도 동일하게 여러 Binder로 Broadcast할 수 있습니다.

```csharp
public void BroadcastCommand<T>(
    string key,
    T parameter)
{
    foreach (var binder in
             _binderRegistry.FindAll<ICommandBinder<T>>(key))
    {
        binder.ExecuteCommand(
            key,
            parameter);
    }
}
```

---

# SearchBinders

`SearchBinders()`는 현재 UI Scope의 Binder Registry를 다시 구축합니다.

Registry를 Clear한 이후에는 기존 Binder도 다시 등록되어야 합니다.

또한 UI Event가 이미 등록된 상태에서 Search를 수행한다면 listener 중복 등록에 주의해야 합니다.

권장 흐름:

```text
현재 Event 등록 상태 저장
        ↓
기존 Event Listener 제거
        ↓
BinderRegistry.Clear()
        ↓
Binder.Bind()
        ↓
Event Listener 재등록
```

Dynamic Binder registration과 Search 과정에서 동일 Event listener가 중복 등록되지 않도록 유지하세요.

---

# UI Event

UI Component에서 `BaseUI`로 전달되는 Event는 `UIRegisterAttribute`를 기반으로 하는 구체적인 Event Attribute를 사용합니다.

`UIRegisterAttribute`는 공통 메타데이터를 정의하는 추상 Attribute입니다.

```csharp
[AttributeUsage(AttributeTargets.Method)]
public abstract class UIRegisterAttribute :
    PreserveAttribute
{
    public string Key;

    public abstract Type DelegateType { get; }

    public UIRegisterAttribute(string Key)
    {
        this.Key = Key;
    }

    public UIRegisterAttribute()
    {
    }
}
```

각 UI Event는 자신의 Delegate Type을 알고 있는 구체적인 Attribute를 제공합니다.

현재 주요 Event Attribute는 다음과 같습니다.

- `ButtonClickEventRegisterAttribute`
- `ToggleValueChangedEventRegisterAttribute`
- `LongTapButtonClickEventRegisterAttribute`
- `DropdownValueChangedEventRegisterAttribute`
- `SliderValueChangedEventRegisterAttribute`
- `InputFieldEventRegisterAttribute`

## Event Attribute 사용

Button Click Event:

```csharp
[ButtonClickEventRegister]
private void Submit(Button button)
{
}
```

`Key`를 지정하지 않으면 Method 이름이 자동으로 Binding Key가 됩니다.

위 코드의 Key는 다음과 같습니다.

```text
Submit
```

명시적인 Key가 필요한 경우 생성자에 전달합니다.

```csharp
[ButtonClickEventRegister("Submit")]
private void OnSubmitButtonClicked(
    Button button)
{
}
```

이 경우 Method 이름과 관계없이 Binding Key는 `Submit`입니다.

다른 UI Event도 동일한 방식으로 사용합니다.

```csharp
[ToggleValueChangedEventRegister]
private void OnToggleChanged(
    Toggle toggle,
    bool value)
{
}

[DropdownValueChangedEventRegister]
private void OnDropdownChanged(
    TMP_Dropdown dropdown,
    int index)
{
}

[SliderValueChangedEventRegister]
private void OnSliderChanged(
    Slider slider,
    float value)
{
}

[InputFieldEventRegister]
private void OnInputChanged(
    TMP_InputField inputField,
    string value)
{
}

[LongTapButtonClickEventRegister]
private void OnLongTap(
    LongTapButton button)
{
}
```

## Event Attribute 설계 원칙

현재의 UI Event별 Attribute 구조는 의도된 Public API입니다.

여러 Event Attribute를 하나의 범용 Attribute로 통합하지 마세요.

예를 들어 다음 형태로 변경하지 않습니다.

```csharp
// 사용하지 않는 방향

[UIEvent(typeof(Action<Button>))]
private void Submit(Button button)
{
}
```

현재 구조에서는 호출부에서 Delegate Type을 반복해서 지정하는 것보다 Event의 의미를 Attribute 이름으로 직접 표현하는 것을 우선합니다.

```csharp
// Preferred

[ButtonClickEventRegister]
private void Submit(Button button)
{
}
```

지원해야 하는 UI Event 종류가 제한적이므로 구체적인 Event Attribute가 여러 개 존재하는 것은 의도된 설계입니다.

새로운 UI Event를 지원해야 하는 경우 기존 Attribute를 범용 Attribute로 통합하기보다 필요에 따라 `UIRegisterAttribute`를 상속하는 새로운 구체 Attribute를 추가하는 것을 우선합니다.

예:

```csharp
public class CustomEventRegisterAttribute :
    UIRegisterAttribute
{
    public override Type DelegateType =>
        typeof(Action<CustomComponent, int>);

    public CustomEventRegisterAttribute()
    {
    }

    public CustomEventRegisterAttribute(string Key)
        : base(Key)
    {
    }
}
```

---

# UI Event Reflection

UI Event Handler를 검색할 때는 특정 Event Attribute 타입을 하나씩 검사하지 않습니다.

공통 부모 타입인 `UIRegisterAttribute`를 기준으로 검색합니다.

```csharp
var attribute =
    method.GetCustomAttribute<UIRegisterAttribute>(true);
```

따라서 새로운 Event Attribute를 추가해도 Event Handler 검색 로직을 수정할 필요가 없어야 합니다.

Event Binding에서 공통적으로 사용하는 정보는 다음 두 가지입니다.

```csharp
attribute.Key
attribute.DelegateType
```

Key가 비어 있으면 Method 이름을 사용합니다.

```csharp
var key =
    string.IsNullOrWhiteSpace(attribute.Key)
        ? method.Name
        : attribute.Key;
```

구체 Attribute마다 Reflection 분기를 추가하지 마세요.

잘못된 예:

```csharp
if (attribute is ButtonClickEventRegisterAttribute)
{
}
else if (attribute is ToggleValueChangedEventRegisterAttribute)
{
}
```

---

# Reflection

Reflection 사용 자체를 무조건 제거하지 마세요.

현재 UI Event metadata 검색에는 Reflection을 사용하지만 결과는 UI Type 단위로 캐싱합니다.

피해야 할 구조:

- 매 Frame Reflection
- Binding 호출마다 Reflection
- AppDomain 전체 Assembly Scan
- Reflection.Emit
- 불필요한 Runtime code generation

허용되는 구조:

```text
Concrete BaseUI Type
       ↓
Reflection Scan
       ↓
Metadata Cache
       ↓
Reuse
```

성능 최적화를 이유로 Source Generator 같은 큰 의존성을 추가하지 마세요.

---

# IL2CPP

`UIRegisterAttribute`는 `UnityEngine.Scripting.PreserveAttribute`를 상속합니다.

```csharp
public abstract class UIRegisterAttribute :
    PreserveAttribute
```

Reflection을 통해 검색되는 UI Event Handler가 IL2CPP Managed Code Stripping 과정에서 제거되지 않도록 하기 위한 의도된 구조입니다.

UI Event Attribute 구조를 수정할 경우 Preserve 동작을 제거하지 마세요.

Reflection Emit이나 Runtime 동적 코드 생성은 사용하지 않습니다.

---

# Editor Tool

String-Key Binding의 단점은 Editor Tool로 보완합니다.

Editor 기능은 다음 역할을 담당합니다.

- Empty Key 검사
- Duplicate Key 검사
- Getter Key와 Binding Key 충돌 검사
- BaseUI Binding 관계 표시
- Binder 위치 탐색

Runtime API를 Editor 편의를 위해 확장하지 마세요.

예를 들어 다음 API를 `IBinder`에 추가하지 마세요.

```csharp
IEnumerable<BindingInfo> GetBindings();
```

Editor에서는 `SerializedObject`를 통해 Binder 정보를 읽는 것을 우선합니다.

---

# Binder Validation

Binder의 serialized Binding은 가능한 한 공통 구조를 유지합니다.

```text
_bindings
 └── Key
 └── Type
```

이를 이용해 하나의 공통 Binder Editor/Validator로 대부분의 Binder를 처리합니다.

모든 Binder마다 별도의 CustomEditor를 만들지 마세요.

특수한 UI가 별도 Inspector 동작을 요구하는 경우에만 전용 Editor를 고려합니다.

Validation severity의 기본 원칙:

```text
Empty Key
→ Warning

Duplicate Key
→ Error

Getter Key와 Binding Key 충돌
→ Error

Event Handler signature 불일치
→ Error
```

Editor 전용 validation 결과 타입은 Runtime assembly에 넣지 않습니다.

---

# Binding Inspector

`BaseUI` Inspector에서는 현재 UI Scope의 Binding 관계를 표시합니다.

예:

```text
CanSubmit
    Binding   ButtonBinder
              Interactable

Submit
    Binding   ButtonBinder
              OnClick

    Handler   CharacterUI
              Submit(Button)
```

`Select` 기능을 통해 해당 Binder 또는 Handler Object를 Hierarchy에서 찾을 수 있도록 할 수 있습니다.

## Binding Inspector Scope

Nested `BaseUI` 내부의 Binder는 부모 `BaseUI` Inspector에 표시하지 않습니다.

Editor의 Scope 판정은 Runtime Binder registration과 동일한 의미를 가져야 합니다.

예:

```text
InventoryUI
├── ButtonBinder
└── ConfirmPopupUI
    └── ButtonBinder
```

`InventoryUI` Binding Inspector에는 첫 번째 `ButtonBinder`만 표시합니다.

`ConfirmPopupUI` 아래의 `ButtonBinder`는 `ConfirmPopupUI` Scope에 속합니다.

## Binding Inspector Event 검색

Editor에서 UI Event Handler를 찾을 때도 구체 Attribute를 각각 검사하지 않습니다.

```csharp
var attribute =
    method.GetCustomAttribute<UIRegisterAttribute>(true);
```

예:

```csharp
[ButtonClickEventRegister]
private void Submit(Button button)
{
}
```

Binder:

```text
Submit → OnClick
```

Inspector:

```text
Submit
    Binding   ButtonBinder
              OnClick

    Handler   CharacterUI
              Submit(Button)
```

`UIRegisterAttribute.DelegateType`을 이용해 공통 처리하고 Event Attribute별 분기 로직을 만들지 마세요.

---

# Runtime / Editor 분리

Runtime assembly에는 `UnityEditor` dependency가 들어가면 안 됩니다.

Editor 전용 코드는 반드시 `Editor` 폴더 또는 Editor 전용 asmdef에 위치시킵니다.

다음과 같은 타입은 Editor 전용으로 유지합니다.

```text
BinderEditor
BinderValidator
ValidationMessage
BaseUIEditor
BindingViewCollector
BindingViewInfo
```

Editor 편의를 위한 데이터 타입을 Runtime assembly에 추가하지 마세요.

---

# Dependency 정책

Runtime package는 가능한 한 외부 dependency 없이 유지합니다.

특별한 이유가 없다면 다음 dependency를 추가하지 마세요.

- UniTask
- Reactive Extensions
- DI Framework
- Tween Framework
- Source Generator Runtime Dependency

Unity 기본 기능으로 충분히 구현 가능한 경우 Unity API를 사용합니다.

예:

```text
UniTask.Yield()
→ Coroutine / yield return null

단순 Frame Delay
→ Coroutine

단순 UI Tween
→ Coroutine + StopCoroutine
```

Dependency를 추가해야 한다면 기존 Unity API만으로 구현 가능한지 먼저 확인하세요.

---

# Coroutine

간단한 UI animation이나 frame delay에는 Coroutine 사용을 허용합니다.

시간 배율의 영향을 받지 않아야 하는 동작을 변경할 때는 기존 semantics를 유지합니다.

예:

```csharp
Time.unscaledDeltaTime
```

또는:

```csharp
WaitForSecondsRealtime
```

UniTask 제거 과정에서 기존:

```text
ignoreTimeScale: true
```

동작을 실수로 `Time.deltaTime` 기반으로 변경하지 마세요.

---

# Unity Object Null

`UnityEngine.Object`는 일반 C# object와 다른 null semantics를 가집니다.

Generic Target을 검사할 때 Unity fake-null을 고려하세요.

예:

```csharp
private bool IsTargetNull()
{
    if (Target == null)
        return true;

    if (Target is UnityEngine.Object unityObject)
        return unityObject == null;

    return false;
}
```

외부 Core extension에 의존하기보다 Binder base class에서 일관되게 처리하는 것을 우선합니다.

---

# Component Target

Binder가 Unity Component를 대상으로 할 때 Target 검색 로직을 중복 구현하지 마세요.

공통 `ComponentBinder<T>`가 담당하도록 합니다.

예:

```csharp
protected virtual T FindTarget()
{
    if (typeof(T) == typeof(GameObject))
        return gameObject as T;

    if (typeof(T) == typeof(Transform))
        return transform as T;

    return GetComponent<T>();
}
```

Concrete Binder는 자신의 Binding 동작에 집중해야 합니다.

---

# BindingType

Concrete Binder 내부의 enum은 `Property`와 같은 이름보다 `BindingType`을 사용합니다.

하나의 Binder에는 Property뿐 아니라 Command/Event 성격의 Binding이 함께 존재할 수 있기 때문입니다.

예:

```csharp
private enum BindingType
{
    Interactable,
    OnClick,
    SetValueWithoutNotify
}
```

Unity API를 직접 호출하는 Binding은 가능한 한 실제 Unity API 이름을 사용합니다.

예:

```text
SetValueWithoutNotify
```

이를 임의의 추상적인 이름으로 바꾸지 마세요.

---

# Command

Command는 `BaseUI → Binder → UI Component Method` 방향의 호출입니다.

인터페이스:

```csharp
public interface ICommandBinder<T>
{
    void ExecuteCommand(
        string key,
        T parameter);
}
```

`InvokeMethod`와 같이 Reflection invocation으로 오해할 수 있는 이름보다 `ExecuteCommand`를 사용합니다.

Command가 반드시 Reflection을 사용해야 하는 것은 아닙니다.

Concrete Binder가 명시적으로 Unity API를 호출하는 형태를 우선합니다.

예:

```csharp
public void ExecuteCommand(
    string key,
    int parameter)
{
    if (!_bindingMap.TryGet(
            key,
            out var type))
    {
        return;
    }

    var target = GetTarget();

    if (target == null)
        return;

    switch (type)
    {
        case BindingType.SetValueWithoutNotify:
            target.SetValueWithoutNotify(parameter);
            break;
    }
}
```

---

# 변경 시 우선순위

코드를 수정할 때 다음 순서를 우선합니다.

1. 기존 Binding 의미 유지
2. Runtime 안정성
3. Public API 일관성
4. 외부 Dependency 최소화
5. Editor 사용성
6. 성능 최적화
7. 코드 간결성

성능 최적화를 위해 기존 의미를 변경하지 마세요.

단순히 코드 줄 수를 줄이기 위해 Binding semantics를 변경하지 마세요.

---

# 변경 전 확인

구조적인 변경을 하기 전에 다음을 확인하세요.

- 동일 Key가 여러 Binder에 존재할 수 있는가?
- 하나의 Binder 내부에서 Key 중복이 방지되는가?
- Nested BaseUI Scope가 유지되는가?
- Dynamic Binder가 정상 등록되는가?
- SearchBinders 이후 기존 Binder가 재등록되는가?
- UI Event가 중복 등록되지 않는가?
- UI Event의 기본 Key가 Method 이름으로 유지되는가?
- 새로운 Event Attribute가 `UIRegisterAttribute` 계층을 따르는가?
- IL2CPP에서 Event Handler가 보존되는가?
- Runtime assembly에 Editor dependency가 추가되지 않는가?
- 새로운 외부 dependency가 정말 필요한가?
- Editor 편의를 위해 Runtime API가 오염되지 않는가?

---

# 테스트

Binding 관련 코드를 변경할 경우 가능한 한 다음 시나리오를 검증합니다.

```text
BindingMap
├── Add
├── Duplicate Key
├── Contains
└── TryGet

BinderRegistry
├── Register
├── Duplicate Register
├── Unregister
├── FindFirst
└── FindAll

BaseUI
├── SearchBinders
├── Re-registration
├── Dynamic Binder
├── Nested BaseUI Scope
└── Event Registration

UI Event
├── Method Name 기본 Key
├── Explicit Key
├── Delegate Signature
├── Register
└── Unregister
```

버그 수정 시 가능하면 해당 버그를 재현하는 테스트를 먼저 추가하거나 함께 추가하세요.

특히 Binder Registry 관련 변경은 다음 시나리오를 확인합니다.

```text
1. Binder 등록
2. Registry Clear
3. 동일 Binder의 Bind() 재호출
4. Binder가 정상적으로 다시 등록되는지 확인
```

---

# 하지 말아야 할 것

특별한 요청 없이 다음과 같은 대규모 변경을 하지 마세요.

- 프로젝트를 MVVM Framework로 변경
- String Key 제거
- String Key를 enum이나 Generated ID로 강제 변경
- Reactive Framework 도입
- Dependency Injection Framework 도입
- Source Generator 도입
- 모든 Binding을 Reflection 기반으로 변경
- 모든 Binding을 자동 생성 방식으로 변경
- Runtime API를 Editor 기능 때문에 확장
- 하나의 Binder 내부에서 Duplicate Key 허용
- Nested BaseUI Scope 제거
- Binder를 모든 Parent Manager에 자동 등록하도록 의미 변경
- `OnDisable()`에서 Binder Registry 등록 해제
- UniTask 재도입
- 기존 Public API의 무분별한 변경

UI Event와 관련해서는 다음도 하지 마세요.

- `UIRegisterAttribute` 계층을 하나의 범용 `UIEventAttribute`로 통합
- Event Handler에서 `DelegateType`을 직접 반복 지정하도록 API 변경
- 구체적인 Event Attribute마다 Reflection/Editor 분기 코드 추가
- `UIRegisterAttribute`의 `PreserveAttribute` 상속 제거
- Method 이름을 기본 Binding Key로 사용하는 규칙 변경

현재의 구체적인 Event Attribute 구조는 단순한 중복이 아니라 사용처의 명확성과 타입별 Delegate 정의를 위한 의도된 API입니다.

더 복잡한 구조가 기술적으로 가능하더라도 이 프로젝트에서는 **작고 명시적이며 의존성이 적은 구현**을 우선합니다.

---

# 문서

Public API 또는 사용 방법이 변경되면 `README.md`도 함께 확인합니다.

특히 다음 변경은 README 수정 여부를 반드시 검토하세요.

- 새로운 Binder 추가
- 새로운 BindingType 추가
- 새로운 Event Register Attribute 추가
- Event Attribute 이름 변경
- Event Delegate signature 변경
- Event Key 결정 규칙 변경
- Public API 이름 변경
- Installation 방식 변경
- Dependency 변경
- Editor Tool 기능 추가

README 예제에서는 실제 Public API와 동일한 Event Attribute를 사용해야 합니다.

예:

```csharp
[ButtonClickEventRegister]
private void Submit(Button button)
{
}
```

코드와 README의 예제가 서로 다른 API를 사용하지 않도록 유지하세요.