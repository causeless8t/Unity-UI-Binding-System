using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI.Tests
{
    public sealed class InitializationTestUI : BaseUI
    {
        public int ClickCount { get; private set; }

        [ButtonClickEventRegister("Submit")]
        public void Submit(Button button) => ClickCount++;
    }

    public sealed class BinderInitializationTests
    {
        private GameObject _template;
        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            if (_instance != null) Object.DestroyImmediate(_instance);
            if (_template != null) Object.DestroyImmediate(_template);
        }

        private InitializationTestUI CreateInactiveUI()
        {
            _template = new GameObject("UI", typeof(RectTransform));
            _template.SetActive(false);
            var ui = _template.AddComponent<InitializationTestUI>();
            var child = new GameObject("Button", typeof(RectTransform));
            child.transform.SetParent(_template.transform);
            child.AddComponent<Button>();
            var binder = child.AddComponent<ButtonBinder>();
            typeof(ButtonBinder).GetField("_bindInfos", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(binder, new List<ButtonBinder.BindInfo>
                {
                    new ButtonBinder.BindInfo
                    {
                        Key = "Submit", bindingTypeType = ButtonBinder.BindingType.OnClick
                    }
                });
            return ui;
        }

        [Test]
        public void SearchBeforeChildAwake_InitializesKeysAndConnectsExactlyOnce()
        {
            var ui = CreateInactiveUI();
            var binder = _template.GetComponentInChildren<ButtonBinder>(true);
            var button = _template.GetComponentInChildren<Button>(true);

            // 부모의 검색과 이벤트 등록을 자식 Awake보다 먼저 실행한다.
            ui.SearchBinders();
            Assert.That(binder.HasKey("Submit"), Is.True);
            ui.RegisterUIEvents();
            binder.Bind();
            binder.Bind();
            _template.SetActive(true);
            button.onClick.Invoke();
            Assert.That(ui.ClickCount, Is.EqualTo(1));

            ui.SearchBinders();
            button.onClick.Invoke();
            Assert.That(ui.ClickCount, Is.EqualTo(2));

            _template.SetActive(false);
            _template.SetActive(true);
            button.onClick.Invoke();
            Assert.That(ui.ClickCount, Is.EqualTo(3));
        }

        [Test]
        public void InstantiatedUI_ConnectsButtonOnFirstActivation()
        {
            CreateInactiveUI();
            _instance = Object.Instantiate(_template);
            _instance.SetActive(true);
            var ui = _instance.GetComponent<InitializationTestUI>();
            _instance.GetComponentInChildren<Button>().onClick.Invoke();
            Assert.That(ui.ClickCount, Is.EqualTo(1));
        }
    }
}
