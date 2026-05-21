using UnityEngine;
using UnityEngine.UIElements;

public class SimpleDragManipulator : PointerManipulator
{
    private Vector2 _startPointerPosition;
    private Vector2 _startElementPosition;
    private bool _enabled;

    public SimpleDragManipulator(VisualElement target)
    {
        this.target = target;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        // บันทึกตำแหน่งเริ่มต้น
        _startPointerPosition = evt.position;
        _startElementPosition = new Vector2(target.layout.x, target.layout.y);

        // ตั้งค่าให้ element อยู่ในโหมด absolute และดึงมาไว้หน้าสุด
        target.style.position = Position.Absolute;
        target.BringToFront();

        // จับ Pointer ไว้ไม่ให้หลุดแม้ลากออกจากขอบ element
        target.CapturePointer(evt.pointerId);
        _enabled = true;
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_enabled || !target.HasPointerCapture(evt.pointerId))
            return;

        // คำนวณระยะห่าง (Delta)
        Vector2 delta = (Vector2)evt.position - _startPointerPosition;

        // อัปเดตตำแหน่ง element ตามระยะลาก
        target.style.left = _startElementPosition.x + delta.x;
        target.style.top = _startElementPosition.y + delta.y;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        _enabled = false;
    }
}
