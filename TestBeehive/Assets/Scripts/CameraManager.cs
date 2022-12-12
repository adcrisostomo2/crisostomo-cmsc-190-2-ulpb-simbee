using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public GameObject main_cam_ref_point;
    public Camera main_cam;

    private Touch init_touch = new Touch();
    private const float rotation_sensitivity = 8.5f;
    private const float scroll_sensitivity = 15.0f;
    private const float perspective_zoom_speed = 0.15f;
    private const float min_fov = 15f;
    private const float max_fov = 110f;
    private bool is_zooming = false;
    private bool is_rotating = false;
    private float rot_x = 0f;
    private float rot_y = 0f;
    
    private void rotateCameraAroundBeehive()
    {
        // if screen touch is detected
        if (Input.touchCount == 1 && !is_zooming)
        {
            is_rotating = true;

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                this.init_touch = touch;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float delta_x = touch.position.x - init_touch.position.x;
                float delta_y = touch.position.y - init_touch.position.y;

                // adjust angles
                this.rot_x += -delta_y * Time.deltaTime * rotation_sensitivity;
                this.rot_y += delta_x * Time.deltaTime * rotation_sensitivity;

                //// limit viewing angles
                this.rot_x = Mathf.Clamp(this.rot_x, 0f, 90f);
                //this.rot_y = Mathf.Clamp(this.rot_y, -90f, 90f);

                // adjust camera view
                this.main_cam_ref_point.transform.localRotation = Quaternion.Euler(rot_x, rot_y, 0);

                this.init_touch = touch;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                this.init_touch = new Touch();
            }

            is_rotating = false;
        }
        // if mouse click is detected
        else if (Input.GetMouseButton(0) && Input.touchCount == 0 && !is_zooming)
        {
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            { 
                is_rotating = true;

                this.rot_x -= Input.GetAxis("Mouse Y") * rotation_sensitivity;
                this.rot_y += Input.GetAxis("Mouse X") * rotation_sensitivity;

                //// limit viewing angles
                this.rot_x = Mathf.Clamp(this.rot_x, 0f, 90f);
                //this.rot_y = Mathf.Clamp(this.rot_y, -90f, 90f);

                // adjust camera view
                this.main_cam_ref_point.transform.localRotation = Quaternion.Euler(this.rot_x, this.rot_y, 0);

                is_rotating = false;
            }
        }    
    }

    private void zoomCamera()
    {
        if (Input.touchCount == 2 && !is_rotating)
        {
            is_zooming = true;

            Touch touch_zero = Input.GetTouch(0);
            Touch touch_one = Input.GetTouch(1);

            Vector2 touch_zero_prev_pos = touch_zero.position - touch_zero.deltaPosition;
            Vector2 touch_one_prev_pos = touch_one.position - touch_one.deltaPosition;

            float prev_touch_delta_magnitude = (touch_zero_prev_pos - touch_one_prev_pos).magnitude;
            float touch_delta_magnitude = (touch_zero.position - touch_one.position).magnitude;

            float delta__magnitude_diff = prev_touch_delta_magnitude - touch_delta_magnitude;

            // increase or decrease FOV
            this.main_cam.fieldOfView += delta__magnitude_diff * perspective_zoom_speed;
            // limit the value of FOV
            this.main_cam.fieldOfView = Mathf.Clamp(main_cam.fieldOfView, min_fov, max_fov);

            is_zooming = false;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0f &&  Input.touchCount == 0 && !is_rotating)
        {
            is_zooming = true;

            float scroll_amount = Input.GetAxis("Mouse ScrollWheel") * scroll_sensitivity;

            // increase or decrease FOV
            this.main_cam.fieldOfView -= scroll_amount;
            // limit the value of FOV
            this.main_cam.fieldOfView = Mathf.Clamp(main_cam.fieldOfView, min_fov, max_fov);

            is_zooming = false;
        }
    }

    private bool isPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return (results.Count > 0) ? true : false;
    }

    private void LateUpdate()
    {
        if (!isPointerOverUIObject())
        {
            this.rotateCameraAroundBeehive();
            this.zoomCamera();
        }
    }
}
