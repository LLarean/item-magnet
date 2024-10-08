using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemMagnet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private RectTransform _fieldMagnetism;
        [SerializeField]
        private RectTransform _item;
        [Space]
        [SerializeField]
        private bool _shouldRotate;
        [SerializeField]
        private bool _isBaseHorizontally;
        
        private bool _isHorizontally;
        
        private float _startAngle;
        private float _rotateAngle = 90f;

        private Vector2 _halfSize;
        private Vector2 _normalizedPosition;

        private bool _isBeingDragged;
        private Coroutine _coroutine;

        private void Awake()
        {
            if (_item == null)
            {
                _item = gameObject.GetComponent<RectTransform>();
            }
            
            if (_fieldMagnetism == null)
            {
                _fieldMagnetism = transform.parent.GetComponent<RectTransform>();
            }
            
            _halfSize = _item.sizeDelta * 0.5f;
            _isHorizontally = _item.sizeDelta.x > _item.sizeDelta.y;
            _startAngle = _item.transform.rotation.eulerAngles.z;
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

            position = GetPosition(position, canvasWidth, canvasHeight);
            position -= canvasRawSize * 0.5f;
            _normalizedPosition.Set(position.x / canvasWidth, position.y / canvasHeight);
            position += new Vector2(canvasBottomLeftX, canvasBottomLeftY);

            SetNormalizedPosition();

            
            StopAnimate();
            _coroutine = StartCoroutine(AnimateMovement(position));
        }

        private Vector2 GetPosition(Vector2 position, float canvasWidth, float canvasHeight)
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
                    }
                    else
                    {
                        position = new Vector2(_halfSize.x, position.y); 
                    }

                    SetRotation(MagnetDirection.Left);
                }
                else
                {
                    // Right
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(canvasWidth - _halfSize.y, position.y);
                    }
                    else
                    {
                        position = new Vector2(canvasWidth - _halfSize.x, position.y);
                    }
                    
                    SetRotation(MagnetDirection.Right);
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
                        // Rotate(0);
                    }
                    else
                    {
                        position = new Vector2(position.x, _halfSize.x);
                        // Rotate(-90);
                    }
                    
                    SetRotation(MagnetDirection.Bottom);
                }
                else
                {
                    // Top
                    if (_isHorizontally == true)
                    {
                        position = new Vector2(position.x, canvasHeight - _halfSize.y);
                    }
                    else
                    {
                        position = new Vector2(position.x, canvasHeight - _halfSize.x);
                    }
                    
                    SetRotation(MagnetDirection.Top);
                }

                position.x = Mathf.Clamp(position.x, _halfSize.x, canvasWidth - _halfSize.x);
            }

            return position;
        }

        private void SetRotation(MagnetDirection magnetDirection)
        {
            if (_shouldRotate == false) return;

            var angle = magnetDirection switch
            {
                MagnetDirection.Top when _isBaseHorizontally == true => _startAngle,
                MagnetDirection.Top when _isBaseHorizontally == false => -_rotateAngle,
                MagnetDirection.Bottom when _isBaseHorizontally == true => _startAngle,
                MagnetDirection.Bottom when _isBaseHorizontally == false => -_rotateAngle,
                MagnetDirection.Left when _isBaseHorizontally == true => _startAngle + _rotateAngle,
                MagnetDirection.Left when _isBaseHorizontally == false => _startAngle,
                MagnetDirection.Right when _isBaseHorizontally == true => _startAngle - _rotateAngle,
                MagnetDirection.Right when _isBaseHorizontally == false => _rotateAngle * 2,
                _ => _startAngle
            };

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
    
    public enum MagnetDirection
    {
        Top,
        Left,
        Right,
        Bottom,
    }

}