using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
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
            SetNormalizedPosition();
            SetAnchorsToHalf();
        }
        
        public void OnBeginDrag(PointerEventData data)
        {
            StopAnimate();
            _isBeingDragged = true;
        }
    
        public void OnDrag(PointerEventData data)
        {
            bool isGotcha = RectTransformUtility.ScreenPointToLocalPointInRectangle(_fieldMagnetism, data.position,
                data.pressEventCamera, out Vector2 localPoint);

            if (isGotcha == true)
            {
                _item.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            _isBeingDragged = false;
            UpdatePosition();
        }
        
        private void SetNormalizedPosition()
        {
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
        
        private void SetAnchorsToHalf()
        {
            _item.anchorMin = new Vector2(.5f, .5f);
            _item.anchorMax = new Vector2(.5f, .5f);
        }

        private void UpdatePosition()
        {
            Vector2 canvasRawSize = _fieldMagnetism.rect.size;

            float canvasWidth = canvasRawSize.x;
            float canvasHeight = canvasRawSize.y;

            float canvasBottomLeftX = 0f;
            float canvasBottomLeftY = 0f;

            Vector2 position = canvasRawSize * 0.5f + (_item.anchoredPosition - new Vector2(canvasBottomLeftX, canvasBottomLeftY));

            float distToLeft = position.x;
            float distToRight = canvasWidth - distToLeft;

            float distToBottom = position.y;
            float distToTop = canvasHeight - distToBottom;

            float horDistance = Mathf.Min(distToLeft, distToRight);
            float vertDistance = Mathf.Min(distToBottom, distToTop);

            if (horDistance < vertDistance)
            {
                if (distToLeft < distToRight)
                {
                    position = new Vector2(_halfSize.x, position.y);
                }
                else
                {
                    position = new Vector2(canvasWidth - _halfSize.x, position.y);
                }

                position.y = Mathf.Clamp(position.y, _halfSize.y, canvasHeight - _halfSize.y);
            }
            else
            {
                if (distToBottom < distToTop)
                {
                    position = new Vector2(position.x, _halfSize.y);
                }
                else
                {
                    position = new Vector2(position.x, canvasHeight - _halfSize.y);
                }

                position.x = Mathf.Clamp(position.x, _halfSize.x, canvasWidth - _halfSize.x);
            }

            position -= canvasRawSize * 0.5f;

            _normalizedPosition.Set(position.x / canvasWidth, position.y / canvasHeight);

            position += new Vector2(canvasBottomLeftX, canvasBottomLeftY);

            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = AnimateMovement(position);
            StartCoroutine(_coroutine);
        
        }
    
        private void StopAnimate()
        {
            if (_coroutine == null) return;
        
            StopCoroutine(_coroutine);
            _coroutine = null;
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
}