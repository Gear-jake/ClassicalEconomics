using UnityEngine;
using UnityEngine.EventSystems;

namespace EconomyMod.UI
{
    /// <summary>
    /// 可拖拽的原版窗口入口按钮（中央银行家账本钮）：
    /// 拖拽移动自身（相对其父级 RectTransform 的局部坐标，含画布缩放换算），
    /// 结束拖拽时通过 OnDragEnded 回调把最终偏移交给调用方保存，
    /// 此后新注入的按钮都出现在该偏移处（会话内记忆）。
    /// 挂在本模组自己的按钮上，不影响原版按钮。
    /// </summary>
    public class DraggableWindowButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>拖拽结束回调：参数 = 按钮最终的 anchoredPosition（相对窗口右上锚点）。</summary>
        public System.Action<Vector2> OnDragEnded;

        private RectTransform _rt;
        private Vector2 _startPos;
        private Vector2 _startMouse;

        private void Awake()
        {
            _rt = transform as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rt == null) _rt = transform as RectTransform;
            if (_rt == null) return;
            _startPos = _rt.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rt.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out _startMouse);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rt == null) return;
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rt.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out mousePos);
            _rt.anchoredPosition = _startPos + (mousePos - _startMouse);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_rt == null) return;
            var cb = OnDragEnded;
            if (cb != null) cb(_rt.anchoredPosition);
        }
    }
}
