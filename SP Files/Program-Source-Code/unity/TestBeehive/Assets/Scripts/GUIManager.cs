using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public CanvasGroup specific_frame_canvas;
    public CanvasGroup frame_slot_canvas;
    public CanvasGroup frame_type_canvas;

    // Start is called before the first frame update
    void Start()
    {
        //HideAllCanvas();
        //ShowAllCanvas();
    }

    public void ShowAllCanvas()
    {
        ShowFrameSlotCanvas();
        ShowFrameTypeCanvas();
        ShowSpecificFrameCanvas();
    }

    public void HideAllCanvas()
    {
        HideFrameTypeCanvas();
        HideFrameSlotCanvas();
        HideSpecificFrameCanvas();
    }

    public void ShowFrameTypeCanvas()
    {
        frame_type_canvas.alpha = 1f;
        frame_type_canvas.blocksRaycasts = true;
    }

    public void ShowFrameSlotCanvas()
    {
        frame_slot_canvas.alpha = 1f;
        frame_slot_canvas.blocksRaycasts = true;
    }

    public void ShowSpecificFrameCanvas()
    {
        specific_frame_canvas.alpha = 1f;
        specific_frame_canvas.blocksRaycasts = true;
    }

    public void HideFrameTypeCanvas()
    {
        frame_type_canvas.alpha = 0f;
        // prevents its child GUI elements to Listen for Events
        frame_type_canvas.blocksRaycasts = false;
    }

    public void HideFrameSlotCanvas()
    {
        frame_slot_canvas.alpha = 0f;
        frame_slot_canvas.blocksRaycasts = false;
    }

    public void HideSpecificFrameCanvas()
    {
        specific_frame_canvas.alpha = 0f;
        specific_frame_canvas.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
