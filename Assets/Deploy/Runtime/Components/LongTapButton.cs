using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [AddComponentMenu("UI/LongTapButton", 31)]
    public sealed class LongTapButton : Button
    {
        /// <summary>
        /// 롱탭을 인식할 최초 진입시간
        /// </summary>
        private static readonly float RecognizeLongTapTime = 0.2f;
        /// <summary>
        /// 롱탭 인식 후 반복처리될 주기
        /// </summary>
        private static readonly float RecognizeCycle = 0.04f;
        private static readonly float RecognizeVeryLongTabCycle = 0.02f;
        /// <summary>
        /// 긴 롱탭 인식할 진입시간
        /// </summary>
        private static readonly float RecognizeVeryLongTapTime = 5f;
        
        private Coroutine _longTapCoroutine;
        private float _loopingTimer; 
        [SerializeField] private ButtonClickedEvent _longTapEvent = new();
        [SerializeField] private ButtonClickedEvent _veryLongTapEvent = new();
        
        
        
        AnimationCurve scale = AnimationCurve.Linear(0, 1.0f, 2, 0.2f);
        public ButtonClickedEvent onLongTap
        {
            get => _longTapEvent;
            set => _longTapEvent = value;
        }

        float GetRecognizeVeryLongTabCycle()
        { 
            var longTabCycle = RecognizeVeryLongTabCycle * (scale.Evaluate(_loopingTimer));
            return longTabCycle;
        }
        
        public ButtonClickedEvent onVeryLongTap
        {
            get => _veryLongTapEvent;
            set => _veryLongTapEvent = value;
        }

        private void StopLongTap()
        {
            if (_longTapCoroutine == null)
                return;

            StopCoroutine(_longTapCoroutine);
            _longTapCoroutine = null;
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            StopLongTap();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            StopLongTap();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            StopLongTap();

            _longTapCoroutine =
                StartCoroutine(LongTapCoroutine());
        }
        
        private IEnumerator LongTapCoroutine()
        {
            _loopingTimer = 0f;

            var nextInvokeTime = RecognizeLongTapTime;

            while (IsPointerPressed())
            {
                _loopingTimer += Time.unscaledDeltaTime;

                if (_loopingTimer < nextInvokeTime)
                {
                    yield return null;
                    continue;
                }

                if (_loopingTimer < RecognizeVeryLongTapTime)
                {
                    UISystemProfilerApi.AddMarker(
                        "LongTapButton.onLongTap",
                        this);

                    _longTapEvent.Invoke();

                    nextInvokeTime += RecognizeCycle;
                }
                else
                {
                    UISystemProfilerApi.AddMarker(
                        "LongTapButton.onVeryLongTap",
                        this);

                    _veryLongTapEvent.Invoke();

                    nextInvokeTime += GetRecognizeVeryLongTabCycle();
                }

                yield return null;
            }

            _longTapCoroutine = null;
        }
        
        private bool IsPointerPressed()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0) && gameObject.activeInHierarchy;
#else
            return Input.touchCount > 0 &&gameObject.activeInHierarchy;
#endif
        }
    }
}
