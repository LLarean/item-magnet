using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemMagnet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private RectTransform _fieldMagnetism;
    [SerializeField]
    private RectTransform _item;

    private bool _isBeingDragged;
    private Vector2 _halfSize;
    private Vector2 _normalizedPosition;

    private IEnumerator _coroutine;

    private void Awake()
    {
        _halfSize = _item.sizeDelta * 0.5f;
        Vector2 position = _item.anchoredPosition;

        if (position.x != 0f || position.y != 0f)
        {
            _normalizedPosition = position.normalized;
        }
        else
        {
            _normalizedPosition = new Vector2(0.5f, 0f);
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        _isBeingDragged = true;

        if (_coroutine == null) return;
        
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    public void OnDrag(PointerEventData data)
    {
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_fieldMagnetism, data.position,
                data.pressEventCamera, out localPoint))
        {
            _item.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        _isBeingDragged = false;
        UpdatePosition();
    }

    // There are 2 different spaces used in these calculations:
    // RectTransform space: raw anchoredPosition of the popup that's in range [-canvasSize/2, canvasSize/2]
    // Safe area space: Screen.safeArea space that's in range [safeAreaBottomLeft, safeAreaTopRight] where these corner positions
    //                  are all positive (calculated from bottom left corner of the screen instead of the center of the screen)
    private void UpdatePosition()
    {
        Vector2 canvasRawSize = _fieldMagnetism.rect.size;

        // Calculate safe area bounds
        float canvasWidth = canvasRawSize.x;
        float canvasHeight = canvasRawSize.y;

        float canvasBottomLeftX = 0f;
        float canvasBottomLeftY = 0f;

        // Calculate safe area position of the popup
        // normalizedPosition allows us to glue the popup to a specific edge of the screen. It becomes useful when
        // the popup is at the right edge and we switch from portrait screen orientation to landscape screen orientation.
        // Without normalizedPosition, popup could jump to bottom or top edges instead of staying at the right edge
        Vector2 pos = canvasRawSize * 0.5f + (_item.anchoredPosition - new Vector2(canvasBottomLeftX, canvasBottomLeftY));

        // Find distances to all four edges of the safe area
        float distToLeft = pos.x;
        float distToRight = canvasWidth - distToLeft;

        float distToBottom = pos.y;
        float distToTop = canvasHeight - distToBottom;

        float horDistance = Mathf.Min(distToLeft, distToRight);
        float vertDistance = Mathf.Min(distToBottom, distToTop);

        // Find the nearest edge's safe area coordinates
        if (horDistance < vertDistance)
        {
            if (distToLeft < distToRight)
                pos = new Vector2(_halfSize.x, pos.y);
            else
                pos = new Vector2(canvasWidth - _halfSize.x, pos.y);

            pos.y = Mathf.Clamp(pos.y, _halfSize.y, canvasHeight - _halfSize.y);
        }
        else
        {
            if (distToBottom < distToTop)
                pos = new Vector2(pos.x, _halfSize.y);
            else
                pos = new Vector2(pos.x, canvasHeight - _halfSize.y);

            pos.x = Mathf.Clamp(pos.x, _halfSize.x, canvasWidth - _halfSize.x);
        }

        pos -= canvasRawSize * 0.5f;

        _normalizedPosition.Set(pos.x / canvasWidth, pos.y / canvasHeight);

        // Safe area's bottom left coordinates are added to pos only after normalizedPosition's value
        // is set because normalizedPosition is in range [-canvasWidth / 2, canvasWidth / 2]
        pos += new Vector2(canvasBottomLeftX, canvasBottomLeftY);

        // If another smooth movement animation is in progress, cancel it
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }


            // Smoothly translate the popup to the specified position
            _coroutine = AnimateMovement(pos);
        StartCoroutine(_coroutine);
        
    }
    
    private IEnumerator AnimateMovement(Vector2 targetPos)
    {
        float modifier = 0f;
        Vector2 initialPosition = _item.anchoredPosition;

        while (modifier < 1f)
        {
            modifier += 4f * Time.unscaledDeltaTime;
            _item.anchoredPosition = Vector2.Lerp(initialPosition, targetPos, modifier);

            yield return null;
        }
    }
}