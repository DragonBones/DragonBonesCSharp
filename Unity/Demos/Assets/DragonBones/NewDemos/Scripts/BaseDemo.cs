using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TouchType
{
    TOUCH_BEGIN,
    TOUCH_MOVE,
    TOUCH_END
}

public abstract class BaseDemo : MonoBehaviour
{
    private readonly List<GameObject> _dragTargets = new List<GameObject>();

    protected bool _isTouched = false;
    protected Vector3 _touchPosition = Vector3.zero;
    protected Vector3 _touchOffset = Vector3.zero;

    private GameObject _currentDragTarget;
    private Vector3 _worldPosition;
    private Vector3 _screenPosition;
    protected Vector3 _dragOffsetPosition;
    private Vector3 _offset;

    void Start()
    {
        this.CreateBackground();
        this.OnStart();
    }

    void Update()
    {
        //
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine("DragDelay");
            this._touchPosition = Input.mousePosition;
            this._touchOffset = Vector3.zero;
            //
            this._currentDragTarget = GetClickTarget();
            if (this._currentDragTarget != null)
            {
                this._worldPosition = this._currentDragTarget.transform.localPosition;
                this._screenPosition = Camera.main.WorldToScreenPoint(this._currentDragTarget.transform.localPosition);
                this._offset = this._currentDragTarget.transform.localPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, this._screenPosition.z));
            }

            this.OnTouch(TouchType.TOUCH_BEGIN);
        }
        //
        if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine("DragDelay");
            this._isTouched = false;

            this._touchPosition = Vector3.zero;
            this._touchOffset = Vector3.zero;

            this._currentDragTarget = null;
            this._dragOffsetPosition = Vector3.zero;
            this.OnTouch(TouchType.TOUCH_END);
        }
        //
        if (this._isTouched)
        {
            this._touchOffset = Input.mousePosition - this._touchPosition;
            if (this._currentDragTarget != null)
            {
                Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this._screenPosition.z);
                Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + this._offset;
                currentPosition.z = this._currentDragTarget.transform.localPosition.z;
                this._currentDragTarget.transform.localPosition = currentPosition;

                this._dragOffsetPosition = currentPosition - this._worldPosition;
            }

            this.OnTouch(TouchType.TOUCH_MOVE);
        }

        this.OnUpdate();
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnTouch(TouchType type) { }

    protected void EnableDrag(GameObject target)
    {
        if (!this._dragTargets.Contains(target))
        {
            this._dragTargets.Add(target);

            var collider = target.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = target.AddComponent<BoxCollider>();
            }
        }
    }

    protected void DisableDrag(GameObject target)
    {
        if (this._dragTargets.Contains(target))
        {
            this._dragTargets.Remove(target);

            var collider = target.GetComponent<BoxCollider>();
            if (collider != null)
            {
                GameObject.Destroy(collider);
            }
        }
    }

    private void CreateBackground()
    {
        var background = new GameObject("Background");
        var renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("background");
        background.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
        background.transform.SetSiblingIndex(this.transform.GetSiblingIndex() + 1);
    }

    private GameObject GetClickTarget()
    {
        if (this._dragTargets.Count == 0)
        {
            return null;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            foreach (var t in _dragTargets)
            {
                if (t == hit.collider.gameObject)
                {
                    return t;
                }
            }
        }

        return null;
    }

    private IEnumerator DragDelay()
    {
        yield return new WaitForSeconds(0.16f);

        this._isTouched = true;
    }
}
