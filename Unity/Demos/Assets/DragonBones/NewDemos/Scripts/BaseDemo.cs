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

    private GameObject _currentDragTarget;
    private Vector3 _startDragWorldPosition;
    private Vector3 _startDragScreenPosition;
    private Vector3 _currentDragWorldPosition;
    private Vector3 _dragOffset;

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
            //
            this._currentDragTarget = this.GetClickTarget();
            if (this._currentDragTarget != null)
            {
                this._startDragWorldPosition = this._currentDragTarget.transform.localPosition;
                this._startDragScreenPosition = Camera.main.WorldToScreenPoint(this._currentDragTarget.transform.localPosition);
                this._dragOffset = this._currentDragTarget.transform.localPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, this._startDragScreenPosition.z));
            }

            this.OnTouch(TouchType.TOUCH_BEGIN);
        }
        //
        if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine("DragDelay");
            //
            this._isTouched = false;
            this._currentDragTarget = null;
            this._startDragWorldPosition = Vector3.zero;
            this._currentDragWorldPosition = Vector3.zero;

            this.OnTouch(TouchType.TOUCH_END);
        }
        //
        if (this._isTouched)
        {
            if (this._currentDragTarget != null)
            {
                Vector3 currentScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this._startDragScreenPosition.z);
                //
                this._currentDragWorldPosition = Camera.main.ScreenToWorldPoint(currentScreenPosition) + this._dragOffset;
                this._currentDragWorldPosition.z = this._currentDragTarget.transform.localPosition.z;
                this._currentDragTarget.transform.localPosition = this._currentDragWorldPosition;
                //
                this.OnDrag(this._currentDragTarget, this._startDragWorldPosition, this._currentDragWorldPosition);
            }

            this.OnTouch(TouchType.TOUCH_MOVE);
        }

        this.OnUpdate();
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnTouch(TouchType type) { }
    protected virtual void OnDrag(GameObject target, Vector3 startDragPos, Vector3 currentDragPos) {}

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