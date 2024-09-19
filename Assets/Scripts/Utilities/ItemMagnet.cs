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
        [SerializeField]
        private bool _shouldRotate;
        
        private bool _isHorizontally;

        private Vector2 _halfSize;
        private Vector2 _normalizedPosition;

        private bool _isBeingDragged;
        private Coroutine _coroutine;

        // private float _size;

        private void Awake()
        {
            _halfSize = _item.sizeDelta * 0.5f;
            _isHorizontally = _item.sizeDelta.x > _item.sizeDelta.y;
            // var temp = _item.rotation.z;
            SetNormalizedPosition();
        }
        
        public void OnBeginDrag(PointerEventData data)
        {
            StopAnimate();
            SetAnchorsToHalf();
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

            position = SetSidePosition(position, canvasWidth, canvasHeight);
            position -= canvasRawSize * 0.5f;
            _normalizedPosition.Set(position.x / canvasWidth, position.y / canvasHeight);
            position += new Vector2(canvasBottomLeftX, canvasBottomLeftY);

            StopAnimate();
            _coroutine = StartCoroutine(AnimateMovement(position));
        }

        private Vector2 SetSidePosition(Vector2 position, float canvasWidth, float canvasHeight)
        {
            float distToLeft = position.x;
            float distToRight = canvasWidth - distToLeft;

            float distToBottom = position.y;
            float distToTop = canvasHeight - distToBottom;

            float horizontalDistance = Mathf.Min(distToLeft, distToRight);
            float verticalDistance = Mathf.Min(distToBottom, distToTop);

            if (horizontalDistance < verticalDistance)
            {
                // Horizontal
                if (distToLeft < distToRight)
                {
                    // Left 
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(_halfSize.y, position.y);
                        Rotate(90);
                    }
                    else
                    {
                        position = new Vector2(_halfSize.x, position.y);
                        Rotate(0);
                    }
                }
                else
                {
                    // Right
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(canvasWidth - _halfSize.y, position.y);
                        Rotate(-90);
                    }
                    else
                    {
                        position = new Vector2(canvasWidth - _halfSize.x, position.y);
                        Rotate(180);
                    }
                }

                position.y = Mathf.Clamp(position.y, _halfSize.y, canvasHeight - _halfSize.y);
            }
            else
            {
                // Vertical
                if (distToBottom < distToTop)
                {
                    // Bottom
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(position.x, _halfSize.y);
                        Rotate(0);
                    }
                    else
                    {
                        position = new Vector2(position.x, _halfSize.x);
                        Rotate(-90);
                    }
                    
                    // position = new Vector2(position.x, _halfSize.y);
                    // Rotate(0);
                }
                else
                {
                    // Top
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(position.x, canvasHeight - _halfSize.y);
                        Rotate(0);
                    }
                    else
                    {
                        position = new Vector2(position.x, canvasHeight - _halfSize.x);
                        Rotate(-90);
                    }
                    
                    // position = new Vector2(position.x, canvasHeight - _halfSize.y);
                    // Rotate(0);
                }

                position.x = Mathf.Clamp(position.x, _halfSize.x, canvasWidth - _halfSize.x);
            }

            return position;
        }

        private void Rotate(float angle)
        {
            if (_shouldRotate == false) return;
            
            var rotateAngle = new Quaternion
            {
                eulerAngles = new Vector3(0f, 0f, angle)
            };
            _item.transform.rotation = rotateAngle;
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